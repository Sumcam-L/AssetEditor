using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct BoundingBox : IEquatable<BoundingBox>, IFormattable, IDataSerializable
{
	public Vector3 Minimum;

	public Vector3 Maximum;

	public BoundingBox(Vector3 minimum, Vector3 maximum)
	{
		Minimum = minimum;
		Maximum = maximum;
	}

	public Vector3[] GetCorners()
	{
		Vector3[] array = new Vector3[8];
		GetCorners(array);
		return array;
	}

	public void GetCorners(Vector3[] corners)
	{
		ref Vector3 reference = ref corners[0];
		reference = new Vector3(Minimum.X, Maximum.Y, Maximum.Z);
		ref Vector3 reference2 = ref corners[1];
		reference2 = new Vector3(Maximum.X, Maximum.Y, Maximum.Z);
		ref Vector3 reference3 = ref corners[2];
		reference3 = new Vector3(Maximum.X, Minimum.Y, Maximum.Z);
		ref Vector3 reference4 = ref corners[3];
		reference4 = new Vector3(Minimum.X, Minimum.Y, Maximum.Z);
		ref Vector3 reference5 = ref corners[4];
		reference5 = new Vector3(Minimum.X, Maximum.Y, Minimum.Z);
		ref Vector3 reference6 = ref corners[5];
		reference6 = new Vector3(Maximum.X, Maximum.Y, Minimum.Z);
		ref Vector3 reference7 = ref corners[6];
		reference7 = new Vector3(Maximum.X, Minimum.Y, Minimum.Z);
		ref Vector3 reference8 = ref corners[7];
		reference8 = new Vector3(Minimum.X, Minimum.Y, Minimum.Z);
	}

	public bool Intersects(ref Ray ray)
	{
		float distance;
		return Collision.RayIntersectsBox(ref ray, ref this, out distance);
	}

	public bool Intersects(ref Ray ray, out float distance)
	{
		return Collision.RayIntersectsBox(ref ray, ref this, out distance);
	}

	public bool Intersects(ref Ray ray, out Vector3 point)
	{
		return Collision.RayIntersectsBox(ref ray, ref this, out point);
	}

	public PlaneIntersectionType Intersects(ref Plane plane)
	{
		return Collision.PlaneIntersectsBox(ref plane, ref this);
	}

	public bool Intersects(ref BoundingBox box)
	{
		return Collision.BoxIntersectsBox(ref this, ref box);
	}

	public bool Intersects(BoundingBox box)
	{
		return Intersects(ref box);
	}

	public bool Intersects(ref BoundingSphere sphere)
	{
		return Collision.BoxIntersectsSphere(ref this, ref sphere);
	}

	public bool Intersects(BoundingSphere sphere)
	{
		return Intersects(ref sphere);
	}

	public ContainmentType Contains(ref Vector3 point)
	{
		return Collision.BoxContainsPoint(ref this, ref point);
	}

	public ContainmentType Contains(Vector3 point)
	{
		return Contains(ref point);
	}

	public ContainmentType Contains(ref BoundingBox box)
	{
		return Collision.BoxContainsBox(ref this, ref box);
	}

	public ContainmentType Contains(BoundingBox box)
	{
		return Contains(ref box);
	}

	public ContainmentType Contains(ref BoundingSphere sphere)
	{
		return Collision.BoxContainsSphere(ref this, ref sphere);
	}

	public ContainmentType Contains(BoundingSphere sphere)
	{
		return Contains(ref sphere);
	}

	public static void FromPoints(Vector3[] points, out BoundingBox result)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Vector3 left = new Vector3(float.MaxValue);
		Vector3 left2 = new Vector3(float.MinValue);
		for (int i = 0; i < points.Length; i++)
		{
			Vector3.Min(ref left, ref points[i], out left);
			Vector3.Max(ref left2, ref points[i], out left2);
		}
		result = new BoundingBox(left, left2);
	}

	public static BoundingBox FromPoints(Vector3[] points)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Vector3 left = new Vector3(float.MaxValue);
		Vector3 left2 = new Vector3(float.MinValue);
		for (int i = 0; i < points.Length; i++)
		{
			Vector3.Min(ref left, ref points[i], out left);
			Vector3.Max(ref left2, ref points[i], out left2);
		}
		return new BoundingBox(left, left2);
	}

	public static void FromSphere(ref BoundingSphere sphere, out BoundingBox result)
	{
		result.Minimum = new Vector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
		result.Maximum = new Vector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
	}

	public static BoundingBox FromSphere(BoundingSphere sphere)
	{
		BoundingBox result = default(BoundingBox);
		result.Minimum = new Vector3(sphere.Center.X - sphere.Radius, sphere.Center.Y - sphere.Radius, sphere.Center.Z - sphere.Radius);
		result.Maximum = new Vector3(sphere.Center.X + sphere.Radius, sphere.Center.Y + sphere.Radius, sphere.Center.Z + sphere.Radius);
		return result;
	}

	public static void Merge(ref BoundingBox value1, ref BoundingBox value2, out BoundingBox result)
	{
		Vector3.Min(ref value1.Minimum, ref value2.Minimum, out result.Minimum);
		Vector3.Max(ref value1.Maximum, ref value2.Maximum, out result.Maximum);
	}

	public static BoundingBox Merge(BoundingBox value1, BoundingBox value2)
	{
		BoundingBox result = default(BoundingBox);
		Vector3.Min(ref value1.Minimum, ref value2.Minimum, out result.Minimum);
		Vector3.Max(ref value1.Maximum, ref value2.Maximum, out result.Maximum);
		return result;
	}

	public static bool operator ==(BoundingBox left, BoundingBox right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BoundingBox left, BoundingBox right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", new object[2]
		{
			Minimum.ToString(),
			Maximum.ToString()
		});
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", new object[2]
		{
			Minimum.ToString(format, CultureInfo.CurrentCulture),
			Maximum.ToString(format, CultureInfo.CurrentCulture)
		});
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "Minimum:{0} Maximum:{1}", new object[2]
		{
			Minimum.ToString(),
			Maximum.ToString()
		});
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(formatProvider, "Minimum:{0} Maximum:{1}", new object[2]
		{
			Minimum.ToString(format, formatProvider),
			Maximum.ToString(format, formatProvider)
		});
	}

	public override int GetHashCode()
	{
		return (Minimum.GetHashCode() * 397) ^ Maximum.GetHashCode();
	}

	public bool Equals(BoundingBox value)
	{
		if (Minimum == value.Minimum)
		{
			return Maximum == value.Maximum;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(BoundingBox)))
		{
			return false;
		}
		return Equals((BoundingBox)value);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref Minimum);
		serializer.Serialize(ref Maximum);
	}
}
