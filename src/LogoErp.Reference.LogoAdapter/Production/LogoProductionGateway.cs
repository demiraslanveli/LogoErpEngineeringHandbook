using System;
using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Production
{
    public sealed class LogoProductionGateway : IProductionGateway
    {
        private readonly IProductionApplicationBridge _bridge;

        public LogoProductionGateway(IProductionApplicationBridge bridge)
        {
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
        }

        public OperationResult CreateProductionOrder(ProductionOrderInput input)
        {
            if (input == null)
                return OperationResult.Failure("VALIDATION", "Production order input is required.");

            var open = _bridge.Open();
            if (!open.Success)
                return open;

            try
            {
                var command = new ProductionApplicationCommand
                {
                    OrderNumber = input.OrderNumber,
                    ItemCode = input.ItemCode,
                    PlannedQuantity = input.PlannedQuantity,
                    PlannedStartDate = input.PlannedStartDate,
                    PlannedEndDate = input.PlannedEndDate
                };

                return _bridge.CreateProductionOrder(command);
            }
            finally
            {
                _bridge.Close();
            }
        }
    }
}
