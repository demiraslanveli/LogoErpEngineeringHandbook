# 05 — IQuery

## 1. IQuery Nedir?

`IQuery`, Logo Objects içerisinde SQL sorgularını çalıştırmak ve sonuçlarını programatik olarak okumak için kullanılan sorgu arayüzüdür.

Logo Objects mimarisinde `IData` kart ve fiş gibi ERP nesnelerinin iş kurallarıyla birlikte yönetilmesini sağlarken, `IQuery` daha çok özel veri okuma, kontrol ve raporlama senaryolarında kullanılır.

Temel ayrım:

```text
IData  -> ERP nesnesini yönet
IQuery -> SQL sorgusu çalıştır
```

Bu iki nesne rakip değildir. Gerçek projelerde çoğu zaman birlikte kullanılır.

---

## 2. IQuery Ne İçin Kullanılır?

`IQuery` özellikle şu senaryolarda faydalıdır:

- Özel `SELECT` sorguları,
- Referans (`LOGICALREF`) bulma,
- Kart varlık kontrolü,
- Entegrasyon öncesi doğrulama,
- Raporlama,
- Kontrol ve mutabakat sorguları,
- Logo Objects veri nesnesinde doğrudan sunulmayan yardımcı verileri okuma,
- Özel view veya fonksiyonlardan veri çekme.

Örnek:

```text
Malzeme kodu geldi
      |
      v
IQuery ile LG_XXX_ITEMS kontrolü
      |
      v
LOGICALREF / özellikleri bul
      |
      v
IData ile fiş oluştur
```

---

## 3. IQuery Oluşturma Mantığı

`IQuery` nesnesi `IApplication` bağlamında oluşturulur.

Kavramsal akış:

```text
IApplication
    |
    v
CreateQuery / Query nesnesi oluştur
    |
    v
SQL metnini ata
    |
    v
ExecuteDirect
    |
    v
First / Next ile sonuçları dolaş
```

Logo Objects sürümüne bağlı olarak metod imzaları değişebileceğinden gerçek API referansı ilgili kurulum üzerinde doğrulanmalıdır.

---

## 4. Temel Kullanım Örneği

Logo uygulama geliştirme tarafında sık kullanılan temel desen şu şekildedir:

```text
CreateQuery(qry)
qry.SetSqlText(sql)
qry.ExecuteDirect()

if qry.First() then
    qry.GetFieldValue(...)
end if

qry.Clear()
```

Bu desen özellikle Logo form özelleştirmelerinde ve script tabanlı geliştirmelerde sık görülür.

Kavramsal C# karşılığı:

```csharp
var query = CreateLogoQuery();
query.SetSqlText(sql);
query.ExecuteDirect();

if (query.First())
{
    // alan değerlerini oku
}

query.Clear();
```

---

## 5. SetSqlText

`SetSqlText`, çalıştırılacak SQL ifadesinin sorgu nesnesine verilmesini sağlar.

Örnek:

```sql
SELECT LOGICALREF, CODE, NAME
FROM LG_040_ITEMS
WHERE CODE = '150.001'
```

Kavramsal kullanım:

```text
qry.SetSqlText(SqlText)
```

SQL metni oluştururken string birleştirme yapılacaksa veri tipi ve SQL injection riski dikkate alınmalıdır.

---

## 6. ExecuteDirect

SQL metni belirlendikten sonra sorgu çalıştırılır.

Örnek desen:

```text
qry.SetSqlText(SqlUser)
qry.ExecuteDirect()
```

Sorgunun başarılı çalışması ile sonuç satırı dönmesi aynı şey değildir.

Örneğin:

```sql
SELECT LOGICALREF
FROM LG_040_ITEMS
WHERE CODE = 'OLMAYAN_KOD'
```

teknik olarak başarıyla çalışabilir ancak hiç satır dönmeyebilir.

Bu nedenle `First()` kontrolü önemlidir.

---

## 7. First

`First()` sorgu sonucundaki ilk kayda konumlanmak için kullanılır.

Tipik desen:

```text
if qry.First() then
    // veri bulundu
else
    // veri bulunamadı
end if
```

Bu yapı tek kayıt beklenen lookup sorguları için oldukça uygundur.

Örneğin:

```sql
SELECT TRCODE
FROM LG_040_01_STFICHE
WHERE LOGICALREF = 12345
```

---

## 8. Next ile Çoklu Sonuç Okuma

Sorgu birden fazla kayıt döndürüyorsa sonuç kümesi satır satır dolaşılmalıdır.

Kavramsal akış:

```text
First()
   |
   v
Satırı oku
   |
   v
Next()
   |
   +--> kayıt var -> oku
   |
   +--> kayıt yok -> bitir
```

Örnek kullanım senaryoları:

- Malzeme listesi,
- Cari listesi,
- Fatura satırları,
- Seri/lot listesi,
- Rapor sonuçları.

---

## 9. GetFieldValue

`GetFieldValue`, sorgu sonucundaki alan değerlerini almak için kullanılır.

Logo scriptlerinde örnek yaklaşım:

```text
qry.GetFieldValue(1, 3, trCode)
```

Buradaki parametrelerin anlamı kullanılan Logo scripting/API sürümüne göre doğrulanmalıdır.

İndeks bazlı alan okurken kolon sırası değişirse kodun yanlış veri okuma riski vardır.

Bu nedenle SQL sorgusundaki kolon listesi açık yazılmalıdır.

Kötü örnek:

```sql
SELECT *
FROM LG_040_01_STFICHE
```

Daha doğru örnek:

```sql
SELECT LOGICALREF, FICHENO, TRCODE, DATE_
FROM LG_040_01_STFICHE
```

---

## 10. Neden SELECT * Kullanılmamalıdır?

Üretim kodunda `SELECT *` kullanımının çeşitli dezavantajları vardır:

- Gereksiz kolonlar okunur,
- Network yükü artar,
- Kolon sırasına bağlı kod kırılabilir,
- Kodun hangi alanlara ihtiyacı olduğu anlaşılmaz,
- Schema değişiklikleri beklenmedik sonuç yaratabilir.

Tercih:

```sql
SELECT
    LOGICALREF,
    CODE,
    NAME
FROM LG_040_ITEMS
WHERE CODE = '150.001';
```

---

## 11. Clear

Sorgu kullanıldıktan sonra `Clear()` çağrısı ile sorgu nesnesinin temizlenmesi sık kullanılan bir yaklaşımdır.

Örnek:

```text
qry.ExecuteDirect()

if qry.First() then
    ...
end if

qry.Clear()
```

Özellikle aynı sorgu nesnesi tekrar kullanılacaksa önceki sorgu durumunun temizlenmesi önemlidir.

---

## 12. Firma ve Dönem Dinamikliği

Logo tablo isimleri firma ve dönem numarasına göre değişebilir.

Örneğin:

```text
LG_040_01_STFICHE
LG_202_01_STFICHE
LG_803_01_STFICHE
```

Logo form scriptlerinde sık görülen yaklaşım firma numarasını dinamik üretmektir.

Örnek mantık:

```text
CompanyId = 40
      |
      v
"040"
      |
      v
LG_040_01_STFICHE
```

Firma numarasını 3 haneli formata dönüştürmek gerekir.

Örnekler:

```text
1   -> 001
40  -> 040
202 -> 202
```

---

## 13. Dinamik Tablo Adı Oluşturma

Örnek kavramsal script:

```text
comId = Application.CompanyId
str(comId, strID)
yeniID = "00" + strID
FrmNo = yeniID.SubStr(yeniID.size - 3, yeniID.size)
```

Ardından SQL:

```text
"SELECT ... FROM LG_" + FrmNo + "_01_STFICHE"
```

Bu yöntem firma bazlı çalışan form özelleştirmelerinde kullanışlıdır.

Ancak dönem `01` sabit yazılıyorsa uygulamanın farklı dönemlerde çalışıp çalışmayacağı ayrıca değerlendirilmelidir.

---

## 14. Dönem Numarasını Sabit Yazmanın Riski

Örnek:

```sql
LG_040_01_STLINE
```

Eğer uygulama yalnızca dönem `01` kullanılan firma yapısı için tasarlandıysa bu kabul edilebilir olabilir.

Ancak çok dönemli yapıda:

```text
01
02
03
```

gibi dönemler bulunabilir.

Bu durumda dönem de dinamik yönetilmelidir.

---

## 15. String Birleştirme ile SQL Oluşturma

Basit Logo scriptlerinde sorgular çoğunlukla string birleştirme ile oluşturulur.

Örnek:

```text
SqlUser = "SELECT TRCODE FROM LG_" + FrmNo +
          "_01_STFICHE WHERE LOGICALREF = " + LogrefStr
```

Bu yaklaşım sayısal `LOGICALREF` gibi kontrollü değerlerde pratik olabilir.

Ancak kullanıcıdan gelen string değerlerde dikkat edilmelidir.

Riskli örnek:

```text
"WHERE CODE = '" + UserInput + "'"
```

Kullanıcı girdisi güvenilir değilse SQL injection riski oluşur.

---

## 16. SQL Injection

Aşağıdaki sorgu düşünelim:

```text
SELECT * FROM LG_040_ITEMS
WHERE CODE = '<kullanıcı girdisi>'
```

Eğer değer doğrudan SQL stringine eklenirse kötü niyetli veri sorgunun yapısını değiştirebilir.

Logo script ortamında parametrik sorgu desteği sınırlıysa en azından:

- Girdi tipini kısıtla,
- Beklenen formatı doğrula,
- Tek tırnak karakterlerini kontrol et,
- Sayısal alanlarda yalnızca sayısal değer kabul et,
- Kullanıcı girdisiyle tablo adı oluşturma.

Kurumsal .NET uygulamalarında mümkün olan her yerde parametrik SQL tercih edilmelidir.

---

## 17. IQuery ile INSERT / UPDATE / DELETE

Logo Objects içerisinde SQL ifadeleri çalıştırılabildiği için teknik olarak `INSERT`, `UPDATE` ve `DELETE` komutları da çalıştırılabilir.

Ancak bu yetenek standart kart/fiş işlemleri için varsayılan yöntem olmamalıdır.

Temel prensip:

> **Logo kart ve fişleri üzerinde veri değişikliği gerekiyorsa öncelikle `IData` kullanılmalıdır.**

Doğrudan SQL değişikliği yalnızca ilgili Logo veri modelinin tüm sonuçları biliniyorsa ve kontrollü bir bakım senaryosu varsa değerlendirilmelidir.

---

## 18. IQuery ile IData Birlikte Kullanımı

En yaygın ve güçlü entegrasyon desenlerinden biridir.

Örnek satış faturası:

```text
1. IQuery
   |
   +--> Cariyi bul
   +--> Malzemeleri doğrula
   +--> Birimleri kontrol et
   +--> Duplicate kontrolü yap

2. IData
   |
   +--> Faturayı oluştur
   +--> Satırları ekle
   +--> Post

3. IQuery
   |
   +--> Sonuç mutabakatını yap
```

Bu yapı SQL'in güçlü okuma kabiliyeti ile Logo Objects'in veri bütünlüğünü birleştirir.

---

## 19. Lookup Sorguları

Entegrasyonlarda sık kullanılan lookup örnekleri:

### Malzeme LOGICALREF bulma

```sql
SELECT LOGICALREF
FROM LG_040_ITEMS
WHERE CODE = '150.001';
```

### Cari LOGICALREF bulma

```sql
SELECT LOGICALREF
FROM LG_040_CLCARD
WHERE CODE = 'CARI.001';
```

### Fiş türü kontrolü

```sql
SELECT TRCODE
FROM LG_040_01_STFICHE
WHERE LOGICALREF = 12345;
```

Bu sorguların sonucu daha sonraki Logo Objects kararlarında kullanılabilir.

---

## 20. EXISTS Kullanımı

Sadece kayıt var mı diye bakılacaksa tüm satırı getirmek gereksizdir.

Örnek:

```sql
IF EXISTS
(
    SELECT 1
    FROM LG_040_ITEMS
    WHERE CODE = '150.001'
)
    SELECT 1;
ELSE
    SELECT 0;
```

Alternatif:

```sql
SELECT TOP 1 LOGICALREF
FROM LG_040_ITEMS
WHERE CODE = '150.001';
```

Amaç sorgunun gereksiz veri taşımamasıdır.

---

## 21. TOP Kullanımı

Tek kayıt beklenen sorgularda `TOP 1` performans ve niyet açısından faydalı olabilir.

Örnek:

```sql
SELECT TOP 1
    LOGICALREF,
    PRICE
FROM LG_040_PRCLIST
WHERE CARDREF = 123
ORDER BY ENDDATE DESC;
```

Ancak `TOP 1` kullanırken mutlaka hangi kaydın seçileceğini belirleyen mantıklı bir `ORDER BY` düşünülmelidir.

Aksi halde dönen kaydın deterministik olduğu garanti değildir.

---

## 22. ORDER BY Olmadan Son Kayıt Bulma Hatası

Yanlış:

```sql
SELECT TOP 1 PRICE
FROM LG_040_01_STLINE
WHERE STOCKREF = 43338;
```

Bu sorgunun "son alış fiyatı" getirdiği garanti değildir.

Doğru yaklaşım işlem tarihini ve gerektiğinde teknik sıra alanlarını kullanmaktır.

Örneğin:

```sql
SELECT TOP 1
    PRICE,
    DATE_,
    LOGICALREF
FROM LG_040_01_STLINE
WHERE STOCKREF = 43338
  AND TRCODE IN (...)
ORDER BY DATE_ DESC, LOGICALREF DESC;
```

Gerçek TRCODE kapsamı iş ihtiyacına göre belirlenmelidir.

---

## 23. NULL Yönetimi

SQL sonuçlarında `NULL` değerler Logo scriptlerinde beklenmeyen davranışlara yol açabilir.

Örnek:

```sql
SELECT ISNULL(SPECODE, '') AS SPECODE
FROM LG_040_ITEMS
WHERE LOGICALREF = 100;
```

Ancak her `NULL` değerini rastgele boş stringe çevirmek doğru değildir.

Örneğin sayısal alanlarda `NULL` ile `0` farklı anlam taşıyabilir.

İş anlamı korunmalıdır.

---

## 24. Veri Tipleri

`GetFieldValue` ile okunan veri tipleri doğru hedef değişkene aktarılmalıdır.

Örnekler:

```text
LOGICALREF -> Integer
PRICE      -> Decimal / Double
DATE_      -> Date
CODE       -> String
```

Yanlış veri tipi kullanılması;

- dönüşüm hatası,
- yuvarlama,
- tarih formatı problemi,
- karşılaştırma hatası

oluşturabilir.

---

## 25. Tarih Sorguları

Tarihleri string birleştirme ile SQL'e vermek bölgesel format problemleri doğurabilir.

Riskli:

```sql
WHERE DATE_ = '08.07.2026'
```

SQL Server ortamında güvenli formatlardan biri:

```sql
WHERE DATE_ = '20260708'
```

veya parametrik sorgu kullanmaktır.

Kullanıcı arayüzündeki `08.07.2026` değeri ile SQL tarih literal'i aynı şey değildir.

---

## 26. Decimal Ayracı Problemi

Türkiye bölgesel ayarlarında ondalık ayıracı virgül olabilir:

```text
0,487584
```

SQL Server literal'inde ise genellikle nokta kullanılır:

```text
0.487584
```

String birleştirmeyle decimal değer göndermek bölgesel ayarlardan etkilenebilir.

Bu nedenle mümkün olduğunda parametrik sorgu veya invariant culture yaklaşımı kullanılmalıdır.

---

## 27. Performans: Her Satırda Sorgu Çalıştırma

Örneğin 10.000 satırlık aktarım düşünelim.

Kötü yaklaşım:

```text
Her satır için:
   SELECT LOGICALREF FROM ITEMS
```

Sonuç:

```text
10.000 satır -> 10.000 ayrı query
```

Daha iyi yaklaşım:

- İhtiyaç duyulan kartları toplu sorgula,
- Dictionary/cache oluştur,
- Satırlar içinde memory lookup kullan.

Örnek:

```text
SELECT LOGICALREF, CODE
FROM LG_040_ITEMS
WHERE CODE IN (...)
```

veya tüm ilgili master data kontrollü cache'e alınabilir.

---

## 28. Sargable Sorgular

Büyük Logo tablolarında indeks kullanımını engelleyen ifadelerden kaçınılmalıdır.

Riskli örnek:

```sql
WHERE YEAR(DATE_) = 2026
```

Daha iyi:

```sql
WHERE DATE_ >= '20260101'
  AND DATE_ <  '20270101'
```

Bu tür sorgular SQL Server'ın indeksleri daha etkin kullanmasına yardımcı olabilir.

---

## 29. CAST ve Fonksiyon Kullanımı

WHERE koşulunda indeksli kolon üzerinde gereksiz `CAST`, `CONVERT`, `LEFT`, `YEAR` gibi fonksiyonlar performansı düşürebilir.

Örneğin:

```sql
WHERE LEFT(CODE, 3) = '150'
```

bazı durumlarda indeks kullanımını sınırlar.

Alternatif:

```sql
WHERE CODE >= '150'
  AND CODE <  '151'
```

veya iş ihtiyacına göre:

```sql
WHERE CODE LIKE '150%'
```

İndeks yapısı ayrıca incelenmelidir.

---

## 30. Büyük Tablolarda Dikkat

Logo ERP'de aşağıdaki tablolar çok büyüyebilir:

```text
STLINE
CLFLINE
EMFLINE
INVOICE
STFICHE
```

Bu nedenle IQuery üzerinden yazılan özel sorgular üretim SQL Server performansını doğrudan etkileyebilir.

Özellikle:

- Tarih filtresiz sorgu,
- `SELECT *`,
- uygun join olmayan sorgu,
- indeks kullanmayan filtre,
- milyonlarca satırda sort

risklidir.

---

## 31. NOLOCK Kullanımı

Logo rapor sorgularında `WITH (NOLOCK)` sık görülebilir.

Ancak `NOLOCK` ücretsiz performans değildir.

Riskleri:

- Dirty read,
- Aynı satırı iki kez okuma,
- Bazı satırları atlama,
- Commit edilmemiş veriyi görme.

Bu nedenle finansal mutabakat veya kritik kontrol sorgularında bilinçsizce kullanılmamalıdır.

---

## 32. View Kullanımı

Karmaşık sorgular tekrar tekrar kullanılacaksa SQL Server tarafında view oluşturmak faydalı olabilir.

Örnek:

```text
IQuery
   |
   v
SELECT ... FROM BV_040_STOKLISTE
```

Avantajları:

- SQL mantığı merkezi olur,
- Logo scripti sadeleşir,
- Sorgu yönetimi kolaylaşır.

Ancak view performansı ayrıca test edilmelidir. View kullanmak sorguyu otomatik olarak hızlı yapmaz.

---

## 33. Stored Procedure Kullanımı

Karmaşık işlerde stored procedure daha uygun olabilir.

Örnek:

```sql
EXEC dbo.SP_LG10_LOD_ALIM_KONTROL_MailGonder
    @Alici = '...';
```

veya parametreli kontrol prosedürleri.

Avantajları:

- İş mantığı merkezi olur,
- Parametre kullanımı kolaylaşır,
- Yetkilendirme yapılabilir,
- Versiyonlama kolaylaşır.

Ancak Logo form scriptinden procedure çağrısının desteklenme şekli kullanılan ortamda doğrulanmalıdır.

---

## 34. Güvenli Güncelleme Prosedürü Yaklaşımı

Doğrudan SQL güncellemesi zorunluysa kontrollü procedure yaklaşımı kullanılabilir.

Örnek özellikler:

```text
@TestModu = 1 -> sadece etkilenecek kayıtları göster
@TestModu = 0 -> update uygula
```

İyi bakım prosedürü:

- Firma parametresi alır,
- Tarihi açıkça alır,
- Hedef kayıt listesini alır,
- Ön kontrol yapar,
- Transaction kullanır,
- Hangi tabloların etkilendiğini raporlar,
- Bulunmayan kayıtta diğerlerine devam edebilir,
- Sonuç özeti verir.

Bu tür işlemler normal `IData` entegrasyonunun alternatifi değil, kontrollü bakım araçlarıdır.

---

## 35. IQuery ile Debug

Logo özelleştirmelerinde IQuery hata araştırmak için çok etkilidir.

Örneğin formdaki seçili fişin `LOGICALREF` değerini aldıktan sonra:

```sql
SELECT TRCODE, FICHENO, DATE_, SOURCEINDEX
FROM LG_040_01_STFICHE
WHERE LOGICALREF = @Logref;
```

ve satırlar:

```sql
SELECT LOGICALREF, STOCKREF, AMOUNT, PRICE, SOURCEINDEX
FROM LG_040_01_STLINE
WHERE STFICHEREF = @Logref;
```

Bu sorgular form davranışının arka plandaki Logo verisiyle eşleştirilmesini sağlar.

---

## 36. Sorgu Sonucunu Warn ile Gösterme

Logo form scriptlerinde debug sırasında sorgudan gelen değer geçici olarak kullanıcıya gösterilebilir.

Örnek:

```text
Warn(trCode)
```

Bu yöntem geliştirme aşamasında hızlı kontrol için faydalıdır.

Ancak üretim kodunda gereksiz `Warn` çağrıları kullanıcı deneyimini bozabilir.

Üretimde log mekanizması tercih edilmelidir.

---

## 37. Query Nesnesini Yeniden Kullanma

Aynı query değişkeni farklı sorgularda kullanılacaksa aradaki state doğru temizlenmelidir.

Örnek:

```text
qry.SetSqlText(Sql1)
qry.ExecuteDirect()
...
qry.Clear()

qry.SetSqlText(Sql2)
qry.ExecuteDirect()
...
qry.Clear()
```

Her sorgu için temiz bir yaşam döngüsü hata riskini azaltır.

---

## 38. Sık Yapılan Hatalar

### Hata 1 — `First()` kontrol etmeden veri okumak

Sorgu sıfır satır döndürebilir.

### Hata 2 — `SELECT *` kullanmak

Kolon sırası ve gereksiz veri sorunu oluşturur.

### Hata 3 — String alanları doğrudan SQL'e birleştirmek

SQL injection ve tek tırnak problemleri doğurabilir.

### Hata 4 — Tarihi bölgesel formatla göndermek

Sunucu ayarına bağlı hata oluşabilir.

### Hata 5 — Firma/dönemi yanlış tablo adına yazmak

Yanlış şirket verisi okunabilir.

### Hata 6 — `TOP 1` kullanıp `ORDER BY` yazmamak

Yanlış kayıt seçilebilir.

### Hata 7 — Her satırda ayrı query çalıştırmak

Toplu işlemlerde ciddi performans kaybı oluşur.

### Hata 8 — IQuery'yi kart/fiş CRUD için ana yöntem yapmak

Logo iş kuralları atlanabilir.

---

## 39. IQuery Best Practices

1. IQuery'yi öncelikle okuma ve kontrol amacıyla kullan.
2. ERP kart/fiş değişikliklerinde `IData` tercih et.
3. `SELECT *` kullanma.
4. `First()` sonucunu kontrol et.
5. Tek kayıt için gerekirse `TOP 1` ve deterministik `ORDER BY` kullan.
6. Firma ve dönem tablo adlarını bilinçli oluştur.
7. Kullanıcı girdisini doğrudan SQL stringine ekleme.
8. Tarih ve decimal değerlerde kültür ayarlarına dikkat et.
9. Büyük tablolarda filtreleri sargable yaz.
10. Batch işlemlerde her satır için lookup sorgusu çalıştırma.
11. `NOLOCK` kullanımının tutarlılık riskini bil.
12. Karmaşık sorguları view/procedure katmanına taşımayı değerlendir.
13. Query nesnesini işlem sonunda temizle.
14. SQL Server execution plan ve indekslerini gerektiğinde incele.

---

## 40. Örnek: Seçili Stok Fişinin TRCODE Değerini Okuma

Logo form scriptindeki tipik bir senaryo:

```text
Popup menüden işlem seçildi
       |
       v
Grid'deki seçili kaydın LOGICALREF'i alındı
       |
       v
Firma numarası hesaplandı
       |
       v
STFICHE sorgulandı
       |
       v
TRCODE okundu
```

Örnek script deseni:

```text
comId = Application.CompanyId
str(comId, strID)
yeniID = "00" + strID
FrmNo = yeniID.SubStr(yeniID.size - 3, yeniID.size)

DBGGetRecAdr("StFicheDataGrid", Logref)
Str(Logref, LogrefStr)

SqlUser = "SELECT TRCODE " +
          "FROM LG_" + FrmNo + "_01_STFICHE " +
          "WHERE LOGICALREF = " + LogrefStr

CreateQuery(qry)
qry.SetSqlText(SqlUser)
qry.ExecuteDirect()

if qry.First() then
    qry.GetFieldValue(1, 3, trCode)
end if

qry.Clear()
```

Bu örnek, `IQuery` kullanımının Logo form özelleştirmelerindeki temel mantığını gösterir.

---

## 41. Bölüm Özeti

`IQuery`, Logo Objects ekosisteminin SQL erişim aracıdır.

```text
IApplication
    |
    v
IQuery
    |
    +--> SetSqlText
    +--> ExecuteDirect
    +--> First
    +--> Next
    +--> GetFieldValue
    +--> Clear
```

Doğru kullanım alanı:

```text
SELECT
Lookup
Kontrol
Raporlama
Debug
Mutabakat
```

Temel mimari kural:

> **IQuery ile bilgiyi bul ve doğrula; Logo kart ve fişlerini değiştirmek için mümkün olduğunca IData kullan.**

Sonraki bölümde `DataFields` ve `Lines` yapıları ayrıntılı biçimde ele alınacaktır.
