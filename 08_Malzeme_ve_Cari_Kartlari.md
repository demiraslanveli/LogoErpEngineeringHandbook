# 08 — Malzeme ve Cari Kartları

## 1. Bölümün Amacı

Bu bölüm, Logo ERP'de iki temel master data grubunun entegrasyon ve geliştirme açısından nasıl ele alınması gerektiğini açıklar:

- Malzeme kartları
- Cari hesap kartları

Bu kartlar birçok operasyonel fişin referans aldığı ana kayıtlardır. Bu nedenle kart oluşturma ve güncelleme işlemleri yalnızca tabloya kayıt atma işlemi olarak görülmemelidir.

Temel prensip:

> Kartlar sistemin ana verisidir; fişlerden önce doğru kodlama, birim, sınıflandırma ve iş kurallarıyla yönetilmelidir.

---

## 2. Master Data Kavramı

ERP sistemlerinde master data, işlemlerin dayandığı kalıcı referans verisidir.

Logo açısından örnekler:

- Malzeme kartı
- Cari hesap kartı
- Birim seti
- Ambar
- Proje
- Muhasebe hesabı
- Fiyat kartı
- İş yeri

Bir satış faturası oluşturulmadan önce cari hesap ve malzeme kartlarının mevcut ve doğru olması gerekir.

---

## 3. Malzeme Kartının Rolü

Malzeme kartı yalnızca kod ve açıklamadan oluşmaz.

Tipik olarak aşağıdaki boyutlar bulunur:

- Malzeme kodu
- Açıklama
- Kart türü
- Grup kodu
- Özel kodlar
- Yetki kodu
- Birim seti
- Ana birim
- Alternatif birimler
- Barkodlar
- KDV bilgileri
- İzleme yöntemi
- Seri/lot ayarları
- Stok yeri kullanımı
- Muhasebe bağlantıları
- Satınalma/satış parametreleri

Bu alanların bir kısmı fiş davranışını doğrudan etkiler.

---

## 4. Kod Tasarımı

Malzeme kodu entegrasyonların en kritik iş anahtarlarından biridir.

İyi bir kod sistemi:

- Benzersizdir.
- Değiştirilmesi nadirdir.
- Harici sistemlerle eşleştirilebilir.
- İnsan tarafından okunabilirliği gerektiği kadar korur.

Örnek:

```text
150.HE.01.00043.VIL
```

Kodun anlamlı segmentlere ayrılması yararlı olabilir; ancak kod yapısı aşırı iş kuralı taşımamalıdır.

---

## 5. LOGICALREF ve CODE Ayrımı

Logo veritabanında kayıtların teknik anahtarı genellikle `LOGICALREF` değeridir.

Ancak entegrasyon anahtarı olarak çoğu durumda `CODE` daha uygundur.

Örnek:

```text
Firma 040:
CODE = T30.100.010
LOGICALREF = 43338

Firma 202:
CODE = T30.100.010
LOGICALREF = farklı olabilir
```

Bu nedenle test ve canlı ortamlar arasında `LOGICALREF` taşımak risklidir.

---

## 6. Malzeme Kartı Oluşturma

Genel Objects yaklaşımı:

```csharp
IData item = application.NewDataObject(DataObjectType.doMaterial);

item.New();
item.DataFields.FieldByName("CODE").Value = "150.001";
item.DataFields.FieldByName("NAME").Value = "Örnek Malzeme";
item.DataFields.FieldByName("AUXIL_CODE").Value = "HAMMADDE";

if (!item.Post())
{
    // Hata detayları kaydedilmelidir.
}
```

Nesne tipi ve alan adları kullanılan Logo ürün/sürümüne göre doğrulanmalıdır.

---

## 7. Birim Seti

Malzeme kartında birim seti kritik öneme sahiptir.

Örnek:

```text
Ana birim: KG
İkinci birim: KOLİ
Üçüncü birim: ADET
```

Birim ilişkileri yanlış tanımlanırsa:

- Stok miktarları yanlış yorumlanabilir.
- Satınalma fiyatları hatalı kıyaslanabilir.
- Fatura miktarları yanlış çevrilebilir.
- Envanter raporları tutarsız görünebilir.

---

## 8. Birim Çevrimleri

Logo tarafında birim dönüşümleri genellikle oran alanları üzerinden tutulur.

Kavramsal örnek:

```text
1 KOLİ = 12 ADET
```

Birim dönüşüm mantığı yalnızca raporlama için değildir; fiş satırlarında miktar ve fiyat davranışını etkileyebilir.

Özellikle çift birimli malzemelerde satınalma fiyatı kontrol edilirken fiyatın hangi birime ait olduğu açıkça bilinmelidir.

---

## 9. Barkodlar

Bir malzemenin farklı birimleri için farklı barkodları olabilir.

Örnek:

```text
ADET → 869000000001
KOLİ → 869000000018
```

Barkod entegrasyonunda sadece malzeme kartına değil, ilgili birim satırına bağlantı kurulması gerekebilir.

Barkodların benzersizliği kontrol edilmelidir.

---

## 10. Malzeme Kartında Seri/Lot

İzlenebilirlik gerektiren ürünlerde kart seviyesinde seri/lot ayarları doğru yapılmalıdır.

Örneğin:

- Seri takipli
- Lot takipli
- Takipsiz

Bu ayar sonradan değiştirildiğinde mevcut hareketlerle uyumsuzluk yaratabilir.

Bu nedenle kart açılışında ürün sınıfına göre doğru takip yöntemi belirlenmelidir.

---

## 11. Malzeme Kartı Güncelleme

Kart güncelleme işlemlerinde Objects üzerinden kayıt okunup tekrar post edilmesi tercih edilir.

```csharp
IData item = application.NewDataObject(DataObjectType.doMaterial);

if (item.Read(itemRef))
{
    item.DataFields.FieldByName("NAME").Value = "Yeni Açıklama";
    item.Post();
}
```

Doğrudan SQL update kullanmak bazı senaryolarda hızlı görünse de kartın bağlı yapılarını ve Logo iş kurallarını atlayabilir.

---

## 12. Cari Hesap Kartı

Cari hesap kartları satış ve satınalma süreçlerinin temel master datasıdır.

Tipik bilgiler:

- Cari kod
- Unvan
- Adres
- Vergi numarası
- Vergi dairesi
- TCKN
- E-posta
- Telefon
- Özel kodlar
- Yetki kodu
- Ödeme planı
- Risk bilgileri
- Döviz kullanımı
- Muhasebe bağlantıları

---

## 13. Cari Kod Tasarımı

Cari kod da malzeme kodu gibi kalıcı bir iş anahtarıdır.

Örnek:

```text
120.01.0001
320.01.0001
```

Ancak muhasebe hesabı mantığını doğrudan cari kod formatına gömmek her projede doğru değildir.

Kodlama standardı firma genelinde tanımlanmalıdır.

---

## 14. Cari Hesap Oluşturma

Kavramsal Objects örneği:

```csharp
IData arp = application.NewDataObject(DataObjectType.doAccountsRP);

arp.New();
arp.DataFields.FieldByName("CODE").Value = "120.01.001";
arp.DataFields.FieldByName("TITLE").Value = "Örnek Müşteri A.Ş.";
arp.DataFields.FieldByName("TAX_ID").Value = "1234567890";

if (!arp.Post())
{
    // Validation hataları kaydedilmelidir.
}
```

Alan isimleri sürüme göre doğrulanmalıdır.

---

## 15. Vergi Numarası ve TCKN Kontrolü

Cari entegrasyonunda duplikasyon yalnızca cari kod üzerinden kontrol edilmemelidir.

Özellikle:

- Vergi numarası
- TCKN
- E-posta
- Harici sistem müşteri ID'si

ikinci kontrol anahtarı olarak değerlendirilebilir.

Örnek hata:

```text
CRM'de müşteri kodu değişti
→ entegrasyon yeni cari açtı
→ aynı vergi numarasına iki cari oluştu
```

Bu nedenle master data entegrasyonunda eşleştirme stratejisi önceden belirlenmelidir.

---

## 16. Harici Sistem ID'si

CRM, e-ticaret veya başka bir ERP ile entegrasyonda her kart için dış sistem ID'si saklanmalıdır.

Örnek eşleme tablosu:

```text
ExternalSystem = CRM
ExternalId     = CUST-89215
LogoFirm       = 040
LogoCode       = 120.01.001
LogoLogicalRef = 1258
```

Bu yapı kod değişikliği olsa bile entegrasyon ilişkisinin korunmasını sağlar.

---

## 17. Kart Açmadan Önce Validasyon

Malzeme için:

- Kod mevcut mu?
- Aynı barkod var mı?
- Birim seti mevcut mu?
- Grup kodu geçerli mi?
- Seri/lot parametresi doğru mu?

Cari için:

- Kod mevcut mu?
- Vergi no/TCKN ile başka kayıt var mı?
- Ülke/şehir bilgileri geçerli mi?
- Ödeme planı mevcut mu?

kontrol edilmelidir.

---

## 18. Kartların SQL'den Okunması

Raporlama ve lookup işlemlerinde SQL son derece kullanışlıdır.

Örneğin malzeme kartı:

```sql
SELECT
    LOGICALREF,
    CODE,
    NAME,
    STGRPCODE,
    SPECODE
FROM LG_040_ITEMS
WHERE ACTIVE = 0;
```

Cari kart:

```sql
SELECT
    LOGICALREF,
    CODE,
    DEFINITION_,
    TAXNR
FROM LG_040_CLCARD
WHERE ACTIVE = 0;
```

Bu sorgular raporlama için uygundur; kart oluşturma/güncelleme için Objects tercih edilmelidir.

---

## 19. Master Data Cache

Yüksek hacimli entegrasyonlarda her fiş satırında SQL sorgusu çalıştırmak performansı düşürebilir.

Örneğin 10.000 fatura satırında her satır için:

```sql
SELECT LOGICALREF FROM LG_040_ITEMS WHERE CODE = @Code
```

çalıştırmak yerine malzeme eşlemeleri bellekte cachelenebilir.

Örnek yapı:

```text
Dictionary<string, int> ItemRefs
Dictionary<string, int> ArpRefs
```

Cache'in yaşam süresi ve invalidation stratejisi dikkatle tasarlanmalıdır.

---

## 20. Aktif/Pasif Kartlar

Entegrasyon, kartın var olmasını tek başına yeterli kabul etmemelidir.

Kart pasif durumda olabilir.

Kontroller:

```text
Kart bulundu mu?
↓
Aktif mi?
↓
İşlem türünde kullanılabilir mi?
```

şeklinde yapılmalıdır.

---

## 21. Kart Silmek Yerine Pasife Alma

Hareket görmüş master data kayıtlarının fiziksel olarak silinmesi genellikle istenmez.

Daha güvenli yaklaşım:

- Pasife almak
- Kullanımı durdurmak
- Yeni kodla devam etmek

Çünkü geçmiş hareketlerin referans bütünlüğü korunmalıdır.

---

## 22. Malzeme ve Cari Kartlarında SQL Update Riski

Örneğin şu işlem basit görünür:

```sql
UPDATE LG_040_ITEMS
SET CODE = 'YENI_KOD'
WHERE LOGICALREF = 100;
```

Ancak kodun başka tablolarda string olarak tutulduğu özel geliştirmeler, entegrasyon eşlemeleri veya raporlar varsa sistemsel tutarsızlık doğabilir.

Benzer şekilde cari kart üzerindeki kritik alanlar Objects üzerinden güncellenmelidir.

---

## 23. Veri Kalitesi Kuralları

Master data için aşağıdaki kalite kuralları önerilir:

### Malzeme

- Kod boş olamaz.
- Kod benzersiz olmalıdır.
- Ana birim zorunludur.
- Barkod benzersiz olmalıdır.
- Grup kodu standart listeden gelmelidir.
- Seri/lot takip kuralı ürün tipine uygun olmalıdır.

### Cari

- Kod boş olamaz.
- Unvan zorunludur.
- Vergi no/TCKN formatı doğrulanmalıdır.
- Ülke/şehir standardize edilmelidir.
- E-posta formatı kontrol edilmelidir.

---

## 24. Entegrasyon Mimarisi

Önerilen yapı:

```text
CRM / PIM / WMS
      │
      ▼
Master Data API
      │
      ├── Validation
      ├── Duplicate Control
      ├── Mapping
      └── Audit Log
      │
      ▼
Logo Objects
      │
      ├── Material IData
      └── AR/AP IData
      │
      ▼
Logo ERP
```

---

## 25. Audit Log

Kart değişikliklerinde kim/ne zaman/ne değiştirdi bilgisi önemlidir.

Örnek:

```text
EntityType: MATERIAL
EntityCode: 150.001
Field: NAME
OldValue: Eski Açıklama
NewValue: Yeni Açıklama
Source: PIM
ChangedAt: 2026-08-07 11:30
```

Bu kayıt özellikle otomatik master data entegrasyonlarında hata araştırmasını kolaylaştırır.

---

## 26. Best Practices

### Yapılması önerilenler

- Kodları kalıcı iş anahtarı olarak tasarla.
- Harici sistem ID eşlemesi tut.
- Duplikasyon kontrolü yap.
- Birim ve barkod yapılarını kartla birlikte yönet.
- Seri/lot parametrelerini kart açılışında doğru belirle.
- Kart oluşturma ve kritik güncellemelerde Objects kullan.
- Yüksek hacimde lookup cache kullan.
- Değişiklikleri audit log ile izle.

### Kaçınılması gerekenler

- Ortamlar arasında LOGICALREF sabitlemek.
- Aynı vergi numarasına kontrolsüz yeni cari açmak.
- Birim çevrimlerini rastgele değiştirmek.
- Barkodu yalnızca string alan olarak görmek.
- Hareket görmüş kartları fiziksel olarak silmek.
- Master data değişikliklerini doğrudan SQL update ile yaygınlaştırmak.

---

## 27. Sonuç

Malzeme ve cari kartları, Logo ERP içindeki operasyonel belgelerin temelidir.

Sağlam bir entegrasyon sırası genellikle şöyledir:

```text
Master Data
   │
   ├── Malzeme
   ├── Cari
   ├── Birim
   └── Barkod
   │
   ▼
Validasyon ve Eşleme
   │
   ▼
Logo Objects
   │
   ▼
Operasyonel Fişler
```

Kart verisi doğru yönetilmeden fiş entegrasyonunun güvenilir olması mümkün değildir.
