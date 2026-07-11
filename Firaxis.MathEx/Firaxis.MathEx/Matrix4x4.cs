namespace Firaxis.MathEx;

public class Matrix4x4
{
	public static readonly Matrix4x4 Identity;

	public static readonly Matrix4x4 Zero;

	public float[,] Data { get; set; }

	public float this[int x, int y]
	{
		get
		{
			return Data[x, y];
		}
		set
		{
			Data[x, y] = value;
		}
	}

	public float this[int i]
	{
		get
		{
			return Data[i / 4, i % 4];
		}
		set
		{
			Data[i / 4, i % 4] = value;
		}
	}

	public Matrix4x4()
	{
		Data = new float[4, 4];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				Data[i, j] = 0f;
			}
		}
	}

	public Matrix4x4(float[,] afInit)
	{
		Data = new float[4, 4];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				Data[i, j] = afInit[i, j];
			}
		}
	}

	public Matrix4x4(Matrix4x4 kCopy)
	{
		Data = new float[4, 4];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				Data[i, j] = kCopy[i, j];
			}
		}
	}

	public Matrix4x4(Vec3 kTranslation, Quaternion kRotation)
	{
		Matrix4x4 identity = Identity;
		Matrix4x4 matrix4x = kRotation.ToMatrix4x4();
		for (int i = 0; i < 3; i++)
		{
			identity[3, i] = 0f - kTranslation[i];
		}
		Matrix4x4 matrix4x2 = identity * matrix4x;
		Data = matrix4x2.Data;
	}

	public static Matrix4x4 operator *(Matrix4x4 m1, Matrix4x4 m2)
	{
		Multiply(out var kResult, m1, m2);
		return kResult;
	}

	public static Vec3 operator *(Vec3 v1, Matrix4x4 m1)
	{
		Vec3 result = default(Vec3);
		for (int i = 0; i < 4; i++)
		{
			result[i] = 0f;
			for (int j = 0; j < 4; j++)
			{
				result[i] += v1[j] * m1.Data[j, i];
			}
		}
		return result;
	}

	public void Transpose()
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = i + 1; j < 4; j++)
			{
				float num = Data[i, j];
				Data[i, j] = Data[j, i];
				Data[j, i] = num;
			}
		}
	}

	public static void Multiply(out Matrix4x4 kResult, Matrix4x4 m1, Matrix4x4 m2)
	{
		Matrix4x4 matrix4x = new Matrix4x4();
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				matrix4x.Data[i, j] = 0f;
				for (int k = 0; k < 4; k++)
				{
					matrix4x.Data[i, j] += m1.Data[i, k] * m2.Data[k, j];
				}
			}
		}
		kResult = matrix4x;
	}

	public static void TransformCoord(out Vec3 kResult, Vec3 v1, Matrix4x4 m1)
	{
		float num = m1.Data[0, 0] * v1.X + m1.Data[1, 0] * v1.Y + m1.Data[2, 0] * v1.Z + m1.Data[3, 0];
		float num2 = m1.Data[0, 1] * v1.X + m1.Data[1, 1] * v1.Y + m1.Data[2, 1] * v1.Z + m1.Data[3, 1];
		float num3 = m1.Data[0, 2] * v1.X + m1.Data[1, 2] * v1.Y + m1.Data[2, 2] * v1.Z + m1.Data[3, 2];
		float num4 = m1.Data[0, 3] * v1.X + m1.Data[1, 3] * v1.Y + m1.Data[2, 3] * v1.Z + m1.Data[3, 3];
		float num5 = 1f / num4;
		kResult = default(Vec3);
		kResult.X = num * num5;
		kResult.Y = num2 * num5;
		kResult.Z = num3 * num5;
	}

	public static void RotationX(out Matrix4x4 kResult, float fRot)
	{
		MathExtension.SinCos(fRot, out var s, out var c);
		kResult = new Matrix4x4();
		kResult.Data[0, 0] = 1f;
		kResult.Data[0, 1] = 0f;
		kResult.Data[0, 2] = 0f;
		kResult.Data[0, 3] = 0f;
		kResult.Data[1, 0] = 0f;
		kResult.Data[1, 1] = c;
		kResult.Data[1, 2] = s;
		kResult.Data[1, 3] = 0f;
		kResult.Data[2, 0] = 0f;
		kResult.Data[2, 1] = 0f - s;
		kResult.Data[2, 2] = c;
		kResult.Data[2, 3] = 0f;
		kResult.Data[3, 0] = 0f;
		kResult.Data[3, 1] = 0f;
		kResult.Data[3, 2] = 0f;
		kResult.Data[3, 3] = 1f;
	}

	public static void RotationY(out Matrix4x4 kResult, float fRot)
	{
		MathExtension.SinCos(fRot, out var s, out var c);
		kResult = new Matrix4x4();
		kResult.Data[0, 0] = c;
		kResult.Data[0, 1] = 0f;
		kResult.Data[0, 2] = 0f - s;
		kResult.Data[0, 3] = 0f;
		kResult.Data[1, 0] = 0f;
		kResult.Data[1, 1] = 1f;
		kResult.Data[1, 2] = 0f;
		kResult.Data[1, 3] = 0f;
		kResult.Data[2, 0] = s;
		kResult.Data[2, 1] = 0f;
		kResult.Data[2, 2] = c;
		kResult.Data[2, 3] = 0f;
		kResult.Data[3, 0] = 0f;
		kResult.Data[3, 1] = 0f;
		kResult.Data[3, 2] = 0f;
		kResult.Data[3, 3] = 1f;
	}

	public static void RotationZ(out Matrix4x4 kResult, float fRot)
	{
		MathExtension.SinCos(fRot, out var s, out var c);
		kResult = new Matrix4x4();
		kResult.Data[0, 0] = c;
		kResult.Data[0, 1] = s;
		kResult.Data[0, 2] = 0f;
		kResult.Data[0, 3] = 0f;
		kResult.Data[1, 0] = 0f - s;
		kResult.Data[1, 1] = c;
		kResult.Data[1, 2] = 0f;
		kResult.Data[1, 3] = 0f;
		kResult.Data[2, 0] = 0f;
		kResult.Data[2, 1] = 0f;
		kResult.Data[2, 2] = 1f;
		kResult.Data[2, 3] = 0f;
		kResult.Data[3, 0] = 0f;
		kResult.Data[3, 1] = 0f;
		kResult.Data[3, 2] = 0f;
		kResult.Data[3, 3] = 1f;
	}

	public static void SetTranslate(Matrix4x4 kResult, Vec3 v1)
	{
		kResult.Data[3, 0] = v1.X;
		kResult.Data[3, 1] = v1.Y;
		kResult.Data[3, 2] = v1.Z;
		kResult.Data[3, 3] = 1f;
	}

	static Matrix4x4()
	{
		Zero = new Matrix4x4();
		Identity = new Matrix4x4();
		for (int i = 0; i < 4; i++)
		{
			Identity.Data[i, i] = 1f;
		}
	}
}
