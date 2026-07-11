using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

[ProvideProperty("EnableVSStyle", typeof(ToolStrip))]
public class VisualStudioToolStripExtender : Component, IExtenderProvider
{
	private class ToolStripProperties
	{
		private VsVersion version;

		private readonly ToolStrip strip;

		private readonly Dictionary<ToolStripItem, string> menuText = new Dictionary<ToolStripItem, string>();

		public VsVersion VsVersion
		{
			get
			{
				return version;
			}
			set
			{
				version = value;
				UpdateMenuText(version == VsVersion.Vs2012 || version == VsVersion.Vs2013);
			}
		}

		public ToolStripProperties(ToolStrip toolstrip)
		{
			if (toolstrip == null)
			{
				throw new ArgumentNullException("toolstrip");
			}
			strip = toolstrip;
			if (strip is MenuStrip)
			{
				SaveMenuStripText();
			}
		}

		private void SaveMenuStripText()
		{
			foreach (ToolStripItem item in strip.Items)
			{
				menuText.Add(item, item.Text);
			}
		}

		public void UpdateMenuText(bool caps)
		{
			foreach (ToolStripItem key in menuText.Keys)
			{
				string text = menuText[key];
				key.Text = (caps ? text.ToUpper() : text);
			}
		}
	}

	public enum VsVersion
	{
		Unkown,
		Vs2003,
		Vs2005,
		Vs2008,
		Vs2010,
		Vs2012,
		Vs2013,
		Vs2015
	}

	private readonly Dictionary<ToolStrip, ToolStripProperties> strips = new Dictionary<ToolStrip, ToolStripProperties>();

	private IContainer components;

	public ToolStripRenderer DefaultRenderer { get; set; }

	public VisualStudioToolStripExtender()
	{
		InitializeComponent();
	}

	public VisualStudioToolStripExtender(IContainer container)
	{
		container.Add(this);
		InitializeComponent();
	}

	public bool CanExtend(object extendee)
	{
		return extendee is ToolStrip;
	}

	[DefaultValue(false)]
	public VsVersion GetStyle(ToolStrip strip)
	{
		if (strips.ContainsKey(strip))
		{
			return strips[strip].VsVersion;
		}
		return VsVersion.Unkown;
	}

	public void SetStyle(ToolStrip strip, VsVersion version, ThemeBase theme)
	{
		ToolStripProperties toolStripProperties = null;
		if (!strips.ContainsKey(strip))
		{
			toolStripProperties = new ToolStripProperties(strip)
			{
				VsVersion = version
			};
			strips.Add(strip, toolStripProperties);
		}
		else
		{
			toolStripProperties = strips[strip];
		}
		if (theme == null)
		{
			if (DefaultRenderer != null)
			{
				strip.Renderer = DefaultRenderer;
			}
		}
		else
		{
			theme.ApplyTo(strip);
		}
		toolStripProperties.VsVersion = version;
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
		components = new Container();
	}
}
