using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Asset;

public class SourceControlPassword : Form
{
	private IContainer components = null;

	private Label userServerText;

	private TextBox txtPassword;

	private Button btnOK;

	private Button btnCancel;

	public string Password
	{
		get
		{
			return txtPassword.Text;
		}
		set
		{
			txtPassword.Text = Password;
		}
	}

	public SourceControlPassword()
	{
		InitializeComponent();
	}

	public void SetConnection(string szUser, string szClient)
	{
		userServerText.Text = $"A password is required for user '{szUser}' on server '{szClient}'";
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
		this.userServerText = new System.Windows.Forms.Label();
		this.txtPassword = new System.Windows.Forms.TextBox();
		this.btnOK = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.userServerText.AutoSize = true;
		this.userServerText.Location = new System.Drawing.Point(12, 9);
		this.userServerText.Name = "userServerText";
		this.userServerText.Size = new System.Drawing.Size(232, 13);
		this.userServerText.TabIndex = 0;
		this.userServerText.Text = "A password is required for user {0} on server {1}";
		this.txtPassword.Location = new System.Drawing.Point(15, 25);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.Size = new System.Drawing.Size(349, 20);
		this.txtPassword.TabIndex = 0;
		this.txtPassword.UseSystemPasswordChar = true;
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Location = new System.Drawing.Point(289, 51);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(75, 23);
		this.btnOK.TabIndex = 1;
		this.btnOK.Text = "OK";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(208, 51);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		base.AcceptButton = this.btnOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(376, 83);
		base.ControlBox = false;
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.txtPassword);
		base.Controls.Add(this.userServerText);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SourceControlPassword";
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Source Control Password";
		base.TopMost = true;
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
