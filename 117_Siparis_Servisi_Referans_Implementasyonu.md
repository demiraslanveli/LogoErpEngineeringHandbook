# 117 — Sipariş Servisi Referans Implementasyonu

Bu bölüm satış ve satınalma siparişlerinin referans .NET Framework 4.8 entegrasyon mimarisinde nasıl servisleştirileceğini açıklar.

## Amaç

Sipariş başlık ve satırlarını Logo Objects üzerinden oluşturmak/güncellemek; müşteri, malzeme, birim, ambar, ödeme planı ve proje referanslarını kontrollü biçimde çözmek.

## Katmanlar

```text
OrderApplicationService
        ↓
OrderValidator
        ↓
OrderReferenceResolver
        ↓
OrderMapper
        ↓
IOrderRepository
        ↓
LogoOrderRepository
        ↓
IData
```

## DTO

```csharp
public sealed class OrderRequest
{
    public string ExternalOrderId { get; set; }
    public string ClientCode { get; set; }
    public DateTime Date { get; set; }
    public string WarehouseCode { get; set; }
    public string PaymentPlanCode { get; set; }
    public List<OrderLineRequest> Lines { get; set; }
}

public sealed class OrderLineRequest
{
    public string ExternalLineId { get; set; }
    public string MaterialCode { get; set; }
    public decimal Quantity { get; set; }
    public string UnitCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VatRate { get; set; }
    public string ProjectCode { get; set; }
}
```

## Referans çözümleme

Sipariş kaydı öncesinde aşağıdaki değerler Logo logical reference değerlerine çevrilmelidir:

- cari
- malzeme
- birim
- ambar
- ödeme planı
- proje

```text
Business Code
    ↓
Read repository / IQuery
    ↓
LOGICALREF
    ↓
IData mapping
```

## Validation

Sipariş validasyonu iki aşamalı olmalıdır.

### Domain validation

- miktar > 0
- malzeme kodu boş değil
- cari kodu boş değil
- tarih geçerli
- satır tekrarları kontrol altında

### ERP reference validation

- cari Logo'da mevcut
- malzeme mevcut ve aktif
- birim malzemeye bağlı
- ambar geçerli
- ödeme planı mevcut
- proje varsa referansı bulunuyor

## Başlık ve satır işlemi

Logo Objects tarafında sipariş bir belge nesnesidir.

```text
Header
  ↓
Lines
  ↓
Line references
  ↓
Post()
```

Satırlar ayrı SQL kayıtları gibi değil, belgenin parçası olarak yönetilmelidir.

## Fiyat ve KDV

Entegrasyonun fiyat kaynağı açıkça belirlenmelidir.

İki yaygın model:

```text
A) Dış sistem fiyatı belirler
B) Logo fiyat kartları belirler
```

Aynı siparişte bu iki model karıştırılmamalıdır.

KDV oranı ve istisna/muafiyet bilgileri de ürün/fatura süreciyle tutarlı olmalıdır.

## Kısmi sevk/faturalama

Sipariş satırı daha sonra irsaliye/fatura satırlarına bağlanabilir.

Bu nedenle external line mapping tutulması önerilir:

```text
ExternalOrderId
ExternalLineId
LogoOrderRef
LogoOrderLineRef
```

Bu mapping kısmi faturalanma ve reconciliation için kritiktir.

## Idempotency

Aynı dış sipariş tekrar gönderildiğinde ikinci bir Logo siparişi oluşmamalıdır.

Önerilen key:

```text
SourceSystem + Order + ExternalOrderId
```

## Update kuralı

Sipariş üzerinde sevk/faturalama başladıktan sonra değişiklik politikası sıkılaştırılmalıdır.

Örnek:

```text
Henüz işlem görmemiş sipariş → kontrollü update
Kısmi sevk edilmiş sipariş     → satır bazlı kontrol
Tamamlanmış/faturalanmış      → doğrudan overwrite yapılmaz
```

## Test senaryoları

1. yeni sipariş
2. duplicate external order
3. geçersiz cari
4. geçersiz malzeme
5. yanlış birim
6. yanlış ambar
7. fiyat/KDV problemi
8. birden fazla satır
9. proje referanslı sipariş
10. kısmi sevk edilmiş sipariş update denemesi
11. Logo Post validation hatası

> Sipariş servisi yalnızca fiş oluşturan bir adapter değil; sipariş yaşam döngüsünü ve sonraki irsaliye/fatura ilişkilerini koruyan application service olmalıdır.
