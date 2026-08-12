using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.IntegrationTests.Fakes
{
    public sealed class FakeLogoMaterialGateway : ILogoMaterialGateway
    {
        public string LastCode { get; private set; }
        public string LastName { get; private set; }

        public OperationResult Create(string code, string name)
        {
            LastCode = code;
            LastName = name;
            return OperationResult.Success();
        }
    }
}
