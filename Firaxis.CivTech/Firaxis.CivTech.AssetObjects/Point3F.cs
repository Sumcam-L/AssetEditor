using System;

namespace Firaxis.CivTech.AssetObjects;

public class Point3F : IEquatable<Point3F>
{
	public static float Epsilon = 0.005f;

	public float x;

	public float y;

	public float z;

	public Point3F(float xVal, float yVal, float zVal)
	{
		x = xVal;
		y = yVal;
		z = zVal;
	}

	public Point3F(Point3F val)
	{
		x = val.x;
		y = val.y;
		z = val.z;
	}

	public bool Equals(Point3F other)
	{
		return other != null && x == other.x && y == other.y && z == other.z;
	}

	public bool EpsilonEquals(Point3F other, float epsilon)
	{
		return Math.Abs(x - other.x) < epsilon && Math.Abs(y - other.y) < epsilon && Math.Abs(z - other.z) < epsilon;
	}
}
