# 01 — Logo ERP Mimarisi

## 1. Bölümün Amacı

Bu bölüm, Logo Tiger / Tiger Wings Enterprise altyapısının geliştirici gözüyle nasıl ele alınması gerektiğini açıklar. Amaç yalnızca tablo isimlerini veya nesne metodlarını ezberlemek değil; **Logo ERP'nin veri bütünlüğünü hangi katmanlarla koruduğunu, Logo Objects'in bu mimaride neden merkezî olduğunu ve SQL erişiminin nerede konumlandırılması gerektiğini** doğru anlamaktır.

Bu mimariyi doğru anlamadan geliştirilen entegrasyonlarda en sık görülen problemler şunlardır:

- Bir fişin yalnızca üst bilgisinin oluşması, bağlı satırların eksik kalması,
- Stok hareketi oluştuğu hâlde cari veya muhasebe hareketinin oluşmaması,
- Seri/lot bağlantılarının kopması,
- Üretim ve maliyetlendirme ilişkilerinin eksik oluşması,
- Logo ekranında görünen kayıt ile doğrudan SQL üzerinden oluşturulan kaydın davranışının farklı olması,
- Silme veya güncelleme sırasında bağlı kayıtların yetim kalması,
- Logo sürüm değişikliklerinden sonra özel entegrasyonların bozulması.

Bu nedenle temel prensip şudur:

> **Logo ERP yalnızca bir SQL veritabanı değildir. Veritabanı, Logo iş kurallarının ürettiği sonuçların saklandığı katmandır.**

Resmî kart ve fiş işlemlerinde mümkün olduğunca Logo Objects kullanılmalı; doğrudan SQL ile `INSERT`, `UPDATE` ve `DELETE` işlemleri yalnızca gerçekten gerekli, etkileri tam olarak bilinen ve kontrollü senaryolarda uygulanmalıdır.

---

# 2. Logo ERP'yi Katmanlı Olarak Düşünmek

Logo ERP entegrasyonlarını aşağıdaki katmanlarla düşünmek faydalıdır:

```text
+----------------------------------------------------------+
|                  Kullanıcı / Dış Sistem                  |
|   Logo UI | Özel Uygulama | MES | WMS | Web | Servis    |
+----------------------------------------------------------+
                         |
                         v
+----------------------------------------------------------+
|                  Uygulama / Entegrasyon                  |
|             Logo Objects / REST / Özel Servis            |
+----------------------------------------------------------+
                         |
                         v
+----------------------------------------------------------+
|                    Logo İş Kuralları                     |
| Kartlar | Fişler | Stok | Cari | Muhasebe | Üretim      |
| Seri/Lot | Kalite | Maliyet | Bağlantılı Hareketler      |
+----------------------------------------------------------+
                         |
                         v
+----------------------------------------------------------+
|                     SQL Server                           |
| LG_XXX_* | LG_XXX_YY_* | L_CAPIDIV | Sistem Tabloları   |
+----------------------------------------------------------+
```

Bu modelde en kritik ayrım şudur:

- **SQL Server**, fiziksel veri saklama katmanıdır.
- **Logo Objects**, Logo iş kurallarına kontrollü erişim sağlayan programlama katmanıdır.
- **Logo ERP uygulaması**, kullanıcı işlemleri ile bu iş kurallarını çalıştıran üst katmandır.

Bir geliştirici yalnızca SQL tablolarına bakarsa mimarinin önemli bir bölümünü görmez.

---

# 3. Firma ve Dönem Mimarisi

Logo ERP veritabanında birçok operasyonel tablo firma ve dönem numarasına göre fiziksel olarak ayrılır.

Örnek firma numarası:

```text
040
```

Örnek dönem numarası:

```text
01
```

Bu durumda bazı tablo örnekleri şöyledir:

```text
LG_040_ITEMS
LG_040_CLCARD
LG_040_01_STFICHE
LG_040_01_STLINE
LG_040_01_INVOICE
LG_040_01_CLFLINE
LG_040_01_EMFICHE
LG_040_01_EMFLINE
```

Burada iki temel tablo grubu vardır.

## 3.1 Firma Bazlı Tablolar

Bazı kartlar dönemden bağımsızdır ve yalnızca firma numarası taşır.

Örnek:

```text
LG_040_ITEMS
LG_040_CLCARD
LG_040_UNITSETF
LG_040_UNITSETL
```

Bunlar genel olarak:

- malzeme kartları,
- cari hesap kartları,
- birim setleri,
- firma seviyesindeki temel tanımlar

gibi yapılardır.

## 3.2 Firma + Dönem Bazlı Tablolar

Operasyonel hareket tablolarının önemli bir bölümü dönem numarası içerir.

Örnek:

```text
LG_040_01_STFICHE
LG_040_01_STLINE
LG_040_01_INVOICE
LG_040_01_CLFLINE
LG_040_01_EMFICHE
LG_040_01_EMFLINE
```

Bu nedenle entegrasyon geliştirirken yalnızca `CompanyId` bilgisi yeterli olmayabilir. İşlemin hangi mali dönemde yapılacağı da bilinmelidir.

---

# 4. LOGICALREF Kavramı

Logo tablolarının büyük bölümünde kayıtların temel anahtarı `LOGICALREF` alanıdır.

Örnek:

```sql
SELECT
    LOGICALREF,
    CODE,
    NAME
FROM LG_040_ITEMS;
```

`LOGICALREF`, Logo içerisindeki kayıtların birbirine bağlanmasında kritik öneme sahiptir.

Örneğin stok hareket satırında:

```text
STOCKREF
```

alanı çoğunlukla malzeme kartının:

```text
LG_XXX_ITEMS.LOGICALREF
```

alanına referans verir.

Benzer şekilde:

```text
CLIENTREF
PROJECTREF
ACCOUNTREF
STFICHEREF
INVOICEREF
```

gibi alanların büyük bölümü farklı tabloların `LOGICALREF` değerleri üzerinden ilişki kurar.

Ancak önemli bir uyarı vardır:

> Bir alanın adının `...REF` ile bitmesi, geliştiricinin bu alanı doğrudan SQL ile doldurmasının güvenli olduğu anlamına gelmez.

Logo Objects kullanıldığında bu ilişkilerin önemli bir bölümü Logo'nun kendi iş kuralları tarafından yönetilir.

---

# 5. Kart ve Fiş Ayrımı

Logo mimarisini anlamanın en temel yollarından biri **kart** ve **fiş** ayrımını doğru yapmaktır.

## 5.1 Kartlar

Kartlar ana veri niteliğindedir.

Örnekler:

- Malzeme kartı,
- Cari hesap kartı,
- Muhasebe hesap kartı,
- Banka hesabı,
- Kasa kartı,
- Birim seti,
- Proje kartı.

Kartlar genellikle başka hareketlerin referans verdiği temel kayıtlardır.

Örneğin:

```text
Malzeme Kartı
    |
    +--> Satınalma hareketleri
    +--> Satış hareketleri
    +--> Ambar hareketleri
    +--> Üretim hareketleri
    +--> Seri/Lot hareketleri
```

## 5.2 Fişler

Fişler operasyonel işlemleri temsil eder.

Örnekler:

- Satınalma irsaliyesi,
- Satış irsaliyesi,
- Satınalma faturası,
- Satış faturası,
- Ambar fişi,
- Sarf fişi,
- Üretimden giriş fişi,
- Cari hesap fişi,
- Muhasebe fişi.

Fiş mimarisi çoğunlukla şu şekildedir:

```text
Fiş Başlığı
    |
    +--> Satır 1
    +--> Satır 2
    +--> Satır 3
    |
    +--> Bağlı hareketler
         +--> Cari hareket
         +--> Muhasebe hareketi
         +--> Seri/Lot hareketi
         +--> Maliyet hareketi
```

Bu nedenle bir fişi yalnızca başlık tablosuna kayıt atarak oluşturmak doğru değildir.

---

# 6. Logo Objects'in Mimarideki Yeri

Logo Objects, Logo ERP üzerinde programatik işlem yapmak için kullanılan nesne tabanlı erişim katmanıdır.

Temel yaklaşım:

```text
IApplication
    |
    +--> IData
    +--> IQuery
    +--> diğer Logo Objects nesneleri
```

Kitabın sonraki bölümlerinde bu nesneler ayrıntılı olarak ele alınacaktır.

Burada mimari açıdan bilinmesi gereken temel ilişki şudur:

## IApplication

Logo Objects oturumunun merkez nesnesidir.

Genellikle:

- bağlantı,
- login,
- firma,
- dönem,
- veri nesnesi oluşturma,
- sorgu nesnesi oluşturma

gibi işlemlerin başlangıç noktasıdır.

## IData

Logo'nun kart ve fiş veri nesnesidir.

Temel kullanım amacı:

- kayıt okuma,
- yeni kayıt oluşturma,
- kayıt güncelleme,
- kayıt silme

işlemleridir.

Örnek kavramsal kullanım:

```text
IApplication
    -> NewDataObject(...)
        -> IData
            -> New()
            -> Fields
            -> Lines
            -> Post()
```

Logo Objects ile kayıt oluşturmanın temel avantajı, Logo'nun kendi veri doğrulama ve iş kurallarının devreye girmesidir.

## IQuery

SQL sorgularını çalıştırmak için kullanılan Logo Objects arayüzüdür.

Özellikle:

- raporlama,
- kontrol,
- özel sorgular,
- Logo Objects içerisinde doğrudan veri nesnesi olarak sunulmayan okuma ihtiyaçları

için kullanılabilir.

Ancak `IQuery` üzerinden SQL çalıştırılabiliyor olması, bütün veri değişikliklerinin SQL ile yapılması gerektiği anlamına gelmez.

---

# 7. IData Neden Önemlidir?

`IData`, Logo Objects dünyasının en önemli nesnelerinden biridir.

Bir kart veya fiş türü seçilerek ilgili Logo nesnesi üzerinde işlem yapılmasını sağlar.

Kavramsal olarak:

```text
IData = Logo iş nesnesinin programatik temsilidir.
```

Bir `IData` nesnesi üzerinden:

- mevcut kayıt okunabilir,
- yeni kayıt oluşturulabilir,
- mevcut kayıt değiştirilebilir,
- kayıt silinebilir.

Ancak asıl değer yalnızca CRUD işlemleri değildir.

`IData` kullanıldığında Logo'nun:

- zorunlu alan kontrolleri,
- veri tipi kontrolleri,
- fiş-satır ilişkileri,
- kart referansları,
- bazı otomatik alan hesaplamaları,
- bağlantılı hareket üretimi,
- iş kuralı doğrulamaları

gibi mekanizmalarından yararlanılır.

Bu nedenle resmî Logo hareketlerinde tercih edilen yol budur.

---

# 8. Doğrudan SQL Neden Risklidir?

Logo ERP ürününe dışarıdan yalnızca SQL seviyesinde yaklaşmak bazı ilişkilerin atlanmasına neden olabilir.

Örneğin teorik olarak şu komut çalışabilir:

```sql
INSERT INTO LG_040_01_STLINE (...)
VALUES (...);
```

Ancak bu satırın fiziksel olarak tabloya yazılması, Logo açısından geçerli ve eksiksiz bir hareket olduğu anlamına gelmez.

Eksik kalabilecek alan veya ilişkiler arasında şunlar bulunabilir:

- fiş başlığı bağlantısı,
- sipariş bağlantısı,
- fatura bağlantısı,
- cari hareket bağlantısı,
- muhasebe bağlantısı,
- seri/lot ilişkisi,
- maliyet ilişkileri,
- ambar bilgileri,
- üretim emri bağlantıları,
- proje bağlantıları,
- kullanıcı / kayıt izleri,
- Logo'nun sürüme özel ek alanları.

Bu nedenle temel kural:

```text
READ       -> SQL / IQuery çoğu durumda uygundur.
INSERT     -> Öncelik IData / Logo Objects.
UPDATE     -> Öncelik IData / Logo Objects.
DELETE     -> Öncelik IData / Logo Objects.
```

Doğrudan SQL güncellemeleri tamamen yasak değildir; fakat kullanılacağı durumda geliştirici **hangi tabloların ve hangi ilişkilerin etkilendiğini kesin olarak bilmelidir**.

---

# 9. Bir İşlemin Tek Tablo Olmadığını Anlamak

Logo ERP'deki önemli işlemlerin çoğu birden fazla tabloya yayılır.

Örneğin satış faturası düşünelim.

Basitleştirilmiş yapı:

```text
LG_XXX_YY_INVOICE
       |
       +--> LG_XXX_YY_STLINE
       |
       +--> LG_XXX_YY_CLFLINE
       |
       +--> LG_XXX_YY_EMFICHE
                 |
                 +--> LG_XXX_YY_EMFLINE
```

Gerçek senaryo işlem türüne ve konfigürasyona göre daha fazla ilişki içerebilir.

Bu yüzden yalnızca:

```sql
UPDATE LG_102_01_INVOICE
SET DATE_ = '2026-07-31'
WHERE FICHENO = '...';
```

şeklinde yapılan bir işlem, bağlı irsaliye, stok satırı, cari hareket veya muhasebe fişi tarihlerini eski tarihte bırakabilir.

Bu tip müdahalelerde ilişki zinciri mutlaka analiz edilmelidir.

---

# 10. TRCODE Mantığı

Logo ERP tablolarında işlem tiplerinin önemli bölümü `TRCODE` alanı ile ayrılır.

Ancak `TRCODE` değerini yalnızca sayısal bir alan olarak görmek hatalıdır.

`TRCODE`:

- tablonun türüne,
- modüle,
- kayıt yapısına

göre farklı anlamlara gelebilir.

Bu nedenle bir sorguda:

```sql
WHERE TRCODE = 8
```

yazmadan önce mutlaka:

1. Hangi tablo üzerinde çalışıldığı,
2. Bu tabloda `TRCODE = 8` değerinin ne anlama geldiği,
3. İşlemin bağlı tablolarda hangi `TRCODE` değerlerini ürettiği

kontrol edilmelidir.

---

# 11. Başlık — Satır — Alt Satır İlişkisi

Logo fişlerinde en yaygın model:

```text
Header
   |
   +--> Lines
```

Logo Objects tarafında bu yapı genellikle `IData` ve onun `Lines` koleksiyonları üzerinden yönetilir.

Örnek kavramsal yapı:

```text
IData
  Fields
    FICHENO
    DATE
    CLIENTREF
    ...

  Lines
    [0]
      TYPE
      MASTER_CODE
      QUANTITY
      PRICE
      ...

    [1]
      TYPE
      MASTER_CODE
      QUANTITY
      PRICE
      ...
```

Bazı nesnelerde satırların da kendi alt satır veya dağıtım yapılarına sahip olabileceği unutulmamalıdır.

---

# 12. Seri/Lot Mimarisi

Seri ve lot kullanılan sistemlerde hareket yalnızca stok miktarından ibaret değildir.

Örneğin bir malzemenin:

```text
Miktar = 100
```

olması yeterli bilgi değildir.

Aynı 100 birim şu şekilde dağılmış olabilir:

```text
LOT-A -> 40
LOT-B -> 35
LOT-C -> 25
```

Ayrıca:

- hangi ambarda olduğu,
- hangi stok yerinde olduğu,
- son kullanma tarihi,
- üretim tarihi,
- giriş hareketi,
- çıkış hareketi,
- üretim emri bağlantısı

gibi bilgiler de önemlidir.

Doğrudan SQL ile stok hareketi oluşturulup seri/lot hareketleri oluşturulmazsa Logo'daki fiziksel stok ile seri/lot stokları arasında tutarsızlık oluşabilir.

Bu nedenle seri/lot kullanılan projelerde Logo Objects üzerinden işlem yapmak daha da kritik hâle gelir.

---

# 13. Üretim Mimarisi

Detaylı üretim kullanılan Logo Tiger Enterprise sistemlerinde veri modeli daha karmaşıktır.

Bir üretim sürecinde yalnızca:

```text
Ham madde çıkışı
Mamül girişi
```

yoktur.

Süreç aşağıdaki nesneleri ve hareketleri kapsayabilir:

```text
Üretim Emri
    |
    +--> Operasyonlar
    +--> İş istasyonları
    +--> Malzeme ihtiyaçları
    +--> Sarf hareketleri
    +--> Fire hareketleri
    +--> Üretimden giriş
    +--> Seri/Lot
    +--> Kalite
    +--> Maliyetlendirme
```

Özellikle ilaç, veteriner ürünleri, gıda veya izlenebilirlik gerektiren üretim ortamlarında seri/lot, kalite ve maliyet zinciri birlikte düşünülmelidir.

Ara yazılım veya MES sistemi kullanılıyorsa ideal yaklaşım:

```text
MES / Operasyon Sistemi
        |
        v
Entegrasyon Katmanı
        |
        v
Logo Objects / Logo Servisleri
        |
        v
Logo ERP
```

Ara yazılım operasyonel süreçleri yönetebilir; ancak resmî stok, üretim ve maliyet hareketleri Logo tarafında eksiksiz oluşmalıdır.

---

# 14. Maliyetlendirme Açısından Mimari

Logo ERP'de stok hareketinin oluşması ile maliyetin doğru oluşması aynı şey değildir.

Maliyetlendirme için aşağıdaki bilgiler kritik olabilir:

- hareket tarihi,
- ambar,
- giriş / çıkış yönü,
- birim,
- miktar,
- fiyat,
- döviz bilgileri,
- üretim bağlantıları,
- sarf hareketleri,
- mamül üretim hareketleri,
- dönem maliyetlendirme işlemleri.

Bu nedenle doğrudan SQL ile geçmiş tarihli hareket değiştirmek, teknik olarak basit görünse bile maliyet sonuçlarını değiştirebilir.

Özellikle:

```text
DATE_
SOURCEINDEX
DESTINDEX
AMOUNT
PRICE
UINFO1
UINFO2
```

gibi alanlara müdahale edilirken bağlı süreçler dikkate alınmalıdır.

---

# 15. Birimler ve Çevrimler

Logo malzeme yapısında ana birim ve alternatif birimler bulunabilir.

Örneğin:

```text
Ana Birim    : ADET
İkinci Birim : KOLİ
```

ve çevrim:

```text
1 KOLİ = 12 ADET
```

şeklinde olabilir.

İlgili yapılar çoğunlukla aşağıdaki tablolarla ilişkilidir:

```text
LG_XXX_UNITSETF
LG_XXX_UNITSETL
LG_XXX_ITMUNITA
LG_XXX_ITEMS
```

Hareket satırlarında yalnızca miktar değil, birim ve çevrim bilgileri de önemlidir.

Yanlış birim kullanımı:

- stok miktarlarını,
- satınalma fiyat analizini,
- satış fiyatlarını,
- maliyet hesaplarını,
- raporları

doğrudan etkileyebilir.

---

# 16. Veri Okumada SQL'in Gücü

Logo Objects her durumda SQL'in alternatifi değildir.

Özellikle raporlama ve analiz işlerinde SQL Server doğrudan kullanılabilir.

Örnek kullanım alanları:

- stok raporu,
- cari yaşlandırma,
- son alış fiyatı,
- üretim performansı,
- satış analizi,
- maliyet analizi,
- hareket kontrolü,
- hata tespiti,
- Power BI veri kaynakları.

Bu durumda en doğru yaklaşım çoğunlukla:

```text
Veri Okuma / Analiz  -> SQL
Resmî Veri Yazma     -> Logo Objects
```

şeklindedir.

Bu kesin bir yasak/izin matrisi değildir; güvenli varsayılan mimari prensiptir.

---

# 17. Entegrasyon Tasarımında Önerilen Katmanlar

Profesyonel bir Logo entegrasyonunda aşağıdaki katmanlar ayrılmalıdır.

## 17.1 Domain Katmanı

İş nesneleri burada tanımlanır.

Örnek:

```text
PurchaseInvoice
SalesInvoice
Material
Customer
ProductionOrder
WarehouseTransaction
```

## 17.2 Logo Adapter Katmanı

Logo Objects'e özel kod burada tutulur.

Örnek:

```text
LogoApplicationService
LogoMaterialService
LogoInvoiceService
LogoProductionService
```

## 17.3 SQL Read Model

Raporlama ve hızlı sorgular için ayrı veri erişim katmanı kullanılır.

Örnek:

```text
LogoReportingRepository
LogoStockRepository
LogoCostRepository
```

## 17.4 Integration Service

Dış sistem ile Logo arasındaki orkestrasyonu yönetir.

```text
MES
 |
 v
Integration Service
 |
 +--> Validation
 +--> Mapping
 +--> Logo Objects
 +--> Logging
 +--> Retry
```

Bu tasarım Logo Objects kodunun uygulamanın tamamına dağılmasını engeller.

---

# 18. Entegrasyonlarda Idempotency

Logo entegrasyonlarında en önemli konulardan biri aynı verinin iki kez gönderilmesini engellemektir.

Örneğin MES sistemi şu üretim kaydını gönderdi:

```text
MES_ID = PRD-2026-000045
```

Entegrasyon servisinin aynı kaydı ikinci kez aldığında yeni bir Logo fişi oluşturmaması gerekir.

Önerilen yapı:

```text
ExternalSystem
    |
    | ExternalId
    v
IntegrationLog
    |
    | LogoLogicalRef
    v
Logo ERP
```

Örnek entegrasyon log alanları:

```text
ID
ExternalSystem
ExternalId
OperationType
CompanyNo
PeriodNo
LogoLogicalRef
Status
RequestDate
ResponseDate
ErrorMessage
```

Bu mekanizma özellikle:

- servis tekrarlarında,
- ağ kesintilerinde,
- timeout durumlarında,
- batch entegrasyonlarında

hayati önem taşır.

---

# 19. Transaction Yönetimi

Bir entegrasyon işlemi birden fazla kayıt üretiyorsa transaction mantığı düşünülmelidir.

Örneğin:

```text
Satınalma Faturası
    +--> Stok hareketleri
    +--> Cari hareket
    +--> Muhasebe hareketi
```

Logo Objects ilgili iş nesnesini kaydederken kendi transaction mekanizmalarını çalıştırabilir.

Dış entegrasyon katmanı ise ayrıca şu senaryoları yönetmelidir:

- Logo kaydı başarılı, dış sistem güncellemesi başarısız,
- dış sistem kaydı başarılı, Logo kaydı başarısız,
- timeout oluştu fakat Logo kaydı gerçekte oluştu,
- aynı istek tekrar gönderildi.

Bu nedenle yalnızca teknik transaction değil, **işlemsel bütünlük ve tekrar çalıştırılabilirlik** de tasarlanmalıdır.

---

# 20. Logging ve İzlenebilirlik

Logo entegrasyonlarında yalnızca hata mesajı kaydetmek yeterli değildir.

En azından aşağıdaki bilgiler loglanmalıdır:

```text
Timestamp
CompanyId
PeriodId
Operation
DataObjectType
ExternalId
LogoLogicalRef
Success
ErrorCode
ErrorMessage
User
Host
Application
```

İdeal olarak ayrıca:

- gönderilen temel alanlar,
- Logo'dan dönen hata bilgileri,
- işlem süresi,
- tekrar sayısı

tutulmalıdır.

Ancak parola, token ve diğer hassas bilgiler loglara yazılmamalıdır.

---

# 21. Logo Objects ile SQL'in Birlikte Kullanımı

En sağlıklı entegrasyonlar genellikle Logo Objects ve SQL'i birbirinin alternatifi değil, tamamlayıcısı olarak kullanır.

Örnek mimari:

```text
                    +-------------------+
                    |   Özel Uygulama   |
                    +---------+---------+
                              |
             +----------------+----------------+
             |                                 |
             v                                 v
   +-------------------+             +-------------------+
   |   Logo Objects    |             |    SQL Server     |
   |                   |             |                   |
   | INSERT / UPDATE   |             | SELECT / REPORT   |
   | DELETE / BUSINESS |             | ANALYSIS / CHECK  |
   +---------+---------+             +---------+---------+
             |                                 |
             +----------------+----------------+
                              |
                              v
                       +-------------+
                       |  Logo ERP   |
                       +-------------+
```

Örneğin bir özel uygulama:

1. SQL ile malzeme mevcut mu kontrol eder,
2. SQL ile raporlama bilgilerini okur,
3. yeni fiş oluşturmak için Logo Objects kullanır,
4. oluşan `LOGICALREF` değerini entegrasyon loguna yazar,
5. sonuçları tekrar SQL üzerinden raporlayabilir.

Bu hibrit yaklaşım çoğu gerçek projede hem performanslı hem güvenlidir.

---

# 22. Logo ERP ile Çalışırken Temel Güvenlik Kuralları

## Kural 1 — Önce Veri Modelini Anla

Bir tabloya müdahale etmeden önce ilişkili tablolar belirlenmelidir.

## Kural 2 — Resmî İşlemlerde Logo Objects'i Tercih Et

Özellikle:

- kart oluşturma,
- fatura oluşturma,
- irsaliye oluşturma,
- üretim hareketi,
- seri/lot hareketi

gibi işlemlerde.

## Kural 3 — SQL UPDATE Öncesi Mutlaka SELECT Çalıştır

Örneğin:

```sql
SELECT *
FROM LG_102_01_INVOICE
WHERE FICHENO = '...';
```

sonucu doğrulanmadan güncelleme yapılmamalıdır.

## Kural 4 — Test Modu Tasarla

Özel bakım prosedürleri mümkünse:

```text
@TestModu = 1
```

benzeri bir mekanizma içermelidir.

Test modu değişiklik yapmadan etkilenecek kayıtları göstermelidir.

## Kural 5 — Transaction Kullan

Birden fazla tabloyu doğrudan SQL ile değiştiren kontrollü bakım işlemleri transaction altında çalışmalıdır.

## Kural 6 — Önce Yedek

Özellikle toplu veri düzeltmelerinde geri dönüş planı olmadan işlem yapılmamalıdır.

## Kural 7 — LOGICALREF İlişkilerini Takip Et

Bir kaydın bağlı hareketlerini bulmanın en güvenilir yollarından biri referans zincirini takip etmektir.

---

# 23. Sık Yapılan Hatalar

## Hata 1 — Logo Veritabanını Sıradan ERP Veritabanı Gibi Görmek

Sonuç:

```text
Eksik hareketler
Yetim kayıtlar
Maliyet tutarsızlığı
Seri/Lot uyuşmazlığı
```

## Hata 2 — Sadece Görünen Tabloyu Güncellemek

Örneğin faturanın yalnızca `INVOICE.DATE_` alanını değiştirmek.

Bağlı:

```text
STFICHE
STLINE
CLFLINE
EMFICHE
EMFLINE
```

kayıtları eski tarihte kalabilir.

## Hata 3 — TRCODE Değerlerini Genellemek

Aynı sayı farklı tablolarda farklı işleme karşılık gelebilir.

## Hata 4 — Seri/Lot'u Stoktan Bağımsız Görmek

Fiziksel stok doğru görünürken lot bazlı stok yanlış olabilir.

## Hata 5 — Üretim Entegrasyonunda Yalnızca Stok Fişi Oluşturmak

Üretim emri, operasyon, sarf, mamül girişi, kalite ve maliyet ilişkileri eksik kalabilir.

## Hata 6 — Logo Objects Hatalarını Loglamamak

`Post()` başarısız olduğunda yalnızca `false` kontrol etmek yeterli değildir. Logo'nun döndürdüğü hata kodu ve açıklama kayıt altına alınmalıdır.

---

# 24. Önerilen Geliştirici Yaklaşımı

Bir Logo geliştirmesine başlamadan önce aşağıdaki sıra izlenebilir.

```text
1. İş sürecini Logo ekranında manuel olarak çalıştır.
2. Oluşan kayıtları SQL üzerinden incele.
3. LOGICALREF ilişkilerini çıkar.
4. Kullanılacak Logo Objects veri tipini belirle.
5. IData ile temel prototipi oluştur.
6. Hata mesajlarını incele.
7. Başarılı kayıt sonrası tabloları tekrar karşılaştır.
8. Seri/Lot, muhasebe ve maliyet etkilerini doğrula.
9. Logging ve idempotency ekle.
10. Son olarak performans optimizasyonu yap.
```

Bu yöntem, yalnızca tablo bilgisine bakarak entegrasyon geliştirmekten çok daha güvenlidir.

---

# 25. Mimari Karar Matrisi

| İhtiyaç | Önerilen Yöntem |
|---|---|
| Malzeme listeleme | SQL / IQuery |
| Cari listeleme | SQL / IQuery |
| Raporlama | SQL |
| Power BI veri kaynağı | SQL View / Stored Procedure |
| Malzeme kartı oluşturma | IData |
| Cari kart oluşturma | IData |
| Fatura oluşturma | IData |
| İrsaliye oluşturma | IData |
| Stok fişi oluşturma | IData |
| Kayıt güncelleme | Öncelikle IData |
| Kayıt silme | Öncelikle IData |
| Seri/Lot hareketi | Logo Objects iş nesneleri |
| Üretim entegrasyonu | ProductionApplication / Logo Objects |
| Kontrol sorguları | SQL / IQuery |
| Toplu tarih düzeltme gibi bakım operasyonu | Kontrollü SQL + transaction + test modu |

Bu tablo mutlak bir kural değildir; güvenli başlangıç noktasıdır.

---

# 26. Kitap Boyunca Kullanılacak Temel Terminoloji

| Terim | Anlam |
|---|---|
| Logo ERP | Logo Tiger / Tiger Wings Enterprise uygulama katmanı |
| Logo Objects | Logo ERP iş nesnelerine programatik erişim katmanı |
| IApplication | Logo Objects ana uygulama / oturum nesnesi |
| IData | Kart ve fiş işlemlerinin temel veri nesnesi |
| IQuery | SQL sorgu çalıştırma arayüzü |
| LOGICALREF | Logo kayıtlarının temel referans anahtarı |
| CompanyId | Aktif firma bilgisi |
| Period | Aktif dönem bilgisi |
| TRCODE | İşlem türünü belirleyen kod |
| Header | Fiş üst bilgisi |
| Lines | Fiş satırları |
| ProductionApplication | Detaylı üretim işlemleri için kullanılan Logo Objects tarafı |
| Seri/Lot | İzlenebilir stok alt kırılımı |
| Maliyetlendirme | Stok ve üretim hareketlerinden maliyet oluşturma süreci |

---

# 27. Bölüm Özeti

Logo ERP mimarisini anlamak için aşağıdaki beş prensip yeterince güçlü bir başlangıç sağlar:

1. **Logo ERP bir SQL veritabanından ibaret değildir.**
2. **Kart ve fiş işlemleri Logo iş kuralları ile birlikte ele alınmalıdır.**
3. **Resmî veri yazma işlemlerinde Logo Objects / IData birincil yöntemdir.**
4. **SQL; raporlama, analiz, kontrol ve kontrollü bakım işlemlerinde çok güçlüdür.**
5. **Üretim, seri/lot ve maliyet süreçlerinde ilişkiler mutlaka uçtan uca korunmalıdır.**

Sonraki bölümlerde bu mimarinin programatik tarafına geçilecek ve önce **Logo Objects mimarisi**, ardından `IApplication`, `IData`, `IQuery`, `DataFields`, `Lines` ve `ProductionApplication` detaylı olarak ele alınacaktır.
