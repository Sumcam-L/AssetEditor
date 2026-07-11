using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Plane : IEquatable<Plane>, IFormattable, IDataSerializable
{
	public Vector3 Normal;

	public float D;

	public float this[int index]
	{
		get
		{
			return index switch
			{
				0 => Normal.X, 
				1 => Normal.Y, 
				2 => Normal.Z, 
				3 => D, 
				_ => throw new ArgumentOutOfRangeException("index", "Indices for Plane run from 0 to 3, inclusive."), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				Normal.X = value;
				break;
			case 1:
				Normal.Y = value;
				break;
			case 2:
				Normal.Z = value;
				break;
			case 3:
				D = value;
				break;
			default:
				throw new ArgumentOutOfRangeException("index", "Indices for Plane run from 0 to 3, inclusive.");
			}
		}
	}

	public Plane(float value)
	{
		Normal.X = (Normal.Y = (Normal.Z = (D = value)));
	}

	public Plane(float a, float b, float c, float d)
	{
		Normal.X = a;
		Normal.Y = b;
		Normal.Z = c;
		D = d;
	}

	public Plane(Vector3 point, Vector3 normal)
	{
		Normal = normal;
		D = 0f - Vector3.Dot(normal, point);
	}

	public Plane(Vector3 value, float d)
	{
		Normal = value;
		D = d;
	}

	public Plane(Vector3 point1, Vector3 point2, Vector3 point3)
	{
		float num = point2.X - point1.X;
		float num2 = point2.Y - point1.Y;
		float num3 = point2.Z - point1.Z;
		float num4 = point3.X - point1.X;
		float num5 = point3.Y - point1.Y;
		float num6 = point3.Z - point1.Z;
		float num7 = num2 * num6 - num3 * num5;
		float num8 = num3 * num4 - num * num6;
		float num9 = num * num5 - num2 * num4;
		float num10 = 1f / (float)Math.Sqrt(num7 * num7 + num8 * num8 + num9 * num9);
		Normal.X = num7 * num10;
		Normal.Y = num8 * num10;
		Normal.Z = num9 * num10;
		D = 0f - (Normal.X * point1.X + Normal.Y * point1.Y + Normal.Z * point1.Z);
	}

	public Plane(float[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length != 4)
		{
			throw new ArgumentOutOfRangeException("values", "There must be four and only four input values for Plane.");
		}
		Normal.X = values[0];
		Normal.Y = values[1];
		Normal.Z = values[2];
		D = values[3];
	}

	public void Normalize()
	{
		float num = 1f / (float)Math.Sqrt(Normal.X * Normal.X + Normal.Y * Normal.Y + Normal.Z * Normal.Z);
		Normal.X *= num;
		Normal.Y *= num;
		Normal.Z *= num;
		D *= num;
	}

	public float[] ToArray()
	{
		return new float[4] { Normal.X, Normal.Y, Normal.Z, D };
	}

	public PlaneIntersectionType Intersects(ref Vector3 point)
	{
		return Collision.PlaneIntersectsPoint(ref this, ref point);
	}

	public bool Intersects(ref Ray ray)
	{
		float distance;
		return Collision.RayIntersectsPlane(ref ray, ref this, out distance);
	}

	public bool Intersects(ref Ray ray, out float distance)
	{
		return Collision.RayIntersectsPlane(ref ray, ref this, out distance);
	}

	public bool Intersects(ref Ray ray, out Vector3 point)
	{
		return Collision.RayIntersectsPlane(ref ray, ref this, out point);
	}

	public bool Intersects(ref Plane plane)
	{
		return Collision.PlaneIntersectsPlane(ref this, ref plane);
	}

	public bool Intersects(ref Plane plane, out Ray line)
	{
		return Collision.PlaneIntersectsPlane(ref this, ref plane, out line);
	}

	public PlaneIntersectionType Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
	{
		return Collision.PlaneIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3);
	}

	public PlaneIntersectionType Intersects(ref BoundingBox box)
	{
		return Collision.PlaneIntersectsBox(ref this, ref box);
	}

	public PlaneIntersectionType Intersects(ref BoundingSphere sphere)
	{
		return Collision.PlaneIntersectsSphere(ref this, ref sphere);
	}

	public static void Multiply(ref Plane value, float scale, out Plane result)
	{
		result.Normal.X = value.Normal.X * scale;
		result.Normal.Y = value.Normal.Y * scale;
		result.Normal.Z = value.Normal.Z * scale;
		result.D = value.D * scale;
	}

	public static Plane Multiply(Plane value, float scale)
	{
		return new Plane(value.Normal.X * scale, value.Normal.Y * scale, value.Normal.Z * scale, value.D * scale);
	}

	public static void Dot(ref Plane left, ref Vector4 right, out float result)
	{
		result = left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z + left.D * right.W;
	}

	public static float Dot(Plane left, Vector4 right)
	{
		return left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z + left.D * right.W;
	}

	public static void DotCoordinate(ref Plane left, ref Vector3 right, out float result)
	{
		result = left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z + left.D;
	}

	public static float DotCoordinate(Plane left, Vector3 right)
	{
		return left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z + left.D;
	}

	public static void DotNormal(ref Plane left, ref Vector3 right, out float result)
	{
		result = left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z;
	}

	public static float DotNormal(Plane left, Vector3 right)
	{
		return left.Normal.X * right.X + left.Normal.Y * right.Y + left.Normal.Z * right.Z;
	}

	public static void Normalize(ref Plane plane, out Plane result)
	{
		float num = 1f / (float)Math.Sqrt(plane.Normal.X * plane.Normal.X + plane.Normal.Y * plane.Normal.Y + plane.Normal.Z * plane.Normal.Z);
		result.Normal.X = plane.Normal.X * num;
		result.Normal.Y = plane.Normal.Y * num;
		result.Normal.Z = plane.Normal.Z * num;
		result.D = plane.D * num;
	}

	public static Plane Normalize(Plane plane)
	{
		float num = 1f / (float)Math.Sqrt(plane.Normal.X * plane.Normal.X + plane.Normal.Y * plane.Normal.Y + plane.Normal.Z * plane.Normal.Z);
		return new Plane(plane.Normal.X * num, plane.Normal.Y * num, plane.Normal.Z * num, plane.D * num);
	}

	public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
	{
		float num = rotation.X + rotation.X;
		float num2 = rotation.Y + rotation.Y;
		float num3 = rotation.Z + rotation.Z;
		float num4 = rotation.W * num;
		float num5 = rotation.W * num2;
		float num6 = rotation.W * num3;
		float num7 = rotation.X * num;
		float num8 = rotation.X * num2;
		float num9 = rotation.X * num3;
		float num10 = rotation.Y * num2;
		float num11 = rotation.Y * num3;
		float num12 = rotation.Z * num3;
		float x = plane.Normal.X;
		float y = plane.Normal.Y;
		float z = plane.Normal.Z;
		result.Normal.X = x * (1f - num10 - num12) + y * (num8 - num6) + z * (num9 + num5);
		result.Normal.Y = x * (num8 + num6) + y * (1f - num7 - num12) + z * (num11 - num4);
		result.Normal.Z = x * (num9 - num5) + y * (num11 + num4) + z * (1f - num7 - num10);
		result.D = plane.D;
	}

	public static Plane Transform(Plane plane, Quaternion rotation)
	{
		float num = rotation.X + rotation.X;
		float num2 = rotation.Y + rotation.Y;
		float num3 = rotation.Z + rotation.Z;
		float num4 = rotation.W * num;
		float num5 = rotation.W * num2;
		float num6 = rotation.W * num3;
		float num7 = rotation.X * num;
		float num8 = rotation.X * num2;
		float num9 = rotation.X * num3;
		float num10 = rotation.Y * num2;
		float num11 = rotation.Y * num3;
		float num12 = rotation.Z * num3;
		float x = plane.Normal.X;
		float y = plane.Normal.Y;
		float z = plane.Normal.Z;
		Plane result = default(Plane);
		result.Normal.X = x * (1f - num10 - num12) + y * (num8 - num6) + z * (num9 + num5);
		result.Normal.Y = x * (num8 + num6) + y * (1f - num7 - num12) + z * (num11 - num4);
		result.Normal.Z = x * (num9 - num5) + y * (num11 + num4) + z * (1f - num7 - num10);
		result.D = plane.D;
		return result;
	}

	public static void Transform(Plane[] planes, ref Quaternion rotation)
	{
		if (planes == null)
		{
			throw new ArgumentNullException("planes");
		}
		float num = rotation.X + rotation.X;
		float num2 = rotation.Y + rotation.Y;
		float num3 = rotation.Z + rotation.Z;
		float num4 = rotation.W * num;
		float num5 = rotation.W * num2;
		float num6 = rotation.W * num3;
		float num7 = rotation.X * num;
		float num8 = rotation.X * num2;
		float num9 = rotation.X * num3;
		float num10 = rotation.Y * num2;
		float num11 = rotation.Y * num3;
		float num12 = rotation.Z * num3;
		for (int i = 0; i < planes.Length; i++)
		{
			float x = planes[i].Normal.X;
			float y = planes[i].Normal.Y;
			float z = planes[i].Normal.Z;
			planes[i].Normal.X = x * (1f - num10 - num12) + y * (num8 - num6) + z * (num9 + num5);
			planes[i].Normal.Y = x * (num8 + num6) + y * (1f - num7 - num12) + z * (num11 - num4);
			planes[i].Normal.Z = x * (num9 - num5) + y * (num11 + num4) + z * (1f - num7 - num10);
		}
	}

	public static void Transform(ref Plane plane, ref Matrix transformation, out Plane result)
	{
		float x = plane.Normal.X;
		float y = plane.Normal.Y;
		float z = plane.Normal.Z;
		float d = plane.D;
		Matrix.Invert(ref transformation, out var result2);
		result.Normal.X = x * result2.M11 + y * result2.M12 + z * result2.M13 + d * result2.M14;
		result.Normal.Y = x * result2.M21 + y * result2.M22 + z * result2.M23 + d * result2.M24;
		result.Normal.Z = x * result2.M31 + y * result2.M32 + z * result2.M33 + d * result2.M34;
		result.D = x * result2.M41 + y * result2.M42 + z * result2.M43 + d * result2.M44;
	}

	public static Plane Transform(Plane plane, Matrix transformation)
	{
		float x = plane.Normal.X;
		float y = plane.Normal.Y;
		float z = plane.Normal.Z;
		float d = plane.D;
		transformation.Invert();
		Plane result = default(Plane);
		result.Normal.X = x * transformation.M11 + y * transformation.M12 + z * transformation.M13 + d * transformation.M14;
		result.Normal.Y = x * transformation.M21 + y * transformation.M22 + z * transformation.M23 + d * transformation.M24;
		result.Normal.Z = x * transformation.M31 + y * transformation.M32 + z * transformation.M33 + d * transformation.M34;
		result.D = x * transformation.M41 + y * transformation.M42 + z * transformation.M43 + d * transformation.M44;
		return result;
	}

	public static void Transform(Plane[] planes, ref Matrix transformation)
	{
		if (planes == null)
		{
			throw new ArgumentNullException("planes");
		}
		Matrix.Invert(ref transformation, out var _);
		for (int i = 0; i < planes.Length; i++)
		{
			Transform(ref planes[i], ref transformation, out planes[i]);
		}
	}

	public static Plane operator *(float scale, Plane plane)
	{
		return new Plane(plane.Normal.X * scale, plane.Normal.Y * scale, plane.Normal.Z * scale, plane.D * scale);
	}

	public static Plane operator *(Plane plane, float scale)
	{
		return new Plane(plane.Normal.X * scale, plane.Normal.Y * scale, plane.Normal.Z * scale, plane.D * scale);
	}

	public static bool operator ==(Plane left, Plane right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Plane left, Plane right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "A:{0} B:{1} C:{2} D:{3}", Normal.X, Normal.Y, Normal.Z, D);
	}

	public string ToString(string format)
	{
		return string.Format(CultureInfo.CurrentCulture, "A:{0} B:{1} C:{2} D:{3}", Normal.X.ToString(format, CultureInfo.CurrentCulture), Normal.Y.ToString(format, CultureInfo.CurrentCulture), Normal.Z.ToString(format, CultureInfo.CurrentCulture), D.ToString(format, CultureInfo.CurrentCulture));
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "A:{0} B:{1} C:{2} D:{3}", Normal.X, Normal.Y, Normal.Z, D);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "A:{0} B:{1} C:{2} D:{3}", Normal.X.ToString(format, formatProvider), Normal.Y.ToString(format, formatProvider), Normal.Z.ToString(format, formatProvider), D.ToString(format, formatProvider));
	}

	public override int GetHashCode()
	{
		return (Normal.GetHashCode() * 397) ^ D.GetHashCode();
	}

	public bool Equals(Plane value)
	{
		if (Normal == value.Normal)
		{
			return D == value.D;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(Plane)))
		{
			return false;
		}
		return Equals((Plane)value);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref Normal);
		serializer.Serialize(ref D);
	}
}
