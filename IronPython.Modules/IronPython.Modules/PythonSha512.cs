using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using IronPython.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

[Documentation("SHA512 hash algorithm")]
public static class PythonSha512
{
	[PythonHidden]
	public sealed class Sha384Object : HashBase, ICloneable
	{
		public const int block_size = 128;

		public const int digest_size = 48;

		public const int digestsize = 48;

		public const string name = "SHA384";

		internal override HashAlgorithm Hasher => GetHasher384();

		internal Sha384Object()
			: this(new byte[0])
		{
		}

		internal Sha384Object(object initialData)
		{
			_bytes = new byte[0];
			update(initialData);
		}

		internal Sha384Object(IList<byte> initialBytes)
		{
			_bytes = new byte[0];
			update(initialBytes);
		}

		[Documentation("copy() -> object (copy of this object)")]
		public Sha384Object copy()
		{
			return new Sha384Object(_bytes);
		}

		object ICloneable.Clone()
		{
			return copy();
		}
	}

	[PythonHidden]
	public sealed class Sha512Object : HashBase, ICloneable
	{
		public const int block_size = 128;

		public const int digest_size = 64;

		public const int digestsize = 64;

		public const string name = "SHA512";

		internal override HashAlgorithm Hasher => GetHasher512();

		internal Sha512Object()
			: this(new byte[0])
		{
		}

		internal Sha512Object(object initialData)
		{
			_bytes = new byte[0];
			update(initialData);
		}

		internal Sha512Object(IList<byte> initialBytes)
		{
			_bytes = new byte[0];
			update(initialBytes);
		}

		[Documentation("copy() -> object (copy of this object)")]
		public Sha512Object copy()
		{
			return new Sha512Object(_bytes);
		}

		object ICloneable.Clone()
		{
			return copy();
		}
	}

	private const int blockSize = 128;

	public const string __doc__ = "SHA512 hash algorithm";

	[ThreadStatic]
	private static SHA512 _hasher512;

	[ThreadStatic]
	private static SHA384 _hasher384;

	private static SHA512 GetHasher512()
	{
		if (_hasher512 == null)
		{
			_hasher512 = SHA512.Create();
		}
		return _hasher512;
	}

	private static SHA384 GetHasher384()
	{
		if (_hasher384 == null)
		{
			_hasher384 = SHA384.Create();
		}
		return _hasher384;
	}

	public static Sha512Object sha512(object data)
	{
		return new Sha512Object(data);
	}

	public static Sha512Object sha512(Bytes data)
	{
		return new Sha512Object(data);
	}

	public static Sha512Object sha512(PythonBuffer data)
	{
		return new Sha512Object(data);
	}

	public static Sha512Object sha512(ByteArray data)
	{
		return new Sha512Object(data);
	}

	public static Sha512Object sha512()
	{
		return new Sha512Object();
	}

	public static Sha384Object sha384(object data)
	{
		return new Sha384Object(data);
	}

	public static Sha384Object sha384(Bytes data)
	{
		return new Sha384Object(data);
	}

	public static Sha384Object sha384(PythonBuffer data)
	{
		return new Sha384Object(data);
	}

	public static Sha384Object sha384(ByteArray data)
	{
		return new Sha384Object(data);
	}

	public static Sha384Object sha384()
	{
		return new Sha384Object();
	}
}
