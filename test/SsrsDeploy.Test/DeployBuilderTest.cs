using NUnit.Framework;
using SsrsDeploy.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace SsrsDeploy
{
    [TestFixture]
    public class DeployBuilderTest
    {
        [Test]
        public void PerformDeploySuccessful()
        {
            var variables = new Dictionary<string, string>();

            var builder = new TestableDeployBuilder()
                            .WithReportServer("http://bnevsv-nemsqldb/ReportServer")
                            .WithParentPath("/")
                            .WithItemsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                            .WithVariables(variables)
                            .WithCredentials(null, null, null)
                            .WithLogToConsole();

            var result = builder.PerformDeploy();

            Assert.IsTrue(result.Successful);
            Assert.AreEqual(3, result.Items.Count());
            Assert.IsNull(result.Exception);
        }

        [Test]
        public void PerformDeployExceptionNotSuccessful()
        {
            var variables = new Dictionary<string, string>();

            var builder = new TestableDeployBuilder { ShouldThrow = true };

            builder
           .WithReportServer("http://bnevsv-nemsqldb/ReportServer")
           .WithParentPath("/")
           .WithItemsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
           .WithVariables(variables)
           .WithCredentials(null, null, null)
           .WithLogToConsole();

            var result = builder.PerformDeploy();

            Assert.IsFalse(result.Successful);
            Assert.AreEqual(0, result.Items.Count());
            Assert.NotNull(result.Exception);
        }

        [Test]
        public void PerformDeployNotSuccessful()
        {
            var variables = new Dictionary<string, string>();

            var builder = new TestableDeployBuilder { ShouldFail = true };

            builder
           .WithReportServer("http://bnevsv-nemsqldb/ReportServer")
           .WithParentPath("/")
           .WithItemsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
           .WithVariables(variables)
           .WithCredentials(null, null, null)
           .WithLogToConsole();

            var result = builder.PerformDeploy();

            Assert.IsFalse(result.Successful);
            Assert.AreEqual(0, result.Items.Count());
            Assert.IsNull(result.Exception);
        }
    }

    class TestableDeployBuilder : DeployBuilder
    {
        public bool ShouldThrow;
        public bool ShouldFail;

        protected override int CreateCatalogItemReportService2010(string reportServiceUrl, string parentPath, ReportItem item, string userName, string password, string domain)
        {
            if (ShouldThrow)
            {
                throw new Exception("test");
            }
            if (ShouldFail)
            {
                return -1;
            }
            return 0;
        }

        protected override string GetExistingItemReportService2010(string itemPath, string reportServerUrl, string userName, string password, string domain)
        {
            return null;
        }
    }
}