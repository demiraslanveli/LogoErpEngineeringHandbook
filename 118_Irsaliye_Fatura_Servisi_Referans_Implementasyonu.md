# 118 — İrsaliye / Fatura Servisi Referans Implementasyonu

Bu bölüm, satış ve satınalma irsaliye/fatura belgelerinin referans .NET Framework 4.8 entegrasyon mimarisinde nasıl servisleştirileceğini açıklar.

## Amaç

Belge başlığı ve satırlarını Logo Objects üzerinden oluşturmak; sipariş bağlantısı, ambar, cari, malzeme, birim, KDV, döviz ve muhasebe etkilerini birlikte ele almak.

## Katmanlar

```text
InvoiceApplicationService
        ↓
InvoiceValidator
        ↓
ReferenceResolver
        ↓
InvoiceMapper
        ↓
IInvoiceRepository
        ↓
LogoInvoiceRepository
        ↓
IData
```

## DTO

```csharp
public sealed class InvoiceRequest
{
    public string ExternalInvoiceId { get; set; }
    public string ClientCode { get; set; }
    public DateTime Date { get; set; }
    public string WarehouseCode { get; set; }
    public string CurrencyCode { get; set; }
    public decimal ExchangeRate { get; set; }
    public List<InvoiceLineRequest> Lines { get; set; }
}
```

## Sipariş bağlantısı

Siparişe bağlı faturada yalnızca aynı ürün/miktar bilgilerini tekrar yazmak yeterli değildir.

Logo satır ilişkisinin korunması gerekir.

```text
ExternalOrderLineId
      ↓
Logo ORFLINE logical reference
      ↓
Invoice/dispatch line relation
```

İlgili exact field isimleri kullanılan Logo sürümüne göre doğrulanmalıdır.

## Kısmi faturalanma

Bir sipariş satırı birden fazla faturaya bölünebilir.

Bu nedenle servis şu değerleri hesaplayabilmelidir:

```text
Sipariş miktarı
- Önceden faturalanan
= Kalan miktar
```

Yeni faturalanacak miktar kalan miktarı aşmamalıdır.

## İrsaliye bağlantısı

Fatura mevcut bir irsaliyeden üretilecekse, yeni stok hareketi yaratıp yaratılmayacağı Logo belge tipine ve iş akışına göre doğru yönetilmelidir.

Amaç aynı fiziksel hareketi iki kez stoktan düşmemektir.

## KDV ve istisna

Satır bazında:

- KDV oranı
- KDV tutarı
- istisna kodu
- muafiyet açıklaması

birlikte değerlendirilmelidir.

KDV oranı 0 olup istisna/muafiyet açıklaması zorunlu olan senaryolar validation katmanında yakalanmalıdır.

## Döviz

Belge ve satır döviz alanları tutarlı olmalıdır.

Kontroller:

- döviz tipi
- işlem kuru
- raporlama kuru
- birim fiyat dövizi
- toplamlar

## Ambar

Başlık ve satır ambarlarının ERP iş kuralına uygun olup olmadığı doğrulanmalıdır.

Özellikle kaynak/destination ambar kullanılan hareketlerde yalnızca header warehouse değerine güvenilmemelidir.

## Muhasebe etkisi

Fatura kaydı sonrası süreç aşağıdaki zinciri etkileyebilir:

```text
INVOICE
  ↓
STFICHE / STLINE
  ↓
CLFLINE
  ↓
EMFICHE / EMFLINE
```

Bu ilişkiler nedeniyle doğrudan tek tablo SQL update işlemleri risklidir.

## Reconciliation

Başarılı kayıt sonrasında en az şu bilgiler saklanmalıdır:

- ExternalInvoiceId
- LogoInvoiceRef
- InvoiceNo
- bağlı sipariş satırları
- bağlı irsaliye
- toplam net
- KDV
- döviz
- kayıt zamanı

## Idempotency

```text
SourceSystem + Invoice + ExternalInvoiceId
```

aynı belge için ikinci kayıt oluşmasını engellemelidir.

## Test senaryoları

1. bağımsız fatura
2. siparişe bağlı fatura
3. kısmi fatura
4. kalan miktarı aşan fatura
5. irsaliyeden fatura
6. KDV 0 + boş muafiyet
7. dövizli fatura
8. yanlış ambar
9. duplicate external invoice
10. Logo Post hatası
11. muhasebe sonrası reconciliation

> İrsaliye/fatura servisi yalnızca belge kaydetmez; sipariş, stok, cari, KDV, döviz ve muhasebe etkilerini tek işlem zinciri olarak korur.
