using System;

namespace Firaxis.CivTech.AssetObjects;

public struct MeshBounds
{
	public Point3F kMin;

	public Point3F kMax;

	public MeshBounds(Point3F min, Point3F max)
	{
		kMin = min;
		kMax = max;
	}

	public void Expand(Point3F min, Point3F max)
	{
		kMin.x = Math.Min(kMin.x, min.x);
		kMin.y = Math.Min(kMin.y, min.y);
		kMin.z = Math.Min(kMin.z, min.z);
		kMax.x = Math.Max(kMax.x, max.x);
		kMax.y = Math.Max(kMax.y, max.y);
		kMax.z = Math.Max(kMax.z, max.z);
	}

	public void Expand(MeshBounds o)
	{
		Expand(o.kMin, o.kMax);
	}
}
