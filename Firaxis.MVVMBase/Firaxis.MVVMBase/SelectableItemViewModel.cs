namespace Firaxis.MVVMBase;

public class SelectableItemViewModel : BaseViewModel
{
	private bool _isSelected;

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

	public SelectableItemViewModel()
	{
		IsSelected = false;
	}

	public SelectableItemViewModel(bool isSel)
	{
		IsSelected = isSel;
	}
}
