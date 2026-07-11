using System.Runtime.InteropServices;

namespace Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

[Guid("2296BA32-7E8B-46C8-838D-290A03094819")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface ITargetInfo
{
	[DispId(1)]
	string HardwareId
	{
		[DispId(1)]
		get;
	}

	[DispId(2)]
	string Name
	{
		[DispId(2)]
		get;
	}

	[DispId(3)]
	string Host
	{
		[DispId(3)]
		get;
	}

	[DispId(6)]
	string ConnectionInfo
	{
		[DispId(6)]
		get;
	}

	[DispId(7)]
	ePowerStatus PowerStatus
	{
		[DispId(7)]
		get;
	}

	[DispId(8)]
	uint CPPackageVersion
	{
		[DispId(8)]
		get;
	}

	[DispId(9)]
	uint NetworkManagerVersion
	{
		[DispId(9)]
		get;
	}
}
