using System;
using System.Collections.Generic;

namespace LogoErp.Reference.Core.Results
{
    public sealed class OperationResult
    {
        public bool Success { get; private set; }
        public string Code { get; private set; }
        public string ErrorCode => Code;
        public string Message { get; private set; }
        public string CorrelationId { get; private set; }
        public IReadOnlyDictionary<string, string> Metadata { get; private set; }

        private OperationResult(
            bool success,
            string code,
            string message,
            string correlationId,
            IDictionary<string, string> metadata)
        {
            Success = success;
            Code = code;
            Message = message;
            CorrelationId = correlationId;
            Metadata = new Dictionary<string, string>(metadata ?? new Dictionary<string, string>());
        }

        public static OperationResult Ok(
            string message = "OK",
            string correlationId = null,
            IDictionary<string, string> metadata = null)
        {
            return new OperationResult(true, null, message, correlationId, metadata);
        }

        public static OperationResult Fail(
            string code,
            string message,
            string correlationId = null,
            IDictionary<string, string> metadata = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Hata kodu boş olamaz.", nameof(code));

            return new OperationResult(false, code, message, correlationId, metadata);
        }

        public static OperationResult Fail(string message)
        {
            return Fail("OPERATION_FAILED", message);
        }

        public static OperationResult Failure(string code, string message)
        {
            return Fail(code, message);
        }
    }
}
