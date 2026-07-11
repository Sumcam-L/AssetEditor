using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Input;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;
using Firaxis.Utility;

namespace Firaxis.AssetBrowser.ViewModels;

[Serializable]
internal class StackFilterViewModel : BaseViewModel, IStackFilterViewModel, IFilterViewModel, IViewModel, IDisposable, INotifyPropertyChanged, IComparable<IViewModel>, System.Runtime.Serialization.ISerializable, IDeserializationCallback
{
	private string _selectedFilterName;

	public string FilterName => "Stack";

	public string FilterText
	{
		get
		{
			return NameFilter.FilterText;
		}
		set
		{
			if (!string.Equals(NameFilter.FilterText, value, StringComparison.CurrentCultureIgnoreCase))
			{
				NameFilter.FilterText = value;
				OnPropertyChanged("FilterText");
				OnFilterChanged();
			}
		}
	}

	public ObservableCollection<FilterBuilderViewModel> FilterStack { get; set; } = new ObservableCollection<FilterBuilderViewModel>();

	public ObservableCollection<string> AvailableFilters { get; set; } = new ObservableCollection<string>();

	public string SelectedFilterName
	{
		get
		{
			return _selectedFilterName;
		}
		set
		{
			if (string.Equals(_selectedFilterName, value, StringComparison.CurrentCulture))
			{
				return;
			}
			_selectedFilterName = value;
			OnPropertyChanged("SelectedFilterName");
			if (!string.IsNullOrEmpty(value))
			{
				FilterBuilderViewModel filterBuilderViewModel = FilterFactory.CreateFilterBuilderViewModel(value);
				filterBuilderViewModel.Owner = this;
				filterBuilderViewModel.FilterChanged += HandleFilterChanged;
				FilterStack.Add(filterBuilderViewModel);
				if (!FilterBuilders.TryGetValue(value, out var value2))
				{
					value2 = new List<FilterBuilderViewModel>();
					FilterBuilders.Add(value, value2);
				}
				value2.Add(filterBuilderViewModel);
				OnFilterChanged();
				SelectedFilterName = string.Empty;
			}
		}
	}

	public ICommand RemoveFilterCommand => new DelegateCommand(RemoveFilter);

	private ICivTechService CivTechService { get; }

	private IEntityCacheService EntityCacheService { get; }

	private IEntityFilteringService FilteringService { get; }

	private IEntityFilteringContext FilterContext { get; }

	private INameFilterDefinition NameFilter { get; }

	private StackFilterFactory FilterFactory { get; }

	private Dictionary<string, List<FilterBuilderViewModel>> FilterBuilders { get; } = new Dictionary<string, List<FilterBuilderViewModel>>();

	public event EventHandler FilterChanged;

	protected virtual void OnFilterChanged()
	{
		this.FilterChanged?.Invoke(this, EventArgs.Empty);
	}

	private void HandleFilterChanged(object sender, EventArgs e)
	{
		OnFilterChanged();
	}

	private void RemoveFilter(object context)
	{
		if (!(context is FilterBuilderViewModel filterBuilderViewModel))
		{
			return;
		}
		filterBuilderViewModel.FilterChanged -= HandleFilterChanged;
		FilterStack.Remove(filterBuilderViewModel);
		foreach (List<FilterBuilderViewModel> value in FilterBuilders.Values)
		{
			if (value.Remove(filterBuilderViewModel))
			{
				break;
			}
		}
		OnFilterChanged();
	}

	public StackFilterViewModel()
	{
		CivTechService = Context.GetService<ICivTechService>();
		EntityCacheService = Context.GetService<IEntityCacheService>();
		FilteringService = Context.GetService<IEntityFilteringService>();
		FilterFactory = new StackFilterFactory(CivTechService, EntityCacheService, FilteringService);
		FilterContext = new EntityFilteringContext();
		NameFilter = FilterContext.GetOrCreateFilterDefinition<INameFilterDefinition>(FilteringService);
		foreach (string registeredFilterName in FilterFactory.RegisteredFilterNames)
		{
			AvailableFilters.Add(registeredFilterName);
		}
	}

	protected StackFilterViewModel(SerializationInfo info, StreamingContext context)
	{
		CivTechService = Context.GetService<ICivTechService>();
		EntityCacheService = Context.GetService<IEntityCacheService>();
		FilteringService = Context.GetService<IEntityFilteringService>();
		FilterFactory = new StackFilterFactory(CivTechService, EntityCacheService, FilteringService);
		FilterContext = new EntityFilteringContext();
		NameFilter = FilterContext.GetOrCreateFilterDefinition<INameFilterDefinition>(FilteringService);
		foreach (string registeredFilterName in FilterFactory.RegisteredFilterNames)
		{
			AvailableFilters.Add(registeredFilterName);
		}
		FilterText = info.GetString("FilterText");
		try
		{
			FilterStack = (ObservableCollection<FilterBuilderViewModel>)info.GetValue("FilterStack", typeof(ObservableCollection<FilterBuilderViewModel>));
			FilterBuilders = (Dictionary<string, List<FilterBuilderViewModel>>)info.GetValue("FilterBuilders", typeof(Dictionary<string, List<FilterBuilderViewModel>>));
		}
		catch (SerializationException)
		{
			FilterStack.Clear();
			FilterBuilders.Clear();
		}
	}

	public void OnDeserialization(object sender)
	{
		foreach (FilterBuilderViewModel item in FilterStack)
		{
			item.Owner = this;
			item.FilterChanged += HandleFilterChanged;
		}
	}

	public IEntityFilterSet GetFilterSet()
	{
		FilterContext.FilterDefinitions.Clear();
		FilterContext.FilterDefinitions.Add(NameFilter);
		foreach (KeyValuePair<string, List<FilterBuilderViewModel>> filterBuilder in FilterBuilders)
		{
			IEntityFilterDefinition entityFilterDefinition = FilterFactory.CreateFilterDefinition(filterBuilder.Key, filterBuilder.Value);
			if (entityFilterDefinition != null)
			{
				FilterContext.FilterDefinitions.Add(entityFilterDefinition);
			}
		}
		return FilterContext.CreateFilterSet();
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("FilterText", FilterText);
		info.AddValue("FilterBuilders", FilterBuilders);
		info.AddValue("FilterStack", FilterStack);
	}

	protected override void Dispose(bool disposeManaged)
	{
		foreach (FilterBuilderViewModel item in FilterStack)
		{
			item.FilterChanged -= HandleFilterChanged;
		}
		FilterStack.Clear();
		FilterBuilders.Clear();
	}
}
