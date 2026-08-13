# 151 — İrsaliye / Fatura Verified IData Binding

Bu bölüm irsaliye ve fatura nesnelerinin gerçek Logo Objects `IData` binding standardını tanımlar.

## Akış

```text
DispatchInvoiceService
      ↓
LogoDispatchInvoiceGateway
      ↓
DispatchInvoiceDataMappingProfile
      ↓
VerifiedLogoDataObjectFactory
      ↓
IData Header + Lines
```

## Doğrulanacak Başlık Alanları

- irsaliye ve fatura `DataObjectType`,
- belge numarası,
- tarih,
- cari,
- ödeme planı,
- işyeri/ambar,
- ticari işlem grubu,
- belge özel kodları,
- e-belge ile ilişkili alanlar gerekiyorsa sürüm davranışı.

## Satır Alanları

- malzeme,
- miktar,
- birim,
- fiyat,
- KDV,
- ambar,
- iskonto,
- proje,
- sipariş/irsaliye kaynak ilişkisi.

## Kaynak Belge İlişkisi

Siparişten irsaliyeye veya faturaya geçişte `ORDTRANSREF`, `PREVLINEREF`, `SOURCELINK` gibi ilişkiler elle uydurulmamalıdır. Logo Objects'in resmi bağlantı mekanizması kullanılmalı ve ortaya çıkan referanslar SQL üzerinden doğrulanmalıdır.

## Kısmi Faturalama

Kısmi miktar aktarımında kalan miktar, önceki belge ilişkileri ve satır bağlantıları birlikte test edilmelidir.

## Muhasebe Etkisi

Fatura post edildikten sonra yalnızca INVOICE/STLINE değil, cari hareket ve varsa muhasebe fişi ilişkileri de kontrol edilmelidir.

## Kabul Testleri

- bağımsız irsaliye,
- bağımsız fatura,
- siparişten irsaliye,
- siparişten fatura,
- irsaliyeden fatura,
- kısmi faturalama,
- KDV/iskonto,
- cari hareket doğrulaması,
- muhasebe bağlantısı doğrulaması.

> Belge oluşturmanın başarı kriteri yalnızca `Post=true` değildir; Logo'nun kaynak belge, stok, cari ve muhasebe ilişkilerinin doğru oluşmasıdır.
