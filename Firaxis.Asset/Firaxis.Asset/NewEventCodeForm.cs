using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Asset.Trigger;
using Firaxis.Controls;

namespace Firaxis.Asset;

public class NewEventCodeForm : Form
{
	private ITriggerSystem trigsys;

	private IContainer components = null;

	private CaptionControl captionControl1;

	private Button btnCancel;

	private Button btnOK;

	private Label label1;

	private TextBox textBox1;

	public string TimelineName
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

	public NewEventCodeForm(ITriggerSystem trigsys)
	{
		this.trigsys = trigsys;
		InitializeComponent();
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		if (trigsys != null && trigsys.Tracks != null && trigsys.Tracks.Find(textBox1.Text) != null)
		{
			MessageBox.Show($"Timeline Name '{textBox1.Text}' already exists.", "New Timeline", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			textBox1.Focus();
		}
		else
		{
			base.DialogResult = DialogResult.OK;
		}
	}

	private void textBox1_TextChanged(object sender, EventArgs e)
	{
		btnOK.Enabled = textBox1.Text.Length > 0;
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
		this.captionControl1 = new Firaxis.Controls.CaptionControl();
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnOK = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.textBox1 = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.captionControl1.BackColor = System.Drawing.SystemColors.ControlDark;
		this.captionControl1.Caption = "Enter unique Timeline Name";
		this.captionControl1.Dock = System.Windows.Forms.DockStyle.Top;
		this.captionControl1.Image = null;
		this.captionControl1.Location = new System.Drawing.Point(0, 0);
		this.captionControl1.Name = "captionControl1";
		this.captionControl1.Size = new System.Drawing.Size(233, 27);
		this.captionControl1.TabIndex = 0;
		this.captionControl1.TabStop = false;
		this.captionControl1.Transparent = System.Drawing.Color.Magenta;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(124, 65);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 4;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.Enabled = false;
		this.btnOK.Location = new System.Drawing.Point(43, 65);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(75, 23);
		this.btnOK.TabIndex = 3;
		this.btnOK.Text = "OK";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 36);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(35, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Name";
		this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.textBox1.Location = new System.Drawing.Point(53, 33);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(168, 20);
		this.textBox1.TabIndex = 2;
		this.textBox1.TextChanged += new System.EventHandler(textBox1_TextChanged);
		base.AcceptButton = this.btnOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(233, 100);
		base.Controls.Add(this.textBox1);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.captionControl1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "NewEventCodeForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "New Timeline";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
