# 73 — Outbox / Inbox Pattern ve Event-Driven Entegrasyon

## 1. Amaç

Bu bölüm, Logo ERP entegrasyonlarında veri kaybını ve mükerrer işlem riskini azaltmak için Outbox / Inbox pattern ve event-driven mimari yaklaşımını açıklar.

## 2. Problem

Doğrudan senkron entegrasyonlarda tipik risk:

```text
Kaynak sistem kaydı oluştu
    ↓
Logo çağrısı başladı
    ↓
Ağ / servis hatası
    ↓
Kaynak sistem işlem tamamlandı mı belirsiz
```

Bu durumda aynı işlem tekrar gönderilebilir veya hiç işlenmeyebilir.

## 3. Outbox Pattern

Kaynak sistem, kendi transaction'ı içinde hem iş kaydını hem de outbox kaydını oluşturur.

Örnek tablo:

```text
INTEGRATION_OUTBOX
------------------
ID
EVENT_TYPE
AGGREGATE_TYPE
AGGREGATE_ID
PAYLOAD
STATUS
RETRY_COUNT
CREATED_AT
PROCESSED_AT
LAST_ERROR
```

Durumlar:

- Pending
- Processing
- Completed
- Failed
- DeadLetter

## 4. Worker Akışı

```text
Pending kayıt
    ↓
Lock / claim
    ↓
Logo Objects işlemi
    ↓
Başarılı → Completed
    ↓
Hata → Retry / Failed
```

Worker aynı kaydı iki instance'ın paralel işlemesini engellemelidir.

## 5. Inbox Pattern

Dış sistemden alınan eventlerin mükerrer işlenmesini engellemek için Inbox tablosu kullanılır.

Örnek:

```text
INTEGRATION_INBOX
-----------------
SOURCE_SYSTEM
MESSAGE_ID
MESSAGE_TYPE
RECEIVED_AT
PROCESSED_AT
STATUS
```

`SOURCE_SYSTEM + MESSAGE_ID` benzersiz olmalıdır.

## 6. Idempotency Key

Logo kaydı oluşturulurken mümkünse dış sistem işlem kimliği özel alan, belge no veya entegrasyon tablosunda saklanmalıdır.

Örnek:

```text
MES-PROD-2026-000123
```

Aynı key tekrar geldiğinde yeni fiş oluşturmak yerine mevcut sonuç döndürülmelidir.

## 7. Event Tipleri

Örnek eventler:

```text
MaterialCreated
SalesOrderApproved
GoodsReceived
ProductionStarted
ProductionCompleted
LotReleased
ShipmentCompleted
InvoiceCreated
```

Event adı teknik tablo hareketini değil iş olayını temsil etmelidir.

## 8. Event Payload

Payload minimum gerekli bilgiyi içermelidir.

Örnek:

```json
{
  "eventId": "8bb5...",
  "eventType": "ProductionCompleted",
  "firmNr": 40,
  "periodNr": 1,
  "productionOrderRef": 4427,
  "externalTransactionId": "MES-987654"
}
```

## 9. Exactly Once Yanılgısı

Dağıtık sistemlerde gerçek anlamda exactly-once davranışı çoğu zaman pratik değildir.

Daha gerçekçi yaklaşım:

> At-least-once delivery + idempotent consumer.

Bu nedenle tekrar gelen event normal kabul edilmelidir.

## 10. Dead Letter Queue

Belirli retry sayısından sonra otomatik tekrar yerine kayıt DeadLetter durumuna alınmalıdır.

Örnek nedenler:

- Malzeme mapping yok
- Cari hesap mapping yok
- Logo iş kuralı kalıcı olarak engelliyor
- Veri formatı hatalı

DeadLetter kayıtları operasyon ekranından tekrar işlenebilmelidir.

## 11. Transaction Boundary

Logo Objects işlemi ile entegrasyon queue kaydı aynı SQL transaction'ında olmayabilir.

Bu nedenle sonuç Logo'dan döndükten sonra entegrasyon kaydına:

- `LOGICALREF`
- Belge numarası
- İşlem zamanı
- Sonuç kodu

kaydedilmelidir.

## 12. Event Ordering

Bazı eventler sıra bağımlıdır.

Örnek:

```text
ProductionOrderCreated
    ↓
ProductionStarted
    ↓
ProductionCompleted
```

`ProductionCompleted` önce gelirse worker bunu geçici hata olarak bekletebilir.

## 13. Partitioning

Yüksek hacimde queue aşağıdaki anahtarlara göre partition edilebilir:

- Firma
- Dönem
- Belge tipi
- Üretim emri
- Cari hesap

Aynı aggregate üzerindeki eventlerin aynı partition içinde sıralı işlenmesi yararlıdır.

## 14. Observability

Her event için takip edilmesi gereken bilgiler:

```text
CorrelationId
EventId
ExternalId
FirmNr
PeriodNr
Attempt
StartedAt
FinishedAt
DurationMs
LogoLogicalRef
Result
Error
```

## 15. Sonuç

Outbox/Inbox pattern, Logo entegrasyonunu güvenilir bir mesaj işleme sistemine dönüştürür.

En önemli kazanımlar:

- Mükerrer kayıtların önlenmesi
- Retry yönetimi
- Veri kaybının azaltılması
- Sorunlu kayıtların izlenebilmesi
- Event-driven mimariye geçiş
