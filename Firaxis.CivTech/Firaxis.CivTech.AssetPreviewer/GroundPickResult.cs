namespace Firaxis.CivTech.AssetPreviewer;

public class GroundPickResult : PickResult, IPointPickComponent
{
	public float[] Point { get; private set; }

	public GroundPickResult(float dist, float[] impact)
		: base(dist)
	{
		Point = impact;
	}
}
