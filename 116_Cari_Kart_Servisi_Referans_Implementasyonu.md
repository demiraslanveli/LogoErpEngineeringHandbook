# 116 — Cari Kart Servisi Referans Implementasyonu

Bu bölüm, cari hesap kartlarının referans .NET Framework 4.8 entegrasyon mimarisinde nasıl servisleştirileceğini açıklar.

## Amaç

Cari kart oluşturma, güncelleme ve okuma işlemlerini Logo Objects üzerinden güvenli biçimde yönetmek; `CLCARD` tablosunu yalnızca veri okuma ve kontrol amacıyla kullanmak.

## Katmanlar

```text
ClientApplicationService
        ↓
ClientValidator
        ↓
ClientMapper
        ↓
IClientRepository
        ↓
LogoClientRepository
        ↓
IData
```

## DTO

```csharp
public sealed class ClientRequest
{
    public string Code { get; set; }
    public string Title { get; set; }
    public string TaxNumber { get; set; }
    public string TaxOffice { get; set; }
    public string PaymentPlanCode { get; set; }
    public bool Active { get; set; }
}
```

## Temel kontroller

Yeni cari kart öncesinde en az:

- cari kodu
- vergi / TCKN bilgisi gerekiyorsa duplicate kontrolü
- ödeme planı referansı
- zorunlu adres/ülke alanları
- e-belge kullanımına ilişkin gerekli alanlar

kontrol edilmelidir.

## Repository sözleşmesi

```csharp
public interface IClientRepository
{
    LogoOperationResult<int> Create(ClientRequest request, LogoContext context);
    LogoOperationResult Update(int logicalRef, ClientRequest request, LogoContext context);
    ClientDto GetByCode(string code, LogoContext context);
}
```

## Ödeme planı

Cari kart üzerindeki ödeme planı ilişkisi yalnızca kullanıcıdan gelen metin olarak tutulmamalıdır.

Önerilen akış:

```text
PaymentPlanCode
    ↓
PAYPLANS lookup
    ↓
LogicalRef
    ↓
IData field mapping
```

## e-Belge alanları

Cari kartın e-Fatura/e-Arşiv/e-İrsaliye süreçlerine etkisi olabilir. Bu alanların kesin isimleri ve davranışları sürüme göre doğrulanmalıdır.

E-belge kullanımı olan müşterilerde entegrasyon testleri mutlaka gerçek Logo davranışıyla doğrulanmalıdır.

## Update yaklaşımı

Cari kart güncellemesinde entegrasyonun sahibi olmadığı alanlara dokunulmamalıdır.

Örnek ownership:

```text
Entegrasyon sahibi:
- CODE
- DEFINITION
- vergi bilgileri
- ödeme planı

Logo kullanıcılarının sahibi:
- risk ayarları
- özel raporlama alanları
- manuel operasyon notları
```

Bu liste projeye göre açıkça tanımlanmalıdır.

## Cari kart ve hareket ayrımı

`CLCARD` master veridir.

Cari hareketler `CLFLINE` tarafındadır.

Kart update işlemi mevcut cari hareket geçmişini doğrudan değiştirmemelidir.

## Idempotency

Dış sistem müşteri ID'si için kalıcı bir eşleştirme tutulmalıdır:

```text
ExternalClientId
    ↔
Logo CLCARD.LOGICALREF
```

## Test senaryoları

1. yeni cari
2. duplicate cari kodu
3. duplicate vergi numarası
4. geçersiz ödeme planı
5. mevcut cari güncelleme
6. pasif cari
7. Logo Post validation hatası
8. tekrar gönderilen aynı external client
9. e-belge alanı bulunan cari

> Cari kart servisi, müşteri master verisinin Logo tarafındaki kontrollü sahibi olmalı; cari hareket mantığıyla karıştırılmamalıdır.
