using System;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Gateways
{
    public sealed class ProductionOrderInput
    {
        public string OrderNumber { get; set; }
        public string ItemCode { get; set; }
        public double PlannedQuantity { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }

    public interface IProductionGateway
    {
        OperationResult CreateProductionOrder(ProductionOrderInput input);
    }
}
