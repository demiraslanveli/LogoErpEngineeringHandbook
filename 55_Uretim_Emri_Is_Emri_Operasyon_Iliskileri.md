# 55 — Üretim Emri, İş Emri ve Operasyon İlişkileri

## Amaç

Detaylı üretim tarafında yalnızca `PRODORD` tablosuna bakmak çoğu analiz için yeterli değildir. Üretim emri; iş emirleri, operasyonlar, malzeme ihtiyaçları, gerçekleşen sarflar, üretimden girişler ve maliyet hareketleriyle birlikte değerlendirilmelidir.

## Kavramsal Zincir

```text
Üretim Emri
    ↓
İş Emri / Operasyon Planı
    ↓
Operasyon Gerçekleşmeleri
    ↓
Sarf Hareketleri
    ↓
Üretimden Giriş
    ↓
Seri/Lot İzlenebilirliği
    ↓
Maliyetlendirme
```

## PRODORD

Üretim emrinin temel üst bilgisidir. Ürün, planlanan miktar, planlanan tarih, durum ve organizasyon bilgileri gibi alanlar bu seviyede izlenir.

Ancak gerçek üretim performansı yalnızca üretim emrinin durumundan çıkarılmamalıdır.

## İş Emri

Detaylı üretimde operasyonların yürütülmesi çoğu zaman iş emri seviyesinde takip edilir. İş emri, belirli bir üretim emrindeki belirli operasyonun planlanan/gerçekleşen çalışmasını temsil eder.

Tablo ve alan adları kullanılan Logo sürümü ve ürün modülüne göre doğrulanmalıdır.

## Operasyon

Operasyon; üretim rotasındaki iş adımıdır. Örnek:

```text
Karıştırma
Dolum
Paketleme
Kontrol
```

Operasyon analizinde tipik metrikler:

- planlanan süre
- gerçekleşen süre
- planlanan miktar
- gerçekleşen miktar
- fire
- iş merkezi
- başlangıç/bitiş zamanı

## Süre Analizi

Gerçek üretim sürelerinde alanların birimleri doğrulanmalıdır. Örneğin `ACTDURATION` gibi alanların saniye/dakika veya farklı iç ölçekte tutulması sürüme göre test edilmelidir.

İdeal rapor:

```text
Üretim Emri
Operasyon
İş Merkezi
Planlanan Süre
Gerçekleşen Süre
Sapma
Planlanan Miktar
Gerçekleşen Miktar
Verimlilik
```

## Malzeme Sarfı

Üretim emrine bağlı sarf hareketleri `STLINE` üzerinden analiz edilirken yalnızca ürün referansı değil üretim bağlantı alanları da incelenmelidir.

Temel kontrol:

```text
Üretim emri
↕
Sarf fişi/satırı
↕
Malzeme
↕
Seri/Lot
```

## Üretimden Giriş

Mamül üretimden giriş hareketi, planlanan üretimin gerçekten stoğa dönüştüğünü gösterir. Üretim emri kapanmış olsa bile üretimden giriş miktarı, seri/lot kaydı ve maliyet sonuçları ayrıca kontrol edilmelidir.

## Planlanan ve Gerçekleşen

Temel sapma hesabı:

```text
Miktar Sapması = Gerçekleşen - Planlanan
Süre Sapması   = Gerçekleşen Süre - Planlanan Süre
```

Yüzdesel analizlerde sıfıra bölme kontrolü yapılmalıdır.

## Üretim Bağlantılarını Bulma Yaklaşımı

Bir üretim emrini analiz ederken şu sırayla ilerlemek güvenlidir:

1. `PRODORD.LOGICALREF` bulunur.
2. İlgili iş emirleri bulunur.
3. Operasyon kayıtları bulunur.
4. Sarf stok hareketleri bulunur.
5. Üretimden giriş hareketleri bulunur.
6. Seri/lot bağlantıları kontrol edilir.
7. Maliyet hareketleri kontrol edilir.
8. Planlanan/gerçekleşen miktar ve süreler karşılaştırılır.

## ProductionApplication

Detaylı üretim işlemleri dış sistemden yönetilecekse SQL ile doğrudan üretim kayıtları oluşturmak yerine Logo’nun üretim nesneleri ve `ProductionApplication` katmanı tercih edilmelidir.

Çünkü üretim kaydı tek bir tablo insert'i değildir; çok sayıda bağlı iş kuralı ve hareket oluşabilir.

## Entegrasyon Idempotency

MES gibi dış sistemlerden operasyon bildirimi alınırken her bildirimin benzersiz bir dış işlem ID’si olmalıdır.

```text
ExternalOperationId
ProductionOrderRef
OperationRef
ReportedQuantity
ReportedDuration
Status
LogoResultRef
ProcessedAt
```

Aynı bildirim tekrar geldiğinde ikinci kez üretim hareketi oluşturulmamalıdır.

## Kontrol Listesi

Bir üretim emrini “tamamlandı” kabul etmeden önce:

- mamül miktarı doğru mu?
- sarf miktarları doğru mu?
- operasyonlar tamamlandı mı?
- süreler doğru mu?
- seri/lot kayıtları oluştu mu?
- üretimden giriş oluştu mu?
- maliyetlendirme tamamlandı mı?
- proje/fabrika/ambar boyutları doğru mu?

kontrol edilmelidir.
