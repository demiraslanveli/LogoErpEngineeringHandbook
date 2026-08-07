# 23 — IData Gerçek Kullanım Örnekleri

## 1. Amaç

Bu bölüm, `IData` nesnesinin yalnızca teorik yapısını değil, sahada kullanılan temel CRUD ve fiş işlem kalıplarını gösterir.

Ana prensip:

> Logo kart ve fişlerinde veri yazma işlemleri mümkün olduğunca `IData` üzerinden yapılmalıdır.

Doğrudan SQL ile `INSERT`, `UPDATE` veya `DELETE` yapmak, Logo'nun iş kurallarını ve bağlı kayıt üretim mekanizmalarını atlayabilir.

---

## 2. IData Oluşturma

`IData`, `IApplication.NewDataObject(...)` ile oluşturulur.

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);
```

Akış:

```text
IApplication
    ↓
NewDataObject
    ↓
IData
    ↓
New / Read / Delete
    ↓
Post
```

---

## 3. Yeni Kart Oluşturma

Genel kart ekleme kalıbı:

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);

data.New();

data.DataFields.FieldByName("CODE").Value = "TEST.001";
data.DataFields.FieldByName("NAME").Value = "Test Kartı";

if (!data.Post())
{
    throw new Exception(data.ErrorDesc);
}
```

Burada `Post()` gerçek kayıt işlemidir.

`New()` yalnızca nesneyi yeni kayıt moduna hazırlar.

---

## 4. Mevcut Kaydı Okuma

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);

if (data.Read(logicalRef))
{
    string code = Convert.ToString(
        data.DataFields.FieldByName("CODE").Value
    );
}
```

`Read(...)` işleminde kullanılan referans çoğunlukla kaydın `LOGICALREF` değeridir.

---

## 5. Kayıt Güncelleme

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);

if (!data.Read(logicalRef))
    throw new Exception("Kayıt bulunamadı.");

data.DataFields.FieldByName("NAME").Value = "Yeni Açıklama";

if (!data.Post())
{
    throw new Exception(data.ErrorDesc);
}
```

Bu yöntem Logo'nun ilgili nesne validasyonlarını çalıştırır.

---

## 6. Kayıt Silme

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);

if (!data.Read(logicalRef))
    throw new Exception("Kayıt bulunamadı.");

if (!data.Delete())
{
    throw new Exception(data.ErrorDesc);
}
```

Silme işlemi özellikle fişlerde doğrudan SQL'e bırakılmamalıdır.

Bağlantılı kayıtlar olabilir:

- stok satırları,
- cari hareketler,
- muhasebe kayıtları,
- seri/lot dağıtımları,
- sipariş bağlantıları,
- üretim bağlantıları.

---

## 7. Fiş Üst Bilgisi Yazma

Fişlerde önce üst bilgi doldurulur.

```csharp
IData fiche = App.NewDataObject(DataObjectType.xxx);

fiche.New();

fiche.DataFields.FieldByName("NUMBER").Value = "~";
fiche.DataFields.FieldByName("DATE").Value = DateTime.Today;
fiche.DataFields.FieldByName("ARP_CODE").Value = "CARI.001";
```

Alan isimleri nesne tipine göre değişebilir.

Bu nedenle üretim ortamında kullanılan Logo Objects sürümündeki gerçek alan adları kontrol edilmelidir.

---

## 8. Satır Ekleme Mantığı

Fişlerin çoğunda satırlar bir `Lines` koleksiyonu üzerinden yönetilir.

Temel mantık:

```csharp
ILines lines = fiche.DataFields.FieldByName("TRANSACTIONS").Lines;

lines.AppendLine();

lines[lines.Count - 1].FieldByName("TYPE").Value = 0;
lines[lines.Count - 1].FieldByName("MASTER_CODE").Value = "150.001";
lines[lines.Count - 1].FieldByName("QUANTITY").Value = 10;
lines[lines.Count - 1].FieldByName("PRICE").Value = 25.50;
```

Kritik nokta:

> Satır eklemek yalnızca koleksiyona eleman eklemek değildir; satır türü, malzeme, miktar, birim, fiyat, ambar ve gerekiyorsa seri/lot bilgilerinin birlikte doğru oluşturulması gerekir.

---

## 9. Çok Satırlı Belge

```csharp
foreach (var item in items)
{
    lines.AppendLine();

    int index = lines.Count - 1;

    lines[index].FieldByName("TYPE").Value = 0;
    lines[index].FieldByName("MASTER_CODE").Value = item.Code;
    lines[index].FieldByName("QUANTITY").Value = item.Quantity;
    lines[index].FieldByName("PRICE").Value = item.Price;
}
```

Burada her satır için bağımsız doğrulama yapılması önerilir.

Örneğin:

```text
Malzeme kodu var mı?
Birim geçerli mi?
Miktar > 0 mı?
Ambar doğru mu?
Fiyat formatı doğru mu?
Seri/Lot zorunlu mu?
```

---

## 10. Satır Okuma

```csharp
ILines lines = fiche.DataFields.FieldByName("TRANSACTIONS").Lines;

for (int i = 0; i < lines.Count; i++)
{
    string code = Convert.ToString(
        lines[i].FieldByName("MASTER_CODE").Value
    );

    double quantity = Convert.ToDouble(
        lines[i].FieldByName("QUANTITY").Value
    );
}
```

---

## 11. Satır Silme

Logo Objects sürümündeki `ILines` API'sine göre satır silme yöntemi değişebilir.

Genel yaklaşım:

```text
İlgili satırı bul
    ↓
Satır koleksiyonundan kaldır
    ↓
Post()
```

Satır silme işleminden önce bağlı dağıtımlar ve referanslar kontrol edilmelidir.

---

## 12. Post Sonrası Kontrol

Kötü yaklaşım:

```csharp
fiche.Post();
```

ve sonucu kontrol etmemek.

Doğru yaklaşım:

```csharp
if (!fiche.Post())
{
    string message = fiche.ErrorDesc;
    throw new Exception(message);
}
```

Logo Objects validasyon hatalarının mutlaka loglanması gerekir.

---

## 13. Hata Yönetimi

Entegrasyonlarda yalnızca `ErrorDesc` göstermek yeterli olmayabilir.

Log kaydında şu bilgiler tutulmalıdır:

```text
Tarih/Saat
Firma
Dönem
Nesne Tipi
İşlem Türü
Belge/Kart Kodu
LOGICALREF
ErrorCode
ErrorDesc
Validasyon Detayları
Çağıran Uygulama
Kullanıcı
```

---

## 14. Transaction Yaklaşımı

Bir entegrasyon birden fazla bağımlı Logo kaydı oluşturuyorsa işlem bütünlüğü önemlidir.

Örneğin:

```text
Önce kart oluştur
Sonra sipariş oluştur
Sonra özel entegrasyon tablosunu güncelle
```

İkinci adım başarısız olduğunda ilk adımın sistemde kalması istenmeyebilir.

Bu nedenle süreç seviyesinde transaction/rollback stratejisi tasarlanmalıdır.

---

## 15. Idempotency

Entegrasyonlarda aynı isteğin iki kez gönderilmesi mümkündür.

Örneğin servis timeout olur:

```text
İstek Logo'ya ulaştı
Logo kaydı oluşturdu
Cevap istemciye ulaşmadı
İstemci tekrar gönderdi
```

Sonuç çift kayıt olabilir.

Bu nedenle entegrasyon tarafında benzersiz dış sistem anahtarı tutulmalıdır.

Örnek:

```text
ExternalTransactionId
SourceSystem
SourceDocumentNo
```

Yeni kayıt öncesi kontrol:

```text
Bu dış sistem kaydı daha önce işlendi mi?
```

---

## 16. Malzeme Kartı Güncelleme Senaryosu

Gerçek uygulamalarda malzeme kartında şu alanlar birlikte yönetilebilir:

```text
Malzeme Kodu
Malzeme Açıklaması
Grup Kodu
Özel Kodlar
Birim Seti
Birim Çevrimleri
Barkodlar
```

Burada malzeme ana kartı dışında:

```text
ITEMS
ITMUNITA
UNITSETL
UNITBARCODE
```

ilişkileri bulunur.

Bu nedenle tek bir `UPDATE LG_XXX_ITEMS` işlemi işin tamamı değildir.

---

## 17. Barkod Duplicate Kontrolü

Yeni barkod eklenmeden önce barkodun başka bir malzemede kullanılıp kullanılmadığı kontrol edilmelidir.

SQL yalnızca kontrol amacıyla kullanılabilir:

```sql
SELECT
    ITEMREF,
    UNITLINEREF,
    BARCODE
FROM LG_040_UNITBARCODE
WHERE BARCODE = @Barcode;
```

Kayıt işlemi mümkünse Logo nesne mekanizması üzerinden yapılmalıdır.

---

## 18. Siparişten Fişe Bağlantı

Bir siparişten irsaliye veya fatura üretildiğinde Logo ilişki alanlarını oluşturur.

SQL tarafında sık görülen bağlantılardan biri:

```text
STLINE.ORDTRANSREF
```

Bu referans ilgili sipariş satırının `LOGICALREF` değerini işaret edebilir.

Bu nedenle doğrudan fiş satırı insert etmek, sipariş kapanma ve sevk miktarı hesaplarını bozabilir.

---

## 19. Seri/Lot Gerektiren Satırlar

Seri/lot takipli malzemelerde yalnızca:

```text
MASTER_CODE
QUANTITY
```

bilgilerini vermek yeterli değildir.

İşlemde ayrıca dağıtım bilgilerinin oluşturulması gerekir.

Kontrol:

```text
Satır miktarı = seri/lot dağıtım toplamı
```

olmalıdır.

---

## 20. KDV Muafiyet Bilgileri

Satış veya satınalma fişlerinde KDV oranı sıfır olan satırlarda, iş sürecine bağlı olarak KDV muafiyet kodu ve açıklaması gerekebilir.

Örnek kontrol mantığı:

```text
VAT = 0
ve
Muafiyet kodu boş
    ↓
Kullanıcıyı uyar
veya
Tanımlı varsayılan açıklamayı ata
```

Bu tür iş kuralları uygulama tarafında kayıt öncesi doğrulanmalıdır.

---

## 21. Test Modu Tasarımı

Toplu işlem yapan uygulamalarda doğrudan `Post()` çağırmak yerine test modu kullanılabilir.

```text
TestMode = true
    ↓
Alanları hazırla
Validasyon yap
SQL kontrol sorgularını çalıştır
Ama Post etme
```

Gerçek mod:

```text
TestMode = false
    ↓
Post()
```

Bu yaklaşım özellikle veri düzeltme ve toplu aktarım araçlarında faydalıdır.

---

## 22. CRUD Servis Katmanı Örneği

```csharp
public class LogoMaterialService
{
    private readonly IApplication _app;

    public LogoMaterialService(IApplication app)
    {
        _app = app;
    }

    public void UpdateName(int logicalRef, string newName)
    {
        IData data = _app.NewDataObject(DataObjectType.xxx);

        if (!data.Read(logicalRef))
            throw new Exception("Malzeme bulunamadı.");

        data.DataFields.FieldByName("NAME").Value = newName;

        if (!data.Post())
            throw new Exception(data.ErrorDesc);
    }
}
```

Bu yapı sayesinde UI kodu Logo Objects detaylarından ayrılır.

---

## 23. Servis Katmanı Neden Önemli?

Kötü mimari:

```text
ButtonClick
    ↓
NewDataObject
    ↓
50 alan doldur
    ↓
Post
```

Daha iyi mimari:

```text
UI
    ↓
Application Service
    ↓
Logo Integration Service
    ↓
IData
```

Böylece:

- kod tekrarları azalır,
- hata yönetimi merkezileşir,
- loglama standartlaşır,
- Logo sürüm geçişleri daha kolay olur.

---

## 24. IData ile SQL'in Birlikte Kullanımı

En sağlıklı yaklaşım çoğu projede hibrittir.

### SQL

Okuma, analiz, raporlama, duplicate kontrolü ve performans gerektiren sorgular.

### IData

Kart/fiş ekleme, güncelleme ve silme gibi Logo iş kuralı çalıştırması gereken işlemler.

Örnek:

```text
SQL ile malzemeyi bul
    ↓
LOGICALREF al
    ↓
IData.Read(LOGICALREF)
    ↓
Alanı değiştir
    ↓
Post()
```

---

## 25. Sonuç

`IData`, Logo Objects tarafındaki temel veri işlem nesnesidir.

En güvenli genel kural:

> SQL'i veriyi anlamak ve bulmak için; `IData`'yı Logo'nun resmi verisini değiştirmek için kullan.
