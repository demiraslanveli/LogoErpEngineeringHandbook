# 121 — Reconciliation Repository ve Karşılaştırma Modeli

Bu bölüm, dış sistem ile Logo ERP arasında oluşturulan kayıtların sonradan karşılaştırılabilmesi için kullanılacak reconciliation repository yaklaşımını tanımlar.

## Neden Reconciliation Gerekir?

Bir entegrasyon kaydı başarıyla gönderilmiş görünse bile sonradan şu problemler ortaya çıkabilir:

- Logo belgesi silinmiş olabilir
- belge iptal edilmiş olabilir
- miktar veya tutar değişmiş olabilir
- dış sistem kaydı tekrar üretilmiş olabilir
- bağlantı referansı bozulmuş olabilir
- işlem yarım kalmış olabilir
- retry nedeniyle duplicate oluşmuş olabilir

Bu nedenle yalnızca "başarıyla gönderildi" logu yeterli değildir.

## Reconciliation Kaydı

Önerilen temel model:

```text
IntegrationKey
SourceSystem
SourceEntity
SourceId
CompanyNo
PeriodNo
LogoEntity
LogoLogicalRef
LogoDocumentNo
ExpectedHash
ActualHash
Status
LastCheckedAt
MismatchReason
CorrelationId
```

## Örnek Tablo

```sql
CREATE TABLE dbo.LOGO_INTEGRATION_RECONCILIATION
(
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    INTEGRATION_KEY NVARCHAR(150) NOT NULL,
    SOURCE_SYSTEM NVARCHAR(50) NOT NULL,
    SOURCE_ENTITY NVARCHAR(50) NOT NULL,
    SOURCE_ID NVARCHAR(100) NOT NULL,
    COMPANY_NO INT NOT NULL,
    PERIOD_NO INT NULL,
    LOGO_ENTITY NVARCHAR(50) NOT NULL,
    LOGO_LOGICALREF INT NULL,
    LOGO_DOCUMENT_NO NVARCHAR(50) NULL,
    EXPECTED_HASH NVARCHAR(128) NULL,
    ACTUAL_HASH NVARCHAR(128) NULL,
    STATUS NVARCHAR(30) NOT NULL,
    MISMATCH_REASON NVARCHAR(1000) NULL,
    CORRELATION_ID UNIQUEIDENTIFIER NULL,
    LAST_CHECKED_AT DATETIME2 NULL,
    CREATED_AT DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE UNIQUE INDEX UX_LOGO_INTEGRATION_RECONCILIATION_KEY
ON dbo.LOGO_INTEGRATION_RECONCILIATION(INTEGRATION_KEY);
```

## Durumlar

Örnek durumlar:

```text
Pending
Matched
MissingInLogo
ChangedInLogo
Duplicate
Cancelled
Failed
NeedsReview
```

## Hash Yaklaşımı

Büyük belge yapılarında tüm alanları tek tek kıyaslamak yerine canonical payload oluşturulabilir.

Örnek:

```text
DocumentNo
ClientCode
Date
Warehouse
LineCount
TotalAmount
VATAmount
Lines...
```

Bu veri normalize edilerek hash üretilir.

Ama hash tek başına yeterli değildir.

Mismatch olduğunda ayrıca detay sorgusu çalıştırılmalıdır.

## Repository Arayüzü

```csharp
public interface IReconciliationRepository
{
    ReconciliationRecord GetByIntegrationKey(string integrationKey);

    void Save(ReconciliationRecord record);

    void MarkMatched(
        string integrationKey,
        string actualHash,
        DateTime checkedAt);

    void MarkMismatch(
        string integrationKey,
        string actualHash,
        string reason,
        DateTime checkedAt);
}
```

## Reconciliation Worker

```text
Pending / eski kayıtlar
        ↓
Logo kayıtlarını oku
        ↓
Belge var mı?
        ↓
Ana alanları karşılaştır
        ↓
Hash oluştur
        ↓
Matched / Mismatch
        ↓
Log + alarm
```

## Önemli Prensip

Reconciliation işlemi Logo verisini değiştirmemelidir.

Görevi:

```text
Oku
Karşılaştır
Raporla
```

Düzeltme başka bir kontrollü workflow üzerinden yapılmalıdır.

## Gerçek Kullanım Alanları

- MES üretim bildirimi
- WMS ambar hareketi
- LIMS kalite sonucu
- satış siparişi entegrasyonu
- irsaliye/fatura entegrasyonu
- e-Fatura durum eşleştirme
- muhasebeleştirme kontrolü

> Reconciliation katmanı, entegrasyonun yalnızca veri gönderen değil veri doğrulayan bir sistem olmasını sağlar.
