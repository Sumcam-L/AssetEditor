namespace Firaxis.CivTech.AssetObjects;

public interface IAnimatable
{
	string DSGName { get; set; }

	IAnimationBindingSet AnimationBindings { get; }
}
