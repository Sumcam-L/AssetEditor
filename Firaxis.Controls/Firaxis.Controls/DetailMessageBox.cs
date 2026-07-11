using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class DetailMessageBox : Form
{
	private IContainer components = null;

	private TextBox lblErrorMessage;

	private Button btnOk;

	public string Message
	{
		get
		{
			return lblErrorMessage.Text;
		}
		set
		{
			lblErrorMessage.Text = value;
		}
	}

	public DetailMessageBox()
	{
		InitializeComponent();
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
		this.lblErrorMessage = new System.Windows.Forms.TextBox();
		this.btnOk = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.lblErrorMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lblErrorMessage.BackColor = System.Drawing.SystemColors.Control;
		this.lblErrorMessage.Location = new System.Drawing.Point(12, 12);
		this.lblErrorMessage.Multiline = true;
		this.lblErrorMessage.Name = "lblErrorMessage";
		this.lblErrorMessage.ReadOnly = true;
		this.lblErrorMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.lblErrorMessage.Size = new System.Drawing.Size(438, 115);
		this.lblErrorMessage.TabIndex = 1;
		this.lblErrorMessage.TabStop = false;
		this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
		this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOk.Location = new System.Drawing.Point(196, 133);
		this.btnOk.Name = "btnOk";
		this.btnOk.Size = new System.Drawing.Size(71, 26);
		this.btnOk.TabIndex = 0;
		this.btnOk.Text = "OK";
		this.btnOk.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(463, 169);
		base.Controls.Add(this.btnOk);
		base.Controls.Add(this.lblErrorMessage);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(328, 123);
		base.Name = "DetailMessageBox";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Message";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
