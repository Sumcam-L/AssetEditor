using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using Firaxis.Utility;

namespace Firaxis.AssetBrowser.ViewModels;

[Serializable]
internal class StaticFilterViewModel : BaseViewModel, IClassicStyleFilterViewModel, IFilterViewModel, IViewModel, IDisposable, INotifyPropertyChanged, IComparable<IViewModel>, System.Runtime.Serialization.ISerializable
{
	private string _filterText = string.Empty;

	private string _filterByChoice;

	[NonSerialized]
	private List<AssetTypeViewModel> _assetTypes;

	private List<ProjectFilterViewModel> _projects = new List<ProjectFilterViewModel>();

	public string FilterName { get; } = "Classic";

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

	private List<string> FilterByChoicesInternal { get; } = new List<string>();

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

	public bool IsClassFilteringEnabled => false;

	public string ClassFilterDisplay => string.Empty;

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

	private IEntityFilteringContext FilterContext { get; set; }

	private INameFilterDefinition NameFilter { get; set; }

	private ITagFilterDefinition TagFilter { get; set; }

	private IInstanceTypeFilterDefinition TypeFilter { get; set; }

	private IProjectFilterDefinition ProjectFilter { get; set; }

	public event EventHandler FilterChanged;

	public StaticFilterViewModel()
	{
		ICivTechService service = Context.GetService<ICivTechService>();
		IEntityFilteringService service2 = Context.GetService<IEntityFilteringService>();
		FilterContext = new EntityFilteringContext();
		NameFilter = service2.CreateFilterDefinition<INameFilterDefinition>();
		TagFilter = service2.CreateFilterDefinition<ITagFilterDefinition>();
		TypeFilter = service2.CreateFilterDefinition<IInstanceTypeFilterDefinition>();
		ProjectFilter = service2.GetDefaultProjectFilter();
		FilterByChoicesInternal.Add("Name");
		FilterByChoicesInternal.Add("Tags");
		FilterByChoice = "Name";
		List<AssetTypeViewModel> list = new List<AssetTypeViewModel>();
		foreach (InstanceType validInstanceType in StaticMethods.GetValidInstanceTypes())
		{
			AssetTypeViewModel item = new AssetTypeViewModel(validInstanceType, "..", isSel: false);
			list.Add(item);
			TypeFilter.ValidInstanceTypes.Add(validInstanceType);
		}
		AssetTypesInternal = list;
		SetupProjectFilters(service);
	}

	protected StaticFilterViewModel(SerializationInfo info, StreamingContext context)
	{
		ICivTechService service = Context.GetService<ICivTechService>();
		IEntityFilteringService service2 = Context.GetService<IEntityFilteringService>();
		FilterContext = new EntityFilteringContext();
		NameFilter = service2.CreateFilterDefinition<INameFilterDefinition>();
		TagFilter = service2.CreateFilterDefinition<ITagFilterDefinition>();
		TypeFilter = service2.CreateFilterDefinition<IInstanceTypeFilterDefinition>();
		ProjectFilter = service2.GetDefaultProjectFilter();
		FilterByChoicesInternal.Add("Name");
		FilterByChoicesInternal.Add("Tags");
		FilterText = info.GetString("FilterText");
		FilterByChoice = info.GetString("FilterByChoice");
		List<InstanceType> list = (List<InstanceType>)info.GetValue("TypeFilter", typeof(List<InstanceType>));
		List<AssetTypeViewModel> list2 = new List<AssetTypeViewModel>();
		foreach (InstanceType validInstanceType in StaticMethods.GetValidInstanceTypes())
		{
			AssetTypeViewModel item = new AssetTypeViewModel(validInstanceType, "..", list.Contains(validInstanceType));
			list2.Add(item);
			TypeFilter.ValidInstanceTypes.Add(validInstanceType);
		}
		AssetTypesInternal = list2;
		SetupProjectFilters(service);
		try
		{
			string text = info.GetString("PrimaryProject");
			if (text.Equals(service.PrimaryProject.Name, StringComparison.CurrentCultureIgnoreCase))
			{
				List<string> validProjectNames = (List<string>)info.GetValue("ProjectFilter", typeof(List<string>));
				ProjectsInternal.ForEach(delegate(ProjectFilterViewModel vm)
				{
					vm.Visible = validProjectNames.Contains(vm.Project);
				});
			}
		}
		catch (SerializationException)
		{
		}
	}

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

	private void ProjFilterVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		OnFilterChanged();
	}

	private void SetupProjectFilters(ICivTechService civTechService)
	{
		ProjectFilterViewModel item = new ProjectFilterViewModel(civTechService.PrimaryProject.Name, enabledByDefault: true);
		ProjectsInternal.Add(item);
		foreach (string dependency in civTechService.PrimaryProject.Dependencies)
		{
			ProjectFilterViewModel item2 = new ProjectFilterViewModel(civTechService.AllProjectsMap[dependency].Name, enabledByDefault: true);
			ProjectsInternal.Add(item2);
		}
		if (ProjectFilter != null)
		{
			ProjectsInternal.ForEach(delegate(ProjectFilterViewModel vm)
			{
				vm.Visible = ProjectFilter.ValidProjectNames.Contains(vm.Project);
			});
		}
		ProjectsInternal.ForEach(delegate(ProjectFilterViewModel vm)
		{
			vm.PropertyChanged += ProjFilterVM_PropertyChanged;
		});
	}

	public IEntityFilterSet GetFilterSet()
	{
		FilterContext.FilterDefinitions.Clear();
		switch (GetFilterRequestType())
		{
		case FilterRequestType.Name:
			NameFilter.FilterText = FilterText;
			FilterContext.FilterDefinitions.Add(NameFilter);
			break;
		case FilterRequestType.Tags:
			TagFilter.FilterText = FilterText;
			FilterContext.FilterDefinitions.Add(TagFilter);
			break;
		}
		FilterContext.FilterDefinitions.Add(TypeFilter);
		TypeFilter.SelectedInstanceTypes.Clear();
		foreach (AssetTypeViewModel assetType in AssetTypes)
		{
			if (assetType.IsSelected)
			{
				TypeFilter.SelectedInstanceTypes.Add(assetType.InstanceType);
			}
		}
		FilterContext.FilterDefinitions.Add(ProjectFilter);
		ProjectFilter.ValidProjectNames.Clear();
		foreach (ProjectFilterViewModel item in ProjectsInternal)
		{
			if (item.Visible)
			{
				ProjectFilter.ValidProjectNames.Add(item.Project);
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
		info.AddValue("FilterText", FilterText);
		info.AddValue("FilterByChoice", FilterByChoice);
		if (ProjectFilter != null)
		{
			ICivTechService civTechService = Context.Get<ICivTechService>();
			if (civTechService != null)
			{
				info.AddValue("PrimaryProject", civTechService.PrimaryProject.Name);
				info.AddValue("ProjectFilter", ProjectFilter.ValidProjectNames.ToList(), typeof(List<string>));
			}
		}
		info.AddValue("TypeFilter", (from vm in AssetTypes
			where vm.IsSelected
			select vm.InstanceType).ToList(), typeof(List<InstanceType>));
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
