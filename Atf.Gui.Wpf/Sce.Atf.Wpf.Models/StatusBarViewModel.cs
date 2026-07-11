using System.ComponentModel;
using System.ComponentModel.Composition;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

[ExportViewModel("StatusBarViewModel")]
public class StatusBarViewModel : NotifyPropertyChangedBase
{
	private IStatusItem[] m_statusItems;

	private static readonly PropertyChangedEventArgs s_statusItemsArgs = ObservableUtil.CreateArgs((StatusBarViewModel x) => x.StatusItems);

	[ImportMany(AllowRecomposition = true)]
	public IStatusItem[] StatusItems
	{
		get
		{
			return m_statusItems;
		}
		set
		{
			m_statusItems = value;
			OnPropertyChanged(s_statusItemsArgs);
		}
	}
}
