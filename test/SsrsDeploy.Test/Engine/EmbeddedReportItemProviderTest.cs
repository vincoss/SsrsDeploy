using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace SsrsDeploy.Engine
{
    [TestFixture]
    public class EmbeddedReportItemProviderTest
    {
        [Test]
        public void GetItemsTest()
        {
            var filter = DeployBuilder.DefaultFilter();

            var provider = new EmbeddedReportItemProvider(new[] { Assembly.GetExecutingAssembly() }, filter, Encoding.Default);

            var items = provider.GetItems();

            Assert.AreEqual(3, items.Count(), "Shall have items");

            Assert.True(items.Any(a => a.FullName.EndsWith(".rdl", StringComparison.InvariantCultureIgnoreCase)), "Shall have an item");
            Assert.True(items.Any(a => a.FullName.EndsWith(".rsd", StringComparison.InvariantCultureIgnoreCase)), "Shall have an item");
            Assert.True(items.Any(a => a.FullName.EndsWith(".rds", StringComparison.InvariantCultureIgnoreCase)), "Shall have an item");
        }

        [Test]
        public void ReportItemDefinitionTest()
        {
            var filter = DeployBuilder.DefaultFilter();

            var provider = new EmbeddedReportItemProvider(new[] { Assembly.GetExecutingAssembly() }, filter, Encoding.Default);

            var items = provider.GetItems().ToArray();

            var item = items[0];

            Assert.AreEqual("Report", item.Name);
            Assert.AreEqual("Report.rdl", item.Content);

            item = items[1];

            Assert.AreEqual("SharedDataSet", item.Name);
            Assert.AreEqual("SharedDataSet.rsd", item.Content);

            item = items[2];

            Assert.AreEqual("SharedDataSource", item.Name);
            Assert.AreEqual("SharedDataSource.rds", item.Content);
        }
    }
}
