using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.VersionControl;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class VersionControlSetupForm : Form
{
	private IContainer components;

	private ComboBox cbUser;

	private Label label1;

	private Label label2;

	private TextBox txtProject;

	private TextBox txtServer;

	private Label label3;

	private Label label4;

	private ComboBox cbWorkspace;

	private Button btnCancel;

	private Button btnOK;

	public string Server { get; private set; }

	public string User { get; private set; }

	public string Workspace { get; private set; }

	public VersionControlSetupForm(string projName)
	{
		InitializeComponent();
		UpdateProjectText(projName);
		UpdateServerText("helix.internal.firaxis.com:1667");
		txtServer.LostFocus += Server_LostFocus;
		cbUser.LostFocus += User_LostFocus;
		cbUser.SelectedIndexChanged += User_LostFocus;
		cbWorkspace.LostFocus += Workspace_LostFocus;
	}

	private void PopulateUserList()
	{
		using (new WaitCursor())
		{
			VersionControlContext context = new VersionControlContext("", Server, "", 30000);
			IVersionControlServer versionControlServer = null;
			cbUser.Items.Clear();
			try
			{
				versionControlServer = new PerforceVersionControlServer(context);
			}
			catch (System.Exception ex)
			{
				MessageBoxes.Show("Failed to connect to version control server. Error=\"" + ex.Message + "\"", "Error Connecting to Version Control", MessageBoxButton.OKCancel, MessageBoxImage.Information);
				return;
			}
			foreach (string user in versionControlServer.Users)
			{
				cbUser.Items.Add(user);
			}
		}
	}

	private void PopulateWorkspaceList()
	{
		using (new WaitCursor())
		{
			PerforceVersionControlServer perforceVersionControlServer = new PerforceVersionControlServer(new VersionControlContext("", Server, User, 30000));
			cbWorkspace.Items.Clear();
			foreach (IVersionControlWorkspace workspace in ((IVersionControlServer)perforceVersionControlServer).Workspaces)
			{
				cbWorkspace.Items.Add(workspace.Name);
			}
		}
	}

	private void Server_LostFocus(object sender, EventArgs e)
	{
		if (!(txtServer.Text == Server))
		{
			Server = txtServer.Text;
			PopulateUserList();
		}
	}

	private void UpdateProjectText(string projName)
	{
		txtProject.Text = projName;
	}

	private void UpdateServerText(string serverAddr)
	{
		Server = serverAddr;
		txtServer.Text = serverAddr;
		PopulateUserList();
	}

	private void User_LostFocus(object sender, EventArgs e)
	{
		if (!(cbUser.Text == User))
		{
			User = cbUser.Text;
			PopulateWorkspaceList();
		}
	}

	private void Workspace_LostFocus(object sender, EventArgs e)
	{
		if (!(cbWorkspace.Text == Workspace))
		{
			Workspace = cbWorkspace.Text;
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
		this.cbUser = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.txtProject = new System.Windows.Forms.TextBox();
		this.txtServer = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.cbWorkspace = new System.Windows.Forms.ComboBox();
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnOK = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.cbUser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cbUser.FormattingEnabled = true;
		this.cbUser.Location = new System.Drawing.Point(16, 126);
		this.cbUser.Name = "cbUser";
		this.cbUser.Size = new System.Drawing.Size(320, 21);
		this.cbUser.TabIndex = 1;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(15, 48);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(38, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Server";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(15, 13);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(40, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Project";
		this.txtProject.Location = new System.Drawing.Point(60, 10);
		this.txtProject.Name = "txtProject";
		this.txtProject.ReadOnly = true;
		this.txtProject.Size = new System.Drawing.Size(276, 20);
		this.txtProject.TabIndex = 3;
		this.txtServer.Location = new System.Drawing.Point(16, 66);
		this.txtServer.Name = "txtServer";
		this.txtServer.Size = new System.Drawing.Size(320, 20);
		this.txtServer.TabIndex = 0;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(15, 108);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(29, 13);
		this.label3.TabIndex = 5;
		this.label3.Text = "User";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(17, 169);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(62, 13);
		this.label4.TabIndex = 7;
		this.label4.Text = "Workspace";
		this.cbWorkspace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.cbWorkspace.FormattingEnabled = true;
		this.cbWorkspace.Location = new System.Drawing.Point(18, 187);
		this.cbWorkspace.Name = "cbWorkspace";
		this.cbWorkspace.Size = new System.Drawing.Size(320, 21);
		this.cbWorkspace.TabIndex = 2;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(262, 249);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 4;
		this.btnCancel.Text = "&Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Location = new System.Drawing.Point(181, 249);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(75, 23);
		this.btnOK.TabIndex = 3;
		this.btnOK.Text = "&OK";
		this.btnOK.UseVisualStyleBackColor = true;
		base.AcceptButton = this.btnOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(349, 284);
		base.Controls.Add(this.btnOK);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.cbWorkspace);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.txtServer);
		base.Controls.Add(this.txtProject);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.cbUser);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "VersionControlSetupForm";
		base.ShowIcon = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		this.Text = "Version Control Setup";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
