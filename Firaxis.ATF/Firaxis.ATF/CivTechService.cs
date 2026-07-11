using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(ICivTechService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CivTechService : IInitializable, ICivTechService, IDisposable
{
	public static readonly string AnyProject = "AllowAnyProject";

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService;

	private bool disposedValue;

	private IProjectSelectionService ProjectSelectionService { get; set; }

	private IProjectConfigService ProjectConfigService { get; set; }

	private IWorkspaceDependencyRegistryService DependencyRegistryService { get; set; }

	string ICivTechService.AnyProject => AnyProject;

	public virtual IProjectMapService ProjectMapService { get; private set; }

	public virtual CivTechContext CivTechContext { get; private set; }

	public virtual IToolHostLoaderService ToolHostLoader { get; private set; }

	public virtual IProjectMap AllProjectsMap => ProjectMapService.AllProjectsMap;

	public virtual IProjectMap ActiveProjectMap => ProjectMapService.ActiveProjectMap;

	public virtual ProjectEnvironment PrimaryProject => ProjectMapService.PrimaryProject;

	public IAssetCloudSettings AssetCloudSettings { get; private set; }

	[ImportingConstructor]
	public CivTechService(IAssetCloudSettingService acss, IProjectConfigService pcs, IProjectSelectionService prjSelSvc, IProjectMapService projectMapService, IWorkspaceDependencyRegistryService wkDepRegSvc, IToolHostPolicy toolHostPolicy, IToolHostLoaderService toolHostLoaderSvc)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			ProjectSelectionService = prjSelSvc;
			ProjectConfigService = pcs;
			ProjectMapService = projectMapService;
			AssetCloudSettings = acss.AssetCloudSettings;
			DependencyRegistryService = wkDepRegSvc;
			ToolHostLoader = toolHostLoaderSvc;
			CivTechContext = Context.EnsureCreated<CivTechContext>();
			ValidateProjectSettings(AssetCloudSettings);
			if (!toolHostPolicy.UseOnDemandLoading)
			{
				ToolHostLoader.LoadToolHost();
				Outputs.WriteLine(OutputMessageType.Info, "Using ToolHost at \"" + ToolHostLoader.ToolHostDllPath + "\"");
			}
			string text = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName()?.Name ?? "UnitTest";
			string activeProject = prjSelSvc.ActiveProject;
			Outputs.WriteLine(OutputMessageType.Info, "{0}: {1} - CivTechService initialized", text, activeProject);
		}
		Context.Add(this);
	}

	public void Initialize()
	{
		if (m_scriptingService != null)
		{
			m_scriptingService.LoadAssembly(GetType().Assembly);
			m_scriptingService.SetVariable("civTech", this);
		}
	}

	public IEnumerable<string> GetActivePantryPaths()
	{
		return ProjectMapService.GetActivePantryPaths();
	}

	public IEnumerable<string> GetActiveProjects()
	{
		yield return PrimaryProject.Name;
		foreach (string dependency in PrimaryProject.Dependencies)
		{
			yield return AllProjectsMap[dependency].Name;
		}
	}

	public string GetBaseGamePantryPath()
	{
		return ProjectMapService.ActiveProjectMap.Projects.Where((ProjectEnvironment proj) => proj.Dependencies.Count == 0).FirstOrDefault().Paths.GamePantry;
	}

	public bool IsFromActiveProject(Uri uri)
	{
		return ProjectMapService.IsFromActiveProject(uri);
	}

	public bool IsFromActiveProjectOrDependencies(Uri uri)
	{
		return ProjectMapService.IsFromActiveProjectOrDependencies(uri);
	}

	public bool IsFromProjectDependencies(Uri uri)
	{
		return ProjectMapService.IsFromProjectDependencies(uri);
	}

	public bool IsFromModDependencies(Uri uri)
	{
		if (AssetCloudSettings.ModTools)
		{
			return !ProjectMapService.IsFromActiveProject(uri);
		}
		return false;
	}

	public bool IsFromPrimaryModProject(Uri uri)
	{
		if (!AssetCloudSettings.ModTools)
		{
			return true;
		}
		return ProjectMapService.IsFromPrimaryProject(uri);
	}

	public bool IsFromActiveProject(EntityID entId)
	{
		return ProjectMapService.IsFromActiveProject(entId);
	}

	public bool IsFromActiveProjectOrDependencies(EntityID entId)
	{
		return ProjectMapService.IsFromActiveProjectOrDependencies(entId);
	}

	public bool IsFromProjectDependencies(EntityID entity)
	{
		return ProjectMapService.IsFromProjectDependencies(entity);
	}

	public string GetBrowserDataPath()
	{
		AssemblyName name = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName();
		string name2 = name.Name;
		Version version = name.Version;
		string text = version.Major + "." + version.Minor;
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		return folderPath + "\\" + name2 + "\\" + text + "\\BrowserData\\browser_data.dat";
	}

	public IWorkspaceDependencyRegistry GetWorkspaceDependencyRegistry(Uri uri)
	{
		string projectName = this.GetProjectName(uri);
		ProjectEnvironment project = null;
		if (ActiveProjectMap.GetProject(projectName, ref project))
		{
			return project.DependencyRegistry;
		}
		return PrimaryProject.DependencyRegistry;
	}

	private void ValidateProjectSettings(IAssetCloudSettings settings)
	{
		if (!settings.UseLocalConfig || !(settings is IAssetCloudSettingValidation assetCloudSettingValidation))
		{
			return;
		}
		CivTechContext.CivTechLogger.AddLogItem(LogEventType.Info, "Tool", "Validating Local Overrides...\n");
		bool flag = false;
		foreach (ProjectEnvironment project in ActiveProjectMap.Projects)
		{
			if (assetCloudSettingValidation.ValidateUserLocalProjectConfig(project.VersionControl.WorkspaceRoot))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			string text = "The project config local override could not be found! Reverting to released tool components.";
			if (MessageBoxes.Show(text, "Asset Cloud", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.Cancel)
			{
				ExceptionLogger.FatalExit(-1, text, "Asset Cloud");
			}
			settings.UseLocalConfig = false;
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				Context.Remove(this);
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
