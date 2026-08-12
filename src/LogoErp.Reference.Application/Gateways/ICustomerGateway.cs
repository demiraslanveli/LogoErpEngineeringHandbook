using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Gateways
{
    public interface ICustomerGateway
    {
        OperationResult Create(string code, string title, string taxNumber, string taxOffice);
    }
}
