using System;

namespace Firaxis.ATF;

public interface IQuietTimeAction : IDisposable
{
	int UpdatesSinceLastAction { get; }

	void UpdateLastChangeTime();
}
