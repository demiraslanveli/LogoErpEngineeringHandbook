# 106 — Transaction Sınırı, Idempotency ve Retry Politikası

Bu bölüm bir Logo entegrasyon operasyonunun transaction sınırını, tekrar çalıştırılabilirliğini ve retry davranışını tanımlar.

## Amaç

Entegrasyon sistemi aynı mesajı ikinci kez aldığında mükerrer belge üretmemeli; geçici hata olduğunda kontrollü retry yapmalı; kalıcı iş kuralı hatasında ise gereksiz tekrar denememelidir.

## Logical Transaction

Logo Objects ile yapılan bir iş çoğu zaman yalnızca SQL transaction değildir.

Örnek:

```text
Dış sipariş
    ↓
Validation
    ↓
Logo session
    ↓
IData Post
    ↓
LOGICALREF al
    ↓
Integration mapping kaydı
    ↓
Log
```

Bu zincirin tamamı logical transaction olarak ele alınmalıdır.

## Idempotency Key

Her dış işlem benzersiz bir anahtarla tanımlanmalıdır.

Örnek:

```text
SourceSystem + OperationType + ExternalId + CompanyNr
```

```csharp
public sealed class IdempotencyKey
{
    public string SourceSystem { get; set; }
    public string OperationType { get; set; }
    public string ExternalId { get; set; }
    public int CompanyNr { get; set; }
}
```

## Idempotency Store

```sql
CREATE TABLE dbo.INTEGRATION_IDEMPOTENCY
(
    ID              BIGINT IDENTITY(1,1) PRIMARY KEY,
    SOURCE_SYSTEM   VARCHAR(50) NOT NULL,
    OPERATION_TYPE  VARCHAR(100) NOT NULL,
    EXTERNAL_ID     VARCHAR(100) NOT NULL,
    COMPANY_NR      INT NOT NULL,
    STATUS          VARCHAR(20) NOT NULL,
    LOGICALREF      INT NULL,
    CORRELATION_ID  VARCHAR(64) NULL,
    CREATED_AT      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UPDATED_AT      DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE UNIQUE INDEX UX_INTEGRATION_IDEMPOTENCY
ON dbo.INTEGRATION_IDEMPOTENCY
(
    SOURCE_SYSTEM,
    OPERATION_TYPE,
    EXTERNAL_ID,
    COMPANY_NR
);
```

## İşlem Öncesi Kontrol

```text
Mesaj geldi
    ↓
Idempotency key üret
    ↓
Store'da var mı?
    ├── Completed → daha önce işlendi, tekrar Post etme
    ├── Processing → concurrent duplicate olabilir
    └── Yok → Processing kaydı oluştur
```

## Başarılı İşlem

```text
IData.Post başarılı
    ↓
LOGICALREF al
    ↓
Idempotency = Completed
    ↓
LogicalRef kaydet
```

## Başarısız İşlem

Kalıcı hata:

```text
Validation error
    ↓
Failed
    ↓
Retry yok
```

Geçici hata:

```text
Transient error
    ↓
RetryPending
    ↓
Backoff
```

## Retry Policy

Örnek politika:

```text
Attempt 1 → hemen
Attempt 2 → 30 saniye
Attempt 3 → 2 dakika
Attempt 4 → 10 dakika
Attempt 5 → dead-letter / manual review
```

Süreler sistemin operasyonel ihtiyacına göre ayarlanmalıdır.

## Retry Edilmemesi Gerekenler

- zorunlu field eksik
- cari kod bulunamadı
- malzeme kodu bulunamadı
- dönem kapalı
- belge iş kuralına aykırı
- birim dönüşümü geçersiz

## Retry Edilebilecekler

- geçici SQL timeout
- network kesintisi
- servis erişilemiyor
- geçici kaynak kilidi
- COM activation transient error

## Concurrent Duplicate

Aynı ExternalId iki worker tarafından aynı anda alınabilir.

Bunu engellemenin güvenli yolu yalnızca uygulama içi `lock` değildir; kalıcı store üzerinde unique constraint kullanılmalıdır.

## Compensating Action

Logo Post başarılı olduktan sonra entegrasyon mapping kaydı yazılamazsa aynı belgeyi tekrar üretmemek gerekir.

Bu nedenle recovery sırasında:

1. dış id üzerinden mevcut Logo kaydı aranabilir
2. LOGICALREF bulunursa mapping tamamlanabilir
3. doğrudan yeni Post yapılmamalıdır

## Transaction Helper

```csharp
public interface IIntegrationTransaction
{
    LogoOperationResult Execute(
        OperationContext context,
        Func<LogoOperationResult> action);
}
```

Bu helper:

- idempotency kontrolü
- correlation id
- log başlangıcı
- action execution
- retry classification
- result persistence

sorumluluklarını merkezi hale getirebilir.

## Temel Kural

> Retry mekanizması idempotency olmadan güvenli değildir. Logo entegrasyonunda ikisi birlikte tasarlanmalıdır.
