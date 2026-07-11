using System.ComponentModel.Composition;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Export(typeof(IInitializable))]
[Export(typeof(ITargetProvider))]
[Export(typeof(Ps3TargetProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class Ps3TargetProvider : TcpIpTargetProvider
{
	public override string Name => "PS3 Target".Localize();

	public override string Id => "Sce.Atf.Ps3TcpIpTargetProvider";

	public override TargetInfo CreateNew()
	{
		return new Ps3TargetInfo();
	}
}
