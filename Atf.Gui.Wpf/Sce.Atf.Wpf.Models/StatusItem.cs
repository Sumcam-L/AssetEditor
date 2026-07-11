using System.ComponentModel;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

public class StatusItem : NotifyPropertyChangedBase, IStatusItem
{
	private bool m_isLeftDock;

	private string m_toolTip;

	private static readonly PropertyChangedEventArgs s_isLeftDockArgs = ObservableUtil.CreateArgs((StatusItem x) => x.IsLeftDock);

	private static readonly PropertyChangedEventArgs s_toolTipArgs = ObservableUtil.CreateArgs((StatusItem x) => x.ToolTip);

	public bool IsLeftDock
	{
		get
		{
			return m_isLeftDock;
		}
		set
		{
			m_isLeftDock = value;
			OnPropertyChanged(s_isLeftDockArgs);
		}
	}

	public string ToolTip
	{
		get
		{
			return m_toolTip;
		}
		set
		{
			m_toolTip = value;
			OnPropertyChanged(s_toolTipArgs);
		}
	}
}
