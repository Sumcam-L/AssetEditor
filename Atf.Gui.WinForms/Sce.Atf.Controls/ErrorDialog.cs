using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class ErrorDialog : Form
{
	private string m_messageId;

	private IContainer components = null;

	private Button okButton;

	private TextBox textBox1;

	private CheckBox checkBox1;

	public string MessageId
	{
		get
		{
			return m_messageId;
		}
		set
		{
			m_messageId = value;
		}
	}

	public string Message
	{
		get
		{
			return textBox1.Text;
		}
		set
		{
			textBox1.Text = value;
		}
	}

	public bool SuppressMessage
	{
		get
		{
			return checkBox1.Checked;
		}
		set
		{
			checkBox1.Checked = value;
		}
	}

	public bool ShowSupressMessageCheckbox
	{
		get
		{
			return checkBox1.Visible;
		}
		set
		{
			checkBox1.Visible = value;
		}
	}

	public event EventHandler SuppressMessageClicked;

	public ErrorDialog()
	{
		InitializeComponent();
		okButton.Click += okButton_Click;
		checkBox1.Click += checkBox1_Click;
	}

	public ErrorDialog(string message, string caption, Exception exception)
		: this()
	{
		Text = caption;
		StringBuilder stringBuilder = new StringBuilder(message);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine(exception.ToString());
		textBox1.Text = stringBuilder.ToString();
		checkBox1.Visible = false;
		textBox1.ScrollBars = ScrollBars.Both;
	}

	public static DialogResult Show(string message, string caption, Exception exception)
	{
		ErrorDialog errorDialog = new ErrorDialog(message, caption, exception);
		return errorDialog.ShowDialog();
	}

	private void okButton_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void checkBox1_Click(object sender, EventArgs e)
	{
		this.SuppressMessageClicked.Raise(this, EventArgs.Empty);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.ErrorDialog));
		this.okButton = new System.Windows.Forms.Button();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		resources.ApplyResources(this.okButton, "okButton");
		this.okButton.Name = "okButton";
		this.okButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.textBox1, "textBox1");
		this.textBox1.BackColor = System.Drawing.SystemColors.Control;
		this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.textBox1.Name = "textBox1";
		resources.ApplyResources(this.checkBox1, "checkBox1");
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.UseVisualStyleBackColor = true;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.checkBox1);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.textBox1);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ErrorDialog";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
