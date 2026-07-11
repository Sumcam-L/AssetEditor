using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class BehaviorComponentAdapterBase : EntityComponentAdapterBase
{
	public IBehaviorProviderAdapter BehaviorProvider { get; set; }

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		BehaviorProvider = base.DomNode.GetRoot().As<IBehaviorProviderAdapter>();
	}
}
