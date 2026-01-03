using System;

namespace Infrastructure.Utilities
{
    public static class Throw
    {
        public static void If(Func<bool> predicate, string message)
        {
            if (predicate())
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void If(bool condition, string message)
        {
            if (condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }

    public static class Throw<T> where T : Exception
    {
        public static void If(Func<bool> predicate, string message)
        {
            if (predicate())
            {
                throw (Exception)Activator.CreateInstance(typeof(T), new object[] { message });
            }
        }

        public static void If(bool condition, string message)
        {
            if (condition)
            {
                throw (Exception)Activator.CreateInstance(typeof(T), new object[] { message });
            }
        }
    }
}
