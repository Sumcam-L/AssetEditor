namespace Firaxis.CivTech.AssetPreviewer;

public class PickResult
{
	public readonly float DistanceToRayImpact;

	protected PickResult(float dist)
	{
		DistanceToRayImpact = dist;
	}
}
