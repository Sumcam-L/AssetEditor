using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.AssetEditing;

public class FindForm : Form
{
	private IContainer components;

	private Label label1;

	private ComboBox cmbFindWhat;

	private Button btnCancel;

	private Button btnFind;

	public string FindWhat { get; set; }

	public FindForm()
	{
		InitializeComponent();
		base.Shown += FindForm_Shown;
		cmbFindWhat.KeyUp += CmbFindWhat_KeyUp;
		btnFind.Click += BtnFind_Click;
	}

	private void CmbFindWhat_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			FindWhat = cmbFindWhat.Text;
			base.DialogResult = DialogResult.OK;
			Close();
		}
		else if (e.KeyCode == Keys.Escape)
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}
	}

	private void BtnFind_Click(object sender, EventArgs e)
	{
		FindWhat = cmbFindWhat.Text;
	}

	private void FindForm_Shown(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(FindWhat))
		{
			if (cmbFindWhat.Items.Count > 0)
			{
				cmbFindWhat.SelectedIndex = 0;
			}
			return;
		}
		if (!cmbFindWhat.Items.Contains(FindWhat))
		{
			cmbFindWhat.Items.Add(FindWhat);
		}
		cmbFindWhat.Text = FindWhat;
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
		this.label1 = new System.Windows.Forms.Label();
		this.cmbFindWhat = new System.Windows.Forms.ComboBox();
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnFind = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(13, 13);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(56, 13);
		this.label1.TabIndex = 0;
		this.label1.Text = "Find what:";
		this.cmbFindWhat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.cmbFindWhat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
		this.cmbFindWhat.FormattingEnabled = true;
		this.cmbFindWhat.Location = new System.Drawing.Point(16, 30);
		this.cmbFindWhat.Name = "cmbFindWhat";
		this.cmbFindWhat.Size = new System.Drawing.Size(466, 21);
		this.cmbFindWhat.TabIndex = 0;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(406, 86);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "&Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnFind.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnFind.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnFind.Location = new System.Drawing.Point(325, 86);
		this.btnFind.Name = "btnFind";
		this.btnFind.Size = new System.Drawing.Size(75, 23);
		this.btnFind.TabIndex = 1;
		this.btnFind.Text = "&Find";
		this.btnFind.UseVisualStyleBackColor = true;
		base.AcceptButton = this.btnFind;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(494, 121);
		base.Controls.Add(this.btnFind);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.cmbFindWhat);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		this.MinimumSize = new System.Drawing.Size(510, 160);
		base.Name = "FindForm";
		this.Text = "Find";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
