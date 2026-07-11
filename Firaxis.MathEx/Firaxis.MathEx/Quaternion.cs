using System;

namespace Firaxis.MathEx;

public class Quaternion
{
	private static Quaternion ms_kIdentity = new Quaternion(0f, 0f, 0f, 1f);

	public float X { get; set; }

	public float Y { get; set; }

	public float Z { get; set; }

	public float W { get; set; }

	public static Quaternion Identity => ms_kIdentity;

	public Quaternion(float fX, float fY, float fZ, float fW)
	{
		X = fX;
		Y = fY;
		Z = fZ;
		W = fW;
		Normalize();
	}

	public static Quaternion operator *(Quaternion quat1, Quaternion quat2)
	{
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 0f);
		quaternion.W = quat1.W * quat2.W - quat1.X * quat2.X - quat1.Y * quat2.Y - quat1.Z * quat2.Z;
		quaternion.X = quat1.W * quat2.X + quat1.X * quat2.W + quat1.Y * quat2.Z - quat1.Z * quat2.Y;
		quaternion.Y = quat1.W * quat2.Y - quat1.X * quat2.Z + quat1.Y * quat2.W + quat1.Z * quat2.X;
		quaternion.Z = quat1.W * quat2.Z + quat1.X * quat2.Y - quat1.Y * quat2.X + quat1.Z * quat2.W;
		return quaternion;
	}

	public float[,] ToMatrix4x4InPlace(ref float[,] akResult)
	{
		akResult[3, 0] = (akResult[3, 1] = (akResult[3, 2] = 0f));
		akResult[0, 3] = (akResult[1, 3] = (akResult[2, 3] = 0f));
		akResult[3, 3] = 1f;
		akResult[0, 0] = 1f - 2f * Y * Y - 2f * Z * Z;
		akResult[0, 1] = 2f * X * Y - 2f * Z * W;
		akResult[0, 2] = 2f * X * Z + 2f * Y * W;
		akResult[1, 0] = 2f * X * Y + 2f * Z * W;
		akResult[1, 1] = 1f - 2f * X * X - 2f * Z * Z;
		akResult[1, 2] = 2f * Y * Z - 2f * X * W;
		akResult[2, 0] = 2f * X * Z - 2f * Y * W;
		akResult[2, 1] = 2f * Y * Z + 2f * X * W;
		akResult[2, 2] = 1f - 2f * X * X - 2f * Y * Y;
		return akResult;
	}

	public Matrix4x4 ToMatrix4x4()
	{
		float[,] akResult = new float[4, 4];
		ToMatrix4x4InPlace(ref akResult);
		return new Matrix4x4(akResult);
	}

	public void Normalize()
	{
		float num = (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W);
		if (num > 0.001f)
		{
			X /= num;
			Y /= num;
			Z /= num;
			W /= num;
		}
	}

	public static Quaternion FromRotationMatrix(Matrix4x4 kMtx)
	{
		float num = 1f + kMtx[0] + kMtx[5] + kMtx[10];
		float fX;
		float fY;
		float fZ;
		float fW;
		if (num > 0.0001f)
		{
			float num2 = (float)Math.Sqrt(num) * 2f;
			fX = (kMtx[9] - kMtx[6]) / num2;
			fY = (kMtx[2] - kMtx[8]) / num2;
			fZ = (kMtx[4] - kMtx[1]) / num2;
			fW = 0.25f * num2;
		}
		else if (kMtx[0] > kMtx[5] && kMtx[0] > kMtx[10])
		{
			float num2 = (float)Math.Sqrt(1.0 + (double)kMtx[0] - (double)kMtx[5] - (double)kMtx[10]) * 2f;
			fX = 0.25f * num2;
			fY = (kMtx[4] + kMtx[1]) / num2;
			fZ = (kMtx[2] + kMtx[8]) / num2;
			fW = (kMtx[9] - kMtx[6]) / num2;
		}
		else if (kMtx[5] > kMtx[10])
		{
			float num2 = (float)Math.Sqrt(1.0 + (double)kMtx[5] - (double)kMtx[0] - (double)kMtx[10]) * 2f;
			fX = (kMtx[4] + kMtx[1]) / num2;
			fY = 0.25f * num2;
			fZ = (kMtx[9] + kMtx[6]) / num2;
			fW = (kMtx[2] - kMtx[8]) / num2;
		}
		else
		{
			float num2 = (float)Math.Sqrt(1.0 + (double)kMtx[10] - (double)kMtx[0] - (double)kMtx[5]) * 2f;
			fX = (kMtx[2] + kMtx[8]) / num2;
			fY = (kMtx[9] + kMtx[6]) / num2;
			fZ = 0.25f * num2;
			fW = (kMtx[4] - kMtx[1]) / num2;
		}
		return new Quaternion(fX, fY, fZ, fW);
	}
}
