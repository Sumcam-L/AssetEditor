using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

public class ProgressViewModel : DialogViewModelBase, IStatusItem
{
	private bool m_isIndeterminate;

	private static readonly PropertyChangedEventArgs s_isIndeterminateArgs = ObservableUtil.CreateArgs((ProgressViewModel x) => x.IsIndeterminate);

	private string m_description;

	private static readonly PropertyChangedEventArgs s_descriptionArgs = ObservableUtil.CreateArgs((ProgressViewModel x) => x.Description);

	private object m_content;

	private static readonly PropertyChangedEventArgs s_contentArgs = ObservableUtil.CreateArgs((ProgressViewModel x) => x.Content);

	private int m_progress;

	private static readonly PropertyChangedEventArgs s_progressArgs = ObservableUtil.CreateArgs((ProgressViewModel x) => x.Progress);

	private readonly BackgroundWorker m_worker = new BackgroundWorker();

	private CultureInfo m_uiCulture;

	private DoWorkEventHandler m_workerCallback;

	public bool IsIndeterminate
	{
		get
		{
			return m_isIndeterminate;
		}
		set
		{
			m_isIndeterminate = value;
			OnPropertyChanged(s_isIndeterminateArgs);
		}
	}

	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			m_description = value;
			OnPropertyChanged(s_descriptionArgs);
		}
	}

	public object Content
	{
		get
		{
			return m_content;
		}
		set
		{
			m_content = value;
			OnPropertyChanged(s_contentArgs);
		}
	}

	public bool Cancellable { get; set; }

	public bool Cancelled { get; private set; }

	public object Result { get; private set; }

	public Exception Error { get; private set; }

	public object Tag { get; set; }

	public int Progress
	{
		get
		{
			return m_progress;
		}
		private set
		{
			m_progress = value;
			OnPropertyChanged(s_progressArgs);
		}
	}

	public event EventHandler RunWorkerCompleted;

	public ProgressViewModel()
	{
		m_worker.WorkerReportsProgress = true;
		m_worker.WorkerSupportsCancellation = true;
		m_worker.DoWork += worker_DoWork;
		m_worker.ProgressChanged += worker_ProgressChanged;
		m_worker.RunWorkerCompleted += worker_RunWorkerCompleted;
		m_isIndeterminate = true;
		m_description = "Please wait...".Localize();
		base.Title = "Progress".Localize();
	}

	public void RunWorkerThread(DoWorkEventHandler workerCallback)
	{
		RunWorkerThread(null, workerCallback);
	}

	public void RunWorkerThread(object argument, DoWorkEventHandler workerCallback)
	{
		m_uiCulture = CultureInfo.CurrentUICulture;
		m_workerCallback = workerCallback;
		m_worker.RunWorkerAsync(argument);
	}

	public void CancelAsync()
	{
		if (Cancellable)
		{
			Cancelled = true;
			m_worker.CancelAsync();
		}
	}

	private void worker_DoWork(object sender, DoWorkEventArgs e)
	{
		try
		{
			Thread.CurrentThread.CurrentUICulture = m_uiCulture;
			m_workerCallback(sender, e);
			m_workerCallback = null;
		}
		catch (Exception)
		{
			Cancelled = true;
			throw;
		}
	}

	private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		if (e.ProgressPercentage == int.MaxValue)
		{
			IsIndeterminate = true;
		}
		else if (e.ProgressPercentage != int.MinValue)
		{
			Progress = e.ProgressPercentage;
		}
		Content = e.UserState;
	}

	private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (e.Error != null)
		{
			Error = e.Error;
		}
		else if (!e.Cancelled)
		{
			Result = e.Result;
		}
		Progress = 100;
		Cancelled = Cancelled || e.Cancelled;
		this.RunWorkerCompleted.Raise(this, EventArgs.Empty);
	}
}
