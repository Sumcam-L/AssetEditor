using System;
using System.Drawing;

namespace Firaxis.Reflection;

public class ColorProviderAttribute : Attribute
{
	public Color Color { get; private set; }

	public ColorProviderAttribute(int alpha, int red, int green, int blue)
	{
		Color = Color.FromArgb(alpha, red, green, blue);
	}

	public ColorProviderAttribute(int red, int green, int blue)
		: this(255, red, green, blue)
	{
	}
}
