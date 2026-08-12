using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Abstractions
{
    public interface ILogoMaterialGateway
    {
        OperationResult CreateMaterial(string code, string name);
    }
}
