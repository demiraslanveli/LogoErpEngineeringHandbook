# 20 — DataObjectType Referansı

## 1. Amaç

Logo Objects ile çalışırken `IApplication.NewDataObject(...)` metoduna verilen `DataObjectType`, hangi kart veya fiş nesnesi üzerinde işlem yapılacağını belirleyen temel seçimdir.

Bu nedenle entegrasyon geliştirirken ilk sorulardan biri şudur:

> Hangi Logo nesnesini açıyorum ve bu nesne hangi iş kuralını temsil ediyor?

`DataObjectType` seçimi yalnızca teknik bir enum seçimi değildir. Yanlış nesne tipi seçildiğinde alan yapısı, satır koleksiyonları, validasyonlar ve post işlemleri beklenenden farklı davranabilir.

---

## 2. Temel Kullanım

```csharp
UnityObjects.IData data = App.NewDataObject(UnityObjects.DataObjectType.xxx);
```

Genel akış:

```text
IApplication
    ↓
NewDataObject(DataObjectType)
    ↓
IData
    ↓
New / Read / Delete / Post
```

---

## 3. Kart ve Fiş Ayrımı

Logo nesnelerini iki ana grupta düşünmek faydalıdır.

### Kart tipi nesneler

Örnek iş alanları:

- Malzeme kartı
- Cari hesap kartı
- Hizmet kartı
- Banka hesabı
- Kasa kartı
- Proje kartı

Kartlarda çoğunlukla tek bir ana kayıt ve ona bağlı yardımcı koleksiyonlar bulunur.

### Fiş tipi nesneler

Örnek iş alanları:

- Satınalma siparişi
- Satış siparişi
- Satınalma irsaliyesi
- Satış irsaliyesi
- Satınalma faturası
- Satış faturası
- Ambar fişi
- Üretim bağlantılı hareketler

Fiş nesnelerinde temel yapı genellikle şöyledir:

```text
Fiş Üst Bilgisi
    ↓
TRANSACTIONS / satır koleksiyonu
    ↓
Malzeme / hizmet / masraf satırları
    ↓
Dağıtımlar / seri-lot / muhasebe bağlantıları
```

---

## 4. Enum Değerlerini Ezberlemeyin

Logo Objects sürümüne ve kullanılan ürün ailesine göre enum adları veya destek kapsamı değişebilir.

Bu nedenle uygulama içinde mümkünse sayısal enum değeri kullanmayın.

Kötü yaklaşım:

```csharp
App.NewDataObject((DataObjectType)17);
```

Daha güvenli yaklaşım:

```csharp
App.NewDataObject(DataObjectType.doMaterial);
```

> Bu dokümanda enum isimleri örnekleme amacı taşır. Gerçek geliştirmede kullanılan Logo Objects sürümündeki enum tanımları doğrulanmalıdır.

---

## 5. Nesne Tipi Seçerken Kontrol Listesi

Bir işleme başlamadan önce şunları belirleyin:

1. İşlem kart mı fiş mi?
2. Satınalma mı satış mı?
3. Sipariş mi irsaliye mi fatura mı?
4. Malzeme mi hizmet mi?
5. İade işlemi mi?
6. İşlem Logo üretim modülüyle ilişkili mi?
7. Seri/lot gerektiriyor mu?
8. Muhasebe bağlantısı bekleniyor mu?

Bu soruların cevabı doğrudan kullanılacak nesne tipini etkiler.

---

## 6. Yeni Kayıt Şablonu

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);

data.New();

data.DataFields.FieldByName("CODE").Value = "...";
data.DataFields.FieldByName("NAME").Value = "...";

if (!data.Post())
{
    // ErrorCode / ErrorDesc / ValidateErrors kontrol edilir.
}
```

---

## 7. Mevcut Kayıt Okuma Şablonu

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);

if (data.Read(logicalRef))
{
    string code = data.DataFields.FieldByName("CODE").Value.ToString();
}
```

Kritik nokta:

`Read` çağrısında kullanılan referans çoğunlukla ilgili kaydın `LOGICALREF` değeridir.

---

## 8. Güncelleme Şablonu

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);

if (data.Read(logicalRef))
{
    data.DataFields.FieldByName("NAME").Value = "Yeni açıklama";

    if (!data.Post())
    {
        // Hata yönetimi
    }
}
```

Logo Objects tarafında kayıt okunduktan sonra alanlar değiştirilip `Post()` çağrılması, doğrudan SQL `UPDATE` işlemine göre iş kurallarına daha uyumlu bir yöntemdir.

---

## 9. Silme Şablonu

Silme işlemi basit görünse de ilişkili kayıtlar nedeniyle en dikkat edilmesi gereken işlemlerden biridir.

```csharp
IData data = App.NewDataObject(DataObjectType.xxx);

if (data.Read(logicalRef))
{
    bool result = data.Delete();
}
```

Doğrudan SQL ile kayıt silmek aşağıdaki ilişkileri bozabilir:

- Fiş-satır ilişkileri
- Cari hareketler
- Muhasebe bağlantıları
- Seri/lot hareketleri
- Sipariş bağlantıları
- Üretim bağlantıları

---

## 10. DataObjectType İçin Mimari İlke

Bir entegrasyonda şu mantık izlenmelidir:

```text
İş İhtiyacı
    ↓
Logo Belge/Kart Tipi
    ↓
DataObjectType
    ↓
IData Alanları
    ↓
Post()
    ↓
Logo İş Kuralları
```

SQL tablosundan geriye doğru nesne tipi tahmin etmek yerine, iş sürecinden nesne tipine gitmek daha güvenlidir.

---

## 11. Sık Yapılan Hatalar

### Yanlış nesne türüyle işlem yapmak

Benzer alanlar içerdiği için yanlış belge türü seçilebilir.

### TRCODE ile DataObjectType'ı aynı şey sanmak

`TRCODE`, Logo veritabanındaki işlem türünü ifade eder.

`DataObjectType` ise Logo Objects tarafında açılan nesnenin tipidir.

Bunlar birbiriyle ilişkili olabilir ancak aynı kavram değildir.

### Enum değerini sabit sayı olarak kullanmak

Bakımı ve sürüm geçişini zorlaştırır.

### Post öncesi zorunlu alanları kontrol etmemek

Logo Objects validasyon hataları oluşabilir.

---

## 12. Önerilen Uygulama Katmanı

Büyük projelerde her yerde doğrudan `NewDataObject` çağırmak yerine servis katmanı kullanılabilir.

```text
LogoApplicationService
    ├── MaterialService
    ├── ClientService
    ├── PurchaseService
    ├── SalesService
    └── ProductionService
```

Böylece:

- nesne tipi seçimi merkezi hale gelir,
- hata yönetimi standartlaşır,
- loglama kolaylaşır,
- test edilebilirlik artar,
- Logo sürüm geçişleri daha kontrollü yapılır.

---

## 13. Sonuç

`DataObjectType`, Logo Objects entegrasyonunun başlangıç noktalarından biridir. Doğru nesne tipi seçilmeden doğru `IData` modeli kurulamaz.

Temel prensip:

> Önce Logo'daki gerçek iş belgesini belirle, sonra uygun `DataObjectType` ile `IData` oluştur.
