using System;
using System.Globalization;
using System.Runtime.InteropServices;
using SharpDX.Serialization;

namespace SharpDX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Viewport : IEquatable<Viewport>, IDataSerializable
{
	public int X;

	public int Y;

	public int Width;

	public int Height;

	public float MinDepth;

	public float MaxDepth;

	public Rectangle Bounds
	{
		get
		{
			return new Rectangle(X, Y, Width, Height);
		}
		set
		{
			X = value.X;
			Y = value.Y;
			Width = value.Width;
			Height = value.Height;
		}
	}

	public float AspectRatio
	{
		get
		{
			if (Height != 0)
			{
				return (float)Width / (float)Height;
			}
			return 0f;
		}
	}

	public Viewport(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
		MinDepth = 0f;
		MaxDepth = 1f;
	}

	public Viewport(int x, int y, int width, int height, float minDepth, float maxDepth)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
		MinDepth = minDepth;
		MaxDepth = maxDepth;
	}

	public Viewport(Rectangle bounds)
	{
		X = bounds.X;
		Y = bounds.Y;
		Width = bounds.Width;
		Height = bounds.Height;
		MinDepth = 0f;
		MaxDepth = 1f;
	}

	public bool Equals(Viewport other)
	{
		if (X == other.X && Y == other.Y && Width == other.Width && Height == other.Height && MathUtil.NearEqual(MinDepth, other.MinDepth))
		{
			return MathUtil.NearEqual(MaxDepth, other.MaxDepth);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (obj is Viewport)
		{
			return Equals((Viewport)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int x = X;
		x = (x * 397) ^ Y;
		x = (x * 397) ^ Width;
		x = (x * 397) ^ Height;
		x = (x * 397) ^ MinDepth.GetHashCode();
		return (x * 397) ^ MaxDepth.GetHashCode();
	}

	public static bool operator ==(Viewport left, Viewport right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Viewport left, Viewport right)
	{
		return !left.Equals(right);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{{X:{0} Y:{1} Width:{2} Height:{3} MinDepth:{4} MaxDepth:{5}}}", X, Y, Width, Height, MinDepth, MaxDepth);
	}

	public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
	{
		Matrix.Multiply(ref world, ref view, out var result);
		Matrix.Multiply(ref result, ref projection, out result);
		Project(ref source, ref result, out var vector);
		return vector;
	}

	public void Project(ref Vector3 source, ref Matrix matrix, out Vector3 vector)
	{
		Vector3.Transform(ref source, ref matrix, out vector);
		float num = source.X * matrix.M14 + source.Y * matrix.M24 + source.Z * matrix.M34 + matrix.M44;
		if (!MathUtil.IsOne(num))
		{
			vector /= num;
		}
		vector.X = (vector.X + 1f) * 0.5f * (float)Width + (float)X;
		vector.Y = (0f - vector.Y + 1f) * 0.5f * (float)Height + (float)Y;
		vector.Z = vector.Z * (MaxDepth - MinDepth) + MinDepth;
	}

	public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
	{
		Matrix.Multiply(ref world, ref view, out var result);
		Matrix.Multiply(ref result, ref projection, out result);
		Matrix.Invert(ref result, out result);
		Unproject(ref source, ref result, out var vector);
		return vector;
	}

	public void Unproject(ref Vector3 source, ref Matrix matrix, out Vector3 vector)
	{
		vector.X = (source.X - (float)X) / (float)Width * 2f - 1f;
		vector.Y = 0f - ((source.Y - (float)Y) / (float)Height * 2f - 1f);
		vector.Z = (source.Z - MinDepth) / (MaxDepth - MinDepth);
		float num = vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + matrix.M44;
		Vector3.Transform(ref vector, ref matrix, out vector);
		if (!MathUtil.IsOne(num))
		{
			vector /= num;
		}
	}

	void IDataSerializable.Serialize(BinarySerializer serializer)
	{
		if (serializer.Mode == SerializerMode.Write)
		{
			serializer.Writer.Write(X);
			serializer.Writer.Write(Y);
			serializer.Writer.Write(Width);
			serializer.Writer.Write(Height);
			serializer.Writer.Write(MinDepth);
			serializer.Writer.Write(MaxDepth);
		}
		else
		{
			X = serializer.Reader.ReadInt32();
			Y = serializer.Reader.ReadInt32();
			Width = serializer.Reader.ReadInt32();
			Height = serializer.Reader.ReadInt32();
			MinDepth = serializer.Reader.ReadSingle();
			MaxDepth = serializer.Reader.ReadSingle();
		}
	}
}
