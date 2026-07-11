using System;

namespace Firaxis.Asset;

public interface ISourceControlLabel
{
	string Name { get; }

	DateTime Date { get; }
}
