namespace Firaxis.CivTech;

public interface INativeCrashHandlerService
{
	ulong SessionHash { get; set; }

	void EnableCollection(bool bEnabled);
}
