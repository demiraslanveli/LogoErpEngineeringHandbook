using System;
using System.Collections.Generic;
using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;
using LogoErp.Reference.LogoAdapter.Data;

namespace LogoErp.Reference.LogoAdapter.Documents
{
    public sealed class LogoDispatchInvoiceGateway : IDispatchInvoiceGateway
    {
        private readonly ILogoDataObjectFactory _factory;
        private readonly DispatchInvoiceDataMappingProfile _profile;

        public LogoDispatchInvoiceGateway(
            ILogoDataObjectFactory factory,
            DispatchInvoiceDataMappingProfile profile)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        public OperationResult CreateDispatch(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<DispatchInvoiceLineInput> lines)
        {
            _profile.ValidateForDispatch();
            return CreateDocument(_profile.DispatchDataObjectTypeKey, documentNumber, date, customerCode, lines);
        }

        public OperationResult CreateInvoice(
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<DispatchInvoiceLineInput> lines)
        {
            _profile.ValidateForInvoice();
            return CreateDocument(_profile.InvoiceDataObjectTypeKey, documentNumber, date, customerCode, lines);
        }

        private OperationResult CreateDocument(
            string dataObjectTypeKey,
            string documentNumber,
            DateTime date,
            string customerCode,
            IReadOnlyCollection<DispatchInvoiceLineInput> lines)
        {
            var created = _factory.Create(dataObjectTypeKey);
            if (!created.Success)
                return OperationResult.Failure(created.Code, created.Message);

            using (var data = created.Value)
            {
                data.SetField(_profile.DocumentNumberField, documentNumber);
                data.SetField(_profile.DateField, date);
                data.SetField(_profile.CustomerCodeField, customerCode);

                foreach (var input in lines)
                {
                    var line = data.AppendLine(_profile.LinesCollectionKey);
                    line.SetField(_profile.ItemCodeField, input.ItemCode);
                    line.SetField(_profile.QuantityField, input.Quantity);
                    line.SetField(_profile.UnitPriceField, input.UnitPrice);
                    line.SetField(_profile.WarehouseNumberField, input.WarehouseNumber);
                    line.SetField(_profile.VatRateField, input.VatRate);
                }

                return data.Post();
            }
        }
    }
}
