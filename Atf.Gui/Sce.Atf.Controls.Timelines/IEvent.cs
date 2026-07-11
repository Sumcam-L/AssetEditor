using System.Drawing;

namespace Sce.Atf.Controls.Timelines;

public interface IEvent : ITimelineObject
{
	float Start { get; set; }

	float Length { get; set; }

	Color Color { get; set; }

	string Name { get; set; }
}
