using System;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InfrastructureTests
{
    [TestClass]
    public class ServiceLocatorTests
    {
        [TestInitialize]
        public void Setup()
        {
            ServiceLocator.Reset();
        }

        [TestMethod]
        public void ServiceLocationWorks()
        {
            ServiceLocator.Register<IServiceA, ServiceA>();

            var service = ServiceLocator.Resolve<IServiceA>();
            Assert.AreEqual("James", service.SurName);
        }

        [TestMethod]
        public void NamedRegistrationsWork()
        {
            var anA = new ConfigurableServiceA("A");
            var aB = new ConfigurableServiceA("B");
            
            ServiceLocator.RegisterInstance<IServiceA>("A", anA);
            ServiceLocator.RegisterInstance<IServiceA>("B", aB);

            var service = ServiceLocator.Resolve<IServiceA>("B");
            Assert.AreEqual(aB, service);
            Assert.AreEqual("B", service.SurName);
        }

        [TestMethod]
        public void DependencyInjectionWorks()
        {
            ServiceLocator.Register<IServiceA, ServiceA>();
            ServiceLocator.Register<IServiceB, ServiceB>();

            var service = ServiceLocator.Resolve<IServiceB>();
            Assert.AreEqual("James Bond", service.FullName);
        }

        [TestMethod]
        public void DisposeIsNotCalledAutomaticallyForTransients()
        {
	        ServiceLocator.RegisterSingleton<DisposableA>();
	        ServiceLocator.RegisterTransient<DisposableB>();
	        ServiceLocator.RegisterTransient<DisposableC>();
	       
	        var dA = ServiceLocator.Resolve<DisposableA>();
	        var dB = ServiceLocator.Resolve<DisposableB>();
	        var dC = ServiceLocator.Resolve<DisposableC>();
	        Assert.IsFalse(dA.Disposed);
	        Assert.IsFalse(dB.Disposed);
	        Assert.IsFalse(dC.Disposed);

            ServiceLocator.Reset();

            Assert.IsTrue(dA.Disposed);
            Assert.IsFalse(dB.Disposed);
            Assert.IsFalse(dC.Disposed);
		}

        [TestMethod]
        public void SingletonBuildUp()
        {
            ServiceLocator.RegisterSingleton<DisposableA>();
           
            var singleton = new Singleton();
            var singleton2 = new Singleton();
            ServiceLocator.RegisterInstance<Singleton>(singleton);
            ServiceLocator.BuildUp(singleton2);

            Assert.IsNull(singleton2.Dependency); // This shows that it is not possible to call BuildUp on an already registered singleton,
            // even for subsequent instances of the type(!)
        }

        [TestMethod]
        public void DefaultInterfacesCanBeResolvedWithoutImplementationType()
        {
	        ServiceLocator.RegisterSingleton<DisposableA>();
	        ServiceLocator.RegisterTransient<DisposableB>();
	        ServiceLocator.RegisterTransient<DisposableC>();
	       
	        var dA = ServiceLocator.Resolve<DisposableA>();
	        var dB = ServiceLocator.Resolve<DisposableB>();
	        var dC = ServiceLocator.Resolve<DisposableC>();
	        Assert.IsFalse(dA.Disposed);
	        Assert.IsFalse(dB.Disposed);
	        Assert.IsFalse(dC.Disposed);

            ServiceLocator.Reset();

            Assert.IsTrue(dA.Disposed);
            Assert.IsFalse(dB.Disposed);
            Assert.IsFalse(dC.Disposed);
		}

        [TestMethod]
        public void DisposeIsCalledAutomaticallyForSingletons()
        {
	        ServiceLocator.RegisterSingleton<DisposableA>();
	        ServiceLocator.RegisterSingleton<DisposableB>();
	        ServiceLocator.RegisterSingleton<DisposableC>();

	        var dA = ServiceLocator.Resolve<DisposableA>();
	        var dB = ServiceLocator.Resolve<DisposableB>();
	        var dC = ServiceLocator.Resolve<DisposableC>();
	        Assert.IsFalse(dA.Disposed);
	        Assert.IsFalse(dB.Disposed);
	        Assert.IsFalse(dC.Disposed);

	        ServiceLocator.Reset();

	        Assert.IsTrue(dA.Disposed);
	        Assert.IsTrue(dB.Disposed);
	        Assert.IsTrue(dC.Disposed);
        }

        [TestMethod]
        public void DisposeIsCalledAutomaticallyForPassedInInstances()
        {
	        var d = new DisposableC();
            ServiceLocator.RegisterInstance<DisposableC>(d);
            Assert.IsFalse(d.Disposed);

	        ServiceLocator.Reset();

	        Assert.IsTrue(d.Disposed);
        }

        [TestMethod]
        public void DefaultInterfacesActuallyWork()
        {
            ServiceLocator.RegisterSingleton<MyClass>();
            MyDefaultInterface m = ServiceLocator.Resolve<MyClass>();
            Assert.AreEqual("Hello", m.Hello());
        }

        /// <summary>
        /// The richest constructor of this implementation takes in a string name.
        /// </summary>
       
    }
}