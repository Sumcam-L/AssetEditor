using System;

namespace Sce.Atf;

public interface IPerformanceTarget
{
	event EventHandler EventOccurred;

	void DoEvent();
}
