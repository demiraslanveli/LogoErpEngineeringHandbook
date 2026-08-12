# 122 — Idempotency Store SQL Şeması

Bu bölüm, entegrasyon işlemlerinin tekrar çalıştırılması durumunda duplicate kayıt oluşmasını engellemek için kullanılacak SQL tabanlı idempotency store tasarımını tanımlar.

## Temel Problem

Aşağıdaki durumlarda aynı işlem birden fazla kez tetiklenebilir:

- HTTP timeout
- servis restart
- network kesintisi
- kullanıcı tekrar gönderimi
- queue retry
- batch yeniden çalıştırma
- SQL Agent job tekrar tetikleme
- dış sistemin aynı eventi yeniden yayınlaması

Logo tarafında işlem aslında tamamlanmış olabilir.

Bu nedenle:

```text
Timeout != Başarısız İşlem
```

## Idempotency Key

Her entegrasyon işlemi deterministik bir key ile temsil edilmelidir.

Örnek:

```text
MES:ProductionCompletion:2026:PO4427:Operation10
WMS:Dispatch:WH801:EXT-93845
CRM:SalesOrder:ORD-100145
```

## Önerilen Tablo

```sql
CREATE TABLE dbo.LOGO_INTEGRATION_IDEMPOTENCY
(
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    IDEMPOTENCY_KEY NVARCHAR(200) NOT NULL,
    SOURCE_SYSTEM NVARCHAR(50) NOT NULL,
    OPERATION_TYPE NVARCHAR(50) NOT NULL,
    COMPANY_NO INT NOT NULL,
    PERIOD_NO INT NULL,

    STATUS NVARCHAR(30) NOT NULL,

    LOGO_ENTITY NVARCHAR(50) NULL,
    LOGO_LOGICALREF INT NULL,
    LOGO_DOCUMENT_NO NVARCHAR(50) NULL,

    REQUEST_HASH NVARCHAR(128) NULL,
    RESPONSE_DATA NVARCHAR(MAX) NULL,
    ERROR_MESSAGE NVARCHAR(MAX) NULL,

    CORRELATION_ID UNIQUEIDENTIFIER NULL,

    CREATED_AT DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    STARTED_AT DATETIME2 NULL,
    COMPLETED_AT DATETIME2 NULL,
    LAST_ATTEMPT_AT DATETIME2 NULL,

    ATTEMPT_COUNT INT NOT NULL DEFAULT 0
);

CREATE UNIQUE INDEX UX_LOGO_INTEGRATION_IDEMPOTENCY_KEY
ON dbo.LOGO_INTEGRATION_IDEMPOTENCY(IDEMPOTENCY_KEY);
```

## Durumlar

Önerilen durum seti:

```text
Pending
Processing
Completed
Failed
Unknown
NeedsReconciliation
```

## İşlem Akışı

```text
Request geldi
    ↓
IdempotencyKey hesapla
    ↓
Kayıt var mı?
    ├── Completed -> mevcut sonucu dön
    ├── Processing -> duplicate concurrent request kontrolü
    ├── Unknown -> reconciliation çalıştır
    └── Yok -> Pending oluştur
    ↓
Processing
    ↓
Logo işlemi
    ↓
Completed / Failed / Unknown
```

## Request Hash

Aynı idempotency key ile farklı payload gelmesi kritik hatadır.

Örnek:

```text
Key = CRM:Order:145

İlk payload:
Amount = 100

İkinci payload:
Amount = 150
```

Bu durumda ikinci isteğin sessizce ilk sonucu kullanması doğru değildir.

Bu nedenle request hash saklanmalıdır.

```text
Same Key + Same Hash
    -> retry

Same Key + Different Hash
    -> conflict
```

## Atomic Claim

İki worker aynı kaydı aynı anda işlememelidir.

Örnek SQL yaklaşımı:

```sql
UPDATE dbo.LOGO_INTEGRATION_IDEMPOTENCY
SET
    STATUS = 'Processing',
    STARTED_AT = SYSUTCDATETIME(),
    LAST_ATTEMPT_AT = SYSUTCDATETIME(),
    ATTEMPT_COUNT = ATTEMPT_COUNT + 1
WHERE
    IDEMPOTENCY_KEY = @Key
    AND STATUS IN ('Pending', 'Failed');

IF @@ROWCOUNT = 0
BEGIN
    -- Başka worker claim etmiş olabilir.
END
```

Gerçek implementasyonda isolation level ve concurrency stratejisi yük profiline göre test edilmelidir.

## Unknown Durumu

En kritik durum budur.

Örnek:

```text
Logo Post()
    ↓
Logo kayıt oluşturdu
    ↓
Network / COM hata verdi
    ↓
Servis sonucu alamadı
```

Bu durumda kayıt doğrudan Failed olarak işaretlenmemelidir.

```text
Unknown
    ↓
Reconciliation
    ↓
Logo kaydı bulunduysa Completed
    ↓
Bulunmadıysa güvenli retry
```

## Cleanup

Completed kayıtlar sonsuza kadar sıcak tabloda tutulmak zorunda değildir.

Öneri:

- aktif tablo
- history/archive tablo
- retention süresi
- düzenli cleanup job

Ancak audit ve yasal gereksinimler dikkate alınmalıdır.

> Idempotency store, retry mekanizmasının güvenli çalışmasını sağlayan temel veri yapısıdır.
