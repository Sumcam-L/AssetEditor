using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace std;

[StructLayout(LayoutKind.Sequential, Size = 8)]
[NativeCppClass]
internal struct tuple_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_0026_003E
{
	private long _003Calignment_0020member_003E;

	[SpecialName]
	public unsafe static void _003CMarshalCopy_003E(tuple_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_0026_003E* A_0, tuple_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_0026_003E* A_1)
	{
		global::_003CModule_003E.std_002Etuple_003C_003E_002E_007Bctor_007D((tuple_003C_003E*)A_0, (tuple_003C_003E*)A_1);
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock(A_0, A_1, 8);
	}
}
