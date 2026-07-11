using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IProjectMapService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ProjectMapService : IProjectMapService
{
	private readonly IAssetCloudSettingService m_assetCloudSettingService;

	private readonly CivTechContext m_civTechContext = Context.EnsureCreated<CivTechContext>();

	private ProjectEnvironment m_primaryProject;

	private readonly IProjectConfigService m_projectConfigService;

	private readonly IProjectSelectionService m_projectSelectionService;

	private IVersionControlSelectionService VersionControlSelectionService { get; set; }

	public virtual IVirtualPantry LayeredPantry { get; private set; }

	public IProjectMap AllProjectsMap { get; } = new ProjectMap();

	public IProjectMap ActiveProjectMap { get; } = new ProjectMap();

	public ProjectEnvironment PrimaryProject
	{
		get
		{
			BugSubmitter.SilentAssert(m_primaryProject != null, "Accessed the ProjectMapService after it has been disposed!  @assign bwhitman");
			return m_primaryProject;
		}
		private set
		{
			m_primaryProject = value;
		}
	}

	[ImportingConstructor]
	public ProjectMapService(IProjectConfigService projCfgSvc, IProjectSelectionService projectSelectionService, IVersionControlSelectionService vcss, IAssetCloudSettingService acs)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			m_projectSelectionService = projectSelectionService;
			m_assetCloudSettingService = acs;
			m_projectConfigService = projCfgSvc;
			VersionControlSelectionService = vcss;
			SetupAllProjects();
			SetupActiveProject(m_assetCloudSettingService.AssetCloudSettings, m_projectConfigService.Config);
			SetupLayeredPantry();
		}
	}

	public IEnumerable<ProjectPaths> GetActiveProjectPaths()
	{
		yield return PrimaryProject.Paths;
		foreach (string dependency in PrimaryProject.Dependencies)
		{
			ProjectEnvironment project = null;
			if (AllProjectsMap.GetProject(dependency, ref project))
			{
				yield return project.Paths;
				continue;
			}
			Outputs.WriteLine(OutputMessageType.Error, "Project '{0}' depends on Project '{1}', but Project '{1}' does not exist in the Project Map. Please ensure the project appears in the AssetCloud.env file", PrimaryProject.Name, dependency);
		}
	}

	public IEnumerable<string> GetActivePantryPaths()
	{
		return GetProjectPantryPaths(PrimaryProject);
	}

	public IEnumerable<string> GetProjectPantryPaths(string projectName)
	{
		ProjectEnvironment project = null;
		if (!AllProjectsMap.GetProject(projectName, ref project))
		{
			return Enumerable.Empty<string>();
		}
		return GetProjectPantryPaths(project);
	}

	public IEnumerable<string> GetProjectPantryPaths(ProjectEnvironment project)
	{
		yield return project.Paths.GamePantry;
		foreach (string dependency in project.Dependencies)
		{
			ProjectEnvironment project2 = null;
			if (AllProjectsMap.GetProject(dependency, ref project2))
			{
				yield return project2.Paths.GamePantry;
				continue;
			}
			Outputs.WriteLine(OutputMessageType.Error, "Project '{0}' depends on Project '{1}', but Project '{1}' does not exist in the Project Map. Please ensure the project appears in the AssetCloud.env file", PrimaryProject.Name, dependency);
		}
	}

	public IEnumerable<ProjectEnvironment> GetProjectAndDependencies(ProjectEnvironment project)
	{
		yield return project;
		foreach (string dependency in project.Dependencies)
		{
			ProjectEnvironment project2 = null;
			if (AllProjectsMap.GetProject(dependency, ref project2))
			{
				yield return project2;
				continue;
			}
			Outputs.WriteLine(OutputMessageType.Error, "Project '{0}' depends on Project '{1}', but Project '{1}' does not exist in the Project Map. Please ensure the project appears in the AssetCloud.env file", PrimaryProject.Name, dependency);
		}
	}

	public virtual void HandleProjectChange()
	{
		SetupActiveProject(m_assetCloudSettingService.AssetCloudSettings, m_projectConfigService.Config);
		RemoveUnusedProjectsFromActiveMap();
		SetupLayeredPantry();
	}

	public string GetProjectNameFromUri(Uri uri)
	{
		string localPath = uri.LocalPath;
		foreach (ProjectEnvironment project in AllProjectsMap.Projects)
		{
			if (PathCompareHelper.StartsWith(localPath, project.Paths.GamePantry, bIgnoreCase: true) || PathCompareHelper.StartsWith(localPath, project.Paths.ArtDev, bIgnoreCase: true))
			{
				return project.Name;
			}
		}
		return PrimaryProject.Name;
	}

	public bool IsFromPrimaryProject(Uri uri)
	{
		string localPath = uri.LocalPath;
		if (PathCompareHelper.StartsWith(localPath, PrimaryProject.Paths.GamePantry, bIgnoreCase: true) || PathCompareHelper.StartsWith(localPath, PrimaryProject.Paths.ArtDev, bIgnoreCase: true))
		{
			return true;
		}
		return false;
	}

	public bool IsFromActiveProject(Uri uri)
	{
		return GetProjectNameFromUri(uri) == PrimaryProject.Name;
	}

	public bool IsFromActiveProjectOrDependencies(Uri uri)
	{
		string projectNameFromUri = GetProjectNameFromUri(uri);
		return ActiveProjectMap.ContainsProject(projectNameFromUri);
	}

	public bool IsFromProjectDependencies(Uri uri)
	{
		if (IsFromActiveProjectOrDependencies(uri))
		{
			return !IsFromActiveProject(uri);
		}
		return false;
	}

	public bool IsFromActiveProject(EntityID entity)
	{
		string pantryPath = LayeredPantry.GetPantryPath(entity.Name, entity.Type);
		if (string.IsNullOrEmpty(pantryPath))
		{
			return false;
		}
		return IsFromActiveProject(new Uri(pantryPath));
	}

	public bool IsFromActiveProjectOrDependencies(EntityID entity)
	{
		string pantryPath = LayeredPantry.GetPantryPath(entity.Name, entity.Type);
		if (string.IsNullOrEmpty(pantryPath))
		{
			return false;
		}
		return IsFromActiveProjectOrDependencies(new Uri(pantryPath));
	}

	public bool IsFromProjectDependencies(EntityID entity)
	{
		if (IsFromActiveProjectOrDependencies(entity))
		{
			return !IsFromActiveProject(entity);
		}
		return false;
	}

	private void SetupAllProjects()
	{
		using (new ScopedStopwatch("Full project map setup took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			ProjectInfo[] projInfos = m_projectSelectionService.Projects.ProjectInfos.ToArray();
			IAssetCloudSettings assetCloudSettings = m_assetCloudSettingService.AssetCloudSettings;
			IProjectConfig config = m_projectConfigService.Config;
			SetupProjectEnvironments(AllProjectsMap, projInfos, assetCloudSettings, config);
			SetupProjectEnvironmentDependencies(AllProjectsMap, assetCloudSettings, config);
			FixupProjectEnvironmentConfigs();
		}
	}

	private void SetupProjectEnvironments(IProjectMap projMap, ProjectInfo[] projInfos, IAssetCloudSettings settings, IProjectConfig projCfg)
	{
		foreach (ProjectInfo projectInfo in projInfos)
		{
			if (!VersionControlSelectionService.VersionControlInfoMap.ContainsKey(projectInfo.Name))
			{
				Outputs.Write(OutputMessageType.Error, "Failed to find version control setup for project {0}, cooking this project will not work in some cases!", projectInfo.Name);
			}
			else
			{
				SetupProject(projMap, settings, projCfg, projectInfo);
			}
		}
	}

	private void SetupProjectEnvironmentDependencies(IProjectMap projMap, IAssetCloudSettings settings, IProjectConfig projCfg)
	{
		Parallel.ForEach(projMap.Projects, delegate(ProjectEnvironment prjEnv)
		{
			IDictionary<string, IList<string>> dictionary = new Dictionary<string, IList<string>>();
			BuildDependencyList(projMap, settings, projCfg, prjEnv.Name, dictionary, prjEnv.Info.ProjectType != ProjectType.eTesting);
			foreach (string key in dictionary.Keys)
			{
				prjEnv.Dependencies.Add(key);
			}
		});
	}

	private void BuildDependencyList(IProjectMap projMap, IAssetCloudSettings settings, IProjectConfig projCfg, string projectName, IDictionary<string, IList<string>> projectDependencies, bool warnOnDuplicateDeps)
	{
		BugSubmitter.SilentAssert(projMap.ContainsProject(projectName), "BuildDependencyListAndProjectMap broken ActiveProjectMap contract!  @assign bwhitman");
		ProjectEnvironment projectEnvironment = projMap[projectName];
		IList<ProjectEnvironment> list = new List<ProjectEnvironment>();
		foreach (IGameArtID requiredGameArtID in projectEnvironment.PrimaryArtSpecification.RequiredGameArtIDs)
		{
			if (projectDependencies.ContainsKey(requiredGameArtID.Name))
			{
				if (warnOnDuplicateDeps)
				{
					if (projectDependencies[requiredGameArtID.Name].Count > 1)
					{
						Outputs.WriteLine(OutputMessageType.Error, "Project '{0}' is a dependent project of {1}, but {2} have also added a dependency to '{0}'. Please ensure the dependency chain only includes each project once in the Art.xml chain.", requiredGameArtID.Name, projectName, string.Join(",", projectDependencies[requiredGameArtID.Name]));
					}
					else
					{
						Outputs.WriteLine(OutputMessageType.Error, "Project '{0}' is a dependent project of {1}, but {2} has also added a dependency to '{0}'. Please ensure the dependency chain only includes each project once in the Art.xml chain.", requiredGameArtID.Name, projectName, projectDependencies[requiredGameArtID.Name].First());
					}
				}
			}
			else if (!m_projectSelectionService.Projects.ContainsProject(requiredGameArtID.Name))
			{
				Outputs.WriteLine(OutputMessageType.Error, "Project '{0}' is a dependent project of {1}, but it does not exist in the Project Map. Please ensure the project appears in the AssetCloud.env file", requiredGameArtID.Name, projectName);
			}
			else
			{
				ProjectInfo projectInfo = m_projectSelectionService.Projects[requiredGameArtID.Name];
				ProjectEnvironment item = projMap[projectInfo.Name];
				projectDependencies[requiredGameArtID.Name] = new List<string> { projectEnvironment.Name };
				list.Add(item);
			}
		}
		foreach (ProjectEnvironment item2 in list)
		{
			BuildDependencyList(projMap, settings, projCfg, item2.Name, projectDependencies, warnOnDuplicateDeps);
		}
	}

	private void EnsureDirectoriesExist(ProjectPaths projectPaths)
	{
		foreach (InstanceType value in Enum.GetValues(typeof(InstanceType)))
		{
			if (value != InstanceType.IT_COUNT && value != InstanceType.IT_INVALID)
			{
				string path = StaticMethods.PantryRootForInstanceType(projectPaths.GamePantry, value).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
		}
		Directory.CreateDirectory(projectPaths.ArtDefRoot);
		Directory.CreateDirectory(projectPaths.XLPRoot);
	}

	private void FixupProjectEnvironmentConfigs()
	{
		foreach (ProjectEnvironment item in AllProjectsMap.Projects.Where((ProjectEnvironment proj) => proj.Config == null && proj.Dependencies != null && proj.Dependencies.Any()))
		{
			string text = item.Dependencies.Last();
			if (!string.IsNullOrEmpty(text))
			{
				ProjectEnvironment project = null;
				if (AllProjectsMap.GetProject(text, ref project))
				{
					item.Config = project.Config;
					continue;
				}
				Outputs.WriteLine(OutputMessageType.Error, "Project '{0}' depends on Project '{1}', but Project '{1}' does not exist in the Project Map.  Please ensure the project appears in the AssetCloud.env file", item.Name, text);
			}
		}
	}

	private IGameArtSpecification GetPrimaryArtSpecification(string projectName, IEnumerable<IGameArtSpecification> artSpecifications)
	{
		IGameArtSpecification gameArtSpecification = null;
		if (artSpecifications.Any())
		{
			foreach (IGameArtSpecification artSpec in artSpecifications)
			{
				if (artSpecifications.Any(delegate(IGameArtSpecification spec)
				{
					IGameArtID iD = spec.ID;
					return !artSpec.RequiredGameArtIDs.Contains(iD);
				}))
				{
					gameArtSpecification = artSpec;
					break;
				}
			}
			if (gameArtSpecification == null)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Unable to determine the primary Art Specification for Project {0} due to cross-dependencies.", projectName);
				gameArtSpecification = artSpecifications.First();
			}
		}
		else
		{
			gameArtSpecification = Context.EnsureCreated<CivTechContext>().CreateInstance<IGameArtSpecification>();
		}
		return gameArtSpecification;
	}

	private IEnumerable<IGameArtSpecification> LoadGameArtSpecifications(ProjectPaths projectPaths)
	{
		string gamePantry = projectPaths.GamePantry;
		if (!Directory.Exists(gamePantry))
		{
			throw new FileNotFoundException("Failed to find pantry for project! Are you sure you selected the appropriate workspace during install?", gamePantry);
		}
		string[] array = Directory.GetFiles(gamePantry, "*.Art.xml", SearchOption.TopDirectoryOnly).ToArray();
		if (!array.Any())
		{
			Outputs.WriteLine(OutputMessageType.Error, "No art specification file found in folder \"{0}\"! BLP searching will not work.", gamePantry);
			return Enumerable.Empty<IGameArtSpecification>();
		}
		ICollection<IGameArtSpecification> collection = new List<IGameArtSpecification>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			IGameArtSpecification gameArtSpecification = m_civTechContext.CreateInstance<IGameArtSpecification>();
			if (!gameArtSpecification.DeserializeFromFile(text))
			{
				Outputs.WriteLine(OutputMessageType.Error, "Failed to deserialize art specification file \"{0}\"! BLP searching will not work.", text);
			}
			collection.Add(gameArtSpecification);
		}
		return collection;
	}

	private ProjectPaths NormalizeProjectPaths(IVersionControlService versionControl, ProjectInfo projectInfo)
	{
		string workspaceRoot = versionControl.WorkspaceRoot;
		char altDirectorySeparatorChar = Path.AltDirectorySeparatorChar;
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		ProjectPaths obj = new ProjectPaths
		{
			GamePantry = Path.Combine(workspaceRoot, projectInfo.Paths.Pantry).Replace(altDirectorySeparatorChar, directorySeparatorChar),
			ArtDev = Path.Combine(workspaceRoot, projectInfo.Paths.ArtDev).Replace(altDirectorySeparatorChar, directorySeparatorChar),
			LooseAssetRoot = Path.Combine(workspaceRoot, projectInfo.Paths.LooseAssetRoot).Replace(altDirectorySeparatorChar, directorySeparatorChar)
		};
		obj.ArtDefRoot = Path.Combine(obj.GamePantry, projectInfo.Paths.ArtDefRoot).Replace(altDirectorySeparatorChar, directorySeparatorChar);
		obj.ArtDefOutputRoot = Path.Combine(workspaceRoot, projectInfo.Paths.ArtDefOutputRoot).Replace(altDirectorySeparatorChar, directorySeparatorChar);
		obj.XLPRoot = Path.Combine(obj.GamePantry, projectInfo.Paths.XLPRoot).Replace(altDirectorySeparatorChar, directorySeparatorChar);
		obj.XLPOutputRoot = Path.Combine(workspaceRoot, projectInfo.Paths.XLPOutputRoot).Replace(altDirectorySeparatorChar, directorySeparatorChar);
		obj.GameDirectory = Path.Combine(workspaceRoot, projectInfo.Paths.GameFolder).Replace(altDirectorySeparatorChar, directorySeparatorChar);
		return obj;
	}

	private bool ProjectDependsOnProject(string containingProjName, string containedProjName)
	{
		ProjectEnvironment project = null;
		if (!AllProjectsMap.GetProject(containingProjName, ref project))
		{
			return false;
		}
		ProjectEnvironment project2 = null;
		if (!AllProjectsMap.GetProject(containedProjName, ref project2))
		{
			return false;
		}
		string containedProjectGUID = project2.PrimaryArtSpecification.ID.ID;
		if (project.PrimaryArtSpecification.RequiredGameArtIDs.Any((IGameArtID artID) => artID.ID == containedProjectGUID))
		{
			return true;
		}
		foreach (IGameArtID requiredGameArtID in project.PrimaryArtSpecification.RequiredGameArtIDs)
		{
			if (ProjectDependsOnProject(requiredGameArtID.Name, containedProjName))
			{
				return true;
			}
		}
		return false;
	}

	private void RemoveUnusedProjectsFromActiveMap()
	{
		string[] array = ActiveProjectMap.ProjectNames.ToArray();
		foreach (string text in array)
		{
			if (!(text == m_primaryProject.Name) && !ProjectDependsOnProject(m_primaryProject.Name, text))
			{
				ActiveProjectMap.RemoveProject(text);
			}
		}
	}

	private void SetupActiveProject(IAssetCloudSettings settings, IProjectConfig projCfg)
	{
		Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, "{0}: Started active project setup", m_projectSelectionService.ActiveProject);
		using (new ScopedStopwatch(m_projectSelectionService.ActiveProject + ": Finished active project setup. Setup took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			if (!m_projectSelectionService.Projects.ContainsProject(m_projectSelectionService.ActiveProject))
			{
				throw new FileNotFoundException("Failed to find active project!", m_projectSelectionService.ActiveProject);
			}
			ProjectInfo projectInfo = m_projectSelectionService.Projects[m_projectSelectionService.ActiveProject];
			if (!VersionControlSelectionService.VersionControlInfoMap.ContainsKey(projectInfo.Name))
			{
				throw new FileNotFoundException("Failed to find version control setup for active project!", m_projectSelectionService.ActiveProject);
			}
			if (!AllProjectsMap.ContainsProject(projectInfo.Name))
			{
				throw new FileNotFoundException("Failed to find active project in all projects list! Is it specified in the AssetCloud.env file?", projectInfo.Name);
			}
			ProjectEnvironment project = AllProjectsMap[projectInfo.Name];
			if (!ActiveProjectMap.ContainsProject(projectInfo.Name))
			{
				ActiveProjectMap.AddProject(project);
			}
			PrimaryProject = ActiveProjectMap[projectInfo.Name];
			AddDependentProjectsToActiveProjectMap(PrimaryProject.Dependencies);
		}
	}

	private void AddDependentProjectsToActiveProjectMap(IEnumerable<string> depProjNames)
	{
		foreach (string depProjName in depProjNames)
		{
			if (!ActiveProjectMap.ContainsProject(depProjName))
			{
				ActiveProjectMap.AddProject(AllProjectsMap[depProjName]);
			}
		}
	}

	private void SetupLayeredPantry()
	{
		IEnumerable<string> activePantryPaths = GetActivePantryPaths();
		LayeredPantry = m_civTechContext.CreateInstance<IVirtualPantry>(new object[1] { activePantryPaths });
	}

	private ProjectEnvironment SetupProject(IProjectMap projMap, IAssetCloudSettings settings, IProjectConfig projCfg, ProjectInfo project)
	{
		if (!projMap.ContainsProject(project.Name))
		{
			IVersionControlService versionControlService = VersionControlSelectionService[project.Name];
			ProjectPaths projectPaths = NormalizeProjectPaths(versionControlService, project);
			EnsureDirectoriesExist(projectPaths);
			IEnumerable<IGameArtSpecification> enumerable = LoadGameArtSpecifications(projectPaths);
			if (!enumerable.Any())
			{
				MessageBoxes.Show("Failed to load Art.xml for project " + project.Name + ". Assets from projects it depends on will not be accessible in the asset browser.", "Project Setup Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			IGameArtSpecification primaryArtSpecification = GetPrimaryArtSpecification(project.Name, enumerable);
			projMap.AddProject(new ProjectEnvironment(project, enumerable, primaryArtSpecification, projCfg, null, versionControlService, settings, projectPaths));
			if (primaryArtSpecification.ID.Name != project.Name)
			{
				m_projectSelectionService.Projects.AddAlternateKey(project.Name, primaryArtSpecification.ID.Name);
			}
		}
		return projMap[project.Name];
	}
}
