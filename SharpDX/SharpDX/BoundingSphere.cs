using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct BoundingSphere : IEquatable<BoundingSphere>, IFormattable, IDataSerializable
{
	public Vector3 Center;

	public float Radius;

	public BoundingSphere(Vector3 center, float radius)
	{
		Center = center;
		Radius = radius;
	}

	public bool Intersects(ref Ray ray)
	{
		float distance;
		return Collision.RayIntersectsSphere(ref ray, ref this, out distance);
	}

	public bool Intersects(ref Ray ray, out float distance)
	{
		return Collision.RayIntersectsSphere(ref ray, ref this, out distance);
	}

	public bool Intersects(ref Ray ray, out Vector3 point)
	{
		return Collision.RayIntersectsSphere(ref ray, ref this, out point);
	}

	public PlaneIntersectionType Intersects(ref Plane plane)
	{
		return Collision.PlaneIntersectsSphere(ref plane, ref this);
	}

	public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
	{
		return Collision.SphereIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
	}

	public bool Intersects(ref BoundingBox box)
	{
		return Collision.BoxIntersectsSphere(ref box, ref this);
	}

	public bool Intersects(BoundingBox box)
	{
		return Intersects(ref box);
	}

	public bool Intersects(ref BoundingSphere sphere)
	{
		return Collision.SphereIntersectsSphere(ref this, ref sphere);
	}

	public bool Intersects(BoundingSphere sphere)
	{
		return Intersects(ref sphere);
	}

	public ContainmentType Contains(ref Vector3 point)
	{
		return Collision.SphereContainsPoint(ref this, ref point);
	}

	public ContainmentType Contains(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
	{
		return Collision.SphereContainsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
	}

	public ContainmentType Contains(ref BoundingBox box)
	{
		return Collision.SphereContainsBox(ref this, ref box);
	}

	public ContainmentType Contains(ref BoundingSphere sphere)
	{
		return Collision.SphereContainsSphere(ref this, ref sphere);
	}

	public static void FromPoints(Vector3[] points, int start, int count, out BoundingSphere result)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		if (start < 0 || start >= points.Length)
		{
			throw new ArgumentOutOfRangeException("start", start, $"Must be in the range [0, {points.Length - 1}]");
		}
		if (count < 0 || start + count > points.Length)
		{
			throw new ArgumentOutOfRangeException("count", count, $"Must be in the range <= {points.Length}");
		}
		int num = start + count;
		Vector3 right = Vector3.Zero;
		for (int i = start; i < num; i++)
		{
			Vector3.Add(ref points[i], ref right, out right);
		}
		right /= (float)count;
		float num2 = 0f;
		for (int j = start; j < num; j++)
		{
			Vector3.DistanceSquared(ref right, ref points[j], out var result2);
			if (result2 > num2)
			{
				num2 = result2;
			}
		}
		num2 = (float)Math.Sqrt(num2);
		result.Center = right;
		result.Radius = num2;
	}

	public static void FromPoints(Vector3[] points, out BoundingSphere result)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		FromPoints(points, 0, points.Length, out result);
	}

	public static BoundingSphere FromPoints(Vector3[] points)
	{
		FromPoints(points, out var result);
		return result;
	}

	public static void FromBox(ref BoundingBox box, out BoundingSphere result)
	{
		Vector3.Lerp(ref box.Minimum, ref box.Maximum, 0.5f, out result.Center);
		float num = box.Minimum.X - box.Maximum.X;
		float num2 = box.Minimum.Y - box.Maximum.Y;
		float num3 = box.Minimum.Z - box.Maximum.Z;
		float num4 = (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
		result.Radius = num4 * 0.5f;
	}

	public static BoundingSphere FromBox(BoundingBox box)
	{
		FromBox(ref box, out var result);
		return result;
	}

	public static void Merge(ref BoundingSphere value1, ref BoundingSphere value2, out BoundingSphere result)
	{
		Vector3 vector = value2.Center - value1.Center;
		float num = vector.Length();
		float radius = value1.Radius;
		float radius2 = value2.Radius;
		if (radius + radius2 >= num)
		{
			if (radius - radius2 >= num)
			{
				result = value1;
				return;
			}
			if (radius2 - radius >= num)
			{
				result = value2;
				return;
			}
		}
		Vector3 vector2 = vector * (1f / num);
		float num2 = Math.Min(0f - radius, num - radius2);
		float num3 = (Math.Max(radius, num + radius2) - num2) * 0.5f;
		result.Center = value1.Center + vector2 * (num3 + num2);
		result.Radius = num3;
	}

	public static BoundingSphere Merge(BoundingSphere value1, BoundingSphere value2)
	{
		Merge(ref value1, ref value2, out var result);
		return result;
	}

	public static bool operator ==(BoundingSphere left, BoundingSphere right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BoundingSphere left, BoundingSphere right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "Center:{0} Radius:{1}", new object[2]
		{
			Center.ToString(),
			Radius.ToString()
		});
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(CultureInfo.CurrentCulture, "Center:{0} Radius:{1}", new object[2]
		{
			Center.ToString(format, CultureInfo.CurrentCulture),
			Radius.ToString(format, CultureInfo.CurrentCulture)
		});
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "Center:{0} Radius:{1}", new object[2]
		{
			Center.ToString(),
			Radius.ToString()
		});
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(formatProvider, "Center:{0} Radius:{1}", new object[2]
		{
			Center.ToString(format, formatProvider),
			Radius.ToString(format, formatProvider)
		});
	}

	public override int GetHashCode()
	{
		return (Center.GetHashCode() * 397) ^ Radius.GetHashCode();
	}

	public bool Equals(BoundingSphere value)
	{
		if (Center == value.Center)
		{
			return Radius == value.Radius;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(BoundingSphere)))
		{
			return false;
		}
		return Equals((BoundingSphere)value);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref Center);
		serializer.Serialize(ref Radius);
	}
}
