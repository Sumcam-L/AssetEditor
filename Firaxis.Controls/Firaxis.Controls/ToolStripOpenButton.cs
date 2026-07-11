using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Firaxis.Controls.Properties;
using Firaxis.Utility;

namespace Firaxis.Controls;

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
public class ToolStripOpenButton : ToolStripSplitButton
{
	private ToolStripMenuItem butNew;

	private ToolStripMenuItem butClose;

	[Category("Behavior")]
	[Description("Set to True for the button support the New command")]
	[DefaultValue(true)]
	public bool SupportsNew { get; set; }

	[Category("Behavior")]
	[Description("Set to True for the Open By File dialog to support multi-select")]
	[DefaultValue(false)]
	public bool SupportsMultiSelect { get; set; }

	[Category("Behavior")]
	[Description("Set to True for the button support the Close command")]
	[DefaultValue(false)]
	public bool SupportsClose { get; set; }

	[Category("Text")]
	public string TextNew
	{
		get
		{
			return butNew.Text;
		}
		set
		{
			butNew.Text = value;
		}
	}

	[Category("Text")]
	public string TextClose
	{
		get
		{
			return butClose.Text;
		}
		set
		{
			butClose.Text = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new Color ImageTransparentColor
	{
		get
		{
			return base.ImageTransparentColor;
		}
		set
		{
			base.ImageTransparentColor = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new ToolStripItemDisplayStyle DisplayStyle
	{
		get
		{
			return base.DisplayStyle;
		}
		set
		{
			base.DisplayStyle = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new Image Image
	{
		get
		{
			return base.Image;
		}
		set
		{
			base.Image = value;
		}
	}

	[Category("Action")]
	public event EventHandler CommandNewPressed;

	[Category("Action")]
	public event EventHandler CommandOpenPressed;

	[Category("Action")]
	public event EventHandler CommandClosePressed;

	public ToolStripOpenButton()
	{
		InitializeComponent();
		DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
		Image = Firaxis.Controls.Properties.Resources.dir_closed;
		Text = "Open";
		SupportsNew = true;
		SupportsClose = false;
	}

	private void InitializeComponent()
	{
		butNew = new ToolStripMenuItem();
		butClose = new ToolStripMenuItem();
		butNew.Image = Firaxis.Controls.Properties.Resources.file_document;
		butNew.Name = "butNew";
		butNew.Size = new Size(149, 22);
		butNew.Text = "New";
		butNew.Click += butNew_Click;
		butClose.Image = Firaxis.Controls.Properties.Resources.tool_delete;
		butClose.Name = "butClose";
		butClose.Size = new Size(149, 22);
		butClose.Text = "Close";
		butClose.Click += butClose_Click;
		base.DropDownItems.AddRange(new ToolStripItem[2] { butNew, butClose });
		base.ButtonClick += ToolStripOpenButton_ButtonClick;
		base.DropDownOpening += ToolStripOpenButton_DropDownOpening;
	}

	private void butNew_Click(object sender, EventArgs e)
	{
		if (!Context.InDesignMode)
		{
			this.CommandNewPressed?.Invoke(this, e);
		}
	}

	private void butClose_Click(object sender, EventArgs e)
	{
		if (!Context.InDesignMode)
		{
			this.CommandClosePressed?.Invoke(this, e);
		}
	}

	private void ToolStripOpenButton_DropDownOpening(object sender, EventArgs e)
	{
		butNew.Visible = SupportsNew;
		butClose.Visible = SupportsClose;
	}

	private void ToolStripOpenButton_ButtonClick(object sender, EventArgs e)
	{
		this.CommandOpenPressed?.Invoke(this, e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			ShowDropDown();
		}
		base.OnMouseUp(e);
	}
}
