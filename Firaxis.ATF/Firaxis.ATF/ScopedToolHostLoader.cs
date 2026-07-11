using System;
using Firaxis.CivTech;
using Sce.Atf;

namespace Firaxis.ATF;

public class ScopedToolHostLoader : IDisposable
{
	private IToolHostLoaderService m_loaderService;

	public ScopedToolHostLoader(IToolHostLoaderService loaderSvc)
	{
		m_loaderService = loaderSvc;
		Outputs.WriteLine(OutputMessageType.Info, "Loading ToolHost at \"" + m_loaderService.ToolHostDllPath + "\"");
		m_loaderService.LoadToolHost();
	}

	public void Dispose()
	{
		Outputs.WriteLine(OutputMessageType.Info, "Unloading ToolHost at \"" + m_loaderService.ToolHostDllPath + "\"");
		m_loaderService.UnloadToolHost();
	}
}
