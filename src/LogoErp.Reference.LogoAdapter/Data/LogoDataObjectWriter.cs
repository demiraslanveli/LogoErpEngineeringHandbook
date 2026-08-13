using System;
using System.Collections.Generic;
using LogoErp.Reference.Core.Results;
using LogoErp.Reference.LogoAdapter.Errors;

namespace LogoErp.Reference.LogoAdapter.Data
{
    public sealed class LogoDataObjectWriter
    {
        private readonly LogoAdapterErrorNormalizer _errors;

        public LogoDataObjectWriter(LogoAdapterErrorNormalizer errors = null)
        {
            _errors = errors ?? new LogoAdapterErrorNormalizer();
        }

        public OperationResult Write(
            ILogoDataObjectFactory factory,
            string dataObjectTypeKey,
            IReadOnlyDictionary<string, object> headerFields,
            Action<ILogoDataObject> configureLines = null,
            string operationName = null)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            try
            {
                var createResult = factory.Create(dataObjectTypeKey);
                if (!createResult.Success)
                    return OperationResult.Fail(createResult.Code, createResult.Message);

                var dataObject = createResult.Value;
                if (dataObject == null)
                    return OperationResult.Failure("LOGO_DATA_OBJECT_NULL", "Logo IData wrapper returned null.");

                if (headerFields != null)
                {
                    foreach (var field in headerFields)
                        dataObject.SetField(field.Key, field.Value);
                }

                configureLines?.Invoke(dataObject);

                if (!dataObject.Post())
                {
                    return _errors.FromSdkFailure(
                        operationName ?? dataObjectTypeKey,
                        dataObject.ErrorCode,
                        dataObject.ErrorDescription);
                }

                return OperationResult.Ok(operationName ?? dataObjectTypeKey);
            }
            catch (Exception ex)
            {
                return _errors.FromException(operationName ?? dataObjectTypeKey, ex);
            }
        }
    }
}
