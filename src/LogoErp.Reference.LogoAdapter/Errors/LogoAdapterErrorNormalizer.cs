using System;
using System.Collections.Generic;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Errors
{
    public sealed class LogoAdapterErrorNormalizer
    {
        public OperationResult FromSdkFailure(
            string operation,
            string sdkErrorCode,
            string sdkErrorDescription,
            string correlationId = null)
        {
            var code = string.IsNullOrWhiteSpace(sdkErrorCode)
                ? "LOGO_SDK_ERROR"
                : $"LOGO_{sdkErrorCode.Trim()}";

            var message = string.IsNullOrWhiteSpace(sdkErrorDescription)
                ? $"Logo SDK operation failed: {operation}."
                : sdkErrorDescription.Trim();

            var metadata = new Dictionary<string, string>
            {
                ["operation"] = operation ?? string.Empty,
                ["sdk_error_code"] = sdkErrorCode ?? string.Empty,
                ["sdk_error_description"] = sdkErrorDescription ?? string.Empty
            };

            return OperationResult.Fail(code, message, correlationId, metadata);
        }

        public OperationResult FromException(string operation, Exception exception, string correlationId = null)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var metadata = new Dictionary<string, string>
            {
                ["operation"] = operation ?? string.Empty,
                ["exception_type"] = exception.GetType().FullName ?? exception.GetType().Name
            };

            return OperationResult.Fail(
                "LOGO_ADAPTER_EXCEPTION",
                exception.Message,
                correlationId,
                metadata);
        }
    }
}
