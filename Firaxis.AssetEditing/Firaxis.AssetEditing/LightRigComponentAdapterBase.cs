using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class LightRigComponentAdapterBase : EntityComponentAdapterBase
{
	private protected LightRigAdapter LightRigAdapter { get; set; }

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		LightRigAdapter = base.DomNode.GetRoot().As<LightRigAdapter>();
	}
}
