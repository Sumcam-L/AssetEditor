using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Sce.Atf.Applications;

internal class RenameCommandDialog : Form
{
	private ISelectionContext m_selectionContext;

	private INamingContext m_namingContext;

	private ITransactionContext m_transactionContext;

	private IContainer components = null;

	private TextBox baseNameTextBox;

	private TextBox prefixTextBox;

	private TextBox suffixTextBox;

	private CheckBox numberCheckBox;

	private Label firstLabel;

	private NumericUpDown firstNumericUpDown;

	private Button renameButton;

	private Label exampleLabel;

	private Label previewLabel;

	private RichTextBox previewTextBox;

	private RadioButton setBaseBtn;

	private RadioButton keepBaseBtn;

	private Label label2;

	private Label label4;

	private Label plusNumberLabel;

	private Label label1;

	private Label label3;

	private Button cancelButton;

	public string Settings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			XmlElement xmlElement = xmlDocument.CreateElement("RenameCommandDialogSettings");
			xmlDocument.AppendChild(xmlElement);
			xmlElement.SetAttribute("setBaseBtn", setBaseBtn.Checked.ToString());
			xmlElement.SetAttribute("prefix", prefixTextBox.Text);
			xmlElement.SetAttribute("baseName", baseNameTextBox.Text);
			xmlElement.SetAttribute("suffix", suffixTextBox.Text);
			xmlElement.SetAttribute("setNumericSuffix", numberCheckBox.Checked.ToString());
			xmlElement.SetAttribute("suffixNumber", firstNumericUpDown.Value.ToString());
			return xmlDocument.InnerXml;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(value);
				XmlElement documentElement = xmlDocument.DocumentElement;
				setBaseBtn.Checked = documentElement.GetAttribute("setBaseBtn") == "True";
				prefixTextBox.Text = documentElement.GetAttribute("prefix");
				baseNameTextBox.Text = documentElement.GetAttribute("baseName");
				suffixTextBox.Text = documentElement.GetAttribute("suffix");
				numberCheckBox.Checked = documentElement.GetAttribute("setNumericSuffix") == "True";
				firstNumericUpDown.Value = decimal.Parse(documentElement.GetAttribute("suffixNumber"));
			}
		}
	}

	public RenameCommandDialog()
	{
		InitializeComponent();
		renameButton.Click += renameButton_Click;
	}

	public void Set(ISelectionContext selectionContext, INamingContext namingContext, ITransactionContext transactionContext)
	{
		m_selectionContext = selectionContext;
		m_namingContext = namingContext;
		m_transactionContext = transactionContext;
		UpdatePreview();
	}

	private void renameButton_Click(object sender, EventArgs e)
	{
		List<Pair<object, string>> newNames = CalculateNewNames(m_selectionContext.Selection);
		if (newNames.Count <= 0)
		{
			return;
		}
		m_transactionContext.DoTransaction(delegate
		{
			foreach (Pair<object, string> item in newNames)
			{
				m_namingContext.SetName(item.First, item.Second);
			}
		}, "Rename selection");
	}

	private void UpdatePreview()
	{
		bool flag = false;
		List<object> list = new List<object>();
		foreach (object item in m_selectionContext.Selection)
		{
			if (m_namingContext.CanSetName(item))
			{
				if (list.Count == 20)
				{
					flag = true;
					break;
				}
				list.Add(item);
			}
		}
		List<Pair<object, string>> list2 = CalculateNewNames(list);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Pair<object, string> item2 in list2)
		{
			string name = m_namingContext.GetName(item2.First);
			string second = item2.Second;
			stringBuilder.AppendLine(name + " => " + second);
		}
		if (flag)
		{
			stringBuilder.AppendLine("...");
		}
		if (list2.Count > 0)
		{
			previewTextBox.Text = stringBuilder.ToString();
			renameButton.Enabled = true;
		}
		else
		{
			renameButton.Enabled = false;
		}
	}

	private List<Pair<object, string>> CalculateNewNames(IEnumerable<object> items)
	{
		List<Pair<object, string>> list = new List<Pair<object, string>>();
		bool flag = setBaseBtn.Checked;
		string text = baseNameTextBox.Text;
		string prefix = prefixTextBox.Text;
		string suffix = suffixTextBox.Text;
		bool flag2 = numberCheckBox.Checked;
		long num = (long)firstNumericUpDown.Value;
		foreach (object item in items)
		{
			if (m_namingContext.CanSetName(item))
			{
				string name = m_namingContext.GetName(item);
				string second = RenameCommand.Rename(name, prefix, flag ? text : null, suffix, flag2 ? num : (-1));
				num++;
				list.Add(new Pair<object, string>(item, second));
			}
		}
		return list;
	}

	private void setBaseBtn_CheckedChanged(object sender, EventArgs e)
	{
		keepBaseBtn.Checked = !setBaseBtn.Checked;
		baseNameTextBox.Enabled = setBaseBtn.Checked;
		UpdatePreview();
	}

	private void keepBaseBtn_CheckedChanged(object sender, EventArgs e)
	{
		setBaseBtn.Checked = !keepBaseBtn.Checked;
	}

	private void numberCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		plusNumberLabel.Enabled = numberCheckBox.Checked;
		firstLabel.Enabled = numberCheckBox.Checked;
		firstNumericUpDown.Enabled = numberCheckBox.Checked;
		UpdatePreview();
	}

	private void prefixTextBox_TextChanged(object sender, EventArgs e)
	{
		UpdatePreview();
	}

	private void baseNameTextBox_TextChanged(object sender, EventArgs e)
	{
		UpdatePreview();
	}

	private void suffixTextBox_TextChanged(object sender, EventArgs e)
	{
		UpdatePreview();
	}

	private void firstNumericUpDown_ValueChanged(object sender, EventArgs e)
	{
		UpdatePreview();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.RenameCommandDialog));
		this.baseNameTextBox = new System.Windows.Forms.TextBox();
		this.prefixTextBox = new System.Windows.Forms.TextBox();
		this.suffixTextBox = new System.Windows.Forms.TextBox();
		this.numberCheckBox = new System.Windows.Forms.CheckBox();
		this.firstLabel = new System.Windows.Forms.Label();
		this.firstNumericUpDown = new System.Windows.Forms.NumericUpDown();
		this.renameButton = new System.Windows.Forms.Button();
		this.exampleLabel = new System.Windows.Forms.Label();
		this.previewLabel = new System.Windows.Forms.Label();
		this.previewTextBox = new System.Windows.Forms.RichTextBox();
		this.setBaseBtn = new System.Windows.Forms.RadioButton();
		this.keepBaseBtn = new System.Windows.Forms.RadioButton();
		this.label2 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.plusNumberLabel = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.cancelButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.firstNumericUpDown).BeginInit();
		base.SuspendLayout();
		resources.ApplyResources(this.baseNameTextBox, "baseNameTextBox");
		this.baseNameTextBox.Name = "baseNameTextBox";
		this.baseNameTextBox.TextChanged += new System.EventHandler(baseNameTextBox_TextChanged);
		resources.ApplyResources(this.prefixTextBox, "prefixTextBox");
		this.prefixTextBox.Name = "prefixTextBox";
		this.prefixTextBox.TextChanged += new System.EventHandler(prefixTextBox_TextChanged);
		resources.ApplyResources(this.suffixTextBox, "suffixTextBox");
		this.suffixTextBox.Name = "suffixTextBox";
		this.suffixTextBox.TextChanged += new System.EventHandler(suffixTextBox_TextChanged);
		resources.ApplyResources(this.numberCheckBox, "numberCheckBox");
		this.numberCheckBox.Checked = true;
		this.numberCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
		this.numberCheckBox.Name = "numberCheckBox";
		this.numberCheckBox.UseVisualStyleBackColor = true;
		this.numberCheckBox.CheckedChanged += new System.EventHandler(numberCheckBox_CheckedChanged);
		resources.ApplyResources(this.firstLabel, "firstLabel");
		this.firstLabel.Name = "firstLabel";
		resources.ApplyResources(this.firstNumericUpDown, "firstNumericUpDown");
		this.firstNumericUpDown.Maximum = new decimal(new int[4] { -1, 2147483647, 0, 0 });
		this.firstNumericUpDown.Name = "firstNumericUpDown";
		this.firstNumericUpDown.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.firstNumericUpDown.ValueChanged += new System.EventHandler(firstNumericUpDown_ValueChanged);
		resources.ApplyResources(this.renameButton, "renameButton");
		this.renameButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.renameButton.Name = "renameButton";
		this.renameButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.exampleLabel, "exampleLabel");
		this.exampleLabel.Name = "exampleLabel";
		resources.ApplyResources(this.previewLabel, "previewLabel");
		this.previewLabel.Name = "previewLabel";
		resources.ApplyResources(this.previewTextBox, "previewTextBox");
		this.previewTextBox.Name = "previewTextBox";
		this.previewTextBox.ReadOnly = true;
		this.previewTextBox.TabStop = false;
		resources.ApplyResources(this.setBaseBtn, "setBaseBtn");
		this.setBaseBtn.Name = "setBaseBtn";
		this.setBaseBtn.UseVisualStyleBackColor = true;
		this.setBaseBtn.CheckedChanged += new System.EventHandler(setBaseBtn_CheckedChanged);
		resources.ApplyResources(this.keepBaseBtn, "keepBaseBtn");
		this.keepBaseBtn.Checked = true;
		this.keepBaseBtn.Name = "keepBaseBtn";
		this.keepBaseBtn.TabStop = true;
		this.keepBaseBtn.UseVisualStyleBackColor = true;
		this.keepBaseBtn.CheckedChanged += new System.EventHandler(keepBaseBtn_CheckedChanged);
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.label4, "label4");
		this.label4.Name = "label4";
		resources.ApplyResources(this.plusNumberLabel, "plusNumberLabel");
		this.plusNumberLabel.Name = "plusNumberLabel";
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.label3, "label3");
		this.label3.Name = "label3";
		resources.ApplyResources(this.cancelButton, "cancelButton");
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.UseVisualStyleBackColor = true;
		base.AcceptButton = this.renameButton;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.plusNumberLabel);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.keepBaseBtn);
		base.Controls.Add(this.setBaseBtn);
		base.Controls.Add(this.previewTextBox);
		base.Controls.Add(this.previewLabel);
		base.Controls.Add(this.exampleLabel);
		base.Controls.Add(this.renameButton);
		base.Controls.Add(this.firstNumericUpDown);
		base.Controls.Add(this.firstLabel);
		base.Controls.Add(this.numberCheckBox);
		base.Controls.Add(this.suffixTextBox);
		base.Controls.Add(this.prefixTextBox);
		base.Controls.Add(this.baseNameTextBox);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Name = "RenameCommandDialog";
		((System.ComponentModel.ISupportInitialize)this.firstNumericUpDown).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
