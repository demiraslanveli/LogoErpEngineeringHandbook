using System;
using LogoErp.Reference.Core.Abstractions;
using LogoErp.Reference.Core.Context;

namespace LogoErp.Reference.LogoAdapter.Session
{
    /// <summary>
    /// Logo Objects / UnityApplication session boundary.
    ///
    /// This class deliberately does not hard-code a concrete Logo COM type.
    /// Add the exact UnityApplication/Logo Objects reference used by the target
    /// installation, then implement login/logout and object creation here.
    /// Keeping the SDK dependency in this project prevents the rest of the
    /// solution from becoming directly coupled to COM and version-specific APIs.
    /// </summary>
    public sealed class LogoSessionAdapter : ILogoSession
    {
        private readonly CompanyPeriodContext _context;
        private bool _isOpen;

        public LogoSessionAdapter(CompanyPeriodContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool IsOpen => _isOpen;

        public void Open()
        {
            if (_isOpen)
                return;

            // TODO: Instantiate the verified Logo UnityApplication object.
            // TODO: Login with the configured Logo user and company/period context.
            // TODO: Verify company/period switch behavior for the deployed SDK version.

            _isOpen = true;
        }

        public void Close()
        {
            if (!_isOpen)
                return;

            // TODO: Logout and release COM objects deterministically.

            _isOpen = false;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
