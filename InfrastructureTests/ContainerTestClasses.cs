using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity;

namespace InfrastructureTests
{
    public interface MyDefaultInterface
    {
        string Hello()
        {
            return "Hello";
        }
    }

	public class MyClass : MyDefaultInterface {}

	public interface IServiceA
	{
		string SurName { get; set; }
	}

	public class ServiceA : IServiceA
	{
		public string SurName { get; set; }

		public ServiceA()
		{
			SurName = "James";
		}
	}

	public interface IServiceB
	{
		string FullName { get; }
		IServiceA ServiceA { get; }
	}

	public class ServiceB : IServiceB
	{
		public string SurName => ServiceA.SurName;

		public string FullName => $"{ServiceA.SurName} Bond";

		public IServiceA ServiceA { get; } = null;

		public ServiceB() { }

		public ServiceB(IServiceA serviceA)
		{
			this.ServiceA = serviceA;
		}
	}

	public class ServiceA2 : IServiceA
	{
		public string SurName { get; set; }
	}

	public class DisposableA : IDisposable
	{
		public bool Disposed { get; set; }
		public void Dispose()
		{
			Disposed = true;
		}

		public DisposableA(DisposableB dependency) { }
	}

	public class DisposableB : IDisposable
	{
		public bool Disposed { get; set; }
		public void Dispose()
		{
			Disposed = true;
		}

		public DisposableB(DisposableC dependency) { }
	}

	public class DisposableC : IDisposable
	{
		public bool Disposed { get; set; }
		public void Dispose()
		{
			Disposed = true;
		}
	}

    public class ConfigurableServiceA : IServiceA
    {
        public string SurName { get; set; }

        public ConfigurableServiceA(string name)
        {
            SurName = name;
        }
    }

    public class Singleton
    {
        [Dependency]
        public DisposableA Dependency{ get; set; }
    }

    public class RichConstructorClass
    {
        public RichConstructorClass(string name, IServiceA serviceA, int age, bool female, string homeTown,
            ConfigurableServiceA complexDependency)
        {
            Assert.IsNotNull(name);
            Assert.AreNotEqual(0, age);
            Assert.AreNotEqual(false, female);
            Assert.IsNotNull(homeTown);
            Assert.IsNotNull(complexDependency);
        }
    }
}
