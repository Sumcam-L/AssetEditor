using System.Runtime.InteropServices;

namespace Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

[Guid("5B04E7E7-1AAB-4CBA-8AF0-DA18F7C0B21F")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IPsp2TmApi
{
	[DispId(2)]
	ITargetCollection Targets { get; }

	void CheckCompatibility(uint uExpectedVersion);

	ITargetInfoCollection DiscoverAllTargets();

	void Dispose();
}
