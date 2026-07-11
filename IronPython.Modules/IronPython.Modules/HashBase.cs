using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public class HashBase
{
	internal byte[] _bytes;

	private byte[] _hash;

	internal virtual HashAlgorithm Hasher
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal HashBase()
	{
	}

	public void update(Bytes newBytes)
	{
		update((IList<byte>)newBytes);
	}

	public void update(ByteArray newBytes)
	{
		update((IList<byte>)newBytes);
	}

	internal void update(IList<byte> newBytes)
	{
		byte[] array = new byte[_bytes.Length + newBytes.Count];
		Array.Copy(_bytes, array, _bytes.Length);
		newBytes.CopyTo(array, _bytes.Length);
		_bytes = array;
		_hash = Hasher.ComputeHash(_bytes);
	}

	[Documentation("update(string) -> None (update digest with string data)")]
	public void update(object newData)
	{
		update(Converter.ConvertToString(newData).MakeByteArray());
	}

	public void update(PythonBuffer buffer)
	{
		update((IList<byte>)buffer);
	}

	[Documentation("digest() -> int (current digest value)")]
	public string digest()
	{
		return _hash.MakeString();
	}

	[Documentation("hexdigest() -> string (current digest as hex digits)")]
	public string hexdigest()
	{
		StringBuilder stringBuilder = new StringBuilder(2 * _hash.Length);
		for (int i = 0; i < _hash.Length; i++)
		{
			stringBuilder.Append(_hash[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}
}
