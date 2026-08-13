# 145 — Logo Hata Normalizasyonu ve Ortak IData Writer

Logo Objects adapter'larında aynı kayıt akışını her gateway içinde tekrar etmek bakım maliyetini artırır.

Tipik tekrar:

```text
Create IData
Set Header Fields
Append Lines
Set Line Fields
Post
Read ErrorCode
Read ErrorDescription
Convert Result
```

Referans uygulamada bu işlem ortaklaştırılmıştır.

## İlgili Sınıflar

```text
LogoDataObjectWriter
LogoLineWriter
LogoAdapterErrorNormalizer
```

## LogoDataObjectWriter

Sorumlulukları:

1. `ILogoDataObjectFactory` ile nesne oluşturmak,
2. header alanlarını set etmek,
3. gerekiyorsa line callback çalıştırmak,
4. `Post()` çağırmak,
5. başarısız durumda SDK hata kodu/açıklamasını normalize etmek,
6. exception'ı ortak hata formatına çevirmek.

Bu sayede gateway'in görevi yalnızca mapping üretmek olur.

```text
Gateway
  ↓
Mapping Profile
  ↓
LogoDataObjectWriter
  ↓
ILogoDataObjectFactory
  ↓
Verified IData Wrapper
```

## LogoLineWriter

Belge satırlarında ortak davranış:

```text
AppendLine(collection)
SetField(...)
SetField(...)
```

tek yardımcı üzerinden yapılır.

Satır koleksiyonunun gerçek SDK adı mapping profile tarafından belirlenmelidir.

## Hata Normalizasyonu

Logo SDK hata bilgisi application katmanına ham COM nesnesi olarak taşınmaz.

Normalize edilen sonuç örneği:

```text
Code: LOGO_<SDK_CODE>
Message: <SDK Error Description>
Metadata:
  operation
  sdk_error_code
  sdk_error_description
```

Exception durumunda:

```text
LOGO_ADAPTER_EXCEPTION
```

kullanılır ve exception type metadata olarak tutulur.

## Correlation ID

Üretim kullanımında writer çağrısı application-level correlation id ile genişletilmelidir. Böylece:

```text
External Event
  ↓ correlationId
Application Service
  ↓
Gateway
  ↓
Logo Adapter
  ↓
OperationResult
```

zinciri uçtan uca izlenebilir.

## Sonuç

Gateway sınıflarında business mapping ile SDK plumbing birbirinden ayrılır.

Bu ayrım özellikle malzeme, cari, sipariş, irsaliye ve fatura adapter'larının aynı hata ve kayıt standardını kullanmasını sağlar.

> Tekrarlanan SDK plumbing kodu ortaklaştırılmalı; belgeye özgü iş kuralları ve mapping ise gateway/profile katmanında kalmalıdır.
