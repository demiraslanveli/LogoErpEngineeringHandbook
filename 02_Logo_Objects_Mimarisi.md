# 02 — Logo Objects Mimarisi

## 1. Logo Objects Nedir?

Logo Objects, Logo ERP ürünleri üzerinde uygulama katmanındaki iş kurallarını kullanarak veri okumak, yeni kayıt oluşturmak, mevcut kayıtları güncellemek, silmek ve çeşitli ERP işlemlerini dış uygulamalardan yönetmek için kullanılan nesne tabanlı entegrasyon katmanıdır.

Logo ERP veritabanındaki tablolar doğrudan erişilebilir olsa da ERP kayıtları yalnızca tablo satırlarından ibaret değildir. Bir kart veya fiş kaydedildiğinde Logo tarafında doğrulamalar, bağlı kayıt üretimleri, numaralandırma, stok/cari/muhasebe ilişkileri, seri-lot bağlantıları ve ürünün ilgili modülüne göre başka iş kuralları devreye girebilir.

Bu nedenle Logo Objects'in temel amacı şudur:

> **Dış uygulamanın Logo veritabanını taklit etmesi yerine, Logo'nun kendi iş kurallarını kullanarak işlem yapmasını sağlamak.**

---

## 2. Logo Objects'in ERP Mimarisi İçindeki Yeri

Tipik bir entegrasyon mimarisini aşağıdaki katmanlarla düşünebiliriz:

```text
Dış Uygulama / Servis / Ara Yazılım
            |
            v
       Logo Objects
            |
            v
      Logo İş Kuralları
            |
            v
      Logo ERP Verileri
            |
            v
       SQL Server
```

Buradaki kritik nokta, Logo Objects'in SQL Server yerine geçen bir teknoloji olmamasıdır.

SQL Server fiziksel veri saklama katmanıdır. Logo Objects ise ERP iş kurallarına erişim katmanıdır.

Bu ayrım özellikle aşağıdaki işlemlerde önemlidir:

- Malzeme kartı oluşturma,
- Cari hesap kartı oluşturma,
- Satınalma ve satış fişleri,
- İrsaliyeler,
- Faturalar,
- Stok fişleri,
- Üretim hareketleri,
- Seri/lot işlemleri,
- Bağlı muhasebe hareketleri,
- Karmaşık güncelleme ve silme işlemleri.

---

## 3. Temel Nesne Modeli

Logo Objects ile çalışırken en sık karşılaşılan temel nesneler şunlardır:

| Nesne | Temel görev |
|---|---|
| `IApplication` | Logo Objects oturumunun ve genel uygulama erişiminin merkezidir. |
| `IData` | Kart ve fiş gibi ERP veri nesneleri üzerinde CRUD işlemleri yapar. |
| `IQuery` | SQL sorgularını Logo Objects arayüzü üzerinden çalıştırmaya yarar. |
| `DataFields` | Bir `IData` nesnesinin alanlarına erişimi sağlar. |
| `Lines` | Fiş veya kart alt satır koleksiyonlarını temsil eder. |
| `ProductionApplication` | Detaylı üretim süreçlerine yönelik özel entegrasyon katmanıdır. |

Bu kitapta bu nesnelerin her biri ayrı bölümlerde ayrıntılı olarak ele alınacaktır.

---

## 4. IApplication: Giriş Noktası

Logo Objects uygulamalarının merkezinde `IApplication` bulunur.

Bir dış uygulama doğrudan `IData` ile başlamaz. Önce Logo ortamına erişen uygulama nesnesi oluşturulur ve gerekli oturum açma işlemleri gerçekleştirilir.

Kavramsal akış:

```text
Uygulamayı başlat
    |
    v
IApplication oluştur
    |
    v
Logo oturumu aç
    |
    v
Firma / dönem bağlamını belirle
    |
    +------------------+
    |                  |
    v                  v
NewDataObject       NewQuery
    |                  |
    v                  v
 IData               IQuery
```

`IApplication`, diğer Logo Objects nesnelerinin üretildiği merkezî nesne olarak düşünülmelidir.

---

## 5. IData: İşlemsel Veri Katmanı

`IData`, Logo Objects'in en önemli nesnelerinden biridir.

Kart veya fiş üzerinde aşağıdaki işlemler `IData` üzerinden yapılabilir:

- Veri okuma,
- Yeni kayıt oluşturma,
- Kayıt güncelleme,
- Kayıt silme.

Bir `IData` nesnesi genellikle `IApplication.NewDataObject(...)` üzerinden oluşturulur.

Buradaki parametre, hangi Logo veri tipinin kullanılacağını belirtir.

Örneğin uygulama;

- Malzeme kartı,
- Cari hesap kartı,
- Satış faturası,
- Satınalma faturası,
- Stok fişi

gibi farklı ERP nesneleri için uygun veri tipinde `IData` oluşturabilir.

Temel yaklaşım:

```text
IApplication
    |
    v
NewDataObject(DataObjectType)
    |
    v
IData
    |
    +--> DataFields
    |
    +--> Lines
    |
    +--> Read / New / Post / Delete
```

`IData` yalnızca tablo yazan bir nesne değildir. İlgili Logo veri tipinin iş kurallarını devreye sokan işlem nesnesidir.

---

## 6. DataFields

Bir `IData` nesnesindeki üst seviye alanlara `DataFields` koleksiyonu üzerinden erişilir.

Örneğin bir kartta kavramsal olarak şu alanlar bulunabilir:

```text
CODE
NAME
SPECODE
ACTIVE
...
```

Bir fişte ise:

```text
NUMBER
DATE
ARP_CODE
SOURCE_WH
...
```

Gerçek alanlar kullanılan veri nesnesinin tipine göre değişir.

Burada önemli olan nokta şudur:

> Logo Objects alan isimleri ve kullanılabilir alanlar, işlem yapılan veri nesnesinin şemasına bağlıdır.

Bu nedenle bir alanın yalnızca SQL tablosunda bulunması, aynı isimle `IData.DataFields` içinde bulunacağı anlamına gelmez.

---

## 7. Lines: Alt Satır Yapısı

Fişlerin çoğunda üst bilgi ile birlikte bir veya daha fazla satır bulunur.

Örnek:

```text
Satış Faturası
|
+-- Üst Bilgi
|   +-- Fiş No
|   +-- Tarih
|   +-- Cari
|   +-- İşyeri
|   +-- Ambar
|
+-- Satırlar
    +-- Malzeme 1
    +-- Malzeme 2
    +-- Hizmet
    +-- İndirim
```

Logo Objects'te bu alt kayıtlar genellikle `Lines` koleksiyonları üzerinden yönetilir.

Tipik işlem mantığı:

```text
Lines.AppendLine()
      |
      v
Yeni satır oluştur
      |
      v
Satır alanlarını doldur
      |
      v
Sonraki satıra geç
```

Fiş entegrasyonlarında en sık yapılan hatalardan biri yalnızca üst bilgiyi düşünmek ve alt satırların kendi veri modelini göz ardı etmektir.

---

## 8. IQuery: Sorgulama Katmanı

Logo Objects içinde SQL sorgularını çalıştırmak için `IQuery` arayüzü kullanılabilir.

Bu özellik özellikle aşağıdaki senaryolarda faydalıdır:

- Özel rapor sorguları,
- Entegrasyon öncesi referans bulma,
- Kontrol sorguları,
- Logo Objects tarafından doğrudan sunulmayan özel veri okumaları,
- Yardımcı lookup işlemleri.

Ancak `IQuery` bulunması, Logo tablolarında serbestçe `INSERT`, `UPDATE` veya `DELETE` yapılmasının doğru olduğu anlamına gelmez.

Logo Objects yaklaşımında kural şudur:

> **Kart ve fiş kayıtları mümkün olduğunca `IData` üzerinden yönetilmelidir.**

Doğrudan SQL veri değiştirme işlemleri istisnai ve kontrollü durumlar için değerlendirilmelidir.

---

## 9. IData ile IQuery Arasındaki Temel Fark

| Konu | IData | IQuery |
|---|---|---|
| Kart/fiş oluşturma | Doğru tercih | Önerilmez |
| Kart/fiş güncelleme | Doğru tercih | Yüksek risk |
| Kart/fiş silme | Doğru tercih | Yüksek risk |
| Özel SELECT sorgusu | Sınırlı | Doğru tercih |
| Logo iş kuralları | Kullanılır | SQL ifadesine bağlıdır |
| Veri bütünlüğü | Logo tarafından yönetilir | Geliştiricinin sorumluluğundadır |

En önemli mimari ayrım budur.

---

## 10. Neden Doğrudan SQL INSERT/UPDATE/DELETE Risklidir?

Logo ERP kayıtları arasında çok sayıda ilişki vardır.

Örneğin bir satınalma faturası aşağıdaki yapılarla ilişkili olabilir:

```text
INVOICE
  |
  +--> STLINE
  |
  +--> STFICHE
  |
  +--> CLFLINE
  |
  +--> EMFICHE / EMFLINE
  |
  +--> Seri/Lot bağlantıları
  |
  +--> Dağıtım ve maliyet ilişkileri
```

Bunların tamamını yalnızca tablo bilgisine bakarak manuel üretmeye çalışmak ciddi risk taşır.

Riskler:

- Eksik kayıt,
- Yanlış referans,
- Bozuk `LOGICALREF` ilişkisi,
- Muhasebeleşme sorunları,
- Stok toplamı ile fiş toplamı arasında tutarsızlık,
- Seri/lot stoklarında fark,
- Maliyet hesaplarında bozulma,
- Logo ekranında açılamayan veya silinemeyen kayıtlar.

Bu nedenle SQL ile doğrudan veri değiştirme yapılacaksa geliştirici, etkilenen tüm tablo ve iş kurallarını gerçekten biliyor olmalıdır.

---

## 11. Logo Objects'in Sağladığı Temel Güvence

Logo Objects ile kayıt oluşturulduğunda amaç yalnızca `INSERT` işlemini kolaylaştırmak değildir.

Asıl değer şudur:

```text
Girdi
  |
  v
Logo Objects
  |
  +--> Alan doğrulamaları
  +--> İş kuralları
  +--> Referans kontrolleri
  +--> Bağlı hareketler
  +--> Gerekli hesaplamalar
  +--> Veri bütünlüğü
  |
  v
ERP Kaydı
```

Dolayısıyla Logo Objects'i bir ORM veya basit database wrapper olarak görmek doğru değildir.

---

## 12. Firma ve Dönem Bağlamı

Logo ERP verilerinin önemli bir kısmı firma ve dönem bazlıdır.

Örnek tablo isimleri:

```text
LG_040_ITEMS
LG_040_01_INVOICE
LG_040_01_STFICHE
LG_040_01_STLINE
```

Burada:

- `040` firma numarasını,
- `01` dönem numarasını

temsil eder.

Logo Objects ile işlem yapılırken de uygulamanın doğru firma ve dönem bağlamında çalışması kritik önem taşır.

Yanlış bağlamda çalışan entegrasyon, teknik olarak başarılı görünse bile yanlış firmaya veya döneme kayıt üretebilir.

---

## 13. LOGICALREF Mantığı

Logo tablolarında kayıtların temel teknik anahtarı çoğunlukla `LOGICALREF` alanıdır.

Bu alan Logo içindeki kayıt ilişkilerinde kritik role sahiptir.

Örneğin:

```text
ITEMS.LOGICALREF
        |
        +--> STLINE.STOCKREF

CLCARD.LOGICALREF
        |
        +--> INVOICE.CLIENTREF

STFICHE.LOGICALREF
        |
        +--> STLINE.STFICHEREF
```

Ancak Logo Objects kullanan bir uygulamanın temel hedefi, bütün bu referans zincirlerini elle üretmek olmamalıdır.

Logo Objects'in kullanılması gereken nokta tam olarak burasıdır.

---

## 14. Transaction Yaklaşımı

Bir ERP entegrasyonunda kayıt başarılı veya başarısız olarak ele alınmalıdır.

Örneğin 100 satırlık bir fiş gönderildiğinde 70 satırın kaydolup kalan 30 satırın başarısız olması çoğu senaryoda kabul edilebilir değildir.

İyi entegrasyon yaklaşımı:

```text
Başla
  |
  v
Veriyi doğrula
  |
  v
IData oluştur
  |
  v
Üst bilgiyi doldur
  |
  v
Tüm satırları oluştur
  |
  v
Post
  |
  +--> Başarılı --> Commit / log
  |
  +--> Hatalı ----> Hata kaydı / geri dönüş
```

Kullanılan Logo Objects sürümünün transaction kabiliyetleri ve ilgili nesnenin davranışı dikkate alınmalıdır.

---

## 15. Hata Yönetimi

Logo Objects entegrasyonlarında yalnızca `true/false` sonucu kontrol etmek yeterli değildir.

Uygulama aşağıdaki bilgileri loglamalıdır:

- İşlem tipi,
- Firma,
- Dönem,
- Veri nesnesi tipi,
- Harici sistem kayıt ID'si,
- Logo kayıt referansı,
- Fiş/kart numarası,
- İşlem başlangıç ve bitiş zamanı,
- Başarı durumu,
- Logo hata mesajı,
- Uygulama exception bilgisi.

Önerilen log modeli:

```text
IntegrationLog
--------------
ID
SourceSystem
SourceRecordId
FirmNo
PeriodNo
ObjectType
LogoLogicalRef
DocumentNo
Status
ErrorMessage
CreatedAt
CompletedAt
```

Bu yapı üretim ortamında hata ayıklamayı ciddi biçimde kolaylaştırır.

---

## 16. Idempotency

ERP entegrasyonlarında aynı belgenin iki kez Logo'ya gönderilmesi önemli bir problemdir.

Örneğin dış sistem aynı satış siparişini ağ hatası nedeniyle tekrar gönderebilir.

Kötü yaklaşım:

```text
Her gelen isteği doğrudan yeni fiş olarak kaydet.
```

Doğru yaklaşım:

```text
Harici kayıt anahtarını kontrol et
        |
        +--> Daha önce işlendi --> Mevcut Logo kaydını döndür
        |
        +--> İşlenmedi --------> Yeni kayıt oluştur
```

Entegrasyon tablosunda örneğin şu bilgiler tutulabilir:

```text
SourceSystem
SourceDocumentId
LogoLogicalRef
LogoDocumentNo
Status
```

Bu sayede retry mekanizması güvenli hâle gelir.

---

## 17. Logo Objects ve Ara Yazılım

Kurumsal projelerde Logo Objects doğrudan kullanıcı uygulamasının içine gömülmek zorunda değildir.

Daha sürdürülebilir yapı çoğu zaman şöyledir:

```text
MES / WMS / Web / Mobil / Diğer Sistem
                 |
                 v
          Entegrasyon API'si
                 |
                 v
         İş Kuralı Katmanı
                 |
                 v
           Logo Objects
                 |
                 v
             Logo ERP
```

Bu ara katman sayesinde:

- Logo kullanıcı bilgileri istemcilere dağıtılmaz,
- Merkezi logging yapılır,
- Retry yönetilir,
- Idempotency sağlanır,
- Veri doğrulama merkezi hâle gelir,
- Birden fazla sistem aynı entegrasyon servisinden yararlanabilir.

---

## 18. Logo Objects Kullanırken Temel Tasarım Prensipleri

### 18.1 Önce Logo nesnesini düşün

Bir kart veya fiş kaydı oluşturacaksan önce ilgili `IData` nesnesini araştır.

SQL tablo tasarımından başlamamak gerekir.

### 18.2 SQL'i okuma ve analiz için güçlü kullan

SQL, Logo ERP projelerinde vazgeçilmezdir.

Özellikle:

- Raporlama,
- Veri analizi,
- Performans analizi,
- Entegrasyon kontrolleri,
- Mutabakat,
- Hata araştırması

için kullanılmalıdır.

### 18.3 Yazma işlemlerinde veri bütünlüğünü önceliklendir

`INSERT`, `UPDATE`, `DELETE` işlemleri yapılabiliyor olması onların güvenli olduğu anlamına gelmez.

### 18.4 Referansları rastgele üretme

Logo kayıtlarındaki referans zincirlerinin nasıl oluştuğu bilinmeden `LOGICALREF` ve bağlı referans alanlarıyla oynanmamalıdır.

### 18.5 Hataları logla

Üretim entegrasyonunda kullanıcıya yalnızca "Kayıt başarısız" demek yeterli değildir.

### 18.6 Retry tasarımını baştan yap

Ağ kopması, servis kapanması veya Logo hatası her zaman mümkündür.

### 18.7 Firma/dönem bilgisini sabit varsayma

Çok firmalı yapılarda firma ve dönem merkezi konfigürasyondan yönetilmelidir.

---

## 19. Ne Zaman Logo Objects, Ne Zaman SQL?

### Logo Objects tercih edilmesi gereken işlemler

- Malzeme kartı oluşturma,
- Cari kart oluşturma,
- Kart güncelleme,
- Sipariş oluşturma,
- İrsaliye oluşturma,
- Fatura oluşturma,
- Stok fişi oluşturma,
- Resmî ERP hareketi oluşturan işlemler,
- Kart veya fiş silme.

### SQL'in güçlü olduğu işlemler

- Raporlama,
- Dashboard,
- Kontrol sorguları,
- Veri karşılaştırma,
- Performans analizi,
- Entegrasyon öncesi lookup,
- Hata araştırma,
- Arşiv ve özel rapor tabloları.

### Kontrollü değerlendirilmesi gereken işlemler

- Doğrudan Logo tablosuna `UPDATE`,
- Doğrudan Logo tablosuna `DELETE`,
- Doğrudan Logo tablosuna `INSERT`.

Bunlar ancak ilgili iş kuralı ve tüm ilişkiler kesin olarak biliniyorsa değerlendirilmelidir.

---

## 20. Örnek Entegrasyon Senaryosu

Bir MES uygulamasının üretim sonucunu Logo'ya aktardığını düşünelim.

Kötü tasarım:

```text
MES
 |
 +--> SQL INSERT STLINE
 +--> SQL INSERT STFICHE
 +--> SQL INSERT başka tablolar
```

Bu tasarım Logo'nun üretim, seri/lot, maliyet ve diğer iş kurallarını atlama riski taşır.

Daha doğru mimari:

```text
MES
 |
 v
Integration Service
 |
 +--> Veri doğrulama
 +--> Eşleştirme
 +--> Idempotency
 +--> Logging
 |
 v
ProductionApplication / Logo Objects
 |
 v
Logo ERP
```

Bu yaklaşımda ara yazılım operasyon sürecini yönetebilir; ancak resmî ERP hareketleri Logo tarafında doğru nesnelerle oluşturulur.

---

## 21. Sık Yapılan Hatalar

### Hata 1 — Logo Objects'i yalnızca SQL alternatifi sanmak

Logo Objects'in asıl değeri sorgu yazmaktan çok ERP iş kurallarına erişmektir.

### Hata 2 — Fişi tek tablo sanmak

Bir fişin bağlı olduğu tüm hareketler hesaba katılmalıdır.

### Hata 3 — Yalnızca başarılı `Post` sonucunu loglamak

Hata detayları ve kaynak kayıt kimliği de saklanmalıdır.

### Hata 4 — Aynı belgeyi yeniden göndermeyi hesaba katmamak

Idempotency tasarımı yapılmalıdır.

### Hata 5 — Firma ve dönemi kod içine gömmek

Konfigürasyonla yönetilmelidir.

### Hata 6 — SQL güncellemesini kısa yol olarak kullanmak

Kısa vadede çalışan çözüm, daha sonra veri bütünlüğü veya maliyet problemi üretebilir.

---

## 22. Bölüm Özeti

Logo Objects mimarisinin temel modeli şöyledir:

```text
IApplication
    |
    +--> IData
    |      |
    |      +--> DataFields
    |      +--> Lines
    |      +--> Kart / Fiş CRUD
    |
    +--> IQuery
    |      |
    |      +--> SQL sorguları
    |
    +--> Üretim ve diğer özel nesneler
```

Temel kural:

> **ERP kaydı oluştururken Logo'nun iş kurallarını kullan; SQL'i özellikle okuma, analiz ve kontrollü yardımcı işlemler için kullan.**

Sonraki bölümde `IApplication` nesnesi ayrıntılı olarak ele alınacaktır.
