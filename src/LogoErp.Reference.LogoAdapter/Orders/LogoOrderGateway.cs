using System;
using System.Collections.Generic;
using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Orders
{
    public sealed class LogoOrderGateway : IOrderGateway
    {
        public OperationResult CreateSalesOrder(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<OrderLineInput> lines)
        {
            // Logo Objects order DataObjectType, header/line fields and posting flow
            // are intentionally not hard-coded here until verified against the target version.
            return OperationResult.Failure(
                "LOGO_ADAPTER_NOT_CONFIGURED",
                "Order adapter requires verified Logo Objects metadata for the installed version.");
        }
    }
}
