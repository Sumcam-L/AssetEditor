using System;

namespace Firaxis.ATF;

public interface ISequencedProjectChangeWatcher
{
	void StartProjectChange(Action<string> statusMessagePrinter);

	void FinishProjectChange(Action<string> statusMessagePrinter);
}
