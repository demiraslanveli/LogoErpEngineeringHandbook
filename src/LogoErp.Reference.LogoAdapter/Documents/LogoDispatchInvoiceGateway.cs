using System;
using System.Collections.Generic;
using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Documents
{
    public sealed class LogoDispatchInvoiceGateway : IDispatchInvoiceGateway
    {
        public OperationResult CreateDispatch(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<DispatchInvoiceLineInput> lines)
        {
            return OperationResult.Failure(
                "LOGO_ADAPTER_NOT_CONFIGURED",
                "Dispatch adapter requires verified Logo Objects metadata for the installed version.");
        }

        public OperationResult CreateInvoice(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<DispatchInvoiceLineInput> lines)
        {
            return OperationResult.Failure(
                "LOGO_ADAPTER_NOT_CONFIGURED",
                "Invoice adapter requires verified Logo Objects metadata for the installed version.");
        }
    }
}
