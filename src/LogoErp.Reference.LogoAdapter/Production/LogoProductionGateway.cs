using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Production
{
    public sealed class LogoProductionGateway : IProductionGateway
    {
        public OperationResult CreateProductionOrder(ProductionOrderInput input)
        {
            // ProductionApplication APIs are version/environment dependent.
            // Exact method names, enums and field mappings must be filled only
            // after verification against the installed Logo version and its Object Browser/docs.
            return OperationResult.Failure(
                "PRODUCTION_ADAPTER_NOT_CONFIGURED",
                "ProductionApplication adapter requires verified API metadata for the installed version.");
        }
    }
}
