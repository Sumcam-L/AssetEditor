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
using Firaxis.Error;
using Firaxis.Threading;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IXLPRegistry))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class XLPRegistry : IXLPRegistry, IInitializable, IDisposable
{
	private class XLPCacheData : IXLPCacheData
	{
		private readonly string m_blpPackage;

		private readonly string m_entryName;

		private readonly string m_objectName;

		private readonly string m_xlpClassName;

		private readonly string m_xlpPath;

		public string XLPPath => m_xlpPath;

		public string BLPPackage => m_blpPackage;

		public string EntryName => m_entryName;

		public string ObjectName => m_objectName;

		public string XLPClassName => m_xlpClassName;

		public XLPCacheData(string xlpPath, string blpPackage, string entryName, string objectName, string xlpClassName)
		{
			m_xlpPath = xlpPath;
			m_blpPackage = blpPackage;
			m_entryName = entryName;
			m_objectName = objectName;
			m_xlpClassName = xlpClassName;
		}

		public XLPCacheData(string xlpPath, string blpPackage, IXLPEntry xlpEntry, string xlpClassName)
			: this(xlpPath, blpPackage, xlpEntry.ID, xlpEntry.ObjectName, xlpClassName)
		{
		}
	}

	private class XLPCollapser : IFlattenedXLP, IXLP, IAssemblyInstance, IDisposable, ISerializable, IVersionedData, IPathCollapser
	{
		private readonly IFlattenedXLP m_flattenedXLP;

		private readonly SortedSet<int> m_pantryIndices = new SortedSet<int>();

		private readonly string m_relativePath;

		private readonly IList<string> m_xlpPantries;

		public IEnumerable<Platforms> AllowedPlatforms => m_flattenedXLP.AllowedPlatforms;

		public string ClassName
		{
			get
			{
				return m_flattenedXLP.ClassName;
			}
			set
			{
				m_flattenedXLP.ClassName = value;
			}
		}

		public string Package
		{
			get
			{
				return m_flattenedXLP.Package;
			}
			set
			{
				m_flattenedXLP.Package = value;
			}
		}

		public string RelativePath => m_relativePath;

		public Version Version => m_flattenedXLP.Version;

		public IList<IXLPEntry> XLPEntries => m_flattenedXLP.XLPEntries;

		public XLPCollapser(IList<string> xlpPantries, string relativePath)
		{
			m_xlpPantries = xlpPantries;
			m_relativePath = relativePath;
			Action<string> errorAction = delegate(string error)
			{
				Outputs.WriteLine(OutputMessageType.Error, error);
				MessageBoxes.Show(error);
			};
			m_flattenedXLP = new FlattenedXLP(errorAction);
		}

		public IXLPEntry AddEntry(string ID, string objectName)
		{
			return m_flattenedXLP.AddEntry(ID, objectName);
		}

		public bool AddRootPantry(string pantry)
		{
			int num = m_xlpPantries.IndexOf(pantry);
			if (num >= 0)
			{
				return m_pantryIndices.Add(num);
			}
			return false;
		}

		public void AddXLP(IXLP xlp)
		{
			m_flattenedXLP.AddXLP(xlp);
		}

		public void AddXLPs(IEnumerable<IXLP> xlps)
		{
			m_flattenedXLP.AddXLPs(xlps);
		}

		public void AllowPlatform(Platforms ePlatform)
		{
			m_flattenedXLP.AllowPlatform(ePlatform);
		}

		public void ClearAllowedPlatforms()
		{
			m_flattenedXLP.ClearAllowedPlatforms();
		}

		public ResultCode DeserializeFromFile(string filename)
		{
			return m_flattenedXLP.DeserializeFromFile(filename);
		}

		public bool DeserializeFromXML(string xmlText)
		{
			return m_flattenedXLP.DeserializeFromXML(xmlText);
		}

		public void Dispose()
		{
			m_flattenedXLP.Dispose();
		}

		public IXLPEntry FindEntry(string ID)
		{
			return m_flattenedXLP.FindEntry(ID);
		}

		public bool IsPlatformAllowed(Platforms ePlatform)
		{
			return m_flattenedXLP.IsPlatformAllowed(ePlatform);
		}

		public void Reload()
		{
			foreach (int pantryIndex in m_pantryIndices)
			{
				string filename = Path.Combine(m_xlpPantries[pantryIndex], m_relativePath);
				IXLP iXLP = Context.EnsureCreated<CivTechContext>().CreateInstance<IXLP>();
				if ((bool)iXLP.DeserializeFromFile(filename))
				{
					m_flattenedXLP.AddXLP(iXLP);
				}
				else
				{
					iXLP.Dispose();
				}
			}
		}

		public void RemoveEntry(string ID)
		{
			m_flattenedXLP.RemoveEntry(ID);
		}

		public bool RemoveRootPantry(string pantry)
		{
			int num = m_xlpPantries.IndexOf(pantry);
			if (num >= 0)
			{
				return m_pantryIndices.Remove(num);
			}
			return false;
		}

		public void Reset()
		{
			m_flattenedXLP.Reset();
		}

		public bool SerializeIntoFile(string filename)
		{
			return m_flattenedXLP.SerializeIntoFile(filename);
		}

		public string SerializeIntoXML()
		{
			return m_flattenedXLP.SerializeIntoXML();
		}

		public void SetVersion(string versionString)
		{
			m_flattenedXLP.SetVersion(versionString);
		}

		public void SetVersion(int major, int minor, int build, int revision)
		{
			m_flattenedXLP.SetVersion(major, minor, build, revision);
		}
	}

	private bool m_disposedValue;

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService;

	private FileWatchThread m_fileWatchThread;

	private RegistryLoader<IXLP> m_xlpLoader;

	private object m_xlpLoadFinishLock = new object();

	private readonly ReaderWriterLockSlim m_cacheLock = new ReaderWriterLockSlim();

	private readonly ICivTechService m_civTechService;

	private readonly IDictionary<string, XLPCollapser> m_xlpCache = new Dictionary<string, XLPCollapser>(StringComparer.InvariantCultureIgnoreCase);

	private string PrimaryXLPPantry { get; set; }

	private IList<string> XLPPantries { get; set; }

	[ImportingConstructor]
	public XLPRegistry(ICivTechService civTechSvc)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			m_civTechService = civTechSvc;
			StartXLPLoad();
		}
	}

	public IXLPCacheData FindXLPData(string xlpEntryName)
	{
		using (new ScopedReaderLock(m_cacheLock))
		{
			foreach (KeyValuePair<string, XLPCollapser> item in m_xlpCache)
			{
				XLPCollapser value = item.Value;
				IXLPEntry iXLPEntry = value.FindEntry(xlpEntryName);
				if (iXLPEntry != null)
				{
					return new XLPCacheData(value.RelativePath, value.Package, iXLPEntry, value.ClassName);
				}
			}
		}
		return null;
	}

	public IXLPCacheData FindXLPEntryData(string relativeXLPPath, string xlpEntryName)
	{
		if (relativeXLPPath != null)
		{
			using (new ScopedReaderLock(m_cacheLock))
			{
				if (m_xlpCache.TryGetValue(relativeXLPPath, out var value))
				{
					IXLPEntry iXLPEntry = value.FindEntry(xlpEntryName);
					if (iXLPEntry != null)
					{
						return new XLPCacheData(value.RelativePath, value.Package, iXLPEntry, value.ClassName);
					}
				}
			}
		}
		return null;
	}

	public IEnumerable<IXLPCacheData> FindXLPEntryData(EntityID entity, string entityClassName)
	{
		IProjectConfig config = m_civTechService.PrimaryProject.Config;
		ICollection<IXLPCacheData> dataCollection = new List<IXLPCacheData>();
		using (new ScopedReaderLock(m_cacheLock))
		{
			Parallel.ForEach(m_xlpCache, delegate(KeyValuePair<string, XLPCollapser> pair)
			{
				IXLP value = pair.Value;
				foreach (IXLPEntry xLPEntry in value.XLPEntries)
				{
					if (xLPEntry.ObjectName == entity.Name && config.XLPClasses.GetClassByName(value.ClassName).AllowedEntityClasses.Contains(entityClassName))
					{
						XLPCacheData item = new XLPCacheData(string.Empty, string.Empty, xLPEntry, value.ClassName);
						lock (dataCollection)
						{
							dataCollection.Add(item);
						}
					}
				}
			});
		}
		return dataCollection;
	}

	public EntityID GetEntityID(string relativeXLPPath, string xlpEntryName)
	{
		if (relativeXLPPath != null)
		{
			using (new ScopedReaderLock(m_cacheLock))
			{
				if (m_xlpCache.TryGetValue(relativeXLPPath, out var value))
				{
					IXLPEntry iXLPEntry = value.FindEntry(xlpEntryName);
					if (iXLPEntry != null)
					{
						string objectName = iXLPEntry.ObjectName;
						string xlpClassName = value.ClassName;
						IXLPClass iXLPClass = m_civTechService.PrimaryProject.Config.XLPClasses.Items.FirstOrDefault((IXLPClass item) => item.Name == xlpClassName);
						if (iXLPClass != null)
						{
							return new EntityID(objectName, iXLPClass.InstanceType);
						}
					}
				}
			}
		}
		return new EntityID(string.Empty, InstanceType.IT_INVALID);
	}

	public EntityID GetEntityID(IXLPCacheData xlpEntryData)
	{
		if (xlpEntryData != null)
		{
			IXLPClass iXLPClass = m_civTechService.PrimaryProject.Config.XLPClasses.Items.FirstOrDefault((IXLPClass item) => item.Name == xlpEntryData.XLPClassName);
			if (iXLPClass != null)
			{
				return new EntityID(xlpEntryData.ObjectName, iXLPClass.InstanceType);
			}
		}
		return new EntityID(string.Empty, InstanceType.IT_INVALID);
	}

	public IDictionary<string, IEnumerable<IXLPCacheData>> GetXLPCacheData()
	{
		Dictionary<string, ICollection<IXLPCacheData>> dictionary = new Dictionary<string, ICollection<IXLPCacheData>>();
		using (new ScopedReaderLock(m_cacheLock))
		{
			foreach (KeyValuePair<string, XLPCollapser> item in m_xlpCache)
			{
				XLPCollapser value = item.Value;
				string relativePath = value.RelativePath;
				if (!string.IsNullOrEmpty(value.Package))
				{
					AddDataToCollection(relativePath, value, dictionary);
				}
			}
		}
		return ConvertDictionary(dictionary);
	}

	public IDictionary<string, IEnumerable<IXLPCacheData>> GetXLPCacheData(IEnumerable<string> packageNames)
	{
		Dictionary<string, ICollection<IXLPCacheData>> dictionary = new Dictionary<string, ICollection<IXLPCacheData>>();
		using (new ScopedReaderLock(m_cacheLock))
		{
			foreach (KeyValuePair<string, XLPCollapser> item in m_xlpCache)
			{
				XLPCollapser value = item.Value;
				string relativePath = value.RelativePath;
				string package = value.Package;
				if (!string.IsNullOrEmpty(package) && packageNames.Any((string packageName) => string.Equals(package, packageName, StringComparison.InvariantCultureIgnoreCase)))
				{
					AddDataToCollection(relativePath, value, dictionary);
				}
			}
		}
		return ConvertDictionary(dictionary);
	}

	public void Initialize()
	{
		if (!m_disposedValue)
		{
			FinishXLPLoad();
			if (m_scriptingService != null)
			{
				m_scriptingService.LoadAssembly(GetType().Assembly);
				m_scriptingService.SetVariable("xlpRegistry", this);
			}
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
		m_xlpLoader?.Dispose();
		m_xlpLoader = null;
		foreach (XLPCollapser value in m_xlpCache.Values)
		{
			((IDisposable)value).Dispose();
		}
		m_xlpCache.Clear();
	}

	public void HandleProjectChange()
	{
		StartXLPLoad();
		using (new ScopedWriterLock(m_cacheLock))
		{
			ClearXLPCache_Lockless();
		}
		FinishXLPLoad();
	}

	private void AddDataToCollection(string xlpPath, IXLP xlp, Dictionary<string, ICollection<IXLPCacheData>> dictionary)
	{
		string package = xlp.Package;
		if (!dictionary.TryGetValue(package, out var value))
		{
			ICollection<IXLPCacheData> collection = (dictionary[package] = new List<IXLPCacheData>());
			value = collection;
		}
		foreach (IXLPEntry xLPEntry in xlp.XLPEntries)
		{
			XLPCacheData item = new XLPCacheData(xlpPath, package, xLPEntry, xlp.ClassName);
			value.Add(item);
		}
	}

	private void AddXLPToCache(string fullPath)
	{
		string relativePath = GetRelativePath(fullPath);
		string pantry = fullPath.Replace(relativePath, "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		if (!m_xlpCache.TryGetValue(relativePath, out var value))
		{
			value = new XLPCollapser(XLPPantries, relativePath);
			m_xlpCache.Add(relativePath, value);
		}
		if (value.AddRootPantry(pantry))
		{
			value.Reset();
			value.Reload();
		}
		if (string.IsNullOrWhiteSpace(value.Package))
		{
			BugSubmitter.SilentAssert(!string.IsNullOrWhiteSpace(value.Package), "XLP with Relative Path {0} and Absolute Path {1} has an empty package name. File exists? {3} @assign matthew.kelley  @summary Assigning invalid package to XLP Registry.", relativePath, fullPath, File.Exists(fullPath));
		}
	}

	private IDictionary<string, IEnumerable<IXLPCacheData>> ConvertDictionary(IDictionary<string, ICollection<IXLPCacheData>> source)
	{
		Dictionary<string, IEnumerable<IXLPCacheData>> dictionary = new Dictionary<string, IEnumerable<IXLPCacheData>>();
		foreach (KeyValuePair<string, ICollection<IXLPCacheData>> item in source)
		{
			dictionary[item.Key] = item.Value;
		}
		return dictionary;
	}

	private void FinishXLPLoad()
	{
		if (m_xlpLoader != null)
		{
			RegistryLoader<IXLP> registryLoader = null;
			lock (m_xlpLoadFinishLock)
			{
				registryLoader = m_xlpLoader;
				m_xlpLoader = null;
			}
			registryLoader.Wait();
			PopulateXLPCache(XLPPantries, registryLoader.Result);
			Outputs.WriteLine(OutputMessageType.Info, "Populating XLP Registry took {0} ms.", registryLoader.LoadTimeInMS);
			registryLoader.Dispose();
			m_fileWatchThread = new FileWatchThread("XLP Registry File Watch");
			m_fileWatchThread.AddWatchPaths(XLPPantries, "*.xlp");
			m_fileWatchThread.FileChanged += HandleFileChanged;
			m_fileWatchThread.FileCreated += HandleFileCreated;
			m_fileWatchThread.FileDeleted += HandleFileDeleted;
			m_fileWatchThread.FileRenamed += HandleFileRenamed;
			m_fileWatchThread.Start();
		}
	}

	private IList<string> GetProjectXLPPantries()
	{
		IList<string> list = new List<string>();
		foreach (string item in m_civTechService.PrimaryProject.Dependencies.Reverse())
		{
			ProjectEnvironment project = null;
			if (m_civTechService.AllProjectsMap.GetProject(item, ref project))
			{
				string xLPRoot = project.Paths.XLPRoot;
				list.Add(xLPRoot.ToLower());
			}
		}
		list.Add(PrimaryXLPPantry.ToLower());
		return list;
	}

	private string GetRelativePath(string fullPath)
	{
		foreach (string xLPPantry in XLPPantries)
		{
			if (fullPath.StartsWith(xLPPantry, StringComparison.InvariantCultureIgnoreCase))
			{
				return GetRelativePath(xLPPantry, fullPath);
			}
		}
		BugSubmitter.SilentAssert(!Path.IsPathRooted(fullPath), "Rooted path \"{0}\" was not part of a valid pantry! @summary XLP path not part of a valid pantry @assign bwhitman", fullPath);
		return fullPath;
	}

	private string GetRelativePath(string xlpPantry, string fullPath)
	{
		return fullPath.ToLower().Replace(xlpPantry.ToLower(), "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	private void HandleFileChanged(object sender, FileChangedEventArgs e)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			UpdateXLPInCache(e.FilePath);
		}
	}

	private void HandleFileCreated(object sender, FileChangedEventArgs e)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			AddXLPToCache(e.FilePath);
		}
	}

	private void HandleFileDeleted(object sender, FileChangedEventArgs e)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			RemoveXLPFromCache(e.FilePath);
		}
	}

	private void HandleFileRenamed(object sender, FileRenamedEventArgs e)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			RemoveXLPFromCache(e.OldPath);
			AddXLPToCache(e.FilePath);
		}
	}

	private void PopulateXLPCache(IList<string> xlpPantryRoots, IDictionary<string, IXLP> fileResults)
	{
		using (new ScopedWriterLock(m_cacheLock))
		{
			ClearXLPCache_Lockless();
			foreach (string pantryRoot in xlpPantryRoots)
			{
				foreach (KeyValuePair<string, IXLP> item in (IEnumerable<KeyValuePair<string, IXLP>>)fileResults.Where(delegate(KeyValuePair<string, IXLP> x)
				{
					KeyValuePair<string, IXLP> keyValuePair = x;
					return keyValuePair.Key.StartsWith(pantryRoot, StringComparison.CurrentCultureIgnoreCase);
				}).OrderBy(delegate(KeyValuePair<string, IXLP> x)
				{
					KeyValuePair<string, IXLP> keyValuePair = x;
					return keyValuePair.Key;
				}).ToArray())
				{
					string relativePath = GetRelativePath(pantryRoot, item.Key);
					if (!m_xlpCache.TryGetValue(relativePath, out var value))
					{
						value = new XLPCollapser(xlpPantryRoots, relativePath);
						m_xlpCache.Add(relativePath, value);
					}
					value.AddRootPantry(pantryRoot);
					value.AddXLP(item.Value);
					if (string.IsNullOrWhiteSpace(value.Package))
					{
						BugSubmitter.SilentAssert(!string.IsNullOrWhiteSpace(value.Package), "XLP with Relative Path {0} and Absolute Path {1} has an empty package name. File exists? {3} @assign matthew.kelley  @summary Assigning invalid package to XLP Registry.", relativePath, item.Key, File.Exists(item.Key));
					}
				}
			}
		}
	}

	private void RemoveXLPFromCache(string fullPath)
	{
		string relativePath = GetRelativePath(fullPath);
		string pantry = fullPath.Replace(relativePath, "").TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		if (m_xlpCache.TryGetValue(relativePath, out var value) && value.RemoveRootPantry(pantry))
		{
			value.Reset();
			value.Reload();
		}
	}

	private void StartXLPLoad()
	{
		if (m_fileWatchThread != null)
		{
			m_fileWatchThread.Dispose();
			m_fileWatchThread = null;
		}
		if (m_xlpLoader != null)
		{
			m_xlpLoader.Dispose();
			m_xlpLoader = null;
		}
		PrimaryXLPPantry = m_civTechService.PrimaryProject.Paths.XLPRoot;
		XLPPantries = GetProjectXLPPantries();
		List<string> list = new List<string>();
		foreach (string xLPPantry in XLPPantries)
		{
			list.AddRange(Directory.GetFiles(xLPPantry, "*.xlp", SearchOption.AllDirectories));
		}
		Func<IXLP> factoryFunction = () => Context.EnsureCreated<CivTechContext>().CreateInstance<IXLP>();
		m_xlpLoader = new RegistryLoader<IXLP>(list, factoryFunction);
		m_xlpLoader.StartLoad();
	}

	private void UpdateXLPInCache(string fullPath)
	{
		string relativePath = GetRelativePath(fullPath);
		if (m_xlpCache.TryGetValue(relativePath, out var value))
		{
			value.Reset();
			value.Reload();
			if (string.IsNullOrWhiteSpace(value.Package))
			{
				BugSubmitter.SilentAssert(!string.IsNullOrWhiteSpace(value.Package), "XLP with Relative Path {0} and Absolute Path {1} has an empty package name. File exists? {3} @assign matthew.kelley @summary Assigning invalid package to XLP Registry.", relativePath, fullPath, File.Exists(fullPath));
			}
		}
	}

	private void ClearXLPCache_Lockless()
	{
		m_xlpCache.Values.ForEach(delegate(XLPCollapser ce)
		{
			ce?.Dispose();
		});
		m_xlpCache.Clear();
	}
}
