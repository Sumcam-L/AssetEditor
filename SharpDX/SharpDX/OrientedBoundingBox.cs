using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct OrientedBoundingBox : IEquatable<OrientedBoundingBox>, IFormattable, IDataSerializable
{
	public Vector3 Extents;

	public Matrix Transformation;

	public Vector3 Size => Extents * 2f;

	public Vector3 Center => Transformation.TranslationVector;

	public OrientedBoundingBox(BoundingBox bb)
	{
		Vector3 vector = bb.Minimum + (bb.Maximum - bb.Minimum) / 2f;
		Extents = bb.Maximum - vector;
		Transformation = Matrix.Translation(vector);
	}

	public OrientedBoundingBox(Vector3 minimum, Vector3 maximum)
	{
		Vector3 vector = minimum + (maximum - minimum) / 2f;
		Extents = maximum - vector;
		Transformation = Matrix.Translation(vector);
	}

	public OrientedBoundingBox(Vector3[] points)
	{
		if (points == null || points.Length == 0)
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
		Vector3 vector = left + (left2 - left) / 2f;
		Extents = left2 - vector;
		Transformation = Matrix.Translation(vector);
	}

	public Vector3[] GetCorners()
	{
		Vector3 normal = new Vector3(Extents.X, 0f, 0f);
		Vector3 normal2 = new Vector3(0f, Extents.Y, 0f);
		Vector3 normal3 = new Vector3(0f, 0f, Extents.Z);
		Vector3.TransformNormal(ref normal, ref Transformation, out normal);
		Vector3.TransformNormal(ref normal2, ref Transformation, out normal2);
		Vector3.TransformNormal(ref normal3, ref Transformation, out normal3);
		Vector3 translationVector = Transformation.TranslationVector;
		return new Vector3[8]
		{
			translationVector + normal + normal2 + normal3,
			translationVector + normal + normal2 - normal3,
			translationVector - normal + normal2 - normal3,
			translationVector - normal + normal2 + normal3,
			translationVector + normal - normal2 + normal3,
			translationVector + normal - normal2 - normal3,
			translationVector - normal - normal2 - normal3,
			translationVector - normal - normal2 + normal3
		};
	}

	public void Transform(ref Matrix mat)
	{
		Transformation *= mat;
	}

	public void Transform(Matrix mat)
	{
		Transformation *= mat;
	}

	public void Scale(ref Vector3 scaling)
	{
		Extents *= scaling;
	}

	public void Scale(Vector3 scaling)
	{
		Extents *= scaling;
	}

	public void Scale(float scaling)
	{
		Extents *= scaling;
	}

	public void Translate(ref Vector3 translation)
	{
		Transformation.TranslationVector += translation;
	}

	public void Translate(Vector3 translation)
	{
		Transformation.TranslationVector += translation;
	}

	public Vector3 GetSize()
	{
		Vector3 normal = new Vector3(Extents.X * 2f, 0f, 0f);
		Vector3 normal2 = new Vector3(0f, Extents.Y * 2f, 0f);
		Vector3 normal3 = new Vector3(0f, 0f, Extents.Z * 2f);
		Vector3.TransformNormal(ref normal, ref Transformation, out normal);
		Vector3.TransformNormal(ref normal2, ref Transformation, out normal2);
		Vector3.TransformNormal(ref normal3, ref Transformation, out normal3);
		return new Vector3(normal.Length(), normal2.Length(), normal3.Length());
	}

	public Vector3 GetSizeSquared()
	{
		Vector3 normal = new Vector3(Extents.X * 2f, 0f, 0f);
		Vector3 normal2 = new Vector3(0f, Extents.Y * 2f, 0f);
		Vector3 normal3 = new Vector3(0f, 0f, Extents.Z * 2f);
		Vector3.TransformNormal(ref normal, ref Transformation, out normal);
		Vector3.TransformNormal(ref normal2, ref Transformation, out normal2);
		Vector3.TransformNormal(ref normal3, ref Transformation, out normal3);
		return new Vector3(normal.LengthSquared(), normal2.LengthSquared(), normal3.LengthSquared());
	}

	public ContainmentType Contains(ref Vector3 point)
	{
		Matrix.Invert(ref Transformation, out var result);
		Vector3.TransformCoordinate(ref point, ref result, out var result2);
		result2.X = Math.Abs(result2.X);
		result2.Y = Math.Abs(result2.Y);
		result2.Z = Math.Abs(result2.Z);
		if (MathUtil.NearEqual(result2.X, Extents.X) && MathUtil.NearEqual(result2.Y, Extents.Y) && MathUtil.NearEqual(result2.Z, Extents.Z))
		{
			return ContainmentType.Intersects;
		}
		if (result2.X < Extents.X && result2.Y < Extents.Y && result2.Z < Extents.Z)
		{
			return ContainmentType.Contains;
		}
		return ContainmentType.Disjoint;
	}

	public ContainmentType Contains(Vector3 point)
	{
		return Contains(ref point);
	}

	public ContainmentType Contains(Vector3[] points)
	{
		Matrix.Invert(ref Transformation, out var result);
		bool flag = true;
		bool flag2 = false;
		for (int i = 0; i < points.Length; i++)
		{
			Vector3.TransformCoordinate(ref points[i], ref result, out var result2);
			result2.X = Math.Abs(result2.X);
			result2.Y = Math.Abs(result2.Y);
			result2.Z = Math.Abs(result2.Z);
			if (MathUtil.NearEqual(result2.X, Extents.X) && MathUtil.NearEqual(result2.Y, Extents.Y) && MathUtil.NearEqual(result2.Z, Extents.Z))
			{
				flag2 = true;
			}
			if (result2.X < Extents.X && result2.Y < Extents.Y && result2.Z < Extents.Z)
			{
				flag2 = true;
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			return ContainmentType.Contains;
		}
		if (flag2)
		{
			return ContainmentType.Intersects;
		}
		return ContainmentType.Disjoint;
	}

	public ContainmentType Contains(BoundingSphere sphere, bool IgnoreScale = false)
	{
		Matrix.Invert(ref Transformation, out var result);
		Vector3.TransformCoordinate(ref sphere.Center, ref result, out var result2);
		float num;
		if (IgnoreScale)
		{
			num = sphere.Radius;
		}
		else
		{
			Vector3 normal = Vector3.UnitX * sphere.Radius;
			Vector3.TransformNormal(ref normal, ref result, out normal);
			num = normal.Length();
		}
		Vector3 min = -Extents;
		Vector3.Clamp(ref result2, ref min, ref Extents, out var result3);
		float num2 = Vector3.DistanceSquared(result2, result3);
		if (num2 > num * num)
		{
			return ContainmentType.Disjoint;
		}
		if (min.X + num <= result2.X && result2.X <= Extents.X - num && Extents.X - min.X > num && min.Y + num <= result2.Y && result2.Y <= Extents.Y - num && Extents.Y - min.Y > num && min.Z + num <= result2.Z && result2.Z <= Extents.Z - num && Extents.X - min.X > num)
		{
			return ContainmentType.Contains;
		}
		return ContainmentType.Intersects;
	}

	private static Vector3[] GetRows(ref Matrix mat)
	{
		return new Vector3[3]
		{
			new Vector3(mat.M11, mat.M12, mat.M13),
			new Vector3(mat.M21, mat.M22, mat.M23),
			new Vector3(mat.M31, mat.M32, mat.M33)
		};
	}

	public ContainmentType Contains(ref OrientedBoundingBox obb)
	{
		ContainmentType containmentType = Contains(obb.GetCorners());
		if (containmentType != ContainmentType.Disjoint)
		{
			return containmentType;
		}
		Vector3 extents = Extents;
		Vector3 extents2 = obb.Extents;
		Vector3[] rows = GetRows(ref Transformation);
		Vector3[] rows2 = GetRows(ref obb.Transformation);
		Matrix matrix = default(Matrix);
		Matrix matrix2 = default(Matrix);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				matrix[i, j] = Vector3.Dot(rows[i], rows2[j]);
				matrix2[i, j] = Math.Abs(matrix[i, j]);
			}
		}
		Vector3 left = obb.Center - Center;
		Vector3 left2 = new Vector3(Vector3.Dot(left, rows[0]), Vector3.Dot(left, rows[1]), Vector3.Dot(left, rows[2]));
		for (int i = 0; i < 3; i++)
		{
			float num = extents[i];
			float num2 = Vector3.Dot(extents2, new Vector3(matrix2[i, 0], matrix2[i, 1], matrix2[i, 2]));
			float num3 = Math.Abs(left2[i]);
			if (num3 > num + num2)
			{
				return ContainmentType.Disjoint;
			}
		}
		for (int j = 0; j < 3; j++)
		{
			float num = Vector3.Dot(extents, new Vector3(matrix2[0, j], matrix2[1, j], matrix2[2, j]));
			float num2 = extents2[j];
			float num3 = Math.Abs(Vector3.Dot(left2, new Vector3(matrix[0, j], matrix[1, j], matrix[2, j])));
			if (num3 > num + num2)
			{
				return ContainmentType.Disjoint;
			}
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int num4 = (i + 1) % 3;
				int num5 = (i + 2) % 3;
				int num6 = (j + 1) % 3;
				int num7 = (j + 2) % 3;
				float num = extents[num4] * matrix2[num5, j] + extents[num5] * matrix2[num4, j];
				float num2 = extents2[num6] * matrix2[i, num7] + extents2[num7] * matrix2[i, num6];
				float num3 = Math.Abs(left2[num5] * matrix[num4, j] - left2[num4] * matrix[num5, j]);
				if (num3 > num + num2)
				{
					return ContainmentType.Disjoint;
				}
			}
		}
		return ContainmentType.Intersects;
	}

	public ContainmentType ContainsLine(ref Vector3 L1, ref Vector3 L2)
	{
		ContainmentType containmentType = Contains(new Vector3[2] { L1, L2 });
		if (containmentType != ContainmentType.Disjoint)
		{
			return containmentType;
		}
		Matrix.Invert(ref Transformation, out var result);
		Vector3.TransformCoordinate(ref L1, ref result, out var result2);
		Vector3.TransformCoordinate(ref L1, ref result, out var result3);
		Vector3 vector = (result2 + result3) * 0.5f;
		Vector3 vector2 = result2 - vector;
		Vector3 vector3 = new Vector3(Math.Abs(vector2.X), Math.Abs(vector2.Y), Math.Abs(vector2.Z));
		if (Math.Abs(vector.X) > Extents.X + vector3.X)
		{
			return ContainmentType.Disjoint;
		}
		if (Math.Abs(vector.Y) > Extents.Y + vector3.Y)
		{
			return ContainmentType.Disjoint;
		}
		if (Math.Abs(vector.Z) > Extents.Z + vector3.Z)
		{
			return ContainmentType.Disjoint;
		}
		if (Math.Abs(vector.Y * vector2.Z - vector.Z * vector2.Y) > Extents.Y * vector3.Z + Extents.Z * vector3.Y)
		{
			return ContainmentType.Disjoint;
		}
		if (Math.Abs(vector.X * vector2.Z - vector.Z * vector2.X) > Extents.X * vector3.Z + Extents.Z * vector3.X)
		{
			return ContainmentType.Disjoint;
		}
		if (Math.Abs(vector.X * vector2.Y - vector.Y * vector2.X) > Extents.X * vector3.Y + Extents.Y * vector3.X)
		{
			return ContainmentType.Disjoint;
		}
		return ContainmentType.Intersects;
	}

	public ContainmentType Contains(ref BoundingBox box)
	{
		ContainmentType containmentType = Contains(box.GetCorners());
		if (containmentType != ContainmentType.Disjoint)
		{
			return containmentType;
		}
		Vector3 vector = box.Minimum + (box.Maximum - box.Minimum) / 2f;
		Vector3 vector2 = box.Maximum - vector;
		Vector3 extents = Extents;
		Vector3 left = vector2;
		Vector3[] rows = GetRows(ref Transformation);
		Matrix.Invert(ref Transformation, out var result);
		Matrix matrix = default(Matrix);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				matrix[i, j] = Math.Abs(result[i, j]);
			}
		}
		Vector3 left2 = vector - Center;
		Vector3 left3 = new Vector3(Vector3.Dot(left2, rows[0]), Vector3.Dot(left2, rows[1]), Vector3.Dot(left2, rows[2]));
		for (int i = 0; i < 3; i++)
		{
			float num = extents[i];
			float num2 = Vector3.Dot(left, new Vector3(matrix[i, 0], matrix[i, 1], matrix[i, 2]));
			float num3 = Math.Abs(left3[i]);
			if (num3 > num + num2)
			{
				return ContainmentType.Disjoint;
			}
		}
		for (int j = 0; j < 3; j++)
		{
			float num = Vector3.Dot(extents, new Vector3(matrix[0, j], matrix[1, j], matrix[2, j]));
			float num2 = left[j];
			float num3 = Math.Abs(Vector3.Dot(left3, new Vector3(result[0, j], result[1, j], result[2, j])));
			if (num3 > num + num2)
			{
				return ContainmentType.Disjoint;
			}
		}
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				int num4 = (i + 1) % 3;
				int num5 = (i + 2) % 3;
				int num6 = (j + 1) % 3;
				int num7 = (j + 2) % 3;
				float num = extents[num4] * matrix[num5, j] + extents[num5] * matrix[num4, j];
				float num2 = left[num6] * matrix[i, num7] + left[num7] * matrix[i, num6];
				float num3 = Math.Abs(left3[num5] * result[num4, j] - left3[num4] * result[num5, j]);
				if (num3 > num + num2)
				{
					return ContainmentType.Disjoint;
				}
			}
		}
		return ContainmentType.Intersects;
	}

	public bool Intersects(ref Ray ray, out Vector3 point)
	{
		Matrix.Invert(ref Transformation, out var result);
		Ray ray2 = default(Ray);
		Vector3.TransformNormal(ref ray.Direction, ref result, out ray2.Direction);
		Vector3.TransformCoordinate(ref ray.Position, ref result, out ray2.Position);
		BoundingBox box = new BoundingBox(-Extents, Extents);
		bool flag = Collision.RayIntersectsBox(ref ray2, ref box, out point);
		if (flag)
		{
			Vector3.TransformCoordinate(ref point, ref Transformation, out point);
		}
		return flag;
	}

	public bool Intersects(ref Ray ray)
	{
		Vector3 point;
		return Intersects(ref ray, out point);
	}

	private Vector3[] GetLocalCorners()
	{
		Vector3 vector = new Vector3(Extents.X, 0f, 0f);
		Vector3 vector2 = new Vector3(0f, Extents.Y, 0f);
		Vector3 vector3 = new Vector3(0f, 0f, Extents.Z);
		return new Vector3[8]
		{
			+vector + vector2 + vector3,
			+vector + vector2 - vector3,
			-vector + vector2 - vector3,
			-vector + vector2 + vector3,
			+vector - vector2 + vector3,
			+vector - vector2 - vector3,
			-vector - vector2 - vector3,
			-vector - vector2 + vector3
		};
	}

	public BoundingBox GetBoundingBox()
	{
		return BoundingBox.FromPoints(GetCorners());
	}

	public static Matrix GetBoxToBoxMatrix(ref OrientedBoundingBox A, ref OrientedBoundingBox B, bool NoMatrixScaleApplied = false)
	{
		if (NoMatrixScaleApplied)
		{
			Vector3[] rows = GetRows(ref A.Transformation);
			Vector3[] rows2 = GetRows(ref B.Transformation);
			Matrix result = default(Matrix);
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					result[i, j] = Vector3.Dot(rows2[i], rows[j]);
				}
			}
			Vector3 left = B.Center - A.Center;
			result.M41 = Vector3.Dot(left, rows[0]);
			result.M42 = Vector3.Dot(left, rows[1]);
			result.M43 = Vector3.Dot(left, rows[2]);
			result.M44 = 1f;
			return result;
		}
		Matrix.Invert(ref A.Transformation, out var result2);
		return B.Transformation * result2;
	}

	public static void Merge(ref OrientedBoundingBox A, ref OrientedBoundingBox B, bool NoMatrixScaleApplied = false)
	{
		Matrix transform = GetBoxToBoxMatrix(ref A, ref B, NoMatrixScaleApplied);
		Vector3[] localCorners = B.GetLocalCorners();
		Vector3.TransformCoordinate(localCorners, ref transform, localCorners);
		BoundingBox value = new BoundingBox(-A.Extents, A.Extents);
		BoundingBox value2 = BoundingBox.FromPoints(localCorners);
		BoundingBox.Merge(ref value2, ref value, out var result);
		Vector3 coordinate = result.Minimum + (result.Maximum - result.Minimum) / 2f;
		A.Extents = result.Maximum - coordinate;
		Vector3.TransformCoordinate(ref coordinate, ref A.Transformation, out coordinate);
		A.Transformation.TranslationVector = coordinate;
	}

	public void MergeInto(ref OrientedBoundingBox OBB, bool NoMatrixScaleApplied = false)
	{
		Merge(ref OBB, ref this, NoMatrixScaleApplied);
	}

	public void Add(ref OrientedBoundingBox OBB, bool NoMatrixScaleApplied = false)
	{
		Merge(ref this, ref OBB, NoMatrixScaleApplied);
	}

	public bool Equals(OrientedBoundingBox value)
	{
		if (Extents == value.Extents)
		{
			return Transformation == value.Transformation;
		}
		return false;
	}

	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (!object.ReferenceEquals(value.GetType(), typeof(OrientedBoundingBox)))
		{
			return false;
		}
		return Equals((OrientedBoundingBox)value);
	}

	public static bool operator ==(OrientedBoundingBox left, OrientedBoundingBox right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(OrientedBoundingBox left, OrientedBoundingBox right)
	{
		return !left.Equals(right);
	}

	public override int GetHashCode()
	{
		return Extents.GetHashCode() + Transformation.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "Center: {0}, Extents: {1}", new object[2] { Center, Extents });
	}

	public string ToString(string format)
	{
		if (format == null)
		{
			return ToString();
		}
		return string.Format(CultureInfo.CurrentCulture, "Center: {0}, Extents: {1}", new object[2]
		{
			Center.ToString(format, CultureInfo.CurrentCulture),
			Extents.ToString(format, CultureInfo.CurrentCulture)
		});
	}

	public string ToString(IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, "Center: {0}, Extents: {1}", new object[2]
		{
			Center.ToString(),
			Extents.ToString()
		});
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null)
		{
			return ToString(formatProvider);
		}
		return string.Format(formatProvider, "Center: {0}, Extents: {1}", new object[2]
		{
			Center.ToString(format, formatProvider),
			Extents.ToString(format, formatProvider)
		});
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		serializer.Serialize(ref Extents);
		serializer.Serialize(ref Transformation);
	}
}
