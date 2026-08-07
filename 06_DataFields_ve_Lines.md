# 06 — DataFields ve Lines

## 1. Bölümün Amacı

Bu bölüm, Logo Objects ile kart ve fiş nesneleri üzerinde çalışırken en sık kullanılan iki temel yapıyı açıklar:

- `DataFields`
- `Lines`

`IData` nesnesi bir kartı veya fişi temsil ederken, `DataFields` üst bilgi alanlarına erişimi; `Lines` ise fişin satırlarını yönetmeyi sağlar.

Logo Objects ile güvenli veri üretmenin ana fikri şudur:

> Üst bilgiler `DataFields`, satır hareketleri `Lines` üzerinden yönetilir; kayıt işlemi ise `IData.Post()` ile Logo iş kuralları çalıştırılarak tamamlanır.

---

## 2. DataFields Nedir?

`DataFields`, aktif `IData` nesnesinin alan koleksiyonudur.

Örneğin bir malzeme kartında:

- Kod
- Açıklama
- Özel kod
- Yetki kodu
- Birim seti

birer `DataField` olarak ele alınır.

Bir satış faturasında ise:

- Fiş numarası
- Tarih
- Cari hesap
- Belge numarası
- Proje
- Açıklamalar

üst bilgi alanları yine `DataFields` üzerinden yönetilir.

Genel kullanım mantığı şöyledir:

```csharp
IData data = application.NewDataObject(DataObjectType.doSalesInvoice);

data.New();

data.DataFields.FieldByName("NUMBER").Value = "SF202600000001";
data.DataFields.FieldByName("DATE").Value = DateTime.Today;
data.DataFields.FieldByName("ARP_CODE").Value = "120.01.001";
```

Buradaki alan adları, kullanılan veri nesnesinin XML şemasında tanımlı alan isimleridir.

---

## 3. FieldByName Kullanımı

Pratikte en okunabilir erişim yöntemi `FieldByName` kullanmaktır.

```csharp
data.DataFields.FieldByName("CODE").Value = "150.001";
data.DataFields.FieldByName("NAME").Value = "Örnek Malzeme";
```

Bu kullanımın avantajları:

- Kod okunabilir olur.
- Alan sırasına bağımlılık azalır.
- XML alan isimleriyle birebir çalışılır.
- Bakım kolaylaşır.

Ancak alan adı yanlış yazılırsa veya ilgili nesnede mevcut değilse çalışma zamanı hatası oluşabilir. Bu nedenle alan isimleri geliştirilen Logo sürümünün nesne şemasıyla doğrulanmalıdır.

---

## 4. DataField Value Tipleri

Logo Objects alanları farklı veri tipleri taşıyabilir.

Yaygın örnekler:

| Veri | Örnek |
|---|---|
| String | Kod, açıklama, belge no |
| Integer | TRCODE, ambar no, durum |
| Double / Decimal | Miktar, fiyat, oran |
| DateTime | Fiş tarihi, vade tarihi |
| Boolean benzeri sayısal alan | 0 / 1 değerleri |

Logo tarafındaki bazı alanlar SQL’de `SMALLINT`, `INT`, `FLOAT` veya tarih tipi olsa bile Objects katmanında farklı biçimde sunulabilir.

Bu nedenle SQL tablo tipini doğrudan Objects veri tipi varsayımı olarak kullanmak doğru değildir.

---

## 5. Lines Nedir?

`Lines`, bir `IData` nesnesinin detay satır koleksiyonudur.

Özellikle fişlerde kullanılır:

- Satınalma faturası
- Satış faturası
- İrsaliye
- Stok fişi
- Sipariş
- Üretim ilişkili belgeler

Örnek:

```csharp
ILines lines = data.DataFields.FieldByName("TRANSACTIONS").Lines;
```

Ardından yeni satır eklenir:

```csharp
lines.AppendLine();
```

ve aktif satırdaki alanlar doldurulur:

```csharp
lines[lines.Count - 1].FieldByName("TYPE").Value = 0;
lines[lines.Count - 1].FieldByName("MASTER_CODE").Value = "150.001";
lines[lines.Count - 1].FieldByName("QUANTITY").Value = 10;
lines[lines.Count - 1].FieldByName("PRICE").Value = 125.50;
```

---

## 6. Satır Eklerken En Önemli Kural

Yeni bir satır oluşturmak için yalnızca alanlara değer vermek yeterli değildir.

Önce satır koleksiyonuna yeni satır eklenmelidir:

```csharp
lines.AppendLine();
```

Daha sonra ilgili satır doldurulmalıdır.

Bu sıra önemlidir:

1. `AppendLine()`
2. Aktif/yeni satıra alan atamaları
3. Gerekiyorsa alt satırlar
4. Sonraki satır için tekrar `AppendLine()`
5. En sonda `IData.Post()`

---

## 7. Birden Fazla Satır Eklemek

Örnek:

```csharp
ILines lines = data.DataFields.FieldByName("TRANSACTIONS").Lines;

lines.AppendLine();
lines[0].FieldByName("TYPE").Value = 0;
lines[0].FieldByName("MASTER_CODE").Value = "150.001";
lines[0].FieldByName("QUANTITY").Value = 5;
lines[0].FieldByName("PRICE").Value = 100;

lines.AppendLine();
lines[1].FieldByName("TYPE").Value = 0;
lines[1].FieldByName("MASTER_CODE").Value = "150.002";
lines[1].FieldByName("QUANTITY").Value = 3;
lines[1].FieldByName("PRICE").Value = 250;
```

Satır indekslerini sabit kullanmak yerine son satırı referans almak çoğu zaman daha güvenlidir:

```csharp
lines.AppendLine();
var line = lines[lines.Count - 1];

line.FieldByName("MASTER_CODE").Value = itemCode;
line.FieldByName("QUANTITY").Value = quantity;
```

---

## 8. Lines İçinde TYPE Alanı

Logo fiş satırlarında `TYPE` alanı kritik öneme sahiptir.

Aynı satır koleksiyonu içinde farklı satır türleri bulunabilir. Örneğin:

- Malzeme
- Hizmet
- İndirim
- Masraf
- Promosyon
- Açıklama benzeri satırlar

Bu yüzden satırın türü doğru belirlenmeden yalnızca `MASTER_CODE` gönderilmesi güvenli değildir.

Logo ekranında aynı grid içinde görünen satırlar arka planda farklı iş kurallarına sahip olabilir.

---

## 9. Satır Silme

Mevcut bir fiş üzerinde düzenleme yapılırken satır silmek gerekebilir.

Genel yaklaşım:

1. `IData.Read(logicalRef)` ile kayıt okunur.
2. Satırlar kontrol edilir.
3. İlgili satır koleksiyondan silinir.
4. `Post()` ile kayıt tekrar işlenir.

Doğrudan SQL ile `STLINE` satırı silmek önerilmez.

Çünkü bağlı olarak:

- Seri/lot kayıtları
- Dağıtım detayları
- Sipariş bağlantıları
- Muhasebe ilişkileri
- Üretim bağlantıları
- Kampanya ve fiyat ilişkileri

etkilenebilir.

---

## 10. Mevcut Satırları Okuma

Bir kayıt okunduktan sonra satırlar dolaşılabilir:

```csharp
if (data.Read(logicalRef))
{
    ILines lines = data.DataFields.FieldByName("TRANSACTIONS").Lines;

    for (int i = 0; i < lines.Count; i++)
    {
        string itemCode = Convert.ToString(
            lines[i].FieldByName("MASTER_CODE").Value
        );

        double quantity = Convert.ToDouble(
            lines[i].FieldByName("QUANTITY").Value
        );
    }
}
```

Bu yöntem, SQL’den yalnızca satır okumaktan farklıdır. Çünkü Objects nesnesi Logo’nun belge modelini kullanır.

---

## 11. İç İçe Lines Yapıları

Logo Objects’te bazı alanların altında tekrar `Lines` koleksiyonları bulunabilir.

Özellikle:

- Seri/lot dağıtımları
- Stok yeri dağıtımları
- Ödeme planı detayları
- Dağıtım satırları
- Üretim detayları

ana satırın altında alt koleksiyonlar şeklinde temsil edilebilir.

Bu nedenle nesne yapısını yalnızca SQL tablo ilişkileri üzerinden anlamaya çalışmak eksik kalabilir.

---

## 12. XML Modelini Anlamak

Logo Objects veri modeli pratikte XML alan yapısıyla çok yakından ilişkilidir.

Bir `IData` nesnesi XML’e aktarıldığında:

- Üst bilgiler XML alanları,
- `TRANSACTIONS` gibi alanlar satır koleksiyonları,
- Satırların altındaki dağıtımlar alt koleksiyonlar

olarak görülebilir.

Bu nedenle yeni bir nesneyle çalışmaya başlarken en faydalı yöntemlerden biri örnek bir kaydı okuyup XML çıktısını incelemektir.

Bu yaklaşım sayesinde:

- Gerçek alan isimleri,
- Zorunlu alanlar,
- Satır koleksiyonları,
- Alt satırlar,
- Varsayılan değerler

daha hızlı anlaşılır.

---

## 13. Yeni Kayıt Örneği

Basitleştirilmiş bir fiş oluşturma akışı:

```csharp
IData invoice = application.NewDataObject(DataObjectType.doSalesInvoice);

invoice.New();

invoice.DataFields.FieldByName("DATE").Value = DateTime.Today;
invoice.DataFields.FieldByName("ARP_CODE").Value = "120.01.001";
invoice.DataFields.FieldByName("SOURCE_WH").Value = 0;

ILines lines = invoice.DataFields.FieldByName("TRANSACTIONS").Lines;

lines.AppendLine();
var line = lines[lines.Count - 1];

line.FieldByName("TYPE").Value = 0;
line.FieldByName("MASTER_CODE").Value = "150.001";
line.FieldByName("QUANTITY").Value = 2;
line.FieldByName("PRICE").Value = 100;

if (!invoice.Post())
{
    // ValidateErrors / ErrorDesc kontrol edilmelidir.
}
```

Kod örneği kavramsaldır. Kullanılan alanlar ve nesne tipi Logo ürün/sürümüne ve belge türüne göre doğrulanmalıdır.

---

## 14. Güncelleme Örneği

```csharp
IData invoice = application.NewDataObject(DataObjectType.doSalesInvoice);

if (invoice.Read(invoiceRef))
{
    invoice.DataFields.FieldByName("AUXIL_CODE").Value = "WEB";

    ILines lines = invoice.DataFields.FieldByName("TRANSACTIONS").Lines;

    if (lines.Count > 0)
    {
        lines[0].FieldByName("QUANTITY").Value = 10;
    }

    if (!invoice.Post())
    {
        // Hata detayları loglanmalıdır.
    }
}
```

Burada kritik nokta, kayıt SQL `UPDATE` ile değil Objects üzerinden tekrar `Post()` edilerek güncellenmesidir.

---

## 15. Zorunlu Alanlar

Her veri nesnesinin zorunlu alanları aynı değildir.

Örneğin bir fişte aşağıdakilerden bazıları zorunlu olabilir:

- Tarih
- Cari hesap
- Ambar
- Satır malzeme kodu
- Miktar
- Birim
- Fiş türüne göre özel alanlar

Logo Objects eksik veya uyumsuz bir veri algılarsa `Post()` başarısız olabilir.

Bu durumda hata mesajı mutlaka okunmalıdır.

---

## 16. Post Sonrası Hata Yönetimi

Üretim kodunda yalnızca şu kontrol yeterli değildir:

```csharp
if (!data.Post())
    return false;
```

Hatanın sebebi kaydedilmelidir.

Loglanması önerilen bilgiler:

- Firma no
- Dönem no
- Nesne tipi
- Belge/kart kodu
- Entegrasyon kayıt ID’si
- `ErrorCode`
- `ErrorDesc`
- Validation hataları
- İşlem zamanı

Özellikle toplu entegrasyonlarda hata sebebi tutulmazsa sorun analizi çok zorlaşır.

---

## 17. DataFields ile SQL Kolonlarını Birebir Eşitleme Hatası

Sık yapılan yanlışlardan biri:

> SQL’de hangi kolon varsa Objects’te de aynı isimle alan vardır.

varsayımıdır.

Bu doğru değildir.

Objects alan isimleri:

- XML şemasına,
- İş nesnesine,
- Logo’nun semantic modeline

göre tanımlanır.

Örneğin SQL’de `STOCKREF` olarak saklanan ilişki Objects tarafında `MASTER_CODE` gibi kod üzerinden beslenebilir.

Objects kullanırken mümkün olduğunca Logo’nun beklediği iş anahtarları kullanılmalıdır.

---

## 18. LOGICALREF Yerine Kod Kullanımı

Yeni entegrasyonlarda dış sistemlerin Logo `LOGICALREF` değerine bağımlı hale getirilmesi çoğu zaman önerilmez.

Örneğin malzeme satırında dış sistem:

```text
STOCKREF = 43338
```

tutmak yerine mümkün olduğunda:

```text
MASTER_CODE = T30.100.010
```

benzeri iş anahtarlarıyla çalışmalıdır.

Sebep:

- Firma kopyalamalarında logicalref değişebilir.
- Test/canlı ortam arasında logicalref farklıdır.
- Başka firmalarda aynı kartın ref değeri farklı olur.

Kod temelli entegrasyon taşınabilirliği artırır.

---

## 19. Lines ve Veri Bütünlüğü

Bir fatura satırı yalnızca `LG_xxx_yy_STLINE` tablosundaki tek kayıttan ibaret değildir.

Satıra bağlı olarak şunlar oluşabilir:

- Stok hareketleri
- Seri/lot dağıtımları
- Sipariş bağlantıları
- Kampanya ilişkileri
- Cari hareketler
- Muhasebe hareketleri
- Maliyet kayıtları
- Proje ilişkileri

Bu nedenle:

> Bir satırın SQL’de fiziksel olarak nasıl tutulduğu ile Logo’nun o satırı iş nesnesi olarak nasıl yönettiği aynı kavram değildir.

---

## 20. Performans Yaklaşımı

Binlerce satırlı entegrasyonlarda Objects kullanımı SQL kadar hızlı görünmeyebilir. Ancak burada performans kadar veri bütünlüğü de değerlendirilmelidir.

İyileştirme yöntemleri:

- Gereksiz login/logout yapmamak,
- Aynı oturum içinde toplu işlem yapmak,
- SQL sorgularıyla yalnızca gerekli ön kontrolleri yapmak,
- Kod/ref eşleştirmelerini önceden cachelemek,
- Her satırda tekrar sorgu çalıştırmamak,
- Hatalı kayıtları ayrı kuyruğa almak,
- Büyük işleri kontrollü batch’lere bölmek.

---

## 21. Önerilen Entegrasyon Deseni

```text
Dış Sistem
   │
   ▼
Validasyon
   │
   ▼
Kod / Referans Eşleme
   │
   ▼
IData.New() veya IData.Read()
   │
   ├── DataFields
   │
   └── Lines
         │
         └── Alt Lines gerekiyorsa
   │
   ▼
Post()
   │
   ├── Başarılı → Logo LOGICALREF sakla
   │
   └── Hatalı → hata kuyruğu / log
```

Bu desen, Logo Objects entegrasyonlarının çoğunda temel alınabilir.

---

## 22. Best Practices

### Yapılması önerilenler

- Alan isimlerini XML/Objects şemasından doğrula.
- Satır eklemeden önce `AppendLine()` kullan.
- Satır türünü doğru belirle.
- `Post()` sonucunu her zaman kontrol et.
- Validation hatalarını logla.
- Dış sistemlerde mümkün olduğunca Logo kodlarını iş anahtarı olarak kullan.
- Seri/lot ve diğer alt koleksiyonları Objects üzerinden yönet.

### Kaçınılması gerekenler

- `STLINE` tablosuna doğrudan INSERT yapmak.
- Sadece üst fişi oluşturup satır ilişkilerini manuel üretmek.
- Objects alan adlarını SQL kolon adlarıyla aynı varsaymak.
- `Post()` hatalarını görmezden gelmek.
- Logicalref değerlerini ortamlar arasında sabit kabul etmek.

---

## 23. Sonuç

`DataFields` ve `Lines`, Logo Objects veri modelinin günlük geliştirmede en çok kullanılan iki temel bileşenidir.

Kavramsal olarak:

```text
IData
 ├── DataFields       → Kart/fiş üst bilgileri
 └── Lines            → Hareket satırları
      └── Alt Lines   → Seri/lot ve diğer detay yapıları
```

Bu yapı doğru kullanıldığında geliştirici SQL tablolarını tek tek üretmek yerine Logo’nun kendi iş nesnesini besler.

Bir sonraki bölümde `ProductionApplication` ve detaylı üretim tarafındaki nesne mimarisi ele alınacaktır.
