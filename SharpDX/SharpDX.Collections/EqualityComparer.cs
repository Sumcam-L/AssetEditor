using System;
using System.Collections.Generic;

namespace SharpDX.Collections;

public static class EqualityComparer
{
	internal class IntPtrComparer : EqualityComparer<IntPtr>
	{
		public override bool Equals(IntPtr x, IntPtr y)
		{
			return x == y;
		}

		public override int GetHashCode(IntPtr obj)
		{
			return obj.GetHashCode();
		}
	}

	public static readonly IEqualityComparer<IntPtr> DefaultIntPtr = new IntPtrComparer();
}
