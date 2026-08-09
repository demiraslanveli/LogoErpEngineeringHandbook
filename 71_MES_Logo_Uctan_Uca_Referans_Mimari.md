# 71 — MES → Logo Uçtan Uca Referans Mimari

## Amaç

Bu bölüm, üretim sahasındaki MES/ara yazılım ile Logo Tiger Enterprise arasında kurulabilecek uçtan uca entegrasyon mimarisini referans model olarak açıklar.

## Hedef

MES operasyonel üretim verisini yönetebilir; Logo ise resmi ERP, stok, üretim, maliyet ve muhasebe kayıtlarının kaynağı olarak kalmalıdır.

## Referans Akış

```text
MES
 ↓
Integration API
 ↓
Validation
 ↓
Integration Queue
 ↓
Logo Worker
 ↓
ProductionApplication / IData
 ↓
Logo ERP
 ↓
Result + Reconciliation
 ↓
MES status update
```

## MES'ten Gelen Tipik Veriler

- üretim emri referansı
- operasyon
- iş merkezi
- başlangıç/bitiş zamanı
- üretilen miktar
- fire
- sarf edilen malzeme
- lot/seri
- operatör
- kalite sonucu

## Domain Mapping

MES alanları doğrudan Logo field adlarına bağlı olmamalıdır.

```text
MES DTO
 ↓
Domain Model
 ↓
Logo Mapping
 ↓
Logo Objects
```

Bu ayrım Logo sürüm değişikliklerinde entegrasyonun kırılmasını azaltır.

## Master Data Senkronizasyonu

MES'in ihtiyaç duyacağı master data genellikle Logo'dan sağlanır:

- malzeme
- birim
- ambar
- işyeri
- fabrika
- iş merkezi
- operasyon
- reçete
- proje

Master data senkronizasyonunda `LOGICALREF` tek başına dış sistem kimliği olarak kullanılmamalıdır; firma context'i de taşınmalıdır.

## Üretim Emri

Üretim emri Logo'da oluşturulup MES'e gönderilebilir.

```text
Logo PRODORD
 ↓
Integration Outbox
 ↓
MES
```

MES sonucu geri döndüğünde aynı üretim emri referansı ile resmi üretim hareketleri oluşturulur.

## Sarf

MES gerçek sarf miktarını gönderir.

Kontroller:

1. malzeme var mı?
2. doğru birim mi?
3. doğru ambar mı?
4. lot/seri gerekli mi?
5. ilgili üretim emri/iş emri doğru mu?
6. miktar negatif/0 mı?

Sarfın doğrudan STLINE insert ile oluşturulması yerine Logo'nun desteklenen iş nesnesi/ProductionApplication akışı tercih edilmelidir.

## Üretim Girişi

Mamül girişinde:

- miktar
- birim
- lot/seri
- SKT/üretim tarihi gibi izlenebilirlik bilgileri
- ambar
- proje
- üretim emri bağlantısı

birlikte ele alınmalıdır.

## Seri / Lot

MES lot numarası üretiyorsa numara üretme sahipliği net olmalıdır.

İki sistem aynı anda lot üretmemelidir.

```text
Lot owner = MES
veya
Lot owner = Logo
```

## Kalite

Kalite sonucu üretim hareketinden önce veya sonra gelebilir.

Örnek state:

```text
Produced
→ QualityPending
→ Released / Rejected
```

Logo'da kalite süreçleri aktifse bu state'in Logo kalite mekanizmalarıyla nasıl eşleşeceği ayrıca tasarlanmalıdır.

## Maliyet

MES'teki operasyon süreleri maliyet için kullanılacaksa:

- işçilik süresi
- makine süresi
- setup süresi
- fire
- gerçek sarf

Logo maliyet hesaplamasına doğru referanslarla aktarılmalıdır.

MES kendi maliyetini hesaplıyor olsa bile resmi muhasebe maliyetinin hangi sistem tarafından üretildiği açık olmalıdır.

## Outbox Pattern

Logo'dan MES'e giden veriler için custom outbox kullanılabilir.

```text
Logo event / job
 ↓
IntegrationOutbox
 ↓
Publisher
 ↓
MES
```

Bu sayede MES geçici olarak kapalı olsa bile veri kaybolmaz.

## Inbound Idempotency

MES'ten gelen her işlem benzersiz external id taşımalıdır.

```text
MES-ProductionId
MES-ConsumptionId
MES-OperationResultId
```

## Reconciliation

Günlük karşılaştırma örnekleri:

- MES üretim miktarı ↔ Logo üretim girişi
- MES sarf ↔ Logo sarf hareketi
- MES lot ↔ Logo lot
- MES completed order ↔ Logo PRODORD status

## Hata Yönetimi

İş kuralı hatası MES'e açıkça dönmelidir.

Örnek:

```text
ITEM_NOT_FOUND
UNIT_MISMATCH
WAREHOUSE_NOT_ALLOWED
LOT_REQUIRED
PRODORD_NOT_FOUND
DUPLICATE_EXTERNAL_ID
```

## Mimari Sorumluluklar

### MES

- saha verisinin doğruluğu
- makine/operatör olayları
- operasyon zamanları
- gerçek üretim miktarları

### Integration Layer

- mapping
- validation
- idempotency
- retry
- log
- queue
- reconciliation

### Logo

- resmi master data
- resmi stok hareketleri
- üretim kayıtları
- seri/lot izlenebilirliği
- maliyetlendirme
- muhasebe entegrasyonu

## Sonuç

Başarılı MES–Logo entegrasyonu, iki sistem arasında tablo kopyalamak değildir. Domain sınırları, kayıt sahipliği ve resmi ERP hareketlerinin hangi katmanda üretileceği net tanımlanmalıdır.
