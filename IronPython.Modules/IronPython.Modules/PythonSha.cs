using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonSha
{
	[PythonHidden]
	[Documentation("new([data]) -> object (object used to calculate hash)")]
	[PythonType]
	public class sha : ICloneable
	{
		private byte[] _bytes;

		private byte[] _hash;

		public static readonly int digest_size = PythonSha.digest_size;

		public static readonly int block_size = blocksize;

		public sha()
			: this(new byte[0])
		{
		}

		public sha(object initialData)
		{
			_bytes = new byte[0];
			update(initialData);
		}

		internal sha(IList<byte> initialBytes)
		{
			_bytes = new byte[0];
			update(initialBytes);
		}

		[Documentation("update(string) -> None (update digest with string data)")]
		public void update(object newData)
		{
			update(Converter.ConvertToString(newData).MakeByteArray());
		}

		public void update(Bytes newBytes)
		{
			update((IList<byte>)newBytes);
		}

		public void update(PythonBuffer newBytes)
		{
			update((IList<byte>)newBytes);
		}

		public void update(ByteArray newBytes)
		{
			update((IList<byte>)newBytes);
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

		[Documentation("copy() -> object (copy of this object)")]
		public sha copy()
		{
			return new sha(_bytes);
		}

		object ICloneable.Clone()
		{
			return copy();
		}
	}

	public const string __doc__ = "implements the SHA1 hash algorithm";

	private const int blockSize = 64;

	[ThreadStatic]
	private static SHA1Managed _hasher;

	public static int digest_size
	{
		[Documentation("Size of the resulting digest in bytes (constant)")]
		get
		{
			return GetHasher().HashSize / 8;
		}
	}

	public static int digestsize
	{
		[Documentation("Size of the resulting digest in bytes (constant)")]
		get
		{
			return digest_size;
		}
	}

	public static int blocksize
	{
		[Documentation("Block size")]
		get
		{
			return 64;
		}
	}

	private static SHA1Managed GetHasher()
	{
		if (_hasher == null)
		{
			_hasher = new SHA1Managed();
		}
		return _hasher;
	}

	[Documentation("new([data]) -> object (object used to calculate hash)")]
	public static sha @new(object data)
	{
		return new sha(data);
	}

	[Documentation("new([data]) -> object (object used to calculate hash)")]
	public static sha @new(Bytes data)
	{
		return new sha(data);
	}

	[Documentation("new([data]) -> object (object used to calculate hash)")]
	public static sha @new(PythonBuffer data)
	{
		return new sha(data);
	}

	[Documentation("new([data]) -> object (object used to calculate hash)")]
	public static sha @new(ByteArray data)
	{
		return new sha(data);
	}

	[Documentation("new([data]) -> object (object used to calculate hash)")]
	public static sha @new()
	{
		return new sha();
	}
}
