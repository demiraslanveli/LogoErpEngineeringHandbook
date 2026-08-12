using System;
using LogoErp.Reference.Application.Gateways;
using LogoErp.Reference.Core.Results;
using LogoErp.Reference.LogoAdapter.Data;
using LogoErp.Reference.LogoAdapter.Session;

namespace LogoErp.Reference.LogoAdapter.Customers
{
    public sealed class LogoCustomerGateway : ICustomerGateway
    {
        private readonly LogoSessionAdapter _session;
        private readonly ILogoDataObjectFactory _factory;
        private readonly CustomerDataMappingProfile _profile;

        public LogoCustomerGateway(
            LogoSessionAdapter session,
            ILogoDataObjectFactory factory,
            CustomerDataMappingProfile profile)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        public OperationResult Create(
            string code,
            string title,
            string taxNumber,
            string taxOffice)
        {
            if (!_session.IsOpen)
            {
                return OperationResult.Fail(
                    "LOGO_SESSION_CLOSED",
                    "Logo session is not open.");
            }

            _profile.Validate();

            var createResult = _factory.Create(
                _profile.DataObjectTypeKey,
                out var dataObject);

            if (!createResult.Success)
                return createResult;

            using (dataObject)
            {
                var result = dataObject.SetField(_profile.CodeField, code);
                if (!result.Success)
                    return result;

                result = dataObject.SetField(_profile.TitleField, title);
                if (!result.Success)
                    return result;

                if (!string.IsNullOrWhiteSpace(taxNumber) &&
                    !string.IsNullOrWhiteSpace(_profile.TaxNumberField))
                {
                    result = dataObject.SetField(_profile.TaxNumberField, taxNumber);
                    if (!result.Success)
                        return result;
                }

                if (!string.IsNullOrWhiteSpace(taxOffice) &&
                    !string.IsNullOrWhiteSpace(_profile.TaxOfficeField))
                {
                    result = dataObject.SetField(_profile.TaxOfficeField, taxOffice);
                    if (!result.Success)
                        return result;
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

                return OperationResult.Ok("Customer card created through Logo IData bridge.");
            }
        }
    }
}
