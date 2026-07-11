using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(UnhandledExceptionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class UnhandledExceptionService : IInitializable
{
	[Import(AllowDefault = true)]
	private IUserFeedbackService m_userFeedbackService = null;

	public void Initialize()
	{
		System.Windows.Forms.Application.ThreadException += Application_ThreadException;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		System.Windows.Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
	}

	protected virtual bool? ShowExceptionDialog(Exception exception)
	{
		Sce.Atf.Wpf.Controls.UnhandledExceptionDialog unhandledExceptionDialog = new Sce.Atf.Wpf.Controls.UnhandledExceptionDialog();
		unhandledExceptionDialog.DataContext = new UnhandledExceptionViewModel(exception.Message);
		if (System.Windows.Application.Current.MainWindow.IsVisible)
		{
			unhandledExceptionDialog.Owner = System.Windows.Application.Current.MainWindow;
		}
		return unhandledExceptionDialog.ShowDialog();
	}

	private void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
	{
		Exception exception = e.Exception;
		ShowExceptionDialogInternal(exception);
	}

	private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Exception exception = new Exception(e.ExceptionObject.ToString());
		ShowExceptionDialogInternal(exception);
	}

	private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		ShowExceptionDialogInternal(e.Exception);
		e.Handled = true;
	}

	private void ShowExceptionDialogInternal(Exception exception)
	{
		bool? result = null;
		try
		{
			if (!(System.Windows.Application.Current is AtfApp { IsShuttingDown: false } atfApp))
			{
				return;
			}
			atfApp.Dispatcher.InvokeIfRequired(delegate
			{
				result = ShowExceptionDialog(exception);
				if (m_userFeedbackService != null)
				{
					m_userFeedbackService.ShowFeedbackForm();
				}
			});
		}
		finally
		{
			if (result.HasValue && !result.Value)
			{
				Environment.Exit(1);
			}
		}
	}
}
