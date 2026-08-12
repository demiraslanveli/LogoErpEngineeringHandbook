using System;
using LogoErp.Reference.Core.Context;

namespace LogoErp.Reference.Core.Abstractions
{
    public interface ILogoSession : IDisposable
    {
        CompanyPeriodContext Context { get; }
        bool IsConnected { get; }
        void Connect();
        void Disconnect();
    }
}
