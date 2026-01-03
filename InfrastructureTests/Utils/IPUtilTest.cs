using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests.Utils
{
    [TestClass]
    public class IPUtilTest
    {
        [TestMethod]
        public void IPCanBeRetreived()
        {
            var localIP = IPUtil.GetLocalIPAddress();
            Assert.IsTrue(IsValidIPAddress(localIP));
        }
        private bool IsValidIPAddress(string ipAddress)
        {
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            var regex = new Regex(pattern);
            return regex.IsMatch(ipAddress);
        }
    }
}
