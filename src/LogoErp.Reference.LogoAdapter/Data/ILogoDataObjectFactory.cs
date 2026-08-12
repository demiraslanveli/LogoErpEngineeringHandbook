using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Data
{
    public interface ILogoDataObjectFactory
    {
        OperationResult<ILogoDataObject> Create(string dataObjectTypeKey);
    }
}
