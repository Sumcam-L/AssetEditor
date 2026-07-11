using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class AboutDialog : Form
{
	private LinkLabel linkLabel;

	private PictureBox pictureBox;

	private Panel clientPanel;

	private TextBox creditsTextBox;

	private Button okButton;

	private Button sysInfoButton;

	private RichTextBox m_richTextBox;

	private readonly Container components = null;

	private readonly string m_url;

	private static AboutSysInfoDialog s_sysInfoDialog;

	public AboutDialog(string title, string url, Control clientControl)
		: this(title, url, clientControl, null, null, addAtfInfo: false)
	{
	}

	public AboutDialog(string title, string url, Control clientControl, Image logo, IList<string> credits)
		: this(title, url, clientControl, logo, credits, addAtfInfo: false)
	{
	}

	public AboutDialog(string title, string url, Control clientControl, Image logo, IList<string> credits, bool addAtfInfo)
	{
		InitializeComponent();
		if (title == null)
		{
			title = "About".Localize();
		}
		Text = title;
		if (logo == null)
		{
			logo = ResourceUtil.GetImage(Resources.AtfLogoImage);
		}
		if (credits == null)
		{
			credits = new List<string>();
		}
		if (addAtfInfo)
		{
			Version version = AtfVersion.GetVersion();
			credits.Add(string.Format("Authoring Tools Framework (ATF {0}), by Ron Little, Jianhua Shen, Julianne Harrington, Alan Beckus, Matt Mahony, Pat O'Leary, Paul Skibitzke, and Max Elliott." + " Copyright © 2014 Sony Computer Entertainment America LLC".Localize("{0} is the version number"), version));
		}
		pictureBox.Size = logo.Size;
		pictureBox.Image = logo;
		if (clientControl != null)
		{
			clientControl.Dock = DockStyle.Fill;
			clientPanel.Controls.Add(clientControl);
			m_richTextBox = clientControl as RichTextBox;
			if (m_richTextBox != null)
			{
				m_richTextBox.LinkClicked += RichTextBoxOnLinkClicked;
			}
		}
		if (url != null)
		{
			m_url = url;
			linkLabel.Links.Add(linkLabel.Text.Length, url.Length, url);
			linkLabel.Text += url;
			linkLabel.MouseDown += linkLabel_MouseDown;
			linkLabel.Visible = true;
		}
		else
		{
			linkLabel.Visible = false;
		}
		if (credits.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string credit in credits)
			{
				stringBuilder.Append(credit);
				stringBuilder.Append(Environment.NewLine);
			}
			creditsTextBox.Text = stringBuilder.ToString();
		}
		else
		{
			base.Controls.Remove(creditsTextBox);
		}
	}

	private void RichTextBoxOnLinkClicked(object sender, LinkClickedEventArgs e)
	{
		Process.Start(e.LinkText);
	}

	private void linkLabel_MouseDown(object sender, MouseEventArgs e)
	{
		Process.Start(m_url);
		linkLabel.LinkVisited = true;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			if (m_richTextBox != null)
			{
				m_richTextBox.LinkClicked -= RichTextBoxOnLinkClicked;
				m_richTextBox = null;
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.AboutDialog));
		this.linkLabel = new System.Windows.Forms.LinkLabel();
		this.pictureBox = new System.Windows.Forms.PictureBox();
		this.clientPanel = new System.Windows.Forms.Panel();
		this.creditsTextBox = new System.Windows.Forms.TextBox();
		this.okButton = new System.Windows.Forms.Button();
		this.sysInfoButton = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).BeginInit();
		base.SuspendLayout();
		resources.ApplyResources(this.linkLabel, "linkLabel");
		this.linkLabel.Name = "linkLabel";
		this.linkLabel.TabStop = true;
		resources.ApplyResources(this.pictureBox, "pictureBox");
		this.pictureBox.BackColor = System.Drawing.SystemColors.Window;
		this.pictureBox.Name = "pictureBox";
		this.pictureBox.TabStop = false;
		resources.ApplyResources(this.clientPanel, "clientPanel");
		this.clientPanel.Name = "clientPanel";
		resources.ApplyResources(this.creditsTextBox, "creditsTextBox");
		this.creditsTextBox.Name = "creditsTextBox";
		this.creditsTextBox.ReadOnly = true;
		resources.ApplyResources(this.okButton, "okButton");
		this.okButton.Name = "okButton";
		this.okButton.UseVisualStyleBackColor = true;
		this.okButton.Click += new System.EventHandler(okButton_Click);
		resources.ApplyResources(this.sysInfoButton, "sysInfoButton");
		this.sysInfoButton.Name = "sysInfoButton";
		this.sysInfoButton.UseVisualStyleBackColor = true;
		this.sysInfoButton.Click += new System.EventHandler(sysInfoButton_Click);
		base.AcceptButton = this.okButton;
		resources.ApplyResources(this, "$this");
		this.BackColor = System.Drawing.SystemColors.Control;
		base.Controls.Add(this.sysInfoButton);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.creditsTextBox);
		base.Controls.Add(this.clientPanel);
		base.Controls.Add(this.pictureBox);
		base.Controls.Add(this.linkLabel);
		base.Name = "AboutDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.Load += new System.EventHandler(AboutDialog_Load);
		((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private void okButton_Click(object sender, EventArgs e)
	{
		Close();
		if (s_sysInfoDialog != null)
		{
			s_sysInfoDialog.Close();
		}
	}

	private void sysInfoButton_Click(object sender, EventArgs e)
	{
		if (s_sysInfoDialog == null)
		{
			s_sysInfoDialog = new AboutSysInfoDialog();
			s_sysInfoDialog.FormClosed += sysInfoDialog_FormClosed;
		}
		s_sysInfoDialog.BringToFront();
		s_sysInfoDialog.Show();
	}

	private static void sysInfoDialog_FormClosed(object sender, FormClosedEventArgs e)
	{
		s_sysInfoDialog = null;
	}

	private void AboutDialog_Load(object sender, EventArgs e)
	{
	}
}
