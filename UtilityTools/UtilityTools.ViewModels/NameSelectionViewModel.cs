using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Firaxis.Collections;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class NameSelectionViewModel : DialogViewModel
{
	public class NameViewModel : SelectableItemViewModel
	{
		private string _name;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged("Name");
				}
			}
		}

		public NameViewModel(string name)
		{
			Name = name;
		}
	}

	private ObservableCollection<NameViewModel> _names = new ObservableCollection<NameViewModel>();

	private string _namesLabel;

	private string _title;

	private List<string> _selectedNames = new List<string>();

	public ObservableCollection<NameViewModel> Names
	{
		get
		{
			return _names;
		}
		set
		{
			if (_names != value)
			{
				_names = value;
				OnPropertyChanged("Names");
			}
		}
	}

	public string NamesLabel
	{
		get
		{
			return _namesLabel;
		}
		set
		{
			if (_namesLabel != value)
			{
				_namesLabel = value;
				OnPropertyChanged("NamesLabel");
			}
		}
	}

	public IEnumerable<string> SelectedNames => _selectedNames;

	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (_title != value)
			{
				_title = value;
				OnPropertyChanged("Title");
			}
		}
	}

	public NameSelectionViewModel(IEnumerable<string> names)
	{
		names.ForEach(delegate(string name)
		{
			Names.Add(new NameViewModel(name));
		});
	}

	protected override void ExecuteOKCommand(object context)
	{
		Names.Where((NameViewModel vm) => vm.IsSelected).ForEach(delegate(NameViewModel vm)
		{
			_selectedNames.Add(vm.Name);
		});
		base.ExecuteOKCommand(context);
	}
}
