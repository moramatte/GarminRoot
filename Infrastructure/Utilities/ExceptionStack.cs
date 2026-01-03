using System;

namespace Infrastructure.Utilities
{
    public static class ExceptionStack
    {
        public static bool Contains(this Exception theException, string partOfMessage)
        {
            while (theException != null)
            {
                if (theException.Message.Contains(partOfMessage))
                {
                    return true;
                }

                theException = theException.InnerException;
            }

            return false;
        }
    }
}
