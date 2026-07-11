using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Threading;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IArtDefRegistry))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ArtDefRegistry : IArtDefRegistry, IInitializable, IDisposable
{
	private RegistryLoader<IArtDef> m_artDefLoader;

	private object m_artDefLoadFinishLock = new object();

	private IList<string> m_artDefPantries;

	private bool m_disposedValue;

	private FileWatchThread m_fileWatchThread;

	private readonly IDictionary<string, ArtDefCollapser> m_artDefCache = new Dictionary<string, ArtDefCollapser>(StringComparer.InvariantCultureIgnoreCase);

	private readonly ReaderWriterLockSlim m_cacheLock = new ReaderWriterLockSlim();

	private readonly ICivTechService m_civTechService;

	private readonly string[] m_emptyArray = new string[0];

	public string PrimaryArtDefPantry { get; private set; }

	[ImportingConstructor]
	public ArtDefRegistry(ICivTechService civTechSvc)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			m_civTechService = civTechSvc;
			StartArtDefLoad();
		}
	}

	public void HandleProjectChange()
	{
		StartArtDefLoad();
		FinishArtDefLoad();
	}

	private string GetArtDefRelativePath(IArtDef registeredArtDef)
	{
		foreach (KeyValuePair<string, ArtDefCollapser> item in m_artDefCache)
		{
			if (item.Value == registeredArtDef)
			{
				return item.Value.RelativePath;
			}
		}
		return string.Empty;
	}

	public IEnumerable<string> GetRelativeArtDefPaths()
	{
		using (new ScopedReaderLock(m_cacheLock))
		{
			return new List<string>(m_artDefCache.Keys);
		}
	}

	public string GetArtDefString(string relativePath)
	{
		using (new ScopedReaderLock(m_cacheLock))
		{
			if (m_artDefCache.TryGetValue(relativePath, out var value))
			{
				return value.ToString();
			}
			return string.Empty;
		}
	}

	public IArtDefCollection[] GetSuitableCollections(string templateName, string collectionName)
	{
		using (new ScopedReaderLock(m_cacheLock))
		{
			IEnumerable<ArtDefCollapser> enumerable = m_artDefCache.Values.Where((ArtDefCollapser artdef) => artdef.ArtDefTemplate == templateName);
			List<IArtDefCollection> list = new List<IArtDefCollection>();
			foreach (IArtDef item in (IEnumerable<IArtDef>)enumerable)
			{
				IArtDefCollection artDefCollection = item.FindArtDefCollectionRootFirst(collectionName);
				if (artDefCollection != null)
				{
					list.Add(artDefCollection);
				}
			}
			return list.ToArray();
		}
	}

	public string[] GetSuitableElements(string templateName, string collectionName)
	{
		using (new ScopedReaderLock(m_cacheLock))
		{
			IEnumerable<ArtDefCollapser> enumerable = m_artDefCache.Values.Where((ArtDefCollapser artdef) => artdef.ArtDefTemplate == templateName);
			SortedSet<string> sortedSet = new SortedSet<string>();
			foreach (IArtDef item in (IEnumerable<IArtDef>)enumerable)
			{
				IArtDefCollection artDefCollection = item.FindArtDefCollectionRootFirst(collectionName);
				if (artDefCollection != null)
				{
					IEnumerable<string> other = artDefCollection.Elements.Select((IArtDefElement element) => element.Name);
					sortedSet.UnionWith(other);
				}
			}
			return sortedSet.ToArray();
		}
	}

	public string[] GetSuitableCollections(IValue artDefRefVal, IParameter artDefRefParam)
	{
		using (new ScopedReaderLock(m_cacheLock))
		{
			IArtDefRefParameter artDefRefParameter = artDefRefParam as IArtDefRefParameter;
			IArtDefRefValue artDefRefValue = artDefRefVal as IArtDefRefValue;
			if (artDefRefParameter == null || artDefRefValue == null)
			{
				return new string[0];
			}
			if (artDefRefParameter.CollectionIsLocked)
			{
				return artDefRefParameter.AllowedCollectionNames.ToArray();
			}
			string artDefPath = string.Empty;
			IEnumerable<string> allowedTemplateNames = Enumerable.Empty<string>();
			GetParameters(artDefRefValue, artDefRefParameter, out artDefPath, out allowedTemplateNames);
			return GetSuitableCollections(artDefPath, allowedTemplateNames);
		}
	}

	private void GetParameters(IArtDefRefValue typedValue, IArtDefRefParameter typedParam, out string artDefPath, out IEnumerable<string> allowedTemplateNames)
	{
		if (typedParam.CollectionIsLocked)
		{
			if (typedParam.AllowedArtDefTemplateNames.Any())
			{
				artDefPath = string.Empty;
			}
			else
			{
				artDefPath = typedParam.DefaultArtDefPath;
			}
			allowedTemplateNames = typedParam.AllowedArtDefTemplateNames;
			return;
		}
		if (!string.IsNullOrEmpty(typedValue.TemplateName))
		{
			allowedTemplateNames = new string[1] { typedValue.TemplateName };
		}
		else
		{
			allowedTemplateNames = typedParam.AllowedArtDefTemplateNames;
		}
		if (!string.IsNullOrEmpty(typedValue.ArtDefPath))
		{
			artDefPath = typedValue.ArtDefPath;
		}
		else
		{
			artDefPath = typedParam.DefaultArtDefPath;
		}
	}

	private string[] GetSuitableCollections(string artDefPath, IEnumerable<string> templateNames)
	{
		if (!string.IsNullOrEmpty(artDefPath))
		{
			if (templateNames.Any())
			{
				return GetCollections(artDefPath, templateNames);
			}
			return GetCollections(artDefPath);
		}
		if (templateNames.Any())
		{
			return GetCollections(templateNames);
		}
		return GetAllCollections();
	}

	private string[] GetCollections(string artDefPath, IEnumerable<string> templateNames)
	{
		if (m_artDefCache.TryGetValue(artDefPath, out var value) && templateNames.Contains(value.ArtDefTemplate))
		{
			return GetCollections(value);
		}
		return m_emptyArray;
	}

	private string[] GetCollections(string artDefPath)
	{
		if (m_artDefCache.TryGetValue(artDefPath, out var value))
		{
			return GetCollections(value);
		}
		return m_emptyArray;
	}

	private string[] GetCollections(IArtDef artDef)
	{
		ISet<string> artDefCols = new HashSet<string>();
		artDef.VisitAllCollections(delegate(IArtDefCollection col)
		{
			artDefCols.Add(col.CollectionName);
		});
		return artDefCols.ToArray();
	}

	private string[] GetCollections(IEnumerable<string> allowedTemplateNames)
	{
		IEnumerable<IArtDef> enumerable = m_artDefCache.Values.Where((ArtDefCollapser artDef) => allowedTemplateNames.Contains(artDef.ArtDefTemplate));
		if (!enumerable.Any())
		{
			return m_emptyArray;
		}
		return GetCollections(enumerable);
	}

	private string[] GetAllCollections()
	{
		return GetCollections(m_artDefCache.Values);
	}

	private string[] GetCollections(IEnumerable<IArtDef> artDefs)
	{
		ISet<string> artDefCols = new HashSet<string>();
		foreach (IArtDef artDef in artDefs)
		{
			artDef.VisitAllCollections(delegate(IArtDefCollection col)
			{
				artDefCols.Add(col.CollectionName);
			});
		}
		return artDefCols.ToArray();
	}

	public string[] GetSuitableElements(IValue artDefRefVal, IParameter artDefRefParam)
	{
		using (new ScopedReaderLock(m_cacheLock))
		{
			IArtDefRefParameter artDefRefParameter = artDefRefParam as IArtDefRefParameter;
			IArtDefRefValue artDefRefValue = artDefRefVal as IArtDefRefValue;
			if (artDefRefParameter == null || artDefRefValue == null)
			{
				return new string[0];
			}
			string artDefPath = string.Empty;
			IEnumerable<string> allowedCollectionNames = Enumerable.Empty<string>();
			IEnumerable<string> allowedTemplateNames = Enumerable.Empty<string>();
			GetParameters(artDefRefValue, artDefRefParameter, out artDefPath, out allowedCollectionNames, out allowedTemplateNames);
			return GetSuitableElements(artDefPath, allowedCollectionNames, allowedTemplateNames);
		}
	}

	private void GetParameters(IArtDefRefValue typedValue, IArtDefRefParameter typedParam, out string artDefPath, out IEnumerable<string> allowedCollectionNames, out IEnumerable<string> allowedTemplateNames)
	{
		if (typedParam.CollectionIsLocked)
		{
			if (typedParam.AllowedArtDefTemplateNames.Any())
			{
				artDefPath = string.Empty;
			}
			else
			{
				artDefPath = typedParam.DefaultArtDefPath;
			}
			allowedCollectionNames = typedParam.AllowedCollectionNames;
			allowedTemplateNames = typedParam.AllowedArtDefTemplateNames;
			return;
		}
		if (!string.IsNullOrEmpty(typedValue.RootCollectionName))
		{
			allowedCollectionNames = new string[1] { typedValue.RootCollectionName };
		}
		else
		{
			allowedCollectionNames = typedParam.AllowedCollectionNames;
		}
		if (!string.IsNullOrEmpty(typedValue.TemplateName))
		{
			allowedTemplateNames = new string[1] { typedValue.TemplateName };
		}
		else
		{
			allowedTemplateNames = typedParam.AllowedArtDefTemplateNames;
		}
		if (!string.IsNullOrEmpty(typedValue.ArtDefPath))
		{
			artDefPath = typedValue.ArtDefPath;
		}
		else
		{
			artDefPath = typedParam.DefaultArtDefPath;
		}
	}

	private string[] GetSuitableElements(string artDefPath, IEnumerable<string> allowedCollectionNames, IEnumerable<string> allowedTemplateNames)
	{
		if (!string.IsNullOrEmpty(artDefPath))
		{
			if (allowedCollectionNames.Any())
			{
				if (allowedTemplateNames.Any())
				{
					return GetElements(artDefPath, allowedCollectionNames, allowedTemplateNames);
				}
				return GetElementsUsingCollectionNames(artDefPath, allowedCollectionNames);
			}
			if (allowedTemplateNames.Any())
			{
				return GetElementsUsingTemplateNames(artDefPath, allowedTemplateNames);
			}
			return GetElements(artDefPath);
		}
		if (allowedCollectionNames.Any())
		{
			if (allowedTemplateNames.Any())
			{
				return GetElements(allowedCollectionNames, allowedTemplateNames);
			}
			return GetElementsUsingCollectionNames(allowedCollectionNames);
		}
		if (allowedTemplateNames.Any())
		{
			return GetElementsUsingTemplateNames(allowedTemplateNames);
		}
		return GetAllElements();
	}

	private string[] GetElements(string artDefPath, IEnumerable<string> collectionNames, IEnumerable<string> templateNames)
	{
		if (m_artDefCache.TryGetValue(artDefPath, out var value) && templateNames.Contains(value.ArtDefTemplate))
		{
			return GetElementsUsingCollectionNames(value, collectionNames);
		}
		return m_emptyArray;
	}

	private string[] GetElementsUsingCollectionNames(string artDefPath, IEnumerable<string> collectionNames)
	{
		if (m_artDefCache.TryGetValue(artDefPath, out var value))
		{
			return GetElementsUsingCollectionNames(value, collectionNames);
		}
		return m_emptyArray;
	}

	private string[] GetElementsUsingCollectionNames(IArtDef artDef, IEnumerable<string> collectionNames)
	{
		ISet<string> set = new HashSet<string>();
		AddElementsToCollection(artDef, collectionNames, set);
		return set.ToArray();
	}

	private string[] GetElementsUsingCollectionNames(IEnumerable<string> collectionNames)
	{
		return GetElements(m_artDefCache.Values, collectionNames);
	}

	private string[] GetElementsUsingTemplateNames(string artDefPath, IEnumerable<string> templateNames)
	{
		if (m_artDefCache.TryGetValue(artDefPath, out var value) && templateNames.Contains(value.ArtDefTemplate))
		{
			return GetElements(value);
		}
		return m_emptyArray;
	}

	private string[] GetElementsUsingTemplateNames(IEnumerable<string> templateNames)
	{
		IEnumerable<IArtDef> suitableArtDefs = GetSuitableArtDefs(templateNames);
		if (!suitableArtDefs.Any())
		{
			return m_emptyArray;
		}
		return GetElements(suitableArtDefs);
	}

	private string[] GetElements(IEnumerable<string> collectionNames, IEnumerable<string> templateNames)
	{
		IEnumerable<IArtDef> suitableArtDefs = GetSuitableArtDefs(templateNames);
		if (!suitableArtDefs.Any())
		{
			return m_emptyArray;
		}
		return GetElements(suitableArtDefs, collectionNames);
	}

	private string[] GetElements(string artDefPath)
	{
		if (m_artDefCache.TryGetValue(artDefPath, out var value))
		{
			return GetElements(value);
		}
		return m_emptyArray;
	}

	private string[] GetAllElements()
	{
		return GetElements(m_artDefCache.Values);
	}

	private string[] GetElements(IArtDef artDef)
	{
		ISet<string> set = new HashSet<string>();
		AddElementsToCollection(artDef, set);
		return set.ToArray();
	}

	private string[] GetElements(IEnumerable<IArtDef> artDefs)
	{
		ISet<string> set = new HashSet<string>();
		foreach (IArtDef artDef in artDefs)
		{
			AddElementsToCollection(artDef, set);
		}
		return set.ToArray();
	}

	private string[] GetElements(IEnumerable<IArtDef> artDefs, IEnumerable<string> collectionNames)
	{
		ISet<string> set = new HashSet<string>();
		foreach (IArtDef artDef in artDefs)
		{
			AddElementsToCollection(artDef, collectionNames, set);
		}
		return set.ToArray();
	}

	private void AddElementsToCollection(IArtDef artDef, IEnumerable<string> collectionNames, ICollection<string> artDefElements)
	{
		foreach (IArtDefCollection rootCollection in artDef.RootCollections)
		{
			if (!collectionNames.Contains(rootCollection.CollectionName))
			{
				continue;
			}
			foreach (IArtDefElement element in rootCollection.Elements)
			{
				artDefElements.Add(element.Name);
			}
		}
	}

	private void AddElementsToCollection(IArtDef artDef, ICollection<string> artDefElements)
	{
		artDef.VisitAllElements(delegate(IArtDefElement elem)
		{
			artDefElements.Add(elem.Name);
		});
	}

	public ArtDefReferenceInfo GetArtDefInfo(string elementName, IValue artDefRefValue, IParameter artDefRefParam)
	{
		using (new ScopedReaderLock(m_cacheLock))
		{
			IArtDefRefValue artDefRefValue2 = artDefRefValue as IArtDefRefValue;
			IArtDefRefParameter artDefRefParameter = artDefRefParam as IArtDefRefParameter;
			if (string.IsNullOrEmpty(elementName) || artDefRefValue2 == null || artDefRefParameter == null)
			{
				return default(ArtDefReferenceInfo);
			}
			string artDefPath = string.Empty;
			IEnumerable<string> allowedCollectionNames = Enumerable.Empty<string>();
			IEnumerable<string> allowedTemplateNames = Enumerable.Empty<string>();
			GetParameters(artDefRefValue2, artDefRefParameter, out artDefPath, out allowedCollectionNames, out allowedTemplateNames);
			ArtDefReferenceInfo result = new ArtDefReferenceInfo
			{
				elementName = elementName,
				isCollectionLocked = artDefRefParameter.CollectionIsLocked
			};
			if (!string.IsNullOrEmpty(artDefPath))
			{
				if (m_artDefCache.TryGetValue(artDefPath, out var value))
				{
					Func<IArtDefCollection, bool> predicate = (IArtDefCollection collection) => allowedCollectionNames.Contains(collection.CollectionName) && collection.GetAllElements().Any((IArtDefElement element) => element.Name == elementName);
					result.artDefPath = value.RelativePath;
					result.templateName = value.ArtDefTemplate;
					IArtDefCollection artDefCollection = value.GetAllCollections().FirstOrDefault(predicate);
					if (artDefCollection != null)
					{
						result.collectionName = artDefCollection.CollectionName;
					}
				}
			}
			else
			{
				string collectionName = string.Empty;
				string templateName = string.Empty;
				FindReferredValue(elementName, allowedCollectionNames, allowedTemplateNames, out artDefPath, out collectionName, out templateName);
				result.artDefPath = artDefPath;
				result.collectionName = collectionName;
				result.templateName = templateName;
			}
			if (string.IsNullOrEmpty(result.collectionName) && allowedCollectionNames.Count() == 1)
			{
				result.collectionName = allowedCollectionNames.First();
			}
			if (string.IsNullOrEmpty(result.templateName) && allowedTemplateNames.Count() == 1)
			{
				result.templateName = allowedTemplateNames.First();
			}
			return result;
		}
	}

	private void FindReferredValue(string elementName, IEnumerable<string> collectionNames, IEnumerable<string> templateNames, out string artDefPath, out string collectionName, out string templateName)
	{
		artDefPath = string.Empty;
		collectionName = string.Empty;
		templateName = string.Empty;
		string scopedCollectionName = string.Empty;
		string scopedTemplateName = string.Empty;
		foreach (IArtDef artDef in GetSuitableArtDefs(templateNames))
		{
			artDef.VisitAllCollections(delegate(IArtDefCollection collection)
			{
				if (string.IsNullOrEmpty(scopedCollectionName) && string.IsNullOrEmpty(scopedTemplateName) && collectionNames.Contains(collection.CollectionName))
				{
					collection.VisitAllElements(delegate(IArtDefElement element)
					{
						if (string.IsNullOrEmpty(scopedCollectionName) && string.IsNullOrEmpty(scopedTemplateName) && element.Name == elementName)
						{
							scopedCollectionName = collection.CollectionName;
							scopedTemplateName = artDef.ArtDefTemplate;
						}
					});
				}
			});
			if (!string.IsNullOrEmpty(scopedCollectionName) && !string.IsNullOrEmpty(scopedTemplateName))
			{
				collectionName = scopedCollectionName;
				templateName = scopedTemplateName;
				artDefPath = GetArtDefRelativePath(artDef);
				break;
			}
		}
	}

	private IEnumerable<IArtDef> GetSuitableArtDefs(IEnumerable<string> allowedTemplateNames)
	{
		if (allowedTemplateNames.Any())
		{
			return m_artDefCache.Values.Where((ArtDefCollapser artDef) => allowedTemplateNames.Contains(artDef.ArtDefTemplate));
		}
		return m_artDefCache.Values;
	}

	public IEnumerable<IXLPEntry> GetOrphanedEntries(IXLP xlp)
	{
		List<IXLPEntry> list = new List<IXLPEntry>();
		using (new ScopedReaderLock(m_cacheLock))
		{
			foreach (IXLPEntry xLPEntry in xlp.XLPEntries)
			{
				if (IsEntryOrphanedImpl(xlp, xLPEntry, null))
				{
					list.Add(xLPEntry);
				}
			}
			return list;
		}
	}

	public bool IsEntryOrphaned(IXLP xlp, IXLPEntry xlpEntry, ICollection<IArtDef> referrers)
	{
		bool flag = false;
		using (new ScopedReaderLock(m_cacheLock))
		{
			return IsEntryOrphanedImpl(xlp, xlpEntry, referrers);
		}
	}

	private bool IsEntryOrphanedImpl(IXLP xlp, IXLPEntry xlpEntry, ICollection<IArtDef> referrers)
	{
		bool found = false;
		Action<IArtDef, IValue> valueVisitor = delegate(IArtDef artdef, IValue value)
		{
			if (!found && value.ParameterType == Firaxis.CivTech.AssetObjects.ValueType.VT_BLP_ENTRY)
			{
				IBLPEntryValue blpEntryValue = (IBLPEntryValue)value;
				if (IsEntryFromXLP(xlp, xlpEntry, blpEntryValue))
				{
					referrers?.Add(artdef);
					found = true;
				}
			}
		};
		Parallel.ForEach(m_artDefCache.Values, delegate(ArtDefCollapser artdef)
		{
			if (!found)
			{
				artdef.VisitAllValues(delegate(IValue val)
				{
					valueVisitor(artdef, val);
				});
			}
		});
		return !found;
	}

	private bool IsEntryFromXLP(IXLP xlp, IXLPEntry xlpEntry, IBLPEntryValue blpEntryValue)
	{
		if (blpEntryValue.BLPPackage == xlp.Package && blpEntryValue.XLPClass == xlp.ClassName)
		{
			return blpEntryValue.EntryName == xlpEntry.ID;
		}
		return false;
	}

	public void Initialize()
	{
		if (!m_disposedValue)
		{
			FinishArtDefLoad();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposedValue || !disposing)
		{
			return;
		}
		m_disposedValue = true;
		if (m_fileWatchThread != null)
		{
			m_fileWatchThread.FileChanged -= HandleFileChanged;
			m_fileWatchThread.FileCreated -= HandleFileCreated;
			m_fileWatchThread.FileDeleted -= HandleFileDeleted;
			m_fileWatchThread.FileRenamed -= HandleFileRenamed;
			m_fileWatchThread.Dispose();
			m_fileWatchThread = null;
		}
		m_artDefLoader?.Dispose();
		m_artDefLoader = null;
		foreach (ArtDefCollapser value in m_artDefCache.Values)
		{
			((IDisposable)value).Dispose();
		}
		m_artDefCache.Clear();
	}

	private void HandleFileChanged(object sender, FileChangedEventArgs e)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			UpdateArtDefInCache(e.FilePath);
		}
	}

	private void HandleFileCreated(object sender, FileChangedEventArgs e)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			AddArtDefToCache(e.FilePath);
		}
	}

	private void HandleFileDeleted(object sender, FileChangedEventArgs e)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			RemoveArtDefFromCache(e.FilePath);
		}
	}

	private void HandleFileRenamed(object sender, FileRenamedEventArgs e)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			RemoveArtDefFromCache(e.OldPath);
			AddArtDefToCache(e.FilePath);
		}
	}

	private void AddArtDefToCache(string fullPath)
	{
		string relativePath = GetRelativePath(fullPath);
		string pantry = fullPath.Replace(relativePath, "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		if (!m_artDefCache.TryGetValue(relativePath, out var value))
		{
			value = new ArtDefCollapser(m_artDefPantries, relativePath, m_civTechService.PrimaryProject.Config);
			m_artDefCache.Add(relativePath, value);
		}
		if (value.AddRootPantry(pantry))
		{
			value.Reset();
			value.Reload();
		}
	}

	private void FinishArtDefLoad()
	{
		if (m_artDefLoader != null)
		{
			RegistryLoader<IArtDef> registryLoader = null;
			lock (m_artDefLoadFinishLock)
			{
				registryLoader = m_artDefLoader;
				m_artDefLoader = null;
			}
			registryLoader.Wait();
			PopulateArtDefCache(m_artDefPantries, registryLoader.Result);
			Outputs.WriteLine(OutputMessageType.Info, "Populating ArtDef Registry took {0} ms.", registryLoader.LoadTimeInMS);
			registryLoader.Dispose();
			m_fileWatchThread = new FileWatchThread("ArtDef Registry File Watch");
			m_fileWatchThread.AddWatchPaths(m_artDefPantries, "*.artdef");
			m_fileWatchThread.FileChanged += HandleFileChanged;
			m_fileWatchThread.FileCreated += HandleFileCreated;
			m_fileWatchThread.FileDeleted += HandleFileDeleted;
			m_fileWatchThread.FileRenamed += HandleFileRenamed;
			m_fileWatchThread.Start();
		}
	}

	private IList<string> GetProjectArtDefPantries()
	{
		IList<string> list = new List<string>();
		foreach (string item in m_civTechService.PrimaryProject.Dependencies.Reverse())
		{
			ProjectEnvironment project = null;
			if (m_civTechService.AllProjectsMap.GetProject(item, ref project))
			{
				string artDefRoot = project.Paths.ArtDefRoot;
				list.Add(artDefRoot);
			}
		}
		list.Add(PrimaryArtDefPantry);
		return list;
	}

	private string GetRelativePath(string fullPath)
	{
		foreach (string artDefPantry in m_artDefPantries)
		{
			if (fullPath.StartsWith(artDefPantry, StringComparison.InvariantCultureIgnoreCase))
			{
				return GetRelativePath(artDefPantry, fullPath);
			}
		}
		BugSubmitter.SilentAssert(!Path.IsPathRooted(fullPath), "Rooted path \"{0}\" was not part of a valid pantry! @summary ArtDef path not part of a valid pantry @assign bwhitman", fullPath);
		return fullPath;
	}

	private string GetRelativePath(string artDefPantry, string fullPath)
	{
		string text = fullPath;
		if (fullPath.StartsWith(artDefPantry, StringComparison.CurrentCultureIgnoreCase))
		{
			text = text.Substring(artDefPantry.Count());
		}
		text = text.Replace(artDefPantry, "");
		return text.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	private void PopulateArtDefCache(IEnumerable<string> artDefPantryRoots, IDictionary<string, IArtDef> fileResults)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			m_artDefCache.Values.ForEach(delegate(ArtDefCollapser ce)
			{
				ce?.Dispose();
			});
			m_artDefCache.Clear();
			foreach (string pantryRoot in artDefPantryRoots)
			{
				foreach (KeyValuePair<string, IArtDef> item in (IEnumerable<KeyValuePair<string, IArtDef>>)fileResults.Where(delegate(KeyValuePair<string, IArtDef> x)
				{
					KeyValuePair<string, IArtDef> keyValuePair = x;
					return PathCompareHelper.StartsWith(keyValuePair.Key, pantryRoot, bIgnoreCase: true);
				}).OrderBy(delegate(KeyValuePair<string, IArtDef> x)
				{
					KeyValuePair<string, IArtDef> keyValuePair = x;
					return keyValuePair.Key;
				}).ToArray())
				{
					string relativePath = GetRelativePath(pantryRoot, item.Key);
					if (!m_artDefCache.TryGetValue(relativePath, out var value))
					{
						value = new ArtDefCollapser(m_artDefPantries, relativePath, m_civTechService.PrimaryProject.Config);
						m_artDefCache.Add(relativePath, value);
					}
					BugSubmitter.SilentAssert(value.AddRootPantry(pantryRoot), "Invalid pantry root {0} for ArtDef {1} @summary Encountered invalid pantry root while populating ArtDef cache @assign bwhitman", pantryRoot, relativePath);
					IArtDef value2 = item.Value;
					BugSubmitter.SilentAssert(string.IsNullOrEmpty(value.ArtDefTemplate) || value.ArtDefTemplate == value2.ArtDefTemplate, "Mismatched ArtDef template with ArtDef {0} of template {1} and ArtDefCollapser {2} of template {3} @summary Mismatched ArtDef templates encountered while populating ArtDef cache @assign bwhitman", item.Key, value2.ArtDefTemplate, relativePath, value.ArtDefTemplate);
					value.AddArtDef(value2);
				}
			}
		}
	}

	private void RemoveArtDefFromCache(string fullPath)
	{
		string relativePath = GetRelativePath(fullPath);
		string pantry = fullPath.Replace(relativePath, "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		if (m_artDefCache.TryGetValue(relativePath, out var value) && value.RemoveRootPantry(pantry))
		{
			value.Reset();
			value.Reload();
		}
	}

	private void StartArtDefLoad()
	{
		if (m_fileWatchThread != null)
		{
			m_fileWatchThread.Dispose();
			m_fileWatchThread = null;
		}
		if (m_artDefLoader != null)
		{
			m_artDefLoader.Dispose();
			m_artDefLoader = null;
		}
		PrimaryArtDefPantry = m_civTechService.PrimaryProject.Paths.ArtDefRoot;
		m_artDefPantries = GetProjectArtDefPantries();
		List<string> list = new List<string>();
		foreach (string artDefPantry in m_artDefPantries)
		{
			list.AddRange(Directory.GetFiles(artDefPantry, "*.artdef", SearchOption.AllDirectories));
		}
		Func<IArtDef> factoryFunction = () => Context.EnsureCreated<CivTechContext>().CreateInstance<IArtDef>(new object[1] { m_civTechService.PrimaryProject.Config });
		m_artDefLoader = new RegistryLoader<IArtDef>(list, factoryFunction);
		m_artDefLoader.StartLoad();
	}

	private void UpdateArtDefInCache(string fullPath)
	{
		string relativePath = GetRelativePath(fullPath);
		if (m_artDefCache.TryGetValue(relativePath, out var value))
		{
			value.Reset();
			value.Reload();
		}
	}
}
