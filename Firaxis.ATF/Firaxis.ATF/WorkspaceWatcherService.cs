using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(ITunerQueueService))]
[Export(typeof(ISequencedProjectChangeWatcher))]
[Export(typeof(WorkspaceWatcherService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class WorkspaceWatcherService : ITunerQueueService, IInitializable, ISequencedProjectChangeWatcher, IDisposable
{
	private class HotloadableInfo : IHotloadable
	{
		public string SubSystem { get; private set; }

		public IEnumerable<string> ConsumerNames { get; private set; }

		public Uri Uri { get; private set; }

		public HotloadableInfo(string sys, IEnumerable<string> consumers, Uri uri)
		{
			SubSystem = sys;
			ConsumerNames = consumers;
			Uri = uri;
		}

		public override int GetHashCode()
		{
			return Uri.GetHashCode();
		}

		public override string ToString()
		{
			return Uri.ToString();
		}
	}

	private interface ICookableInfo : IComparable, IComparable<ICookableInfo>, IEquatable<ICookableInfo>
	{
		Uri Uri { get; }
	}

	private class CookableInfo : ICookableInfo, IComparable, IComparable<ICookableInfo>, IEquatable<ICookableInfo>
	{
		public Uri Uri { get; private set; }

		public CookableInfo(Uri u)
		{
			Uri = u;
		}

		public int CompareTo(object obj)
		{
			if (obj is ICookableInfo other)
			{
				return CompareTo(other);
			}
			return 1;
		}

		public int CompareTo(ICookableInfo other)
		{
			return Uri.Compare(Uri, other.Uri, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.CurrentCultureIgnoreCase);
		}

		public bool Equals(ICookableInfo other)
		{
			return Uri.Equals(other);
		}

		public override int GetHashCode()
		{
			return Uri.GetHashCode();
		}

		public override string ToString()
		{
			return Uri.ToString();
		}
	}

	private class HotLoadableAndCookableInfo : ICookableInfo, IComparable, IComparable<ICookableInfo>, IEquatable<ICookableInfo>, IHotloadable
	{
		private IHotloadable _hotloadable;

		public Uri Uri { get; private set; }

		public string SubSystem => _hotloadable.SubSystem;

		public IEnumerable<string> ConsumerNames => _hotloadable.ConsumerNames;

		public HotLoadableAndCookableInfo(Uri u, IHotloadable hl)
		{
			Uri = u;
			_hotloadable = hl;
		}

		public int CompareTo(object obj)
		{
			if (obj is ICookableInfo other)
			{
				return CompareTo(other);
			}
			return 1;
		}

		public int CompareTo(ICookableInfo other)
		{
			return Uri.Compare(Uri, other.Uri, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.CurrentCultureIgnoreCase);
		}

		public bool Equals(ICookableInfo other)
		{
			return Uri.Equals(other.Uri);
		}

		public bool Equals(IHotloadable other)
		{
			return _hotloadable.Equals(other);
		}

		public override int GetHashCode()
		{
			return Uri.GetHashCode();
		}

		public override string ToString()
		{
			return Uri.ToString();
		}
	}

	private class CookList
	{
		public readonly IEnumerable<EntityID> SingleAssetCooks;

		public readonly IEnumerable<ICookableInfo> CookableInfos;

		public CookList(IEnumerable<EntityID> singleAssetCooks, IEnumerable<ICookableInfo> cookableInfo)
		{
			SingleAssetCooks = singleAssetCooks;
			CookableInfos = cookableInfo;
		}

		public bool Any()
		{
			if (!SingleAssetCooks.Any())
			{
				return CookableInfos.Any();
			}
			return true;
		}
	}

	private ManualResetEvent _cooksInFlightLock = new ManualResetEvent(initialState: true);

	private readonly ICookService _cookService;

	private readonly ICivTechService _civTechService;

	private readonly IDocumentService _documentService;

	private readonly ITunerService _tunerService;

	private readonly IHotLoadService _hotLoadService;

	private readonly ICookableRegistry _cookRegistry;

	private readonly ISettingsService _settingsService;

	private readonly ITemporaryArtProjectService _temporaryWorkspaceService;

	private readonly ISplashScreenService _splashScreenService;

	private readonly Thread _cookAndHotLoadThread;

	private readonly AutoResetEvent _cookThreadSignal = new AutoResetEvent(initialState: false);

	private volatile bool _runQueue = true;

	private readonly ConcurrentQueue<Uri> _urisToProcess = new ConcurrentQueue<Uri>();

	private readonly IBatchCooker _standardCooker;

	private readonly SingleAssetCooker _singleAssetCooker;

	public void StartProjectChange(Action<string> statusMessagePrinter)
	{
		statusMessagePrinter?.Invoke("Draining hot load cook queue...");
		_cooksInFlightLock.WaitOne();
	}

	public void FinishProjectChange(Action<string> statusMessagePrinter)
	{
	}

	[ImportingConstructor]
	public WorkspaceWatcherService(ICivTechService civTechSvc, IDocumentService documentService, ITunerService tunerService, IHotLoadService hotLoadService, ICookableRegistry cookReg, ICookService cookService, ISettingsService settingsService, ITemporaryArtProjectService temporaryWorkspaceService, ISplashScreenService splashScreenService)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Starting up WorkspaceWatcherService");
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			_civTechService = civTechSvc;
			_documentService = documentService;
			_tunerService = tunerService;
			_hotLoadService = hotLoadService;
			_cookService = cookService;
			_cookRegistry = cookReg;
			_settingsService = settingsService;
			_temporaryWorkspaceService = temporaryWorkspaceService;
			_splashScreenService = splashScreenService;
			_cookAndHotLoadThread = new Thread(ProcessQueue);
			_cookAndHotLoadThread.IsBackground = false;
			_cookAndHotLoadThread.Name = "Cook and Hotload Thread";
			_standardCooker = new BatchCooker(civTechSvc, cookService);
			_singleAssetCooker = new SingleAssetCooker(civTechSvc, cookService);
			_hotLoadService.HotLoadCompleted += SignalCookThread;
		}
	}

	public void Initialize()
	{
		_cookAndHotLoadThread.Start();
	}

	public void AddDocumentToQueue(IDocument document)
	{
		AddFileToQueue(document.Uri);
	}

	public void AddFileToQueue(Uri uri)
	{
		AddFilesToQueue(new Uri[1] { uri });
	}

	public void AddFilesToQueue(IEnumerable<Uri> fileUris)
	{
		if (!_tunerService.IsConnected)
		{
			return;
		}
		foreach (Uri fileUri in fileUris)
		{
			_urisToProcess.Enqueue(fileUri);
		}
		if (!_hotLoadService.IsHotLoading)
		{
			_cookThreadSignal.Set();
		}
	}

	private void SignalCookThread(object sender, EventArgs e)
	{
		_cookThreadSignal.Set();
	}

	private void ProcessQueue(object context)
	{
		while (_runQueue && _cookThreadSignal.WaitOne())
		{
			_cooksInFlightLock.Reset();
			List<EntityID> list = new List<EntityID>();
			CookList cookList = BuildCookList();
			if (cookList.Any())
			{
				_hotLoadService.BeginHotLoadRequest();
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				if (_temporaryWorkspaceService.SingleAssetCookEnabled && cookList.SingleAssetCooks.Any())
				{
					IEnumerable<Uri> cookUris = _temporaryWorkspaceService.GetCookUris(cookList.SingleAssetCooks);
					if (cookUris.Any())
					{
						_singleAssetCooker.SetOutputPathOverrides(_temporaryWorkspaceService);
						CookResult cookResult = _singleAssetCooker.Cook(cookUris);
						if (!cookResult.Result)
						{
							Outputs.WriteLine(OutputMessageType.Error, "{0}\n\n{1}", cookResult.Result.Message, string.Join("\n", from itm in cookResult.ItemResults
								where !itm.Result
								select itm.Item));
						}
						IHotLoadData hotLoadData = _singleAssetCooker.GetHotLoadData();
						if (hotLoadData != null)
						{
							_hotLoadService.AddHotLoadData(hotLoadData);
						}
					}
					else
					{
						list.AddRange(cookList.SingleAssetCooks);
					}
				}
				if (cookList.CookableInfos.Any())
				{
					IEnumerable<Uri> enumerable = cookList.CookableInfos.Select((ICookableInfo info) => info.Uri).ToArray();
					CookResult cookResult2 = _standardCooker.Cook(enumerable);
					if (!cookResult2.Result)
					{
						Outputs.WriteLine(OutputMessageType.Error, "{0}\n\n{1}", cookResult2.Result.Message, string.Join("\n", from itm in cookResult2.ItemResults
							where !itm.Result
							select itm.Item));
					}
					IHotLoadData hotLoadData2 = _standardCooker.GetHotLoadData();
					if (hotLoadData2 != null)
					{
						_hotLoadService.AddHotLoadData(hotLoadData2);
					}
					if (_temporaryWorkspaceService.SingleAssetCookEnabled && !cookList.SingleAssetCooks.Any())
					{
						CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
						foreach (Uri item in enumerable)
						{
							IXLP iXLP = civTechContext.CreateInstance<IXLP>();
							iXLP.DeserializeFromFile(item.LocalPath);
							IEnumerable<Uri> urisToCook = _temporaryWorkspaceService.RemoveOverlappingTempXLPEntries(iXLP);
							_singleAssetCooker.SetOutputPathOverrides(_temporaryWorkspaceService);
							CookResult cookResult3 = _singleAssetCooker.Cook(urisToCook);
							if (!cookResult3.Result)
							{
								Outputs.WriteLine(OutputMessageType.Error, "{0}\n\n{1}", cookResult3.Result.Message, string.Join("\n", from itm in cookResult3.ItemResults
									where !itm.Result
									select itm.Item));
							}
							IHotLoadData hotLoadData3 = _singleAssetCooker.GetHotLoadData();
							if (hotLoadData2 != null)
							{
								_hotLoadService.AddHotLoadData(hotLoadData3);
							}
							iXLP.Dispose();
						}
					}
				}
				stopwatch.Stop();
				Outputs.WriteLine(OutputMessageType.Info, "Processing the cook queue and cooking all files took {0}.", TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString());
				_hotLoadService.EndHotLoadRequest();
			}
			if (list.Any((EntityID entId) => EntityHasDependents(entId)))
			{
				MessageBoxes.Show("The following entities will not cook or hotload, because one or more cookable entities that reference them are not open in the editor.\n\nPlease open one or more entities that should be cooked and hotloaded or disable the single asset cook feature.\n\n" + string.Format("The following entities will not be cooked or hotloaded:\n\t{0}", string.Join("\t", from entId in list
					where EntityHasDependents(entId)
					select entId.Type.ToString() + ": " + entId.Name)), "Single Asset Cook Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			_cooksInFlightLock.Set();
		}
	}

	private bool EntityHasDependents(EntityID entId)
	{
		Uri uri = new Uri(_civTechService.GetEntityPath(entId.Name, entId.Type));
		return _civTechService.GetWorkspaceDependencyRegistry(uri).GetDependents(uri).Any();
	}

	private CookList BuildCookList()
	{
		IProjectMapService projectMapService = _civTechService.ProjectMapService;
		ISet<ICookableInfo> set = new HashSet<ICookableInfo>();
		ISet<EntityID> set2 = new SortedSet<EntityID>();
		List<Uri> list = new List<Uri>();
		Uri result;
		while (_urisToProcess.TryDequeue(out result))
		{
			string instanceName;
			InstanceType type;
			if (IsCookable(result))
			{
				if (IsCookingEnabled(result))
				{
					list.Add(result);
					continue;
				}
				Outputs.WriteLine(OutputMessageType.Info, "Not cooking {0} because cooking has been explicitly disabled for this file", result.LocalPath);
			}
			else if (_temporaryWorkspaceService.SingleAssetCookEnabled && StaticMethods.GetInstanceNameAndType(projectMapService, result.LocalPath, out instanceName, out type))
			{
				EntityID entityID = new EntityID(instanceName, type);
				set2.Add(entityID);
				Outputs.WriteLine(OutputMessageType.Info, "Added {0} to the cook queue.", entityID);
			}
			else
			{
				IEnumerable<Uri> collection = FindCookableDependants(result);
				list.AddRange(collection);
			}
		}
		foreach (Uri item in list)
		{
			ICookableInfo cookableInfo = CreateCookableInfo(item);
			if (IsCookingEnabled(cookableInfo.Uri))
			{
				if (set.Add(cookableInfo))
				{
					Outputs.WriteLine(OutputMessageType.Info, "Added {0} to the cook queue.", item);
				}
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Info, "Not cooking {0} because cooking has been explicitly disabled for this file", cookableInfo.Uri.LocalPath);
			}
		}
		return new CookList(set2, set);
	}

	private IHotloadable LoadHotLoadInfo(Uri uri)
	{
		if (IsArtDef(uri))
		{
			string text = uri.LocalPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			string oldValue = _civTechService.PrimaryProject.Paths.ArtDefRoot.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.AltDirectorySeparatorChar;
			string artDefOutputRoot = _civTechService.PrimaryProject.Paths.ArtDefOutputRoot;
			return new HotloadableInfo(TunerHelper.GetArtDefSubSystem(), TunerHelper.GetArtDefConsumerNames(text.Replace(oldValue, string.Empty), artDefOutputRoot), uri);
		}
		if (IsXLP(uri))
		{
			IXLP xlp = global::DatabaseWrapper.DatabaseWrapper.LoadXLP(uri);
			return new HotloadableInfo(TunerHelper.GetXLPSubSystem(xlp), TunerHelper.GetXLPConsumerNames(xlp, _civTechService.PrimaryProject.Paths.GameDirectory), uri);
		}
		return null;
	}

	private ICookableInfo CreateCookableInfo(Uri uri)
	{
		IHotloadable hotloadable = LoadHotLoadInfo(uri);
		if (hotloadable != null)
		{
			return new HotLoadableAndCookableInfo(uri, hotloadable);
		}
		return new CookableInfo(uri);
	}

	private bool IsCookingEnabled(Uri uri)
	{
		return _cookRegistry.IsCookingEnabled(uri);
	}

	private bool IsArtDef(Uri doc)
	{
		return Path.GetExtension(doc.LocalPath).ToLower() == ".artdef";
	}

	private bool IsXLP(Uri doc)
	{
		return Path.GetExtension(doc.LocalPath).ToLower() == ".xlp";
	}

	private bool IsCookable(Uri doc)
	{
		string text = Path.GetExtension(doc.LocalPath).ToLower();
		if (!(text == ".artdef"))
		{
			return text == ".xlp";
		}
		return true;
	}

	private IEnumerable<Uri> FindCookableDependants(Uri doc)
	{
		ISet<Uri> set = new HashSet<Uri>();
		DependencyTree dependentTree = _civTechService.PrimaryProject.DependencyRegistry.GetDependentTree(doc);
		AddDependentsToSet(set, dependentTree);
		return set;
	}

	private void AddDependentsToSet(ISet<Uri> cookables, DependencyTree dependentTree)
	{
		Uri root = dependentTree.Root;
		if (IsCookable(root))
		{
			cookables.Add(root);
		}
		foreach (DependencyTree dependent in dependentTree.Dependents)
		{
			AddDependentsToSet(cookables, dependent);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		_hotLoadService.HotLoadCompleted -= SignalCookThread;
		_runQueue = false;
		_cookThreadSignal.Set();
		if (_cookAndHotLoadThread.IsAlive)
		{
			Action action = delegate
			{
				_cookAndHotLoadThread.Join();
			};
			string message = "Waiting for cooker thread to finish...";
			_splashScreenService.ShowSplashScreen(action, "Document Watcher Service", message);
		}
		_cookThreadSignal.Dispose();
	}
}
