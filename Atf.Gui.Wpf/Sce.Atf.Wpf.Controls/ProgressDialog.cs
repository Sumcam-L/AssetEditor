using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

public partial class ProgressDialog : CommonDialog, IComponentConnector
{
	private bool m_runWorkerComplete;

	private const int GWL_STYLE = -16;

	private const int WS_SYSMENU = 524288;

	public ProgressDialog()
	{
		InitializeComponent();
		base.Loaded += ProgressDialog_Loaded;
		base.DataContextChanged += ProgressDialog_DataContextChanged;
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		ProgressViewModel progressViewModel = base.DataContext as ProgressViewModel;
		bool? dialogResult = base.DialogResult;
		bool flag = false;
		if (dialogResult == true == flag && dialogResult.HasValue && !m_runWorkerComplete)
		{
			if (progressViewModel.Cancellable)
			{
				progressViewModel.CancelAsync();
			}
			e.Cancel = true;
		}
		else if (progressViewModel != null)
		{
			progressViewModel.RunWorkerCompleted -= DataContext_RunWorkerCompleted;
			m_runWorkerComplete = false;
		}
	}

	private void ProgressDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		m_runWorkerComplete = false;
		if (base.DataContext is ProgressViewModel progressViewModel)
		{
			progressViewModel.RunWorkerCompleted += DataContext_RunWorkerCompleted;
			TrySetCancellable();
		}
	}

	private void ProgressDialog_Loaded(object sender, RoutedEventArgs e)
	{
		TrySetCancellable();
	}

	private void DataContext_RunWorkerCompleted(object sender, EventArgs e)
	{
		ProgressViewModel progressViewModel = (ProgressViewModel)sender;
		m_runWorkerComplete = true;
		base.DialogResult = progressViewModel.Error == null && !progressViewModel.Cancelled;
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void TrySetCancellable()
	{
		if (base.DataContext is ProgressViewModel progressViewModel && base.IsLoaded && !progressViewModel.Cancellable)
		{
			IntPtr handle = new WindowInteropHelper(this).Handle;
			SetWindowLong(handle, -16, GetWindowLong(handle, -16) & -524289);
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


}
