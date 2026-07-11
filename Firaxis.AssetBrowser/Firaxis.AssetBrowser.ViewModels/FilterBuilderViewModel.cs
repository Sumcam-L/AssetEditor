using System;
using System.Collections.Generic;
using System.Windows.Input;
using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

[Serializable]
internal class FilterBuilderViewModel : Notifier
{
	private SortedSet<string> _possibleItems;

	private string _selectedItem = string.Empty;

	[NonSerialized]
	private StackFilterViewModel _owner;

	public string FilterName { get; }

	public IEnumerable<string> PossibleItems => _possibleItems;

	public string SelectedItem
	{
		get
		{
			return _selectedItem;
		}
		set
		{
			if (!string.Equals(_selectedItem, value, StringComparison.CurrentCultureIgnoreCase))
			{
				_selectedItem = value;
				OnPropertyChanged("SelectedItem");
				OnFilterChanged();
			}
		}
	}

	public ICommand ClearFilterCommand => new DelegateCommand(ClearFilterImpl);

	public ICommand RemoveFilterCommand => new DelegateCommand(RemoveFilterCommandImpl);

	public StackFilterViewModel Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			_owner = value;
		}
	}

	public event EventHandler FilterChanged;

	protected virtual void OnFilterChanged()
	{
		this.FilterChanged?.Invoke(this, EventArgs.Empty);
	}

	private void ClearFilterImpl(object context)
	{
		SelectedItem = string.Empty;
		OnFilterChanged();
	}

	private void RemoveFilterCommandImpl(object context)
	{
		Owner?.RemoveFilterCommand.Execute(this);
	}

	public FilterBuilderViewModel(string filterName, IEnumerable<string> itemsSource)
	{
		FilterName = filterName;
		_possibleItems = new SortedSet<string>(itemsSource);
	}
}
