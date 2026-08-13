using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Production
{
    /// <summary>
    /// Version-specific ProductionApplication boundary.
    /// Concrete Logo SDK/COM calls must be implemented only after verification.
    /// </summary>
    public interface IProductionApplicationBridge
    {
        OperationResult Open();
        OperationResult Close();
        OperationResult CreateProductionOrder(ProductionApplicationCommand command);
    }
}
