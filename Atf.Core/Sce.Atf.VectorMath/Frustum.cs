using System;

namespace Sce.Atf.VectorMath;

public class Frustum : IFormattable
{
	public const int iRight = 0;

	public const int iLeft = 1;

	public const int iTop = 2;

	public const int iBottom = 3;

	public const int iNear = 4;

	public const int iFar = 5;

	private readonly Plane3F[] m_planes;

	public float FarZ
	{
		get
		{
			return 0f - m_planes[5].Distance;
		}
		set
		{
			if (value <= 0f)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_planes[5].Distance = 0f - value;
		}
	}

	public float NearZ
	{
		get
		{
			return m_planes[4].Distance;
		}
		set
		{
			m_planes[4].Distance = value;
		}
	}

	public float Right => 0f - m_planes[0].Distance;

	public float Left => m_planes[1].Distance;

	public float Top => 0f - m_planes[2].Distance;

	public float Bottom => m_planes[3].Distance;

	public float Near => m_planes[4].Distance;

	public float Far => 0f - m_planes[5].Distance;

	public bool IsOrtho
	{
		get
		{
			float num = Math.Abs(Vec3F.Dot(m_planes[0].Normal, m_planes[1].Normal));
			return (double)num >= 0.9999;
		}
	}

	public float FovX
	{
		get
		{
			float num = Vec3F.Dot(m_planes[1].Normal, new Vec3F(1f, 0f, 0f));
			float num2 = (float)Math.Acos(num);
			return num2 * 2f;
		}
	}

	public float FovY
	{
		get
		{
			float num = Vec3F.Dot(m_planes[3].Normal, new Vec3F(0f, 1f, 0f));
			float num2 = (float)Math.Acos(num);
			return num2 * 2f;
		}
	}

	public Frustum()
	{
		m_planes = new Plane3F[6];
	}

	public Frustum(Frustum other)
		: this()
	{
		Set(other);
	}

	public Frustum(float fovY, float aspectRatio, float near, float far)
		: this()
	{
		SetPerspective(fovY, aspectRatio, near, far);
	}

	public Frustum(float right, float left, float top, float bottom, float near, float far)
		: this()
	{
		SetOrtho(right, left, top, bottom, near, far);
	}

	public void Set(Frustum other)
	{
		for (int i = 0; i < 6; i++)
		{
			m_planes[i] = other.m_planes[i];
		}
	}

	public void SetPerspective(float fovY, float aspectRatio, float near, float far)
	{
		float num = (float)Math.Tan((double)fovY / 2.0);
		float num2 = num * aspectRatio;
		Vec3F vec3F = new Vec3F(-1f, 0f, 0f - num2);
		Vec3F vec3F2 = new Vec3F(0f, -1f, 0f - num);
		float length = vec3F.Length;
		float length2 = vec3F2.Length;
		m_planes[0].Set(new Vec3F(-1f, 0f, 0f - num2) / length, 0f);
		m_planes[1].Set(new Vec3F(1f, 0f, 0f - num2) / length, 0f);
		m_planes[2].Set(new Vec3F(0f, -1f, 0f - num) / length2, 0f);
		m_planes[3].Set(new Vec3F(0f, 1f, 0f - num) / length2, 0f);
		m_planes[4].Set(new Vec3F(0f, 0f, -1f), near);
		m_planes[5].Set(new Vec3F(0f, 0f, 1f), 0f - far);
	}

	public void SetOrtho(float right, float left, float top, float bottom, float near, float far)
	{
		m_planes[0].Set(new Vec3F(-1f, 0f, 0f), 0f - right);
		m_planes[1].Set(new Vec3F(1f, 0f, 0f), left);
		m_planes[2].Set(new Vec3F(0f, -1f, 0f), 0f - top);
		m_planes[3].Set(new Vec3F(0f, 1f, 0f), bottom);
		m_planes[4].Set(new Vec3F(0f, 0f, -1f), near);
		m_planes[5].Set(new Vec3F(0f, 0f, 1f), 0f - far);
	}

	public void Clip(float left, float right, float top, float bottom)
	{
		if (!IsOrtho)
		{
			ClipPerspective(left, right, top, bottom);
		}
		else
		{
			ClipOrtho(left, right, top, bottom);
		}
	}

	public Plane3F GetPlane(int i)
	{
		return m_planes[i];
	}

	public bool Contains(Sphere3F sphere)
	{
		for (int i = 0; i < 6; i++)
		{
			float num = m_planes[i].SignedDistance(sphere.Center);
			if (num < 0f - sphere.Radius)
			{
				return false;
			}
		}
		return true;
	}

	public bool Contains(Box box)
	{
		Vec3F point = (box.Min + box.Max) * 0.5f;
		Vec3F vec3F = (box.Max - box.Min) * 0.5f;
		for (int i = 0; i < 6; i++)
		{
			float num = m_planes[i].SignedDistance(point);
			float num2 = vec3F.X * Math.Abs(m_planes[i].Normal.X) + vec3F.Y * Math.Abs(m_planes[i].Normal.Y) + vec3F.Z * Math.Abs(m_planes[i].Normal.Z);
			if (num + num2 < 0f)
			{
				return false;
			}
		}
		return true;
	}

	public bool ContainsPolygon(Vec3F[] vertices, Vec3F eye, bool backfaceCull)
	{
		for (int i = 0; i < 6; i++)
		{
			int j;
			for (j = 0; j < vertices.Length; j++)
			{
				float num = m_planes[i].SignedDistance(vertices[j]);
				if (num > 0f)
				{
					break;
				}
			}
			if (j == vertices.Length)
			{
				return false;
			}
		}
		if (backfaceCull)
		{
			Vec3F u = default(Vec3F);
			for (int k = 2; k < vertices.Length; k++)
			{
				u = Vec3F.Cross(vertices[0] - vertices[1], vertices[k] - vertices[1]);
				if (u.LengthSquared != 0f)
				{
					break;
				}
			}
			if (Vec3F.Dot(u, vertices[0] - eye) <= 0f)
			{
				return false;
			}
		}
		return true;
	}

	public void Transform(Matrix4F m)
	{
		Matrix4F matrix4F = new Matrix4F(m);
		matrix4F.Invert(matrix4F);
		matrix4F.Transpose(matrix4F);
		for (int i = 0; i < 6; i++)
		{
			m.Transform(m_planes[i], matrix4F, out m_planes[i]);
		}
	}

	public override string ToString()
	{
		return $"Frustum:\nright: {Right}\nLeft: {Left}\nTop:  {Top}\nBottom: {Bottom}\nNear {Near}\nFar: {Far}";
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		return string.Format("{0}{6} {1}{6} {2}{6} {3}{6} {4}{6} {5}", Right.ToString(format, formatProvider), Left.ToString(format, formatProvider), Top.ToString(format, formatProvider), Bottom.ToString(format, formatProvider), Near.ToString(format, formatProvider), Far.ToString(format, formatProvider), numberListSeparator);
	}

	private void ClipPerspective(float left, float right, float top, float bottom)
	{
		float near = Near;
		float num = near * (float)Math.Tan(FovY / 2f) * 2f;
		float num2 = near * (float)Math.Tan(FovX / 2f) * 2f;
		CalcPlaneNormal(1, new Vec3F(0f, 1f, 0f), new Vec3F(right * num2, 0f, 0f - near));
		CalcPlaneNormal(0, new Vec3F(0f, -1f, 0f), new Vec3F(left * num2, 0f, 0f - near));
		CalcPlaneNormal(3, new Vec3F(1f, 0f, 0f), new Vec3F(0f, bottom * num, 0f - near));
		CalcPlaneNormal(2, new Vec3F(-1f, 0f, 0f), new Vec3F(0f, top * num, 0f - near));
	}

	private void ClipOrtho(float nLeft, float nRight, float nTop, float nBottom)
	{
		float num = Top * 2f;
		float num2 = Right * 2f;
		float distance = nLeft * num2;
		float num3 = nRight * num2;
		float num4 = nTop * num;
		float distance2 = nBottom * num;
		m_planes[0].Distance = 0f - num3;
		m_planes[1].Distance = distance;
		m_planes[2].Distance = 0f - num4;
		m_planes[3].Distance = distance2;
	}

	private void CalcPlaneNormal(int planeIndex, Vec3F u1, Vec3F u2)
	{
		m_planes[planeIndex].Normal = Vec3F.Cross(u1, u2);
		m_planes[planeIndex].Normal.Normalize();
		m_planes[planeIndex].Distance = 0f;
	}
}
