using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.IntegrationTests.Fakes
{
    public sealed class FakeCustomerGateway : ICustomerGateway
    {
        public string LastCode { get; private set; }
        public string LastTitle { get; private set; }

        public OperationResult Create(string code, string title, string taxNumber, string taxOffice)
        {
            LastCode = code;
            LastTitle = title;
            return OperationResult.Success();
        }
    }
}
