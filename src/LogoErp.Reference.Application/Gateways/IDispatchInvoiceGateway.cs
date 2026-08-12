using System;
using System.Collections.Generic;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Gateways
{
    public sealed class DispatchInvoiceLineInput
    {
        public string ItemCode { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public int WarehouseNumber { get; set; }
        public double VatRate { get; set; }
    }

    public interface IDispatchInvoiceGateway
    {
        OperationResult CreateDispatch(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<DispatchInvoiceLineInput> lines);

        OperationResult CreateInvoice(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<DispatchInvoiceLineInput> lines);
    }
}
