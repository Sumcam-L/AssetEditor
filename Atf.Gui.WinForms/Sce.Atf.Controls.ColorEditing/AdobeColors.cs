using System.Drawing;

namespace Sce.Atf.Controls.ColorEditing;

public class AdobeColors
{
	public class HSL
	{
		private double _a;

		private double _h;

		private double _s;

		private double _l;

		public double A
		{
			get
			{
				return _a;
			}
			set
			{
				_a = value;
				_a = ((_a > 1.0) ? 1.0 : ((_a < 0.0) ? 0.0 : _a));
			}
		}

		public double H
		{
			get
			{
				return _h;
			}
			set
			{
				_h = value;
				_h = ((_h > 1.0) ? 1.0 : ((_h < 0.0) ? 0.0 : _h));
			}
		}

		public double S
		{
			get
			{
				return _s;
			}
			set
			{
				_s = value;
				_s = ((_s > 1.0) ? 1.0 : ((_s < 0.0) ? 0.0 : _s));
			}
		}

		public double L
		{
			get
			{
				return _l;
			}
			set
			{
				_l = value;
				_l = ((_l > 1.0) ? 1.0 : ((_l < 0.0) ? 0.0 : _l));
			}
		}

		public HSL()
		{
			_a = 1.0;
			_h = 0.0;
			_s = 0.0;
			_l = 0.0;
		}
	}

	public class CMYK
	{
		private double _a;

		private double _c;

		private double _m;

		private double _y;

		private double _k;

		public double A
		{
			get
			{
				return _a;
			}
			set
			{
				_a = value;
				_a = ((_a > 1.0) ? 1.0 : ((_a < 0.0) ? 0.0 : _a));
			}
		}

		public double C
		{
			get
			{
				return _c;
			}
			set
			{
				_c = value;
				_c = ((_c > 1.0) ? 1.0 : ((_c < 0.0) ? 0.0 : _c));
			}
		}

		public double M
		{
			get
			{
				return _m;
			}
			set
			{
				_m = value;
				_m = ((_m > 1.0) ? 1.0 : ((_m < 0.0) ? 0.0 : _m));
			}
		}

		public double Y
		{
			get
			{
				return _y;
			}
			set
			{
				_y = value;
				_y = ((_y > 1.0) ? 1.0 : ((_y < 0.0) ? 0.0 : _y));
			}
		}

		public double K
		{
			get
			{
				return _k;
			}
			set
			{
				_k = value;
				_k = ((_k > 1.0) ? 1.0 : ((_k < 0.0) ? 0.0 : _k));
			}
		}

		public CMYK()
		{
			_a = 1.0;
			_c = 0.0;
			_m = 0.0;
			_y = 0.0;
			_k = 0.0;
		}
	}

	public static Color SetBrightness(Color c, double brightness)
	{
		HSL hSL = RGB_to_HSL(c);
		hSL.L = brightness;
		return HSL_to_RGB(hSL);
	}

	public static Color ModifyBrightness(Color c, double brightness)
	{
		HSL hSL = RGB_to_HSL(c);
		hSL.L *= brightness;
		return HSL_to_RGB(hSL);
	}

	public static Color SetSaturation(Color c, double Saturation)
	{
		HSL hSL = RGB_to_HSL(c);
		hSL.S = Saturation;
		return HSL_to_RGB(hSL);
	}

	public static Color ModifySaturation(Color c, double Saturation)
	{
		HSL hSL = RGB_to_HSL(c);
		hSL.S *= Saturation;
		return HSL_to_RGB(hSL);
	}

	public static Color SetHue(Color c, double Hue)
	{
		HSL hSL = RGB_to_HSL(c);
		hSL.H = Hue;
		return HSL_to_RGB(hSL);
	}

	public static Color ModifyHue(Color c, double Hue)
	{
		HSL hSL = RGB_to_HSL(c);
		hSL.H *= Hue;
		return HSL_to_RGB(hSL);
	}

	public static Color HSL_to_RGB(HSL hsl)
	{
		int alpha = Round(255.0 * hsl.A);
		int num = Round(hsl.L * 255.0);
		int num2 = Round((1.0 - hsl.S) * (hsl.L / 1.0) * 255.0);
		double num3 = (double)(num - num2) / 255.0;
		if (hsl.H >= 0.0 && hsl.H <= 1.0 / 6.0)
		{
			int green = Round((hsl.H - 0.0) * num3 * 1530.0 + (double)num2);
			return Color.FromArgb(alpha, num, green, num2);
		}
		if (hsl.H <= 1.0 / 3.0)
		{
			int green = Round((0.0 - (hsl.H - 1.0 / 6.0) * num3) * 1530.0 + (double)num);
			return Color.FromArgb(alpha, green, num, num2);
		}
		if (hsl.H <= 0.5)
		{
			int green = Round((hsl.H - 1.0 / 3.0) * num3 * 1530.0 + (double)num2);
			return Color.FromArgb(alpha, num2, num, green);
		}
		if (hsl.H <= 2.0 / 3.0)
		{
			int green = Round((0.0 - (hsl.H - 0.5) * num3) * 1530.0 + (double)num);
			return Color.FromArgb(alpha, num2, green, num);
		}
		if (hsl.H <= 5.0 / 6.0)
		{
			int green = Round((hsl.H - 2.0 / 3.0) * num3 * 1530.0 + (double)num2);
			return Color.FromArgb(alpha, green, num2, num);
		}
		if (hsl.H <= 1.0)
		{
			int green = Round((0.0 - (hsl.H - 5.0 / 6.0) * num3) * 1530.0 + (double)num);
			return Color.FromArgb(alpha, num, num2, green);
		}
		return Color.FromArgb(alpha, 0, 0, 0);
	}

	public static HSL RGB_to_HSL(Color c)
	{
		HSL hSL = new HSL();
		hSL.A = (double)(int)c.A / 255.0;
		int num;
		int num2;
		if (c.R > c.G)
		{
			num = c.R;
			num2 = c.G;
		}
		else
		{
			num = c.G;
			num2 = c.R;
		}
		if (c.B > num)
		{
			num = c.B;
		}
		else if (c.B < num2)
		{
			num2 = c.B;
		}
		int num3 = num - num2;
		hSL.L = (double)num / 255.0;
		if (num == 0)
		{
			hSL.S = 0.0;
		}
		else
		{
			hSL.S = (double)num3 / (double)num;
		}
		double num4 = ((num3 != 0) ? (60.0 / (double)num3) : 0.0);
		if (num == c.R)
		{
			if (c.G < c.B)
			{
				hSL.H = (360.0 + num4 * (double)(c.G - c.B)) / 360.0;
			}
			else
			{
				hSL.H = num4 * (double)(c.G - c.B) / 360.0;
			}
		}
		else if (num == c.G)
		{
			hSL.H = (120.0 + num4 * (double)(c.B - c.R)) / 360.0;
		}
		else if (num == c.B)
		{
			hSL.H = (240.0 + num4 * (double)(c.R - c.G)) / 360.0;
		}
		else
		{
			hSL.H = 0.0;
		}
		return hSL;
	}

	public static CMYK RGB_to_CMYK(Color c)
	{
		CMYK cMYK = new CMYK();
		double num = 1.0;
		cMYK.A = (double)(int)c.A / 255.0;
		cMYK.C = (double)(255 - c.R) / 255.0;
		if (num > cMYK.C)
		{
			num = cMYK.C;
		}
		cMYK.M = (double)(255 - c.G) / 255.0;
		if (num > cMYK.M)
		{
			num = cMYK.M;
		}
		cMYK.Y = (double)(255 - c.B) / 255.0;
		if (num > cMYK.Y)
		{
			num = cMYK.Y;
		}
		if (num > 0.0)
		{
			cMYK.K = num;
		}
		return cMYK;
	}

	public static Color CMYK_to_RGB(CMYK _cmyk)
	{
		int alpha = Round(255.0 * _cmyk.A);
		int red = Round(255.0 - 255.0 * _cmyk.C);
		int green = Round(255.0 - 255.0 * _cmyk.M);
		int blue = Round(255.0 - 255.0 * _cmyk.Y);
		return Color.FromArgb(alpha, red, green, blue);
	}

	private static int Round(double val)
	{
		int num = (int)val;
		int num2 = (int)(val * 100.0);
		if (num2 % 100 >= 50)
		{
			num++;
		}
		return num;
	}
}
