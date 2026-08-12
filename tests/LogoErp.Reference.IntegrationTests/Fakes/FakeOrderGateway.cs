using System.Collections.Generic;
using LogoErp.Reference.Application.Abstractions;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.IntegrationTests.Fakes
{
    public sealed class FakeOrderGateway : IOrderGateway
    {
        public readonly List<string> CreatedOrders = new List<string>();

        public OperationResult Create(string orderNo)
        {
            if (string.IsNullOrWhiteSpace(orderNo))
                return OperationResult.Fail("ORDER_NO_REQUIRED", "Sipariş numarası zorunludur.");

            CreatedOrders.Add(orderNo);
            return OperationResult.Ok();
        }
    }
}
