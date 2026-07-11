using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class PopupSlider : Form
{
	public class PopupSliderEventArgs : EventArgs
	{
		public float Current { get; private set; }

		public PopupSliderEventArgs(float current)
		{
			Current = current;
		}
	}

	public delegate void PopupSliderHandler(object sender, PopupSliderEventArgs e);

	private IContainer components = null;

	private TrackBar trackBar1;

	public TrackBar TrackBar => trackBar1;

	public float MinValue { get; set; }

	public float MaxValue { get; set; }

	public int Steps { get; set; }

	public float Current => (float)trackBar1.Value / (float)Steps * (MaxValue - MinValue) + MinValue;

	private float HiddenValue { get; set; }

	public event PopupSliderHandler CurrentValueChanged;

	public PopupSlider()
	{
		InitializeComponent();
		trackBar1.LostFocus += trackBar1_LostFocus;
	}

	private void UpdateTrackBar()
	{
		trackBar1.Minimum = 0;
		trackBar1.Maximum = Steps;
		trackBar1.SmallChange = 1;
		trackBar1.LargeChange = Steps / 10;
		trackBar1.Value = (int)(Math.Max(MinValue, HiddenValue - MinValue) / (MaxValue - MinValue) * (float)Steps);
	}

	public void ShowPopup(int x, int y, float current)
	{
		ShowPopup(x, y, 0f, 1f, current, 50);
	}

	public void ShowPopup(int x, int y, float min, float max, float current, int steps)
	{
		MinValue = min;
		MaxValue = max;
		Steps = steps;
		HiddenValue = Math.Min(Math.Max(min, current), max);
		UpdateTrackBar();
		base.Location = new Point(x, y);
		Show();
		trackBar1.Focus();
	}

	private void trackBar1_LostFocus(object sender, EventArgs e)
	{
		CommitChanges();
	}

	private void trackBar1_ValueChanged(object sender, EventArgs e)
	{
		this.CurrentValueChanged?.Invoke(this, new PopupSliderEventArgs(Current));
	}

	private void trackBar1_MouseUp(object sender, MouseEventArgs e)
	{
		CommitChanges();
	}

	private void CommitChanges()
	{
		Hide();
		this.CurrentValueChanged?.Invoke(this, new PopupSliderEventArgs(Current));
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
		this.trackBar1 = new System.Windows.Forms.TrackBar();
		((System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
		base.SuspendLayout();
		this.trackBar1.CausesValidation = false;
		this.trackBar1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.trackBar1.Location = new System.Drawing.Point(0, 0);
		this.trackBar1.Name = "trackBar1";
		this.trackBar1.Size = new System.Drawing.Size(150, 27);
		this.trackBar1.TabIndex = 0;
		this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.None;
		this.trackBar1.ValueChanged += new System.EventHandler(trackBar1_ValueChanged);
		this.trackBar1.MouseUp += new System.Windows.Forms.MouseEventHandler(trackBar1_MouseUp);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CausesValidation = false;
		base.ClientSize = new System.Drawing.Size(150, 27);
		base.Controls.Add(this.trackBar1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "PopupSlider";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "PopupSlider";
		base.TopMost = true;
		((System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
