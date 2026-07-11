using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Applications.NetworkTargetServices;

public class TargetEditDialog : Form
{
	private Target m_target;

	private bool m_changed;

	private IContainer components = null;

	private GroupBox grpTarget;

	private Label lblPort;

	private Label lblHost;

	private Label lblName;

	private TextBox txtPort;

	private TextBox txtHost;

	private TextBox txtName;

	private Button m_btnOK;

	private Button m_btnCancel;

	private Label lblProtocol;

	private ComboBox cmbProtocol;

	public bool Changed => m_changed;

	public TargetEditDialog(int defaultPortNumber, bool canEditPortNumber, string[] protocols)
		: this(null, defaultPortNumber, canEditPortNumber, protocols)
	{
	}

	public TargetEditDialog(Target target, int defaultPortNumber, bool canEditPortNumber, string[] protocols)
	{
		InitializeComponent();
		if (protocols != null && protocols.Length != 0)
		{
			cmbProtocol.DataSource = protocols;
		}
		else
		{
			cmbProtocol.Enabled = false;
		}
		if (target != null)
		{
			Text = "Edit Target".Localize();
			txtName.Text = target.Name;
			txtHost.Text = target.Host;
			txtPort.Text = target.Port.ToString();
			txtPort.ReadOnly = !canEditPortNumber;
			if (cmbProtocol.Enabled)
			{
				cmbProtocol.SelectedItem = target.Protocol;
			}
			m_target = target;
		}
		else
		{
			Text = "Add Target".Localize();
			if (defaultPortNumber > 0)
			{
				txtPort.Text = defaultPortNumber.ToString();
				txtPort.ReadOnly = !canEditPortNumber;
			}
			if (cmbProtocol.Enabled)
			{
				cmbProtocol.SelectedIndex = 0;
			}
		}
	}

	private void m_btnOK_Click(object sender, EventArgs e)
	{
		if (ValidateAndFillInData())
		{
			base.DialogResult = DialogResult.OK;
		}
	}

	private bool ValidateAndFillInData()
	{
		string text = txtName.Text.Trim();
		string text2 = txtHost.Text.Trim();
		string s = txtPort.Text;
		int num = 0;
		if (StringUtil.IsNullOrEmptyOrWhitespace(text))
		{
			MessageBox.Show(this, "Fill target name".Localize());
			txtName.Focus();
			return false;
		}
		if (StringUtil.IsNullOrEmptyOrWhitespace(text2))
		{
			MessageBox.Show(this, "Fill host name".Localize());
			txtHost.Focus();
			return false;
		}
		try
		{
			num = int.Parse(s);
			if (num < 0 || num > 65535)
			{
				throw new Exception();
			}
		}
		catch
		{
			MessageBox.Show(this, string.Format("Invalid port number".Localize(), 0, 65535));
			return false;
		}
		if (m_target == null)
		{
			m_target = new Target(text, text2, num);
			m_changed = true;
		}
		else if (m_target.Name != text || m_target.Host != text2 || m_target.Port != num)
		{
			m_target.Set(text, text2, num);
			m_changed = true;
		}
		if (cmbProtocol.Enabled)
		{
			m_target.Protocol = (string)cmbProtocol.SelectedItem;
			m_changed = true;
		}
		return true;
	}

	public Target GetTarget()
	{
		if (m_target == null)
		{
			throw new Exception("Target is empty");
		}
		return m_target;
	}

	private void m_btnCancel_Click(object sender, EventArgs e)
	{
		m_target = null;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.NetworkTargetServices.TargetEditDialog));
		this.grpTarget = new System.Windows.Forms.GroupBox();
		this.lblPort = new System.Windows.Forms.Label();
		this.lblHost = new System.Windows.Forms.Label();
		this.lblName = new System.Windows.Forms.Label();
		this.txtPort = new System.Windows.Forms.TextBox();
		this.txtHost = new System.Windows.Forms.TextBox();
		this.txtName = new System.Windows.Forms.TextBox();
		this.m_btnOK = new System.Windows.Forms.Button();
		this.m_btnCancel = new System.Windows.Forms.Button();
		this.lblProtocol = new System.Windows.Forms.Label();
		this.cmbProtocol = new System.Windows.Forms.ComboBox();
		this.grpTarget.SuspendLayout();
		base.SuspendLayout();
		this.grpTarget.Controls.Add(this.cmbProtocol);
		this.grpTarget.Controls.Add(this.lblProtocol);
		this.grpTarget.Controls.Add(this.lblPort);
		this.grpTarget.Controls.Add(this.lblHost);
		this.grpTarget.Controls.Add(this.lblName);
		this.grpTarget.Controls.Add(this.txtPort);
		this.grpTarget.Controls.Add(this.txtHost);
		this.grpTarget.Controls.Add(this.txtName);
		resources.ApplyResources(this.grpTarget, "grpTarget");
		this.grpTarget.Name = "grpTarget";
		this.grpTarget.TabStop = false;
		resources.ApplyResources(this.lblPort, "lblPort");
		this.lblPort.Name = "lblPort";
		resources.ApplyResources(this.lblHost, "lblHost");
		this.lblHost.Name = "lblHost";
		resources.ApplyResources(this.lblName, "lblName");
		this.lblName.Name = "lblName";
		resources.ApplyResources(this.txtPort, "txtPort");
		this.txtPort.Name = "txtPort";
		resources.ApplyResources(this.txtHost, "txtHost");
		this.txtHost.Name = "txtHost";
		resources.ApplyResources(this.txtName, "txtName");
		this.txtName.Name = "txtName";
		resources.ApplyResources(this.m_btnOK, "m_btnOK");
		this.m_btnOK.Name = "m_btnOK";
		this.m_btnOK.UseVisualStyleBackColor = true;
		this.m_btnOK.Click += new System.EventHandler(m_btnOK_Click);
		this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
		this.m_btnCancel.Name = "m_btnCancel";
		this.m_btnCancel.UseVisualStyleBackColor = true;
		this.m_btnCancel.Click += new System.EventHandler(m_btnCancel_Click);
		resources.ApplyResources(this.lblProtocol, "lblProtocol");
		this.lblProtocol.Name = "lblProtocol";
		this.cmbProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cmbProtocol.FormattingEnabled = true;
		resources.ApplyResources(this.cmbProtocol, "cmbProtocol");
		this.cmbProtocol.Name = "cmbProtocol";
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_btnCancel);
		base.Controls.Add(this.m_btnOK);
		base.Controls.Add(this.grpTarget);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.Name = "TargetEditDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.grpTarget.ResumeLayout(false);
		this.grpTarget.PerformLayout();
		base.ResumeLayout(false);
	}
}
