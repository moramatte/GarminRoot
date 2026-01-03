using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infrastructure
{
	public interface IAssemblyStore
	{
		IEnumerable<Assembly> DomainAssemblies { get; }
		IEnumerable<Assembly> Awos7Assemblies { get; }
		
		void EnsureAssemblyLoaded(string name);
	}

    public class AssemblyStore : IAssemblyStore
    {
        public AssemblyStore()
        {
			_domainAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
		}

		private List<Assembly> _domainAssemblies;
		public IEnumerable<Assembly> DomainAssemblies => _domainAssemblies;
		public IEnumerable<Assembly> Awos7Assemblies => DomainAssemblies.Where(a => a.FullName.StartsWith("Awos7"));

		public void EnsureAssemblyLoaded(string name)
		{
			if (!_domainAssemblies.Any(x => x.FullName == name))
			{
				_domainAssemblies.Add(Assembly.Load(name));
			}
		}
	}
}
