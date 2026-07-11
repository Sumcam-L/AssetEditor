using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.ATF;

public class TextInputValidationForm : Form
{
	private string m_userText = "";

	private Predicate<string> m_validationFunction;

	private IContainer components;

	private TextBox textInputBox;

	private Button okButton;

	private Button cancelButton;

	private Label textInputLabel;

	private Label errorLabel;

	private CheckBox cbForced;

	public Predicate<string> ForceActionResponse { get; set; }

	public string ForceMessage
	{
		get
		{
			return cbForced.Text;
		}
		set
		{
			cbForced.Text = value;
		}
	}

	public string FormTitle
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

	public string InputLabel
	{
		get
		{
			return textInputLabel.Text;
		}
		set
		{
			textInputLabel.Text = value;
		}
	}

	public string InvalidInputLabel
	{
		get
		{
			return errorLabel.Text;
		}
		set
		{
			errorLabel.Text = value;
		}
	}

	public string UserText => m_userText;

	public bool WasForced => cbForced.Checked;

	public TextInputValidationForm(Predicate<string> validationFunction)
		: this(validationFunction, "")
	{
	}

	public TextInputValidationForm(Predicate<string> validationFunction, string name)
	{
		InitializeComponent();
		textInputBox.Text = name;
		textInputLabel.Text = "";
		m_validationFunction = validationFunction;
	}

	private void cbForced_CheckedChanged(object sender, EventArgs e)
	{
		if (cbForced.Checked && ForceActionResponse != null)
		{
			string obj = textInputBox.Text;
			cbForced.Checked = ForceActionResponse(obj);
		}
	}

	private void entityNameTextBox_TextChanged(object sender, EventArgs e)
	{
		if (errorLabel.Visible)
		{
			errorLabel.Visible = false;
		}
	}

	private void TextInputValidationForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (base.DialogResult == DialogResult.OK)
		{
			string text = textInputBox.Text;
			if (m_validationFunction(text) || WasForced)
			{
				m_userText = text;
				return;
			}
			cbForced.Visible = true;
			errorLabel.Visible = true;
			e.Cancel = true;
		}
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
		this.textInputBox = new System.Windows.Forms.TextBox();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.textInputLabel = new System.Windows.Forms.Label();
		this.errorLabel = new System.Windows.Forms.Label();
		this.cbForced = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.textInputBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.textInputBox.Location = new System.Drawing.Point(18, 31);
		this.textInputBox.Name = "textInputBox";
		this.textInputBox.Size = new System.Drawing.Size(257, 20);
		this.textInputBox.TabIndex = 0;
		this.textInputBox.TextChanged += new System.EventHandler(entityNameTextBox_TextChanged);
		this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(281, 29);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(75, 23);
		this.okButton.TabIndex = 1;
		this.okButton.Text = "OK";
		this.okButton.UseVisualStyleBackColor = true;
		this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(362, 29);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(75, 23);
		this.cancelButton.TabIndex = 2;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.UseVisualStyleBackColor = true;
		this.textInputLabel.AutoSize = true;
		this.textInputLabel.Location = new System.Drawing.Point(16, 13);
		this.textInputLabel.Name = "textInputLabel";
		this.textInputLabel.Size = new System.Drawing.Size(0, 13);
		this.textInputLabel.TabIndex = 3;
		this.errorLabel.AutoSize = true;
		this.errorLabel.ForeColor = System.Drawing.Color.Red;
		this.errorLabel.Location = new System.Drawing.Point(16, 56);
		this.errorLabel.Name = "errorLabel";
		this.errorLabel.Size = new System.Drawing.Size(0, 13);
		this.errorLabel.TabIndex = 4;
		this.errorLabel.Visible = false;
		this.cbForced.AutoSize = true;
		this.cbForced.Location = new System.Drawing.Point(281, 56);
		this.cbForced.Name = "cbForced";
		this.cbForced.Size = new System.Drawing.Size(53, 17);
		this.cbForced.TabIndex = 5;
		this.cbForced.Text = "Force";
		this.cbForced.UseVisualStyleBackColor = true;
		this.cbForced.Visible = false;
		this.cbForced.CheckedChanged += new System.EventHandler(cbForced_CheckedChanged);
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(446, 79);
		base.Controls.Add(this.cbForced);
		base.Controls.Add(this.errorLabel);
		base.Controls.Add(this.textInputLabel);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.textInputBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MinimizeBox = false;
		base.Name = "TextInputValidationForm";
		base.ShowIcon = false;
		this.Text = "Text Input Validator";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(TextInputValidationForm_FormClosing);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
