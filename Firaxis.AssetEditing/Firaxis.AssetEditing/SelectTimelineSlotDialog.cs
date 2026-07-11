using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Collections;

namespace Firaxis.AssetEditing;

public class SelectTimelineSlotDialog : Form
{
	private IContainer components;

	private ComboBox cbSlots;

	private Label label1;

	private Button btnCancel;

	private Button btnOK;

	public string SelectedSlot { get; private set; }

	private IEnumerable<string> Slots { get; set; }

	public SelectTimelineSlotDialog(IEnumerable<string> slots)
	{
		InitializeComponent();
		Slots = slots;
		Slots.ForEach(delegate(string slot)
		{
			cbSlots.Items.Add(slot);
		});
		cbSlots.SelectedIndex = 0;
	}

	private void cbSlots_SelectedIndexChanged(object sender, EventArgs e)
	{
		SelectedSlot = cbSlots.SelectedItem as string;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.AssetEditing.SelectTimelineSlotDialog));
		this.cbSlots = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnOK = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.cbSlots.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.cbSlots.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cbSlots.FormattingEnabled = true;
		this.cbSlots.Location = new System.Drawing.Point(12, 25);
		this.cbSlots.Name = "cbSlots";
		this.cbSlots.Size = new System.Drawing.Size(315, 21);
		this.cbSlots.TabIndex = 1;
		this.cbSlots.SelectedIndexChanged += new System.EventHandler(cbSlots_SelectedIndexChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(59, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Slot Name:";
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(251, 68);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 3;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Location = new System.Drawing.Point(170, 68);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(75, 23);
		this.btnOK.TabIndex = 2;
		this.btnOK.Text = "OK";
		this.btnOK.UseVisualStyleBackColor = true;
		base.AcceptButton = this.btnOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(339, 103);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.cbSlots);
		this.Cursor = System.Windows.Forms.Cursors.Default;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SelectTimelineSlotDialog";
		this.Text = "Add Timeline";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
