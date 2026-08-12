using System;
using LogoErp.Reference.Core.Configuration;

namespace LogoErp.Reference.Infrastructure.Configuration
{
    public sealed class EnvironmentConfigurationLoader
    {
        public LogoErpOptions Load()
        {
            var options = new LogoErpOptions
            {
                FirmNumber = ReadRequiredInt("LOGOERP_FIRM_NUMBER"),
                PeriodNumber = ReadRequiredInt("LOGOERP_PERIOD_NUMBER"),
                LogoUserName = ReadRequired("LOGOERP_USER"),
                LogoPassword = ReadRequired("LOGOERP_PASSWORD"),
                SqlConnectionString = ReadRequired("LOGOERP_SQL"),
                WorkerIntervalSeconds = ReadOptionalInt("LOGOERP_WORKER_INTERVAL_SECONDS", 30)
            };

            options.Validate();
            return options;
        }

        private static string ReadRequired(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException("Required environment variable is missing: " + name);

            return value;
        }

        private static int ReadRequiredInt(string name)
        {
            int value;
            if (!int.TryParse(ReadRequired(name), out value))
                throw new InvalidOperationException("Environment variable must be an integer: " + name);

            return value;
        }

        private static int ReadOptionalInt(string name, int defaultValue)
        {
            var raw = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(raw))
                return defaultValue;

            int value;
            if (!int.TryParse(raw, out value))
                throw new InvalidOperationException("Environment variable must be an integer: " + name);

            return value;
        }
    }
}
