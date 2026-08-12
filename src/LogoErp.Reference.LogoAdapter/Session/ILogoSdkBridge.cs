using System;
using LogoErp.Reference.Core.Configuration;
using LogoErp.Reference.Core.Context;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Session
{
    /// <summary>
    /// Version-specific Logo Objects / UnityApplication calls live behind this boundary.
    /// No other project should depend directly on COM types.
    /// </summary>
    public interface ILogoSdkBridge : IDisposable
    {
        bool IsLoggedIn { get; }

        OperationResult Login(
            LogoErpOptions options,
            CompanyPeriodContext context);

        OperationResult Logout();

        OperationResult Ping();
    }
}
