using System;
using System.ComponentModel.Composition;
using System.Reflection;
using DatabaseWrapper;
using Firaxis.Asset;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Granny;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FiraxisATFScriptVariables : IInitializable
{
	[Import(AllowDefault = true)]
	private IArtDefRegistry m_artDefRegistry;

	[Import(AllowDefault = true)]
	private AssetBrowserFileCommands m_assetBrowserFileCommands;

	[Import(AllowDefault = true)]
	private IXLPRegistry m_xlpRegistry;

	[Import(AllowDefault = true)]
	private IEntityCacheService m_entityCacheService;

	[Import(AllowDefault = true)]
	private IFileWatcherService m_fileWatchService;

	private readonly ScriptingService m_scriptingService;

	[ImportingConstructor]
	public FiraxisATFScriptVariables(ScriptingService scriptService)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Starting up FiraxisATFScriptVariables");
		m_scriptingService = scriptService;
	}

	public virtual void Initialize()
	{
		if (m_scriptingService == null)
		{
			return;
		}
		if (m_assetBrowserFileCommands != null)
		{
			m_scriptingService.SetVariable("FiraxisAtfFileCommands", m_assetBrowserFileCommands);
		}
		if (m_xlpRegistry != null)
		{
			m_scriptingService.SetVariable("XLPRegistry", m_xlpRegistry);
		}
		if (m_artDefRegistry != null)
		{
			m_scriptingService.SetVariable("ArtDefRegistry", m_artDefRegistry);
		}
		if (m_entityCacheService != null)
		{
			m_scriptingService.SetVariable("EntityCacheService", m_entityCacheService);
		}
		if (m_fileWatchService != null)
		{
			m_scriptingService.SetVariable("FileWatchService", m_fileWatchService);
		}
		m_scriptingService.LoadAssembly(typeof(global::DatabaseWrapper.DatabaseWrapper).Assembly);
		m_scriptingService.LoadAssembly(typeof(IValueSet).Assembly);
		m_scriptingService.LoadAssembly(typeof(Context).Assembly);
		m_scriptingService.LoadAssembly(typeof(Timeline).Assembly);
		m_scriptingService.LoadAssembly(typeof(WorkspaceDependencyRegistry).Assembly);
		m_scriptingService.LoadAssembly(typeof(GrannyContext).Assembly);
		m_scriptingService.LoadAssembly(typeof(XLPRegistry).Assembly);
		m_scriptingService.LoadAssembly(typeof(ArtDefRegistry).Assembly);
		m_scriptingService.LoadAssembly(typeof(EntityCacheService).Assembly);
		m_scriptingService.LoadAssembly(typeof(FileWatcherService).Assembly);
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.GetName().Name.Equals("System.Linq"))
			{
				m_scriptingService.LoadAssembly(assembly);
			}
		}
	}
}
