using System.Drawing;

namespace Firaxis.Utility;

public interface IBoundsProvider
{
	RectangleF Bounds { get; set; }
}
