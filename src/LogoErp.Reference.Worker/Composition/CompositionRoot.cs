using LogoErp.Reference.Infrastructure.Configuration;

namespace LogoErp.Reference.Worker.Composition
{
    public sealed class CompositionRoot
    {
        public LogoErpOptions Options { get; }

        public CompositionRoot(LogoErpOptions options)
        {
            Options = options;
        }

        public static CompositionRoot Build()
        {
            var options = EnvironmentConfigurationLoader.Load();
            return new CompositionRoot(options);
        }
    }
}
