using System.ComponentModel;

namespace Sce.Atf.Wpf.Models;

public class StatusImage : StatusItem
{
	private object m_imageKey;

	private static readonly PropertyChangedEventArgs s_imageKeyArgs = ObservableUtil.CreateArgs((StatusImage x) => x.ImageSourceKey);

	public object ImageSourceKey
	{
		get
		{
			return m_imageKey;
		}
		set
		{
			m_imageKey = value;
			OnPropertyChanged(s_imageKeyArgs);
		}
	}
}
