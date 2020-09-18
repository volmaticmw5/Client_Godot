using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

public class Security
{
	public static byte[] GetSalt()
	{
		return Encoding.ASCII.GetBytes("gJUF6ZvYNSG2PwvJfBCBT3hx");
	}

	public static byte[] Hash(string value, byte[] salt)
	{
		return Hash(Encoding.UTF8.GetBytes(value), salt);
	}

	public static byte[] Hash(byte[] value, byte[] salt)
	{
		byte[] saltedValue = value.Concat(salt).ToArray();
		return new SHA256Managed().ComputeHash(saltedValue);
	}

	public static bool Verify(string password, string hashedPassword)
	{
		byte[] passwordHash = Hash(password, GetSalt());
		return passwordHash.SequenceEqual(passwordHash);
	}
}
