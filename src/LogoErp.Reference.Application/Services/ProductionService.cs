using LogoErp.Reference.Application.Abstractions;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Services
{
    public sealed class ProductionService
    {
        private readonly IProductionGateway _gateway;

        public ProductionService(IProductionGateway gateway)
        {
            _gateway = gateway;
        }

        public OperationResult Start(string productionOrderNo)
        {
            if (string.IsNullOrWhiteSpace(productionOrderNo))
                return OperationResult.Fail("PRODUCTION_ORDER_REQUIRED", "Üretim emri numarası zorunludur.");

            return _gateway.StartProduction(productionOrderNo.Trim());
        }
    }
}
