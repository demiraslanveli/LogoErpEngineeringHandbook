# 33 — Logo Objects ile Fiş ve Fatura Oluşturma Örneği

## 1. Amaç

Bu bölüm satınalma/satış fişi veya faturası üretirken kullanılacak genel Logo Objects yaklaşımını açıklar. Buradaki amaç tek bir belge tipine bağımlı kod vermek değil; üst bilgi + satırlar + bağlı alanlar + `Post()` + hata kontrolü modelini standartlaştırmaktır.

> Not: `DataObjectType`, `TRCODE` ve bazı field isimleri ürün/sürüm bazında doğrulanmalıdır.

## 2. Genel Belge Akışı

```text
IApplication
    ↓
Login
    ↓
NewDataObject(BelgeTipi)
    ↓
New()
    ↓
Header DataFields
    ↓
Transactions / Lines
    ↓
Post()
    ↓
Error kontrolü
```

## 3. Örnek C# Kalıbı

```csharp
UnityApplication.IApplication app = new UnityApplication.UnityApplication();

if (!app.Login("LOGO_USER", "LOGO_PASSWORD", 40, 1))
    throw new Exception("Logo login başarısız.");

try
{
    UnityApplication.IData fiche =
        app.NewDataObject(UnityApplication.DataObjectType.doSalesInvoice);

    fiche.New();

    fiche.DataFields.FieldByName("TYPE").Value = 8;
    fiche.DataFields.FieldByName("NUMBER").Value = "~";
    fiche.DataFields.FieldByName("DATE").Value = DateTime.Today;
    fiche.DataFields.FieldByName("ARP_CODE").Value = "120.001";
    fiche.DataFields.FieldByName("SOURCE_WH").Value = 0;

    var lines = fiche.DataFields.FieldByName("TRANSACTIONS").Lines;

    lines.AppendLine();
    lines[lines.Count - 1].FieldByName("TYPE").Value = 0;
    lines[lines.Count - 1].FieldByName("MASTER_CODE").Value = "150.001";
    lines[lines.Count - 1].FieldByName("QUANTITY").Value = 10;
    lines[lines.Count - 1].FieldByName("PRICE").Value = 100;
    lines[lines.Count - 1].FieldByName("VAT_RATE").Value = 20;

    if (!fiche.Post())
        throw new Exception("Belge kaydedilemedi: " + fiche.ErrorDesc);
}
finally
{
    app.Disconnect();
}
```

Bu örnek kavramsal kalıptır. Field adları ve belge enum'u kullanılan Logo Objects sürümünde doğrulanmalıdır.

## 4. Üst Bilgi Alanları

Belge türüne göre tipik üst bilgi alanları:

```text
Belge tipi / TRCODE
Belge numarası
Tarih
Cari hesap
Ambar
İşyeri
Bölüm
Özel kod
Yetki kodu
Proje
Döviz bilgileri
Açıklamalar
```

## 5. Satır Alanları

Malzeme satırında tipik olarak:

```text
Satır tipi
Malzeme kodu
Miktar
Birim
Fiyat
KDV
Ambar
Proje
Açıklama
Sipariş bağlantısı
Seri/Lot bilgisi
```

alanları bulunabilir.

## 6. Satır Ekleme Mantığı

Logo Objects'te fiş/fatura satırları genellikle `Lines` koleksiyonu üzerinden yönetilir.

Genel kalıp:

```csharp
var lines = data.DataFields.FieldByName("TRANSACTIONS").Lines;
lines.AppendLine();
var line = lines[lines.Count - 1];
```

Ardından satır alanları doldurulur.

## 7. Birden Fazla Satır

```csharp
foreach (var row in model.Lines)
{
    lines.AppendLine();
    var line = lines[lines.Count - 1];

    line.FieldByName("MASTER_CODE").Value = row.ItemCode;
    line.FieldByName("QUANTITY").Value = row.Quantity;
    line.FieldByName("PRICE").Value = row.Price;
}
```

Bu model dış sistem → Logo entegrasyonlarında temel yaklaşımdır.

## 8. Sipariş Bağlantısı

Fatura veya irsaliye bir siparişten türetiliyorsa yalnızca aynı malzeme kodunu yazmak sipariş bağlantısı oluşturmaz.

Sipariş satırı referansı ve Objects'in beklediği bağlantı alanları doğru şekilde verilmelidir.

SQL tarafında kontrol edilen alanlardan biri `ORDTRANSREF` olabilir.

```sql
SELECT
    LOGICALREF,
    ORDFICHEREF,
    ORDTRANSREF,
    INVOICEREF,
    STFICHEREF
FROM LG_040_01_STLINE
WHERE INVOICEREF = @InvoiceRef;
```

## 9. Fatura ve İrsaliye İlişkisi

Fatura kaydedildiğinde kullanılan senaryoya göre bağlı irsaliye/stok fişi kayıtları oluşabilir veya mevcut irsaliye faturaya bağlanabilir.

Bu nedenle belge sonrası şu zincir doğrulanmalıdır:

```text
INVOICE
  ↓
STFICHE
  ↓
STLINE
  ↓
CLFLINE
  ↓
EMFICHE / EMFLINE (muhasebeleşmişse)
```

Her belge tipinde tüm katmanların oluşması zorunlu değildir; işlem senaryosuna göre kontrol edilmelidir.

## 10. KDV Muafiyet Alanları

KDV oranı 0 olan satırlarda iş kuralına göre muafiyet kodu ve açıklaması gerekebilir.

Kontrol mantığı:

```text
VAT = 0
AND malzeme satırı
AND muafiyet sebebi boş
→ kullanıcı uyarısı / otomatik doldurma
```

SQL doğrulama örneği:

```sql
SELECT
    LOGICALREF,
    STOCKREF,
    VAT,
    VATEXCEPTCODE,
    VATEXCEPTREASON
FROM LG_040_01_STLINE
WHERE
    INVOICEREF = @InvoiceRef
    AND LINETYPE = 0
    AND VAT = 0;
```

## 11. Döviz ve Fiyat

Dövizli belgelerde sadece `PRICE` alanını dikkate almak yeterli değildir.

Kontrol edilmesi gerekebilecek bilgiler:

```text
TRCURR
TRRATE
REPORTRATE
PRICE
TOTAL
UINFO1 / UINFO2
Birim
```

Özellikle son alış fiyatı kontrolünde birim ve kur normalizasyonu birlikte yapılmalıdır.

## 12. Post Sonrası Doğrulama

Başarılı `Post()` işleminden sonra yalnızca metodun `true` dönmesine güvenmek yerine kritik entegrasyonlarda SQL doğrulama yapılabilir.

Örnek:

```sql
SELECT
    LOGICALREF,
    FICHENO,
    DATE_,
    TRCODE,
    CLIENTREF
FROM LG_040_01_INVOICE
WHERE FICHENO = @FicheNo;
```

ve satırlar:

```sql
SELECT
    LOGICALREF,
    STOCKREF,
    AMOUNT,
    PRICE,
    VAT,
    ORDTRANSREF
FROM LG_040_01_STLINE
WHERE INVOICEREF = @InvoiceRef;
```

## 13. Duplicate Koruması

Dış sistemden gelen belge için benzersiz bir anahtar tutulmalıdır.

Örnek:

```text
SourceSystem = MES
ExternalDocumentId = 481552
```

İşlem öncesi:

```text
Bu dış belge daha önce işlendi mi?
```

kontrolü yapılmalıdır.

## 14. Hata Logu

Başarısız fiş/fatura logunda en az:

```text
Firma
Dönem
Belge tipi
Cari kodu
Dış belge ID
Belge tarihi
Satır sayısı
Logo ErrorDesc
Payload özeti
```

tutulmalıdır.

## 15. Test Modu

Kritik entegrasyonlarda üretim öncesi dry-run/test mantığı faydalıdır:

```text
1. Referansları çöz
2. Cari var mı?
3. Malzemeler var mı?
4. Birimler doğru mu?
5. Ambar geçerli mi?
6. Sipariş bağlantıları geçerli mi?
7. Seri/Lot gereksinimi var mı?
8. Ancak sonra Post()
```

## 16. Best Practice

- Belgeyi doğrudan SQL insert ile oluşturma.
- Header ve line field'larını ayrı doğrula.
- `Post()` hatasını mutlaka logla.
- Sipariş bağlantısını malzeme kodundan türetme; gerçek satır referansını kullan.
- Birim, döviz ve KDV bilgisini birlikte doğrula.
- Seri/lot kullanılan malzemelerde lot detayını zorunlu kontrol et.
- Duplicate belge koruması kullan.
- Post sonrası kritik bağlantıları SQL ile read-only doğrula.

## 17. Özet

Logo Objects ile fiş/fatura entegrasyonunda en önemli konu belgeyi kaydetmek değil, Logo'nun iş kurallarına uygun tam bir belge zinciri oluşturmaktır. Üst bilgi, satırlar, sipariş ilişkileri, ambar, birim, KDV, seri/lot ve hata yönetimi tek bütün olarak ele alınmalıdır.
