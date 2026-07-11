using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Firaxis.MVVMBase;

public class SlotViewModel : Notifier
{
	private DelegateCommand _clearCommand;

	private string _selectedValue = string.Empty;

	public ICommand ClearCommand
	{
		get
		{
			if (_clearCommand == null)
			{
				_clearCommand = new DelegateCommand(delegate
				{
					SelectedValue = string.Empty;
				});
			}
			return _clearCommand;
		}
	}

	public string SlotName { get; private set; }

	public IEnumerable<string> Domain { get; private set; }

	public string SelectedValue
	{
		get
		{
			return _selectedValue;
		}
		set
		{
			if (_selectedValue != value && Domain.Contains(value))
			{
				_selectedValue = value;
				OnPropertyChanged("SelectedValue");
			}
		}
	}

	public SlotViewModel(string slotName, IEnumerable<string> domain)
		: this(slotName, domain, string.Empty)
	{
	}

	public SlotViewModel(string slotName, IEnumerable<string> domain, string defaultValue)
	{
		SlotName = slotName;
		Domain = domain;
		SelectedValue = defaultValue;
	}
}
