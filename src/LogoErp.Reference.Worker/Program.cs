using System;
using LogoErp.Reference.Core.Context;
using LogoErp.Reference.LogoAdapter.Session;

namespace LogoErp.Reference.Worker
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var context = LoadContext();

                using (var logoSession = new LogoSessionAdapter(context))
                {
                    logoSession.Open();

                    // TODO: Resolve application services from Composition Root.
                    // TODO: Run batch/background workflow.
                    // TODO: Write structured operation and health logs.

                    logoSession.Close();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static CompanyPeriodContext LoadContext()
        {
            // Example only. Production values must come from protected configuration.
            return new CompanyPeriodContext(
                companyNumber: 1,
                periodNumber: 1);
        }
    }
}
