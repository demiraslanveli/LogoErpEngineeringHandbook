# 105 — Structured Logging, CorrelationId ve Audit Context

Bu bölüm Logo entegrasyon uygulamasında logların yalnızca metin mesajı değil, sorgulanabilir operasyon verisi olarak tasarlanmasını açıklar.

## Amaç

Üretim ortamında şu sorulara hızlı cevap verilebilmelidir:

- hangi dış kayıt Logo'ya işlendi?
- hangi firma/dönemde çalıştı?
- hangi belge oluştu?
- hangi LOGICALREF üretildi?
- işlem kaç kez retry edildi?
- hata hangi katmanda oluştu?
- aynı isteğin tüm logları nasıl bulunur?

Bu nedenle structured logging ve correlation id zorunlu kabul edilmelidir.

## CorrelationId

Her logical integration operation benzersiz bir kimlik taşımalıdır.

```csharp
public sealed class OperationContext
{
    public string CorrelationId { get; set; }
    public int CompanyNr { get; set; }
    public int PeriodNr { get; set; }
    public string OperationType { get; set; }
    public string ExternalId { get; set; }
}
```

CorrelationId dış sistemden gelmiyorsa uygulama üretmelidir.

```csharp
var correlationId = Guid.NewGuid().ToString("N");
```

## Structured Log Modeli

```csharp
public sealed class IntegrationLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; }
    public string CorrelationId { get; set; }
    public int CompanyNr { get; set; }
    public int PeriodNr { get; set; }
    public string OperationType { get; set; }
    public string ExternalId { get; set; }
    public int? LogicalRef { get; set; }
    public string Message { get; set; }
    public string ErrorCode { get; set; }
    public int RetryCount { get; set; }
    public long DurationMs { get; set; }
}
```

## Log Seviyeleri

Önerilen kullanım:

```text
Information → normal operasyon akışı
Warning     → işlem tamamlandı fakat dikkat gereken durum
Error       → işlem başarısız
Critical    → servis veya entegrasyon altyapısı çalışamıyor
Debug       → geliştirme / detay teşhis bilgisi
```

## Örnek Akış

```text
INFO  OperationStarted
INFO  LogoSessionOpened
INFO  ValidationCompleted
INFO  LogoPostStarted
INFO  LogoPostCompleted
INFO  OperationCompleted
```

Hata durumunda:

```text
INFO   OperationStarted
INFO   LogoSessionOpened
ERROR  LogoPostFailed
WARN   RetryScheduled
```

## Süre Ölçümü

Her Logo çağrısının süresi ölçülmelidir.

```csharp
var sw = Stopwatch.StartNew();

try
{
    // Logo operation
}
finally
{
    sw.Stop();
    logger.LogDuration(sw.ElapsedMilliseconds);
}
```

Bu veri servis yavaşlığının SQL, Logo Objects veya istemci katmanından kaynaklanıp kaynaklanmadığını anlamaya yardım eder.

## Hassas Veri

Loglara şu bilgiler açık şekilde yazılmamalıdır:

- kullanıcı parolası
- connection string password
- erişim token'ı
- özel anahtar
- gereksiz kişisel veri

## SQL Log Tablosu

Basit bir merkezi log tablosu örneği:

```sql
CREATE TABLE dbo.INTEGRATION_LOG
(
    ID              BIGINT IDENTITY(1,1) PRIMARY KEY,
    CREATED_AT      DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CORRELATION_ID  VARCHAR(64) NULL,
    COMPANY_NR      INT NULL,
    PERIOD_NR       INT NULL,
    OPERATION_TYPE  VARCHAR(100) NULL,
    EXTERNAL_ID     VARCHAR(100) NULL,
    LOGICALREF      INT NULL,
    LEVEL_NAME      VARCHAR(20) NOT NULL,
    ERROR_CODE      VARCHAR(100) NULL,
    MESSAGE         NVARCHAR(MAX) NULL,
    RETRY_COUNT     INT NOT NULL DEFAULT 0,
    DURATION_MS     BIGINT NULL
);
```

Bu tablo Logo'nun kendi tablolarından ayrı entegrasyon veritabanında tutulabilir.

## Index Önerisi

```sql
CREATE INDEX IX_INTEGRATION_LOG_CORRELATION
ON dbo.INTEGRATION_LOG(CORRELATION_ID);

CREATE INDEX IX_INTEGRATION_LOG_EXTERNAL
ON dbo.INTEGRATION_LOG(COMPANY_NR, OPERATION_TYPE, EXTERNAL_ID);
```

## Audit ile Operational Log Ayrımı

Operational log:

```text
ne oldu?
ne kadar sürdü?
hata oldu mu?
```

Audit:

```text
hangi kayıt hangi değerden hangi değere değişti?
kim değiştirdi?
ne zaman değiştirdi?
```

İki amaç aynı tabloya zorlanmamalıdır.

## Temel Kural

> Üretim entegrasyonunda her iş uçtan uca tek bir CorrelationId ile izlenebilir olmalıdır.
