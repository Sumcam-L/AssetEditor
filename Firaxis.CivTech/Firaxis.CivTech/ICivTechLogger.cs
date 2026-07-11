using System;

namespace Firaxis.CivTech;

public interface ICivTechLogger : IAssemblyInstance, IDisposable
{
	event SourcedLogEventHandler EngineLog;

	void AddLogItem(LogEventType evtType, string source, string msg);
}
