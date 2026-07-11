using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Ray : IEquatable<Ray>, IFormattable, IDataSerializable
{
	public Vector3 Position;

	public Vector3 Direction;

	public Ray(Vector3 position, Vector3 direction)
	{
		Position = position;
		Direction = direction;
	}

	public bool Intersects(ref Vector3 point)
	{
		return Collision.RayIntersectsPoint(ref this, ref point);
	}

	public bool Intersects(ref Ray ray)
	{
		Vector3 point;
		return Collision.RayIntersectsRay(ref this, ref ray, out point);
	}

	public bool Intersects(ref Ray ray, out Vector3 point)
	{
		return Collision.RayIntersectsRay(ref this, ref ray, out point);
	}

	public bool Intersects(ref Plane plane)
	{
		float distance;
		return Collision.RayIntersectsPlane(ref this, ref plane, out distance);
	}

	public bool Intersects(ref Plane plane, out float distance)
	{
		return Collision.RayIntersectsPlane(ref this, ref plane, out distance);
	}

	public bool Intersects(ref Plane plane, out Vector3 point)
	{
		return Collision.RayIntersectsPlane(ref this, ref plane, out point);
	}

	public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3)
	{
		float distance;
		return Collision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out distance);
	}

	public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out float distance)
	{
		return Collision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out distance);
	}

	public bool Intersects(ref Vector3 vertex1, ref Vector3 vertex2, ref Vector3 vertex3, out Vector3 point)
	{
		return Collision.RayIntersectsTriangle(ref this, ref vertex1, ref vertex2, ref vertex3, out point);
	}

	public bool Intersects(ref BoundingBox box)
	{
		float distance;
		return Collision.RayIntersectsBox(ref this, ref box, out distance);
	}

	public bool Intersects(BoundingBox box)
	{
		return Intersects(ref box);
	}

	public bool Intersects(ref BoundingBox box, out float distance)
	{
		return Collision.RayIntersectsBox(ref this, ref box, out distance);
	}

	public bool Intersects(ref BoundingBox box, out Vector3 point)
	{
		return Collision.RayIntersectsBox(ref this, ref box, out point);
	}

	public bool Intersects(ref BoundingSphere sphere)
	{
		float distance;
		return Collision.RayIntersectsSphere(ref this, ref sphere, out distance);
	}

	public bool Intersects(BoundingSphere sphere)
	{
		return Intersects(ref sphere);
	}

	public bool Intersects(ref BoundingSphere sphere, out float distance)
	{
		return Collision.RayIntersectsSphere(ref this, ref sphere, out distance);
	}

	public bool Intersects(ref BoundingSphere sphere, out Vector3 point)
	{
		return Collision.RayIntersectsSphere(ref this, ref sphere, out point);
	}

	public static Ray GetPickRay(int x, int y, ViewportF viewport, Matrix worldViewProjection)
	{
		Vector3 vector = new Vector3(x, y, 0f);
		Vector3 vector2 = new Vector3(x, y, 1f);
		vector = Vector3.Unproject(vector, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth, worldViewProjection);
		vector2 = Vector3.Unproject(vector2, viewport.X, viewport.Y, viewport.Width, viewport.Height, viewport.MinDepth, viewport.MaxDepth, worldViewProjection);
		Vector3 direction = vector2 - vector;
		direction.Normalize();
		return new Ray(vector, direction);
	}

	public static bool operator ==(Ray left, Ray right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Ray left, Ray right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "Position:{0} Direction:{1}", new object[2]
		{
			Position.ToString(),
			Direction.ToString()
		});
	}

	public string ToString(string format)
	{
		return string.Format(CultureInfo.CurrentCulture, "Position:{0} Direction:{1}", new object[2]
		{
			Position.ToString(format, CultureInfo.CurrentCulture),
			Direction.ToString(format, CultureInfo.CurrentCulture)
		});
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "Position:{0} Direction:{1}", new object[2]
		{
			Position.ToString(),
			Direction.ToString()
		});
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "Position:{0} Direction:{1}", new object[2]
		{
			Position.ToString(format, formatProvider),
			Direction.ToString(format, formatProvider)
		});
	}

	public override int GetHashCode()
	{
		return (Position.GetHashCode() * 397) ^ Direction.GetHashCode();
	}

	public bool Equals(Ray value)
	{
		if (Position == value.Position)
		{
			return Direction == value.Direction;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(Ray)))
		{
			return false;
		}
		return Equals((Ray)value);
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref Position);
		serializer.Serialize(ref Direction);
	}
}
