using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("14885CC9-BAB0-4f90-B6ED-5C366A2CD03D")]
public class NumberSubstitution : ComObject
{
	public NumberSubstitution(Factory factory, NumberSubstitutionMethod substitutionMethod, string localeName, bool ignoreUserOverride)
	{
		factory.CreateNumberSubstitution(substitutionMethod, localeName, ignoreUserOverride, this);
	}

	public NumberSubstitution(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator NumberSubstitution(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new NumberSubstitution(nativePointer);
		}
		return null;
	}
}
