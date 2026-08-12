using System;
using System.Threading;
using LogoErp.Reference.Infrastructure.Configuration;

namespace LogoErp.Reference.Worker.Runtime
{
    public sealed class WorkerLoop
    {
        private readonly LogoErpOptions _options;
        private readonly Action _iteration;

        public WorkerLoop(LogoErpOptions options, Action iteration)
        {
            _options = options;
            _iteration = iteration;
        }

        public void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _iteration();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Worker iteration failed: " + ex);
                }

                var delay = TimeSpan.FromSeconds(_options.WorkerIntervalSeconds <= 0 ? 60 : _options.WorkerIntervalSeconds);
                if (token.WaitHandle.WaitOne(delay))
                    break;
            }
        }
    }
}
