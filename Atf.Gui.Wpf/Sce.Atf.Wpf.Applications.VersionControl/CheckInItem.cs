using System.ComponentModel;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications.VersionControl;

internal class CheckInItem : NotifyPropertyChangedBase
{
	private readonly CheckInViewModel m_parent;

	private bool m_isChecked = true;

	private static readonly PropertyChangedEventArgs IsCheckedArgs = ObservableUtil.CreateArgs((CheckInItem x) => x.IsChecked);

	private bool m_isSelected;

	private static readonly PropertyChangedEventArgs IsSelectedArgs = ObservableUtil.CreateArgs((CheckInItem x) => x.IsSelected);

	public IResource Resource { get; private set; }

	public string LocalPath => Resource.Uri.LocalPath;

	public bool IsChecked
	{
		get
		{
			return m_isChecked;
		}
		set
		{
			if (m_isChecked != value)
			{
				m_isChecked = value;
				OnPropertyChanged(IsCheckedArgs);
				m_parent.CheckAllSelected(value);
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			if (m_isSelected != value)
			{
				m_isSelected = value;
				OnPropertyChanged(IsSelectedArgs);
			}
		}
	}

	public CheckInItem(CheckInViewModel parent, IResource resource)
	{
		m_parent = parent;
		Resource = resource;
	}
}
