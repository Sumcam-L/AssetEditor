using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Error;

public class ErrorReportWnd : IErrorReportForm
{
	private string m_sErrorReport = string.Empty;

	private IContainer components = null;

	private Label lblDescription;

	private TextBox txtComments;

	private Label label1;

	private Button btnOk;

	private Button btnViewReport;

	public override string Comments
	{
		get
		{
			return txtComments.Text;
		}
		set
		{
			txtComments.Text = value;
		}
	}

	public ErrorReportWnd()
	{
		InitializeComponent();
	}

	private void btnViewReport_Click(object sender, EventArgs e)
	{
		MessageBox.Show(base.ErrorReport);
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
		this.lblDescription = new System.Windows.Forms.Label();
		this.txtComments = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.btnOk = new System.Windows.Forms.Button();
		this.btnViewReport = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.lblDescription.AutoSize = true;
		this.lblDescription.Location = new System.Drawing.Point(9, 9);
		this.lblDescription.Name = "lblDescription";
		this.lblDescription.Size = new System.Drawing.Size(267, 26);
		this.lblDescription.TabIndex = 0;
		this.lblDescription.Text = "An error has occured and a report will be sent.\r\nPlease add any comments or reproduction steps below.";
		this.txtComments.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.txtComments.Location = new System.Drawing.Point(12, 60);
		this.txtComments.Multiline = true;
		this.txtComments.Name = "txtComments";
		this.txtComments.Size = new System.Drawing.Size(399, 129);
		this.txtComments.TabIndex = 1;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(9, 44);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(59, 13);
		this.label1.TabIndex = 2;
		this.label1.Text = "Comments:";
		this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnOk.Location = new System.Drawing.Point(247, 197);
		this.btnOk.Name = "btnOk";
		this.btnOk.Size = new System.Drawing.Size(79, 23);
		this.btnOk.TabIndex = 3;
		this.btnOk.Text = "OK";
		this.btnOk.UseVisualStyleBackColor = false;
		this.btnViewReport.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnViewReport.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.btnViewReport.Location = new System.Drawing.Point(332, 197);
		this.btnViewReport.Name = "btnViewReport";
		this.btnViewReport.Size = new System.Drawing.Size(79, 23);
		this.btnViewReport.TabIndex = 4;
		this.btnViewReport.Text = "View Report";
		this.btnViewReport.UseVisualStyleBackColor = false;
		this.btnViewReport.Click += new System.EventHandler(btnViewReport_Click);
		base.AcceptButton = this.btnOk;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(423, 232);
		base.ControlBox = false;
		base.Controls.Add(this.btnViewReport);
		base.Controls.Add(this.btnOk);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txtComments);
		base.Controls.Add(this.lblDescription);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(299, 178);
		base.Name = "ErrorReportWnd";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Error";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
