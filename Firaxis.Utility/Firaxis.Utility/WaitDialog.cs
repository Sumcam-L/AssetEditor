using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Firaxis.Utility;

public class WaitDialog : Form
{
	private Label lblMessage;

	private Action<WaitDialog> m_worker;

	private const int CP_NOCLOSE_BUTTON = 512;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ClassStyle |= 512;
			return createParams;
		}
	}

	public WaitDialog(string title)
		: this(title, ContentAlignment.MiddleCenter, null)
	{
	}

	public WaitDialog(string title, Action<WaitDialog> worker)
		: this(title, ContentAlignment.MiddleCenter, worker)
	{
	}

	public WaitDialog(string title, ContentAlignment msgAlign, Action<WaitDialog> worker)
	{
		InitializeComponent();
		lblMessage.TextAlign = msgAlign;
		Text = title;
		m_worker = worker;
		base.VisibleChanged += Dialog_VisibleChanged;
	}

	private void BeginInvokeIfNeeded(Action func)
	{
		if (!base.IsDisposed)
		{
			if (base.InvokeRequired)
			{
				BeginInvoke(func);
			}
			else
			{
				func();
			}
		}
	}

	public void SetTitle(string title)
	{
		BeginInvokeIfNeeded(delegate
		{
			if (!base.IsDisposed)
			{
				Text = title;
				Update();
			}
		});
	}

	public void SetAction(Action<WaitDialog> worker)
	{
		BeginInvokeIfNeeded(delegate
		{
			if (!base.IsDisposed)
			{
				m_worker = worker;
			}
		});
	}

	public void SetMessage(string msg)
	{
		BeginInvokeIfNeeded(delegate
		{
			if (!lblMessage.IsDisposed)
			{
				lblMessage.Text = msg;
				lblMessage.Update();
			}
		});
	}

	private void Dialog_VisibleChanged(object sender, EventArgs e)
	{
		if (!base.Visible)
		{
			return;
		}
		CenterToParent();
		Task.Factory.StartNew(delegate
		{
			m_worker?.Invoke(this);
			BeginInvoke((Action)delegate
			{
				if (!base.IsDisposed)
				{
					Close();
				}
			});
		});
	}

	private void InitializeComponent()
	{
		this.lblMessage = new System.Windows.Forms.Label();
		base.Controls.Add(this.lblMessage);
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		base.MinimizeBox = false;
		base.MaximizeBox = false;
		base.Name = "WaitDialog";
		base.Size = new System.Drawing.Size(480, 180);
		this.Font = new System.Drawing.Font("Segoe UI", 12f);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		this.lblMessage.Text = "";
		this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
		base.ResumeLayout(false);
	}
}
