using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Data
{
    public sealed class UnconfiguredLogoDataObjectFactory : ILogoDataObjectFactory
    {
        public OperationResult Create(
            string dataObjectTypeKey,
            out ILogoDataObject dataObject)
        {
            dataObject = null;

            return OperationResult.Fail(
                "LOGO_IDATA_NOT_CONFIGURED",
                "Verified IData factory binding has not been configured for this Logo installation.");
        }
    }
}
