using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class AnimatableComponentAdapterBase : BehaviorComponentAdapterBase
{
	public IAnimatableEntityAdapter AnimatableEntityAdapter { get; set; }

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		AnimatableEntityAdapter = base.DomNode.GetRoot().As<IAnimatableEntityAdapter>();
	}
}
