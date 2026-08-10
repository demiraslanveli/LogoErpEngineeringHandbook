# 90 — Logo Servis Performansı ve Uçtan Uca Gecikme Analizi

## 1. Amaç

Logo Objects, REST Service veya özel middleware kullanan entegrasyonlarda performans yalnızca SQL süresinden ibaret değildir.

Toplam gecikme şu katmanların bileşimidir:

```text
İstemci
  ↓
API / Service
  ↓
Logo Objects / COM
  ↓
SQL Server
  ↓
Logo Objects dönüşümleri
  ↓
Serialization
  ↓
Network
  ↓
İstemci işleme
```

## 2. Ölçüm Noktaları

Her işlem için ayrı zaman ölçümü tutulmalıdır:

- request received
- Logo login/session hazır
- IData oluşturuldu
- validation tamamlandı
- Post başladı
- Post bitti
- response serialize edildi
- response gönderildi

## 3. Correlation ID

Her entegrasyon çağrısına benzersiz correlation ID verilmelidir.

Örnek:

```text
CORR-20260810-000145
```

Bu kimlik:

- API logunda
- integration queue'da
- error logunda
- Logo document reference alanında mümkünse

izlenebilir olmalıdır.

## 4. Login Cost

Her request için tekrar Logo login olmak pahalı olabilir.

Ancak session reuse tasarlanırken:

- COM thread affinity
- firma/dönem izolasyonu
- timeout
- exception sonrası session sağlığı

kontrol edilmelidir.

## 5. Batch vs Tekil İşlem

10.000 kayıt için 10.000 bağımsız request göndermek yerine kontrollü batch yaklaşımı düşünülebilir.

Ancak batch boyutu çok büyütülürse:

- transaction süresi
- memory kullanımı
- hata rollback kapsamı

artar.

## 6. SQL Süresi Düşük Ama API Yavaşsa

Kontrol edilmesi gerekenler:

- Logo Objects validation süresi
- COM çağrı sayısı
- DataFields üzerinde gereksiz tekrar erişim
- XML/JSON serialization
- network transferi
- synchronous blocking

## 7. N+1 Çağrı Problemi

Örneğin her satır için ayrı SQL veya Logo lookup çağrısı yapılması:

```text
1 fiş
100 satır
100 ayrı kart lookup
100 ayrı birim lookup
100 ayrı fiyat lookup
```

şeklinde N+1 probleme dönüşebilir.

Mümkün olduğunda toplu ön yükleme veya cache tasarlanmalıdır.

## 8. Cache Kullanımı

Cache için uygun adaylar:

- malzeme kodu → LOGICALREF
- cari kodu → LOGICALREF
- birim kodu → LOGICALREF
- ambar numarası
- proje kartı

Ancak cache invalidation stratejisi olmalıdır.

## 9. Timeout Tasarımı

Timeout yalnızca HTTP timeout değildir.

Ayrı katmanlar düşünülmelidir:

- DB command timeout
- Logo operation timeout
- worker execution timeout
- API gateway timeout

## 10. Performans Log Örneği

```text
CorrelationId: CORR-20260810-000145
Operation: PurchaseInvoicePost
SQL: 180 ms
LogoValidation: 420 ms
LogoPost: 780 ms
Serialization: 35 ms
Network: 25 ms
Total: 1440 ms
```

Bu yapı darboğazı net gösterir.

## 11. Temel Prensip

> Logo entegrasyon performansı uçtan uca ölçülmelidir; yalnızca SQL sorgusunu hızlandırmak toplam gecikmeyi açıklamayabilir.
