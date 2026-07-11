using System.Collections.Generic;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Operations;

namespace IronPython.Modules;

public static class PythonMarshal
{
	public const string __doc__ = "Provides functions for serializing and deserializing primitive data types.";

	public const int version = 2;

	public static void dump(object value, PythonFile file)
	{
		dump(value, file, 2);
	}

	public static void dump(object value, PythonFile file, int version)
	{
		if (file == null)
		{
			throw PythonOps.TypeError("expected file, found None");
		}
		file.write(dumps(value, version));
	}

	public static object load(PythonFile file)
	{
		if (file == null)
		{
			throw PythonOps.TypeError("expected file, found None");
		}
		return MarshalOps.GetObject(FileEnumerator(file));
	}

	public static object dumps(object value)
	{
		return dumps(value, 2);
	}

	public static string dumps(object value, int version)
	{
		byte[] bytes = MarshalOps.GetBytes(value, version);
		StringBuilder stringBuilder = new StringBuilder(bytes.Length);
		for (int i = 0; i < bytes.Length; i++)
		{
			stringBuilder.Append((char)bytes[i]);
		}
		return stringBuilder.ToString();
	}

	public static object loads(string @string)
	{
		return MarshalOps.GetObject(StringEnumerator(@string));
	}

	private static IEnumerator<byte> FileEnumerator(PythonFile file)
	{
		while (true)
		{
			string data = file.read(1);
			if (data.Length != 0)
			{
				yield return (byte)data[0];
				continue;
			}
			break;
		}
	}

	private static IEnumerator<byte> StringEnumerator(string str)
	{
		for (int i = 0; i < str.Length; i++)
		{
			yield return (byte)str[i];
		}
	}
}
