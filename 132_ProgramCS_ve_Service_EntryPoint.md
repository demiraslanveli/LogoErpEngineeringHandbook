# 132 — Program.cs ve Service Entry Point

Bu bölüm referans entegrasyon uygulamasının gerçek başlangıç noktasını tanımlar.

Amaç `Main()` metodunda iş kuralı yazmak değil; uygulamayı doğru host modunda başlatmak, dependency graph'i kurmak ve kapanışı kontrollü yönetmektir.

## Temel yaklaşım

```text
Program.Main
    ↓
Configuration Load
    ↓
CompositionRoot.Build()
    ↓
Console veya Windows Service Host
    ↓
Worker.Start()
```

## Örnek Program.cs

```csharp
using System;
using System.ServiceProcess;
using LogoErpEngineering.ServiceHost;

namespace LogoErpEngineering.Service
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var runtime = CompositionRoot.Build();

            if (Environment.UserInteractive)
            {
                Console.WriteLine("Logo ERP Integration Service starting...");

                runtime.Start();

                Console.WriteLine("Press ENTER to stop.");
                Console.ReadLine();

                runtime.Stop();
                runtime.Dispose();
                return;
            }

            ServiceBase.Run(new LogoIntegrationWindowsService(runtime));
        }
    }
}
```

## Neden UserInteractive kontrolü?

Aynı binary'nin:

- geliştirici bilgisayarında console olarak,
- sunucuda Windows Service olarak

çalıştırılabilmesini sağlar.

Bu yaklaşım servis debug sürecini ciddi şekilde kolaylaştırır.

## Main içerisinde olmaması gerekenler

```text
Logo login kodu
IData oluşturma
SQL sorgusu
malzeme oluşturma
sipariş aktarma
retry döngüsü
Timer iş mantığı
```

Bunların tamamı alt katmanlarda bulunmalıdır.

## Kritik prensip

`Program.cs` uygulamanın wiring ve lifecycle giriş noktasıdır; ERP iş kurallarının bulunduğu yer değildir.

> Entry point mümkün olduğunca küçük, deterministik ve test edilebilir tutulmalıdır.
