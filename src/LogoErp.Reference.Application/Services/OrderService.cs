using System;
using System.Collections.Generic;
using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Services
{
    public sealed class OrderService
    {
        private readonly IOrderGateway _gateway;

        public OrderService(IOrderGateway gateway)
        {
            _gateway = gateway;
        }

        public OperationResult CreateSalesOrder(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<OrderLineInput> lines)
        {
            if (string.IsNullOrWhiteSpace(customerCode))
                return OperationResult.Failure("VALIDATION", "Customer code is required.");

            if (lines == null || lines.Count == 0)
                return OperationResult.Failure("VALIDATION", "At least one order line is required.");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line.ItemCode))
                    return OperationResult.Failure("VALIDATION", "Item code is required on every order line.");

                if (line.Quantity <= 0)
                    return OperationResult.Failure("VALIDATION", "Order quantity must be greater than zero.");
            }

            return _gateway.CreateSalesOrder(documentNumber, date, customerCode.Trim(), lines);
        }
    }
}
