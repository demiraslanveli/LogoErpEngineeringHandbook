using System;
using LogoErp.Reference.Core.Abstractions;
using LogoErp.Reference.Core.Configuration;
using LogoErp.Reference.Core.Context;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.LogoAdapter.Session
{
    /// <summary>
    /// Logo Objects / UnityApplication session boundary.
    ///
    /// Version-specific COM calls are delegated to ILogoSdkBridge. The session
    /// is considered open only when the bridge reports a successful login.
    /// </summary>
    public sealed class LogoSessionAdapter : ILogoSession
    {
        private readonly CompanyPeriodContext _context;
        private readonly LogoErpOptions _options;
        private readonly ILogoSdkBridge _bridge;
        private bool _disposed;

        public LogoSessionAdapter(
            CompanyPeriodContext context,
            LogoErpOptions options,
            ILogoSdkBridge bridge)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
        }

        public bool IsOpen => !_disposed && _bridge.IsLoggedIn;

        public OperationResult OpenSession()
        {
            ThrowIfDisposed();

            if (IsOpen)
                return OperationResult.Ok();

            _options.Validate();

            var result = _bridge.Login(_options, _context);
            if (!result.Success)
                return result;

            if (!_bridge.IsLoggedIn)
                return OperationResult.Fail(
                    "LOGO_LOGIN_STATE_INVALID: Bridge returned success but session is not logged in.");

            return OperationResult.Ok();
        }

        public OperationResult CheckHealth()
        {
            ThrowIfDisposed();

            if (!IsOpen)
                return OperationResult.Fail("LOGO_SESSION_CLOSED: Logo session is not open.");

            return _bridge.Ping();
        }

        public OperationResult CloseSession()
        {
            if (_disposed)
                return OperationResult.Ok();

            if (!IsOpen)
                return OperationResult.Ok();

            return _bridge.Logout();
        }

        void ILogoSession.Open()
        {
            var result = OpenSession();
            if (!result.Success)
                throw new InvalidOperationException(result.Message);
        }

        void ILogoSession.Close()
        {
            var result = CloseSession();
            if (!result.Success)
                throw new InvalidOperationException(result.Message);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                CloseSession();
            }
            finally
            {
                _bridge.Dispose();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LogoSessionAdapter));
        }
    }
}
