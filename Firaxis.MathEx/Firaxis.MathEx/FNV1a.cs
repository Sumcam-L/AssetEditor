using System.IO;
using System.Linq;

namespace Firaxis.MathEx;

public static class FNV1a
{
	private static class HashDetails
	{
		public static readonly int DirectorySeparatorHashCode = Path.DirectorySeparatorChar.GetHashCode();

		public static readonly uint OffsetBasis32 = 2166136261u;

		public static readonly uint Prime32 = 16777619u;

		public static readonly ulong OffsetBasis64 = 14695981039346656037uL;

		public static readonly ulong Prime64 = 1099511628211uL;
	}

	private static uint nand(uint a, uint b)
	{
		return ~(a & b);
	}

	private static ulong nand(ulong a, ulong b)
	{
		return ~(a & b);
	}

	private static uint xor(uint a, uint b)
	{
		return nand(nand(a, nand(a, b)), nand(b, nand(a, b)));
	}

	private static ulong xor(ulong a, ulong b)
	{
		return nand(nand(a, nand(a, b)), nand(b, nand(a, b)));
	}

	public static uint HashString32(string str, bool bIgnoreCase)
	{
		return str.Aggregate(HashDetails.OffsetBasis32, (uint r, char o) => (o != Path.AltDirectorySeparatorChar) ? (xor(r, (uint)(bIgnoreCase ? char.ToUpperInvariant(o) : o).GetHashCode()) * HashDetails.Prime32) : (xor(r, (uint)HashDetails.DirectorySeparatorHashCode) * HashDetails.Prime32));
	}

	public static ulong HashString64(string str, bool bIgnoreCase)
	{
		return str.Aggregate(HashDetails.OffsetBasis64, (ulong r, char o) => (o != Path.AltDirectorySeparatorChar) ? (xor(r, (ulong)(bIgnoreCase ? char.ToUpperInvariant(o) : o).GetHashCode()) * HashDetails.Prime64) : (xor(r, (ulong)HashDetails.DirectorySeparatorHashCode) * HashDetails.Prime64));
	}

	public static uint HashValue32(params object[] objs)
	{
		return objs.Aggregate(HashDetails.OffsetBasis32, (uint r, object o) => xor(r, (uint)o.GetHashCode()) * HashDetails.Prime32);
	}

	public static ulong HashValue64(params object[] objs)
	{
		return objs.Aggregate(HashDetails.OffsetBasis64, (ulong r, object o) => xor(r, (ulong)o.GetHashCode()) * HashDetails.Prime64);
	}
}
