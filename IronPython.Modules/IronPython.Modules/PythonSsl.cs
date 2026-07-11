using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonSsl
{
	public const string __doc__ = "Implementation module for SSL socket operations.";

	public const int OPENSSL_VERSION_NUMBER = 9437184;

	public const string OPENSSL_VERSION = "OpenSSL 0.0.0 (.NET SSL)";

	private const int ClassOffset = 6;

	private const int ClassMask = 192;

	private const int ClassUniversal = 0;

	private const int ClassApplication = 64;

	private const int ClassContextSpecific = 128;

	private const int ClassPrivate = 192;

	private const int NumberMask = 31;

	private const int UnivesalSequence = 16;

	private const int UniversalInteger = 2;

	private const int UniversalOctetString = 4;

	public const int CERT_NONE = 0;

	public const int CERT_OPTIONAL = 1;

	public const int CERT_REQUIRED = 2;

	public const int PROTOCOL_SSLv2 = 0;

	public const int PROTOCOL_SSLv3 = 1;

	public const int PROTOCOL_SSLv23 = 2;

	public const int PROTOCOL_TLSv1 = 3;

	public const int SSL_ERROR_SSL = 1;

	public const int SSL_ERROR_WANT_READ = 2;

	public const int SSL_ERROR_WANT_WRITE = 3;

	public const int SSL_ERROR_WANT_X509_LOOKUP = 4;

	public const int SSL_ERROR_SYSCALL = 5;

	public const int SSL_ERROR_ZERO_RETURN = 6;

	public const int SSL_ERROR_WANT_CONNECT = 7;

	public const int SSL_ERROR_EOF = 8;

	public const int SSL_ERROR_INVALID_ERROR_CODE = 9;

	public static PythonTuple OPENSSL_VERSION_INFO = PythonTuple.MakeTuple(0, 0, 0, 0, 0);

	public static PythonType SSLType = DynamicHelpers.GetPythonTypeFromType(typeof(PythonSocket.ssl));

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		PythonModule builtinModule = context.GetBuiltinModule("socket");
		PythonType socketError = PythonSocket.GetSocketError(context, builtinModule.__dict__);
		context.EnsureModuleException("SSLError", socketError, dict, "SSLError", "ssl");
	}

	public static void RAND_add(string buf, double entropy)
	{
	}

	public static void RAND_egd(string source)
	{
	}

	public static int RAND_status()
	{
		return 1;
	}

	public static PythonSocket.ssl sslwrap(CodeContext context, PythonSocket.socket socket, bool server_side, [DefaultParameterValue(null)] string keyfile, [DefaultParameterValue(null)] string certfile, [DefaultParameterValue(0)] int certs_mode, [DefaultParameterValue(-1)] int protocol, [DefaultParameterValue(null)] string cacertsfile, [DefaultParameterValue(null)] object ciphers)
	{
		return new PythonSocket.ssl(context, socket, server_side, keyfile, certfile, certs_mode, protocol, cacertsfile);
	}

	internal static PythonType SSLError(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("SSLError");
	}

	public static PythonDictionary _test_decode_cert(CodeContext context, string filename, [DefaultParameterValue(false)] bool complete)
	{
		X509Certificate2 cert = ReadCertificate(context, filename);
		return CertificateToPython(context, cert, complete);
	}

	internal static PythonDictionary CertificateToPython(CodeContext context, X509Certificate cert, bool complete)
	{
		CommonDictionaryStorage commonDictionaryStorage = new CommonDictionaryStorage();
		commonDictionaryStorage.AddNoLock("notAfter", ToPythonDateFormat(cert.GetExpirationDateString()));
		commonDictionaryStorage.AddNoLock("subject", IssuerToPython(context, cert.Subject));
		if (complete)
		{
			commonDictionaryStorage.AddNoLock("notBefore", ToPythonDateFormat(cert.GetEffectiveDateString()));
			commonDictionaryStorage.AddNoLock("serialNumber", SerialNumberToPython(cert));
			commonDictionaryStorage.AddNoLock("version", cert.GetCertHashString());
			commonDictionaryStorage.AddNoLock("issuer", IssuerToPython(context, cert.Issuer));
		}
		return new PythonDictionary(commonDictionaryStorage);
	}

	private static string ToPythonDateFormat(string date)
	{
		return DateTime.Parse(date).ToUniversalTime().ToString("MMM d HH:mm:ss yyyy") + " GMT";
	}

	private static string SerialNumberToPython(X509Certificate cert)
	{
		string serialNumberString = cert.GetSerialNumberString();
		for (int i = 0; i < serialNumberString.Length; i++)
		{
			if (serialNumberString[i] != '0')
			{
				return serialNumberString.Substring(i);
			}
		}
		return serialNumberString;
	}

	private static PythonTuple IssuerToPython(CodeContext context, string issuer)
	{
		string[] array = issuer.Split(',');
		object[] array2 = new object[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = PythonTuple.MakeTuple(IssuerFieldToPython(context, array[i].Trim()));
		}
		return PythonTuple.MakeTuple(array2);
	}

	private static PythonTuple IssuerFieldToPython(CodeContext context, string p)
	{
		if (string.Compare(p, 0, "CN=", 0, 3) == 0)
		{
			return PythonTuple.MakeTuple("commonName", p.Substring(3));
		}
		if (string.Compare(p, 0, "OU=", 0, 3) == 0)
		{
			return PythonTuple.MakeTuple("organizationalUnitName", p.Substring(3));
		}
		if (string.Compare(p, 0, "O=", 0, 2) == 0)
		{
			return PythonTuple.MakeTuple("organizationName", p.Substring(2));
		}
		if (string.Compare(p, 0, "L=", 0, 2) == 0)
		{
			return PythonTuple.MakeTuple("localityName", p.Substring(2));
		}
		if (string.Compare(p, 0, "S=", 0, 2) == 0)
		{
			return PythonTuple.MakeTuple("stateOrProvinceName", p.Substring(2));
		}
		if (string.Compare(p, 0, "C=", 0, 2) == 0)
		{
			return PythonTuple.MakeTuple("countryName", p.Substring(2));
		}
		if (string.Compare(p, 0, "E=", 0, 2) == 0)
		{
			return PythonTuple.MakeTuple("email", p.Substring(2));
		}
		throw PythonExceptions.CreateThrowable(SSLError(context), "Unknown field: ", p);
	}

	internal static X509Certificate2 ReadCertificate(CodeContext context, string filename)
	{
		string[] array;
		try
		{
			array = File.ReadAllLines(filename);
		}
		catch (IOException)
		{
			throw PythonExceptions.CreateThrowable(SSLError(context), "Can't open file ", filename);
		}
		X509Certificate2 x509Certificate = null;
		RSACryptoServiceProvider rSACryptoServiceProvider = null;
		try
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == "-----BEGIN CERTIFICATE-----")
				{
					string text = ReadToEnd(array, ref i, "-----END CERTIFICATE-----");
					try
					{
						x509Certificate = new X509Certificate2(Convert.FromBase64String(text.ToString()));
					}
					catch (Exception ex2)
					{
						throw ErrorDecoding(context, filename, ex2);
					}
				}
				else if (array[i] == "-----BEGIN RSA PRIVATE KEY-----")
				{
					string text2 = ReadToEnd(array, ref i, "-----END RSA PRIVATE KEY-----");
					try
					{
						byte[] x = Convert.FromBase64String(text2.ToString());
						rSACryptoServiceProvider = ParsePkcs1DerEncodedPrivateKey(context, filename, x);
					}
					catch (Exception ex3)
					{
						throw ErrorDecoding(context, filename, ex3);
					}
				}
			}
		}
		catch (InvalidOperationException ex4)
		{
			throw ErrorDecoding(context, filename, ex4.Message);
		}
		if (x509Certificate != null)
		{
			if (rSACryptoServiceProvider != null)
			{
				try
				{
					x509Certificate.PrivateKey = rSACryptoServiceProvider;
				}
				catch (CryptographicException ex5)
				{
					throw ErrorDecoding(context, filename, "cert and private key are incompatible", ex5);
				}
			}
			return x509Certificate;
		}
		throw ErrorDecoding(context, filename, "certificate not found");
	}

	private static RSACryptoServiceProvider ParsePkcs1DerEncodedPrivateKey(CodeContext context, string filename, byte[] x)
	{
		if ((x[0] & 0xC0) != 0)
		{
			throw ErrorDecoding(context, filename, "failed to find universal class");
		}
		if ((x[0] & 0x1F) != 16)
		{
			throw ErrorDecoding(context, filename, "failed to read sequence header");
		}
		int offset = 1;
		ReadLength(x, ref offset);
		int num = ReadUnivesalInt(x, ref offset);
		if (num != 0)
		{
			throw new InvalidOperationException($"bad vesion: {num}");
		}
		RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
		rSACryptoServiceProvider.ImportParameters(new RSAParameters
		{
			Modulus = ReadUnivesalIntAsBytes(x, ref offset),
			Exponent = ReadUnivesalIntAsBytes(x, ref offset),
			D = ReadUnivesalIntAsBytes(x, ref offset),
			P = ReadUnivesalIntAsBytes(x, ref offset),
			Q = ReadUnivesalIntAsBytes(x, ref offset),
			DP = ReadUnivesalIntAsBytes(x, ref offset),
			DQ = ReadUnivesalIntAsBytes(x, ref offset),
			InverseQ = ReadUnivesalIntAsBytes(x, ref offset)
		});
		return rSACryptoServiceProvider;
	}

	private static byte[] ReadUnivesalIntAsBytes(byte[] x, ref int offset)
	{
		ReadIntType(x, ref offset);
		int num = ReadLength(x, ref offset);
		while (x[offset] == 0)
		{
			num--;
			offset++;
		}
		byte[] array = new byte[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = x[offset++];
		}
		return array;
	}

	private static void ReadIntType(byte[] x, ref int offset)
	{
		int num = x[offset++];
		if (num != 2)
		{
			throw new InvalidOperationException($"expected version, fonud {num}");
		}
	}

	private static int ReadUnivesalInt(byte[] x, ref int offset)
	{
		ReadIntType(x, ref offset);
		return ReadInt(x, ref offset);
	}

	private static int ReadLength(byte[] x, ref int offset)
	{
		int num = x[offset++];
		if ((num & 0x80) == 0)
		{
			return num;
		}
		return ReadInt(x, ref offset, num & -129);
	}

	private static int ReadInt(byte[] x, ref int offset, int bytes)
	{
		if (bytes + offset > x.Length)
		{
			throw new InvalidOperationException();
		}
		int num = 0;
		for (int i = 0; i < bytes; i++)
		{
			num = (num << 8) | x[offset++];
		}
		return num;
	}

	private static int ReadInt(byte[] x, ref int offset)
	{
		int bytes = x[offset++];
		return ReadInt(x, ref offset, bytes);
	}

	private static string ReadToEnd(string[] lines, ref int start, string end)
	{
		StringBuilder stringBuilder = new StringBuilder();
		start++;
		while (start < lines.Length)
		{
			if (lines[start] == end)
			{
				return stringBuilder.ToString();
			}
			stringBuilder.Append(lines[start]);
			start++;
		}
		return null;
	}

	private static Exception ErrorDecoding(CodeContext context, params object[] args)
	{
		return PythonExceptions.CreateThrowable(SSLError(context), ArrayUtils.Insert("Error decoding PEM-encoded file ", args));
	}
}
