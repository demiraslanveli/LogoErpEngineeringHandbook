using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Services
{
    public sealed class CustomerService
    {
        private readonly ICustomerGateway _gateway;

        public CustomerService(ICustomerGateway gateway)
        {
            _gateway = gateway;
        }

        public OperationResult Create(string code, string title, string taxNumber, string taxOffice)
        {
            if (string.IsNullOrWhiteSpace(code))
                return OperationResult.Failure("VALIDATION", "Customer code is required.");

            if (string.IsNullOrWhiteSpace(title))
                return OperationResult.Failure("VALIDATION", "Customer title is required.");

            return _gateway.Create(code.Trim(), title.Trim(), taxNumber, taxOffice);
        }
    }
}
