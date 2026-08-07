# 32 — Logo Objects ile Malzeme Kartı Örneği

## 1. Amaç

Bu bölüm Logo Objects üzerinden malzeme kartı oluşturma ve güncelleme akışını örnekler. Amaç yalnızca çalışan birkaç satır kod göstermek değil; `IApplication`, `IData`, `DataFields`, `Post()` ve hata kontrolü zincirini gerçek kullanım modeli olarak göstermektir.

> Önemli: `DataObjectType` enum adı ve bazı field isimleri kullanılan Logo Objects sürümüne göre doğrulanmalıdır. Buradaki yapı entegrasyon kalıbını anlatır.

## 2. Temel Akış

```text
IApplication
    ↓
Login
    ↓
NewDataObject(DataObjectType)
    ↓
New()
    ↓
DataFields
    ↓
Post()
    ↓
Error kontrolü
```

## 3. Malzeme Kartı Oluşturma Kalıbı

```csharp
UnityApplication.IApplication app = new UnityApplication.UnityApplication();

bool loginOk = app.Login(
    "LOGO_USER",
    "LOGO_PASSWORD",
    40,
    1
);

if (!loginOk)
{
    throw new Exception("Logo login başarısız.");
}

try
{
    UnityApplication.IData item =
        app.NewDataObject(UnityApplication.DataObjectType.doMaterial);

    item.New();

    item.DataFields.FieldByName("CODE").Value = "150.001";
    item.DataFields.FieldByName("NAME").Value = "Örnek Malzeme";
    item.DataFields.FieldByName("CARD_TYPE").Value = 1;
    item.DataFields.FieldByName("ACTIVE").Value = 0;

    if (!item.Post())
    {
        string errorText = item.ErrorDesc;
        throw new Exception("Malzeme kartı kaydedilemedi: " + errorText);
    }
}
finally
{
    app.Disconnect();
}
```

## 4. Alanların Doğrulanması

Malzeme kartında kullanılacak field isimleri Logo Objects Data Browser / dokümantasyon / çalışan örnek üzerinden doğrulanmalıdır.

Sık ihtiyaç duyulan bilgi grupları:

```text
Kod
Açıklama
Kart tipi
Aktif/pasif durumu
Grup kodu
Özel kodlar
Yetki kodu
Birim seti
KDV bilgileri
İzleme yöntemi
Seri/Lot ayarları
Muhasebe bağlantıları
```

## 5. Mevcut Kartı Bulma

Yeni kart oluşturmadan önce duplicate kontrolü yapılmalıdır.

Bunun için `IQuery` kullanılabilir:

```sql
SELECT LOGICALREF
FROM LG_040_ITEMS
WHERE CODE = '150.001'
```

Mantık:

```text
Kart varsa → update senaryosu
Kart yoksa → new senaryosu
```

## 6. Güncelleme Akışı

Güncelleme için genel yaklaşım:

```text
IData oluştur
↓
Read / GetBy... ile mevcut kayıt yükle
↓
Field değiştir
↓
Post
```

Örnek pseudo-code:

```csharp
IData item = app.NewDataObject(DataObjectType.doMaterial);

if (item.Read(itemRef))
{
    item.DataFields.FieldByName("NAME").Value = "Yeni Açıklama";

    if (!item.Post())
        throw new Exception(item.ErrorDesc);
}
```

`Read` imzası ve davranışı kullanılan Logo Objects sürümüne göre doğrulanmalıdır.

## 7. Birim Bilgisi

Malzeme kartında birim seti kritik alanlardan biridir. Sadece malzeme kodu ve adı girerek kart açmak bazı senaryolarda yeterli olmayabilir.

Doğru entegrasyon şu bilgileri doğrulamalıdır:

- `UNITSETREF`
- Ana birim
- Alternatif birimler
- Dönüşüm oranları
- Barkod ilişkileri

## 8. Barkod Ekleme

Barkodların malzeme ve birim ilişkisi vardır. Doğrudan SQL ile barkod eklemek yerine mümkünse Logo Objects nesne hiyerarşisi kullanılmalıdır.

Doğrudan SQL zorunlu ise en az şu kontroller yapılmalıdır:

```text
Aynı barkod başka malzemede var mı?
Aynı malzeme/birim için duplicate var mı?
UNITLINEREF doğru mu?
LINENR sırası doğru mu?
```

## 9. Hata Yönetimi

Sadece `Post() == false` kontrolü yeterli değildir. Hata açıklaması loglanmalıdır.

Örnek:

```csharp
if (!item.Post())
{
    Log.Error(
        "Material post failed. Code={Code}, Error={Error}",
        materialCode,
        item.ErrorDesc
    );
}
```

Log içine mümkünse şu bilgiler de eklenmelidir:

```text
Firma
Dönem
Kullanıcı
Malzeme kodu
İşlem tipi
Dış sistem ID
Logo hata açıklaması
```

## 10. Idempotency

Bir dış sistem aynı malzemeyi iki kez gönderirse duplicate kart oluşmamalıdır.

Önerilen anahtar:

```text
ExternalSystem + ExternalId
```

veya doğal anahtar olarak kontrollü şekilde:

```text
Firma + Malzeme Kodu
```

kullanılabilir.

## 11. Transaction Yaklaşımı

Malzeme kartı oluşturulduktan sonra ayrıca birim, barkod veya fiyat kartı gibi bağlı kayıtlar üretilecekse işlem bütünlüğü düşünülmelidir.

Örnek süreç:

```text
Malzeme kartı
↓
Birim
↓
Barkod
↓
Satış fiyatı
↓
Entegrasyon logu
```

Aradaki bir adım başarısız olursa sistemin hangi durumda kalacağı önceden tasarlanmalıdır.

## 12. SQL ile Doğrulama

Objects işlemi sonrası kontrol için:

```sql
SELECT
    LOGICALREF,
    CODE,
    NAME,
    UNITSETREF,
    ACTIVE
FROM LG_040_ITEMS
WHERE CODE = '150.001';
```

Bu sorgu kayıt oluşturma amacıyla değil, doğrulama amacıyla kullanılmalıdır.

## 13. Production Checklist

Canlıya çıkmadan önce:

- [ ] DataObjectType doğrulandı
- [ ] Zorunlu field'lar belirlendi
- [ ] Duplicate kontrolü var
- [ ] Birim seti doğrulanıyor
- [ ] Post hata açıklaması loglanıyor
- [ ] Firma ve dönem parametrik
- [ ] Kullanıcı bilgileri config'te güvenli tutuluyor
- [ ] Test firmasında kayıt/silme/güncelleme denendi
- [ ] SQL doğrulama sorgusu hazır

## 14. Özet

Logo Objects ile malzeme kartı entegrasyonunda esas konu `Post()` çağırmak değildir. Doğru DataObjectType, zorunlu alanlar, birim seti, duplicate kontrolü, hata yönetimi ve idempotency birlikte tasarlanmalıdır.
