using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class ThreadSafeProgressDialog : IDisposable
{
	private class BackgroundThread
	{
		private readonly ThreadSafeProgressDialog m_parent;

		private readonly Thread m_thread;

		private bool m_alreadyStopped;

		private ProgressDialog m_dialog;

		private ProgressBar m_progressBarControl;

		public BackgroundThread(ThreadSafeProgressDialog parent)
		{
			m_parent = parent;
			m_thread = new Thread(Run);
			m_thread.Name = "progress dialog";
			m_thread.IsBackground = true;
			m_thread.SetApartmentState(ApartmentState.STA);
			m_thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			m_thread.Start();
		}

		public void Stop()
		{
			lock (this)
			{
				if (m_dialog != null && m_dialog.IsHandleCreated)
				{
					m_dialog.BeginInvoke(new MethodInvoker(m_dialog.Close));
				}
				m_alreadyStopped = true;
			}
		}

		public void UpdateLabel()
		{
			lock (this)
			{
				if (m_dialog != null && m_dialog.IsHandleCreated)
				{
					m_dialog.BeginInvoke(new MethodInvoker(ThreadUnsafeUpdate));
				}
			}
		}

		public void UpdateLocation()
		{
			lock (this)
			{
				if (m_dialog != null && m_dialog.IsHandleCreated)
				{
					m_dialog.BeginInvoke(new MethodInvoker(ThreadUnsafeUpdateLocation));
				}
			}
		}

		private void Run()
		{
			try
			{
				lock (this)
				{
					if (!m_alreadyStopped)
					{
						m_dialog = new ProgressDialog();
						m_dialog.Cancelled += m_dialog_Cancelled;
						foreach (Control control in m_dialog.Controls)
						{
							if (control is ProgressBar progressBarControl)
							{
								m_progressBarControl = progressBarControl;
								break;
							}
						}
						ThreadUnsafeUpdate();
						m_dialog.Visible = true;
					}
				}
				if (!m_alreadyStopped)
				{
					Application.Run(m_dialog);
				}
			}
			finally
			{
				lock (this)
				{
					if (m_dialog != null)
					{
						bool visible = m_dialog.Visible;
						m_dialog.Dispose();
						m_dialog = null;
						m_progressBarControl = null;
						if (visible && m_parent.Owner != null)
						{
							m_parent.Owner.BeginInvoke(new MethodInvoker(m_parent.Show));
						}
					}
				}
			}
		}

		private void m_dialog_Cancelled(object sender, EventArgs e)
		{
			ThreadUnsafeUpdate();
		}

		private void ThreadUnsafeUpdate()
		{
			if (m_dialog != null)
			{
				string title = m_parent.m_title;
				if (title != null)
				{
					m_dialog.Title = title;
				}
				m_dialog.Label = m_parent.m_description;
				m_dialog.Percent = m_parent.m_percent;
				m_dialog.CanCancel = m_parent.CanCancel;
				m_dialog.CancelVisible = m_parent.CanCancel;
				m_progressBarControl.Style = ((!m_parent.m_marquee) ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee);
				if (m_dialog.IsCanceled)
				{
					m_parent.IsCanceled = true;
				}
			}
		}

		private void ThreadUnsafeUpdateLocation()
		{
			if (m_dialog != null)
			{
				if (m_parent.m_ownerBounds == null)
				{
					m_dialog.TopMost = false;
					return;
				}
				Rectangle rectangle = (Rectangle)m_parent.m_ownerBounds;
				Point location = rectangle.Location;
				location.Offset((rectangle.Width - m_dialog.Width) / 2, (rectangle.Height - m_dialog.Height) / 2);
				m_dialog.DesktopLocation = location;
				m_dialog.TopMost = true;
			}
		}
	}

	private Form m_owner = null;

	private volatile object m_ownerBounds;

	private volatile string m_title = null;

	private volatile string m_description = string.Empty;

	private volatile int m_percent;

	private BackgroundThread m_backgroundThread;

	private volatile bool m_marquee;

	private volatile bool m_isCanceled;

	private volatile bool m_canCancel;

	public Form Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			if (m_owner != value)
			{
				if (m_owner != null)
				{
					m_owner.Resize -= OnOwnerMove;
					m_owner.Move -= OnOwnerMove;
				}
				m_owner = value;
				if (m_owner != null)
				{
					m_owner.Resize += OnOwnerMove;
					m_owner.Move += OnOwnerMove;
				}
				OnOwnerMove(value, EventArgs.Empty);
			}
		}
	}

	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			if (m_title != value)
			{
				m_title = value;
				Show();
				if (m_backgroundThread != null)
				{
					m_backgroundThread.UpdateLabel();
				}
			}
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
			if (m_description != value)
			{
				m_description = value;
				Show();
				if (m_backgroundThread != null)
				{
					m_backgroundThread.UpdateLabel();
				}
			}
		}
	}

	public int Percent
	{
		get
		{
			if (m_marquee)
			{
				throw new InvalidOperationException("not allowed in Marquee mode");
			}
			return m_percent;
		}
		set
		{
			if (m_marquee)
			{
				throw new InvalidOperationException("not allowed in Marquee mode");
			}
			if (m_percent != value)
			{
				m_percent = value;
				Show();
				if (m_backgroundThread != null)
				{
					m_backgroundThread.UpdateLabel();
				}
			}
		}
	}

	public bool IsCanceled
	{
		get
		{
			return m_isCanceled;
		}
		set
		{
			m_isCanceled = value;
			if (m_isCanceled)
			{
				OnCancelled(EventArgs.Empty);
				Close();
			}
		}
	}

	public bool CanCancel
	{
		get
		{
			return m_canCancel;
		}
		set
		{
			if (m_canCancel != value)
			{
				m_canCancel = value;
				if (m_backgroundThread != null)
				{
					m_backgroundThread.UpdateLabel();
				}
			}
		}
	}

	public bool IsMarquee
	{
		get
		{
			return m_marquee;
		}
		set
		{
			if (m_marquee != value)
			{
				m_marquee = value;
				if (m_backgroundThread != null)
				{
					m_backgroundThread.UpdateLabel();
				}
			}
		}
	}

	public bool IsDialogVisible => m_backgroundThread != null;

	public event EventHandler Cancelled;

	public ThreadSafeProgressDialog()
		: this(show: true, marquee: false, canCancel: false)
	{
	}

	public ThreadSafeProgressDialog(bool show, bool canCancel)
		: this(show, marquee: false, canCancel)
	{
	}

	public ThreadSafeProgressDialog(bool show, bool marquee, bool canCancel)
	{
		m_marquee = marquee;
		m_canCancel = canCancel;
		if (show)
		{
			Show();
		}
	}

	void IDisposable.Dispose()
	{
		Close();
		Owner = null;
	}

	public void Show()
	{
		if (!IsDialogVisible)
		{
			if (m_backgroundThread == null)
			{
				m_backgroundThread = new BackgroundThread(this);
			}
			m_backgroundThread.UpdateLabel();
			if (m_isCanceled)
			{
				Close();
			}
		}
	}

	public void Close()
	{
		if (m_backgroundThread != null)
		{
			m_backgroundThread.Stop();
			m_backgroundThread = null;
		}
	}

	protected virtual void OnCancelled(EventArgs e)
	{
		this.Cancelled.Raise(this, e);
	}

	private void OnOwnerMove(object sender, EventArgs e)
	{
		if (m_owner != null)
		{
			m_ownerBounds = m_owner.DesktopBounds;
		}
		else
		{
			m_ownerBounds = null;
		}
		if (m_backgroundThread != null)
		{
			m_backgroundThread.UpdateLocation();
		}
	}
}
