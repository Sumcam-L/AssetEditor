using System;

namespace Sce.Atf.VectorMath;

public struct Plane3F : IFormattable
{
	public Vec3F Normal;

	public float Distance;

	public Plane3F(Vec3F normal, float distance)
	{
		Normal = normal;
		Distance = distance;
	}

	public Plane3F(Vec3F normal, Vec3F pointOnPlane)
	{
		Normal = normal;
		Distance = Vec3F.Dot(normal, pointOnPlane);
	}

	public Plane3F(Vec3F p1, Vec3F p2, Vec3F p3)
	{
		Vec3F v = p2 - p1;
		Vec3F v2 = p3 - p1;
		Vec3F v3 = Vec3F.Cross(v, v2);
		Normal = Vec3F.Normalize(v3);
		Distance = Vec3F.Dot(Normal, p1);
	}

	public Plane3F(Plane3F plane)
	{
		Normal = plane.Normal;
		Distance = plane.Distance;
	}

	public void Set(Vec3F n, float d)
	{
		Normal = n;
		Distance = d;
	}

	public Vec3F PointOnPlane()
	{
		return Vec3F.Mul(Normal, Distance);
	}

	public float SignedDistance(Vec3F point)
	{
		return Vec3F.Dot(Normal, point) - Distance;
	}

	public Vec3F Project(Vec3F point)
	{
		return point - SignedDistance(point) * Normal;
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		return string.Format("{0}{4} {1}{4} {2}{4} {3}", ((double)Normal.X).ToString(format, formatProvider), ((double)Normal.Y).ToString(format, formatProvider), ((double)Normal.Z).ToString(format, formatProvider), ((double)Distance).ToString(format, formatProvider), numberListSeparator);
	}
}
