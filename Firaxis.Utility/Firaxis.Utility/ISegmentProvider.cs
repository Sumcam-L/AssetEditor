using Firaxis.MathEx;

namespace Firaxis.Utility;

public interface ISegmentProvider
{
	Vec2 SegA { get; set; }

	Vec2 SegB { get; set; }
}
