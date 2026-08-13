# 147 — Verified ILogoDataObjectFactory COM Wrapper

Bu bölüm `ILogoDataObjectFactory` sözleşmesinin gerçek Logo Objects `IData` nesnelerine bağlanma standardını tanımlar.

## Hedef Akış

```text
Gateway
  ↓
ILogoDataObjectFactory
  ↓
VerifiedLogoDataObjectFactory
  ↓
IApplication.NewDataObject(...)
  ↓
IData Wrapper
```

Factory'nin görevi `DataObjectType` anahtarını doğrulanmış SDK enum değerine çevirmek, `IData` oluşturmak ve onu `ILogoDataObject` adaptörü ile sarmalamaktır.

## Temel Kurallar

- enum değerleri kod içine rastgele yazılmaz,
- `LogoSdkBindingManifest` üzerinden çözülür,
- oluşturulan COM nesnesinin sahibi bellidir,
- başarısız object creation kontrollü `OperationResult` üretir,
- `IData` wrapper disposal sorumluluğunu taşır.

## Önerilen Mapping

```text
MATERIAL_CARD     → verified DataObjectType
CUSTOMER_CARD     → verified DataObjectType
SALES_ORDER       → verified DataObjectType
DISPATCH          → verified DataObjectType
SALES_INVOICE     → verified DataObjectType
```

Gerçek sayısal enum değerleri hedef SDK doğrulanmadan handbook'a sabitlenmez.

## IData Wrapper

Wrapper aşağıdaki yetenekleri sunmalıdır:

```csharp
SetField(string fieldName, object value)
AppendLine(string collectionName)
Post()
ErrorCode
ErrorDescription
Dispose()
```

## Hata Sınıfları

```text
LOGO_DATAOBJECT_TYPE_NOT_MAPPED
LOGO_DATAOBJECT_CREATE_FAILED
LOGO_DATAOBJECT_FIELD_NOT_FOUND
LOGO_DATAOBJECT_POST_FAILED
LOGO_DATAOBJECT_RELEASE_FAILED
```

## Kabul Testleri

- her desteklenen object type create edilebilmeli,
- bilinmeyen key fail-fast olmalı,
- field set sırasında COM exception normalize edilmeli,
- post hatasında Logo hata açıklaması korunmalı,
- dispose sonrası COM referansı bırakılmalı.

> Factory katmanı, application kodunun Logo SDK enum ve COM detaylarından tamamen bağımsız kalmasını sağlar.
