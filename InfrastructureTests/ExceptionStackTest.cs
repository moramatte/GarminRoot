using System;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
	[TestClass]
	public class ExceptionStackTest
	{
		[TestMethod]
        public void InnerExceptionsAreChecked()
        {
            Exception exception = null;
            try
            {
                DeepCallStack("Nothing to declare");
            }
            catch (Exception e)
            {
                exception = e;
            }
            Assert.IsFalse(exception.Contains("Token"));

            try
            {
                DeepCallStack("My Token is here");
            }
            catch (Exception e)
            {
                exception = e;
            }
            Assert.IsTrue(exception.Contains("Token"));
		}

        private void DeepCallStack(string innerMostException)
        {
            try
            {
                try
                {
                    try
                    {
                        throw new Exception(innerMostException);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Nested", e);
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Nested", e);
                }
            }
            catch (Exception e)
            {
				throw new Exception("Outer", e);
			}
        }
	}
}
