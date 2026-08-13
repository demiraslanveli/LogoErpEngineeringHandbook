using System;
using System.Collections.Generic;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Binding
{
    public sealed class LogoSdkCompatibilityChecker
    {
        public OperationResult Validate(LogoSdkBindingManifest manifest, IEnumerable<string> requiredKeys)
        {
            if (manifest == null)
                return OperationResult.Failure("SDK_BINDING_MISSING", "Logo SDK binding manifest is missing.");

            if (string.IsNullOrWhiteSpace(manifest.SdkVersion))
                return OperationResult.Failure("SDK_VERSION_MISSING", "Logo SDK version must be recorded in the binding manifest.");

            if (requiredKeys != null)
            {
                foreach (var key in requiredKeys)
                {
                    if (!manifest.TryGet(key, out var value) || string.IsNullOrWhiteSpace(value))
                    {
                        return OperationResult.Failure(
                            "SDK_BINDING_INCOMPLETE",
                            $"Required Logo SDK binding is missing: {key}");
                    }
                }
            }

            return OperationResult.Ok($"Logo SDK binding manifest is valid for version {manifest.SdkVersion}.");
        }
    }
}
