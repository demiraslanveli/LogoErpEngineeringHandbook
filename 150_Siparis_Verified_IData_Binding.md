# 150 — Sipariş Verified IData Binding

Bu bölüm satış siparişinin gerçek Logo Objects `IData` binding standardını tanımlar.

## Akış

```text
OrderService
  ↓
LogoOrderGateway
  ↓
OrderDataMappingProfile
  ↓
VerifiedLogoDataObjectFactory
  ↓
IData Header + Lines
```

## Header Doğrulama Seti

- satış siparişi `DataObjectType`,
- fiş numarası,
- tarih,
- cari referansı/kodu çözümleme yöntemi,
- işyeri/fabrika/ambar bağlamı,
- ödeme planı,
- ticari işlem grubu gibi kuruluma özel alanlar.

## Line Doğrulama Seti

- malzeme referansı/kodu,
- miktar,
- birim,
- fiyat,
- KDV,
- ambar,
- proje,
- satır tipi,
- iskonto satırlarının gerçek Lines modeli.

## Referans Çözümleme

Kodları doğrudan reference alanına yazmak yerine hedef SDK'nın beklediği ilişki şekli doğrulanmalıdır. Gerekirse read/query repository ile `LOGICALREF` çözülür.

## İskonto ve LINETYPE

İskonto davranışı yalnızca yüzde alanı gibi ele alınmamalıdır. Logo'nun satır tipi ve bağlı satır yapısı hedef sürümde doğrulanmalıdır.

## Post Sonrası

Başarılı post sonrası en az şu bilgiler loglanmalıdır:

```text
OrderLogicalRef
OrderNumber
CustomerCode
LineCount
CorrelationId
```

## Testler

- tek satırlı sipariş,
- çok satırlı sipariş,
- farklı ambarlar,
- iskonto,
- KDV,
- invalid item,
- invalid customer,
- duplicate external event,
- SQL read-back ile ORFICHE/ORFLINE kontrolü.

> Sipariş binding'i daha sonraki irsaliye ve fatura ilişkilerinin başlangıç noktasıdır; referans bağlantıları Logo tarafından doğru üretilmelidir.
