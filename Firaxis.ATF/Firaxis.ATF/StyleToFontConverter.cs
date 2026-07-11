using System;
using System.Drawing;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

internal static class StyleToFontConverter
{
	public static Font Convert(ValueInfo value)
	{
		if (value.Type != typeof(Font))
		{
			throw new ArgumentException("This will only convert font values!", "value");
		}
		string familyName = "Segoe UI";
		float emSize = 9f;
		FontStyle style = FontStyle.Regular;
		if (value.ConstructorParams.Count > 0)
		{
			familyName = value.ConstructorParams[0].Value;
		}
		if (value.ConstructorParams.Count > 1)
		{
			emSize = float.Parse(value.ConstructorParams[1].Value);
		}
		if (value.ConstructorParams.Count > 2)
		{
			style = (FontStyle)Enum.Parse(typeof(FontStyle), value.ConstructorParams[2].Value);
		}
		return new Font(familyName, emSize, style);
	}
}
