# 56 — Logo Objects Tam CRUD Örnekleri

## Amaç

Bu bölüm `IData` nesnesi ile kart ve fişlerde temel CRUD akışlarını tek yerde toplar. Amaç kopyala-yapıştır kod vermekten çok doğru işlem sırasını standartlaştırmaktır.

> Logo Objects sürümlerinde `DataObjectType`, field adları ve bazı metod davranışları farklılık gösterebilir. Üretim ortamında kullanılan Objects sürümündeki enum ve field adları mutlaka doğrulanmalıdır.

## 1. Genel Akış

Logo Objects ile CRUD işlemlerinin ortak iskeleti:

```text
IApplication oluştur
    ↓
Login
    ↓
Firma / dönem seç
    ↓
NewDataObject
    ↓
New / Read / Edit / Delete
    ↓
DataFields ve Lines işlemleri
    ↓
Post
    ↓
Hata kontrolü
    ↓
Logout / Dispose
```

## 2. Yeni Kayıt

Temel mantık:

```csharp
IData data = app.NewDataObject(DataObjectType.xxx);

data.New();

data.DataFields.FieldByName("CODE").Value = "TEST.001";
data.DataFields.FieldByName("NAME").Value = "Test Kartı";

if (!data.Post())
{
    // ErrorCode / ErrorDesc ve validation hataları loglanmalı.
}
```

`xxx` yerine kullanılan Objects sürümündeki gerçek `DataObjectType` değeri yazılmalıdır.

## 3. Kayıt Okuma

Kayıt okumada mümkün olduğunda `LOGICALREF` gibi benzersiz referans kullanılmalıdır.

Örnek yaklaşım:

```csharp
IData data = app.NewDataObject(DataObjectType.xxx);

if (data.Read(logicalRef))
{
    string code = Convert.ToString(
        data.DataFields.FieldByName("CODE").Value);
}
```

Okuma başarılı değilse kayıt yok, yetki sorunu, yanlış firma/dönem veya nesne türü uyuşmazlığı kontrol edilmelidir.

## 4. Güncelleme

Genel sıra:

```text
Read
↓
Edit
↓
Alan değişiklikleri
↓
Post
```

Örnek:

```csharp
IData data = app.NewDataObject(DataObjectType.xxx);

if (data.Read(logicalRef))
{
    data.Edit();
    data.DataFields.FieldByName("NAME").Value = "Yeni Açıklama";

    if (!data.Post())
    {
        // Hata detayını logla.
    }
}
```

Doğrudan SQL `UPDATE` yerine bu akışın tercih edilmesinin nedeni Logo iş kurallarının çalışmasıdır.

## 5. Silme

Silme işlemi nesne tipine ve kullanılan Objects sürümüne göre doğrulanmalıdır.

Temel fikir:

```text
Kaydı bul
↓
Silme işlemini çağır
↓
Bağlı kayıt / kullanım kontrolü
↓
Sonucu doğrula
```

Kartlar kullanımda ise Logo silmeye izin vermeyebilir. Bu hata SQL ile zorlanarak aşılmamalıdır.

## 6. Fiş Satırı Ekleme

Fiş tipi nesnelerde satırlar `Lines` koleksiyonu üzerinden yönetilir.

Genel kalıp:

```csharp
IData fiche = app.NewDataObject(DataObjectType.xxx);
fiche.New();

fiche.DataFields.FieldByName("NUMBER").Value = "~";
fiche.DataFields.FieldByName("DATE").Value = DateTime.Today;

ILines lines = fiche.DataFields.FieldByName("TRANSACTIONS").Lines;

lines.AppendLine();
lines[lines.Count - 1].FieldByName("TYPE").Value = 0;
lines[lines.Count - 1].FieldByName("MASTER_CODE").Value = "MALZEME.001";
lines[lines.Count - 1].FieldByName("QUANTITY").Value = 10;
lines[lines.Count - 1].FieldByName("PRICE").Value = 100;

if (!fiche.Post())
{
    // Validation hatalarını yaz.
}
```

Field isimleri nesne tipine göre değişebilir; kullanılan XML/schema veya çalışan örnek üzerinden doğrulanmalıdır.

## 7. Satır Güncelleme

Mevcut fiş okunduktan sonra `Lines` üzerinde ilgili satır bulunur.

Satırı yalnızca sıra numarasına göre bulmak risklidir. Mümkünse:

- satır referansı,
- malzeme kodu,
- dış sistem satır ID’si,
- benzersiz entegrasyon anahtarı

kullanılmalıdır.

## 8. Validation Hataları

`Post()` false döndüğünde yalnızca genel hata metni gösterilmemelidir.

Loglanması gerekenler:

```text
Firma
Dönem
DataObjectType
İşlem türü
Kayıt anahtarı
ErrorCode
ErrorDesc
ValidationErrors
Payload özeti
```

Bu yaklaşım üretim ortamındaki sorunların tekrar üretilebilmesini sağlar.

## 9. Transaction Yaklaşımı

Bir dış işlem birden fazla Logo nesnesi oluşturuyorsa atomiklik ihtiyacı ayrıca tasarlanmalıdır.

Örnek:

```text
Sipariş oluştur
↓
İrsaliye oluştur
↓
Fatura oluştur
```

Üç adımın tek iş süreci olduğu durumda ikinci adım başarısızken ilk adımın kalıp kalmayacağı önceden belirlenmelidir.

Logo Objects'in desteklediği transaction mekanizmaları kullanılan sürümde doğrulanmalıdır. Desteklenmeyen senaryolarda telafi (compensation) tasarımı yapılmalıdır.

## 10. Idempotent Create

Dış sistemden kayıt oluştururken tekrar gönderime dayanıklı tasarım kullanılmalıdır.

Önce:

```text
ExternalId daha önce işlendi mi?
```

kontrol edilir.

İşlendiyse yeni kayıt oluşturmak yerine mevcut sonuç döndürülür.

Önerilen log yapısı:

```text
ExternalId
OperationType
FirmNo
PeriodNo
LogoLogicalRef
Status
CreatedAt
LastAttemptAt
ErrorMessage
```

## 11. Create Öncesi Kontroller

Kart oluştururken:

- kod zaten var mı?
- zorunlu alanlar dolu mu?
- birim seti geçerli mi?

Fiş oluştururken:

- cari var mı?
- malzeme var mı?
- birim geçerli mi?
- ambar geçerli mi?
- tarih dönem içinde mi?
- seri/lot gerekiyorsa mevcut mu?
- proje/fabrika/işyeri doğru mu?

kontrol edilmelidir.

## 12. Read için SQL, Write için Objects

Pratik entegrasyon mimarisinde sık kullanılan güçlü yaklaşım:

```text
SQL / IQuery → hızlı kontrol ve raporlama
IData        → kayıt oluşturma/güncelleme/silme
```

Örneğin malzemenin varlığı SQL ile hızlı kontrol edilebilir; yeni malzeme kartı oluşturulacaksa `IData` tercih edilir.

## 13. Doğrudan SQL Ne Zaman?

`INSERT`, `UPDATE`, `DELETE` işlemleri ancak:

- resmi nesne desteği yoksa,
- Logo teknik dokümantasyonu açıkça izin veriyorsa,
- tüm bağlı kayıt yapısı biliniyorsa,
- transaction ve rollback planı varsa,
- test ortamında doğrulanmışsa

istisnai olarak düşünülmelidir.

## 14. Servis Katmanı Önerisi

Kurumsal projelerde Logo Objects çağrılarını UI içine dağıtmak yerine servis katmanında toplamak daha sağlıklıdır.

```text
UI / API
   ↓
Application Service
   ↓
LogoObjectsService
   ↓
IApplication / IData / IQuery
```

Örnek servisler:

```text
MaterialService
ClientService
OrderService
InvoiceService
ProductionService
```

## 15. Sonuç

Logo Objects CRUD işlemlerinde ana hedef yalnızca kayıt oluşturmak değildir.

Doğru işlem:

```text
Doğru firma/dönem
+ doğru DataObjectType
+ doğru field yapısı
+ doğru Lines kullanımı
+ Post doğrulaması
+ ayrıntılı hata logu
+ idempotency
= güvenilir Logo entegrasyonu
```
