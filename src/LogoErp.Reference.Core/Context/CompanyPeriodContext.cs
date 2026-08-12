using System;

namespace LogoErp.Reference.Core.Context
{
    public sealed class CompanyPeriodContext
    {
        public int FirmNumber { get; }
        public int PeriodNumber { get; }

        public CompanyPeriodContext(int firmNumber, int periodNumber)
        {
            if (firmNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(firmNumber));

            if (periodNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(periodNumber));

            FirmNumber = firmNumber;
            PeriodNumber = periodNumber;
        }

        public string FirmCode => FirmNumber.ToString("000");
        public string PeriodCode => PeriodNumber.ToString("00");

        public override string ToString()
        {
            return $"Firm={FirmCode}, Period={PeriodCode}";
        }
    }
}
