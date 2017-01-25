using NUnit.Framework;
using System.IO;
using System.Text;


namespace SsrsDeploy.Engine
{
    [TestFixture]
    public class ReportItemTest
    {
        [Test]
        public void PropertiesLoadTest()
        {
            var bytes = Encoding.Default.GetBytes("Test Content");
            var ms = new MemoryStream();
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;

            var item = ReportItem.FromStream("SsrsDeploy.Test.Reports.ReportName.rdl", ms);

            Assert.AreEqual("ReportName", item.Name);
            Assert.AreEqual("SsrsDeploy.Test.Reports.ReportName.rdl", item.FullName);
            Assert.AreEqual("Test Content", item.Content);
            Assert.NotNull(item.ContentHash);
        }

        [TestCase("a.b.c.d.txt", ExpectedResult = "d")]
        [TestCase("a1.b1.c1.d1.txt", ExpectedResult = "d1")]
        [TestCase("a.txt", ExpectedResult = "a")]
        [TestCase(".txt", ExpectedResult = "")]
        [TestCase(".txt.", ExpectedResult = "")]
        [TestCase("txt", ExpectedResult = "txt")]
        [TestCase("", ExpectedResult = "")]
        public string GetFileNameFromResourceNameTest(string resourceName)
        {
            var fileName = ReportItem.GetFileNameFromResourceName(resourceName);
            return fileName;
        }
    }
}
