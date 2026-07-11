namespace Sce.Atf.Controls.Adaptable.Graphs;

public class EdgeStyleData
{
	public enum EdgeShape
	{
		Default,
		Line,
		Bezier,
		BezierSpline,
		Polyline,
		None
	}

	public EdgeShape ShapeType { get; set; }

	public float Thickness { get; set; }

	public object EdgeData { get; set; }

	public EdgeStyleData()
	{
		ShapeType = EdgeShape.Default;
		Thickness = 1.5f;
	}
}
