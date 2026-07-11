namespace Firaxis.CivTech.AssetPreviewer;

public class SplineWidgetPickResult : WidgetPickResult, IPointPickComponent
{
	public readonly float SplineParameter;

	public float[] Point { get; private set; }

	public SplineWidgetPickResult(float dist, IWidget wid, float splineParam, float[] pos)
		: base(dist, wid)
	{
		SplineParameter = splineParam;
		Point = pos;
	}
}
