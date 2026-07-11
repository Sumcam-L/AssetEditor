using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Sce.Atf.Applications.WebServices.com.scea.ship.submitBug;

namespace Sce.Atf.Applications.WebServices;

public class FeedbackForm : Form
{
	private TextBox m_userTextBox;

	private Label m_descLabel;

	private RichTextBox m_descTextBox;

	private TextBox m_titleTextBox;

	private Label m_titleLabel;

	private Label m_passwordLabel;

	private Label m_userLabel;

	private TextBox m_passwordTextBox;

	private Button m_submitBtn;

	private Button m_cancelBtn;

	private StatusBar m_statusBar;

	private Label m_lblEmail;

	private TextBox m_txtEmail;

	private Label m_lblPriority;

	private ComboBox m_cmbPriority;

	private readonly Container components = null;

	private readonly string m_mappingName;

	private Exception m_exception;

	private readonly BugReportingService m_bugService = new BugReportingService();

	private readonly bool m_anonymous;

	public Exception Exception
	{
		get
		{
			return m_exception;
		}
		set
		{
			m_exception = value;
		}
	}

	public string Title
	{
		get
		{
			return m_titleTextBox.Text;
		}
		set
		{
			m_titleTextBox.Text = value;
		}
	}

	public FeedbackForm(bool anon)
	{
		Assembly element = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
		ProjectMappingAttribute projectMappingAttribute = (ProjectMappingAttribute)Attribute.GetCustomAttribute(element, typeof(ProjectMappingAttribute));
		if (projectMappingAttribute != null && projectMappingAttribute.Mapping != null && projectMappingAttribute.Mapping.Trim().Length != 0)
		{
			m_mappingName = projectMappingAttribute.Mapping.Trim();
		}
		else
		{
			m_mappingName = null;
		}
		InitializeComponent();
		m_cmbPriority.SelectedIndex = 2;
		m_anonymous = anon;
		if (anon)
		{
			m_passwordLabel.Visible = false;
			m_passwordTextBox.Visible = false;
			m_userLabel.Visible = false;
			m_userTextBox.Visible = false;
		}
		else
		{
			m_txtEmail.Visible = false;
			m_lblEmail.Visible = false;
		}
	}

	private bool ValidateData()
	{
		m_lblEmail.ForeColor = Color.Black;
		m_passwordLabel.ForeColor = Color.Black;
		m_userLabel.ForeColor = Color.Black;
		m_titleLabel.ForeColor = Color.Black;
		if (m_mappingName == null)
		{
			MessageBox.Show(this, "Assembly mapping attribute not found.\nCannot proceed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return false;
		}
		if (m_titleTextBox.Text.Trim().Length == 0)
		{
			MessageBox.Show(this, "Please fill in title", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			m_titleLabel.ForeColor = Color.Red;
			m_titleTextBox.Focus();
			return false;
		}
		if (m_anonymous)
		{
			if (!Regex.IsMatch(m_txtEmail.Text.Trim(), "^([\\w-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([\\w-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$"))
			{
				MessageBox.Show(this, "Invalid email address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				m_lblEmail.ForeColor = Color.Red;
				m_txtEmail.Focus();
				return false;
			}
		}
		else
		{
			if (m_userTextBox.Text.Trim().Length == 0)
			{
				MessageBox.Show(this, "User name required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				m_userLabel.ForeColor = Color.Red;
				m_userTextBox.Focus();
				return false;
			}
			if (m_passwordTextBox.Text.Trim().Length == 0)
			{
				MessageBox.Show(this, "Password required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				m_passwordLabel.ForeColor = Color.Red;
				m_passwordTextBox.Focus();
				return false;
			}
		}
		return true;
	}

	private void DoSubmit()
	{
		try
		{
			using (new WaitCursor())
			{
				if (!ValidateData())
				{
					return;
				}
				m_statusBar.Text = "Submitting bug...";
				WebPermission webPermission = new WebPermission(PermissionState.Unrestricted);
				webPermission.Demand();
				string text = m_descTextBox.Text;
				if (m_exception != null)
				{
					text += "\n\n-----------------------------------\n";
					text += "Exception:\n";
					text += m_exception.ToString();
				}
				text += "\n\n-----------------------------------\n";
				text += "Modules:\n";
				foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
				{
					FileVersionInfo fileVersionInfo = module.FileVersionInfo;
					if (!(fileVersionInfo.FileVersion == ""))
					{
						text += $"{module.ModuleName} {fileVersionInfo.FileVersion}\n";
					}
				}
				if (m_anonymous)
				{
					string description = "Reported by: " + m_txtEmail.Text + "\n" + text;
					m_bugService.submitBug(m_mappingName, m_titleTextBox.Text, description, m_cmbPriority.SelectedIndex);
				}
				else
				{
					m_bugService.submitBug(m_mappingName, m_userTextBox.Text, m_passwordTextBox.Text, m_titleTextBox.Text, text, m_cmbPriority.SelectedIndex);
				}
				Close();
			}
		}
		catch (Exception ex)
		{
			m_statusBar.Text = "Bug not submitted.";
			MessageBox.Show("There were errors while submitting this bug\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.WebServices.FeedbackForm));
		this.m_userTextBox = new System.Windows.Forms.TextBox();
		this.m_descLabel = new System.Windows.Forms.Label();
		this.m_descTextBox = new System.Windows.Forms.RichTextBox();
		this.m_titleTextBox = new System.Windows.Forms.TextBox();
		this.m_titleLabel = new System.Windows.Forms.Label();
		this.m_passwordLabel = new System.Windows.Forms.Label();
		this.m_userLabel = new System.Windows.Forms.Label();
		this.m_passwordTextBox = new System.Windows.Forms.TextBox();
		this.m_submitBtn = new System.Windows.Forms.Button();
		this.m_cancelBtn = new System.Windows.Forms.Button();
		this.m_statusBar = new System.Windows.Forms.StatusBar();
		this.m_lblEmail = new System.Windows.Forms.Label();
		this.m_txtEmail = new System.Windows.Forms.TextBox();
		this.m_lblPriority = new System.Windows.Forms.Label();
		this.m_cmbPriority = new System.Windows.Forms.ComboBox();
		base.SuspendLayout();
		resources.ApplyResources(this.m_userTextBox, "m_userTextBox");
		this.m_userTextBox.Name = "m_userTextBox";
		resources.ApplyResources(this.m_descLabel, "m_descLabel");
		this.m_descLabel.Name = "m_descLabel";
		resources.ApplyResources(this.m_descTextBox, "m_descTextBox");
		this.m_descTextBox.Name = "m_descTextBox";
		resources.ApplyResources(this.m_titleTextBox, "m_titleTextBox");
		this.m_titleTextBox.Name = "m_titleTextBox";
		resources.ApplyResources(this.m_titleLabel, "m_titleLabel");
		this.m_titleLabel.Name = "m_titleLabel";
		resources.ApplyResources(this.m_passwordLabel, "m_passwordLabel");
		this.m_passwordLabel.Name = "m_passwordLabel";
		resources.ApplyResources(this.m_userLabel, "m_userLabel");
		this.m_userLabel.Name = "m_userLabel";
		resources.ApplyResources(this.m_passwordTextBox, "m_passwordTextBox");
		this.m_passwordTextBox.Name = "m_passwordTextBox";
		resources.ApplyResources(this.m_submitBtn, "m_submitBtn");
		this.m_submitBtn.BackColor = System.Drawing.SystemColors.Control;
		this.m_submitBtn.Name = "m_submitBtn";
		this.m_submitBtn.UseVisualStyleBackColor = false;
		this.m_submitBtn.Click += new System.EventHandler(m_submitBtn_Click);
		resources.ApplyResources(this.m_cancelBtn, "m_cancelBtn");
		this.m_cancelBtn.BackColor = System.Drawing.SystemColors.Control;
		this.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_cancelBtn.Name = "m_cancelBtn";
		this.m_cancelBtn.UseVisualStyleBackColor = false;
		this.m_cancelBtn.Click += new System.EventHandler(m_cancelBtn_Click);
		resources.ApplyResources(this.m_statusBar, "m_statusBar");
		this.m_statusBar.Name = "m_statusBar";
		resources.ApplyResources(this.m_lblEmail, "m_lblEmail");
		this.m_lblEmail.Name = "m_lblEmail";
		resources.ApplyResources(this.m_txtEmail, "m_txtEmail");
		this.m_txtEmail.Name = "m_txtEmail";
		resources.ApplyResources(this.m_lblPriority, "m_lblPriority");
		this.m_lblPriority.Name = "m_lblPriority";
		resources.ApplyResources(this.m_cmbPriority, "m_cmbPriority");
		this.m_cmbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cmbPriority.Items.AddRange(new object[6]
		{
			resources.GetString("m_cmbPriority.Items"),
			resources.GetString("m_cmbPriority.Items1"),
			resources.GetString("m_cmbPriority.Items2"),
			resources.GetString("m_cmbPriority.Items3"),
			resources.GetString("m_cmbPriority.Items4"),
			resources.GetString("m_cmbPriority.Items5")
		});
		this.m_cmbPriority.Name = "m_cmbPriority";
		base.AcceptButton = this.m_submitBtn;
		resources.ApplyResources(this, "$this");
		this.BackColor = System.Drawing.SystemColors.Control;
		base.CancelButton = this.m_cancelBtn;
		base.Controls.Add(this.m_cmbPriority);
		base.Controls.Add(this.m_lblPriority);
		base.Controls.Add(this.m_txtEmail);
		base.Controls.Add(this.m_lblEmail);
		base.Controls.Add(this.m_statusBar);
		base.Controls.Add(this.m_cancelBtn);
		base.Controls.Add(this.m_submitBtn);
		base.Controls.Add(this.m_titleTextBox);
		base.Controls.Add(this.m_passwordTextBox);
		base.Controls.Add(this.m_userTextBox);
		base.Controls.Add(this.m_titleLabel);
		base.Controls.Add(this.m_passwordLabel);
		base.Controls.Add(this.m_userLabel);
		base.Controls.Add(this.m_descLabel);
		base.Controls.Add(this.m_descTextBox);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FeedbackForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void m_cancelBtn_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void m_submitBtn_Click(object sender, EventArgs e)
	{
		DoSubmit();
	}
}
