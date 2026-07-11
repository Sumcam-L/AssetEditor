using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetEditing;

public class PlatformSelectorControl : UserControl
{
	private IPlatformSelectorContext m_context;

	private List<CheckBox> m_createdControls = new List<CheckBox>();

	private IContainer components;

	private FlowLayoutPanel flowCtl = new FlowLayoutPanel();

	public PlatformSelectorControl()
	{
		InitializeComponent();
		AddPlatformCheckboxes();
	}

	public void Bind(IPlatformSelectorContext context)
	{
		if (m_context != null)
		{
			ClearCheckedStatus();
		}
		m_context = context;
		if (m_context != null)
		{
			SetCheckedStatus();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (CheckBox createdControl in m_createdControls)
			{
				flowCtl.Controls.Remove(createdControl);
				createdControl.CheckedChanged -= UpdateAllowedPlatforms;
				createdControl.Dispose();
			}
			m_createdControls.Clear();
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private int AddPlatformCheckbox(Platforms platform, string displayName, int xPos, EventHandler handler)
	{
		CheckBox checkBox = new CheckBox();
		checkBox.Text = displayName;
		checkBox.Tag = platform;
		checkBox.Location = new Point(xPos, 2);
		checkBox.AutoSize = true;
		checkBox.TextAlign = ContentAlignment.MiddleLeft;
		if (handler != null)
		{
			checkBox.CheckedChanged += handler;
			m_createdControls.Add(checkBox);
		}
		flowCtl.Controls.Add(checkBox);
		return checkBox.Width + 5;
	}

	private void AddPlatformCheckboxes()
	{
		int num = 10;
		foreach (Platforms usablePlatform in PlatformsAssistant.GetUsablePlatforms())
		{
			num += AddPlatformCheckbox(usablePlatform, PlatformsAssistant.GetNameFromPlatform(usablePlatform), num, UpdateAllowedPlatforms);
		}
	}

	private void ClearCheckedStatus()
	{
		foreach (CheckBox createdControl in m_createdControls)
		{
			createdControl.CheckedChanged -= UpdateAllowedPlatforms;
			createdControl.Checked = false;
			createdControl.CheckedChanged += UpdateAllowedPlatforms;
		}
	}

	private CheckBox GetCheckBoxByPlatform(Platforms platform)
	{
		return m_createdControls.Where((CheckBox control) => ((Platforms)control.Tag & platform) > Platforms.PLATFORM_INVALID).FirstOrDefault();
	}

	private void SetCheckedStatus()
	{
		Platforms platforms = Platforms.PLATFORM_INVALID;
		foreach (Platforms allowedPlatform in m_context.AllowedPlatforms)
		{
			if (allowedPlatform != Platforms.PLATFORM_INVALID && allowedPlatform != Platforms.PLATFORM_ALL)
			{
				CheckBox checkBoxByPlatform = GetCheckBoxByPlatform(allowedPlatform);
				checkBoxByPlatform.CheckedChanged -= UpdateAllowedPlatforms;
				platforms |= allowedPlatform;
				checkBoxByPlatform.Checked = true;
				checkBoxByPlatform.CheckedChanged += UpdateAllowedPlatforms;
			}
		}
	}

	private void UpdateAllowedPlatforms(object sender, EventArgs e)
	{
		if (sender is CheckBox checkBox)
		{
			Platforms platform = (Platforms)checkBox.Tag;
			if (checkBox.Checked)
			{
				m_context.AllowPlatform(platform);
			}
			else
			{
				m_context.RemovePlatform(platform);
			}
		}
		ClearCheckedStatus();
		SetCheckedStatus();
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		this.flowCtl.Dock = System.Windows.Forms.DockStyle.Fill;
		this.flowCtl.Location = new System.Drawing.Point(0, 0);
		this.flowCtl.Name = "flowCtl";
		this.flowCtl.Size = new System.Drawing.Size(515, 214);
		this.flowCtl.TabIndex = 0;
		this.flowCtl.AutoSize = true;
		base.Controls.Add(this.flowCtl);
		base.Name = "PlatformSelectorControl";
		base.Size = new System.Drawing.Size(515, 214);
		base.ResumeLayout(false);
	}
}
