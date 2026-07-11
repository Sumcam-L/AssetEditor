using System.Drawing;

namespace Sce.Atf.Direct2D;

public struct D2dEllipse
{
	public PointF Center;

	public float RadiusX;

	public float RadiusY;

	public D2dEllipse(PointF center, float radiusX, float radiusY)
	{
		Center = center;
		RadiusX = radiusX;
		RadiusY = radiusY;
	}

	public static explicit operator D2dEllipse(Rectangle rect)
	{
		D2dEllipse result = default(D2dEllipse);
		float num = (float)rect.Width / 2f;
		float num2 = (float)rect.Height / 2f;
		result.Center.X = (float)rect.X + num;
		result.Center.Y = (float)rect.Y + num2;
		result.RadiusX = num;
		result.RadiusY = num2;
		return result;
	}

	public static explicit operator D2dEllipse(RectangleF rect)
	{
		D2dEllipse result = default(D2dEllipse);
		float num = rect.Width / 2f;
		float num2 = rect.Height / 2f;
		result.Center.X = rect.X + num;
		result.Center.Y = rect.Y + num2;
		result.RadiusX = num;
		result.RadiusY = num2;
		return result;
	}
}
