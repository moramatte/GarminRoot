using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Infrastructure.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
    [TestClass]
    public class ReflectionTests
    {
        [TestMethod]
        public void GetPrivateMethodsIncludeInherited()
        {
            var subject = new MySubclass();

            var methods = subject.GetType().GetPrivateMethods();

            Assert.IsTrue(methods.Any(m => m.Name == "PrivateBaseMethod"));
        }

        [TestMethod]
        public void DerivesFromGenericTypeWorks()
        {
            Assert.IsFalse(new Dosan().DerivesFromGenericType(typeof(DosanGeneric<>)));
            Assert.IsTrue(new DerivedDosan().DerivesFromGenericType(typeof(DosanGeneric<>)));
        }

        [TestMethod]
        public void SetPrivatePropertyWorks()
        {
            var subject = new PropWithPrivateSetter();
            subject.SetPrivateProperty(nameof(PropWithPrivateSetter.Name), "Joe");
            Assert.AreEqual("Joe", subject.Name);

            var subject2 = new PropWithoutPrivateSetter();
            subject2.SetPrivateProperty(nameof(PropWithoutPrivateSetter.Name), "Joe");
            Assert.AreEqual("Joe", subject2.Name);
        }

        [TestMethod]
        public void SetPrivatePropertyThrowsOnDerivedProperty()
        {
            var subject = new DerivedProp();
            Exception expectedException = null;
            try
            {
                subject.SetPrivateProperty(nameof(DerivedProp.Name), "Joe");
            }
            catch (Exception e)
            {
                expectedException = e;
            }

            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public void EnumParsingWithAsWorks()
        {
            var chosen = "Two".As<TestEnum>();
            Assert.AreEqual(TestEnum.Two, chosen);
        }

        [TestMethod]
        public void IsVirtualWorks()
        {
            var prop1 = typeof(PropWithoutPrivateSetter).GetProperty(nameof(PropWithoutPrivateSetter.Name));
            var prop2 = typeof(PropWithoutPrivateSetter).GetProperty(nameof(PropWithoutPrivateSetter.SirName));
            Assert.IsFalse(prop1.IsVirtual());
            Assert.IsTrue(prop2.IsVirtual());
        }

        [TestMethod]
        public void HasAttributeWorks()
        {
            Assert.IsFalse(new PropWithPrivateSetter().HasAttribute<SAAB>());
            Assert.IsTrue(new PropWithoutPrivateSetter().HasAttribute<SAAB>());
        }

        [TestMethod]
        public void GetAllMethodsWorks()
        {
            var allMethods = typeof(MySubclass).GetAllMethods();
            Assert.IsTrue(allMethods.Count() > 5);
        }

        [TestMethod]
        public void GetPropertyFromSetterWorks()
        {
            var setter = $"set_{nameof(PropWithoutPrivateSetter.SirName)}";
            var prop = typeof(PropWithoutPrivateSetter).GetPropertyFromSetter(setter);
            Assert.IsNotNull(prop);
        }

        [TestMethod]
        public void HasPublicSetterWorks()
        {
            Assert.IsFalse(new PropWithPrivateSetter().HasPublicSetter(nameof(PropWithPrivateSetter.Name)));
            Assert.IsTrue(new PropWithoutPrivateSetter().HasPublicSetter(nameof(PropWithoutPrivateSetter.SirName)));
        }

        [TestMethod]
        public void GetSetterOfWorks()
        {
            var obj = new DerivedProp();
            var setter = obj.GetSetterOf(typeof(SAAB));
            Assert.IsNotNull(setter);
        }

        [TestMethod]
        public void GetDescriptionWorks()
        {
            var enumValue = TestEnum.Three;
            Assert.AreEqual("3", enumValue.GetDescription());

            Exception expectedException = null;

            try
            {
                TestEnum.One.GetDescription();
            }
            catch (Exception e)
            {
                expectedException = e;
            }

            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public void IsCollectionWorks()
        {
            var t = typeof(DerivedProp);
            Assert.IsFalse(t.GetProperty(nameof(DerivedProp.MySAAB)).IsCollection());
            Assert.IsTrue(t.GetProperty(nameof(DerivedProp.MyCollection)).IsCollection());
        }

        [TestMethod]
        public void IsReferenceTypeWorks()
        {
            var t = typeof(DerivedProp);
            Assert.IsTrue(t.GetProperty(nameof(DerivedProp.MySAAB)).IsReferenceType());
            Assert.IsFalse(t.GetProperty(nameof(DerivedProp.Name)).IsReferenceType());
        }

        [TestMethod]
        public void DefaultValueWorks()
        {
            Assert.AreEqual(0d, typeof(double).DefaultValue());
            Assert.AreEqual(null, typeof(SAAB).DefaultValue());
        }

        [TestMethod]
        public void IsDerivedWorks()
        {
            Assert.IsTrue(typeof(DerivedProp).GetProperty(nameof(DerivedProp.Name)).IsDerived());
            Assert.IsFalse(typeof(DerivedProp).GetProperty(nameof(DerivedProp.MySAAB)).IsDerived());
        }

        [TestMethod]
        public void GetRichestConstructorWorks()
        {
            var ctor = typeof(Dosan).GetRichestConstructor();
            Assert.AreEqual(2, ctor.GetParameters().Length);
        }
    }

    public enum TestEnum
    {
        One,
        Two,

        [System.ComponentModel.Description("3")]
        Three
    }

    public class PropWithPrivateSetter
    {
        public string Name { get; private set; }
    }

    public class SAAB : Attribute
    {
    }

    [SAAB]
    public class PropWithoutPrivateSetter
    {
        public string Name { get; }

        public virtual string SirName { get; set; }
    }

    public class DerivedProp
    {
        public string Name => "Joe";

        public IEnumerable<string> MyCollection { get; }

        public SAAB MySAAB { get; set; }
    }

    public class MyBase
    {
        private void PrivateBaseMethod()
        {
        }
    }

    public class MySubclass : MyBase
    {
        private void PrivateSubMethod()
        {
        }
    }

    public class Dosan
    {
        public Dosan()
        {
        }

        public Dosan(string name, int age)
        {
        }

        public Dosan(string name)
        {
        }
    }

    public class HMSInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            throw new NotImplementedException();
        }
    }

    public class DosanGeneric<T>
    {
    }

    public class DerivedDosan : DosanGeneric<Dosan> { }
}
