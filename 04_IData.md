# 04 — IData

## 1. IData Nedir?

`IData`, Logo Objects içindeki temel veri nesnesidir.

Bir `IData` nesnesinin tipi herhangi bir kart veya fiş türü olarak belirlenir ve o veri tipi üzerinde işlem yapılır.

`IData` ile kart veya fişler üzerinde temel olarak şu işlemler gerçekleştirilebilir:

- Veri okuma,
- Yeni kayıt oluşturma,
- Kayıt güncelleme,
- Kayıt silme.

`IData` nesnesi doğrudan oluşturulmaz. Genellikle `IApplication` nesnesinin `NewDataObject` metodu kullanılarak oluşturulur.

Temel kavram:

```text
IApplication
    |
    v
NewDataObject(DataObjectType)
    |
    v
IData
    |
    +--> New
    +--> Read
    +--> Post
    +--> Delete
    +--> DataFields
    +--> Lines
```

> **Logo ERP üzerinde resmî kart ve fiş hareketleri oluşturulacaksa ilk tercih `IData` olmalıdır.**

---

## 2. Neden IData Kullanılmalıdır?

Logo ERP ürününe dışarıdan doğrudan SQL ile erişip kayıt oluşturmak teknik olarak mümkün görünse de ciddi risk taşır.

Doğrudan tabloya kayıt atmak;

- kayıtlar arasındaki ilişkileri atlamak,
- eksik bağlı kayıt üretmek,
- Logo iş kurallarını devre dışı bırakmak,
- veri bütünlüğünü bozmak

anlamına gelebilir.

Özellikle aşağıdaki işlemlerde `IData` tercih edilmelidir:

```text
INSERT
UPDATE
DELETE
```

Logo Objects içinde SQL ifadeleri çalıştırmak için `IQuery` arayüzü bulunmasına rağmen, kart ve fişlerin standart ERP yaşam döngüsünü yönetmek için doğru araç `IData` nesnesidir.

---

## 3. IData'nın Temel Rolü

`IData`, geliştiricinin Logo ERP veri nesneleriyle nesne tabanlı biçimde çalışmasını sağlar.

Örneğin bir malzeme kartı için:

```text
IData
 |
 +--> CODE
 +--> NAME
 +--> SPECODE
 +--> UNITSET_CODE
 +--> diğer alanlar
```

Bir fatura için:

```text
IData
 |
 +--> NUMBER
 +--> DATE
 +--> ARP_CODE
 +--> SOURCE_WH
 +--> TRANSACTIONS
       |
       +--> Malzeme satırı
       +--> Malzeme satırı
       +--> İndirim satırı
```

Kullanılabilir alanlar seçilen veri nesnesinin tipine göre değişir.

---

## 4. IData Oluşturma

`IData` oluşturmanın temel mantığı:

```csharp
IApplication app = GetLogoApplication();

IData data = app.NewDataObject(/* DataObjectType */);
```

Buradaki veri nesnesi tipi, hangi Logo kartı veya fişi üzerinde işlem yapılacağını belirler.

Örneğin farklı nesne tipleri şu iş sınıflarını temsil edebilir:

- Malzeme kartı,
- Cari hesap kartı,
- Satınalma siparişi,
- Satış siparişi,
- Satınalma irsaliyesi,
- Satış irsaliyesi,
- Satınalma faturası,
- Satış faturası,
- Stok fişi.

Gerçek enum isimleri kullanılan Logo Objects sürümüne göre doğrulanmalıdır.

---

## 5. IData Yaşam Döngüsü

Bir `IData` nesnesinin temel yaşam döngüsü aşağıdaki gibi düşünülebilir:

```text
IData oluştur
    |
    +--> Yeni kayıt mı?
    |       |
    |       v
    |      New()
    |
    +--> Mevcut kayıt mı?
            |
            v
          Read(...)
            |
            v
      Alanları değiştir
            |
            v
          Post()
```

Silme senaryosu:

```text
IData oluştur
    |
    v
Kaydı bul / oku
    |
    v
Delete(...)
```

Burada kullanılan metodların tam imzaları Logo Objects sürümüne göre kontrol edilmelidir.

---

## 6. Yeni Kayıt Oluşturma

Yeni bir kart veya fiş oluştururken genel işlem sırası şöyledir:

```text
IData oluştur
     |
     v
New()
     |
     v
Üst alanları doldur
     |
     v
Varsa Lines oluştur
     |
     v
Post()
     |
     +--> Başarılı
     |
     +--> Hatalı -> ErrorDesc / log
```

Kavramsal örnek:

```csharp
IData data = app.NewDataObject(/* nesne tipi */);

data.New();

// Üst alanları doldur
// Satırları ekle

bool result = data.Post();

if (!result)
{
    // Logo hata bilgisini oku ve logla
}
```

Bu örnekteki API detayları kullanılan sürüme göre uyarlanmalıdır.

---

## 7. DataFields Kullanımı

`IData` içindeki alanlara `DataFields` koleksiyonu üzerinden erişilir.

Kavramsal yaklaşım:

```text
IData
  |
  v
DataFields
  |
  +--> FieldByName("CODE")
  +--> FieldByName("NAME")
  +--> FieldByName("DATE")
```

Örnek mantık:

```csharp
data.DataFields.FieldByName("CODE").Value = "150.001";
data.DataFields.FieldByName("NAME").Value = "Örnek Malzeme";
```

Alan isimleri doğrudan varsayılmamalıdır. Veri nesnesinin Logo Objects şeması kontrol edilmelidir.

---

## 8. SQL Alanı ile IData Alanı Aynı Şey Değildir

Logo geliştiricilerinin yaptığı yaygın hatalardan biri SQL tablosundaki alan adının `IData` içinde de birebir bulunacağını varsaymaktır.

Örneğin SQL tarafında şu alan görülebilir:

```text
LG_040_ITEMS.CODE
```

Logo Objects tarafında ilgili alanın kullanılabilirliği ve adı veri nesnesinin export şemasına bağlıdır.

Bu nedenle şu varsayım yanlıştır:

```text
SQL'de alan var -> IData'da kesin aynı isimle vardır
```

Doğru yaklaşım:

```text
IData veri şemasını incele
       |
       v
Alan adını doğrula
       |
       v
Değeri ata
```

---

## 9. FieldByName Yaklaşımı

Alanlara indeks yerine isimle erişmek çoğu durumda kod okunabilirliğini artırır.

Örnek:

```csharp
var fields = data.DataFields;

fields.FieldByName("CODE").Value = item.Code;
fields.FieldByName("NAME").Value = item.Name;
```

Avantajları:

- Kod daha okunabilir olur,
- Alanın ne amaçla kullanıldığı bellidir,
- Sıra değişimlerinden daha az etkilenir.

Ancak alan adı yanlışsa runtime sırasında hata oluşabileceğinden geliştirme ve test ortamında şema doğrulanmalıdır.

---

## 10. Lines Nedir?

Fişlerin ve bazı kartların alt kayıtları `Lines` koleksiyonları üzerinden yönetilir.

Örneğin bir satış faturasında:

```text
Fatura IData
    |
    +--> Header DataFields
    |
    +--> TRANSACTIONS / Lines
            |
            +--> Satır 1
            +--> Satır 2
            +--> Satır 3
```

Satırlar genellikle fiş veri nesnesinin ilgili alanı üzerinden alınır.

---

## 11. Satır Ekleme Mantığı

Yeni fiş oluştururken her hareket satırı için yeni bir line oluşturulur.

Kavramsal örnek:

```csharp
var lines = data.DataFields.FieldByName("TRANSACTIONS").Lines;

lines.AppendLine();

lines[lines.Count - 1].FieldByName("TYPE").Value = 0;
lines[lines.Count - 1].FieldByName("MASTER_CODE").Value = "150.001";
lines[lines.Count - 1].FieldByName("QUANTITY").Value = 10;
```

Gerçek koleksiyon ve alan isimleri ilgili fiş türüne göre doğrulanmalıdır.

Buradaki esas fikir şudur:

> **Fiş üst bilgisi ve fiş satırları tek bir `IData` veri ağacının parçalarıdır.**

---

## 12. Header ve Line İlişkisi

Bir fişi aşağıdaki gibi düşünmek faydalıdır:

```text
IData
|
+-- DataFields
|   |
|   +-- NUMBER
|   +-- DATE
|   +-- ARP_CODE
|   +-- SOURCE_WH
|   +-- DOC_TRACK_NR
|
+-- TRANSACTIONS
    |
    +-- Line 0
    |    +-- TYPE
    |    +-- MASTER_CODE
    |    +-- QUANTITY
    |    +-- PRICE
    |
    +-- Line 1
         +-- TYPE
         +-- MASTER_CODE
         +-- QUANTITY
         +-- PRICE
```

Fiş entegrasyonlarında yalnızca header alanlarını doğru doldurmak yeterli değildir.

---

## 13. Post İşlemi

`Post`, hazırlanan `IData` nesnesinin Logo ERP'ye kaydedilmesini sağlayan kritik adımdır.

Temel akış:

```text
New / Read
    |
Alanları doldur
    |
Satırları düzenle
    |
    v
Post()
    |
    +--> true  -> kayıt başarılı
    |
    +--> false -> hata bilgilerini incele
```

`Post()` başarısız olduğunda uygulama bunu sessizce geçmemelidir.

Kayıt başarısızlığının nedeni loglanmalıdır.

---

## 14. Post Sonrası Kontroller

`Post()` başarılı olduktan sonra entegrasyon aşağıdaki bilgileri mümkün olduğunca kayıt altına almalıdır:

- Logo `LOGICALREF`,
- Fiş/kart numarası,
- Kaynak sistem kayıt numarası,
- Firma numarası,
- Dönem numarası,
- İşlem zamanı.

Örnek entegrasyon sonucu:

```json
{
  "success": true,
  "sourceId": "MES-18427",
  "logoLogicalRef": 9539,
  "documentNo": "FAT202600001245"
}
```

Bu bilgi daha sonraki güncelleme, iptal ve mutabakat işlemlerinde kullanılır.

---

## 15. Hata Bilgisini Okuma

Logo Objects entegrasyonlarında en önemli konulardan biri `Post()` başarısızlığındaki hata bilgisidir.

Kötü hata yönetimi:

```text
Kayıt başarısız.
```

İyi hata yönetimi:

```text
Firma: 202
Dönem: 01
Nesne: Satınalma Faturası
Kaynak ID: ERPAPI-8121
Belge No: A12345
Logo Hatası: <Logo tarafından dönen açıklama>
```

Bu bilgi üretim ortamındaki destek süresini ciddi biçimde azaltır.

---

## 16. Kayıt Okuma

Mevcut bir kart veya fişi güncellemek için önce kayıt okunmalıdır.

Okuma işlemi çoğu senaryoda kayıt referansı üzerinden yapılır.

Kavramsal yapı:

```text
IData oluştur
   |
   v
Read(LOGICALREF)
   |
   +--> Bulundu
   |      |
   |      v
   |   Alanları kullan
   |
   +--> Bulunamadı
          |
          v
       Hata / yeni kayıt kararı
```

Burada `LOGICALREF`, Logo ERP veri modelindeki teknik referanstır.

---

## 17. Güncelleme

Güncellemede genel prensip:

```text
IData oluştur
     |
     v
Kaydı oku
     |
     v
Mevcut alanları değiştir
     |
     v
Post()
```

Önemli nokta:

> Güncelleme yapmak için aynı kaydı SQL tarafında `UPDATE` etmek yerine, mümkün olduğunda `IData` üzerinden okuyup tekrar `Post()` etmek tercih edilmelidir.

Bu sayede Logo iş kuralları devrede kalır.

---

## 18. Silme

Kayıt silme de yalnızca ilgili ana tablo satırını silmek anlamına gelmez.

Örneğin bir fatura çok sayıda bağlı hareketle ilişkili olabilir.

Doğrudan:

```sql
DELETE FROM LG_XXX_YY_INVOICE
WHERE LOGICALREF = ...
```

şeklinde işlem yapmak ciddi veri bütünlüğü sorunları oluşturabilir.

Silme işlemi mümkün olduğunda Logo Objects'in ilgili veri nesnesi üzerinden gerçekleştirilmelidir.

---

## 19. Fiş Numarası ve Otomatik Numaralandırma

Logo fişlerinde numaralandırma konusu dikkat gerektirir.

Entegrasyon iki farklı yaklaşım kullanabilir:

### Harici numara kullanımı

Dış sistem kendi belge numarasını Logo'ya gönderir.

### Logo numaralandırması

Numaranın Logo tarafındaki numaralandırma kuralıyla oluşması hedeflenir.

Hangisinin kullanılacağı iş sürecine göre belirlenmelidir.

Her iki durumda da duplicate belge kontrolü yapılmalıdır.

---

## 20. Zorunlu Alanlar

Her `IData` nesnesinin zorunlu alanları aynı değildir.

Örneğin bir fişte aşağıdaki alanlardan bazıları zorunlu olabilir:

- Tarih,
- Belge türü,
- Cari hesap,
- Ambar,
- Malzeme,
- Miktar,
- Birim,
- Fiyat.

Logo ekranında kullanıcı tarafından otomatik doldurulan bir alanın Objects entegrasyonunda da otomatik dolacağı varsayılmamalıdır.

Entegrasyon her veri nesnesini gerçek test kayıtlarıyla doğrulamalıdır.

---

## 21. Malzeme Koduyla mı LOGICALREF ile mi?

Entegrasyonların kaynak sistemleri genellikle Logo `LOGICALREF` değerini bilmez.

Örneğin MES şu bilgiyi gönderir:

```text
Malzeme Kodu = T30.100.010
```

Logo tarafında ise gerçek teknik ilişki şu şekilde olabilir:

```text
LG_202_ITEMS.LOGICALREF = 43338
```

`IData` şemasına göre bazı alanlarda kod ile, bazı alanlarda referansla çalışılabilir.

Bu nedenle entegrasyon katmanında merkezi bir eşleştirme yaklaşımı faydalıdır.

```text
External Code
     |
     v
Mapping / Lookup
     |
     v
Logo Code / LOGICALREF
```

---

## 22. Birim Bilgileri

Malzeme hareketlerinde yalnızca miktarı göndermek yeterli değildir.

Birim seçimi özellikle birden fazla birim kullanılan malzemelerde önemlidir.

Örneğin:

```text
Ana birim : AD
İkinci birim: KOLİ
1 KOLİ = 24 AD
```

Yanlış birim seçilirse:

- Stok miktarı yanlış olabilir,
- Birim fiyat hatalı yorumlanabilir,
- Maliyetler bozulabilir,
- Raporlarda anormal hareketler oluşabilir.

Bu nedenle `IData` satırlarında kullanılan birim alanları ilgili malzemenin Logo birim setiyle uyumlu olmalıdır.

---

## 23. Fiyat ve Döviz Alanları

Fatura ve sipariş satırlarında fiyat tek başına yeterli olmayabilir.

İşleme göre şu kavramlar gündeme gelebilir:

- Birim fiyat,
- İşlem dövizi,
- Döviz kuru,
- Raporlama dövizi,
- KDV,
- İndirim,
- Birim dönüşümü.

Entegrasyon dış sistemdeki fiyatın hangi para biriminde olduğunu açıkça bilmelidir.

Örneğin:

```text
PRICE = 100
```

tek başına yeterli bir bilgi değildir.

Şu sorular da cevaplanmalıdır:

```text
100 TL mi?
100 USD mi?
KDV dahil mi?
Ana birim fiyatı mı?
İkinci birim fiyatı mı?
```

---

## 24. KDV ve Vergi Alanları

Satış ve satınalma fişlerinde KDV bilgisi kritik iş kuralıdır.

Özellikle KDV oranı `0` olan satırlarda muafiyet veya istisna bilgileri gerekebilir.

Örneğin iş sürecinde aşağıdaki bilgiler kullanılabilir:

```text
VAT = 0
VATEXCEPTCODE
VATEXCEPTREASON
```

KDV sıfır olduğu için alanları boş bırakmak her zaman doğru değildir.

Entegrasyon, Logo ekranında kullanıcıdan beklenen vergi bilgisini `IData` tarafında da doğru göndermelidir.

---

## 25. Seri / Lot Kullanılan Malzemeler

Seri/lot takipli malzemelerde normal stok satırı tek başına yeterli olmayabilir.

İşlem ilişkisi kavramsal olarak şöyledir:

```text
Fiş Satırı
    |
    v
Malzeme
    |
    v
Seri / Lot dağılımı
    |
    v
Stok hareketi
```

Bu nedenle seri/lot kullanılan firmalarda `IData` fiş entegrasyonu tasarlarken alt dağıtım nesneleri ayrıca incelenmelidir.

Doğrudan `STLINE` oluşturmak özellikle bu yapılarda yüksek risk taşır.

---

## 26. Üretim Senaryolarında IData

Üretim süreçlerinde `IData` önemli olsa da detaylı üretimde her işlem standart stok fişi mantığıyla çözülmemelidir.

Detaylı üretim süreçlerinde:

- Üretim emirleri,
- Operasyonlar,
- İş emirleri,
- Gerçekleşen üretim,
- Sarf,
- Fire,
- Seri/lot,
- Kalite,
- Maliyet

gibi ilişkiler bulunduğu için `ProductionApplication` gibi üretime özel Logo Objects bileşenleri gündeme gelebilir.

`IData` ve `ProductionApplication` birbirinin alternatifi değil; işlem türüne göre kullanılan farklı entegrasyon araçlarıdır.

---

## 27. IData ve IQuery Birlikte Kullanımı

Gerçek projelerde `IData` ve `IQuery` birlikte kullanılır.

Örnek senaryo:

```text
IQuery
  |
  +--> Malzeme var mı kontrol et
  +--> LOGICALREF bul
  +--> Son alış fiyatını oku
  |
  v
İş kuralı
  |
  v
IData
  |
  +--> Faturayı oluştur
```

Bu oldukça güçlü bir desendir.

`IQuery` okuma ve yardımcı veri toplama için, `IData` ise resmî ERP kaydını oluşturmak için kullanılır.

---

## 28. Doğrudan SQL ile Güncelleme Ne Zaman Düşünülebilir?

Bazı istisnai bakım veya veri düzeltme işlemlerinde doğrudan SQL gerekebilir.

Örneğin kontrollü bir bakım prosedürü:

```text
Test modu
    |
    v
Etkilenecek kayıtları göster
    |
    v
Kullanıcı kontrolü
    |
    v
Transaction
    |
    v
Update
    |
    v
Bağlı kayıt kontrolleri
    |
    v
Commit / Rollback
```

Ancak bu yaklaşım normal entegrasyon yöntemi haline getirilmemelidir.

Logo veri yapısının tamamı bilinmeden yapılan doğrudan SQL güncellemeleri büyük risk taşır.

---

## 29. IData İçin Servis Katmanı

Kurumsal uygulamalarda doğrudan her ekrandan `IData` kullanılmaması önerilir.

Örneğin:

```text
UI / API
   |
   v
InvoiceService
   |
   v
LogoInvoiceRepository
   |
   v
IData
```

Bu yapı sayesinde Logo Objects detayları uygulamanın geri kalanından izole edilir.

Örnek servis metodu:

```csharp
CreateSalesInvoice(request)
```

UI tarafı şunları bilmek zorunda değildir:

```text
IData nasıl oluşturulur?
Hangi field adı kullanılır?
Lines nasıl eklenir?
Post nasıl kontrol edilir?
```

---

## 30. DTO Kullanımı

Logo Objects nesnelerini doğrudan API modeli olarak kullanmak yerine uygulama DTO'ları oluşturmak daha sağlıklıdır.

Örnek:

```csharp
public class SalesInvoiceRequest
{
    public string DocumentNo { get; set; }
    public DateTime Date { get; set; }
    public string ClientCode { get; set; }
    public List<SalesInvoiceLineRequest> Lines { get; set; }
}
```

Ardından mapper:

```text
SalesInvoiceRequest
       |
       v
Logo Mapper
       |
       v
IData
```

Bu tasarım Logo Objects bağımlılığını uygulamanın belirli bir katmanında tutar.

---

## 31. Veri Doğrulama

`Post()` çağrısından önce dış sistemden gelen veri doğrulanmalıdır.

Örnek kontroller:

- Firma mevcut mu?
- Dönem açık mı?
- Cari kodu var mı?
- Malzeme kodu var mı?
- Malzeme aktif mi?
- Birim geçerli mi?
- Ambar mevcut mu?
- Miktar sıfırdan büyük mü?
- Fiyat beklenen para biriminde mi?
- Aynı kaynak belge daha önce işlendi mi?

Böylece Logo Objects'e gereksiz başarısız kayıt denemeleri gönderilmez.

---

## 32. Idempotent IData İşlemi

Yeni kayıt oluşturmadan önce kaynak sistem kaydının daha önce işlenip işlenmediği kontrol edilmelidir.

Örnek:

```text
SourceSystem = MES
SourceId     = 874421
```

Akış:

```text
İstek geldi
    |
    v
IntegrationMap kontrol et
    |
    +--> Var
    |     |
    |     v
    |  Mevcut Logo kaydını döndür
    |
    +--> Yok
          |
          v
       IData ile oluştur
          |
          v
       Mapping kaydet
```

Bu yaklaşım duplicate fiş oluşmasını önler.

---

## 33. Batch İşlemler

Binlerce kart veya fiş aktarılacaksa her kaydın sonucu ayrı izlenmelidir.

Örnek batch sonucu:

```text
Toplam       : 10.000
Başarılı     : 9.842
Hatalı       : 158
Tekrar       : 34
Duplicate    : 21
```

Hatalı bir kaydın tüm batch'i kontrolsüz biçimde durdurması veya hataların sessizce geçilmesi doğru değildir.

İş sürecine göre transaction sınırı belirlenmelidir.

---

## 34. Performans

`IData` ile çok yüksek hacimli aktarım yapılırken performans yalnızca Logo Objects'in hızıyla ilgili değildir.

Aşağıdakiler de etkilidir:

- Her satır için ayrı SQL lookup yapılması,
- Gereksiz login/logout,
- Aynı master datanın tekrar tekrar sorgulanması,
- Ağ gecikmesi,
- SQL Server disk performansı,
- Logo trigger ve custom geliştirmeleri,
- Seri/lot ve maliyet hesapları.

Örneğin 10.000 satırlık aktarımda her satır için ayrı malzeme sorgusu yapmak yerine kontrollü cache kullanılabilir.

---

## 35. Lookup Cache

Örnek cache:

```text
Dictionary<string, int> ItemRefs
Dictionary<string, int> ClientRefs
Dictionary<string, int> UnitRefs
```

Akış:

```text
Malzeme kodu geldi
    |
    +--> Cache var -> kullan
    |
    +--> Cache yok -> sorgula -> cache'e ekle
```

Ancak cache sonsuza kadar geçerli kabul edilmemelidir. Master data değişikliklerinin nasıl yenileneceği tasarlanmalıdır.

---

## 36. Test Modu

Özellikle kritik entegrasyonlarda gerçek kayıttan önce validation/test modu faydalıdır.

Örnek request:

```json
{
  "testMode": true,
  "firmNo": 202,
  "documentNo": "TEST001"
}
```

Test modu:

- Cariyi kontrol eder,
- Malzemeleri kontrol eder,
- Birimleri kontrol eder,
- Ambarı kontrol eder,
- Zorunlu alanları kontrol eder,
- Ancak ERP kaydı oluşturmaz.

Bu yaklaşım özellikle toplu geçiş projelerinde değerlidir.

---

## 37. Gerçek Proje Kontrol Listesi

Bir `IData` entegrasyonu canlıya alınmadan önce aşağıdaki kontroller yapılmalıdır:

### Bağlantı

- [ ] Logo Objects login başarılı.
- [ ] Firma doğru.
- [ ] Dönem doğru.

### Master data

- [ ] Cari kodları doğrulandı.
- [ ] Malzeme kodları doğrulandı.
- [ ] Birimler doğrulandı.
- [ ] Ambarlar doğrulandı.

### Fiş

- [ ] Tarih doğru.
- [ ] Fiş türü doğru.
- [ ] Satırlar doğru.
- [ ] Birim fiyat doğru.
- [ ] KDV doğru.
- [ ] Döviz doğru.
- [ ] Seri/lot varsa doğru.

### Sonuç

- [ ] `Post()` sonucu kontrol ediliyor.
- [ ] Logo hata açıklaması loglanıyor.
- [ ] `LOGICALREF` saklanıyor.
- [ ] Duplicate kontrolü var.
- [ ] Retry mekanizması var.

---

## 38. Best Practices

1. Kart ve fiş CRUD işlemlerinde öncelikle `IData` kullan.
2. SQL tablo yapısını `IData` şeması sanma.
3. `Post()` sonucunu mutlaka kontrol et.
4. Logo hata bilgisini sakla.
5. Yeni kayıttan önce duplicate kontrolü yap.
6. Kaynak sistem ID'sini Logo `LOGICALREF` ile eşleştir.
7. Her fişte firma ve dönem bilgisini logla.
8. Birim, döviz ve vergi alanlarını açıkça yönet.
9. Seri/lot kullanılan malzemelerde alt dağıtımları göz ardı etme.
10. Güncelleme ve silmede doğrudan SQL'i varsayılan çözüm yapma.
11. `IData` kullanımını servis/repository katmanında merkezileştir.
12. Toplu işlemlerde lookup cache ve batch logging kullan.

---

## 39. Bölüm Özeti

`IData`, Logo Objects'in temel ERP veri işlem nesnesidir.

```text
IApplication
    |
    v
IData
    |
    +--> New
    +--> Read
    +--> Update
    +--> Delete
    +--> DataFields
    +--> Lines
    +--> Post
```

`IData` kullanmanın asıl amacı geliştiricinin SQL yazmasını azaltmak değil, Logo ERP'nin iş kurallarını ve veri bütünlüğünü koruyarak kayıt oluşturmaktır.

Temel prensip:

> **SELECT için SQL çok güçlüdür; resmî Logo kart ve fiş hareketlerinin oluşturulması ve değiştirilmesi için `IData` birincil araçtır.**

Sonraki bölümde `IQuery` nesnesi ayrıntılı olarak ele alınacaktır.
