using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using IronPython.Runtime;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

[Documentation("SHA256 hash algorithm")]
public static class PythonSha256
{
	[PythonHidden]
	public sealed class Sha256Object : HashBase, ICloneable
	{
		public const int block_size = 64;

		public const int digest_size = 32;

		public const int digestsize = 32;

		public const string name = "SHA256";

		internal override HashAlgorithm Hasher => GetHasher();

		internal Sha256Object()
			: this(new byte[0])
		{
		}

		internal Sha256Object(object initialData)
		{
			_bytes = new byte[0];
			update(initialData);
		}

		internal Sha256Object(IList<byte> initialBytes)
		{
			_bytes = new byte[0];
			update(initialBytes);
		}

		[Documentation("copy() -> object (copy of this object)")]
		public Sha256Object copy()
		{
			return new Sha256Object(_bytes);
		}

		object ICloneable.Clone()
		{
			return copy();
		}
	}

	private const int blockSize = 64;

	public const string __doc__ = "SHA256 hash algorithm";

	[ThreadStatic]
	private static SHA256 _hasher256;

	private static SHA256 GetHasher()
	{
		if (_hasher256 == null)
		{
			_hasher256 = new SHA256Managed();
		}
		return _hasher256;
	}

	public static Sha256Object sha256(object data)
	{
		return new Sha256Object(data);
	}

	public static Sha256Object sha256(Bytes data)
	{
		return new Sha256Object(data);
	}

	public static Sha256Object sha256(PythonBuffer data)
	{
		return new Sha256Object(data);
	}

	public static Sha256Object sha256(ByteArray data)
	{
		return new Sha256Object(data);
	}

	public static Sha256Object sha256()
	{
		return new Sha256Object();
	}

	public static Sha256Object sha224(object data)
	{
		throw new NotImplementedException();
	}

	public static Sha256Object sha224()
	{
		throw new NotImplementedException();
	}
}
