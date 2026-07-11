using System;

namespace Sce.Atf.VectorMath;

public struct Sphere3F : IFormattable
{
	public Vec3F Center;

	public float Radius;

	private bool m_initialized;

	public static Sphere3F Empty
	{
		get
		{
			Vec3F center = new Vec3F(0f, 0f, 0f);
			Sphere3F result = new Sphere3F(center, 0f);
			result.m_initialized = false;
			return result;
		}
	}

	public bool IsValid => m_initialized;

	public Sphere3F(Vec3F center, float radius)
	{
		Center = center;
		Radius = radius;
		m_initialized = true;
	}

	public Sphere3F(Sphere3F sphere)
	{
		Center = sphere.Center;
		Radius = sphere.Radius;
		m_initialized = true;
	}

	public void Set(Vec3F center, float radius)
	{
		Center = center;
		Radius = radius;
		m_initialized = true;
	}

	public Sphere3F Extend(Sphere3F sphere)
	{
		if (!m_initialized)
		{
			Center = sphere.Center;
			Radius = sphere.Radius;
			m_initialized = true;
		}
		else if (!Contains(sphere))
		{
			if (Center == sphere.Center)
			{
				Radius = sphere.Radius;
			}
			else
			{
				Vec3F vec3F = Vec3F.Normalize(sphere.Center - Center);
				Vec3F vec3F2 = sphere.Center + vec3F * sphere.Radius;
				Vec3F vec3F3 = Center - vec3F * Radius;
				Radius = (vec3F3 - vec3F2).Length / 2f;
				Center = (vec3F2 + vec3F3) / 2f;
			}
		}
		return this;
	}

	public Sphere3F Extend(Vec3F pt)
	{
		if (!m_initialized)
		{
			Center = pt;
			Radius = 0f;
			m_initialized = true;
		}
		else if (!Contains(pt))
		{
			Vec3F vec3F = Vec3F.Normalize(pt - Center);
			Vec3F vec3F2 = Center - vec3F * Radius;
			Radius = (pt - vec3F2).Length / 2f;
			Center = pt + vec3F2 / 2f;
		}
		return this;
	}

	public Sphere3F Extend(Box box)
	{
		Sphere3F sphere = new Sphere3F((box.Max + box.Min) / 2f, (box.Max - box.Min).Length / 2f);
		Extend(sphere);
		return this;
	}

	public bool Contains(Sphere3F sphere)
	{
		float num = (sphere.Center - Center).Length + sphere.Radius;
		return num < Radius;
	}

	public bool Contains(Vec3F pt)
	{
		return (pt - Center).Length < Radius;
	}

	public Sphere3F Transform(Matrix4F m)
	{
		m.Transform(Center, out Center);
		float length = m.XAxis.Length;
		float length2 = m.YAxis.Length;
		float length3 = m.ZAxis.Length;
		float num = length;
		if (num < length2)
		{
			num = length2;
		}
		if (num < length3)
		{
			num = length3;
		}
		Radius *= num;
		return this;
	}

	public bool Intersects(Ray3F ray, out Vec3F x)
	{
		Vec3F u = Center - ray.Origin;
		float num = Vec3F.Dot(u, ray.Direction);
		float lengthSquared = u.LengthSquared;
		float num2 = Radius * Radius;
		if (num < 0f && lengthSquared > num2)
		{
			x = default(Vec3F);
			return false;
		}
		float num3 = lengthSquared - num * num;
		if (num3 > num2)
		{
			x = default(Vec3F);
			return false;
		}
		float num4 = (float)Math.Sqrt(num2 - num3);
		float num5 = ((!(lengthSquared > num2)) ? (num + num4) : (num - num4));
		x = ray.Origin + num5 * ray.Direction;
		return true;
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
		return string.Format("{0}{4} {1}{4} {2}{4} {3}", Center.X.ToString(format, formatProvider), Center.Y.ToString(format, formatProvider), Center.Z.ToString(format, formatProvider), Radius.ToString(format, formatProvider), numberListSeparator);
	}
}
