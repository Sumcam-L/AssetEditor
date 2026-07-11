using System;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Docking;

public class DockContent : IDockContent, INotifyPropertyChanged
{
	private string m_header = string.Empty;

	private object m_icon;

	private bool m_isVisible;

	private bool m_isFocused;

	internal ContentSettings Settings { get; set; }

	public string Header
	{
		get
		{
			return m_header;
		}
		set
		{
			if (m_header != value)
			{
				m_header = ((value != null) ? value : string.Empty);
				NotifyPropertyChanged("Header");
			}
		}
	}

	public object Icon
	{
		get
		{
			return m_icon;
		}
		set
		{
			if (m_icon != value)
			{
				m_icon = value;
				NotifyPropertyChanged("Icon");
			}
		}
	}

	public string UID { get; private set; }

	public bool IsVisible
	{
		get
		{
			return m_isVisible;
		}
		internal set
		{
			if (m_isVisible != value)
			{
				m_isVisible = value;
				NotifyPropertyChanged("IsVisible");
				if (this.IsVisibleChanged != null)
				{
					this.IsVisibleChanged(this, new BooleanArgs(m_isVisible));
				}
			}
		}
	}

	public bool IsFocused
	{
		get
		{
			return m_isFocused;
		}
		internal set
		{
			if (m_isFocused != value)
			{
				m_isFocused = value;
				NotifyPropertyChanged("IsFocused");
				if (this.IsFocusedChanged != null)
				{
					this.IsFocusedChanged(this, new BooleanArgs(m_isFocused));
				}
			}
		}
	}

	public object Content { get; private set; }

	public event PropertyChangedEventHandler PropertyChanged;

	public event EventHandler<BooleanArgs> IsVisibleChanged;

	public event EventHandler<BooleanArgs> IsFocusedChanged;

	public event ContentClosedEvent Closing;

	public DockContent(object content, string uid)
	{
		Content = content;
		UID = uid;
	}

	public DockContent(object content, string uid, string header)
		: this(content, uid)
	{
		Header = header;
	}

	public DockContent(object content, string uid, string header, object icon)
		: this(content, uid, header)
	{
		Icon = icon;
	}

	public void NotifyPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public void OnClose(object sender, ContentClosedEventArgs args)
	{
		if (this.Closing != null)
		{
			this.Closing(this, args);
		}
	}
}
