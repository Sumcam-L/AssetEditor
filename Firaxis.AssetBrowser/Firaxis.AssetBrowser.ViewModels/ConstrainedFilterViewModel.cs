using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using Firaxis.Utility;

namespace Firaxis.AssetBrowser.ViewModels;

internal class ConstrainedFilterViewModel : BaseViewModel, IClassicStyleFilterViewModel, IFilterViewModel, IViewModel, IDisposable, INotifyPropertyChanged, IComparable<IViewModel>, System.Runtime.Serialization.ISerializable
{
	private string _filterText = string.Empty;

	private string _filterByChoice;

	private List<AssetTypeViewModel> _assetTypes = null;

	private List<ProjectFilterViewModel> _projects = new List<ProjectFilterViewModel>();

	public string FilterName => "Classic";

	public string FilterText
	{
		get
		{
			return _filterText;
		}
		set
		{
			if (!_filterText.Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				_filterText = value;
				OnPropertyChanged("FilterText");
				OnFilterChanged();
			}
		}
	}

	public List<string> FilterByChoices => FilterByChoicesInternal;

	private List<string> FilterByChoicesInternal { get; set; } = new List<string>();

	public string FilterByChoice
	{
		get
		{
			return _filterByChoice;
		}
		set
		{
			if (!string.Equals(_filterByChoice, value, StringComparison.CurrentCulture))
			{
				_filterByChoice = value;
				OnPropertyChanged("FilterByChoice");
				OnFilterChanged();
			}
		}
	}

	public List<AssetTypeViewModel> AssetTypes => AssetTypesInternal;

	private List<AssetTypeViewModel> AssetTypesInternal
	{
		get
		{
			return _assetTypes;
		}
		set
		{
			if (_assetTypes == value)
			{
				return;
			}
			if (_assetTypes != null)
			{
				foreach (AssetTypeViewModel assetType in _assetTypes)
				{
					assetType.PropertyChanged -= AssetTypeVM_PropertyChanged;
				}
			}
			_assetTypes = value;
			if (_assetTypes != null)
			{
				foreach (AssetTypeViewModel assetType2 in _assetTypes)
				{
					assetType2.PropertyChanged += AssetTypeVM_PropertyChanged;
				}
			}
			OnPropertyChanged("AssetTypes");
			OnFilterChanged();
		}
	}

	public bool IsClassFilteringEnabled => ClassFilter != null;

	public string ClassFilterDisplay { get; private set; }

	public List<ProjectFilterViewModel> Projects => ProjectsInternal;

	private List<ProjectFilterViewModel> ProjectsInternal
	{
		get
		{
			return _projects;
		}
		set
		{
			if (_projects != value)
			{
				_projects = value;
				OnPropertyChanged("Projects");
				OnFilterChanged();
			}
		}
	}

	private IEntityFilteringService FilteringService { get; set; }

	private IEntityFilteringContext FilterContext { get; set; }

	private INameFilterDefinition NameFilter { get; set; }

	private ITagFilterDefinition TagFilter { get; set; }

	private IInstanceTypeFilterDefinition TypeFilter { get; set; }

	private IProjectFilterDefinition ProjectFilter { get; set; }

	private IClassFilterDefinition ClassFilter { get; set; }

	public event EventHandler FilterChanged;

	protected virtual void OnFilterChanged()
	{
		this.FilterChanged?.Invoke(this, EventArgs.Empty);
	}

	private void AssetTypeVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "IsSelected")
		{
			OnFilterChanged();
		}
	}

	public ConstrainedFilterViewModel(IEntityFilteringContext filteringContext)
	{
		ICivTechService service = Context.GetService<ICivTechService>();
		IEntityFilteringService service2 = Context.GetService<IEntityFilteringService>();
		FilteringService = service2;
		FilterContext = filteringContext;
		NameFilter = FilterContext.GetFilterDefinition<INameFilterDefinition>();
		TagFilter = FilterContext.GetFilterDefinition<ITagFilterDefinition>();
		TypeFilter = FilterContext.GetFilterDefinition<IInstanceTypeFilterDefinition>();
		ProjectFilter = FilterContext.GetFilterDefinition<IProjectFilterDefinition>();
		ClassFilter = FilterContext.GetFilterDefinition<IClassFilterDefinition>();
		FilterByChoicesInternal.Add("Name");
		FilterByChoicesInternal.Add("Tags");
		ITextInputFilter filterDefinition = FilterContext.GetFilterDefinition<ITextInputFilter>();
		if (filterDefinition != null)
		{
			if (filterDefinition.FilterName == "Tag")
			{
				FilterByChoice = "Tags";
			}
			else
			{
				FilterByChoice = filterDefinition.FilterName;
			}
			FilterText = filterDefinition.FilterText;
		}
		else
		{
			FilterByChoice = "Name";
		}
		ClassFilterDisplay = GetClassFilterString(ClassFilter);
		List<AssetTypeViewModel> list = new List<AssetTypeViewModel>();
		if (TypeFilter != null)
		{
			bool flag = TypeFilter.ValidInstanceTypes.Count < 2;
			foreach (InstanceType validInstanceType in TypeFilter.ValidInstanceTypes)
			{
				AssetTypeViewModel item = new AssetTypeViewModel(validInstanceType, "..", flag || TypeFilter.SelectedInstanceTypes.Contains(validInstanceType));
				list.Add(item);
			}
		}
		AssetTypesInternal = list;
		SetupProjectFilters(service);
	}

	private string GetClassFilterString(IClassFilterDefinition classFilter)
	{
		if (classFilter == null || classFilter.ValidClassNames.Count == 0)
		{
			return null;
		}
		return string.Join("|", classFilter.ValidClassNames);
	}

	private void SetupProjectFilters(ICivTechService civTechService)
	{
		ProjectFilterViewModel item = CreateProjectFilterVM(civTechService.PrimaryProject.Name);
		ProjectsInternal.Add(item);
		foreach (string dependency in civTechService.PrimaryProject.Dependencies)
		{
			ProjectFilterViewModel item2 = CreateProjectFilterVM(civTechService.AllProjectsMap[dependency].Name);
			ProjectsInternal.Add(item2);
		}
	}

	private ProjectFilterViewModel CreateProjectFilterVM(string projectName)
	{
		ProjectFilterViewModel projectFilterViewModel = new ProjectFilterViewModel(projectName, ProjectFilter == null || ProjectFilter.ValidProjectNames.Contains(projectName));
		projectFilterViewModel.PropertyChanged += ProjFilterVM_PropertyChanged;
		return projectFilterViewModel;
	}

	private void ProjFilterVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		OnFilterChanged();
	}

	public IEntityFilterSet GetFilterSet()
	{
		switch (GetFilterRequestType())
		{
		case FilterRequestType.Name:
		{
			INameFilterDefinition orCreateFilterDefinition2 = FilterContext.GetOrCreateFilterDefinition<INameFilterDefinition>(FilteringService);
			orCreateFilterDefinition2.FilterText = FilterText;
			FilterContext.RemoveFilterDefinition<ITagFilterDefinition>();
			break;
		}
		case FilterRequestType.Tags:
		{
			ITagFilterDefinition orCreateFilterDefinition = FilterContext.GetOrCreateFilterDefinition<ITagFilterDefinition>(FilteringService);
			orCreateFilterDefinition.FilterText = FilterText;
			FilterContext.RemoveFilterDefinition<INameFilterDefinition>();
			break;
		}
		default:
			FilterContext.RemoveFilterDefinition<INameFilterDefinition>();
			FilterContext.RemoveFilterDefinition<ITagFilterDefinition>();
			break;
		}
		if (TypeFilter != null)
		{
			TypeFilter.SelectedInstanceTypes.Clear();
			foreach (AssetTypeViewModel assetType in AssetTypes)
			{
				if (assetType.IsSelected)
				{
					TypeFilter.SelectedInstanceTypes.Add(assetType.InstanceType);
				}
			}
		}
		if (ProjectFilter != null)
		{
			ProjectFilter.ValidProjectNames.Clear();
			foreach (ProjectFilterViewModel item in ProjectsInternal)
			{
				if (item.Visible)
				{
					ProjectFilter.ValidProjectNames.Add(item.Project);
				}
			}
		}
		return FilterContext.CreateFilterSet();
	}

	private FilterRequestType GetFilterRequestType()
	{
		string filterByChoice = FilterByChoice;
		if (!(filterByChoice == "Tags"))
		{
			if (filterByChoice == "Name")
			{
				return FilterRequestType.Name;
			}
			return FilterRequestType.Default;
		}
		return FilterRequestType.Tags;
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	protected override void Dispose(bool disposeManaged)
	{
		ProjectsInternal.ForEach(delegate(ProjectFilterViewModel vm)
		{
			vm.PropertyChanged -= ProjFilterVM_PropertyChanged;
		});
		ProjectsInternal.Clear();
		AssetTypesInternal.ForEach(delegate(AssetTypeViewModel vm)
		{
			vm.PropertyChanged -= AssetTypeVM_PropertyChanged;
		});
		AssetTypesInternal.Clear();
	}
}
