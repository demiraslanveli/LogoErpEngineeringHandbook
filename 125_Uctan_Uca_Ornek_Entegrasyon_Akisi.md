# 125 — Uçtan Uca Örnek Entegrasyon Akışı

Bu bölüm, 100–124 arasındaki tüm mimari parçaları tek bir gerçekçi entegrasyon senaryosunda birleştirir.

Senaryo:

```text
Dış sistem satış siparişi oluşturur
        ↓
Entegrasyon servisi kuyruğa alır
        ↓
Logo ERP satış siparişi oluşturulur
        ↓
Sonuç loglanır
        ↓
Idempotency kaydı tamamlanır
        ↓
Reconciliation ile doğrulanır
```

## 1. Gelen Mesaj

```json
{
  "sourceSystem": "CRM",
  "sourceId": "SO-2026-100145",
  "companyNo": 40,
  "periodNo": 1,
  "customerCode": "120.01.001",
  "orderDate": "2026-08-12",
  "warehouseNo": 100,
  "lines": [
    {
      "itemCode": "MALZEME.001",
      "quantity": 10,
      "unitCode": "AD",
      "unitPrice": 150.00,
      "vatRate": 20
    }
  ]
}
```

## 2. CorrelationId

Mesaj alındığında yeni bir correlation id üretilir.

```csharp
var correlationId = Guid.NewGuid();
```

Bu değer tüm log, idempotency ve reconciliation kayıtlarında taşınır.

## 3. Idempotency Key

```text
CRM:SalesOrder:SO-2026-100145
```

İlk kontrol:

```text
Completed -> önceki sonucu dön
Processing -> duplicate concurrent işlem
Unknown -> reconciliation
Yok -> yeni işlem
```

## 4. Validation Pipeline

Kontroller:

```text
Firma geçerli mi?
Dönem geçerli mi?
Cari kodu dolu mu?
Cari Logo'da var mı?
Ambar geçerli mi?
En az bir satır var mı?
Malzeme kodları geçerli mi?
Birimler geçerli mi?
Miktar > 0 mı?
Fiyat geçerli mi?
KDV oranı geçerli mi?
```

Validation başarısızsa Logo çağrısı yapılmaz.

## 5. Reference Resolution

```text
customerCode -> CLIENTREF
itemCode     -> STOCKREF
unitCode     -> UOMREF
warehouseNo  -> SOURCEINDEX
```

Referanslar query adapter üzerinden okunur.

## 6. Mapping

Request DTO, servis modeline ve ardından Logo IData alanlarına map edilir.

```text
OrderCreateRequest
      ↓
OrderModel
      ↓
Logo IData
```

Mapper iş kuralı içermez.

## 7. Logo Session

İşlem için gerekli firma/dönem bağlamında session oluşturulur.

```text
Company = 040
Period  = 01
```

Session başka request ile paylaşılmaz.

## 8. IData Oluşturma

Konsept akış:

```csharp
var data = dataAdapter.Create(orderDataObjectType);

data.New();

// Header alanları
// Lines alanları

var postResult = data.Post();
```

Gerçek `DataObjectType` enum ve field isimleri kullanılan Logo Objects sürümünden doğrulanmalıdır.

## 9. Post Başarılı

Sonuç:

```text
LogoLogicalRef
LogoDocumentNo
CompletedAt
```

idempotency kaydına yazılır.

```text
Status = Completed
```

## 10. Post Sonucu Belirsiz

Örnek:

```text
Post çağrıldı
Logo kayıt oluşturdu
COM bağlantısı koptu
LogicalRef alınamadı
```

Durum:

```text
Status = Unknown
```

Doğrudan retry yapılmaz.

## 11. Reconciliation

Kaynak sistem kimliği veya entegrasyon anahtarı üzerinden Logo tarafında kayıt aranır.

Bulunursa:

```text
Unknown -> Completed
```

Bulunmazsa kontrollü retry değerlendirilir.

## 12. Structured Log

Örnek log context:

```text
CorrelationId
IdempotencyKey
SourceSystem
SourceId
CompanyNo
PeriodNo
Operation
LogoLogicalRef
LogoDocumentNo
DurationMs
Result
ErrorCode
ErrorMessage
ServiceVersion
```

## 13. Worker Davranışı

Worker:

```text
Queue item claim
    ↓
Correlation context
    ↓
Application service
    ↓
Result
    ↓
Queue status update
```

Worker Logo alanlarına doğrudan dokunmaz.

## 14. Retry

Retry yalnızca transient kabul edilen hatalarda çalışır.

Örnek:

```text
SQL timeout
geçici network problemi
geçici dependency erişim problemi
```

Validation ve business rule hatalarında retry yapılmaz.

## 15. Health Check

Servisin sağlıklı sayılması için yalnızca process çalışıyor olması yeterli değildir.

Kontrol edilebilir:

```text
SQL erişimi
Queue erişimi
Logo session oluşturabilme
Worker heartbeat
Son başarılı işlem zamanı
Pending/Failed queue büyüklüğü
```

## 16. Uçtan Uca Katman Haritası

```text
CRM
 ↓
Inbox / Queue
 ↓
Background Worker
 ↓
Application Service
 ↓
Validation Pipeline
 ↓
Idempotency Store
 ↓
Reference Resolver
 ↓
Mapper
 ↓
Logo Adapter
 ↓
IApplication / IData
 ↓
Logo ERP
 ↓
Operation Result
 ↓
Idempotency Complete
 ↓
Reconciliation Repository
 ↓
Monitoring / Audit
```

## 17. En Kritik Tasarım Kararları

1. Logo Objects çağrısı servis katmanı dışında dağılmamalıdır.
2. Firma/dönem context her işlemde açık olmalıdır.
3. Retry, idempotency olmadan uygulanmamalıdır.
4. Timeout doğrudan başarısız kabul edilmemelidir.
5. Reconciliation entegrasyon mimarisinin parçası olmalıdır.
6. CorrelationId tüm katmanlarda taşınmalıdır.
7. SQL yalnızca uygun okuma, queue, log ve entegrasyon altyapısı amaçlarıyla kullanılmalıdır.
8. Resmi Logo kart/fiş hareketlerinde IData/Objects tercih edilmelidir.

## Sonuç

Bu akış, kitabın ilk 99 bölümünde açıklanan Logo ERP bilgisini 100–124 arasındaki uygulama mimarisiyle birleştirir.

Artık yapı yalnızca teorik bir knowledge base değil, gerçek bir entegrasyon framework'ünün referans tasarımıdır.

> Amaç tek bir projeyi kopyalamak değil; Logo ERP entegrasyon projelerinde tekrar kullanılabilecek güvenli bir mühendislik standardı oluşturmaktır.
