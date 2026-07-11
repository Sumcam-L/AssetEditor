using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Firaxis.MVVMBase.Controls;

public class WpfContentHost : System.Windows.Forms.UserControl
{
	private IContainer components;

	private ElementHost Host { get; set; }

	private ContentControl ContentDisplay { get; set; }

	public object Content
	{
		get
		{
			return ContentDisplay.Content;
		}
		set
		{
			ContentDisplay.Content = value;
		}
	}

	public WpfContentHost()
	{
		InitializeComponent();
	}

	public WpfContentHost(object content)
		: this()
	{
		ContentDisplay.Content = content;
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.ContentDisplay = new System.Windows.Controls.ContentControl();
		this.Host = new System.Windows.Forms.Integration.ElementHost();
		this.Host.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.Host.Dock = System.Windows.Forms.DockStyle.Fill;
		this.Host.Child = this.ContentDisplay;
		base.Controls.Add(this.Host);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
			(Content as IDisposable)?.Dispose();
			if (Host != null)
			{
				Host.Dispose();
				Host = null;
			}
		}
		base.Dispose(disposing);
	}
}
