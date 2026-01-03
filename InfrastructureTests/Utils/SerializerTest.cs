using Infrastructure;
using Infrastructure.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureTests.Utils;

[TestClass]
public class SerializerTest
{
	[TestInitialize]
	public void Init()
	{
		ServiceLocator.Reset();
	}

	public class Foo
	{
		public string Data;
	}

	[TestMethod]
	public void XmlSerializersAreCached()
	{
		var serializer = SerializerNet.GetXmlSerializer(typeof(Foo));
		var anotherSerializer = SerializerNet.GetXmlSerializer(typeof(Foo));
		Assert.AreEqual(serializer, anotherSerializer);
	}

	[TestMethod]
	public void XmlPrettyReturnsIndented()
	{
		var xml = SerializerNet.ToXml(new Foo());
		var prettified = SerializerNet.Pretty(xml);
		Assert.AreEqual(xml, prettified);
	}
}
