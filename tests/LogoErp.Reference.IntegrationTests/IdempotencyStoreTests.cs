using System;
using LogoErp.Reference.Infrastructure.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogoErp.Reference.IntegrationTests
{
    [TestClass]
    public class IdempotencyStoreTests
    {
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            _connectionString = Environment.GetEnvironmentVariable("LOGOERP_TEST_SQL");

            if (string.IsNullOrWhiteSpace(_connectionString))
                Assert.Inconclusive("LOGOERP_TEST_SQL environment variable tanımlı değil.");
        }

        [TestMethod]
        public void MarkStarted_ThenSucceeded_ShouldBeDetectedAsExisting()
        {
            var store = new SqlIdempotencyStore(_connectionString);
            var operationKey = "TEST-" + Guid.NewGuid().ToString("N");

            Assert.IsFalse(store.Exists(operationKey));

            store.MarkStarted(operationKey, "INTEGRATION_TEST", Guid.NewGuid().ToString("N"));
            store.MarkSucceeded(operationKey, "TEST-LOGO-REF");

            Assert.IsTrue(store.Exists(operationKey));
        }
    }
}
