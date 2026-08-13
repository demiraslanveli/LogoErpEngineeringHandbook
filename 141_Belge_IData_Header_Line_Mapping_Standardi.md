# 141 — Belge IData Header / Line Mapping Standardı

Sipariş, irsaliye ve fatura gibi belgelerde yalnızca header alanlarının map edilmesi yeterli değildir. Satır koleksiyonunun, ambar bilgisinin, miktar/fiyat/KDV alanlarının ve post işleminin aynı sözleşme altında yönetilmesi gerekir.

## Hedef

Logo Objects sürümüne bağlı `DataObjectType`, header field, line collection ve satır field adlarını application katmanından izole etmek.

```text
Application Service
      ↓
Gateway
      ↓
Mapping Profile
      ↓
ILogoDataObjectFactory
      ↓
ILogoDataObject
      ↓
IData / Lines
```

## Mapping Profile

Belge adapter'ında şu bilgiler profile üzerinden tanımlanır:

- belge `DataObjectType` anahtarı,
- belge numarası alanı,
- tarih alanı,
- cari kod alanı,
- satır koleksiyonu anahtarı,
- malzeme kodu,
- miktar,
- birim fiyat,
- ambar,
- KDV oranı.

Bu değerlerin gerçek karşılıkları kullanılan Logo Objects sürümünde doğrulanmadan kod içine sabitlenmemelidir.

## Header / Line Akışı

```text
factory.Create(objectType)
        ↓
header.SetField(...)
        ↓
AppendLine(linesCollection)
        ↓
line.SetField(...)
        ↓
Post()
```

### İrsaliye ve Fatura Ayrımı

Aynı teknik mapping sınıfı kullanılabilir ancak farklı `DataObjectType` anahtarları tanımlanmalıdır.

```text
DispatchDataObjectTypeKey
InvoiceDataObjectTypeKey
```

Böylece ortak field yapısı tekrar kullanılabilirken belge türü ayrımı korunur.

## Neden Bu Katman Gerekli?

Doğrudan gateway içinde aşağıdaki bilgileri sabitlemek risklidir:

```text
DataObjectType numeric enum
LINES adı
ITEM_CODE field adı
SOURCEINDEX / warehouse field adı
VAT field adı
Post / Apply davranışı
```

Bu bilgiler Logo Objects sürümüne ve kullanılan nesne tipine göre doğrulanmalıdır.

## Hata Yönetimi

Factory belgeyi oluşturamazsa gateway field mapping'e geçmemelidir.

```text
Factory Create
   ↓ failure
OperationResult
```

`Post()` sonrasında Logo Objects tarafından dönen hata kodu/açıklaması adapter seviyesinde `OperationResult` biçimine çevrilmelidir.

## İlişkili Belgeler

Sipariş → irsaliye → fatura bağlantılarında yalnızca yeni belge oluşturmak yeterli değildir. Kaynak satır referansları ve Logo'nun kendi bağlantı mekanizması doğrulanarak kullanılmalıdır.

Özellikle şu alanların SQL tarafındaki karşılıkları ilişki analizi için önemlidir:

- `ORDTRANSREF`
- `ORDFICHEREF`
- `PREVLINEREF`
- `SOURCELINK`
- `INVOICEREF`
- `STFICHEREF`

Ancak Logo Objects üzerinden kaynak belge bağlantısı yapılırken SQL alanlarına doğrudan müdahale edilmemelidir.

## Sonuç

Belge entegrasyonunun kalıcı standardı:

```text
DTO
 ↓
Application Validation
 ↓
Gateway
 ↓
Version-specific Mapping Profile
 ↓
Generic IData Bridge
 ↓
Logo Objects
```

Bu model sipariş, irsaliye ve fatura entegrasyonlarının aynı teknik omurgayı kullanmasını sağlar.
