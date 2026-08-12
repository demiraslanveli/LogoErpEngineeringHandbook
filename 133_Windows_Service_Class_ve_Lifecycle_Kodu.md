# 133 — Windows Service Class ve Lifecycle Kodu

Bu bölüm Windows Service sınıfının gerçek sorumluluklarını tanımlar.

## Temel görevler

Windows Service sınıfı yalnızca yaşam döngüsünü yönetmelidir:

```text
OnStart
OnStop
OnShutdown
Dispose
```

İş kuralları worker ve application service katmanlarında kalmalıdır.

## Örnek servis sınıfı

```csharp
using System.ServiceProcess;

namespace LogoErpEngineering.ServiceHost
{
    public sealed class LogoIntegrationWindowsService : ServiceBase
    {
        private readonly IServiceRuntime _runtime;

        public LogoIntegrationWindowsService(IServiceRuntime runtime)
        {
            _runtime = runtime;
            ServiceName = "LogoErpIntegrationService";
            CanStop = true;
            CanShutdown = true;
        }

        protected override void OnStart(string[] args)
        {
            _runtime.Start();
        }

        protected override void OnStop()
        {
            _runtime.Stop();
        }

        protected override void OnShutdown()
        {
            _runtime.Stop();
            base.OnShutdown();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _runtime.Dispose();

            base.Dispose(disposing);
        }
    }
}
```

## IServiceRuntime

```csharp
using System;

public interface IServiceRuntime : IDisposable
{
    void Start();
    void Stop();
}
```

## Kapanış neden önemlidir?

Logo Objects / COM kullanan servislerde ani kapanış:

- açık Logo session'larının kalmasına,
- yarım batch durumlarına,
- logların flush edilmemesine,
- worker thread'lerinin kontrolsüz sonlanmasına

neden olabilir.

## Stop sırası

Önerilen sıra:

```text
Yeni iş alma durdurulur
↓
Aktif worker kontrollü tamamlanır
↓
Queue state kaydedilir
↓
Logo session dispose edilir
↓
Log sink flush edilir
↓
Service kapanır
```

> Windows Service yalnızca host'tur. Logo Objects nesnelerinin lifecycle yönetimi host sınıfına dağılmamalıdır.
