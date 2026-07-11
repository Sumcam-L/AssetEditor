using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class CopyableTextViewer : Form
{
	private IContainer components = null;

	private Button btnOk;

	private Button btnCopy;

	private RichTextBox txtText;

	public string DisplayedText
	{
		get
		{
			return txtText.Text;
		}
		set
		{
			txtText.Text = value;
		}
	}

	public bool WordWrap
	{
		get
		{
			return txtText.WordWrap;
		}
		set
		{
			txtText.WordWrap = value;
		}
	}

	public CopyableTextViewer()
	{
		InitializeComponent();
	}

	private void btnCopy_Click(object sender, EventArgs e)
	{
		int selectionStart = txtText.SelectionStart;
		int selectionLength = txtText.SelectionLength;
		txtText.SelectionStart = 0;
		txtText.SelectionLength = txtText.Text.Length;
		txtText.Copy();
		txtText.SelectionStart = selectionStart;
		txtText.SelectionLength = selectionLength;
	}

	private void btnOk_Click(object sender, EventArgs e)
	{
		Close();
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
		this.btnCopy = new System.Windows.Forms.Button();
		this.txtText = new System.Windows.Forms.RichTextBox();
		base.SuspendLayout();
		this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOk.Location = new System.Drawing.Point(270, 170);
		this.btnOk.Name = "btnOk";
		this.btnOk.Size = new System.Drawing.Size(83, 25);
		this.btnOk.TabIndex = 2;
		this.btnOk.Text = "OK";
		this.btnOk.UseVisualStyleBackColor = true;
		this.btnOk.Click += new System.EventHandler(btnOk_Click);
		this.btnCopy.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.btnCopy.Location = new System.Drawing.Point(12, 170);
		this.btnCopy.Name = "btnCopy";
		this.btnCopy.Size = new System.Drawing.Size(77, 25);
		this.btnCopy.TabIndex = 1;
		this.btnCopy.Text = "Copy Text";
		this.btnCopy.UseVisualStyleBackColor = true;
		this.btnCopy.Click += new System.EventHandler(btnCopy_Click);
		this.txtText.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.txtText.BackColor = System.Drawing.Color.White;
		this.txtText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtText.ForeColor = System.Drawing.Color.Black;
		this.txtText.Location = new System.Drawing.Point(12, 12);
		this.txtText.Name = "txtText";
		this.txtText.ReadOnly = true;
		this.txtText.Size = new System.Drawing.Size(341, 152);
		this.txtText.TabIndex = 3;
		this.txtText.Text = "";
		this.txtText.WordWrap = false;
		base.AcceptButton = this.btnOk;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(365, 204);
		base.Controls.Add(this.txtText);
		base.Controls.Add(this.btnCopy);
		base.Controls.Add(this.btnOk);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(216, 142);
		base.Name = "CopyableTextViewer";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		base.ResumeLayout(false);
	}
}
