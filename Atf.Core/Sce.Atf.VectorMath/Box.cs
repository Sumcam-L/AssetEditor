using System;
using System.Collections.Generic;

namespace Sce.Atf.VectorMath;

public class Box : IFormattable
{
	public Vec3F Min;

	public Vec3F Max;

	private bool m_initialized;

	public bool IsEmpty => Min == Max;

	public Vec3F Centroid => Vec3F.Mul(Min + Max, 0.5f);

	public Box()
	{
	}

	public Box(Vec3F min, Vec3F max)
	{
		Min = min;
		Max = max;
		m_initialized = true;
	}

	public Box Extend(Vec3F p)
	{
		if (!m_initialized)
		{
			Min = (Max = p);
			m_initialized = true;
		}
		else
		{
			Min.X = Math.Min(Min.X, p.X);
			Min.Y = Math.Min(Min.Y, p.Y);
			Min.Z = Math.Min(Min.Z, p.Z);
			Max.X = Math.Max(Max.X, p.X);
			Max.Y = Math.Max(Max.Y, p.Y);
			Max.Z = Math.Max(Max.Z, p.Z);
		}
		return this;
	}

	public Box Extend(IList<float> v)
	{
		if (v.Count >= 3)
		{
			if (!m_initialized)
			{
				Max.X = (Min.X = v[0]);
				Max.Y = (Min.Y = v[1]);
				Max.Z = (Min.Z = v[2]);
				m_initialized = true;
			}
			for (int i = 0; i < v.Count; i += 3)
			{
				Min.X = Math.Min(Min.X, v[i]);
				Min.Y = Math.Min(Min.Y, v[i + 1]);
				Min.Z = Math.Min(Min.Z, v[i + 2]);
				Max.X = Math.Max(Max.X, v[i]);
				Max.Y = Math.Max(Max.Y, v[i + 1]);
				Max.Z = Math.Max(Max.Z, v[i + 2]);
			}
		}
		return this;
	}

	public Box Extend(Sphere3F sphere)
	{
		float radius = sphere.Radius;
		Extend(sphere.Center + new Vec3F(radius, radius, radius));
		Extend(sphere.Center - new Vec3F(radius, radius, radius));
		return this;
	}

	public Box Extend(Box other)
	{
		if (!other.IsEmpty)
		{
			Extend(other.Min);
			Extend(other.Max);
		}
		return this;
	}

	public override string ToString()
	{
		return string.Concat(Min, Environment.NewLine, Max);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		return string.Format("{0}{6} {1}{6} {2}{6} {3}{6} {4}{6} {5}", Min.X.ToString(format, formatProvider), Min.Y.ToString(format, formatProvider), Min.Z.ToString(format, formatProvider), Max.X.ToString(format, formatProvider), Max.Y.ToString(format, formatProvider), Max.Z.ToString(format, formatProvider), numberListSeparator);
	}

	public void Transform(Matrix4F M)
	{
		float[] array = new float[3] { Min.X, Min.Y, Min.Z };
		float[] array2 = new float[3] { Max.X, Max.Y, Max.Z };
		float[] array3 = new float[3] { M.M41, M.M42, M.M43 };
		float[] array4 = new float[3]
		{
			array3[0],
			array3[1],
			array3[2]
		};
		float[] array5 = M.ToArray();
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				float num = array5[j * 4 + i] * array[j];
				float num2 = array5[j * 4 + i] * array2[j];
				if (num < num2)
				{
					array3[i] += num;
					array4[i] += num2;
				}
				else
				{
					array3[i] += num2;
					array4[i] += num;
				}
			}
		}
		Min.X = array3[0];
		Min.Y = array3[1];
		Min.Z = array3[2];
		Max.X = array4[0];
		Max.Y = array4[1];
		Max.Z = array4[2];
		m_initialized = true;
	}

	public float[] ToArray()
	{
		return new float[6] { Min.X, Min.Y, Min.Z, Max.X, Max.Y, Max.Z };
	}

	public bool Intersects(Ray3F ray)
	{
		Vec3F vec3F = ray.Direction * 100000f;
		bool flag = true;
		float num;
		if (ray.Origin.X < Min.X)
		{
			num = Min.X - ray.Origin.X;
			if (num > vec3F.X)
			{
				return false;
			}
			num /= vec3F.X;
			flag = false;
		}
		else if (ray.Origin.X > Max.X)
		{
			num = Max.X - ray.Origin.X;
			if (num < vec3F.X)
			{
				return false;
			}
			num /= vec3F.X;
			flag = false;
		}
		else
		{
			num = -1f;
		}
		float num2;
		if (ray.Origin.Y < Min.Y)
		{
			num2 = Min.Y - ray.Origin.Y;
			if (num2 > vec3F.Y)
			{
				return false;
			}
			num2 /= vec3F.Y;
			flag = false;
		}
		else if (ray.Origin.Y > Max.Y)
		{
			num2 = Max.Y - ray.Origin.Y;
			if (num2 < vec3F.Y)
			{
				return false;
			}
			num2 /= vec3F.Y;
			flag = false;
		}
		else
		{
			num2 = -1f;
		}
		float num3;
		if (ray.Origin.Z < Min.Z)
		{
			num3 = Min.Z - ray.Origin.Z;
			if (num3 > vec3F.Z)
			{
				return false;
			}
			num3 /= vec3F.Z;
			flag = false;
		}
		else if (ray.Origin.Z > Max.Z)
		{
			num3 = Max.Z - ray.Origin.Z;
			if (num3 < vec3F.Z)
			{
				return false;
			}
			num3 /= vec3F.Z;
			flag = false;
		}
		else
		{
			num3 = -1f;
		}
		if (flag)
		{
			return true;
		}
		float num4 = num;
		if (num2 > num4)
		{
			num4 = num2;
		}
		if (num3 > num4)
		{
			num4 = num3;
		}
		if (num4 == num)
		{
			float num5 = ray.Origin.Y + vec3F.Y * num4;
			float num6 = ray.Origin.Z + vec3F.Z * num4;
			if (num5 < Min.Y || num5 > Max.Y)
			{
				return false;
			}
			if (num6 < Min.Z || num6 > Max.Z)
			{
				return false;
			}
		}
		else if (num4 == num2)
		{
			float num7 = ray.Origin.X + vec3F.X * num4;
			float num8 = ray.Origin.Z + vec3F.Z * num4;
			if (num7 < Min.X || num7 > Max.X)
			{
				return false;
			}
			if (num8 < Min.Z || num8 > Max.Z)
			{
				return false;
			}
		}
		else
		{
			float num9 = ray.Origin.X + vec3F.X * num4;
			float num10 = ray.Origin.Y + vec3F.Y * num4;
			if (num9 < Min.X || num9 > Max.X)
			{
				return false;
			}
			if (num10 < Min.Y || num10 > Max.Y)
			{
				return false;
			}
		}
		return true;
	}
}
