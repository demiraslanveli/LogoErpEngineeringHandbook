# 127 — Örnek Interface ve Class Sözleşmeleri

Bu bölüm referans solution içindeki temel interface ve class sınırlarını tanımlar.

Amaç Logo Objects çağrılarını servis katmanından ayırmak ve test edilebilir bir mimari oluşturmaktır.

## ILogoSessionFactory

```csharp
public interface ILogoSessionFactory
{
    ILogoSession Open(LogoCompanyPeriodContext context);
}
```

## ILogoSession

```csharp
public interface ILogoSession : IDisposable
{
    bool IsConnected { get; }
    LogoCompanyPeriodContext Context { get; }

    object NativeApplication { get; }
}
```

`NativeApplication` yalnızca adapter katmanında kullanılmalıdır. Application katmanına taşınmamalıdır.

## ILogoDataAdapter

```csharp
public interface ILogoDataAdapter
{
    LogoOperationResult Create(
        LogoCompanyPeriodContext context,
        LogoObjectRequest request);

    LogoOperationResult Update(
        LogoCompanyPeriodContext context,
        LogoObjectRequest request);
}
```

`LogoObjectRequest`, Logo'nun gerçek `IData` nesnesi değildir. Uygulamanın kendi taşıma modelidir.

## ILogoQueryService

```csharp
public interface ILogoQueryService
{
    T QuerySingle<T>(
        LogoCompanyPeriodContext context,
        string queryKey,
        object parameters);

    IReadOnlyList<T> QueryList<T>(
        LogoCompanyPeriodContext context,
        string queryKey,
        object parameters);
}
```

SQL metninin business service içinde dağılması yerine `queryKey` yaklaşımı kullanılabilir.

## IIdempotencyStore

```csharp
public interface IIdempotencyStore
{
    IdempotencyRecord Get(string operationKey);
    bool TryStart(string operationKey, string correlationId);
    void MarkCompleted(string operationKey, string logoReference);
    void MarkFailed(string operationKey, string errorCode, string errorMessage);
}
```

## IReconciliationRepository

```csharp
public interface IReconciliationRepository
{
    void Add(ReconciliationRecord record);
    IReadOnlyList<ReconciliationRecord> GetPending(int take);
    void MarkResolved(long id, string resolutionNote);
}
```

## IIntegrationLogger

```csharp
public interface IIntegrationLogger
{
    void Info(string eventName, object data);
    void Warn(string eventName, object data);
    void Error(string eventName, Exception exception, object data);
}
```

## IClock

Tarih/saat bağımlılığını test edilebilir hale getirir.

```csharp
public interface IClock
{
    DateTime Now { get; }
}
```

## IServiceResult

Tüm application servislerinin aynı sonuç standardını kullanması önerilir.

```csharp
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string CorrelationId { get; set; }
}
```

## Application Service Örneği

```csharp
public interface IItemService
{
    ServiceResult<ItemResult> Create(ItemCreateRequest request);
    ServiceResult<ItemResult> Update(ItemUpdateRequest request);
}
```

## Repository Sınırı

Logo üzerinde gerçek kart/fiş yazan repository ile uygulamanın kendi SQL tablolarını yöneten repository aynı şey değildir.

Önerilen ayrım:

```text
LogoItemRepository
LogoCustomerRepository
LogoOrderRepository

SqlIdempotencyRepository
SqlReconciliationRepository
SqlAuditRepository
```

## Composition Prensibi

```text
Application Service
    ↓ interface
Repository / Adapter
    ↓
Logo Objects veya SQL
```

## Önemli Not

Bu bölümdeki interface adları referans mimari önerisidir. Logo Objects ürününün resmi interface isimleriyle karıştırılmamalıdır.

Logo SDK'ya ait gerçek `IApplication`, `IData`, `IQuery` ve benzeri tipler yalnızca LogoAdapter implementasyonunda kullanılmalıdır.
