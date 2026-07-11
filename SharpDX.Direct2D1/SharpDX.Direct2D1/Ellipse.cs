namespace SharpDX.Direct2D1;

public struct Ellipse
{
	public Vector2 Point;

	public float RadiusX;

	public float RadiusY;

	public Ellipse(Vector2 center, float radiusX, float radiusY)
	{
		Point = center;
		RadiusX = radiusX;
		RadiusY = radiusY;
	}
}
