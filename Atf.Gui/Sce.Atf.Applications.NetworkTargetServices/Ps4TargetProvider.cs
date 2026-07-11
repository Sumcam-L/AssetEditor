using System.ComponentModel.Composition;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Export(typeof(IInitializable))]
[Export(typeof(ITargetProvider))]
[Export(typeof(Ps4TargetProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class Ps4TargetProvider : TcpIpTargetProvider
{
	public override string Name => "PS4 Target".Localize();

	public override string Id => "Sce.Atf.Ps4TcpIpTargetProvider";

	public override TargetInfo CreateNew()
	{
		return new Ps4TargetInfo();
	}
}
