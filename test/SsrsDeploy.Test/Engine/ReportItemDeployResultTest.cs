using NUnit.Framework;
using System;
using System.Linq;


namespace SsrsDeploy.Engine
{
    [TestFixture]
    public class ReportItemDeployResultTest
    {
        [Test]
        public void SuccessfulTest()
        {
            var result = new ReportItemDeployResult(new[] { new ReportItem("Test", "Content") }, true, null);

            Assert.AreEqual(1, result.Items.Count());
            Assert.True(result.Successful, "Shall be successful");
            Assert.IsNull(result.Exception);
        }

        [Test]
        public void UnsuccessfulTest()
        {
            var result = new ReportItemDeployResult(new[] { new ReportItem("Test", "Content") }, true, new Exception("test"));

            Assert.AreEqual(1, result.Items.Count());
            Assert.False(result.Successful, "Shall be unsuccessful");
            Assert.AreEqual("test", result.Exception.Message);
        }
    }
}
