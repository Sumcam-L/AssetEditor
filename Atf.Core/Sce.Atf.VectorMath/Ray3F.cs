using System;

namespace Sce.Atf.VectorMath;

public struct Ray3F : IFormattable
{
	public Vec3F Origin;

	public Vec3F Direction;

	private static readonly Vec3F s_blankPoint;

	public Ray3F(Vec3F origin, Vec3F direction)
	{
		Origin = origin;
		Direction = direction;
	}

	public Vec3F IntersectPlane(Vec3F planeNormal, float planeDistance)
	{
		float num = (0f - (Vec3F.Dot(planeNormal, Origin) + planeDistance)) / Vec3F.Dot(planeNormal, Direction);
		return Origin + Direction * num;
	}

	public bool IntersectPlane(Plane3F plane, out Vec3F intersectionPoint)
	{
		float num = Vec3F.Dot(plane.Normal, Direction);
		if (Math.Abs(num) > 0.0001f)
		{
			float num2 = plane.SignedDistance(Origin);
			if ((double)(num * num2) < 0.0)
			{
				float num3 = (0f - num2) / num;
				intersectionPoint = Origin + Direction * num3;
				return true;
			}
		}
		intersectionPoint = new Vec3F(0f, 0f, 0f);
		return false;
	}

	public bool IntersectPolygon(Vec3F[] vertices, out Vec3F intersectionPoint)
	{
		Vec3F nearestVert;
		Vec3F normal;
		return IntersectPolygon(vertices, out intersectionPoint, out nearestVert, out normal, backFaceCull: false);
	}

	public bool IntersectPolygon(Vec3F[] vertices, out Vec3F intersectionPoint, out Vec3F nearestVert, out Vec3F normal, bool backFaceCull)
	{
		bool flag = true;
		int num = 0;
		normal = default(Vec3F);
		for (int i = 2; i < vertices.Length; i++)
		{
			normal = Vec3F.Cross(vertices[i] - vertices[1], vertices[0] - vertices[1]);
			float lengthSquared = normal.LengthSquared;
			if (lengthSquared != 0f)
			{
				normal *= 1f / (float)Math.Sqrt(lengthSquared);
				break;
			}
		}
		if (backFaceCull && (double)Vec3F.Dot(normal, Direction) >= 0.0)
		{
			intersectionPoint = default(Vec3F);
			nearestVert = default(Vec3F);
			return false;
		}
		flag = IntersectPlane(new Plane3F(normal, vertices[1]), out intersectionPoint);
		for (int j = 0; j < vertices.Length && flag; j++)
		{
			int num2 = j;
			int num3 = ((j == 0) ? (vertices.Length - 1) : (j - 1));
			Vec3F u = Vec3F.Cross(vertices[num2] - vertices[num3], intersectionPoint - vertices[num3]);
			if (u.LengthSquared == 0f)
			{
				flag = (vertices[num2] - vertices[num3]).LengthSquared >= (intersectionPoint - vertices[num3]).LengthSquared;
				break;
			}
			float value = Vec3F.Dot(u, normal);
			if (j == 0)
			{
				num = Math.Sign(value);
			}
			else if (Math.Sign(value) != num)
			{
				flag = false;
			}
		}
		if (flag)
		{
			nearestVert = vertices[0];
			float num4 = (intersectionPoint - nearestVert).LengthSquared;
			for (int k = 1; k < vertices.Length; k++)
			{
				float lengthSquared2 = (vertices[k] - intersectionPoint).LengthSquared;
				if (lengthSquared2 < num4)
				{
					num4 = lengthSquared2;
					nearestVert = vertices[k];
				}
			}
		}
		else
		{
			nearestVert = s_blankPoint;
		}
		return flag;
	}

	public Vec3F ProjectPoint(Vec3F point)
	{
		Vec3F vec3F = Vec3F.Normalize(Direction);
		float num = Vec3F.Dot(point - Origin, vec3F);
		return Origin + num * vec3F;
	}

	public float GetDistanceToProjection(Vec3F p)
	{
		return Vec3F.Dot(p - Origin, Direction);
	}

	public void MoveToIncludePoint(Vec3F point)
	{
		Vec3F vec3F = ProjectPoint(point);
		Origin += point - vec3F;
	}

	public void Transform(Matrix4F M)
	{
		M.Transform(Origin, out Origin);
		M.TransformVector(Direction, out Direction);
		Direction.Normalize();
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
		return string.Format("{0}{6} {1}{6} {2}{6} {3}{6} {4}{6} {5}", Origin.X.ToString(format, formatProvider), Origin.Y.ToString(format, formatProvider), Origin.Z.ToString(format, formatProvider), Direction.X.ToString(format, formatProvider), Direction.Y.ToString(format, formatProvider), Direction.Z.ToString(format, formatProvider), numberListSeparator);
	}
}
