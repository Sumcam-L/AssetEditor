using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonMD5
{
	[PythonType]
	[Documentation("new([data]) -> object (object used to calculate MD5 hash)")]
	public class MD5Type : ICloneable
	{
		public const int digest_size = 16;

		public const int digestsize = 16;

		private byte[] _bytes;

		private byte[] _hash;

		public string name => "MD5";

		public int block_size => GetHasher().InputBlockSize;

		public MD5Type()
			: this(new byte[0])
		{
		}

		public MD5Type(object initialData)
		{
			_bytes = new byte[0];
			update(initialData);
		}

		internal MD5Type(IList<byte> initialBytes)
		{
			_bytes = new byte[0];
			update(initialBytes);
		}

		[Documentation("update(string) -> None (update digest with string data)")]
		public void update(object newData)
		{
			update(Converter.ConvertToString(newData).MakeByteArray());
		}

		[Documentation("update(bytes) -> None (update digest with string data)")]
		public void update(Bytes newData)
		{
			update((IList<byte>)newData);
		}

		[Documentation("update(bytes) -> None (update digest with string data)")]
		public void update(ByteArray newData)
		{
			update((IList<byte>)newData);
		}

		[Documentation("update(bytes) -> None (update digest with string data)")]
		public void update(PythonBuffer newData)
		{
			update((IList<byte>)newData);
		}

		private void update(IList<byte> newBytes)
		{
			byte[] array = new byte[_bytes.Length + newBytes.Count];
			Array.Copy(_bytes, array, _bytes.Length);
			newBytes.CopyTo(array, _bytes.Length);
			_bytes = array;
			_hash = GetHasher().ComputeHash(_bytes);
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

		[Documentation("copy() -> object (copy of this md5 object)")]
		public MD5Type copy()
		{
			return new MD5Type(_bytes);
		}

		object ICloneable.Clone()
		{
			return copy();
		}
	}

	public const string __doc__ = "MD5 hash algorithm";

	[ThreadStatic]
	private static MD5CryptoServiceProvider _hasher;

	public static int digest_size
	{
		[Documentation("Size of the resulting digest in bytes (constant)")]
		get
		{
			return GetHasher().HashSize / 8;
		}
	}

	private static MD5CryptoServiceProvider GetHasher()
	{
		if (_hasher == null)
		{
			_hasher = new MD5CryptoServiceProvider();
		}
		return _hasher;
	}

	[Documentation("new([data]) -> object (new md5 object)")]
	public static MD5Type @new(object data)
	{
		return new MD5Type(data);
	}

	[Documentation("new([data]) -> object (new md5 object)")]
	public static MD5Type @new(Bytes data)
	{
		return new MD5Type(data);
	}

	[Documentation("new([data]) -> object (new md5 object)")]
	public static MD5Type @new(PythonBuffer data)
	{
		return new MD5Type(data);
	}

	[Documentation("new([data]) -> object (new md5 object)")]
	public static MD5Type @new(ByteArray data)
	{
		return new MD5Type(data);
	}

	[Documentation("new([data]) -> object (new md5 object)")]
	public static MD5Type @new()
	{
		return new MD5Type();
	}
}
