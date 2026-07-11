using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Controls.Properties;

namespace Firaxis.Controls;

public class TimeLineControl : UserControl
{
	private TimeLineTrackPanel timeLineTrackPanel1;

	private bool inInit;

	private IContainer components = null;

	private SplitterControl splitterControl1;

	private TimeRulerControl timeRulerControl1;

	private ScrollableTree scrollableTree1;

	private ToolStrip toolStrip1;

	private Panel panel1;

	private ToolStripTrackBar toolStripTrackBar1;

	private ToolStripButton butLockKeys;

	[Category("Graph")]
	[DefaultValue(false)]
	public bool AllowEdit { get; set; }

	public bool GoofyFoot { get; set; }

	public ScrollableTree ScrollableTree => scrollableTree1;

	public TimeRulerControl TimeRuler => timeRulerControl1;

	public TimeLineTrackPanel TimeTrack => timeLineTrackPanel1;

	public ToolStrip ToolStrip => toolStrip1;

	public TrackBar TrackBar => toolStripTrackBar1.TrackBar;

	public bool VerticalConstraint { get; set; }

	public bool HorizontalConstraint { get; set; }

	public int SplitterDistance
	{
		get
		{
			return splitterControl1.SplitterDistance;
		}
		set
		{
			splitterControl1.SplitterDistance = value;
		}
	}

	public TimeLineControl()
	{
		InitializeComponent();
		timeLineTrackPanel1 = new TimeLineTrackPanel(this, panel1);
		panel1.Controls.Add(timeLineTrackPanel1);
		TrackBar trackBar = toolStripTrackBar1.TrackBar;
		trackBar.SetRange(30, 1500);
		trackBar.SmallChange = (trackBar.Maximum - trackBar.Minimum) / 100;
		inInit = true;
		toolStripTrackBar1.TrackBar.Value = ComputeMajorScale((int)TimeRuler.MajorScale);
		inInit = false;
	}

	public void SetMajorScale(float scale)
	{
		TimeRuler.MajorScale = Math.Min(Math.Max(scale, TrackBar.Minimum), TrackBar.Maximum);
		inInit = true;
		int val = ComputeMajorScale((int)TimeRuler.MajorScale);
		toolStripTrackBar1.TrackBar.Value = Math.Min(Math.Max(val, toolStripTrackBar1.TrackBar.Minimum), toolStripTrackBar1.TrackBar.Maximum);
		inInit = false;
	}

	private int ComputeMajorScale(int val)
	{
		if (GoofyFoot)
		{
			return toolStripTrackBar1.TrackBar.Maximum - val + toolStripTrackBar1.TrackBar.Minimum;
		}
		return val + toolStripTrackBar1.TrackBar.Minimum;
	}

	private void toolStripTrackBar1_ValueChanged(object sender, EventArgs e)
	{
		if (!inInit)
		{
			TimeRuler.MajorScale = ComputeMajorScale(toolStripTrackBar1.TrackBar.Value);
		}
	}

	private void butTimeBegin_Click(object sender, EventArgs e)
	{
		TimeRuler.CurrentTime = 0f;
		TimeRuler.EnsureVisible();
	}

	private void butLockKeys_Click(object sender, EventArgs e)
	{
		TimeTrack.LockKeys = !TimeTrack.LockKeys;
		butLockKeys.Checked = TimeTrack.LockKeys;
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
		this.splitterControl1 = new Firaxis.Controls.SplitterControl();
		this.scrollableTree1 = new Firaxis.Controls.ScrollableTree();
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.toolStripTrackBar1 = new Firaxis.Controls.ToolStripTrackBar();
		this.butLockKeys = new System.Windows.Forms.ToolStripButton();
		this.panel1 = new System.Windows.Forms.Panel();
		this.timeRulerControl1 = new Firaxis.Controls.TimeRulerControl();
		this.splitterControl1.Panel1.SuspendLayout();
		this.splitterControl1.Panel2.SuspendLayout();
		this.splitterControl1.SuspendLayout();
		this.toolStrip1.SuspendLayout();
		base.SuspendLayout();
		this.splitterControl1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitterControl1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
		this.splitterControl1.Location = new System.Drawing.Point(0, 0);
		this.splitterControl1.Name = "splitterControl1";
		this.splitterControl1.Panel1.Controls.Add(this.scrollableTree1);
		this.splitterControl1.Panel1.Controls.Add(this.toolStrip1);
		this.splitterControl1.Panel1MinSize = 48;
		this.splitterControl1.Panel2.Controls.Add(this.panel1);
		this.splitterControl1.Panel2.Controls.Add(this.timeRulerControl1);
		this.splitterControl1.Size = new System.Drawing.Size(650, 375);
		this.splitterControl1.SplitterDistance = 243;
		this.splitterControl1.SplitterWidth = 3;
		this.splitterControl1.TabIndex = 0;
		this.scrollableTree1.BackColor = System.Drawing.Color.White;
		this.scrollableTree1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.scrollableTree1.ForeColor = System.Drawing.Color.White;
		this.scrollableTree1.Location = new System.Drawing.Point(0, 25);
		this.scrollableTree1.Name = "scrollableTree1";
		this.scrollableTree1.Size = new System.Drawing.Size(243, 350);
		this.scrollableTree1.TabIndex = 0;
		this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.toolStripTrackBar1, this.butLockKeys });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
		this.toolStrip1.Size = new System.Drawing.Size(243, 25);
		this.toolStrip1.TabIndex = 0;
		this.toolStrip1.Text = "toolStrip1";
		this.toolStripTrackBar1.Name = "toolStripTrackBar1";
		this.toolStripTrackBar1.Size = new System.Drawing.Size(104, 22);
		this.toolStripTrackBar1.Text = "toolStripTrackBar";
		this.toolStripTrackBar1.ValueChanged += new System.EventHandler(toolStripTrackBar1_ValueChanged);
		this.butLockKeys.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.butLockKeys.Image = Firaxis.Controls.Properties.Resources.lock_keys;
		this.butLockKeys.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.butLockKeys.Name = "butLockKeys";
		this.butLockKeys.Size = new System.Drawing.Size(23, 22);
		this.butLockKeys.Text = "Lock Keys";
		this.butLockKeys.ToolTipText = "Toggle locking key movements";
		this.butLockKeys.Click += new System.EventHandler(butLockKeys_Click);
		this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(0, 25);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(404, 350);
		this.panel1.TabIndex = 1;
		this.timeRulerControl1.BackColor = System.Drawing.Color.FromArgb(232, 232, 232);
		this.timeRulerControl1.CurrentTime = 0f;
		this.timeRulerControl1.Dock = System.Windows.Forms.DockStyle.Top;
		this.timeRulerControl1.ForeColor = System.Drawing.Color.Black;
		this.timeRulerControl1.Location = new System.Drawing.Point(0, 0);
		this.timeRulerControl1.MajorScale = 540f;
		this.timeRulerControl1.Name = "timeRulerControl1";
		this.timeRulerControl1.Origin = 0f;
		this.timeRulerControl1.ShuttleColor = System.Drawing.Color.FromArgb(216, 30, 0);
		this.timeRulerControl1.ShuttleVisible = true;
		this.timeRulerControl1.Size = new System.Drawing.Size(404, 25);
		this.timeRulerControl1.TabIndex = 0;
		this.timeRulerControl1.TickColor = System.Drawing.Color.FromArgb(186, 182, 169);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splitterControl1);
		base.Name = "TimeLineControl";
		base.Size = new System.Drawing.Size(650, 375);
		this.splitterControl1.Panel1.ResumeLayout(false);
		this.splitterControl1.Panel1.PerformLayout();
		this.splitterControl1.Panel2.ResumeLayout(false);
		this.splitterControl1.ResumeLayout(false);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		base.ResumeLayout(false);
	}
}
