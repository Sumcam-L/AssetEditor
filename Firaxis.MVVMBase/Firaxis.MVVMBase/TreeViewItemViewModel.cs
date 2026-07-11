namespace Firaxis.MVVMBase;

public abstract class TreeViewItemViewModel : Notifier
{
	private bool _isExpanded;

	private bool _isSelected;

	public bool IsExpanded
	{
		get
		{
			return _isExpanded;
		}
		set
		{
			if (_isExpanded != value)
			{
				_isExpanded = value;
				OnPropertyChanged("IsExpanded");
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				OnPropertyChanged("IsSelected");
			}
		}
	}

	public TreeViewItemViewModel()
	{
		IsExpanded = true;
		IsSelected = false;
	}
}
