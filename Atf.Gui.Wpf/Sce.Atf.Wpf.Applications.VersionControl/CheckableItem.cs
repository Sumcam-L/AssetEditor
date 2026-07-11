using System;
using System.ComponentModel;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications.VersionControl;

internal class CheckableItem : NotifyPropertyChangedBase
{
	private readonly ReconcileViewModel m_parent;

	private bool m_isChecked = true;

	private static readonly PropertyChangedEventArgs IsCheckedArgs = ObservableUtil.CreateArgs((CheckableItem x) => x.IsChecked);

	private bool m_isSelected;

	private static readonly PropertyChangedEventArgs IsSelectedArgs = ObservableUtil.CreateArgs((CheckableItem x) => x.IsSelected);

	public Uri Uri { get; private set; }

	public string LocalPath => Uri.LocalPath;

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

	public CheckableItem(ReconcileViewModel parent, Uri uri)
	{
		m_parent = parent;
		Uri = uri;
	}
}
