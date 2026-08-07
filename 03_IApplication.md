# 03 — IApplication

## 1. IApplication Nedir?

`IApplication`, Logo Objects ile geliştirilen uygulamaların merkezî uygulama nesnesidir.

Logo ERP ile programatik olarak çalışırken kullanılacak `IData`, `IQuery` ve diğer Logo Objects nesneleri doğrudan bağımsız biçimde oluşturulmaz. Uygulama önce Logo Objects ortamını temsil eden `IApplication` nesnesini oluşturur, gerekli oturum ve firma/dönem bağlamını kurar, ardından ihtiyaç duyduğu veri veya sorgu nesnelerini bu uygulama nesnesi üzerinden üretir.

Bu nedenle `IApplication` için en doğru zihinsel model şudur:

> **Logo Objects dünyasına açılan ana oturum ve nesne fabrikasıdır.**

Kavramsal yapı:

```text
Dış Uygulama
    |
    v
IApplication
    |
    +--> Login / User Context
    +--> Company Context
    +--> Period Context
    |
    +--> NewDataObject(...)
    |         |
    |         v
    |       IData
    |
    +--> NewQuery(...)
              |
              v
            IQuery
```

---

## 2. IApplication'ın Sorumlulukları

`IApplication` nesnesi entegrasyonun yalnızca başlangıç noktası değildir. Uygulama oturumu boyunca Logo tarafındaki çalışma bağlamını temsil eder.

Temel sorumlulukları şu başlıklarda ele alınabilir:

- Logo Objects ortamına erişim,
- Kullanıcı oturumu yönetimi,
- Firma seçimi,
- Dönem seçimi,
- Veri nesnesi üretimi,
- Sorgu nesnesi üretimi,
- Oturumun kontrollü biçimde kapatılması.

Kullanılan Logo Objects sürümüne göre mevcut metod ve özelliklerin isimleri veya ayrıntıları değişebilir. Bu nedenle sürüme özel API referansı her zaman ayrıca kontrol edilmelidir.

---

## 3. Neden Önce IApplication Oluşturulur?

Logo ERP çok firmalı ve dönemli bir yapıdır.

Bir entegrasyonun yaptığı işlemin yalnızca "hangi tabloya" yazıldığı değil, şu bağlamlarda yürütüldüğü de önemlidir:

```text
Kullanıcı
   |
Firma
   |
Dönem
   |
Veri Nesnesi
   |
İşlem
```

Örneğin aynı malzeme kodu farklı firmalarda farklı kayıtları temsil edebilir. Benzer şekilde dönem bazlı fiş tabloları birbirinden ayrıdır.

Dolayısıyla `IApplication` kullanılmadan önce tasarımda şu soruların cevabı net olmalıdır:

1. Hangi Logo kullanıcısı ile bağlanılacak?
2. Hangi firma üzerinde işlem yapılacak?
3. Hangi dönemde işlem yapılacak?
4. Uygulama tek firma mı, çok firma mı destekleyecek?
5. Oturum sürekli açık mı tutulacak, işlem bazlı mı açılacak?

---

## 4. Tipik Başlangıç Akışı

Bir Logo Objects uygulamasında başlangıç süreci kavramsal olarak aşağıdaki gibidir:

```text
Uygulama başlat
      |
      v
IApplication oluştur
      |
      v
Kullanıcı ile giriş yap
      |
      v
Firma bağlamını belirle
      |
      v
Dönem bağlamını belirle
      |
      v
Logo Objects işlemlerini gerçekleştir
      |
      v
Logout / Dispose / uygulamayı kapat
```

Buradaki her adım hata kontrolüne tabi tutulmalıdır.

---

## 5. Giriş Bilgilerinin Yönetimi

Logo Objects kullanan üretim uygulamalarında kullanıcı adı ve parola doğrudan kaynak kod içine yazılmamalıdır.

Yanlış örnek:

```csharp
string userName = "LOGO";
string password = "123456";
```

Bunun yerine aşağıdaki yöntemlerden biri tercih edilmelidir:

- Şifrelenmiş uygulama konfigürasyonu,
- Windows Credential Manager,
- Secret store,
- Ortam değişkenleri,
- Kurumsal secret management sistemi.

Özellikle servis uygulamalarında Logo kullanıcı hesabı teknik servis hesabı olarak yönetilmeli ve yalnızca gerekli yetkilere sahip olmalıdır.

---

## 6. Teknik Kullanıcı Tasarımı

ERP entegrasyonlarında kişisel kullanıcı hesabı yerine entegrasyona özel teknik kullanıcı kullanılması genellikle daha doğru yaklaşımdır.

Örnek:

```text
LOGO_INTEGRATION
LOGO_MES
LOGO_WMS
LOGO_API
```

Avantajları:

- İşlemlerin hangi entegrasyondan geldiği anlaşılır,
- Kullanıcı değişimlerinden etkilenmez,
- Yetki kapsamı sınırlanabilir,
- Audit kolaylaşır,
- Şifre rotasyonu merkezi yapılabilir.

Teknik kullanıcıya gereksiz yönetici yetkileri verilmemelidir.

---

## 7. Firma Bağlamı

Logo ERP'de firma numarası kritik bir çalışma bağlamıdır.

SQL tarafında örnek:

```text
LG_040_ITEMS
LG_102_ITEMS
LG_803_ITEMS
```

Buradaki `040`, `102`, `803` gibi değerler farklı firma numaralarını temsil eder.

Logo Objects kullanan bir uygulama da hangi firma üzerinde işlem yaptığını açıkça bilmelidir.

Çok firmalı sistemlerde firma numarası şu şekilde sabit kodlanmamalıdır:

```csharp
int firmNo = 40;
```

Bunun yerine konfigürasyon veya iş emri üzerinden belirlenmesi daha sağlıklıdır.

Örnek konfigürasyon modeli:

```json
{
  "Logo": {
    "FirmNo": 40,
    "PeriodNo": 1
  }
}
```

---

## 8. Dönem Bağlamı

Logo ERP'deki birçok hareket tablosu dönem bazlıdır.

Örneğin:

```text
LG_040_01_INVOICE
LG_040_01_STLINE
LG_040_01_STFICHE
```

Bu nedenle entegrasyon sırasında yalnızca firma numarasını belirlemek yeterli değildir.

Dönem değişimi özellikle yıl geçişlerinde kritik hale gelir.

Riskli senaryo:

```text
31.12.2026 -> Dönem 01
01.01.2027 -> Yeni mali dönem
```

Eğer entegrasyon dönem bilgisini sabit tutuyorsa yeni yıl başladığında eski döneme kayıt üretme veya kayıt oluşturamama riski doğabilir.

Bu nedenle dönem yönetimi bilinçli tasarlanmalıdır.

---

## 9. NewDataObject

`IApplication` nesnesinin en önemli kullanım alanlarından biri `IData` oluşturmaktır.

Temel mantık:

```text
IApplication
      |
      v
NewDataObject(ObjectType)
      |
      v
IData
```

Burada kullanılan nesne tipi, yapılacak ERP işlemini belirler.

Örneğin uygulama farklı veri nesneleri oluşturarak:

- Malzeme kartı,
- Cari hesap kartı,
- Sipariş,
- İrsaliye,
- Fatura,
- Stok fişi

gibi kayıtlar üzerinde işlem yapabilir.

Bu yaklaşımın önemli avantajı, geliştiricinin doğrudan Logo tablo ilişkilerini elle üretmek zorunda kalmamasıdır.

---

## 10. NewDataObject Kullanım Deseni

Kavramsal C# örneği:

```csharp
// Gerçek enum ve API isimleri kullanılan Logo Objects sürümüne göre kontrol edilmelidir.
IApplication app = GetLogoApplication();

IData data = app.NewDataObject(/* ilgili veri nesnesi tipi */);

// data üzerinden yeni kayıt / okuma / güncelleme işlemleri
```

Buradaki önemli prensip şudur:

> `IData` nesnesinin yaşam döngüsü `IApplication` oturum bağlamına bağlıdır.

Oturum kapanmışken aynı veri nesnesini kullanmaya çalışmak doğru tasarım değildir.

---

## 11. IQuery Oluşturma

Logo Objects üzerinden sorgu çalıştırmak gerektiğinde `IApplication` üzerinden sorgu nesnesi oluşturulur.

Kavramsal akış:

```text
IApplication
    |
    v
Query oluştur
    |
    v
SQL metnini belirle
    |
    v
Execute
    |
    v
Sonuçları oku
```

`IQuery`, özellikle özel SELECT sorguları ve Logo Objects'in standart nesne modeliyle kolay erişilemeyen yardımcı bilgilerin okunması için yararlıdır.

---

## 12. Uygulama Yaşam Döngüsü

Bir Logo Objects uygulamasında `IApplication` nesnesinin yaşam döngüsü bilinçli tasarlanmalıdır.

İki yaygın model vardır.

### Model A — İşlem başına oturum

```text
İstek geldi
   |
Login
   |
İşlemi yap
   |
Logout
```

Avantaj:

- İzolasyonu yüksektir.

Dezavantaj:

- Çok yoğun sistemlerde oturum açma/kapatma maliyeti artabilir.

### Model B — Uzun yaşayan servis oturumu

```text
Servis başlar
   |
Login
   |
İstek 1
İstek 2
İstek 3
...
   |
Servis kapanır
   |
Logout
```

Avantaj:

- Oturum kurma maliyeti azalabilir.

Dezavantaj:

- Bağlantı kopması,
- Oturumun geçersiz hale gelmesi,
- COM nesne yaşam döngüsü,
- Thread safety

gibi konular daha dikkatli yönetilmelidir.

---

## 13. COM Nesne Yaşam Döngüsü

Logo Objects kullanılan projelerde sürüme ve entegrasyon teknolojisine bağlı olarak COM tabanlı nesnelerle çalışılabilir.

Bu durumda özellikle uzun yaşayan Windows Service veya masaüstü uygulamalarında şu riskler ortaya çıkabilir:

- Nesnelerin serbest bırakılmaması,
- Bellek kullanımının zamanla büyümesi,
- Askıda kalan Logo süreçleri,
- Yeniden bağlantı problemleri.

Bu nedenle oluşturulan Logo Objects nesnelerinin yaşam döngüsü kontrol altında tutulmalıdır.

.NET tarafında kullanılan teknolojiye göre COM nesnelerinin release edilmesi gerekebilir.

Ancak her nesne için körü körüne zorla release çağrısı yapmak yerine kullanılan Logo Objects sürümü ve uygulamanın COM interop davranışı test edilmelidir.

---

## 14. Thread Safety

`IApplication` nesnesinin çok iş parçacıklı kullanımında dikkatli olunmalıdır.

Örneğin bir Web API aynı anda 20 istek aldığında tek bir `IApplication` nesnesinin tüm thread'ler arasında paylaşılması güvenli olduğu varsayılmamalıdır.

Riskli mimari:

```text
             +--> Request 1
             |
Tek IApplication --> Request 2
             |
             +--> Request 3
```

Daha kontrollü yaklaşım:

```text
Request Queue
     |
     v
Logo Worker / Controlled Session
     |
     v
IApplication
```

veya ölçek ihtiyacına göre izole worker oturumları kullanılabilir.

Logo Objects'in ilgili sürümündeki thread davranışı resmi dokümantasyon ve gerçek yük testi ile doğrulanmalıdır.

---

## 15. Windows Service İçinde Kullanım

Logo Objects bir Windows Service içinde kullanılacaksa yalnızca kodun çalışması yeterli değildir.

Aşağıdaki konular test edilmelidir:

- Servisin hangi Windows hesabıyla çalıştığı,
- Logo kurulum bileşenlerine erişim,
- Registry erişimleri,
- COM registration,
- 32-bit / 64-bit uyumluluğu,
- Logo lisans erişimi,
- Kullanıcı profil bağımlılıkları,
- Ağ erişimleri,
- SQL Server erişimi.

Masaüstü uygulamada çalışan kodun Windows Service içinde otomatik olarak çalışacağı varsayılmamalıdır.

---

## 16. 32-bit / 64-bit Uyumluluğu

Logo Objects entegrasyonlarında kullanılan Logo sürümü ve COM bileşenlerinin mimarisi dikkate alınmalıdır.

Örneğin Logo Objects bileşeni 32-bit olarak çalışıyorsa .NET projesinin `Any CPU` veya 64-bit derlenmesi sorun oluşturabilir.

Kontrol edilmesi gerekenler:

```text
Logo Objects mimarisi
       |
       +--> x86 ?
       +--> x64 ?

Uygulama build target
       |
       +--> x86
       +--> x64
       +--> Any CPU
```

Üretime çıkmadan önce build target mutlaka gerçek Logo ortamında test edilmelidir.

---

## 17. Login Hatalarında Yaklaşım

Kullanıcı adı ve parola doğru olduğu halde login başarısız olabilir.

Böyle bir durumda yalnızca kullanıcı bilgisini kontrol etmek yeterli değildir.

Kontrol listesi:

1. Logo Objects bileşenleri doğru kurulmuş mu?
2. Uygulama doğru bitness ile çalışıyor mu?
3. Logo lisans sunucusuna erişiliyor mu?
4. Firma/dönem aktif mi?
5. Teknik kullanıcının ilgili firma yetkisi var mı?
6. Uygulamanın çalıştığı Windows hesabı gerekli erişime sahip mi?
7. Logo versiyonu ile referans verilen Objects kütüphanesi uyumlu mu?
8. Sunucu ve istemci bileşenlerinin versiyonları uyumlu mu?

Bu kontroller özellikle sunucu taşıma ve versiyon güncelleme sonrasında önemlidir.

---

## 18. Hata Yönetimi Deseni

`IApplication` ile gerçekleştirilen tüm oturum işlemleri kontrollü hata yönetimine sahip olmalıdır.

Kavramsal örnek:

```csharp
try
{
    // IApplication oluştur
    // Login yap
    // Firma ve dönem bağlamını kur
    // İşlemi gerçekleştir
}
catch (Exception ex)
{
    // Teknik hata logu
}
finally
{
    // Oluşturulan nesneleri temizle
    // Gerekliyse logout
}
```

Amaç yalnızca exception yakalamak değildir.

Log içinde şu bilgiler bulunmalıdır:

```text
Timestamp
ApplicationName
MachineName
FirmNo
PeriodNo
LogoUser
Operation
ExceptionType
ExceptionMessage
StackTrace
```

---

## 19. Connection Manager Tasarımı

Orta ve büyük entegrasyonlarda `IApplication` yönetimini uygulamanın her yerine dağıtmak yerine merkezi bir sınıf üzerinden yönetmek faydalıdır.

Örnek tasarım:

```text
LogoConnectionManager
    |
    +--> Connect()
    +--> Disconnect()
    +--> IsConnected
    +--> CreateDataObject()
    +--> CreateQuery()
```

Avantajları:

- Login kodu tek yerde bulunur,
- Credential yönetimi merkezi olur,
- Retry politikası eklenebilir,
- Firma/dönem kontrolü merkezi olur,
- Logging standartlaşır.

---

## 20. Örnek Servis Katmanı

Daha temiz bir kurumsal yapı:

```text
Controllers / UI
       |
       v
Application Services
       |
       v
Logo Services
       |
       +--> ItemService
       +--> ClientService
       +--> InvoiceService
       +--> ProductionService
       |
       v
LogoConnectionManager
       |
       v
IApplication
```

Bu tasarımda iş kodunun tamamı doğrudan `IApplication` çağrılarıyla dolmaz.

Örneğin kullanıcı arayüzü şu kodu bilmek zorunda değildir:

```text
IData nasıl yaratılır?
Hangi Logo enum tipi kullanılır?
Hangi alan adı doldurulur?
```

Bunları Logo servis katmanı yönetir.

---

## 21. Sağlık Kontrolü

Uzun yaşayan entegrasyon servislerinde yalnızca prosesin çalışıyor olması yeterli değildir.

Logo bağlantısının gerçekten çalıştığını kontrol eden health check mekanizması tasarlanabilir.

Örnek:

```text
Service Health
    |
    +--> Process alive
    +--> Database reachable
    +--> Logo Objects initialized
    +--> Login/session valid
    +--> Test query successful
```

Bu sayede servis Windows tarafında `Running` görünürken Logo bağlantısının kopmuş olması gözden kaçmaz.

---

## 22. Retry Politikası

Her Logo Objects hatasında otomatik retry yapılmamalıdır.

Örneğin:

### Retry yapılabilecek durumlar

- Geçici network problemi,
- Geçici servis erişim problemi,
- Session yeniden kurulabilecek bağlantı hatası.

### Retry yapılmaması gereken durumlar

- Zorunlu alan eksik,
- Geçersiz cari kodu,
- Geçersiz malzeme,
- İş kuralı ihlali,
- Aynı fiş numarası,
- Yetki problemi.

Aksi halde hatalı kayıt sonsuz retry döngüsüne girebilir.

Örnek politika:

```text
Attempt 1
   |
   X Geçici hata
   |
30 sn
   |
Attempt 2
   |
   X
   |
2 dk
   |
Attempt 3
   |
Dead Letter / Manual Review
```

---

## 23. Çok Firmalı Entegrasyon

Aynı servis birden fazla Logo firmasına hizmet verebilir.

Örnek:

```text
Integration API
    |
    +--> Firm 040
    +--> Firm 102
    +--> Firm 202
    +--> Firm 803
```

Burada her isteğin firma bağlamının açık olması gerekir.

Önerilen istek modeli:

```json
{
  "firmNo": 202,
  "periodNo": 1,
  "sourceDocumentId": "MES-98213"
}
```

Firma bilgisini yalnızca uygulama ayarından okumak, çok firmalı yapılarda hatalı kayıt riskini artırır.

---

## 24. Audit Bilgileri

Logo'ya veri gönderen entegrasyonun kendi audit tablosunu tutması önerilir.

Örnek:

```text
LogoIntegrationAudit
--------------------
ID
FirmNo
PeriodNo
LogoUser
OperationType
ObjectType
SourceSystem
SourceRecordId
LogoLogicalRef
LogoDocumentNo
StartedAt
FinishedAt
Success
ErrorMessage
```

Bu tablo sayesinde şu sorular cevaplanabilir:

- Bu faturayı hangi sistem gönderdi?
- Ne zaman gönderdi?
- Hangi Logo kullanıcısıyla kaydoldu?
- Kaç kez denendi?
- İlk denemede hangi hata oluştu?
- Logo `LOGICALREF` değeri nedir?

---

## 25. IApplication İçin Best Practices

1. Kullanıcı adı ve parolayı kaynak koda yazma.
2. Entegrasyona özel teknik Logo kullanıcısı kullan.
3. Firma ve dönemi merkezi olarak yönet.
4. Oturum açma sonucunu mutlaka kontrol et.
5. Login başarısızlığında yalnızca şifreyi suçlama; ortamı da kontrol et.
6. COM nesne yaşam döngüsünü göz ardı etme.
7. Tek `IApplication` nesnesini kontrolsüz biçimde çok thread'e dağıtma.
8. Uzun yaşayan servislerde reconnect mekanizması tasarla.
9. Her işlemde firma/dönem bilgisini logla.
10. Uygulama kapanırken oturumu kontrollü kapat.
11. Logo versiyonuyla Objects kütüphanesi uyumluluğunu koru.
12. Üretim öncesinde gerçek Logo ortamında load test yap.

---

## 26. Bölüm Özeti

`IApplication`, Logo Objects uygulamasının temel omurgasıdır.

```text
IApplication
    |
    +--> Authentication
    +--> Company
    +--> Period
    +--> IData
    +--> IQuery
    +--> Session Lifecycle
```

Başarılı bir Logo Objects uygulaması yalnızca `Login()` çağrısını çalıştıran uygulama değildir.

Doğru tasarım;

- güvenli credential yönetimi,
- doğru firma/dönem bağlamı,
- kontrollü oturum yaşam döngüsü,
- sağlam hata yönetimi,
- logging,
- retry,
- thread ve COM yönetimi

gibi konuları birlikte ele alır.

Sonraki bölümde Logo Objects'in temel veri işlem nesnesi olan `IData` ayrıntılı biçimde incelenecektir.
