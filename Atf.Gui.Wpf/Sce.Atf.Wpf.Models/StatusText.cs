using System.ComponentModel;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Models;

public class StatusText : StatusItem
{
	private string m_text;

	private Brush m_foreColor;

	private static readonly PropertyChangedEventArgs s_textArgs = ObservableUtil.CreateArgs((StatusText x) => x.Text);

	private static readonly PropertyChangedEventArgs s_foreColorArgs = ObservableUtil.CreateArgs((StatusText x) => x.ForeColor);

	public int MinWidth { get; private set; }

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
			OnPropertyChanged(s_textArgs);
		}
	}

	public Brush ForeColor
	{
		get
		{
			return m_foreColor;
		}
		set
		{
			m_foreColor = value;
			OnPropertyChanged(s_foreColorArgs);
		}
	}

	public StatusText(int minWidth)
	{
		MinWidth = minWidth;
	}

	public StatusText(string text, int minWidth)
	{
		m_text = text;
		MinWidth = minWidth;
	}
}
