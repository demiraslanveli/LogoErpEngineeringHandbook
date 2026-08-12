using System;
using System.Data.SqlClient;
using LogoErp.Reference.Core.Results;
using LogoErp.Reference.Infrastructure.Configuration;

namespace LogoErp.Reference.Worker.Runtime
{
    public sealed class HealthCheckRunner
    {
        private readonly LogoErpOptions _options;

        public HealthCheckRunner(LogoErpOptions options)
        {
            _options = options;
        }

        public OperationResult CheckSql()
        {
            if (string.IsNullOrWhiteSpace(_options.SqlConnectionString))
                return OperationResult.Fail("SQL_CONNECTION_REQUIRED", "SQL connection string bulunamadı.");

            try
            {
                using (var connection = new SqlConnection(_options.SqlConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        command.ExecuteScalar();
                    }
                }

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                return OperationResult.Fail("SQL_HEALTHCHECK_FAILED", ex.Message);
            }
        }
    }
}
