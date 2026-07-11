using System;

namespace Firaxis.Utility;

public interface IMaker
{
	string Name { get; }

	Type Type { get; }

	object Make();

	object Make(params object[] args);
}
