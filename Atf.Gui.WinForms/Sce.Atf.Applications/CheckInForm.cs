using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class CheckInForm : Form
{
	private readonly ISourceControlService m_sourceControlService;

	private readonly List<IResource> m_resources;

	private IContainer components = null;

	private CheckedListBox m_checkBox;

	private Label label1;

	private Label label2;

	private Button m_submitButton;

	private Button m_cancelButton;

	private TextBox m_textBox;

	public CheckInForm(ISourceControlService sourceControlService, IEnumerable<IResource> resources)
	{
		InitializeComponent();
		m_resources = new List<IResource>(resources);
		foreach (IResource resource in resources)
		{
			m_checkBox.Items.Add(resource.Uri.OriginalString, isChecked: true);
		}
		m_sourceControlService = sourceControlService;
		m_submitButton.Enabled = false;
	}

	private void m_textBox_TextChanged(object sender, EventArgs e)
	{
		m_submitButton.Enabled = m_textBox.Text.Length > 0;
	}

	private void submit_Click(object sender, EventArgs e)
	{
		List<Uri> list = new List<Uri>();
		string description = m_textBox.Text;
		for (int i = 0; i < m_checkBox.Items.Count; i++)
		{
			CheckState itemCheckState = m_checkBox.GetItemCheckState(i);
			if (itemCheckState == CheckState.Checked)
			{
				list.Add(m_resources[i].Uri);
			}
		}
		m_sourceControlService.CheckIn(list, description);
		Close();
	}

	private void cancel_Click(object sender, EventArgs e)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.CheckInForm));
		this.m_checkBox = new System.Windows.Forms.CheckedListBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_submitButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_textBox = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		resources.ApplyResources(this.m_checkBox, "m_checkBox");
		this.m_checkBox.FormattingEnabled = true;
		this.m_checkBox.Name = "m_checkBox";
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.m_submitButton, "m_submitButton");
		this.m_submitButton.Name = "m_submitButton";
		this.m_submitButton.UseVisualStyleBackColor = true;
		this.m_submitButton.Click += new System.EventHandler(submit_Click);
		resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(cancel_Click);
		resources.ApplyResources(this.m_textBox, "m_textBox");
		this.m_textBox.Name = "m_textBox";
		this.m_textBox.TextChanged += new System.EventHandler(m_textBox_TextChanged);
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_textBox);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_submitButton);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_checkBox);
		base.Name = "CheckInForm";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
