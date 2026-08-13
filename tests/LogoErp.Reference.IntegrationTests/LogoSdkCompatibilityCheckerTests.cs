using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogoErp.Reference.LogoAdapter.Binding;

namespace LogoErp.Reference.IntegrationTests
{
    [TestClass]
    public sealed class LogoSdkCompatibilityCheckerTests
    {
        [TestMethod]
        public void Validate_ShouldFail_WhenRequiredBindingIsMissing()
        {
            var manifest = new LogoSdkBindingManifest
            {
                SdkVersion = "TEST"
            };

            var checker = new LogoSdkCompatibilityChecker();
            var result = checker.Validate(
                manifest,
                new[] { LogoSdkBindingKeys.MaterialDataObjectType });

            Assert.IsFalse(result.Success);
            Assert.AreEqual("SDK_BINDING_INCOMPLETE", result.Code);
        }

        [TestMethod]
        public void Validate_ShouldSucceed_WhenRequiredBindingExists()
        {
            var manifest = new LogoSdkBindingManifest
            {
                SdkVersion = "TEST"
            };

            manifest.Set(LogoSdkBindingKeys.MaterialDataObjectType, "VERIFIED_VALUE");

            var checker = new LogoSdkCompatibilityChecker();
            var result = checker.Validate(
                manifest,
                new[] { LogoSdkBindingKeys.MaterialDataObjectType });

            Assert.IsTrue(result.Success);
        }
    }
}
