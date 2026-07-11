using System.Collections;
using System.Runtime.InteropServices;

namespace Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

[Guid("CBA7194A-4994-4881-9100-D47DB8D051BD")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface ITargetInfoCollection : IEnumerable
{
	[DispId(1610743808)]
	uint Count
	{
		[DispId(1610743808)]
		get;
	}

	[DispId(0)]
	ITargetInfo this[[In] uint Index]
	{
		[DispId(0)]
		get;
	}
}
