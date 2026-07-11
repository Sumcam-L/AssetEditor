using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IStatusService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StatusService : IStatusService, IInitializable
{
	private struct StatusBarProgressContext
	{
		public readonly EventHandler<ProgressCompleteEventArgs> ProgressComplete;

		public readonly ComposablePart StatusItemPart;

		public StatusBarProgressContext(EventHandler<ProgressCompleteEventArgs> progressComplete, ComposablePart statusItemPart)
		{
			ProgressComplete = progressComplete;
			StatusItemPart = statusItemPart;
		}
	}

	[Export(typeof(IStatusItem))]
	private StatusText m_mainStatusText = new StatusText(100);

	[Import]
	private IComposer m_composer = null;

	public Exception ProgressError { get; private set; }

	public object ProgressResult { get; private set; }

	public void Initialize()
	{
		m_mainStatusText.IsLeftDock = true;
		ShowStatus("Ready".Localize("Application is ready"));
	}

	public void ShowStatus(string status)
	{
		m_mainStatusText.Text = status;
	}

	public bool RunProgressDialog(string message, bool canCancel, object argument, DoWorkEventHandler workHandler, bool autoIncrement)
	{
		ProgressViewModel progressViewModel = new ProgressViewModel
		{
			Cancellable = canCancel,
			Description = message,
			IsIndeterminate = autoIncrement
		};
		progressViewModel.RunWorkerThread(argument, workHandler);
		DialogUtils.ShowDialogWithViewModel<ProgressDialog>(progressViewModel);
		ProgressResult = progressViewModel.Result;
		ProgressError = progressViewModel.Error;
		return progressViewModel.Cancelled;
	}

	public void RunProgressInStatusBarAsync(string message, object argument, DoWorkEventHandler workHandler, EventHandler<ProgressCompleteEventArgs> progressCompleteHandler, bool autoIncrement)
	{
		ProgressViewModel progressViewModel = new ProgressViewModel
		{
			Cancellable = false,
			Description = message,
			IsIndeterminate = autoIncrement
		};
		ComposablePart statusItemPart = m_composer.AddPart(progressViewModel);
		progressViewModel.Tag = new StatusBarProgressContext(progressCompleteHandler, statusItemPart);
		progressViewModel.RunWorkerThread(argument, workHandler);
		progressViewModel.RunWorkerCompleted += statusItem_RunWorkerCompleted;
	}

	private void statusItem_RunWorkerCompleted(object sender, EventArgs e)
	{
		ProgressViewModel progressViewModel = (ProgressViewModel)sender;
		StatusBarProgressContext statusBarProgressContext = (StatusBarProgressContext)progressViewModel.Tag;
		m_composer.RemovePart(statusBarProgressContext.StatusItemPart);
		ProgressCompleteEventArgs e2 = new ProgressCompleteEventArgs(progressViewModel.Error, progressViewModel.Result, progressViewModel.Cancelled);
		statusBarProgressContext.ProgressComplete.Raise(this, e2);
		progressViewModel.RunWorkerCompleted -= statusItem_RunWorkerCompleted;
	}
}
