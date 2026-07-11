using System.ComponentModel;
using System.Drawing;
using Firaxis.Controls.Scrollables;

namespace Firaxis.Asset;

public class ScrollableItemAnimationBase : ScrollableItemTree
{
	[Browsable(false)]
	public Color DurationEndColor { get; set; } = Color.LightGray;

	[Browsable(false)]
	public Color ActiveFillColor { get; set; } = Color.FromArgb(96, Color.Orange);

	[Browsable(false)]
	public Color InactiveFillColor { get; set; } = Color.FromArgb(128, Color.Gray);

	[Browsable(false)]
	public Color ActiveTextColor { get; set; } = Color.Black;

	[Browsable(false)]
	public Color InactiveTextColor { get; set; } = Color.LightGray;

	public ScrollableItemAnimationBase(Font font, Image image)
		: base(font)
	{
		Image = image;
	}
}
