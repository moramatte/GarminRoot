using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Utilities
{
	public class GuidUtils
	{
		public static Guid HashToGuid(string input)
		{
			using SHA1 sha1 = SHA1.Create();
			byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

			byte[] guidBytes = new byte[16];
			Array.Copy(hash, guidBytes, 16); // Copy the first 16 bytes of the hash into the GUID array

			// Adjust the bytes to match the GUID version 5 pattern
			guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50); // Set the version to 5 (0101 in binary)
			guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // Set the variant to RFC 4122

			return new Guid(guidBytes);
		}
	}
}
