using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Infrastructure.Extensions;

namespace Infrastructure.Utilities
{
    public static class Compare
    {
        static Dictionary<string, object> checkedProperties = new();

        public static IEnumerable<Difference> Deep(object objA, object objB)
        {
            checkedProperties.Clear();
            var result = new List<Difference>();
            Deep("", objA, objB, result);
            return result;
        }

        public static IEnumerable<(string, object)> LastSet => checkedProperties.Select(kvp => (kvp.Key, kvp.Value));
        
        private static void Assert<T>(string property, T objA, T objB, List<Difference> result)
        {
            Checking(property);
            if (objA.Equals(objB))
            {
                return;
            }

            result.Add(new Difference(property, objA, objB));
        }

        private static void Checking(string property)
        {
            checkedProperties[property] = null;
        }

        /// <summary>
        /// Returns whether we should continue or not
        /// </summary>
        private static bool AssessNull(string property, object objA, object objB, List<Difference> result)
        {
            Checking(property);
            if (objA == null && objB != null)
            {
                result.Add(new Difference(property, objA, objB));
                return false;
            }
            if (objB == null && objA != null)
            {
                result.Add(new Difference(property, objA, objB));
                return false;
            }
            if (objA == null && objB == null)
            {
                return false;
            }
            return true;
        }

        private static void Deep(string propertyName, object objA, object objB, List<Difference> result)
        {
            var typeA = objA.GetType();
            var typeB = objB.GetType();
            Assert("Type", typeA, typeB, result);

            if (typeA.FullName == "System.RuntimeType")
            {
                return;
            }

            if (typeA == typeof(string))
            {
                Assert(propertyName, objA, objB, result);
                return;
            }

            var properties = typeA.GetProperties();

            foreach (var propertyInfo in properties)
            {
                propertyName = propertyInfo.Name;
                
                if (ExcludeProperty(propertyName, propertyInfo))
                    continue;

                if (propertyInfo.IsCollection())
                {
                    var collectionA = propertyInfo.GetGetMethod().Invoke(objA, null) as ICollection;
                    var collectionB = propertyInfo.GetGetMethod().Invoke(objB, null) as ICollection;
                    if (!AssessNull(propertyName, collectionA, collectionB, result))
                    {
                        continue;
                    }
                    else
                    {
                        Assert($"{PropertyFriendlyName(typeA, propertyInfo)} Count", collectionA.Count, collectionB.Count, result);

                        var enumeratorOfA = collectionA.GetEnumerator();
                        var enumeratorOfB = collectionB.GetEnumerator();
                        for (int i = 0; i < collectionA.Count; i++)
                        {
                            enumeratorOfA.MoveNext();
                            enumeratorOfB.MoveNext();
                            Deep(propertyName, enumeratorOfA.Current, enumeratorOfB.Current, result);
                        }
                    }
                }
                else
                {
                    var valA = propertyInfo.GetGetMethod().Invoke(objA, null);
                    var valB = propertyInfo.GetGetMethod().Invoke(objB, null);

                    if (!AssessNull(PropertyFriendlyName(typeA, propertyInfo), valA, valB, result))
                    {
                        continue;
                    }

                    if (propertyInfo.IsReferenceType())
                    {
                        Deep(PropertyFriendlyName(typeA, propertyInfo), valA, valB, result);
                    }
                    else
                    {
                        Assert(PropertyFriendlyName(typeA, propertyInfo), valA, valB, result);
                    }
                }
            }
        }

        private static string PropertyFriendlyName(Type owner, PropertyInfo propertyInfo)
        {
            return $"{owner.FullName}.{propertyInfo.Name}";
        }

        private static bool ExcludeProperty(string propertyName, PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType == typeof(System.Runtime.Serialization.ExtensionDataObject))
            {
                return true;
            }

            if (propertyInfo.HasAttribute<XmlIgnoreAttribute>())
            {
                return true;
            }

            return false;
        }
    }
}