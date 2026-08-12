using LogoErp.Reference.Core.Configuration;
using LogoErp.Reference.Core.Context;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Session
{
    /// <summary>
    /// Safe default used until the target Logo Objects SDK reference is verified.
    /// It never reports a successful login.
    /// </summary>
    public sealed class UnconfiguredLogoSdkBridge : ILogoSdkBridge
    {
        public bool IsLoggedIn => false;

        public OperationResult Login(LogoErpOptions options, CompanyPeriodContext context)
        {
            return OperationResult.Fail(
                "LOGO_SDK_NOT_CONFIGURED: Verified UnityApplication/Logo Objects binding has not been configured.");
        }

        public OperationResult Logout()
        {
            return OperationResult.Ok();
        }

        public OperationResult Ping()
        {
            return OperationResult.Fail(
                "LOGO_SDK_NOT_CONFIGURED: Logo SDK session is not available.");
        }

        public void Dispose()
        {
        }
    }
}
