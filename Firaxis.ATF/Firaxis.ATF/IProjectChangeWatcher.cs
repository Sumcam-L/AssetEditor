using System;

namespace Firaxis.ATF;

public interface IProjectChangeWatcher
{
	void HandleProjectChange(Action<string> statusMessagePrinter);
}
