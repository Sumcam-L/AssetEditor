using System.ComponentModel.Composition;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Export(typeof(IInitializable))]
[Export(typeof(ITargetProvider))]
[Export(typeof(X86TargetProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class X86TargetProvider : TcpIpTargetProvider
{
	public override string Name => "X86 Target".Localize();

	public override string Id => "Sce.Atf.X86TcpIpTargetProvider";

	public override TargetInfo CreateNew()
	{
		return new X86TargetInfo();
	}
}
