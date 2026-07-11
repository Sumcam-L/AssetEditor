using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class ItemSelectorForm : Form
{
	private IContainer components = null;

	private Button okButton;

	private Button cancelButton;

	private ComboBox itemsToSelect;

	private Label caption;

	public string Title
	{
		get
		{
			return Text;
		}
		set
		{
			Text = value;
		}
	}

	public string Caption
	{
		get
		{
			return caption.Text;
		}
		set
		{
			caption.Text = value;
		}
	}

	public string SelectedItem
	{
		get
		{
			if (itemsToSelect.SelectedIndex >= 0)
			{
				return itemsToSelect.SelectedItem as string;
			}
			return string.Empty;
		}
	}

	public ItemSelectorForm(string[] choices)
	{
		InitializeComponent();
		itemsToSelect.Items.AddRange(choices);
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
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.itemsToSelect = new System.Windows.Forms.ComboBox();
		this.caption = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(228, 61);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(75, 23);
		this.okButton.TabIndex = 0;
		this.okButton.Text = "OK";
		this.okButton.UseVisualStyleBackColor = true;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(309, 61);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(75, 23);
		this.cancelButton.TabIndex = 1;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.UseVisualStyleBackColor = true;
		this.itemsToSelect.FormattingEnabled = true;
		this.itemsToSelect.Location = new System.Drawing.Point(12, 61);
		this.itemsToSelect.Name = "itemsToSelect";
		this.itemsToSelect.Size = new System.Drawing.Size(210, 21);
		this.itemsToSelect.TabIndex = 2;
		this.caption.AutoSize = true;
		this.caption.Location = new System.Drawing.Point(9, 9);
		this.caption.Name = "caption";
		this.caption.Size = new System.Drawing.Size(35, 13);
		this.caption.TabIndex = 3;
		this.caption.Text = "label1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(395, 96);
		base.Controls.Add(this.caption);
		base.Controls.Add(this.itemsToSelect);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.okButton);
		base.Name = "ItemSelectorForm";
		this.Text = "ItemSelectorForm";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
