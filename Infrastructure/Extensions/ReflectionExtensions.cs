using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Comparer method for method signatures taking into account name and types of the arguments.
    /// </summary>
    public static class ReflectionExtensions
    {
        public static bool MatchesSignature(this MethodInfo m, MethodInfo m2)
        {
            if (!m.Name.Equals(m2.Name))
            {
                return false;
            }

            var mArgs = m.GetParameters();
            var m2Args = m2.GetParameters();

            if (mArgs.Length != m2Args.Length)
            {
                return false;
            }

            for (int i = 0; i < mArgs.Length; i++)
            {
                if (mArgs[i].ParameterType != m2Args[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if t either implements interface or inherits from base class interfaceOrBase
        /// </summary
        /// <param name="t">..</param>
        /// <param name="interfaceOrBase">..</param>
        /// <returns>..</returns>
        public static bool DerivesFrom(this Type t, Type interfaceOrBase)
        {
            return interfaceOrBase.IsAssignableFrom(t);
        }

        public static bool DerivesFromGenericType(this object obj, Type genericType)
        {
            var type = obj.GetType();
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                    return true;
                type = type.BaseType;
            }
            return false;
        }

        public static T As<T>(this string enumString)
        {
            return (T)Enum.Parse(typeof(T), enumString);
        }

        public static string GetDescription(this Enum enumVal)
        {
            var valueInfo = enumVal.GetType().GetMember(enumVal.ToString());
            var attributes = valueInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = (DescriptionAttribute)attributes[0];
            return description.Description;
        }

        /// <summary>
        /// Returns true if the property is an IEnumerable.
        /// Note that string is exceptioned and NOT considered to be an IEnumerable even though it is technically.
        /// </summary>
        /// <param name="propertyInfo">..</param>
        /// <returns>..</returns>
        public static bool IsCollection(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType == typeof(string))
            {
                return false;
            }

            return propertyInfo.PropertyType.DerivesFrom(typeof(IEnumerable));
        }

        /// <summary>
        /// Gets the richest constructor of t. (The constructor with the most parameters)
        /// </summary>
        /// <param name="t">The type of interest</param>
        /// <returns>The richest constructor of t</returns>
        public static ConstructorInfo GetRichestConstructor(this Type t)
        {
	        var ctors = t.GetConstructors().ToList();
	        ConstructorInfo richestCtor = ctors.First();
	        ctors.ForEach(c =>
	        {
		        if (c.GetParameters().Length > richestCtor.GetParameters().Length)
		        {
			        richestCtor = c;
		        }
	        });

	        return richestCtor;
        }

		/// <summary>
		/// Returns true if the property is a reference type.
		/// Note that strings are considered value types.
		/// </summary>        
		/// <param name="propertyInfo">..</param>
		/// <returns>..</returns>
		public static bool IsReferenceType(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType == typeof(string))
            {
                return false;
            }

            return !propertyInfo.PropertyType.IsValueType;
        }

        /// <summary>
        /// Returns if the GetMethod is virtual
        /// </summary>
        /// <param name="propertyInfo">..</param>
        /// <returns>TRUE if virtual</returns>
        public static bool IsVirtual(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetGetMethod().IsVirtual;
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property != null;
        }

        public static object GetValue(this object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null)
            {
                throw new Exception($"Property {propertyName} not found on type {obj.GetType()}");
            }
            return property.GetValue(obj);
        }

        public static T GetValue<T>(this object obj, string propertyName)
        {
            var o = GetValue(obj, propertyName);
            return (T)o;
        }

        /// <summary>
        /// Gets the attribute of type T decorated on the given object, and null if the attribute is not decorated on the object.
        /// Note that if there are multiple instances of T decorated on the object, this method will throw. Use GetAttributes instead.
        /// </summary>
        /// <param name="m">Object to get attribute from</param>
        /// <returns>The attribute of type T decorated on the given object, and null if the attribute is not decorated on the object</returns>
        public static T GetAttribute<T>(this object m) where T : Attribute
        {
            object[] attributes = null;
            if (m is MemberInfo)
            {
                attributes = ((MemberInfo)m).GetCustomAttributes(typeof(T), false);
            }
            else if (m is Enum)
            {
                attributes = m.GetType().GetField(m.ToString()).GetCustomAttributes(typeof(T), false);
            }
            else
            {
                attributes = m.GetType().GetCustomAttributes(typeof(T), false);
            }

            if (attributes.Length == 0)
            {
                return null;
            }

            if (attributes.Length > 1)
            {
                throw new Exception($"Multiple instances of {nameof(T)} decorated on {typeof(object)}. Use GetAttributes instead.");
            }

            return attributes.First() as T;
        }

        /// <summary>
        /// Returns true if an attribute of type T is present on the given object.
        /// </summary>
        /// <param name="o">Object to find attribute for</param>
        /// <returns>Returns true if an attribute of type T is present on the given object, otherwise false</returns>
        public static bool HasAttribute<T>(this object o) where T : Attribute
        {
            return GetAttributes<T>(o).Any();
        }

        /// <summary>
        /// Returns true if an attribute of type t is present on the given object.
        /// </summary>
        /// <param name="o">Object to find attribute for</param>
        /// <param name="t">Required attribute type</param>
        /// <returns>Returns true if an attribute of type t is present on the given object, otherwise false</returns>
        public static bool HasAttribute(this object o, Type t)
        {
            return o.GetType().GetCustomAttributes(t, false).Any();
        }

        /// <summary>
        /// Gets all attribute instances of type T decorated on the given object.
        /// </summary>
        /// <param name="m">Object to get attributes from</param>
        /// <returns>All attribute instances of type T decorated on the given object</returns>
        public static List<object> GetAttributes<T>(this object m) where T : Attribute
        {
            if (m is Enum)
            {
                var type = m.GetType();
                var memInfo = type.GetMember(m.ToString());
                var attrs = memInfo[0].GetCustomAttributes(typeof(T), false);
                return attrs.ToList();
            }

            if (m is MemberInfo)
            {
                return ((MemberInfo)m).GetCustomAttributes(typeof(T), true).ToList();
            }

            if (m is Assembly)
            {
                return ((Assembly)m).GetCustomAttributes(typeof(T), true).ToList();
            }

            var attributes = m.GetType().GetCustomAttributes(typeof(T), true);
            return attributes.ToList();
        }
        
        public static IEnumerable<MethodInfo> GetPrivateMethods(this Type t)
        {
            var result = new List<MethodInfo>();
            result.AddRange(t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance));

            var baseClass = t.BaseType;
            while (baseClass != typeof(object))
            {
                result.AddRange(baseClass.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance));
                baseClass = baseClass.BaseType;
            }
           
            return result;
        }

        public static void SetPrivateProperty<T>(this object subject, string fieldName, T payload)
        {
            var backingField = GetBackingField(subject, fieldName);
            backingField.SetValue(subject, payload);
        }

        private static string GetBackingFieldName(string propertyName)
        {
            return string.Format("<{0}>k__BackingField", propertyName);
        }

        private static FieldInfo GetBackingField(object obj, string propertyName)
        {
            return obj.GetType().GetField(GetBackingFieldName(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Gets all public and non-public methods.
        /// </summary>
        /// <param name="t">The type</param>
        /// <returns>All methods</returns>
        public static IEnumerable<MethodInfo> GetAllMethods(this Type t)
        {
            return t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        }
        
        /// <summary>
        /// A property setter name is prefixed with set_
        /// This helper method strips the prefix and returns the actual property.
        /// </summary>
        /// <param name="t">The target type</param>
        /// <param name="setter">Property setter name. Example: set_Name</param>
        /// <returns>PropertyInfo</returns>
        public static PropertyInfo GetPropertyFromSetter(this Type t, string setter)
        {
            var propertyName = setter.Substring(4);
            return t.GetProperty(propertyName);
        }

        /// <summary>
        /// Checks if the setter of the given property is public.
        /// </summary>
        /// <param name="obj">The target object</param>
        /// <param name="propertyName">Property name without prefix</param>
        /// <returns>Throws if the property doesn´t exist.</returns>
        public static bool HasPublicSetter(this object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            var setter = property.GetSetMethod(nonPublic: true);
            return setter.IsPublic;
        }

        /// <summary>
        /// Checks if the property is derived (hos no setter).
        /// </summary>
        /// <param name="property">..</param>
        /// <returns>Throws if the property doesn´t exist.</returns>
        public static bool IsDerived(this PropertyInfo property)
        {
            var setter = property.GetSetMethod(nonPublic: true);
            if (setter != null)
            {
                return false;
            }

            var getter = property.GetGetMethod(true);
            if (getter.HasAttribute<CompilerGeneratedAttribute>())
            {
                return false;
            }

            return true;
        }

        public static MethodInfo GetSetterOf(this object instance, Type t)
        {
            var allProperties = instance.GetType().GetProperties();
            return allProperties.First(p => p.PropertyType == t).GetSetMethod();
        }
        
        /// <summary>
        /// Returns the instantiated default value of the type
        /// I.e. 0 for integers, null for reference types and the 0 representation of enums
        /// </summary>
        /// <param name="t">..</param>
        /// <returns>..</returns>
        public static object DefaultValue(this Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }
    }
}
