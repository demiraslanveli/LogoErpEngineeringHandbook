using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Production
{
    public sealed class UnconfiguredProductionApplicationBridge : IProductionApplicationBridge
    {
        public OperationResult Open()
        {
            return OperationResult.Failure(
                "PRODUCTION_SDK_NOT_CONFIGURED",
                "ProductionApplication SDK binding has not been verified for this installation.");
        }

        public OperationResult Close()
        {
            return OperationResult.Ok("ProductionApplication bridge is not configured; nothing to close.");
        }

        public OperationResult CreateProductionOrder(ProductionApplicationCommand command)
        {
            return OperationResult.Failure(
                "PRODUCTION_SDK_NOT_CONFIGURED",
                "ProductionApplication order creation requires verified SDK metadata.");
        }
    }
}
