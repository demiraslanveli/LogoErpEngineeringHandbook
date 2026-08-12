using LogoErp.Reference.Application.Abstractions;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Services
{
    public sealed class DispatchInvoiceService
    {
        private readonly IDispatchInvoiceGateway _gateway;

        public DispatchInvoiceService(IDispatchInvoiceGateway gateway)
        {
            _gateway = gateway;
        }

        public OperationResult CreateDispatch(string documentNo)
        {
            if (string.IsNullOrWhiteSpace(documentNo))
                return OperationResult.Fail("DOCUMENT_NO_REQUIRED", "İrsaliye numarası zorunludur.");

            return _gateway.CreateDispatch(documentNo.Trim());
        }

        public OperationResult CreateInvoice(string documentNo)
        {
            if (string.IsNullOrWhiteSpace(documentNo))
                return OperationResult.Fail("DOCUMENT_NO_REQUIRED", "Fatura numarası zorunludur.");

            return _gateway.CreateInvoice(documentNo.Trim());
        }
    }
}
