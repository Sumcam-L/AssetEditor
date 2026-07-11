using System.Runtime.InteropServices;

namespace Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

[Guid("FF0B24DF-D981-400A-B842-E7E5565F5BBB")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
public interface ITarget
{
	[DispId(31)]
	string HardwareId { get; }

	[DispId(40)]
	string Name { get; set; }
}
