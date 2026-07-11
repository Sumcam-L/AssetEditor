using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace Sce.Atf.Wpf.Controls;

public class CommonDialogHost : CommonDialogBase, IDialogContentHost
{
	public event EventHandler<HostClosingEventArgs> DialogClosing;

	public void RequestClose(bool? dialogResult)
	{
		if (ComponentDispatcher.IsThreadModal)
		{
			try
			{
				base.DialogResult = dialogResult;
				return;
			}
			catch (InvalidOperationException)
			{
				return;
			}
		}
		Close();
	}

	protected override void OnInitialized(EventArgs e)
	{
		if (IsOverridingWindowsChrome)
		{
			SetResourceReference(FrameworkElement.StyleProperty, typeof(CommonDialogHost));
		}
		base.OnInitialized(e);
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		if (!e.Cancel)
		{
			HostClosingEventArgs e2 = new HostClosingEventArgs
			{
				DialogResult = base.DialogResult
			};
			this.DialogClosing.Raise(this, e2);
			e.Cancel = e2.Cancel;
			if (!e.Cancel && base.Owner != null)
			{
				base.Owner.Focus();
			}
		}
	}

	static CommonDialogHost()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CommonDialogHost), new FrameworkPropertyMetadata(typeof(CommonDialogHost)));
	}

	bool? IDialogContentHost.ShowDialog()
	{
		return ShowDialog();
	}
}
