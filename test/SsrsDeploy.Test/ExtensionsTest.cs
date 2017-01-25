using NUnit.Framework;
using System;


namespace SsrsDeploy
{
    [TestFixture]
    public class ExtensionsTest
    {
        [Test]
        public void StringToSecureStringReturnsEmptySecureStringOnNullTest()
        {
            var ss = Extensions.StringToSecureString(null);

            Assert.AreEqual(0, ss.Length);
        }

        [Test]
        public void StringToSecureStringReturnsEmptySecureStringOnEmptyStringTest()
        {
            var ss = Extensions.StringToSecureString("");

            Assert.AreEqual(0, ss.Length);
        }

        [Test]
        public void StringToSecureStringTest()
        {
            var ss = Extensions.StringToSecureString("test");

            Assert.AreEqual(4, ss.Length);
        }

        [Test]
        public void SecureStringToStringThrowsOnNullTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Extensions.SecureStringToString(null);
            });
        }

        [Test]
        public void SecureStringToStringTest()
        {
            var ss = Extensions.StringToSecureString("test");

            var result = Extensions.SecureStringToString(ss);

            Assert.AreEqual("test", result);
        }

        [Test]
        public void GetKnuthHashNullTest()
        {
            var result = Extensions.GetKnuthHash(null);

            Assert.IsTrue(result > 0);
        }

        [Test]
        public void GetKnuthHashStringEmptyTest()
        {
            var result = Extensions.GetKnuthHash("");

            Assert.IsTrue(result > 0);
        }

        [Test]
        public void GetKnuthHashTest()
        {
            var result = Extensions.GetKnuthHash("some content");

            Assert.IsTrue(result > 0);
        }
    }
}
