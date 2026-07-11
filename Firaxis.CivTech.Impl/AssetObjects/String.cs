using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AssetObjects;

[StructLayout(LayoutKind.Sequential, Size = 24)]
[NativeCppClass]
internal struct String
{
	private long _003Calignment_0020member_003E;

	[SpecialName]
	public unsafe static void _003CMarshalCopy_003E(String* A_0, String* A_1)
	{
		global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D(A_0, A_1);
	}

	[SpecialName]
	public unsafe static void _003CMarshalDestroy_003E(String* A_0)
	{
		global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D(A_0);
	}
}
