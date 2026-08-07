# 10 — Detaylı Üretim

## 1. Bölümün Amacı

Bu bölüm, Logo Tiger / Tiger Wings Enterprise detaylı üretim yapısının iş mantığını ve entegrasyon bakış açısını daha ayrıntılı ele alır.

Bir önceki `ProductionApplication` bölümünde uygulama katmanı ve entegrasyon mimarisi açıklanmıştı. Bu bölümde ise üretim sürecinin ERP içindeki mantıksal bileşenleri ele alınır.

Odak noktaları:

- Üretim emri
- Reçete
- Operasyon
- İş istasyonu
- İş emri
- Sarf
- Fire
- Mamul/yarı mamul üretimi
- Seri/lot
- Planlanan ve gerçekleşen değerler
- Maliyetlendirme ilişkisi

---

## 2. Detaylı Üretimin Temel Mantığı

Detaylı üretim, yalnızca bir mamul stok girişinden ibaret değildir.

Üretim sistemi şu soruların tamamına cevap verebilmelidir:

- Ne üretilecek?
- Ne kadar üretilecek?
- Hangi reçeteyle üretilecek?
- Hangi hammaddeler kullanılacak?
- Hangi operasyonlardan geçecek?
- Hangi iş istasyonlarında çalışılacak?
- Ne kadar süre harcanacak?
- Gerçekte ne kadar üretildi?
- Ne kadar sarf edildi?
- Ne kadar fire oluştu?
- Hangi seri/lotlar kullanıldı?
- Üretilen mamul hangi lotla oluştu?
- Üretimin gerçek maliyeti nedir?

Bu nedenle detaylı üretim, stok + operasyon + izlenebilirlik + maliyet bütünlüğü olarak değerlendirilmelidir.

---

## 3. Üretim Emri

Üretim emri, üretim sürecinin ana belgesidir.

Tipik olarak aşağıdaki bilgileri taşır:

- Mamul
- Planlanan miktar
- Üretim başlangıç tarihi
- Bitiş tarihi
- Reçete
- Proje
- Ambar
- Durum
- İlgili operasyonlar

Üretim emrinin `LOGICALREF` değeri birçok bağlı hareketin ana referansı olabilir.

Harici sistemlerde üretim emri numarası ve ref birlikte saklanabilir; ancak harici benzersiz ID de ayrıca tutulmalıdır.

---

## 4. Reçete

Reçete mamulün nasıl üretileceğini tanımlar.

Örnek:

```text
Mamul: Ürün X
Planlanan: 1.000 KG

Hammadde A: 500 KG
Hammadde B: 300 KG
Hammadde C: 150 KG
Ambalaj: 1000 ADET
Fire: %2
```

Gerçek üretim sırasında reçete değerleri ile gerçekleşen sarf farklı olabilir.

Bu fark, reçete sapma analizinde önemlidir.

---

## 5. Reçete ve Gerçekleşen Sarf Ayrımı

Yanlış yaklaşım:

```text
Reçetede 500 KG yazıyor
→ Gerçek sarf da 500 KG kabul edilir
```

Doğru yaklaşım:

```text
Planlanan sarf: 500 KG
Gerçek sarf: 512 KG
Sapma: +12 KG
```

Gerçek sarf, maliyetlendirme açısından kritik olabilir.

---

## 6. Operasyon

Operasyon, üretim sürecindeki iş adımlarını temsil eder.

Örnek:

```text
10 — Tartım
20 — Karıştırma
30 — Dolum
40 — Paketleme
```

Her operasyon:

- İş istasyonu
- Planlanan süre
- Gerçekleşen süre
- Operatör
- Miktar
- Fire

gibi bilgilerle ilişkilendirilebilir.

---

## 7. İş İstasyonu

İş istasyonu üretimin fiziksel veya mantıksal gerçekleştirildiği kaynaktır.

Örnek:

```text
MIXER-01
FILLING-LINE-02
PACK-01
```

İş istasyonları kapasite planlama ve maliyet analizinde kullanılabilir.

---

## 8. İş Emri

Üretim emrinin operasyon bazında uygulanabilir iş parçalarına ayrılması iş emirleri üzerinden yönetilebilir.

Kavramsal yapı:

```text
Üretim Emri
   │
   ├── İş Emri / Operasyon 10
   ├── İş Emri / Operasyon 20
   ├── İş Emri / Operasyon 30
   └── İş Emri / Operasyon 40
```

MES entegrasyonunda çoğu zaman operatör doğrudan iş emri seviyesinde çalışır.

---

## 9. Planlanan Süre ve Gerçek Süre

Örnek:

```text
Planlanan süre: 120 dakika
Gerçek süre: 145 dakika
Sapma: +25 dakika
```

Gerçek süre bilgisi:

- Operasyon performansı
- Kapasite planlama
- İşçilik maliyeti
- Makine maliyeti
- OEE

için kullanılabilir.

---

## 10. ACTDURATION

Logo üretim raporlarında `ACTDURATION` benzeri alanlar gerçekleşen operasyon süresini temsil edebilir.

Bu alan sorgulanırken:

- Biriminin ne olduğu
- Hangi kayıt seviyesinde tutulduğu
- Operasyon mu iş emri mi temsil ettiği
- Tamamlanan ve açık kayıt davranışı

kullanılan sürüm üzerinden doğrulanmalıdır.

---

## 11. ACTAMOUNT

`ACTAMOUNT` benzeri alanlar gerçekleşen miktarı temsil edebilir.

Planlanan miktar ile gerçekleşen miktarın ayrıştırılması gerekir.

Örnek:

```text
Planlanan: 10.000
ACTAMOUNT: 9.840
```

Bu fark üretim kaybı, fire veya eksik gerçekleşme olarak analiz edilebilir.

---

## 12. Sarf

Üretimde hammadde çıkışı sarf olarak değerlendirilir.

Sarf sırasında bilinmesi gereken minimum bilgiler:

- Üretim emri
- Malzeme
- Miktar
- Birim
- Ambar
- Stok yeri
- Seri/lot
- Tarih

Eksik bağlantıyla yapılan stok çıkışı üretim maliyetine doğru yansımayabilir.

---

## 13. Mamul Girişi

Üretim tamamlandığında mamul veya yarı mamul stoka girer.

Bu giriş:

- Üretim emrine bağlı olmalı
- Doğru ambara yapılmalı
- Seri/lot bilgisi doğru olmalı
- Gerçekleşen miktarı temsil etmeli
- Maliyet bağlantısını korumalıdır

---

## 14. Yarı Mamul

Çok aşamalı üretimde ara ürünler yarı mamul olarak yönetilebilir.

Örnek:

```text
Hammadde
   ↓
Bulk Ürün
   ↓
Dolum
   ↓
Ambalajlı Mamul
```

Her aşama ayrı stok hareketi ve üretim emri üretebilir.

Bu durumda izlenebilirlik zinciri daha da önem kazanır.

---

## 15. Fire

Fire, planlanan girdinin tamamının mamule dönüşmediği durumları temsil eder.

Örnek:

```text
Girdi: 1.000 KG
Mamul: 970 KG
Fire: 30 KG
```

Fire:

- Normal fire
- Anormal fire
- Operasyon kaynaklı kayıp
- Numune
- Kalite reddi

gibi kategorilere ayrılabilir.

Maliyet hesapları fire türüne göre farklı sonuç verebilir.

---

## 16. Yan Ürün

Bazı üretimlerde ana mamul dışında yan ürün oluşabilir.

Örnek:

```text
Ana mamul: 950 KG
Yan ürün: 30 KG
Fire: 20 KG
```

Yan ürünlerin stok ve maliyet davranışı proje tasarımında netleştirilmelidir.

---

## 17. Seri/Lot Bağlantısı

Detaylı üretimin izlenebilirlik zinciri:

```text
Hammadde Lotları
       ↓
Sarf Hareketleri
       ↓
Üretim Emri
       ↓
Mamul Lotu
```

Bu bağlantı geri çağırma ve kalite analizleri için kritik öneme sahiptir.

---

## 18. İleri İzlenebilirlik

Soru:

> Bu hammadde lotu nerelerde kullanıldı?

Örnek cevap:

```text
Hammadde Lot: HM-2026-00125

Kullanıldığı mamuller:
- MM-2026-00551
- MM-2026-00552
- MM-2026-00558
```

Bu analiz özellikle ilaç ve gıda sektöründe zorunlu olabilir.

---

## 19. Geri İzlenebilirlik

Soru:

> Bu mamul lotu hangi hammadde lotlarından üretildi?

Örnek:

```text
Mamul Lot: MM-2026-00551

Hammadde:
HM-A-260701
HM-B-260708
HM-C-260709
```

Logo veri modeli içinde bu zincirin eksiksiz tutulması gerekir.

---

## 20. Kalite ile İlişki

Üretim sonrasında mamul doğrudan kullanılabilir stok olmayabilir.

Örnek süreç:

```text
Üretildi
   ↓
Karantina
   ↓
Numune Alındı
   ↓
Kalite Analizi
   ↓
Onaylandı
   ↓
Serbest Stok
```

Entegrasyon kalite kontrol adımlarını atlamamalıdır.

---

## 21. Maliyetlendirme

Detaylı üretimin maliyeti yalnızca hammadde toplamından oluşmayabilir.

Örnek maliyet bileşenleri:

- Hammadde
- Ambalaj
- İşçilik
- Makine
- Genel üretim gideri
- Enerji
- Fire

Üretim hareketleri doğru ilişkilendirilmezse mamul maliyeti hatalı oluşabilir.

---

## 22. Üretim Maliyeti Kontrolü

Entegrasyon kabul testinde aşağıdaki kontrol yapılmalıdır:

```text
Üretim tamamlandı mı?      ✓
Stok doğru mu?             ✓
Lot ilişkisi doğru mu?     ✓
Maliyet oluştu mu?         ?
```

Maliyet kontrol edilmeden entegrasyonun tamamlandığı kabul edilmemelidir.

---

## 23. Üretim Tarihi

Üretim işlemlerinde tarih alanları dikkatle yönetilmelidir.

Örneğin:

- Üretim emri tarihi
- Planlanan başlangıç
- Gerçek başlangıç
- Gerçek bitiş
- Stok hareket tarihi
- Lot üretim tarihi
- Son kullanma tarihi

birbirinden farklı kavramlardır.

---

## 24. Son Kullanma Tarihi

Lot takipli ürünlerde SKT üretim entegrasyonunun önemli bir parçasıdır.

Örnek:

```text
Üretim tarihi: 07.08.2026
Raf ömrü: 24 ay
SKT: 07.08.2028
```

SKT hesaplama kuralı ürün grubuna göre değişebilir ve merkezi parametreyle yönetilmelidir.

---

## 25. Ambarlar

Üretim sürecinde birden fazla ambar kullanılabilir.

Örnek:

```text
Hammadde Ambarı
      ↓
Üretim Ambarı
      ↓
Karantina
      ↓
Mamul Ambarı
```

Sarf ve giriş hareketlerinde ambarların doğru kullanılması stok doğruluğu için kritiktir.

---

## 26. Stok Yeri

Ambar içinde stok yerleri varsa detay daha da artar.

Örnek:

```text
Ambar 4
  ├── RAF-A01
  ├── RAF-A02
  └── RAF-B01
```

Lot ve stok yeri birlikte takip edildiğinde hangi lotun hangi rafta olduğu bilinir.

---

## 27. MES Entegrasyonu

MES’in amacı Logo’nun üretim iş kurallarını kopyalamak değildir.

Önerilen görev dağılımı:

```text
MES:
- Operatör deneyimi
- Makine verisi
- Tartım
- Barkod
- Süre
- Gerçekleşen miktar

Logo:
- Resmi üretim emri
- Stok
- Lot
- Maliyet
- Muhasebe
```

---

## 28. Üretim Bildirim Paketi

Harici sistemden ERP’ye örnek payload mantığı:

```json
{
  "externalId": "PRD-2026-000154",
  "productionOrder": "UE-000154",
  "productCode": "150.MM.001",
  "quantity": 980,
  "lot": "MM26080701",
  "consumptions": [
    {
      "itemCode": "150.HM.001",
      "quantity": 502,
      "lot": "HM26070101"
    }
  ]
}
```

Bu veri Objects/ProductionApplication katmanında Logo nesnelerine dönüştürülmelidir.

---

## 29. Ön Validasyon

Logo’ya kayıt göndermeden önce:

- Üretim emri var mı?
- Açık mı?
- Mamul kodu doğru mu?
- Sarf malzemeleri doğru mu?
- Lotlar var mı?
- Stok yeterli mi?
- Ambarlar doğru mu?
- Miktar pozitif mi?
- Aynı external ID işlendi mi?

kontrol edilmelidir.

---

## 30. Post Sonrası Kontrol

Başarılı `Post()` her zaman işin tamamen bittiği anlamına gelmez.

Sonrasında:

- Logo ref alındı mı?
- Üretim emri gerçekleşmesi güncellendi mi?
- Sarf satırları oluştu mu?
- Mamul girişi oluştu mu?
- Lot dağıtımı oluştu mu?
- Mutabakat doğru mu?

kontrol edilebilir.

---

## 31. Hata Kuyruğu

Üretim hataları kaybolmamalıdır.

Örnek hata kaydı:

```text
ExternalId: PRD-2026-000154
ProductionOrder: UE-000154
Status: FAILED
ErrorType: BUSINESS
Error: Lot HM26070101 stokta bulunamadı
RetryCount: 0
```

Bu kayıt operasyona görünür olmalıdır.

---

## 32. Mutabakat

Günlük mutabakat örneği:

```text
MES üretim bildirimi: 154
Logo başarılı kayıt:   151
Hatalı:                  3
Bekleyen:                 0
```

Miktar bazlı mutabakat da yapılmalıdır.

---

## 33. SQL Kontrol Sorguları

SQL, üretim sonrası kontrol için kullanılabilir.

Örneğin:

```sql
SELECT
    LOGICALREF,
    FICHENO,
    TRCODE,
    DATE_
FROM LG_XXX_YY_STFICHE
WHERE DATE_ = @Date;
```

Ancak yalnızca stok fişlerine bakarak üretim sürecinin tam olduğu varsayılmamalıdır.

---

## 34. Performans

Yüksek hacimli üretimde:

- Tek tek login yapılmamalı
- Malzeme/ref lookup cachelenmeli
- Lot kontrolleri toplu yapılmalı
- Gereksiz SQL round-trip azaltılmalı
- İşlemler kontrollü batch’lere ayrılmalı
- Retry sınırlı olmalı

---

## 35. Best Practices

### Yapılması önerilenler

- Üretim emri merkezli tasarla.
- Planlanan ve gerçekleşeni ayır.
- Gerçek sarfı kaydet.
- Operasyon sürelerini ayrı tut.
- Lot izlenebilirliğini uçtan uca koru.
- Fire ve yan ürünü modelle.
- Maliyet sonucu kontrol et.
- MES ve ERP veri sahipliğini net tanımla.
- Her işlemde idempotency kullan.
- Günlük mutabakat yap.

### Kaçınılması gerekenler

- Üretimi sadece stok giriş/çıkışı olarak görmek.
- Reçete miktarını gerçek sarf kabul etmek.
- Üretim fişlerini SQL insert ile üretmek.
- Lot bağlantısını yalnızca MES’te tutmak.
- Üretim tamamlandıktan sonra maliyeti kontrol etmemek.
- Plan ve gerçekleşen alanları aynı amaçla kullanmak.

---

## 36. Sonuç

Detaylı üretim sisteminde doğru veri zinciri:

```text
Üretim Emri
   ↓
Reçete
   ↓
Operasyon / İş Emri
   ↓
Gerçek Sarf
   ↓
Seri/Lot
   ↓
Mamul Üretimi
   ↓
Kalite
   ↓
Maliyetlendirme
```

şeklinde düşünülmelidir.

Bu zincirin herhangi bir halkasının atlanması ilk anda stok miktarını bozmayabilir; ancak ileride izlenebilirlik, maliyet, kalite veya muhasebe problemleri doğurabilir.
