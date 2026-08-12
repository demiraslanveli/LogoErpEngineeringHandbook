# 120 — Ortak Mapper ve Validator Kütüphanesi

Bu bölüm, farklı Logo servislerinin aynı mapping ve validation kurallarını tekrar tekrar yazmasını engelleyen ortak kütüphane yaklaşımını tanımlar.

## Amaç

Malzeme, cari, sipariş, irsaliye/fatura ve üretim servislerinin tamamında aşağıdaki ihtiyaçlar ortaktır:

- zorunlu alan kontrolü
- string normalizasyonu
- tarih kontrolü
- firma / dönem bağlamı doğrulaması
- referans çözümleme
- kod -> LOGICALREF eşleştirme
- birim / ambar / proje kontrolü
- dto -> Logo field mapping
- hata mesajlarının standartlaştırılması

Bu işlemleri her servis içinde ayrı yazmak yerine ortak bir katmanda toplamak daha sürdürülebilirdir.

## Önerilen Katman

```text
LogoIntegration.Shared
│
├── Mapping
│   ├── IMapper<TSource, TTarget>
│   ├── ItemMapper
│   ├── ClientMapper
│   ├── OrderMapper
│   ├── InvoiceMapper
│   └── ProductionMapper
│
├── Validation
│   ├── IValidator<T>
│   ├── ValidationResult
│   ├── ValidationError
│   └── Validators
│
├── Normalization
│   ├── StringNormalizer
│   ├── CodeNormalizer
│   └── DateNormalizer
│
└── ReferenceResolution
    ├── IReferenceResolver
    └── LogoReferenceResolver
```

## ValidationResult

```csharp
public sealed class ValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public List<ValidationError> Errors { get; }
        = new List<ValidationError>();

    public void Add(string field, string message)
    {
        Errors.Add(new ValidationError
        {
            Field = field,
            Message = message
        });
    }
}

public sealed class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
}
```

## IValidator

```csharp
public interface IValidator<T>
{
    ValidationResult Validate(T model);
}
```

## Örnek

```csharp
public sealed class ItemCreateRequestValidator
    : IValidator<ItemCreateRequest>
{
    public ValidationResult Validate(ItemCreateRequest model)
    {
        var result = new ValidationResult();

        if (model == null)
        {
            result.Add("Request", "İstek boş olamaz.");
            return result;
        }

        if (string.IsNullOrWhiteSpace(model.Code))
            result.Add("Code", "Malzeme kodu zorunludur.");

        if (string.IsNullOrWhiteSpace(model.Name))
            result.Add("Name", "Malzeme açıklaması zorunludur.");

        if (string.IsNullOrWhiteSpace(model.MainUnitCode))
            result.Add("MainUnitCode", "Ana birim zorunludur.");

        return result;
    }
}
```

## Mapping Prensibi

Mapper katmanı iş kuralı çalıştırmamalıdır.

Doğru ayrım:

```text
Validator
    ↓
Reference Resolver
    ↓
Mapper
    ↓
Logo Adapter
```

Mapper yalnızca veriyi hedef nesneye taşır.

## Referans Çözümleme

Kodlarla gelen dış veri, Logo tarafında çoğu zaman LOGICALREF ile çalışır.

Örnekler:

```text
Cari Kodu      -> CLCARD.LOGICALREF
Malzeme Kodu   -> ITEMS.LOGICALREF
Birim Kodu     -> UNITSETL.LOGICALREF
Proje Kodu     -> PROJECT.LOGICALREF
```

Bu nedenle ortak bir resolver katmanı kullanılmalıdır.

```csharp
public interface IReferenceResolver
{
    int ResolveItemRef(string code);
    int ResolveClientRef(string code);
    int ResolveProjectRef(string code);
    int ResolveUnitRef(string code);
}
```

## Cache Kullanımı

Sık kullanılan sabit referanslar kontrollü cache'e alınabilir.

Örneğin:

- birimler
- ambarlar
- projeler
- işyerleri

Ancak cache için mutlaka TTL veya invalidation stratejisi tanımlanmalıdır.

## Anti-Pattern

Her servis içinde şu tip sorguları tekrar etmek:

```text
SELECT LOGICALREF FROM LG_XXX_ITEMS WHERE CODE = ...
```

Bu yaklaşım zamanla:

- duplicate kod
- farklı hata mesajları
- performans farkları
- yanlış firma bağlamı
- bakım maliyeti

oluşturur.

> Ortak mapper/validator/resolver katmanı, Logo entegrasyon framework'ünün tekrar kullanım oranını ciddi biçimde artırır.
