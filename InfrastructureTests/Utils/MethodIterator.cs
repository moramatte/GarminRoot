using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InfrastructureTests
{
    public class MethodData
    {
        public MethodInfo MethodInfo { get; set; }
        public object[] Variables { get; set; }
    }

    public class MethodIterator<T> : MethodIterator
    {
        public MethodIterator() : base(typeof(T)) { }
    }

    /// <summary>
    /// Iterates over all methods of a given type and creates variables that can be passed to each method
    /// </summary>
    public class MethodIterator : IEnumerable<MethodData>
    {
        private Type _type;
        public MethodIterator(Type type) 
        { 
            _type = type; 
        }

        public IEnumerator<MethodData> GetEnumerator()
        {
            foreach (var method in _type.GetMethods().Where(x => !x.IsConstructor && !x.IsSpecialName))
            {
                var parameters = method.GetParameters();
                object[] variables = null;
                if (parameters.Length > 0)
                {
                    variables = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].HasDefaultValue)
                        {
                            variables[i] = parameters[i].DefaultValue;
                        }
                        else if (parameters[i].ParameterType == typeof(string))
                        {
                            variables[i] = string.Empty;
                        }
                        else if (parameters[i].ParameterType.IsAbstract)
                        {
                            var typesInSameAssembly = Assembly.GetAssembly(parameters[i].ParameterType).GetTypes();
                            var subType = typesInSameAssembly.First(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(parameters[i].ParameterType));
                            variables[i] = Activator.CreateInstance(subType);
                        }
                        else if (parameters[i].ParameterType.IsArray)
                        {
                            variables[i] = Activator.CreateInstance(parameters[i].ParameterType, new object[] { 1 });
                        }
                        else if (parameters[i].ParameterType.GetConstructor(Type.EmptyTypes) != null)
                        {
                            variables[i] = Activator.CreateInstance(parameters[i].ParameterType);
                        }
                        else
                        {
                            variables[i] = null;
                        }
                    }
                }
                yield return new MethodData() { MethodInfo = method, Variables = variables };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
