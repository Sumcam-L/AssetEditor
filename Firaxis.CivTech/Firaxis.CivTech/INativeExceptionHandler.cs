using System;

namespace Firaxis.CivTech;

public interface INativeExceptionHandler : IAssemblyInstance, IDisposable
{
	ulong SessionHash { get; set; }

	void EnableCollection(bool bEnabled);

	void CrashCLI();

	void CrashNative();
}
