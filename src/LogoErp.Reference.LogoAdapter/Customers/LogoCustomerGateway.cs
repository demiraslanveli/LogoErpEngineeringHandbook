using System;
using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Customers
{
    public sealed class LogoCustomerGateway : ICustomerGateway
    {
        public OperationResult Create(string code, string title, string taxNumber, string taxOffice)
        {
            // IMPORTANT:
            // The concrete Logo Objects DataObjectType value, IData field names and
            // Post/Apply method sequence must be wired only after they are verified
            // against the installed Logo Objects version.
            //
            // This class intentionally defines the adapter boundary without guessing
            // version-dependent SDK identifiers.
            return OperationResult.Failure(
                "LOGO_ADAPTER_NOT_CONFIGURED",
                "Customer adapter requires verified Logo Objects metadata for the installed version.");
        }
    }
}
