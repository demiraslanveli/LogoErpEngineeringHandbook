# 124 — Composition Root ve Dependency Wiring

Bu bölüm, referans entegrasyon uygulamasındaki tüm bağımlılıkların tek noktada oluşturulması ve bağlanması için kullanılacak composition root yaklaşımını tanımlar.

## Neden Composition Root?

Servis sınıflarının kendi bağımlılıklarını oluşturması şu sorunlara yol açar:

- sıkı bağımlılık
- test edilebilirliğin azalması
- farklı implementasyonlara geçiş zorluğu
- configuration dağınıklığı
- lifecycle yönetim problemleri

Bu nedenle object graph tek bir başlangıç noktasında kurulmalıdır.

## Hedef Akış

```text
Program / Windows Service
        ↓
Bootstrapper
        ↓
Configuration
        ↓
Infrastructure
        ↓
Logo Adapter
        ↓
Repositories
        ↓
Application Services
        ↓
Workers
```

## Örnek Bootstrapper

```csharp
public static class Bootstrapper
{
    public static ServiceContainer Build()
    {
        var config = AppConfigurationLoader.Load();

        var logger = new StructuredLogger(config.Logging);

        var sqlConnectionFactory =
            new SqlConnectionFactory(config.SqlConnectionString);

        var idempotencyRepository =
            new SqlIdempotencyRepository(sqlConnectionFactory);

        var reconciliationRepository =
            new SqlReconciliationRepository(sqlConnectionFactory);

        var sessionFactory =
            new LogoSessionFactory(config.Logo, logger);

        var queryAdapter =
            new LogoQueryAdapter(sessionFactory, logger);

        var dataAdapter =
            new LogoDataAdapter(sessionFactory, logger);

        var referenceResolver =
            new LogoReferenceResolver(queryAdapter);

        var itemService =
            new ItemApplicationService(
                dataAdapter,
                referenceResolver,
                idempotencyRepository,
                logger);

        var worker =
            new IntegrationBackgroundWorker(
                itemService,
                reconciliationRepository,
                logger,
                config.Worker);

        return new ServiceContainer(worker);
    }
}
```

Bu yalnızca mimari örnektir. Kullanılan gerçek sınıf ve dependency seti projeye göre değişir.

## Lifetime Yönetimi

Her dependency aynı lifetime ile yaşatılmamalıdır.

Örnek:

```text
Configuration            Singleton
Logger                   Singleton
SQL Connection           Operation scoped
Logo Session             Operation / worker scoped
Application Service      Stateless / reusable
Repository               Stateless veya scoped
Request Context          Operation scoped
Correlation Context      Operation scoped
```

Logo Objects COM nesnelerinin lifetime davranışı ayrıca test edilmelidir.

## Service Locator Kullanmayın

Anti-pattern:

```csharp
public void CreateOrder()
{
    var repo = GlobalContainer.Resolve<IOrderRepository>();
}
```

Tercih:

```csharp
public OrderService(IOrderRepository repository)
{
    _repository = repository;
}
```

Bağımlılıklar constructor üzerinden görünür olmalıdır.

## Environment Bazlı Wiring

Test ve production ortamı farklı adapter kullanabilir.

```text
Production
    ILogoDataAdapter -> LogoDataAdapter

Unit Test
    ILogoDataAdapter -> FakeLogoDataAdapter

Integration Test
    ILogoDataAdapter -> TestEnvironmentLogoDataAdapter
```

## Configuration Validation

Object graph kurulmadan önce configuration doğrulanmalıdır.

```text
SQL connection string var mı?
Logo kullanıcı bilgileri var mı?
Firma/dönem tanımlı mı?
Worker interval geçerli mi?
Log path erişilebilir mi?
```

## Startup Fail-Fast

Kalıcı konfigürasyon hatasında servis fail-fast davranabilir.

Örneğin:

```text
CompanyNo = 0
PeriodNo tanımsız
Connection string boş
```

Ancak geçici network sorunu ile configuration hatası birbirinden ayrılmalıdır.

## Composition Root Testi

En az bir test, tüm object graph'ın kurulabildiğini doğrulamalıdır.

```csharp
[Test]
public void Bootstrapper_Should_Build_ServiceGraph()
{
    var container = Bootstrapper.Build();

    Assert.IsNotNull(container);
}
```

## Prensip

```text
new kullanımı yasak değildir.

Dağınık new kullanımı problemdir.
```

`new` çağrılarının merkezi composition root içinde olması mimariyi sadeleştirir.

> Composition root, uygulamanın teknik bileşenlerini birbirine bağlayan tek merkez olmalıdır.
