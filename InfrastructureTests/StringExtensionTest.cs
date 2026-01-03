using Infrastructure;
using Infrastructure.Validation;
using InfrastructureTests.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infrastructure.Extensions;

namespace InfrastructureTests
{
    [TestClass]
    public class StringExtensionTest
    {
        [TestMethod]
        public void String_can_be_converted_to_secure_and_back()
        {
            var text = "HelloTest";
            var secureText = text.ToSecureString();
            Assert.AreEqual(text, secureText.ToUnsecureString());
        }

		[TestMethod]
		public void SecureString_can_be_compared()
		{
			var text = "HelloTest";
			var secureText1 = text.ToSecureString();
			var secureText2 = text.ToSecureString();
			Assert.IsTrue(secureText1.IsEqualTo(secureText2));
		}
	}
}
