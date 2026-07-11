using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(IDisposable))]
[Export(typeof(ITemporaryArtProjectService))]
[Export(typeof(IProjectChangeWatcher))]
public class TemporaryArtProjectService : IInitializable, IDisposable, ITemporaryArtProjectService, ITemporaryArtOutputPaths, IProjectChangeWatcher
{
	private string _temporaryCookLocation = string.Empty;

	private string _temporaryPantryDirectory = string.Empty;

	private bool disposedValue;

	public bool SingleAssetCookEnabled { get; set; }

	public string ArtDefOutputDirectory => ProjectWorkspaceDictionary[CurrentProjectName].ArtDefOutputDirectory;

	public string BLPOutputDirectory => ProjectWorkspaceDictionary[CurrentProjectName].BLPOutputDirectory;

	public string TemporaryCookLocation => ProjectWorkspaceDictionary[CurrentProjectName].TemporaryCookLocation;

	public string TemporaryPantryDirectory => ProjectWorkspaceDictionary[CurrentProjectName].TemporaryPantryDirectory;

	private string TemporaryXLPPantry => ProjectWorkspaceDictionary[CurrentProjectName].TemporaryXLPPantry;

	private string TemporaryArtSpecLocation => ProjectWorkspaceDictionary[CurrentProjectName].TemporaryArtSpecLocation;

	private IGameArtSpecification TemporaryGameArtSpec => ProjectWorkspaceDictionary[CurrentProjectName].GameArtSpecification;

	private Dictionary<string, IXLP> XLPDictionary => ProjectWorkspaceDictionary[CurrentProjectName].TemporaryXLPDictionary;

	public string TemporaryCookLocationInternal
	{
		get
		{
			return _temporaryCookLocation;
		}
		set
		{
			if (!_temporaryCookLocation.Equals(value, StringComparison.CurrentCultureIgnoreCase) && IsValidPath(value))
			{
				_temporaryCookLocation = value;
				if (!Directory.Exists(_temporaryCookLocation))
				{
					Directory.CreateDirectory(_temporaryCookLocation);
				}
				ProjectWorkspaceDictionary.Values.ForEach(delegate(TemporaryArtWorkspace workspace)
				{
					workspace.TemporaryCookLocation = value;
				});
			}
		}
	}

	public string TemporaryPantryDirectoryInternal
	{
		get
		{
			return _temporaryPantryDirectory;
		}
		set
		{
			if (!_temporaryPantryDirectory.Equals(value, StringComparison.CurrentCultureIgnoreCase) && IsValidPath(value))
			{
				_temporaryPantryDirectory = value;
				if (!Directory.Exists(_temporaryPantryDirectory))
				{
					Directory.CreateDirectory(_temporaryPantryDirectory);
				}
				ProjectWorkspaceDictionary.Values.ForEach(delegate(TemporaryArtWorkspace workspace)
				{
					workspace.TemporaryPantryDirectory = value;
				});
			}
		}
	}

	private string CurrentProjectName => CivTechService.PrimaryProject.Name;

	private ICivTechService CivTechService { get; }

	private IEntityCacheService EntityCacheService { get; }

	private ISettingsService SettingsService { get; }

	private IXLPRegistry XLPRegistry { get; }

	private Dictionary<string, TemporaryArtWorkspace> ProjectWorkspaceDictionary { get; } = new Dictionary<string, TemporaryArtWorkspace>();

	[ImportingConstructor]
	public TemporaryArtProjectService(ICivTechService civTechService, ISettingsService settingsService, IXLPRegistry xlpRegistry, IEntityCacheService cacheService)
	{
		CivTechService = civTechService;
		SettingsService = settingsService;
		XLPRegistry = xlpRegistry;
		EntityCacheService = cacheService;
		TemporaryCookLocationInternal = Path.Combine(Path.GetTempPath(), "HotLoadData");
		TemporaryPantryDirectoryInternal = Path.Combine(Path.GetTempPath(), "Temporary_Civ6_Pantry");
		float num = TimedOperation.Do(delegate
		{
			ProjectEnvironment primaryProject = CivTechService.PrimaryProject;
			TemporaryArtWorkspace value = CreateTemporaryWorkspace(primaryProject);
			ProjectWorkspaceDictionary.Add(CurrentProjectName, value);
		});
		Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, GetType().Name + " construction took {0} seconds.", num);
	}

	public void Initialize()
	{
		BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => SingleAssetCookEnabled, "Enable Single Asset Cook".Localize(), "Single Asset Cook".Localize(), "When enabled, saving a file will cause a new BLP to be generated for the asset that was saved instead of recooking the entire BLP.");
		BoundPropertyDescriptor boundPropertyDescriptor2 = new BoundPropertyDescriptor(this, () => TemporaryCookLocationInternal, "Single Asset Cook Location".Localize(), "Single Asset Cook".Localize(), "Location to cook single asset BLPs.".Localize());
		BoundPropertyDescriptor boundPropertyDescriptor3 = new BoundPropertyDescriptor(this, () => TemporaryPantryDirectoryInternal, "Single Asset Cook Pantry Location".Localize(), "Single Asset Cook".Localize(), "Location to store single asset XLPs.".Localize());
		SettingsService.RegisterSettings("Tuner".Localize(), boundPropertyDescriptor);
		SettingsService.RegisterUserSettings("Tuner".Localize(), boundPropertyDescriptor);
		SettingsService.RegisterSettings("Tuner".Localize(), boundPropertyDescriptor2);
		SettingsService.RegisterUserSettings("Tuner".Localize(), boundPropertyDescriptor2);
		SettingsService.RegisterSettings("Tuner".Localize(), boundPropertyDescriptor3);
		SettingsService.RegisterUserSettings("Tuner".Localize(), boundPropertyDescriptor3);
	}

	private bool IsValidPath(string value)
	{
		if (Uri.TryCreate(value, UriKind.Absolute, out var result) && result != null)
		{
			return result.IsLoopback;
		}
		return false;
	}

	public IEnumerable<Uri> GetCookUris(IEnumerable<EntityID> changedEntities)
	{
		IEnumerable<EntityID> referencedRootEntities = GetReferencedRootEntities(changedEntities);
		return ReconcileTemporaryFiles(referencedRootEntities);
	}

	private IEnumerable<EntityID> GetReferencedRootEntities(IEnumerable<EntityID> changedEntities)
	{
		SortedSet<EntityID> sortedSet = new SortedSet<EntityID>();
		HashSet<string> hashSet = new HashSet<string>(new PathComparer());
		foreach (IWorkspaceDependencyRegistry activeDependencyRegistry in GetActiveDependencyRegistries())
		{
			if (activeDependencyRegistry == null)
			{
				BugSubmitter.SilentAssert(activeDependencyRegistry != null, "Accessing a null dependency registry in the active project stack.\n\nProjects:\n *{0}  @assign bwhitman @summary Null active dependency registry.", string.Join("\n *", GetActiveProjectNames()));
				continue;
			}
			foreach (EntityID changedEntity in changedEntities)
			{
				if (changedEntity == null)
				{
					BugSubmitter.SilentAssert(changedEntity != null, "Given a null root-changed entity.  This is not valid.  @assign bwhitman");
					continue;
				}
				string entityPath = CivTechService.GetEntityPath(changedEntity.Name, changedEntity.Type);
				if (!Uri.TryCreate(entityPath, UriKind.Absolute, out var result) || result == null)
				{
					BugSubmitter.SilentReport($"CivTechService.GetEntityPath({changedEntity.Name}, {changedEntity.Type}) returned {entityPath} @summary CivTechService.GetEntityPath returned an invalid Uri while attempting to hot load an entity @assign dgurley");
					continue;
				}
				DependencyTree dependentTree = activeDependencyRegistry.GetDependentTree(result);
				List<DependencyTree> list = new List<DependencyTree> { dependentTree };
				BugSubmitter.SilentAssert(dependentTree.Dependents.Any(), "Requested a hot load on an entity with no dependents found.  Entity: {0}.  @assign dgurley @summary hot_load_no_dependents", changedEntity.ToString());
				while (list.Count > 0)
				{
					int index = list.Count - 1;
					DependencyTree dependencyTree = list[index];
					list.RemoveAt(index);
					if (dependencyTree == null)
					{
						BugSubmitter.SilentAssert(dependencyTree != null, "GetDependentTree returned a null tree.  Skipping this tree.  @assign bwhitman");
						continue;
					}
					if (dependencyTree.Root == null)
					{
						BugSubmitter.SilentAssert(dependencyTree.Root != null, "GetDependentTree returned a tree without a root.  @assign bwhitman");
						continue;
					}
					list.AddRange(dependencyTree.Dependents);
					string localPath = dependencyTree.Root.LocalPath;
					if (hashSet.Add(localPath))
					{
						EntityID entityIDFromPath = StaticMethods.GetEntityIDFromPath(CivTechService.ProjectMapService, localPath);
						if (IsRootEntity(entityIDFromPath))
						{
							sortedSet.Add(entityIDFromPath);
						}
					}
				}
			}
		}
		return sortedSet;
	}

	private IEnumerable<IWorkspaceDependencyRegistry> GetActiveDependencyRegistries()
	{
		ProjectEnvironment primaryProject = CivTechService.PrimaryProject;
		yield return primaryProject.DependencyRegistry;
		foreach (string dependency in primaryProject.Dependencies)
		{
			ProjectEnvironment projectEnvironment = CivTechService.AllProjectsMap[dependency];
			yield return projectEnvironment.DependencyRegistry;
		}
	}

	private IEnumerable<string> GetActiveProjectNames()
	{
		ProjectEnvironment primaryProject = CivTechService.PrimaryProject;
		yield return primaryProject.Name;
		foreach (string dependency in primaryProject.Dependencies)
		{
			yield return dependency;
		}
	}

	private bool IsRootEntity(EntityID entity)
	{
		IEnumerable<IEntityCacheData> cacheData = EntityCacheService.GetCacheData(entity);
		IXLPClassSet xLPClasses = CivTechService.PrimaryProject.Config.XLPClasses;
		IEnumerable<string> allowedClassNames = xLPClasses.Items.Where((IXLPClass xlpClass) => xlpClass.InstanceType == entity.Type).SelectMany((IXLPClass xlpClass) => xlpClass.AllowedEntityClasses);
		return cacheData.Any((IEntityCacheData data) => allowedClassNames.Contains(data.Class));
	}

	private IEnumerable<Uri> ReconcileTemporaryFiles(IEnumerable<EntityID> entitiesToCook)
	{
		PopulateTemporaryXLPs(entitiesToCook);
		return ReconcileFilesOnDisk(entitiesToCook);
	}

	private void PopulateTemporaryXLPs(IEnumerable<EntityID> changedEntities)
	{
		foreach (EntityID changedEntity in changedEntities)
		{
			string entityClass = GetEntityClass(changedEntity);
			foreach (IXLPCacheData item in XLPRegistry.FindXLPEntryData(changedEntity, entityClass))
			{
				string xLPClassName = item.XLPClassName;
				string xlpPath = Path.Combine(TemporaryXLPPantry, xLPClassName + "_temp.xlp");
				IXLP xLP = GetXLP(xlpPath, xLPClassName);
				if (xLP.FindEntry(item.EntryName) == null)
				{
					xLP.AddEntry(item.EntryName, item.ObjectName);
				}
			}
		}
	}

	private IXLP GetXLP(string xlpPath, string xlpClass)
	{
		if (!XLPDictionary.TryGetValue(xlpPath, out var value))
		{
			value = Context.EnsureCreated<CivTechContext>().CreateInstance<IXLP>();
			if (File.Exists(xlpPath))
			{
				value.DeserializeFromFile(xlpPath);
			}
			else
			{
				value.ClassName = xlpClass;
				value.Package = string.Format("{0}_{1}", xlpClass, "temp");
				value.AllowPlatform(Platforms.PLATFORM_ALL);
			}
			XLPDictionary.Add(xlpPath, value);
		}
		return value;
	}

	private string GetEntityClass(EntityID entity)
	{
		IEntityCacheData entityCacheData = EntityCacheService.GetCacheData(entity).FirstOrDefault((IEntityCacheData data) => !string.IsNullOrEmpty(data.Class));
		if (entityCacheData == null)
		{
			return string.Empty;
		}
		return entityCacheData.Class;
	}

	private IEnumerable<Uri> ReconcileFilesOnDisk(IEnumerable<EntityID> changedEntities)
	{
		List<Uri> list = new List<Uri>();
		IXLPClassSet xLPClasses = CivTechService.PrimaryProject.Config.XLPClasses;
		IEnumerable<IGameArtSpecification> activeArtSpecs = GetActiveGameArtSpecifications().ToArray();
		IDictionary<string, IEnumerable<IXLPCacheData>> xLPCacheData = XLPRegistry.GetXLPCacheData();
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, IXLP> item in XLPDictionary)
		{
			IXLP value = item.Value;
			IXLPClass xlpClass = xLPClasses.GetClassByName(value.ClassName);
			if (xlpClass == null)
			{
				continue;
			}
			IEnumerable<string> validEntityNames = from entity in changedEntities
				where xlpClass.InstanceType == entity.Type
				select entity.Name;
			if (!value.XLPEntries.Any((IXLPEntry entry) => validEntityNames.Contains(entry.ObjectName)))
			{
				continue;
			}
			string key = item.Key;
			if (!value.SerializeIntoFile(key) || !Uri.TryCreate(key, UriKind.Absolute, out var result) || !(result != null))
			{
				continue;
			}
			list.Add(result);
			flag |= AddPackageToArtSpecLibrary(value, activeArtSpecs, xLPCacheData);
			stringBuilder.AppendFormat("Adding XLP '{0}' to the cook queue.", result.LocalPath);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("***** XLP Data *****");
			stringBuilder.AppendLine("XLP Package: " + value.Package);
			stringBuilder.AppendLine("XLP Class: " + value.ClassName);
			stringBuilder.AppendLine("***** XLP Entries *****");
			foreach (IXLPEntry xLPEntry in value.XLPEntries)
			{
				string text = "ID: " + xLPEntry.ID + "; Entity Name: " + xLPEntry.ObjectName;
				stringBuilder.AppendLine(" *  " + text);
			}
			stringBuilder.AppendLine("**********");
			stringBuilder.AppendLine();
		}
		if (flag)
		{
			TemporaryGameArtSpec.SerializeIntoFile(TemporaryArtSpecLocation);
		}
		if (File.Exists(TemporaryArtSpecLocation) && Uri.TryCreate(TemporaryArtSpecLocation, UriKind.Absolute, out var result2) && result2 != null)
		{
			list.Add(result2);
			stringBuilder.AppendFormat("Adding Art Specification '{0}' to the cook queue.", result2.LocalPath);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("***** Art Specification Data *****");
			stringBuilder.AppendLine("Art Specification Name: " + TemporaryGameArtSpec.ID.Name);
			stringBuilder.AppendLine("***** Art Specification Dependencies *****");
			foreach (IGameArtID requiredGameArtID in TemporaryGameArtSpec.RequiredGameArtIDs)
			{
				stringBuilder.AppendLine(" *  Depends on ArtSpec: " + requiredGameArtID.Name);
			}
			if (TemporaryGameArtSpec.ArtConsumers.Any())
			{
				stringBuilder.AppendLine("***** Art Specification Consumers *****");
				foreach (IArtConsumer artConsumer in TemporaryGameArtSpec.ArtConsumers)
				{
					stringBuilder.AppendLine(" *  Consumer Name: " + artConsumer.ConsumerName);
					stringBuilder.AppendLine(" ***** Library Dependencies *****");
					foreach (string referencedLibrary in artConsumer.ReferencedLibraries)
					{
						stringBuilder.AppendLine(" * * Library Name: " + referencedLibrary);
					}
				}
			}
			if (TemporaryGameArtSpec.GameLibraries.Any())
			{
				stringBuilder.AppendLine("***** Art Specification Libraries *****");
				foreach (IGameLibrary gameLibrary in TemporaryGameArtSpec.GameLibraries)
				{
					stringBuilder.AppendLine(" *  Library Name: " + gameLibrary.LibraryName);
				}
			}
			stringBuilder.AppendLine("**********");
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("***** Cooked Uris *****");
		foreach (Uri item2 in list)
		{
			stringBuilder.AppendLine(" *  " + item2.LocalPath);
		}
		Outputs.WriteLine(OutputMessageType.Diagnostic, stringBuilder.ToString());
		return list;
	}

	private bool AddPackageToArtSpecLibrary(IXLP xlp, IEnumerable<IGameArtSpecification> activeArtSpecs, IDictionary<string, IEnumerable<IXLPCacheData>> packageDataMap)
	{
		bool flag = false;
		foreach (string libraryName in GetLibraryNames(xlp, activeArtSpecs, packageDataMap))
		{
			IGameLibrary gameLibrary = TemporaryGameArtSpec.GameLibraries.FirstOrDefault((IGameLibrary lib) => lib.LibraryName == libraryName);
			if (gameLibrary == null)
			{
				gameLibrary = TemporaryGameArtSpec.AddGameLibrary(libraryName);
				flag = true;
			}
			flag |= gameLibrary.AddPath(xlp.Package);
		}
		return flag;
	}

	private IEnumerable<string> GetLibraryNames(IXLP xlp, IEnumerable<IGameArtSpecification> activeArtSpecs, IDictionary<string, IEnumerable<IXLPCacheData>> packageDataMap)
	{
		ISet<string> set = new HashSet<string>();
		foreach (IGameArtSpecification activeArtSpec in activeArtSpecs)
		{
			foreach (IGameLibrary gameLibrary in activeArtSpec.GameLibraries)
			{
				foreach (string relativePackagePath in gameLibrary.RelativePackagePaths)
				{
					if (packageDataMap.TryGetValue(relativePackagePath, out var value) && value.Any((IXLPCacheData data) => data.XLPClassName == xlp.ClassName))
					{
						set.Add(gameLibrary.LibraryName);
					}
				}
			}
		}
		return set;
	}

	private IEnumerable<IGameArtSpecification> GetActiveGameArtSpecifications()
	{
		foreach (IGameArtSpecification artSpecification in CivTechService.PrimaryProject.ArtSpecifications)
		{
			yield return artSpecification;
		}
		foreach (string dependency in CivTechService.PrimaryProject.Dependencies)
		{
			ProjectEnvironment projectEnvironment = CivTechService.AllProjectsMap[dependency];
			foreach (IGameArtSpecification artSpecification2 in projectEnvironment.ArtSpecifications)
			{
				yield return artSpecification2;
			}
		}
	}

	public IEnumerable<Uri> RemoveOverlappingTempXLPEntries(IXLP xlp)
	{
		ICollection<Uri> collection = new List<Uri>();
		ICollection<string> collection2 = new List<string>();
		foreach (KeyValuePair<string, IXLP> item in XLPDictionary)
		{
			string key = item.Key;
			IXLP value = item.Value;
			if (value.ClassName != xlp.ClassName)
			{
				continue;
			}
			foreach (IXLPEntry tempXLPEntry in value.XLPEntries)
			{
				if (xlp.XLPEntries.Any((IXLPEntry gameXLPEntry) => gameXLPEntry.ID == tempXLPEntry.ID))
				{
					collection2.Add(tempXLPEntry.ID);
				}
			}
			if (!collection2.Any())
			{
				continue;
			}
			foreach (string item2 in collection2)
			{
				value.RemoveEntry(item2);
			}
			if (Uri.TryCreate(key, UriKind.Absolute, out var result) && value.SerializeIntoFile(key))
			{
				collection.Add(result);
			}
			collection2.Clear();
		}
		return collection;
	}

	public void HandleProjectChange(Action<string> statusMessagePrinter)
	{
		if (!ProjectWorkspaceDictionary.ContainsKey(CurrentProjectName))
		{
			ProjectEnvironment primaryProject = CivTechService.PrimaryProject;
			TemporaryArtWorkspace value = CreateTemporaryWorkspace(primaryProject);
			ProjectWorkspaceDictionary.Add(CurrentProjectName, value);
		}
	}

	private TemporaryArtWorkspace CreateTemporaryWorkspace(ProjectEnvironment targetProject)
	{
		return new TemporaryArtWorkspace(targetProject, TemporaryCookLocationInternal, TemporaryPantryDirectoryInternal);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (disposing)
		{
			ProjectWorkspaceDictionary.Values.ForEach(delegate(TemporaryArtWorkspace workspace)
			{
				workspace.Dispose();
			});
			ProjectWorkspaceDictionary.Clear();
		}
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
