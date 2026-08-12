using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Data
{
    public interface ILogoDataObjectFactory
    {
        OperationResult Create(
            string dataObjectTypeKey,
            out ILogoDataObject dataObject);
    }
}
