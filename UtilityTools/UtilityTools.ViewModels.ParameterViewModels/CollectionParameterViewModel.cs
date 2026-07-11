using System.Collections.Generic;
using System.Collections.ObjectModel;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels.ParameterViewModels;

public class CollectionParameterViewModel : BaseParameterViewModel
{
	private ObservableCollection<BaseParameterViewModel> m_items;

	public ObservableCollection<BaseParameterViewModel> Items
	{
		get
		{
			return m_items;
		}
		set
		{
			if (m_items != value)
			{
				m_items = value;
				OnPropertyChanged("Items");
			}
		}
	}

	public CollectionParameterViewModel(ICollectionParameter param, ICollectionValue value, IEnumerable<BaseParameterViewModel> valueVMs)
		: base(param, value)
	{
		Items = new ObservableCollection<BaseParameterViewModel>(valueVMs);
	}
}
