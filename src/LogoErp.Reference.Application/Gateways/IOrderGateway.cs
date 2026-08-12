using System;
using System.Collections.Generic;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Gateways
{
    public sealed class OrderLineInput
    {
        public string ItemCode { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public int WarehouseNumber { get; set; }
    }

    public interface IOrderGateway
    {
        OperationResult CreateSalesOrder(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<OrderLineInput> lines);
    }
}
