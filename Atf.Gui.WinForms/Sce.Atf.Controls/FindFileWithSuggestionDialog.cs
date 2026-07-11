using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class FindFileWithSuggestionDialog : Form
{
	private IContainer components = null;

	private Label label1;

	private Label label2;

	private RadioButton specifyRadioButton;

	private RadioButton ignoreRadioButton;

	private RadioButton ignoreAllRadioButton;

	private Button OkButton;

	private Button cancelButton;

	private RadioButton acceptAllRadioButton;

	private RadioButton acceptRadioButton;

	private Label label3;

	private TextBox missingFileLabel;

	private TextBox suggestedFileLabel;

	public FindFileAction Action
	{
		get
		{
			if (acceptRadioButton.Checked)
			{
				return FindFileAction.AcceptSuggestion;
			}
			if (acceptAllRadioButton.Checked)
			{
				return FindFileAction.AcceptAllSuggestions;
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

	public FindFileWithSuggestionDialog(string originalPath, string suggestedPath)
	{
		InitializeComponent();
		missingFileLabel.Text = originalPath;
		suggestedFileLabel.Text = suggestedPath;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.FindFileWithSuggestionDialog));
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.specifyRadioButton = new System.Windows.Forms.RadioButton();
		this.ignoreRadioButton = new System.Windows.Forms.RadioButton();
		this.ignoreAllRadioButton = new System.Windows.Forms.RadioButton();
		this.OkButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.acceptAllRadioButton = new System.Windows.Forms.RadioButton();
		this.acceptRadioButton = new System.Windows.Forms.RadioButton();
		this.label3 = new System.Windows.Forms.Label();
		this.missingFileLabel = new System.Windows.Forms.TextBox();
		this.suggestedFileLabel = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.specifyRadioButton, "specifyRadioButton");
		this.specifyRadioButton.Name = "specifyRadioButton";
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
		resources.ApplyResources(this.acceptAllRadioButton, "acceptAllRadioButton");
		this.acceptAllRadioButton.Checked = true;
		this.acceptAllRadioButton.Name = "acceptAllRadioButton";
		this.acceptAllRadioButton.TabStop = true;
		this.acceptAllRadioButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.acceptRadioButton, "acceptRadioButton");
		this.acceptRadioButton.Name = "acceptRadioButton";
		this.acceptRadioButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.label3, "label3");
		this.label3.Name = "label3";
		resources.ApplyResources(this.missingFileLabel, "missingFileLabel");
		this.missingFileLabel.Name = "missingFileLabel";
		this.missingFileLabel.ReadOnly = true;
		resources.ApplyResources(this.suggestedFileLabel, "suggestedFileLabel");
		this.suggestedFileLabel.Name = "suggestedFileLabel";
		this.suggestedFileLabel.ReadOnly = true;
		base.AcceptButton = this.OkButton;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.Controls.Add(this.suggestedFileLabel);
		base.Controls.Add(this.missingFileLabel);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.acceptRadioButton);
		base.Controls.Add(this.acceptAllRadioButton);
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
		base.Name = "FindFileWithSuggestionDialog";
		base.TopMost = true;
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
