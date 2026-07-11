using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace fbxsdk;

[StructLayout(LayoutKind.Sequential, Size = 8)]
[NativeCppClass]
internal struct FbxStringSymbol
{
	private long _003Calignment_0020member_003E;

	[SpecialName]
	public unsafe static void _003CMarshalCopy_003E(FbxStringSymbol* A_0, FbxStringSymbol* A_1)
	{
		global::_003CModule_003E.fbxsdk_002EFbxStringSymbol_002E_007Bctor_007D(A_0, A_1);
	}

	[SpecialName]
	public unsafe static void _003CMarshalDestroy_003E(FbxStringSymbol* A_0)
	{
		global::_003CModule_003E.fbxsdk_002EFbxStringSymbol_002E_007Bdtor_007D(A_0);
	}
}
