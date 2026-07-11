using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class FindFileDialog : Form
{
	private IContainer components = null;

	private Label label1;

	private Label label2;

	private RadioButton specifyRadioButton;

	private RadioButton ignoreRadioButton;

	private RadioButton ignoreAllRadioButton;

	private Button OkButton;

	private Button cancelButton;

	private RadioButton searchRadioButton;

	private RadioButton searchAllRadioButton;

	private TextBox missingFileLabel;

	public FindFileAction Action
	{
		get
		{
			if (searchRadioButton.Checked)
			{
				return FindFileAction.SearchDirectory;
			}
			if (searchAllRadioButton.Checked)
			{
				return FindFileAction.SearchDirectoryForAll;
			}
			if (specifyRadioButton.Checked)
			{
				return FindFileAction.UserSpecify;
			}
			if (ignoreRadioButton.Checked)
			{
				return FindFileAction.Ignore;
			}
			if (ignoreAllRadioButton.Checked)
			{
				return FindFileAction.IgnoreAll;
			}
			throw new InvalidOperationException("no radio buttons were checked");
		}
	}

	public FindFileDialog(string originalPath)
	{
		InitializeComponent();
		missingFileLabel.Text = originalPath;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.FindFileDialog));
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.specifyRadioButton = new System.Windows.Forms.RadioButton();
		this.ignoreRadioButton = new System.Windows.Forms.RadioButton();
		this.ignoreAllRadioButton = new System.Windows.Forms.RadioButton();
		this.OkButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.searchRadioButton = new System.Windows.Forms.RadioButton();
		this.searchAllRadioButton = new System.Windows.Forms.RadioButton();
		this.missingFileLabel = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.label1.AutoEllipsis = true;
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.specifyRadioButton, "specifyRadioButton");
		this.specifyRadioButton.Checked = true;
		this.specifyRadioButton.Name = "specifyRadioButton";
		this.specifyRadioButton.TabStop = true;
		this.specifyRadioButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.ignoreRadioButton, "ignoreRadioButton");
		this.ignoreRadioButton.Name = "ignoreRadioButton";
		this.ignoreRadioButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.ignoreAllRadioButton, "ignoreAllRadioButton");
		this.ignoreAllRadioButton.Name = "ignoreAllRadioButton";
		this.ignoreAllRadioButton.UseVisualStyleBackColor = true;
		this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		resources.ApplyResources(this.OkButton, "OkButton");
		this.OkButton.Name = "OkButton";
		this.OkButton.UseVisualStyleBackColor = true;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		resources.ApplyResources(this.cancelButton, "cancelButton");
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.searchRadioButton, "searchRadioButton");
		this.searchRadioButton.Name = "searchRadioButton";
		this.searchRadioButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.searchAllRadioButton, "searchAllRadioButton");
		this.searchAllRadioButton.Name = "searchAllRadioButton";
		this.searchAllRadioButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.missingFileLabel, "missingFileLabel");
		this.missingFileLabel.Name = "missingFileLabel";
		this.missingFileLabel.ReadOnly = true;
		base.AcceptButton = this.OkButton;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.Controls.Add(this.missingFileLabel);
		base.Controls.Add(this.searchAllRadioButton);
		base.Controls.Add(this.searchRadioButton);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.OkButton);
		base.Controls.Add(this.ignoreAllRadioButton);
		base.Controls.Add(this.ignoreRadioButton);
		base.Controls.Add(this.specifyRadioButton);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FindFileDialog";
		base.TopMost = true;
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
