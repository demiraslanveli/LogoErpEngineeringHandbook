using System;
using System.Threading;
using LogoErp.Reference.Worker.Composition;
using LogoErp.Reference.Worker.Runtime;

namespace LogoErp.Reference.Worker
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var root = CompositionRoot.Build();
                var healthCheck = new HealthCheckRunner(root.Options);

                var health = healthCheck.CheckSql();
                if (!health.Success)
                {
                    Console.Error.WriteLine($"Health check failed: {health.ErrorCode} - {health.Message}");
                    return 2;
                }

                using (var cancellation = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (sender, eventArgs) =>
                    {
                        eventArgs.Cancel = true;
                        cancellation.Cancel();
                    };

                    var worker = new WorkerLoop(root.Options, RunIteration);
                    worker.Run(cancellation.Token);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static void RunIteration()
        {
            // Composition Root ilerleyen adımda gerçek application service'lerini burada çözecek.
            // Logo Objects / ProductionApplication session yaşam döngüsü iteration veya scoped unit sınırında yönetilecek.
            Console.WriteLine($"Worker iteration: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
    }
}
