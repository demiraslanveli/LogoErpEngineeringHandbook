using System;

namespace LogoErp.Reference.Core.Abstractions
{
    public interface IIdempotencyStore
    {
        bool Exists(string operationKey);
        void MarkStarted(string operationKey, string operationType, string correlationId);
        void MarkSucceeded(string operationKey, string logoReference, string responsePayload = null);
        void MarkFailed(string operationKey, string errorCode, string errorMessage);
    }
}
