# 149 — Cari Kart Verified IData Binding

Bu bölüm cari hesap kartı için gerçek Logo Objects `IData` binding standardını tanımlar.

## Akış

```text
CustomerService
   ↓
LogoCustomerGateway
   ↓
CustomerDataMappingProfile
   ↓
VerifiedLogoDataObjectFactory
   ↓
IData
```

## Doğrulanacak Alanlar

- cari kart `DataObjectType`,
- cari kodu,
- ünvan/açıklama,
- kart türü,
- vergi numarası / TCKN davranışı,
- vergi dairesi,
- ödeme planı gerekiyorsa ilişkisi,
- adres/iletişim alt kayıtları gerekiyorsa Lines yapısı,
- post/save davranışı,
- logical reference erişimi.

## Validation

Application katmanında format ve zorunluluk kontrolü yapılır; SDK seviyesinde Logo'nun iş kuralı hataları ayrıca normalize edilir.

## Duplicate ve Idempotency

Cari kodu dış sistem açısından doğal anahtar olarak kullanılabilir ancak entegrasyon idempotency key'i ile karıştırılmamalıdır.

```text
External Event Id → Idempotency
Customer Code     → ERP Business Key
```

## Update İlkesi

Kart değişikliğinde resmi `IData` read/edit/post akışı tercih edilir. SQL UPDATE yalnızca Logo dışı integration metadata için kullanılmalıdır.

## Kabul Testleri

- gerçek cari açılışı,
- duplicate cari kodu,
- vergi alanı validasyonu,
- eksik zorunlu alan,
- post hata mesajı,
- logicalref ve CLCARD read-back doğrulaması.

> Cari kart binding'i finansal süreçlerin temel master data sınırıdır; hatalı kart açılışı sonraki fatura ve muhasebe ilişkilerini doğrudan etkiler.
