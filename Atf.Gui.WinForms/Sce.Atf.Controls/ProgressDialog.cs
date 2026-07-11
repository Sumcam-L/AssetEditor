using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class ProgressDialog : Form
{
	private ProgressBar progressBar1;

	private Label label1;

	private Button cancelButton;

	private bool m_canceled;

	private bool m_canCancel;

	public bool IsCanceled
	{
		get
		{
			return m_canceled;
		}
		set
		{
			m_canceled = value;
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
			m_canCancel = value;
			cancelButton.Enabled = m_canCancel;
		}
	}

	public bool CancelVisible
	{
		get
		{
			return cancelButton.Visible;
		}
		set
		{
			cancelButton.Visible = value;
		}
	}

	public int Percent
	{
		get
		{
			return progressBar1.Value;
		}
		set
		{
			progressBar1.Value = value;
		}
	}

	public string Label
	{
		get
		{
			return label1.Text;
		}
		set
		{
			label1.Text = value;
		}
	}

	public string Title
	{
		get
		{
			return Text;
		}
		set
		{
			Text = value;
		}
	}

	public event EventHandler Cancelled;

	public ProgressDialog()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.ProgressDialog));
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.label1 = new System.Windows.Forms.Label();
		this.cancelButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		resources.ApplyResources(this.progressBar1, "progressBar1");
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Step = 5;
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.cancelButton, "cancelButton");
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Click += new System.EventHandler(OnCancelled);
		resources.ApplyResources(this, "$this");
		base.CausesValidation = false;
		base.ControlBox = false;
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.progressBar1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ProgressDialog";
		base.ShowInTaskbar = false;
		base.TopMost = false;
		base.ResumeLayout(false);
	}

	protected virtual void OnCancelled(object sender, EventArgs e)
	{
		m_canceled = true;
		this.Cancelled.Raise(this, e);
	}
}
