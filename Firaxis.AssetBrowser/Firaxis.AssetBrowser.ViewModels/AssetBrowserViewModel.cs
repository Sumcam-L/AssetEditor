using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;
using Firaxis.MVVMBase;
using Firaxis.MVVMBase.Helpers;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.AssetBrowser.ViewModels;

public class AssetBrowserViewModel : BaseViewModel, INotifyCollectionChanged
{
	private class AssetBrowserFilterRequest
	{
		public IEntityFilterSet FilterSet { get; private set; }

		public AssetBrowserFilterRequest(IEntityFilterSet entityFilteringContext)
		{
			FilterSet = entityFilteringContext;
		}
	}

	private const int _itemsPerPage = 100;

	private int currentPageIndex;

	private readonly object _filteredEntitiesLock = new object();

	private Thread _perforceUpdaterThread;

	private PerforceUpdateBacklog _perforceQueue = new PerforceUpdateBacklog();

	private readonly Dictionary<int, IList<EntityID>> _loadedEntities = new Dictionary<int, IList<EntityID>>();

	private IList<EntityID> _filterResults = new List<EntityID>();

	private static readonly byte[] _VersionBytes = Encoding.Default.GetBytes("Version1");

	public object locker = new object();

	private ViewMode _selectedViewMode = ViewMode.ClassicDetails;

	private readonly List<ToolbarButton> _entityCommands = new List<ToolbarButton>();

	private InstanceEntityViewModel _selectedEntity;

	private IList<InstanceEntityViewModel> _filteredEntities = new List<InstanceEntityViewModel>();

	private bool _allowMultipleSelection = true;

	private int _numEntitiesSelected;

	private IFilterViewModel _filterVM;

	private volatile bool m_filterThreadRunning = true;

	protected RelayCommand _openSelectedEntitiesCommand;

	protected RelayCommand _clearSearchCommand;

	public IEnumerable<ViewMode> ViewModes { get; } = new List<ViewMode>(Enum.GetValues(typeof(ViewMode)).OfType<ViewMode>());

	public ViewMode SelectedViewMode
	{
		get
		{
			return _selectedViewMode;
		}
		set
		{
			if (_selectedViewMode != value)
			{
				InstanceEntityViewModel[] selectedEntities = ((NumEntitiesSelected == 1) ? new InstanceEntityViewModel[1] { SelectedEntity } : ((NumEntitiesSelected <= 1) ? new InstanceEntityViewModel[0] : SelectedEntities.ToArray()));
				_selectedViewMode = value;
				OnPropertyChanged("SelectedViewMode");
				UpdateSelectedEntities(selectedEntities);
			}
		}
	}

	public bool HasCommands => _entityCommands.Count > 0;

	public IEnumerable<ToolbarButton> EntityCommands => _entityCommands;

	public InstanceEntityViewModel SelectedEntity
	{
		get
		{
			return _selectedEntity;
		}
		set
		{
			if (_selectedEntity != value)
			{
				if (_selectedEntity != null)
				{
					_selectedEntity.IsSelected = NumEntitiesSelected != 0 && SelectedEntities.Contains(_selectedEntity);
				}
				_selectedEntity = value;
				if (_selectedEntity != null)
				{
					_selectedEntity.IsSelected = true;
				}
				OnPropertyChanged("SelectedEntity");
			}
		}
	}

	[DependsOn("SelectedEntity")]
	public IEnumerable<object> SelectedEntityInfo
	{
		get
		{
			if (_selectedEntity != null)
			{
				yield return new
				{
					label = "Name:",
					value = _selectedEntity.Name
				};
				yield return new
				{
					label = "Type:",
					value = _selectedEntity.Type
				};
				IInstanceEntity entity = _selectedEntity.Entity;
				if (entity != null)
				{
					yield return new
					{
						label = "Class:",
						value = entity.ClassName
					};
					yield return new
					{
						label = "Description:",
						value = entity.Description
					};
					yield return new
					{
						label = "Tags:",
						value = string.Join(", ", entity.Tags)
					};
					yield return new
					{
						label = "Extension:",
						value = entity.XMLExtension
					};
				}
				else
				{
					yield return new
					{
						label = "Entity:",
						value = "failed to load!"
					};
				}
			}
		}
	}

	public IList<InstanceEntityViewModel> FilteredEntities
	{
		get
		{
			return _filteredEntities;
		}
		set
		{
			if (_filteredEntities == value)
			{
				return;
			}
			lock (_filteredEntitiesLock)
			{
				SelectedEntity = null;
				UpdateSelectedEntities(new InstanceEntityViewModel[0]);
				_filteredEntities = value;
				if (AutoSelectFirstEntity && FilterVM != null && !string.IsNullOrWhiteSpace(FilterVM.FilterText))
				{
					SelectedEntity = _filteredEntities.FirstOrDefault();
				}
			}
		}
	}

	public bool AutoSelectFirstEntity { get; set; } = false;

	public bool AllowMultipleSelection
	{
		get
		{
			return _allowMultipleSelection;
		}
		set
		{
			if (_allowMultipleSelection != value)
			{
				_allowMultipleSelection = value;
				OnPropertyChanged("AllowMultipleSelection");
			}
		}
	}

	public int NumEntitiesSelected
	{
		get
		{
			return _numEntitiesSelected;
		}
		set
		{
			if (_numEntitiesSelected != value)
			{
				_numEntitiesSelected = value;
				OnPropertyChanged("NumEntitiesSelected");
			}
		}
	}

	public IEnumerable<InstanceEntityViewModel> SelectedEntities { get; private set; } = new List<InstanceEntityViewModel>();

	private IInstanceSet Instances { get; set; }

	private ICivTechService CivTechService { get; }

	private IEntityFilteringService EntityFilteringService { get; set; }

	public IFilterViewModel FilterVM
	{
		get
		{
			return _filterVM;
		}
		set
		{
			if (_filterVM != value)
			{
				if (_filterVM != null)
				{
					_filterVM.FilterChanged -= HandleFilterChanged;
				}
				_filterVM = value;
				if (_filterVM != null)
				{
					_filterVM.FilterChanged += HandleFilterChanged;
					EnqueueFilterRequest(_filterVM.GetFilterSet());
				}
				OnPropertyChanged("FilterVM");
			}
		}
	}

	private ConcurrentQueue<AssetBrowserFilterRequest> FilterRequests { get; set; } = new ConcurrentQueue<AssetBrowserFilterRequest>();

	private Thread FilterThread { get; set; }

	private AutoResetEvent FilterThreadSignal { get; set; } = new AutoResetEvent(initialState: false);

	private string BrowserStatePath { get; }

	public string FilterText
	{
		get
		{
			return FilterVM?.FilterText;
		}
		set
		{
			if (FilterVM != null && !(FilterVM.FilterText == value))
			{
				FilterVM.FilterText = value;
				OnPropertyChanged("FilterText");
			}
		}
	}

	public Action<IList, IList, IList> EntitySelectionChangedAction => EntitySelectionChanged;

	public Func<IDataObject> CreateDragDataObjectFunc => CreateDragDataObject;

	public Func<bool> IsShowingEntitiesFunc => IsShowingEntities;

	public bool StartFocused { get; set; }

	public Action DialogAction { get; }

	public RelayCommand OpenSelectedEntitiesCommand
	{
		get
		{
			return _openSelectedEntitiesCommand ?? (_openSelectedEntitiesCommand = new RelayCommand(OpenSelectedEntities));
		}
		protected set
		{
			if (_openSelectedEntitiesCommand == value)
			{
				_openSelectedEntitiesCommand = value;
				OnPropertyChanged("OpenSelectedEntitiesCommand");
			}
		}
	}

	public RelayCommand ClearSearchCommand
	{
		get
		{
			return _clearSearchCommand ?? (_clearSearchCommand = new RelayCommand(ClearSearch));
		}
		protected set
		{
			if (_clearSearchCommand == value)
			{
				_clearSearchCommand = value;
				OnPropertyChanged("ClearSearchCommand");
			}
		}
	}

	public event NotifyCollectionChangedEventHandler CollectionChanged;

	public AssetBrowserViewModel(ICivTechService civTechSvc, IEntityFilteringService filteringService, Action dialogAction = null, bool allowMultipleSelection = true, bool startFocused = true)
	{
		CivTechService = civTechSvc;
		EntityFilteringService = filteringService;
		BrowserStatePath = civTechSvc.GetBrowserDataPath();
		StartFocused = startFocused;
		FilterVM = new FilterStyleCollectionViewModel
		{
			Filters = 
			{
				(IFilterViewModel)new StaticFilterViewModel(),
				(IFilterViewModel)new StackFilterViewModel()
			},
			SelectedTabIndex = 0
		};
		Instances = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() });
		InitializeFilterThread();
		DialogAction = dialogAction;
		AllowMultipleSelection = allowMultipleSelection;
		Messenger.RegisterByToken("PerforceRefresh", QueueFilteredEntityPerforceUpdate);
	}

	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		currentPageIndex++;
		if (currentPageIndex < _loadedEntities.Count)
		{
			LoadPage(_loadedEntities[currentPageIndex]);
		}
		if (_filterResults.Count == _filteredEntities.Count)
		{
			OnPropertyChanged("FilteredEntities");
			QueueFilteredEntityPerforceUpdate();
		}
	}

	private void HandleFilterChanged(object sender, EventArgs e)
	{
		EnqueueFilterRequest(FilterVM.GetFilterSet());
	}

	private void LoadPage(object args)
	{
		IList<EntityID> state = (IList<EntityID>)args;
		ThreadPool.QueueUserWorkItem(LoadPageWork, state);
	}

	private void LoadPageWork(object args)
	{
		lock (locker)
		{
			IList<EntityID> inputEntities = (IList<EntityID>)args;
			try
			{
				lock (_filteredEntitiesLock)
				{
					_filteredEntities.AddRange(BuildFilteredEntityCollection(inputEntities));
				}
			}
			finally
			{
				FireCollectionReset();
			}
		}
	}

	private void FireCollectionReset()
	{
		NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
		OnCollectionChanged(e);
	}

	private void PopulateEntities()
	{
		_loadedEntities.Clear();
		currentPageIndex = 0;
		int num = _filterResults.Count / 100;
		int count = _filterResults.Count % 100;
		for (int i = 0; i < num; i++)
		{
			_loadedEntities.Add(i, null);
			_loadedEntities[i] = FetchRange(i * 100, 100);
		}
		_loadedEntities[num] = FetchRange(num * 100, count);
	}

	private IList<EntityID> FetchRange(int startIndex, int count)
	{
		List<EntityID> list = new List<EntityID>();
		for (int i = startIndex; i < startIndex + count; i++)
		{
			list.Add(_filterResults[i]);
		}
		return list;
	}

	private void InitializeFilterThread()
	{
		FilterThread = new Thread(FilterThreadCall);
		FilterThread.Name = "Asset Browser Filter Thread";
		FilterThread.IsBackground = true;
		FilterThread.Start();
	}

	public void RegisterEntityCommand(string commandName, ImageSource content, Action<object> executeAction, [Optional] Predicate<object> canExecute)
	{
		if (string.IsNullOrEmpty(commandName))
		{
			throw new ArgumentException("Command name cannot be empty.", "commandName");
		}
		if (executeAction == null)
		{
			throw new ArgumentNullException("executeAction", "Cannot execute a null action.");
		}
		ToolbarButton item = new ToolbarButton(commandName, content, executeAction, canExecute);
		_entityCommands.Add(item);
		OnPropertyChanged("EntityCommands");
		OnPropertyChanged("HasCommands");
	}

	public void RegisterEntityCommand(string commandName, ImageSource content, ICommand command)
	{
		if (command == null)
		{
			throw new ArgumentNullException("command", "command cannot be null.");
		}
		RegisterEntityCommand(commandName, content, command.Execute, command.CanExecute);
	}

	public void RegisterCommands(IAssetBrowserCommandProvider commands)
	{
		if (commands == null)
		{
			throw new ArgumentNullException("commands", "command cannot be null.");
		}
		IEnumerableExtensions.ForEach(commands.Commands, delegate(IAssetBrowserCommandDefinition cmd)
		{
			RegisterEntityCommand(cmd.Name, cmd.Content, cmd.Command);
		});
	}

	public void ImportAllSelectedEntities()
	{
		IEnumerable<IImportedEntity> entities = from viewModel in SelectedEntities
			where viewModel.Entity is IImportedEntity
			select viewModel.Entity as IImportedEntity;
		global::DatabaseWrapper.DatabaseWrapper.ImportEntities(CivTechService, CivTechService.PrimaryProject.Name, entities);
	}

	public void QueueFilteredEntityPerforceUpdate()
	{
		if (_perforceQueue == null)
		{
			return;
		}
		EntityFileInfo[] infos;
		lock (_filteredEntitiesLock)
		{
			infos = (from e in FilteredEntities
				where !string.IsNullOrWhiteSpace(e.Name)
				select new EntityFileInfo(e.InstanceType, e.Name)).ToArray();
		}
		_perforceQueue.AddPendingInfos(infos);
		if (_perforceUpdaterThread == null || _perforceUpdaterThread.ThreadState == ThreadState.Stopped)
		{
			_perforceUpdaterThread = new Thread(PerforceUpdateWorker);
			_perforceUpdaterThread.IsBackground = true;
			_perforceUpdaterThread.Start(this);
		}
	}

	private void PerforceUpdateWorker(object owner)
	{
		AssetBrowserViewModel assetBrowserViewModel = owner as AssetBrowserViewModel;
		PerforceUpdateBacklog perforceQueue = _perforceQueue;
		while (perforceQueue != null)
		{
			EntityFileInfo[] nextCurrent = perforceQueue.GetNextCurrent();
			while (nextCurrent.Length != 0)
			{
				IDictionary<int, string> dictionary = new Dictionary<int, string>();
				IDictionary<int, DateTime> dictionary2 = new Dictionary<int, DateTime>();
				EntityFileInfo[] array = CivTechHelper.UpdateStatus(nextCurrent);
				if (array.Length != 0)
				{
					EntityFileInfo[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						EntityFileInfo entityFileInfo = array2[i];
						int hashCode = entityFileInfo.GetHashCode();
						dictionary[hashCode] = entityFileInfo.status;
						dictionary2[hashCode] = entityFileInfo.lastModified;
					}
					IEnumerable<InstanceEntityViewModel> enumerable;
					lock (assetBrowserViewModel._filteredEntitiesLock)
					{
						enumerable = assetBrowserViewModel.FilteredEntities.ToArray();
					}
					foreach (InstanceEntityViewModel item in enumerable)
					{
						int key = EntityFileInfo.GenerateHashCode(item.InstanceType, item.Name);
						if (dictionary.ContainsKey(key))
						{
							item.PerforceStatus = dictionary[key];
							item.PerforceModTime = dictionary2[key];
						}
					}
				}
				nextCurrent = perforceQueue.GetNextCurrent();
			}
			perforceQueue = _perforceQueue;
			Thread.Sleep(1);
		}
	}

	public void UpdateSelectedEntities(InstanceEntityViewModel[] selectedEntities)
	{
		int num = (NumEntitiesSelected = selectedEntities.Length);
		SelectedEntities = selectedEntities;
		if (num > 0)
		{
			if (!AllowMultipleSelection || num == 1)
			{
				SelectedEntity = selectedEntities.First();
			}
			else
			{
				SelectedEntity = null;
			}
		}
		else
		{
			SelectedEntity = null;
		}
		Firaxis.MVVMBase.Helpers.ApplicationHelper.BeginInvokeIfNeeded(delegate
		{
			_entityCommands.ForEach(delegate(ToolbarButton cmd)
			{
				cmd.UpdateCanExecute();
			});
		});
	}

	private void EnqueueFilterRequest(IEntityFilterSet filterSet)
	{
		if (filterSet != null)
		{
			AssetBrowserFilterRequest item = new AssetBrowserFilterRequest(filterSet);
			FilterRequests.Enqueue(item);
			FilterThreadSignal.Set();
		}
	}

	private void FilterThreadCall(object context)
	{
		while (m_filterThreadRunning && FilterThreadSignal.WaitOne())
		{
			AssetBrowserFilterRequest assetBrowserFilterRequest = null;
			AssetBrowserFilterRequest result;
			while (FilterRequests.TryDequeue(out result))
			{
				assetBrowserFilterRequest = result;
			}
			if (assetBrowserFilterRequest?.FilterSet != null)
			{
				IList<EntityID> entities = CivTechRegistry.EntityCacheService.GetAllEntities().ToList();
				List<EntityID> source = assetBrowserFilterRequest.FilterSet.FilterEntities(entities).ToList();
				_filterResults = source.OrderBy((EntityID x) => x).ToList();
				PopulateEntities();
				lock (_filteredEntitiesLock)
				{
					FilteredEntities = new List<InstanceEntityViewModel>();
					OnPropertyChanged("FilteredEntities");
				}
				if (_loadedEntities.Count > 0)
				{
					ThreadPool.QueueUserWorkItem(LoadPage, _loadedEntities[0]);
				}
			}
		}
	}

	private IList<InstanceEntityViewModel> BuildFilteredEntityCollection(IList<EntityID> inputEntities)
	{
		IList<InstanceEntityViewModel> list = new List<InstanceEntityViewModel>();
		foreach (EntityID item2 in inputEntities.OrderBy((EntityID x) => x))
		{
			InstanceEntityViewModel item = InstanceEntityViewModel.Create(item2.Type, CivTechService, item2.Name, Instances.LoadEntityByName);
			list.Add(item);
		}
		return list;
	}

	public void ClearUnselectedEntities()
	{
		if (Instances == null)
		{
			return;
		}
		if (NumEntitiesSelected == 0)
		{
			Instances.Clear();
		}
		else if (SelectedEntities != null)
		{
			IEnumerable<EntityID> entityIDs = SelectedEntities.Select((InstanceEntityViewModel ent) => new EntityID(ent.Name, ent.InstanceType)).ToArray();
			Instances.RemoveExcept(entityIDs);
		}
	}

	protected override void Dispose(bool disposeManaged)
	{
		m_filterThreadRunning = false;
		if (disposeManaged && !FilterThreadSignal.SafeWaitHandle.IsClosed)
		{
			FilterThreadSignal.Set();
			Thread.Sleep(25);
		}
		SaveState();
		if (FilterThread.IsAlive)
		{
			FilterThread.Join();
		}
		lock (_filteredEntitiesLock)
		{
			IEnumerableExtensions.ForEach(FilteredEntities, delegate(InstanceEntityViewModel vm)
			{
				vm.Dispose();
			});
			FilteredEntities.Clear();
		}
		if (Instances != null)
		{
			Instances.Dispose();
			Instances = null;
		}
		if (_perforceQueue != null)
		{
			_perforceQueue.Dispose();
			_perforceQueue = null;
		}
		if (_perforceUpdaterThread != null)
		{
			_perforceUpdaterThread.Join(50);
			_perforceUpdaterThread = null;
		}
		while (!FilterRequests.IsEmpty)
		{
			FilterRequests.TryDequeue(out var _);
		}
		FilterThreadSignal.Dispose();
		if (FilterVM != null)
		{
			FilterVM.FilterChanged -= HandleFilterChanged;
			FilterVM.Dispose();
			FilterVM = null;
		}
	}

	public void ClearSearch()
	{
		FilterText = "";
	}

	public void EntitySelectionChanged(IList selectedItems, IList addedItems, IList removedItems)
	{
		IEnumerable<InstanceEntityViewModel> source = selectedItems.Cast<InstanceEntityViewModel>();
		UpdateSelectedEntities(source.ToArray());
	}

	public void OpenSelectedEntities()
	{
		if (DialogAction != null)
		{
			DialogAction();
			return;
		}
		foreach (EntityID item in SelectedEntities.Select((InstanceEntityViewModel ent) => new EntityID(ent.Name, ent.InstanceType)))
		{
			Messenger.SendByType(new OpenEntityID(item));
		}
	}

	public IDataObject CreateDragDataObject()
	{
		if (SelectedEntity == null && CivTechService == null)
		{
			return null;
		}
		DataObject dataObject = new DataObject();
		if (SelectedEntity != null)
		{
			dataObject.SetData("Object", new EntityDragDropDataObject(SelectedEntity.Name, SelectedEntity.InstanceType, SelectedEntity.Entity?.ClassName));
		}
		if (CivTechService != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (InstanceEntityViewModel selectedEntity in SelectedEntities)
			{
				string entityPath = CivTechService.GetEntityPath(selectedEntity.Name, selectedEntity.InstanceType);
				stringBuilder.AppendLine(entityPath);
			}
			dataObject.SetData(DataFormats.Text, stringBuilder.ToString());
		}
		return dataObject;
	}

	public bool IsShowingEntities()
	{
		return FilteredEntities.Count > 0;
	}

	public void SaveState()
	{
		if (string.IsNullOrEmpty(BrowserStatePath))
		{
			return;
		}
		try
		{
			string directoryName = Path.GetDirectoryName(BrowserStatePath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			using Stream stream = File.Open(BrowserStatePath, FileMode.Create);
			stream.Write(_VersionBytes, 0, _VersionBytes.Length);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Serialize(stream, FilterVM);
			stream.Close();
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Unable to save docked Asset Browser State.  Reason: '{0}'.", ex.ToString());
			BugSubmitter.SilentException(ex);
		}
	}

	public void LoadState()
	{
		if (string.IsNullOrEmpty(BrowserStatePath) || !File.Exists(BrowserStatePath))
		{
			return;
		}
		try
		{
			using Stream stream = File.Open(BrowserStatePath, FileMode.Open);
			byte[] array = new byte[_VersionBytes.Length];
			stream.Read(array, 0, array.Length);
			if (!array.SequenceEqual(_VersionBytes))
			{
				Outputs.WriteLine(OutputMessageType.Info, "Unable to restore docked Asset Browser State from file of incorrect version");
				return;
			}
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			IFilterViewModel filterVM = (IFilterViewModel)binaryFormatter.Deserialize(stream);
			FilterVM = filterVM;
		}
		catch (SerializationException exObj)
		{
			Outputs.WriteLine(OutputMessageType.Info, $"Unable to restore docked Asset Browser State from file {BrowserStatePath}");
			BugSubmitter.SilentException(exObj);
		}
		catch (Exception exObj2)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Unable to restore docked Asset Browser State for unknown reason");
			BugSubmitter.SilentException(exObj2);
		}
	}
}
