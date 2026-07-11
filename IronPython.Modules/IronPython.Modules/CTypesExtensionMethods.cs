using System;

namespace IronPython.Modules;

internal static class CTypesExtensionMethods
{
	public static IntPtr Add(this IntPtr self, int offset)
	{
		return new IntPtr(checked(self.ToInt64() + offset));
	}
}
