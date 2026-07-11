namespace Firaxis.CivTech;

public static class ICivTechLoggerExtensions
{
	public static void AddLogItem(this ICivTechLogger logger, LogEventType evtType, string source, string msg, params object[] args)
	{
		logger.AddLogItem(evtType, source, string.Format(msg, args));
	}
}
