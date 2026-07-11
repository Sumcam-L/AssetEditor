using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Firaxis.Controls;

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
public class ToolStripTrackBar : ToolStripControlHost
{
	private class ToolStripHostedTrackBar : TrackBar
	{
	}

	public TrackBar TrackBar => base.Control as TrackBar;

	[DefaultValue(0)]
	public int Value
	{
		get
		{
			return TrackBar.Value;
		}
		set
		{
			TrackBar.Value = value;
		}
	}

	protected override Size DefaultSize => new Size(200, 16);

	public event EventHandler ValueChanged;

	public ToolStripTrackBar()
		: base(CreateControlInstance())
	{
	}

	private static Control CreateControlInstance()
	{
		TrackBar trackBar = new ToolStripHostedTrackBar();
		trackBar.AutoSize = false;
		trackBar.Height = 16;
		return trackBar;
	}

	protected override void OnSubscribeControlEvents(Control control)
	{
		base.OnSubscribeControlEvents(control);
		TrackBar trackBar = control as TrackBar;
		trackBar.ValueChanged += trackBar_ValueChanged;
	}

	protected override void OnUnsubscribeControlEvents(Control control)
	{
		base.OnUnsubscribeControlEvents(control);
		TrackBar trackBar = control as TrackBar;
		trackBar.ValueChanged -= trackBar_ValueChanged;
	}

	private void trackBar_ValueChanged(object sender, EventArgs e)
	{
		if (this.ValueChanged != null)
		{
			this.ValueChanged(sender, e);
		}
	}
}
