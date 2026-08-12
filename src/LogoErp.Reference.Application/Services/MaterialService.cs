using System;
using LogoErp.Reference.Application.Abstractions;
using LogoErp.Reference.Core.Results;

namespace LogoErp.Reference.Application.Services
{
    public sealed class MaterialService : IMaterialService
    {
        private readonly ILogoMaterialGateway _gateway;

        public MaterialService(ILogoMaterialGateway gateway)
        {
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        public OperationResult Create(string code, string name)
        {
            if (string.IsNullOrWhiteSpace(code))
                return OperationResult.Fail("Material code is required.");

            if (string.IsNullOrWhiteSpace(name))
                return OperationResult.Fail("Material name is required.");

            return _gateway.CreateMaterial(code.Trim(), name.Trim());
        }
    }
}
