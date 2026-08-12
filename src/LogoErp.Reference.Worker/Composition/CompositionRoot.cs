using System;
using LogoErp.Reference.Core.Configuration;
using LogoErp.Reference.Core.Context;
using LogoErp.Reference.Infrastructure.Configuration;
using LogoErp.Reference.LogoAdapter.Session;

namespace LogoErp.Reference.Worker.Composition
{
    public sealed class CompositionRoot
    {
        public LogoErpOptions Options { get; }

        public CompositionRoot(LogoErpOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public static CompositionRoot Build()
        {
            var options = EnvironmentConfigurationLoader.Load();
            options.Validate();
            return new CompositionRoot(options);
        }

        public LogoSessionAdapter CreateLogoSession()
        {
            var context = new CompanyPeriodContext(
                Options.FirmNumber,
                Options.PeriodNumber);

            // Safe default. Replace this bridge only after the exact target
            // Logo Objects / UnityApplication binding has been verified.
            ILogoSdkBridge bridge = new UnconfiguredLogoSdkBridge();

            return new LogoSessionAdapter(
                context,
                Options,
                bridge);
        }
    }
}
