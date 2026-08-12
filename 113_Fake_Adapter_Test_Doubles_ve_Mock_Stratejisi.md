# 113 — Fake Adapter, Test Doubles ve Mock Stratejisi

Logo Objects COM bağımlılığı unit testleri zorlaştırabilir. Test edilebilir mimarinin amacı uygulama servislerini gerçek Logo oturumu açmadan sınayabilmektir.

## Temel İlke

Application katmanı doğrudan `UnityApplication` veya `IData` tiplerine bağımlı olmamalıdır.

Bunun yerine kendi abstraction katmanımız kullanılmalıdır.

Örnek:

```csharp
public interface ILogoDataGateway
{
    LogoOperationResult CreateItem(ItemRequest request, LogoContext context);
}
```

Gerçek implementasyon:

```text
LogoDataGateway
   ↓
IApplication / IData
```

Test implementasyonu:

```text
FakeLogoDataGateway
```

## Test Double Türleri

### Stub

Belirli girdiye sabit cevap döner.

### Fake

Basitleştirilmiş çalışan implementasyondur.

Örnek:

```text
InMemoryIdempotencyStore
FakeLogoItemRepository
FakeQueueRepository
```

### Mock

Belirli çağrıların yapılıp yapılmadığını doğrulamak için kullanılır.

### Spy

Çağrıları kaydeder ve test sonunda incelenir.

## Fake Logo Repository Örneği

```csharp
public sealed class FakeItemRepository : IItemRepository
{
    private readonly Dictionary<string, ItemModel> _items =
        new Dictionary<string, ItemModel>();

    public bool Exists(string code)
    {
        return _items.ContainsKey(code);
    }

    public LogoOperationResult Create(ItemModel item)
    {
        if (Exists(item.Code))
            return LogoOperationResult.Fail("ITEM_EXISTS", "Malzeme zaten mevcut.");

        _items[item.Code] = item;
        return LogoOperationResult.Ok();
    }
}
```

## Neler Unit Test Edilebilir?

Gerçek Logo olmadan:

- validation kuralları
- mapping
- idempotency
- retry kararı
- batch orchestration
- error classification
- application service akışı
- correlation id yayılımı

unit test edilebilir.

## Neler Integration Test Gerektirir?

- gerçek `IData.Post()` davranışı
- Logo enum/field uyumu
- referans çözümleme
- seri/lot davranışı
- ProductionApplication çağrıları
- gerçek şirket/dönem context'i

## Test Piramidi

```text
           End-to-End
              ▲
        Integration Tests
              ▲
           Unit Tests
```

En fazla test unit seviyesinde olmalıdır.

## COM Tiplerini Domain'e Sızdırmama

Yanlış:

```csharp
public IData CreateInvoice(...)
```

Daha iyi:

```csharp
public LogoOperationResult CreateInvoice(InvoiceRequest request)
```

Böylece application/domain katmanı Logo COM tiplerinden bağımsız kalır.

## Fake Session Factory

```csharp
public interface ILogoSessionFactory
{
    ILogoSession Create(LogoContext context);
}
```

Testte:

```text
FakeLogoSessionFactory
```

kullanılarak login ve session lifecycle senaryoları simüle edilebilir.

## Hata Senaryolarını Test Etme

Fake adapter şu durumları bilinçli üretebilmelidir:

- login failure
- validation failure
- transient error
- duplicate document
- post failure
- timeout simulation

Bu sayede retry ve error handling kodu gerçek arızayı beklemeden test edilir.

> Test edilebilir Logo entegrasyonu, Logo Objects'i mock etmeye çalışmakla değil; kendi adapter sınırını doğru çizmekle başlar.
