namespace Firaxis.MathEx;

public struct Vec4
{
	public float X;

	public float Y;

	public float Z;

	public float W;

	public Vec4(float x, float y, float z, float w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Vec4(float[] afComponents)
	{
		X = afComponents[0];
		Y = afComponents[1];
		Z = afComponents[2];
		W = afComponents[3];
	}
}
