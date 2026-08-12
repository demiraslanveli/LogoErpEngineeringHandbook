# 140 — IData Bridge ve Master Data Mapping Standardı

Bu bölüm malzeme ve cari kart gibi master-data nesnelerinin Logo Objects `IData` üzerinden oluşturulmasını, fakat sürüm bağımlı COM tiplerinin uygulamanın geri kalanına yayılmamasını sağlayan adapter standardını tanımlar.

## Neden Bridge?

Logo ERP kartlarının doğrudan SQL `INSERT` ile oluşturulması veri bütünlüğü açısından risklidir. Referans uygulamada kart işlemlerinin `IData` üzerinden yapılması hedeflenir.

Ancak `IData`, `DataObjectType`, field koleksiyonları ve line davranışı Logo Objects sürümüne bağlı olabilir.

Bu nedenle yapı ikiye ayrılır:

```text
Application Service
        ↓
Domain Gateway
        ↓
LogoMaterialGateway / LogoCustomerGateway
        ↓
ILogoDataObjectFactory
        ↓
ILogoDataObject
        ↓
Verified IData COM Wrapper
```

## ILogoDataObject

Referans contract şu operasyonları soyutlar:

- field set etme
- line ekleme
- post/save
- Logo hata kodu alma
- Logo hata açıklaması alma
- deterministic dispose

Amaç `IData` COM nesnesini saklamak değil, yalnızca ihtiyaç duyulan operasyon yüzeyini expose etmektir.

## Mapping Profile

Field isimleri gateway koduna gömülmez.

Malzeme için örnek:

```text
MaterialDataMappingProfile
    DataObjectTypeKey
    CodeField
    NameField
```

Cari için:

```text
CustomerDataMappingProfile
    DataObjectTypeKey
    CodeField
    TitleField
    TaxNumberField
    TaxOfficeField
```

Bu alanlar hedef Logo sürümünde doğrulanmadan profile'a production değeri verilmemelidir.

## Create Akışı

Malzeme örneği:

```text
Session Open?
    ↓
Mapping Validate
    ↓
Factory.Create(DataObjectTypeKey)
    ↓
SetField(Code)
    ↓
SetField(Name)
    ↓
Post
    ↓
Logo ErrorCode / ErrorDescription kontrolü
```

## Hata Yönetimi

Her field operasyonu ve `Post` sonucu kontrol edilir.

Yanlış yaklaşım:

```text
Set CODE
Set NAME
Post
return OK
```

Doğru yaklaşım:

```text
result = Set CODE
if failed → return

result = Set NAME
if failed → return

result = Post
if failed → Logo error parser
```

## Unconfigured Factory

SDK binding henüz tamamlanmadığında:

```text
UnconfiguredLogoDataObjectFactory
```

kullanılır.

Bu sınıf veri yazmaz ve şu tip hata döndürür:

```text
LOGO_IDATA_NOT_CONFIGURED
```

Bu sayede eksik SDK konfigurasyonu yanlışlıkla production kaydı üretmez.

## Optional Field Mantığı

Cari kart örneğinde vergi numarası veya vergi dairesi gibi alanlar ancak iki şart birlikte sağlanıyorsa yazılır:

1. Input değer mevcut.
2. Mapping profile içindeki field adı doğrulanmış.

Bu yaklaşım version-dependent optional alanlarda güvenlidir.

## Lines

Fiş, sipariş, irsaliye ve fatura gibi line içeren nesnelerde aynı bridge genişletilir:

```text
AppendLine(collectionName, mapLine)
```

Gerçek COM wrapper şu sorumluluklara sahiptir:

- line koleksiyonunu bulmak
- yeni line oluşturmak
- line field'larını set etmek
- COM hata bilgisini normalize etmek

## Master Data Doğrulama Checklist

### Malzeme

- [ ] DataObjectType doğrulandı
- [ ] CODE field doğrulandı
- [ ] NAME field doğrulandı
- [ ] zorunlu birim seti davranışı doğrulandı
- [ ] duplicate code davranışı doğrulandı
- [ ] Post hata bilgisi doğrulandı

### Cari

- [ ] DataObjectType doğrulandı
- [ ] CODE field doğrulandı
- [ ] unvan field doğrulandı
- [ ] vergi numarası field doğrulandı
- [ ] vergi dairesi field doğrulandı
- [ ] duplicate code davranışı doğrulandı
- [ ] Post hata bilgisi doğrulandı

## Temel Prensip

```text
Domain mapping bilinir
COM implementation izole edilir
SDK metadata doğrulanır
IData ile resmi kayıt oluşturulur
```

> Referans uygulamanın amacı Logo Objects API isimlerini tahmin etmek değil, doğrulanmış SDK bilgisinin güvenli biçimde kullanılacağı sürdürülebilir bir entegrasyon çerçevesi oluşturmaktır.
