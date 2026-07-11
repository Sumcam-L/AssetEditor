using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Error;

public class ErrorMessage : Form
{
	private Exception m_Error = null;

	private IContainer components = null;

	private Button btnOk;

	private Button btnReport;

	private Button btnView;

	private TextBox lblErrorMessage;

	private Button btnIgnore;

	private Button btnDebug;

	public Exception Error
	{
		get
		{
			return m_Error;
		}
		set
		{
			m_Error = value;
		}
	}

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

	public ErrorMessage()
	{
		InitializeComponent();
		btnDebug.Visible = Debugger.IsAttached;
	}

	private void btnReport_Click(object sender, EventArgs e)
	{
		ErrorHandling.Error(Error, Message, ErrorLevel.SendReport);
		Close();
	}

	private void btnView_Click(object sender, EventArgs e)
	{
		MessageBox.Show(ErrorHandling.GetErrorReportText(m_Error, Message));
	}

	private void btnIgnore_Click(object sender, EventArgs e)
	{
		ErrorHandling.ShowErrorMessages = false;
		Close();
	}

	private void btnDebug_Click(object sender, EventArgs e)
	{
		Debugger.Break();
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
		this.btnOk = new System.Windows.Forms.Button();
		this.btnReport = new System.Windows.Forms.Button();
		this.btnView = new System.Windows.Forms.Button();
		this.lblErrorMessage = new System.Windows.Forms.TextBox();
		this.btnIgnore = new System.Windows.Forms.Button();
		this.btnDebug = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOk.Location = new System.Drawing.Point(379, 133);
		this.btnOk.Name = "btnOk";
		this.btnOk.Size = new System.Drawing.Size(71, 26);
		this.btnOk.TabIndex = 0;
		this.btnOk.Text = "OK";
		this.btnOk.UseVisualStyleBackColor = true;
		this.btnReport.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnReport.Location = new System.Drawing.Point(12, 133);
		this.btnReport.Name = "btnReport";
		this.btnReport.Size = new System.Drawing.Size(85, 26);
		this.btnReport.TabIndex = 2;
		this.btnReport.Text = "Report Error";
		this.btnReport.UseVisualStyleBackColor = true;
		this.btnReport.Click += new System.EventHandler(btnReport_Click);
		this.btnView.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnView.Location = new System.Drawing.Point(103, 133);
		this.btnView.Name = "btnView";
		this.btnView.Size = new System.Drawing.Size(85, 26);
		this.btnView.TabIndex = 1;
		this.btnView.Text = "View Details";
		this.btnView.UseVisualStyleBackColor = true;
		this.btnView.Click += new System.EventHandler(btnView_Click);
		this.lblErrorMessage.AcceptsReturn = true;
		this.lblErrorMessage.AcceptsTab = true;
		this.lblErrorMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lblErrorMessage.Location = new System.Drawing.Point(12, 12);
		this.lblErrorMessage.Multiline = true;
		this.lblErrorMessage.Name = "lblErrorMessage";
		this.lblErrorMessage.ReadOnly = true;
		this.lblErrorMessage.Size = new System.Drawing.Size(438, 115);
		this.lblErrorMessage.TabIndex = 3;
		this.lblErrorMessage.TabStop = false;
		this.btnIgnore.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnIgnore.Location = new System.Drawing.Point(194, 133);
		this.btnIgnore.Name = "btnIgnore";
		this.btnIgnore.Size = new System.Drawing.Size(85, 26);
		this.btnIgnore.TabIndex = 4;
		this.btnIgnore.Text = "Ignore Errors";
		this.btnIgnore.UseVisualStyleBackColor = true;
		this.btnIgnore.Click += new System.EventHandler(btnIgnore_Click);
		this.btnDebug.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnDebug.Location = new System.Drawing.Point(285, 133);
		this.btnDebug.Name = "btnDebug";
		this.btnDebug.Size = new System.Drawing.Size(85, 26);
		this.btnDebug.TabIndex = 5;
		this.btnDebug.Text = "Debug";
		this.btnDebug.UseVisualStyleBackColor = true;
		this.btnDebug.Click += new System.EventHandler(btnDebug_Click);
		base.AcceptButton = this.btnOk;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(463, 169);
		base.Controls.Add(this.btnDebug);
		base.Controls.Add(this.btnIgnore);
		base.Controls.Add(this.lblErrorMessage);
		base.Controls.Add(this.btnView);
		base.Controls.Add(this.btnReport);
		base.Controls.Add(this.btnOk);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(328, 123);
		base.Name = "ErrorMessage";
		base.ShowIcon = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Error";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
