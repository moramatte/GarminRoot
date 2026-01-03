using System;
using Infrastructure.Extensions;

namespace Infrastructure.Utilities
{
    public static class CommandLineParser
    {
        public static bool HasFlag<TEnum>(this string[] args, TEnum flag)
        {
            return HasFlag(args.ToWhitespaceSeparatedString(), flag);
        }

        public static string GetSwitch<TEnum>(this string[] args, TEnum flag)
        {
            return GetSwitch(args.ToWhitespaceSeparatedString(), flag);
        }

        public static bool HasFlag<TEnum>(this string args, TEnum flag)
        {
            var flagName = TrimSwitch(flag);
           
            if (args.Contains(flagName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetSwitch<TEnum>(this string args, TEnum flag)
        {
            if (!HasFlag(args, flag))
            {
                throw new Exception($"Missing expected switch {flag}");
            }

            var theSwitch = TrimSwitch(flag);
            var parts = args.Split(' ');
            for (int i = 0; i < parts.Length; i++)
            {
                {
                    var part = TrimSwitch(parts[i]);
                    if (theSwitch == part)
                    {
                        return parts[i + i];
                    }
                }
            }
            throw new Exception($"Did not find switch {flag}");
        }

        private static string TrimSwitch<TEnum>(TEnum part)
        {
            return TrimSwitch(part.ToString());
        }

        private static string TrimSwitch(string part)
        {
            return part.Trim('-').Trim('/');
        }
    }
}
