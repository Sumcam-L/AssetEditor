using System;

namespace Firaxis.MathEx;

public class QuadraticEquation
{
	private struct Root
	{
		public double R1_Re;

		public double R1_Im;

		public double R2_Re;

		public double R2_Im;
	}

	private Root MyRoot;

	private double cA = 0.0;

	private double cB = 0.0;

	private double cC = 0.0;

	public double Acoefficient
	{
		get
		{
			return cA;
		}
		set
		{
			cA = value;
		}
	}

	public double Bcoefficient
	{
		get
		{
			return cB;
		}
		set
		{
			cB = value;
		}
	}

	public double Ccoefficient
	{
		get
		{
			return cC;
		}
		set
		{
			cC = value;
		}
	}

	public double Root1RealPart => MyRoot.R1_Re;

	public double Root1ImagPart => MyRoot.R1_Im;

	public double Root2RealPart => MyRoot.R2_Re;

	public double Root2ImagPart => MyRoot.R2_Im;

	public QuadraticEquation()
	{
	}

	public QuadraticEquation(double A, double B, double C)
	{
		cA = A;
		cB = B;
		cC = C;
	}

	public double Discriminant()
	{
		return cB * cB - 4.0 * cA * cC;
	}

	public void Solve()
	{
		if (cA == 0.0)
		{
			MyRoot.R1_Re = (0.0 - cC) / cB;
			MyRoot.R2_Re = MyRoot.R1_Re;
			return;
		}
		double num = Discriminant();
		double num2 = cA + cA;
		double num3 = (0.0 - cB) / num2;
		if (num == 0.0)
		{
			MyRoot.R1_Re = (MyRoot.R2_Re = num3);
			MyRoot.R1_Im = (MyRoot.R2_Im = 0.0);
		}
		else if (num > 0.0)
		{
			MyRoot.R1_Re = num3 + Math.Sqrt(num) / num2;
			MyRoot.R2_Re = num3 - Math.Sqrt(num) / num2;
			MyRoot.R1_Im = 0.0;
			MyRoot.R2_Im = 0.0;
		}
		else if (num < 0.0)
		{
			MyRoot.R1_Re = (MyRoot.R2_Re = num3);
			num = 0.0 - num;
			MyRoot.R1_Im = Math.Sqrt(num) / num2;
			MyRoot.R2_Im = (0.0 - Math.Sqrt(num)) / num2;
		}
	}
}
