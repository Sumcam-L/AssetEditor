using System.Collections;
using System.Runtime.InteropServices;

namespace Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

[Guid("B053B37D-81ED-4CDC-8F7A-8FE60A165CE7")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
public interface ITargetCollection : IEnumerable
{
	[DispId(1610743808)]
	uint Count { get; }

	[DispId(5)]
	ITarget DefaultTarget { get; }

	[DispId(3)]
	ITarget FirstAvailable { get; }

	[DispId(4)]
	ITarget FirstConnected { get; }

	[DispId(0)]
	ITarget this[uint Index] { get; }

	[DispId(1)]
	ITarget GetByHardwareId(string bstrId);

	[DispId(2)]
	ITarget GetByName(string bstrName);

	[DispId(7)]
	ITargetCollection GetTargetsByHardwareId(string bstrName);

	[DispId(6)]
	ITargetCollection GetTargetsByName(string bstrName);
}
