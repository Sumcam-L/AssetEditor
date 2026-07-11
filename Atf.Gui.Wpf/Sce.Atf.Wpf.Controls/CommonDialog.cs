using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

public class CommonDialog : CommonDialogBase
{
	private IDialogViewModel m_viewModel;

	private bool m_closing;

	public CommonDialog()
	{
		base.DataContextChanged += CommonDialog_DataContextChanged;
	}

	protected override void OnInitialized(EventArgs e)
	{
		if (IsOverridingWindowsChrome)
		{
			SetResourceReference(FrameworkElement.StyleProperty, typeof(CommonDialog));
		}
		base.OnInitialized(e);
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		IDialogViewModel viewModel = m_viewModel;
		if (!m_closing && viewModel != null && viewModel.CancelCommand != null)
		{
			if (!viewModel.CancelCommand.CanExecute(null))
			{
				e.Cancel = true;
			}
			else
			{
				try
				{
					m_closing = true;
					viewModel.CancelCommand.Execute(null);
					if (ComponentDispatcher.IsThreadModal)
					{
						base.DialogResult = e.Cancel;
					}
				}
				finally
				{
					m_closing = false;
				}
			}
		}
		if (!e.Cancel && base.Owner != null)
		{
			base.Owner.Focus();
		}
	}

	static CommonDialog()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CommonDialog), new FrameworkPropertyMetadata(typeof(CommonDialog)));
	}

	private void CommonDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (m_viewModel != null)
		{
			m_viewModel.CloseDialog -= ViewModel_CloseDialog;
		}
		m_viewModel = base.DataContext as IDialogViewModel;
		if (m_viewModel != null)
		{
			m_viewModel.CloseDialog += ViewModel_CloseDialog;
			SetBinding(Window.TitleProperty, new Binding("Title")
			{
				Source = base.DataContext
			});
		}
	}

	private void ViewModel_CloseDialog(object sender, CloseDialogEventArgs e)
	{
		if (m_closing)
		{
			return;
		}
		try
		{
			m_closing = true;
			base.DataContext = null;
			if (ComponentDispatcher.IsThreadModal)
			{
				try
				{
					base.DialogResult = e.DialogResult;
				}
				catch (InvalidOperationException)
				{
				}
			}
			Close();
		}
		finally
		{
			m_closing = false;
		}
	}
}
