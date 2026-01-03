using System.Linq;
using System.Reflection;

namespace InfrastructureTests
{
    /// <summary>
    /// Utilities for test subject (class under test), that are not suitable to be part of the prod code.
    /// </summary>
    public static class TestSubjectExtensions
    {
        /// <summary>
        /// We typically want to have event handler methods private, while still be able to call them directly from tests.
        /// This method uses reflection to get a reference to the private method named "Handle" that has an argument of type T (The actual event object),
        /// and calls that method.
        /// </summary>
        /// <typeparam name="T">Type of the event</typeparam>
        /// <param name="subject">The test subject</param>
        /// <param name="payload">Instance of the event type.</param>
        public static void CallPrivateEventHandler<T>(this object subject, T payload)
        {
            var handleMethods = subject.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.Name.Equals("Handle"));
            foreach (var handleMethod in handleMethods)
            {
                var argument = handleMethod.GetParameters().First();
                if (argument.ParameterType == typeof(T))
                {
                    handleMethod.Invoke(subject, new object[] { payload });
                    return;
                }
            }
        }

        /// <summary>
        /// For tests, utility that invokes a private method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="subject">The test subject</param>
        /// <param name="methodName">The method name</param>
        /// <param name="args">All method parameters</param>
        public static void CallPrivateMethod(this object subject, string methodName, params object[] args)
        {
            var handleMethod = subject.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).First(m => m.Name.Equals(methodName) && m.GetParameters().Length == args.Length);
            handleMethod.Invoke(subject, args);
        }

        /// <summary>
        /// For tests, utility that invokes a private method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="subject">The test subject</param>
        /// <param name="methodName">The method name</param>
        /// <param name="args">All method parameters</param>
        /// <returns>An expected result of type T</returns>
        public static T CallPrivateMethod<T>(this object subject, string methodName, params object[] args)
        {
            var handleMethod = subject.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).First(m => m.Name.Equals(methodName));
            var result = handleMethod.Invoke(subject, args);
            return (T)result;
        }

        /// <summary>
        /// In tests, it can be convenient to just assign a private field a mocked/designed value. 
        /// </summary>
        /// <typeparam name="T">Type of the field</typeparam>
        /// <param name="subject">the object that holds the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="payload">The new value to assign</param>
        public static void SetPrivateField<T>(this object subject, string fieldName, T payload)
        {
			var t = subject.GetType();
            const BindingFlags bf = BindingFlags.Instance |
                                    BindingFlags.NonPublic |
                                    BindingFlags.DeclaredOnly;

            FieldInfo fi;
            while ((fi = t.GetField(fieldName, bf)) == null && (t = t.BaseType) != null) ;

            fi.SetValue(subject, payload);
		}

        /// <summary>
        /// In tests, it can be convenient to just extract a private field. 
        /// </summary>
        /// <typeparam name="T">Type of the field</typeparam>
        /// <param name="subject">the object that holds the field</param>
        /// <param name="fieldName">Name of the field</param>
        public static T GetPrivateField<T>(this object subject, string fieldName)
        {
            var field = subject.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(subject);
        }
    }
}
