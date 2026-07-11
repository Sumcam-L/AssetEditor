using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class ConfirmationDialog : Form
{
	private Button m_cancelButton;

	private Button m_yesButton;

	private TextBox m_textBox;

	private Button m_noButton;

	public string YesButtonText
	{
		get
		{
			return m_yesButton.Text;
		}
		set
		{
			m_yesButton.Text = value;
		}
	}

	public string NoButtonText
	{
		get
		{
			return m_noButton.Text;
		}
		set
		{
			m_noButton.Text = value;
		}
	}

	public string CancelButtonText
	{
		get
		{
			return m_cancelButton.Text;
		}
		set
		{
			m_cancelButton.Text = value;
		}
	}

	public ConfirmationDialog(string title, string message)
	{
		InitializeComponent();
		base.CancelButton = m_cancelButton;
		Text = title;
		m_textBox.Text = message;
	}

	public void HideCancelButton()
	{
		m_cancelButton.Hide();
		int num = m_cancelButton.Location.X - (m_noButton.Location.X + m_noButton.Width);
		int num2 = m_cancelButton.Width + num;
		m_noButton.Location = new Point(m_noButton.Location.X + num2, m_noButton.Location.Y);
		m_yesButton.Location = new Point(m_yesButton.Location.X + num2, m_yesButton.Location.Y);
	}

	public void ResizeContent(int inWidthDelta, int inHeightDelta)
	{
		base.Size = new Size(base.Width + inWidthDelta, base.Height + inHeightDelta);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.ConfirmationDialog));
		this.m_yesButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_noButton = new System.Windows.Forms.Button();
		this.m_textBox = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		resources.ApplyResources(this.m_yesButton, "m_yesButton");
		this.m_yesButton.AutoEllipsis = true;
		this.m_yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
		this.m_yesButton.Name = "m_yesButton";
		resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
		this.m_cancelButton.AutoEllipsis = true;
		this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelButton.Name = "m_cancelButton";
		resources.ApplyResources(this.m_noButton, "m_noButton");
		this.m_noButton.AutoEllipsis = true;
		this.m_noButton.DialogResult = System.Windows.Forms.DialogResult.No;
		this.m_noButton.Name = "m_noButton";
		resources.ApplyResources(this.m_textBox, "m_textBox");
		this.m_textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_textBox.Name = "m_textBox";
		this.m_textBox.ReadOnly = true;
		resources.ApplyResources(this, "$this");
		base.ControlBox = false;
		base.Controls.Add(this.m_textBox);
		base.Controls.Add(this.m_noButton);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_yesButton);
		base.Name = "ConfirmationDialog";
		base.ShowInTaskbar = false;
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
