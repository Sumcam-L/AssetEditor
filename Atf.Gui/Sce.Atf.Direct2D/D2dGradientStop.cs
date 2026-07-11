using System.Drawing;

namespace Sce.Atf.Direct2D;

public struct D2dGradientStop
{
	public Color Color;

	public float Position;

	public D2dGradientStop(Color color, float position)
	{
		Color = color;
		Position = position;
	}
}
