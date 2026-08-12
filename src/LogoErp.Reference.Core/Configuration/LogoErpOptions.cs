namespace LogoErp.Reference.Core.Configuration
{
    public sealed class LogoErpOptions
    {
        public int FirmNumber { get; set; }
        public int PeriodNumber { get; set; }
        public string LogoUserName { get; set; }
        public string LogoPassword { get; set; }
        public string SqlConnectionString { get; set; }
        public int WorkerIntervalSeconds { get; set; } = 30;

        public void Validate()
        {
            if (FirmNumber <= 0)
                throw new System.InvalidOperationException("FirmNumber must be greater than zero.");

            if (PeriodNumber <= 0)
                throw new System.InvalidOperationException("PeriodNumber must be greater than zero.");

            if (string.IsNullOrWhiteSpace(LogoUserName))
                throw new System.InvalidOperationException("LogoUserName is required.");

            if (string.IsNullOrWhiteSpace(LogoPassword))
                throw new System.InvalidOperationException("LogoPassword is required.");

            if (string.IsNullOrWhiteSpace(SqlConnectionString))
                throw new System.InvalidOperationException("SqlConnectionString is required.");

            if (WorkerIntervalSeconds <= 0)
                throw new System.InvalidOperationException("WorkerIntervalSeconds must be greater than zero.");
        }
    }
}
