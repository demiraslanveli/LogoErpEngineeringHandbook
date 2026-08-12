using System.Collections.Generic;
using LogoErp.Reference.Application.Abstractions;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.IntegrationTests.Fakes
{
    public sealed class FakeDispatchInvoiceGateway : IDispatchInvoiceGateway
    {
        public readonly List<string> Dispatches = new List<string>();
        public readonly List<string> Invoices = new List<string>();

        public OperationResult CreateDispatch(string documentNo)
        {
            if (string.IsNullOrWhiteSpace(documentNo))
                return OperationResult.Fail("DOCUMENT_NO_REQUIRED", "Belge numarası zorunludur.");

            Dispatches.Add(documentNo);
            return OperationResult.Ok();
        }

        public OperationResult CreateInvoice(string documentNo)
        {
            if (string.IsNullOrWhiteSpace(documentNo))
                return OperationResult.Fail("DOCUMENT_NO_REQUIRED", "Belge numarası zorunludur.");

            Invoices.Add(documentNo);
            return OperationResult.Ok();
        }
    }
}
