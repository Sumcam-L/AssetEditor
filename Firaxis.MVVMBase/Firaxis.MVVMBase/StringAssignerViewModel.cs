using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Firaxis.MVVMBase;

public class StringAssignerViewModel : DialogViewModel
{
	private string _title;

	private string _slotLabel;

	private string _domainLabel;

	private ObservableCollection<SlotViewModel> _slots;

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

	public string SlotLabel
	{
		get
		{
			return _slotLabel;
		}
		set
		{
			if (_slotLabel != value)
			{
				_slotLabel = value;
				OnPropertyChanged("SlotLabel");
			}
		}
	}

	public string DomainLabel
	{
		get
		{
			return _domainLabel;
		}
		set
		{
			if (_domainLabel != value)
			{
				_domainLabel = value;
				OnPropertyChanged("DomainLabel");
			}
		}
	}

	public ObservableCollection<SlotViewModel> Slots
	{
		get
		{
			return _slots;
		}
		set
		{
			if (_slots != value)
			{
				_slots = value;
				OnPropertyChanged("Slots");
			}
		}
	}

	public StringAssignerViewModel(IEnumerable<string> slots, IEnumerable<string> domain)
	{
		Slots = new ObservableCollection<SlotViewModel>();
		ISet<string> set = new SortedSet<string> { string.Empty };
		foreach (string item2 in domain)
		{
			set.Add(item2);
		}
		foreach (string item3 in slots.OrderBy((string x) => x))
		{
			SlotViewModel item = new SlotViewModel(item3, set);
			Slots.Add(item);
		}
	}

	public StringAssignerViewModel(IEnumerable<string> slots, IEnumerable<string> domain, IDictionary<string, string> assignedSlots)
	{
		ISet<string> set = new SortedSet<string> { string.Empty };
		foreach (string item in domain)
		{
			set.Add(item);
		}
		List<SlotViewModel> list = new List<SlotViewModel>();
		foreach (string item2 in slots.OrderBy((string x) => x))
		{
			SlotViewModel slotViewModel = new SlotViewModel(item2, set);
			if (assignedSlots.ContainsKey(item2))
			{
				slotViewModel.SelectedValue = assignedSlots[item2];
			}
			list.Add(slotViewModel);
		}
		List<SlotViewModel> first = new List<SlotViewModel>(from x in list
			where !string.IsNullOrEmpty(x.SelectedValue)
			orderby x.SlotName
			select x);
		List<SlotViewModel> second = new List<SlotViewModel>(from x in list
			where string.IsNullOrEmpty(x.SelectedValue)
			orderby x.SlotName
			select x);
		Slots = new ObservableCollection<SlotViewModel>(first.Union(second));
	}

	public IDictionary<string, string> GetAssignedSlots()
	{
		IDictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (SlotViewModel item in Slots.Where((SlotViewModel vm) => !string.IsNullOrEmpty(vm.SelectedValue)))
		{
			dictionary[item.SlotName] = item.SelectedValue;
		}
		return dictionary;
	}
}
