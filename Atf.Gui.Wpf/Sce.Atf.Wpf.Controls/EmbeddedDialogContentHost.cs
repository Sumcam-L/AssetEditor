using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Sce.Atf.Wpf.Controls;

public class EmbeddedDialogContentHost : IDialogContentHost
{
	private readonly Control m_dialogContent;

	private bool? m_dialogResult;

	private DispatcherFrame m_frame;

	public Window Owner { get; set; }

	public event EventHandler<HostClosingEventArgs> DialogClosing;

	public EmbeddedDialogContentHost(IDialogContent content)
	{
		if (content == null)
		{
			throw new ArgumentException("context");
		}
		ContentControl dialogContent = new ContentControl
		{
			Content = content,
			Margin = new Thickness(12.0)
		};
		content.Host = this;
		m_dialogContent = dialogContent;
	}

	public bool? ShowDialog()
	{
		if (m_frame != null)
		{
			throw new InvalidOperationException("Already showing dialog");
		}
		if (!(Owner is IDialogSite dialogSite))
		{
			throw new InvalidOperationException("Owner must implement IDialogSite");
		}
		m_dialogResult = null;
		dialogSite.Site.Children.Add(m_dialogContent);
		dialogSite.ShowSite();
		m_frame = new DispatcherFrame();
		Dispatcher.PushFrame(m_frame);
		return m_dialogResult;
	}

	public void RequestClose(bool? dialogResult)
	{
		if (!(Owner is IDialogSite dialogSite))
		{
			throw new InvalidOperationException("");
		}
		HostClosingEventArgs e = new HostClosingEventArgs
		{
			DialogResult = dialogResult
		};
		this.DialogClosing.Raise(this, e);
		if (!e.Cancel)
		{
			dialogSite.HideSite();
			dialogSite.Site.Children.Remove(m_dialogContent);
			m_dialogResult = e.DialogResult;
			m_frame.Continue = false;
		}
	}
}
