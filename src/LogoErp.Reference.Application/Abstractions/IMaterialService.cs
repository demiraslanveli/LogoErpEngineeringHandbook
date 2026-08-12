using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Abstractions
{
    public interface IMaterialService
    {
        OperationResult Create(string code, string name);
    }
}
