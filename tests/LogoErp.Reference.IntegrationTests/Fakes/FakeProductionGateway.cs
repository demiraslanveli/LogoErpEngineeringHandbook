using System.Collections.Generic;
using LogoErp.Reference.Application.Abstractions;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.IntegrationTests.Fakes
{
    public sealed class FakeProductionGateway : IProductionGateway
    {
        public readonly List<string> StartedOrders = new List<string>();

        public OperationResult StartProduction(string productionOrderNo)
        {
            if (string.IsNullOrWhiteSpace(productionOrderNo))
                return OperationResult.Fail("PRODUCTION_ORDER_REQUIRED", "Üretim emri numarası zorunludur.");

            StartedOrders.Add(productionOrderNo);
            return OperationResult.Ok();
        }
    }
}
