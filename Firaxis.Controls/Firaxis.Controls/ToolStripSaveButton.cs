using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Firaxis.Controls.Properties;

namespace Firaxis.Controls;

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
public class ToolStripSaveButton : ToolStripSplitButton
{
	private ToolStripMenuItem butSaveAs;

	private ToolStripMenuItem butSaveAll;

	private ToolStripMenuItem butSaveSubmit;

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

	[Category("Behavior")]
	[Description("Set to True for the button support the Save As command")]
	[DefaultValue(true)]
	public bool SupportsSaveAs { get; set; }

	[Category("Behavior")]
	[Description("Set to True for the button support the Save All command")]
	[DefaultValue(false)]
	public bool SupportsSaveAll { get; set; }

	[Category("Behavior")]
	[Description("Set to True for the button support the Save And Submit command")]
	[DefaultValue(false)]
	public bool SupportsSaveSubmit { get; set; }

	[Category("Action")]
	public event EventHandler CommandSavePressed;

	[Category("Action")]
	public event EventHandler CommandSaveAsPressed;

	[Category("Action")]
	public event EventHandler CommandSaveAllPressed;

	[Category("Action")]
	public event EventHandler CommandSaveSubmitPressed;

	public ToolStripSaveButton()
	{
		InitializeComponent();
		DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
		Image = Firaxis.Controls.Properties.Resources.save;
		Text = "Save";
		SupportsSaveAs = true;
		SupportsSaveAll = false;
		SupportsSaveSubmit = false;
	}

	private void InitializeComponent()
	{
		butSaveAs = new ToolStripMenuItem();
		butSaveAll = new ToolStripMenuItem();
		butSaveSubmit = new ToolStripMenuItem();
		butSaveAs.Image = Firaxis.Controls.Properties.Resources.save;
		butSaveAs.Name = "butSaveAs";
		butSaveAs.Size = new Size(123, 22);
		butSaveAs.Text = "Save As...";
		butSaveAs.Click += butSaveAs_Click;
		butSaveAll.Image = Firaxis.Controls.Properties.Resources.save_all;
		butSaveAll.Name = "butSaveAll";
		butSaveAll.Size = new Size(123, 22);
		butSaveAll.Text = "Save All";
		butSaveAll.Click += butSaveAll_Click;
		butSaveSubmit.Image = Firaxis.Controls.Properties.Resources.save_submit;
		butSaveSubmit.Name = "butSaveSubmit";
		butSaveSubmit.Size = new Size(123, 22);
		butSaveSubmit.Text = "Save and Submit";
		butSaveSubmit.Click += butSaveSubmit_Click;
		base.DropDownItems.AddRange(new ToolStripItem[3] { butSaveAs, butSaveAll, butSaveSubmit });
		base.Click += ToolStripSaveButton_Click;
		base.ButtonClick += ToolStripSaveButton_ButtonClick;
		base.DropDownOpening += ToolStripSaveButton_DropDownOpening;
	}

	private void ToolStripSaveButton_Click(object sender, EventArgs e)
	{
	}

	private void butSaveAs_Click(object sender, EventArgs e)
	{
		this.CommandSaveAsPressed?.Invoke(this, e);
	}

	private void butSaveAll_Click(object sender, EventArgs e)
	{
		this.CommandSaveAllPressed?.Invoke(this, e);
	}

	private void butSaveSubmit_Click(object sender, EventArgs e)
	{
		this.CommandSaveSubmitPressed?.Invoke(this, e);
	}

	private void ToolStripSaveButton_ButtonClick(object sender, EventArgs e)
	{
		this.CommandSavePressed?.Invoke(this, e);
	}

	private void ToolStripSaveButton_DropDownOpening(object sender, EventArgs e)
	{
		butSaveAs.Visible = SupportsSaveAs;
		butSaveAll.Visible = SupportsSaveAll;
		butSaveSubmit.Visible = SupportsSaveSubmit;
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
