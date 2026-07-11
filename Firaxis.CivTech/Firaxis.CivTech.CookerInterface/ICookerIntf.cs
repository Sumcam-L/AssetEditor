using System;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.CivTech.CookerInterface;

public interface ICookerIntf : IAssemblyInstance, IDisposable
{
	event LogEventHandler CookerLog;

	void Startup(IToolHostInterface ToolHost);

	void Configure(ICookerOptions options);

	void Shutdown();

	ICookerResult Cook(ICookerOptions options);
}
