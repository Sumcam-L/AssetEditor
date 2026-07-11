using System.Drawing;

namespace Firaxis.Granny;

public interface IGrannyVertex
{
	Point Position { get; }

	Point Normal { get; }
}
