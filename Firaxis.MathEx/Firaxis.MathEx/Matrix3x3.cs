namespace Firaxis.MathEx;

public class Matrix3x3
{
	public static readonly Matrix3x3 Identity;

	public static readonly Matrix3x3 Zero;

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
			return Data[i / 3, i % 3];
		}
		set
		{
			Data[i / 3, i % 3] = value;
		}
	}

	public Matrix3x3()
	{
		Data = new float[3, 3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Data[i, j] = 0f;
			}
		}
	}

	public Matrix3x3(float[,] afInit)
	{
		Data = new float[3, 3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Data[i, j] = afInit[i, j];
			}
		}
	}

	public Matrix3x3(Matrix3x3 kCopy)
	{
		Data = new float[3, 3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				Data[i, j] = kCopy[i, j];
			}
		}
	}

	static Matrix3x3()
	{
		Zero = new Matrix3x3();
		Identity = new Matrix3x3();
		for (int i = 0; i < 3; i++)
		{
			Identity.Data[i, i] = 1f;
		}
	}
}
