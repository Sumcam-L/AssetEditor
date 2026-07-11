using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class UnhandledExceptionDialog : Form
{
	private IContainer components = null;

	private Label label1;

	private Label label2;

	private Button ContinueBtn;

	private Button QuitBtn;

	public TextBox ExceptionTextBox;

	public UnhandledExceptionDialog()
	{
		InitializeComponent();
	}

	public void HideContinueButton()
	{
		QuitBtn.Left = ContinueBtn.Left;
		ContinueBtn.Visible = false;
		label2.Visible = false;
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (keyData == Keys.Return || keyData == Keys.Escape)
		{
			return true;
		}
		return base.ProcessDialogKey(keyData);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.UnhandledExceptionDialog));
		this.ExceptionTextBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.ContinueBtn = new System.Windows.Forms.Button();
		this.QuitBtn = new System.Windows.Forms.Button();
		base.SuspendLayout();
		resources.ApplyResources(this.ExceptionTextBox, "ExceptionTextBox");
		this.ExceptionTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
		this.ExceptionTextBox.Name = "ExceptionTextBox";
		this.ExceptionTextBox.ReadOnly = true;
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.ContinueBtn, "ContinueBtn");
		this.ContinueBtn.DialogResult = System.Windows.Forms.DialogResult.Yes;
		this.ContinueBtn.Name = "ContinueBtn";
		this.ContinueBtn.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.QuitBtn, "QuitBtn");
		this.QuitBtn.DialogResult = System.Windows.Forms.DialogResult.No;
		this.QuitBtn.Name = "QuitBtn";
		this.QuitBtn.UseVisualStyleBackColor = true;
		base.AcceptButton = this.ContinueBtn;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.QuitBtn;
		base.Controls.Add(this.QuitBtn);
		base.Controls.Add(this.ContinueBtn);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.ExceptionTextBox);
		base.Name = "UnhandledExceptionDialog";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
