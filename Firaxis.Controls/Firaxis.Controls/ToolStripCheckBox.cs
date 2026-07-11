using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Firaxis.Controls;

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
public class ToolStripCheckBox : ToolStripControlHost
{
	public CheckBox CheckBox => base.Control as CheckBox;

	public bool Checked
	{
		get
		{
			return CheckBox.Checked;
		}
		set
		{
			CheckBox.Checked = value;
		}
	}

	protected override Size DefaultSize => new Size(200, 16);

	public event EventHandler CheckStateChanged;

	public ToolStripCheckBox()
		: base(CreateControlInstance())
	{
	}

	private static Control CreateControlInstance()
	{
		CheckBox checkBox = new CheckBox();
		checkBox.AutoSize = false;
		checkBox.Height = 16;
		return checkBox;
	}

	protected override void OnSubscribeControlEvents(Control control)
	{
		base.OnSubscribeControlEvents(control);
		CheckBox.CheckStateChanged += CheckBox_CheckStateChanged;
	}

	protected override void OnUnsubscribeControlEvents(Control control)
	{
		base.OnUnsubscribeControlEvents(control);
		CheckBox.CheckStateChanged -= CheckBox_CheckStateChanged;
	}

	private void CheckBox_CheckStateChanged(object sender, EventArgs e)
	{
		this.CheckStateChanged?.Invoke(sender, e);
	}
}
