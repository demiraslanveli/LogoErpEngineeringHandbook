# 123 — Windows Service Host ve Runtime Yönetimi

Bu bölüm, .NET Framework 4.8 tabanlı Logo entegrasyon servisinin Windows Service olarak nasıl host edilmesi gerektiğini tanımlar.

## Amaç

Servis host katmanı iş kuralı içermez.

Görevleri:

- process lifecycle
- start / stop
- configuration yükleme
- dependency composition
- worker başlatma
- graceful shutdown
- health state
- log bootstrap
- unhandled exception yönetimi

## Katman Ayrımı

```text
Windows Service Host
        ↓
Composition Root
        ↓
Background Worker
        ↓
Application Services
        ↓
Logo Adapter
```

Host katmanının doğrudan `IData` veya `IQuery` kullanması önerilmez.

## ServiceBase Örneği

```csharp
public sealed class LogoIntegrationWindowsService : ServiceBase
{
    private IBackgroundWorker _worker;

    protected override void OnStart(string[] args)
    {
        var container = Bootstrapper.Build();

        _worker = container.Resolve<IBackgroundWorker>();
        _worker.Start();
    }

    protected override void OnStop()
    {
        if (_worker == null)
            return;

        _worker.Stop();
        _worker.Dispose();
    }
}
```

Kullanılan DI container veya factory yaklaşımı projeye göre değişebilir.

## Graceful Shutdown

Servis kapanırken aktif işlem aniden kesilmemelidir.

Önerilen akış:

```text
STOP sinyali
    ↓
Yeni iş alma kapatılır
    ↓
Aktif iş için cancellation sinyali
    ↓
Belirlenen shutdown politikası
    ↓
Logo session cleanup
    ↓
Log flush
    ↓
Process kapanır
```

## Cancellation

.NET Framework projelerinde CancellationToken kontrollü şekilde kullanılabilir.

```csharp
while (!token.IsCancellationRequested)
{
    ProcessNextBatch(token);
}
```

Logo Objects çağrılarının gerçekten cancellation destekleyip desteklemediği varsayılmamalıdır.

Bu nedenle cancellation genellikle yeni Logo operasyonu başlatmamak için kullanılmalıdır.

## Startup Kontrolleri

Servis açılışında aşağıdaki kontroller yapılabilir:

```text
Configuration okunabiliyor mu?
SQL bağlantısı var mı?
Logo runtime erişilebilir mi?
Gerekli klasör izinleri var mı?
Queue tablolarına erişim var mı?
```

Ancak geçici dependency hatasında servis tamamen kapanmak yerine degraded mod + retry stratejisi kullanabilir.

## Unhandled Exception

Global exception handler yalnızca son savunma hattıdır.

Örnek log alanları:

```text
Application
Machine
ServiceVersion
Company
Period
CorrelationId
ThreadId
ExceptionType
Message
StackTrace
```

## Watchdog

Worker heartbeat üretebilir.

Örnek tablo:

```text
ServiceName
MachineName
InstanceId
LastHeartbeatAt
CurrentState
CurrentOperation
Version
```

Bu sayede servis process olarak açık olsa bile iş üretmiyorsa fark edilebilir.

## Çoklu Instance

Aynı queue üzerinde birden fazla servis instance çalışacaksa:

- atomic claim
- lease timeout
- duplicate prevention
- worker instance id

zorunludur.

## Servis Hesabı

Windows Service mümkünse özel bir service account ile çalıştırılmalıdır.

Hesap için yalnızca gerekli yetkiler verilmelidir:

- Log on as a service
- gerekli SQL erişimi
- gerekiyorsa Logo runtime erişimi
- belirli klasör read/write izinleri

Local Administrator ile çalıştırmak varsayılan çözüm olmamalıdır.

## Deployment

Minimum release paketi:

```text
Service executable
Required assemblies
Logo integration dependencies
Configuration template
Install script
Uninstall script
Version information
Release notes
```

## Sürüm Bilgisi

Her log kaydında uygulama sürümü bulunması teşhis için çok değerlidir.

```text
ServiceVersion = 2.4.17
```

> Windows Service host, entegrasyon kodunun çalıştığı kabuktur; ERP iş kuralları host katmanından bağımsız tutulmalıdır.
