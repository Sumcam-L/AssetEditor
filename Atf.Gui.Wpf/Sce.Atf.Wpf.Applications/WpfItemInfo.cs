using System.Drawing;
using System.Windows;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public class WpfItemInfo : ItemInfo
{
	private bool? m_checkState;

	private bool m_isEnabled = true;

	private bool m_isVisible = true;

	public override bool Checked
	{
		get
		{
			return m_checkState == true;
		}
		set
		{
			m_checkState = value;
		}
	}

	public bool? CheckState
	{
		get
		{
			return m_checkState;
		}
		set
		{
			m_checkState = value;
		}
	}

	public object ImageKey { get; set; }

	public object StateImageKey { get; set; }

	public FontWeight FontWeight
	{
		get
		{
			return ((base.FontStyle & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold) ? FontWeights.Bold : FontWeights.Normal;
		}
		set
		{
			if (value == FontWeights.Bold)
			{
				base.FontStyle |= System.Drawing.FontStyle.Bold;
			}
			else
			{
				base.FontStyle &= ~System.Drawing.FontStyle.Bold;
			}
		}
	}

	public System.Windows.FontStyle FontItalicStyle
	{
		get
		{
			return ((base.FontStyle & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic) ? FontStyles.Italic : FontStyles.Normal;
		}
		set
		{
			if (value == FontStyles.Italic)
			{
				base.FontStyle |= System.Drawing.FontStyle.Italic;
			}
			else
			{
				base.FontStyle &= ~System.Drawing.FontStyle.Italic;
			}
		}
	}

	public object OverlayImageKey { get; set; }

	public bool IsEnabled
	{
		get
		{
			return m_isEnabled;
		}
		set
		{
			m_isEnabled = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			return m_isVisible;
		}
		set
		{
			m_isVisible = value;
		}
	}
}
