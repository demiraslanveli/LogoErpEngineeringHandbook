# 70 — Entegrasyon Log, Queue ve Reconciliation Modeli

## Amaç

Logo entegrasyonlarında yalnızca hata logu tutmak yeterli değildir. Sistemin hangi dış kaydı ne zaman aldığını, işleme başlayıp başlamadığını, Logo'da hangi kaydı oluşturduğunu ve başarısız kayıtların nasıl tekrar işlendiğini izleyebilmek gerekir.

## Önerilen Ana Tablolar

### IntegrationMessage

```text
Id
SourceSystem
ExternalId
MessageType
CompanyNr
PeriodNr
Payload
Status
RetryCount
NextRetryAt
CreatedAt
StartedAt
CompletedAt
LastError
CorrelationId
```

### IntegrationResult

```text
Id
IntegrationMessageId
LogoEntityType
LogoLogicalRef
LogoDocumentNo
CreatedAt
```

### IntegrationAttempt

```text
Id
IntegrationMessageId
AttemptNo
StartedAt
FinishedAt
Success
DurationMs
ErrorCode
ErrorMessage
```

## Status Modeli

Örnek durumlar:

```text
Pending
Processing
Completed
Failed
RetryScheduled
DeadLetter
Cancelled
```

Status geçişleri kontrol edilmelidir.

```text
Pending → Processing → Completed
                  ↘ Failed → RetryScheduled → Processing
                                  ↘ DeadLetter
```

## Unique Constraint

Duplicate aktarımı önlemek için doğal anahtar kullanılmalıdır.

Örnek:

```text
UNIQUE(SourceSystem, ExternalId, MessageType, CompanyNr)
```

Dönem business gereksinimine göre anahtara eklenebilir.

## Payload Saklama

Orijinal request payload'ının saklanması hata analizini kolaylaştırır.

Ancak kişisel veri veya secret içeriyorsa maskeleme/retention politikası uygulanmalıdır.

## Correlation ID

Bir işlem API'den SQL'e, worker'a ve Logo Objects'e kadar aynı correlation id ile izlenmelidir.

```text
API log
Queue row
Worker log
Logo result
SQL reconciliation
```

## Reconciliation Nedir?

Entegrasyon sistemindeki kayıt ile Logo ERP'deki gerçek kaydın periyodik olarak karşılaştırılmasıdır.

Örnek kontroller:

- `Completed` görünüyor ama LogoLogicalRef yok.
- LogoLogicalRef var ama Logo kaydı silinmiş.
- Aynı ExternalId için iki Logo belgesi var.
- IntegrationMessage `Processing` durumunda uzun süredir bekliyor.
- Logo kaydı var fakat entegrasyon sonucu kaydedilmemiş.

## Reconciliation Job

Periyodik job örneği:

```text
Her 15 dk:
- stuck Processing kayıtları
- unknown outcome kayıtları

Her gece:
- Completed ↔ Logo existence kontrolü
- duplicate business key kontrolü
```

## Stuck Processing

Worker crash olursa kayıt `Processing` durumunda kalabilir.

Bu nedenle lease/heartbeat yaklaşımı kullanılabilir.

```text
ProcessingStartedAt
WorkerId
LeaseUntil
```

`LeaseUntil < now` ise kayıt yeniden claim edilebilir.

## Audit Trail

Bir kaydın manuel retry edilmesi de loglanmalıdır.

```text
TriggeredBy
TriggerType = Automatic / Manual
Reason
```

## Operasyon Dashboard'u

Entegrasyon dashboard metrikleri:

- pending count
- processing count
- failed count
- dead-letter count
- avg duration
- p95 duration
- success rate
- retry rate
- oldest pending age

## Sonuç

Güvenilir Logo entegrasyonu, yalnızca başarılı kayıt üretmek değil; başarısız ve belirsiz kayıtları da izleyebilmek ve uzlaştırabilmektir. Queue + attempt log + result + reconciliation birlikte tasarlanmalıdır.
