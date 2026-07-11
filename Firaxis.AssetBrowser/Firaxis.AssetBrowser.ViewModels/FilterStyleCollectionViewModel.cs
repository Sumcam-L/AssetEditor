using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

[Serializable]
internal class FilterStyleCollectionViewModel : BaseViewModel, IFilterViewModel, IViewModel, IDisposable, INotifyPropertyChanged, IComparable<IViewModel>, System.Runtime.Serialization.ISerializable, IDeserializationCallback
{
	private int _selectedTabIndex = -1;

	private IFilterViewModel _selectedFilter;

	public string FilterName => SelectedFilter?.FilterName;

	public string FilterText
	{
		get
		{
			return SelectedFilter?.FilterText;
		}
		set
		{
			if (SelectedFilter != null)
			{
				SelectedFilter.FilterText = value;
			}
		}
	}

	public ObservableCollection<IFilterViewModel> Filters { get; } = new ObservableCollection<IFilterViewModel>();

	public int SelectedTabIndex
	{
		get
		{
			return _selectedTabIndex;
		}
		set
		{
			if (_selectedTabIndex != value)
			{
				_selectedTabIndex = value;
				if (_selectedTabIndex >= 0 && _selectedTabIndex < Filters.Count)
				{
					SelectedFilter = Filters[value];
				}
				else
				{
					SelectedFilter = null;
				}
				OnPropertyChanged("SelectedTabIndex");
			}
		}
	}

	public IFilterViewModel SelectedFilter
	{
		get
		{
			return _selectedFilter;
		}
		set
		{
			if (_selectedFilter != value)
			{
				if (_selectedFilter != null)
				{
					_selectedFilter.FilterChanged -= HandleFilterChanged;
				}
				_selectedFilter = value;
				OnPropertyChanged("SelectedFilter");
				OnPropertyChanged("FilterText");
				if (_selectedFilter != null)
				{
					_selectedFilter.FilterText = FilterText;
					_selectedFilter.FilterChanged += HandleFilterChanged;
				}
				this.FilterChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public event EventHandler FilterChanged;

	private void HandleFilterChanged(object sender, EventArgs e)
	{
		this.FilterChanged?.Invoke(sender, e);
	}

	public IEntityFilterSet GetFilterSet()
	{
		return SelectedFilter?.GetFilterSet();
	}

	public FilterStyleCollectionViewModel()
	{
	}

	protected FilterStyleCollectionViewModel(SerializationInfo info, StreamingContext context)
	{
		Filters = (ObservableCollection<IFilterViewModel>)info.GetValue("Filters", typeof(ObservableCollection<IFilterViewModel>));
		SelectedTabIndex = info.GetInt32("SelectedTabIndex");
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Filters", Filters);
		info.AddValue("SelectedTabIndex", SelectedTabIndex);
		info.AddValue("FilterText", FilterText);
	}

	public void OnDeserialization(object sender)
	{
		int selectedTabIndex = SelectedTabIndex;
		SelectedTabIndex = -1;
		SelectedTabIndex = selectedTabIndex;
	}

	protected override void Dispose(bool disposeManaged)
	{
		SelectedFilter = null;
		Filters?.Clear();
	}
}
