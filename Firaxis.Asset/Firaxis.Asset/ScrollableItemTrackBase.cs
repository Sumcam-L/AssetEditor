using System.ComponentModel;
using System.Drawing;
using Firaxis.Controls.Scrollables;

namespace Firaxis.Asset;

public class ScrollableItemTrackBase : ScrollableItemTree
{
	protected const float Saturate = 0f;

	[Browsable(false)]
	public Color DurationEndColor { get; set; } = Color.LightGray;

	[Browsable(false)]
	public Color ChangingBackColor { get; set; } = Color.FromArgb(64, Color.Black);

	[Browsable(false)]
	public Color ChangingTextColor { get; set; } = Color.White;

	[Browsable(false)]
	public Color MarkColor { get; set; } = Color.Orange;

	public ScrollableItemTrackBase(Font font)
		: this(font, null)
	{
	}

	public ScrollableItemTrackBase(Font font, Image img)
		: base(font)
	{
		Image = img;
	}
}
