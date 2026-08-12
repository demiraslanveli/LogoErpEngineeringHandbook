# 134 — Composition Root Gerçek Kod İskeleti

Bu bölüm uygulamanın dependency graph'inin gerçek C# kodunda nasıl kurulacağını gösterir.

## Amaç

Tüm bağımlılıkları tek noktada oluşturmak:

```text
Configuration
Logger
Repositories
LogoSessionFactory
Adapters
Services
Workers
Runtime
```

## Örnek

```csharp
public static class CompositionRoot
{
    public static IServiceRuntime Build()
    {
        var settings = AppSettingsLoader.Load();

        var logger = LoggerFactory.Create(settings.Logging);

        var sqlConnectionFactory = new SqlConnectionFactory(
            settings.Sql.ConnectionString);

        var idempotencyRepository = new SqlIdempotencyRepository(
            sqlConnectionFactory,
            logger);

        var reconciliationRepository = new SqlReconciliationRepository(
            sqlConnectionFactory,
            logger);

        var logoSessionFactory = new LogoSessionFactory(
            settings.Logo,
            logger);

        var materialService = new MaterialApplicationService(
            logoSessionFactory,
            idempotencyRepository,
            logger);

        var customerService = new CustomerApplicationService(
            logoSessionFactory,
            idempotencyRepository,
            logger);

        var orderService = new OrderApplicationService(
            logoSessionFactory,
            idempotencyRepository,
            reconciliationRepository,
            logger);

        var worker = new IntegrationWorker(
            materialService,
            customerService,
            orderService,
            logger,
            settings.Worker);

        return new ServiceRuntime(worker, logger);
    }
}
```

## Neden Service Locator kullanılmamalı?

Şu yaklaşım tercih edilmemelidir:

```csharp
var service = GlobalContainer.Resolve<IOrderService>();
```

Çünkü dependency'ler görünmez hale gelir.

Constructor injection daha nettir:

```csharp
public OrderApplicationService(
    ILogoSessionFactory sessionFactory,
    IIdempotencyRepository idempotencyRepository,
    IReconciliationRepository reconciliationRepository,
    ILogger logger)
{
    ...
}
```

## Lifetime kontrolü

Özellikle Logo Objects tarafında lifetime çok önemlidir.

Singleton olarak paylaşılması güvenli olduğu doğrulanmamış COM nesneleri global singleton yapılmamalıdır.

Öneri:

```text
Configuration        -> Singleton
Logger               -> Singleton
SQL ConnectionFactory-> Singleton
Repository           -> Stateless / Singleton olabilir
Logo Session         -> Operation / Worker scoped
IData / IQuery       -> Operation scoped
```

Logo Objects thread-safety davranışı ilgili sürümde doğrulanmadan eşzamanlı global session paylaşımı yapılmamalıdır.

> Composition Root, uygulama bağımlılıklarının tamamının görünür olduğu tek merkez olmalıdır.
