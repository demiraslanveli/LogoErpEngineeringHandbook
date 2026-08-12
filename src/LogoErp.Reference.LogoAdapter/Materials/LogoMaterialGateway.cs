using LogoErp.Reference.Application.Abstractions;
using LogoErp.Reference.Core.Results;
using LogoErp.Reference.LogoAdapter.Session;

namespace LogoErp.Reference.LogoAdapter.Materials
{
    public sealed class LogoMaterialGateway : ILogoMaterialGateway
    {
        private readonly LogoSessionAdapter _session;

        public LogoMaterialGateway(LogoSessionAdapter session)
        {
            _session = session;
        }

        public OperationResult CreateMaterial(string code, string name)
        {
            if (!_session.IsOpen)
                return OperationResult.Fail("Logo session is not open.");

            // TODO: Create the verified material IData object for the deployed Logo version.
            // TODO: Map CODE, NAME and required master/unit fields.
            // TODO: Execute Post/Save through Logo Objects and parse ErrorDesc/ErrorCode.
            //
            // Direct SQL INSERT is intentionally not used here because material-card
            // creation must pass through Logo business rules and related records.

            return OperationResult.Fail(
                "Logo Objects material mapping is not configured for this installation.");
        }
    }
}
