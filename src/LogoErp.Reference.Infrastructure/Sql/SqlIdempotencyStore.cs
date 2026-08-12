using System;
using System.Data;
using System.Data.SqlClient;
using LogoErp.Reference.Core.Abstractions;

namespace LogoErp.Reference.Infrastructure.Sql
{
    public sealed class SqlIdempotencyStore : IIdempotencyStore
    {
        private readonly string _connectionString;

        public SqlIdempotencyStore(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string boş olamaz.", nameof(connectionString));

            _connectionString = connectionString;
        }

        public bool Exists(string operationKey)
        {
            const string sql = @"
SELECT CASE WHEN EXISTS
(
    SELECT 1
    FROM dbo.INTEGRATION_IDEMPOTENCY
    WHERE OPERATION_KEY = @OperationKey
      AND STATUS = 'SUCCEEDED'
)
THEN 1 ELSE 0 END;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@OperationKey", SqlDbType.NVarChar, 200).Value = operationKey;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        public void MarkStarted(string operationKey, string operationType, string correlationId)
        {
            const string sql = @"
INSERT INTO dbo.INTEGRATION_IDEMPOTENCY
(
    OPERATION_KEY,
    OPERATION_TYPE,
    STATUS,
    CORRELATION_ID
)
VALUES
(
    @OperationKey,
    @OperationType,
    'STARTED',
    @CorrelationId
);";

            Execute(sql,
                new SqlParameter("@OperationKey", SqlDbType.NVarChar, 200) { Value = operationKey },
                new SqlParameter("@OperationType", SqlDbType.NVarChar, 100) { Value = operationType },
                new SqlParameter("@CorrelationId", SqlDbType.NVarChar, 100) { Value = (object)correlationId ?? DBNull.Value });
        }

        public void MarkSucceeded(string operationKey, string logoReference, string responsePayload = null)
        {
            const string sql = @"
UPDATE dbo.INTEGRATION_IDEMPOTENCY
SET STATUS = 'SUCCEEDED',
    LOGO_REFERENCE = @LogoReference,
    RESPONSE_PAYLOAD = @ResponsePayload,
    ERROR_CODE = NULL,
    ERROR_MESSAGE = NULL,
    UPDATED_AT = SYSUTCDATETIME()
WHERE OPERATION_KEY = @OperationKey;";

            Execute(sql,
                new SqlParameter("@OperationKey", SqlDbType.NVarChar, 200) { Value = operationKey },
                new SqlParameter("@LogoReference", SqlDbType.NVarChar, 100) { Value = (object)logoReference ?? DBNull.Value },
                new SqlParameter("@ResponsePayload", SqlDbType.NVarChar, -1) { Value = (object)responsePayload ?? DBNull.Value });
        }

        public void MarkFailed(string operationKey, string errorCode, string errorMessage)
        {
            const string sql = @"
UPDATE dbo.INTEGRATION_IDEMPOTENCY
SET STATUS = 'FAILED',
    ERROR_CODE = @ErrorCode,
    ERROR_MESSAGE = @ErrorMessage,
    UPDATED_AT = SYSUTCDATETIME()
WHERE OPERATION_KEY = @OperationKey;";

            Execute(sql,
                new SqlParameter("@OperationKey", SqlDbType.NVarChar, 200) { Value = operationKey },
                new SqlParameter("@ErrorCode", SqlDbType.NVarChar, 100) { Value = (object)errorCode ?? DBNull.Value },
                new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 2000) { Value = (object)errorMessage ?? DBNull.Value });
        }

        private void Execute(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
