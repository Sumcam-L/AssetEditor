using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(IStatusService))]
[Export(typeof(IInitializable))]
[Export(typeof(StatusService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StatusService : IStatusService, IInitializable
{
	private class TextPanel : ToolStripStatusLabel, IStatusText
	{
		public TextPanel(int width)
		{
			DisplayStyle = ToolStripItemDisplayStyle.Text;
			base.Width = width;
			base.AutoSize = false;
			TextAlign = ContentAlignment.MiddleLeft;
			base.BorderSides = ToolStripStatusLabelBorderSides.All;
		}
	}

	private class ImagePanel : ToolStripStatusLabel, IStatusImage
	{
		public ImagePanel()
		{
			DisplayStyle = ToolStripItemDisplayStyle.Image;
			base.AutoSize = false;
		}
	}

	private readonly Form m_mainForm;

	private ToolStripContainer m_toolStripContainer;

	private readonly StatusStrip m_statusStrip = new StatusStrip();

	private readonly ToolStripStatusLabel m_mainPanel;

	private double m_progress;

	private double m_autoIncrement;

	private readonly System.Threading.Timer m_progressTimer;

	private readonly ThreadSafeProgressDialog m_progressDialog;

	private const int ProgressInterval = 250;

	private static int s_controlCount;

	public Control StatusControl => m_statusStrip;

	public event EventHandler ProgressCancelled;

	[ImportingConstructor]
	public StatusService(Form mainForm)
	{
		m_mainForm = mainForm;
		m_statusStrip = new StatusStrip();
		m_statusStrip.Name = "StatusBar";
		m_statusStrip.Dock = DockStyle.Bottom;
		m_statusStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
		m_statusStrip.ShowItemToolTips = false;
		m_mainPanel = new ToolStripStatusLabel();
		m_mainPanel.Width = 256;
		m_mainPanel.AutoSize = true;
		m_mainPanel.Spring = true;
		m_mainPanel.TextAlign = ContentAlignment.MiddleLeft;
		m_statusStrip.Items.Add(m_mainPanel);
		m_progressTimer = new System.Threading.Timer(progressCallback, this, -1, 250);
		m_progressDialog = new ThreadSafeProgressDialog(show: false, canCancel: true);
		m_progressDialog.Cancelled += progressDialog_Cancelled;
	}

	void IInitializable.Initialize()
	{
		foreach (Control control in m_mainForm.Controls)
		{
			m_toolStripContainer = control as ToolStripContainer;
			if (m_toolStripContainer != null)
			{
				m_toolStripContainer.BottomToolStripPanel.Controls.Add(m_statusStrip);
				break;
			}
		}
		if (m_toolStripContainer == null)
		{
			m_mainForm.Controls.Add(m_statusStrip);
		}
		ShowStatus("Ready".Localize("Application is ready"));
	}

	public void ShowStatus(string status)
	{
		if (string.IsNullOrEmpty(status))
		{
			status = "Ready".Localize();
		}
		m_mainPanel.Text = status;
	}

	public IStatusText AddText(int width)
	{
		TextPanel textPanel = new TextPanel(width);
		textPanel.Name = "$Status" + s_controlCount++;
		m_statusStrip.Items.Add(textPanel);
		return textPanel;
	}

	public IStatusImage AddImage()
	{
		ImagePanel imagePanel = new ImagePanel();
		imagePanel.Name = "$Status" + s_controlCount++;
		m_statusStrip.Items.Add(imagePanel);
		return imagePanel;
	}

	public void BeginProgress(string message)
	{
		BeginProgress(message, 0, canCancel: true);
	}

	public void BeginProgress(string message, int expectedDuration)
	{
		BeginProgress(message, expectedDuration, canCancel: true);
	}

	public void BeginProgress(string message, bool canCancel)
	{
		BeginProgress(message, 0, canCancel);
	}

	public void BeginProgress(string message, int expectedDuration, bool canCancel)
	{
		m_autoIncrement = ((expectedDuration == 0) ? 0.0 : (250.0 / (double)expectedDuration));
		m_progress = 0.0;
		m_progressDialog.IsCanceled = false;
		m_progressDialog.CanCancel = canCancel;
		m_progressDialog.Description = message;
		m_progressTimer.Change(250, 250);
	}

	public void ShowProgress(double progress)
	{
		m_progress = progress;
	}

	public void EndProgress()
	{
		m_progress = 0.0;
		m_progressTimer.Change(-1, 250);
		m_progressDialog.Close();
	}

	private void progressCallback(object state)
	{
		lock (this)
		{
			if (m_progress < 1.0)
			{
				if (!m_progressDialog.IsDialogVisible)
				{
					m_progressDialog.Show();
				}
				int num = (int)(100.0 * m_progress);
				if (num < 0)
				{
					num = 0;
				}
				else if (num > 100)
				{
					num = 100;
				}
				m_progressDialog.Percent = num;
				m_progress += m_autoIncrement;
			}
			else
			{
				EndProgress();
			}
		}
	}

	private void progressDialog_Cancelled(object sender, EventArgs e)
	{
		this.ProgressCancelled.Raise(this, e);
	}
}
