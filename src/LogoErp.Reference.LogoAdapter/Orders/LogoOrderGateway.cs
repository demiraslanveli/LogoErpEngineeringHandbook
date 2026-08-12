using System;
using System.Collections.Generic;
using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;
using LogoErp.Reference.LogoAdapter.Data;
using LogoErp.Reference.LogoAdapter.Session;

namespace LogoErp.Reference.LogoAdapter.Orders
{
    public sealed class LogoOrderGateway : IOrderGateway
    {
        private readonly LogoSessionAdapter _session;
        private readonly ILogoDataObjectFactory _factory;
        private readonly OrderDataMappingProfile _profile;

        public LogoOrderGateway(
            LogoSessionAdapter session,
            ILogoDataObjectFactory factory,
            OrderDataMappingProfile profile)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        public OperationResult CreateSalesOrder(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<OrderLineInput> lines)
        {
            if (!_session.IsOpen)
                return OperationResult.Fail("LOGO_SESSION_CLOSED", "Logo session is not open.");

            _profile.Validate();

            var createResult = _factory.Create(_profile.DataObjectTypeKey, out var dataObject);
            if (!createResult.Success)
                return createResult;

            using (dataObject)
            {
                OperationResult result;

                if (!string.IsNullOrWhiteSpace(documentNumber) &&
                    !string.IsNullOrWhiteSpace(_profile.DocumentNumberField))
                {
                    result = dataObject.SetField(_profile.DocumentNumberField, documentNumber);
                    if (!result.Success)
                        return result;
                }

                result = dataObject.SetField(_profile.DateField, date);
                if (!result.Success)
                    return result;

                result = dataObject.SetField(_profile.CustomerCodeField, customerCode);
                if (!result.Success)
                    return result;

                foreach (var input in lines)
                {
                    OperationResult lineError = null;

                    result = dataObject.AppendLine(_profile.LinesCollection, line =>
                    {
                        if (lineError != null)
                            return;

                        lineError = SetLineField(line, _profile.ItemCodeField, input.ItemCode);
                        if (!lineError.Success)
                            return;

                        lineError = SetLineField(line, _profile.QuantityField, input.Quantity);
                        if (!lineError.Success)
                            return;

                        if (!string.IsNullOrWhiteSpace(_profile.UnitPriceField))
                        {
                            lineError = SetLineField(line, _profile.UnitPriceField, input.UnitPrice);
                            if (!lineError.Success)
                                return;
                        }

                        if (!string.IsNullOrWhiteSpace(_profile.WarehouseField))
                        {
                            lineError = SetLineField(line, _profile.WarehouseField, input.WarehouseNumber);
                        }
                    });

                    if (!result.Success)
                        return result;

                    if (lineError != null && !lineError.Success)
                        return lineError;
                }

                result = dataObject.Post();
                if (!result.Success)
                {
                    return OperationResult.Fail(
                        string.IsNullOrWhiteSpace(dataObject.ErrorCode)
                            ? "LOGO_IDATA_POST_FAILED"
                            : dataObject.ErrorCode,
                        string.IsNullOrWhiteSpace(dataObject.ErrorDescription)
                            ? result.Message
                            : dataObject.ErrorDescription);
                }

                return OperationResult.Ok("Sales order created through Logo IData bridge.");
            }
        }

        private static OperationResult SetLineField(
            ILogoDataObjectLine line,
            string fieldName,
            object value)
        {
            return line.SetField(fieldName, value);
        }
    }
}
