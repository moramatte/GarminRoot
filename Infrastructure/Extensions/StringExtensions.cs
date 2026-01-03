using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;

namespace Infrastructure.Extensions
{
    /// <summary>
    /// Convenience methods for string.
    /// </summary>
    public static class StringExtensions
    {
		public static bool OrdinalEquals(this string source, string other)
		{
			return string.Equals(source, other, StringComparison.Ordinal);
		}

		public static bool OrdinalIgnoreCaseEquals(this string source, string other)
		{
			return string.Equals(source, other, StringComparison.OrdinalIgnoreCase);
		}

        public static int ParseTrailingNumbers(this string s)
        {
            var match = Regex.Match(s, @"(\d+)$");
            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            throw new Exception($"No trailing number found in string '{s}'");
        }

		/// <summary>
		/// Syntactically smoother string.IsNullOrEmpty
		/// </summary>
		public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? theString)
        {
            return string.IsNullOrWhiteSpace(theString);
        }

        /// <summary>
        /// Negated IsNullOrEmpty
        /// </summary>
        public static bool HasContent([NotNullWhen(true)] this string? theString)
        {
            return !theString.IsNullOrWhiteSpace();
        }

        public static bool HasContent(this string[] theStringArray)
        {
            if (theStringArray == null)
            {
                return false;
            }
            return theStringArray.Length > 0;
        }

        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? theString)
        {
            return string.IsNullOrEmpty(theString);
        }

        public static bool IsAnInteger(this string theString)
        {
            return Int32.TryParse(theString, out _);
        }

        public static bool IsAnInteger(this char theCharacter)
        {
            return Int32.TryParse($"{theCharacter}", out _);
        }

        /// <summary>
        /// Negated Equals
        /// </summary>
        public static bool DiffersFrom(this string theString, string matchString)
        {
            return !theString.Equals(matchString);
        }

		/// <summary>
		/// Negated Equals
		/// </summary>
		public static bool DiffersFrom(this string theString, string matchString, StringComparison stringComparison)
		{
			return !theString.Equals(matchString, stringComparison);
		}

		/// <summary>
		/// Simple validation. Checks theString against nullOrEmpty and throws if necessary
		/// </summary>
		/// <param name="theString">theString</param>
		/// <returns>theString as-is</returns>
		public static string Asserted(this string theString)
        {
            if (theString.IsNullOrWhiteSpace())
            {
                throw new Exception("The string is empty when it must not be.");
            }

            return theString;
        }

        /// <summary>
        /// Appends Environment.Newline(s) to the given string
        /// </summary>
        /// <param name="theString">theString</param>
        /// <param name="linebreaks">The number of linebreaks. Defaults to 1</param>
        /// <returns>theString plus linebreaks</returns>
        public static string Linebreak(this string theString, int linebreaks = 1)
        {
            for (int i = 0; i < linebreaks; i++)
            {
                theString += Environment.NewLine;
            }

            return theString;
        }

        public static Stream ToStream(this string theString)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(theString);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string Without(this string theString, string toBeRemoved)
        {
            return theString.Replace(toBeRemoved, string.Empty);
        }

        public static bool Lacks(this string theString, string match)
        {
            return !theString.Contains(match);
        }

        /// <summary>
        /// Removes the last comma from the string
        /// </summary>
        public static string StripLastComma(this string theString)
        {
            var lastCommaIndex = theString.LastIndexOf(',');
            if (lastCommaIndex < 0)
            {
                return theString;
            }

            return theString.Substring(0, lastCommaIndex);
        }

        public static IEnumerable<string> ToLines(this string theString)
        {
            return theString.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static SecureString ToSecureString(this string str)
        {
            if (str.IsNullOrEmpty()) return null;

            var secure = new SecureString();
            foreach (char c in str)
                secure.AppendChar(c);
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToUnsecureString(this SecureString secureStr)
        {
            if (secureStr == null) return null;
            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secureStr);
                return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

		public static bool IsEqualTo(this SecureString ss1, SecureString ss2)
		{
			IntPtr bstr1 = IntPtr.Zero;
			IntPtr bstr2 = IntPtr.Zero;
			try
			{
				bstr1 = Marshal.SecureStringToBSTR(ss1);
				bstr2 = Marshal.SecureStringToBSTR(ss2);
				int length1 = Marshal.ReadInt32(bstr1, -4);
				int length2 = Marshal.ReadInt32(bstr2, -4);
				if (length1 == length2)
				{
					for (int x = 0; x < length1; ++x)
					{
						byte b1 = Marshal.ReadByte(bstr1, x);
						byte b2 = Marshal.ReadByte(bstr2, x);
						if (b1 != b2) return false;
					}
				}
				else return false;
				return true;
			}
			finally
			{
				if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
				if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
			}
		}

        public static string QueryToRelativeFilePath(this string query)
        {
            return query.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        }
	}
}
