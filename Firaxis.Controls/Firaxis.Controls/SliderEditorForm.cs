using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Firaxis.Controls;

public class SliderEditorForm : Form
{
	private IContainer components = null;

	private Button btnTrack;

	private TrackBar trackBar1;

	public IWindowsFormsEditorService WFES { get; set; }

	public double BarValue { get; private set; }

	public SliderEditorForm(int initialValue = 1)
	{
		InitializeComponent();
		base.TopLevel = false;
		if (initialValue > trackBar1.Maximum)
		{
			initialValue = trackBar1.Maximum;
		}
		if (initialValue < trackBar1.Minimum)
		{
			initialValue = trackBar1.Minimum;
		}
		trackBar1.Value = initialValue;
		SetButtonTrackText();
		base.FormClosed += HandleFormClosed;
	}

	protected virtual void HandleFormClosed(object sender, FormClosedEventArgs e)
	{
		if (WFES != null)
		{
			WFES.CloseDropDown();
		}
		base.FormClosed -= HandleFormClosed;
	}

	private void btnTrack_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void SetButtonTrackText()
	{
		BarValue = trackBar1.Value;
		btnTrack.Text = $"{BarValue:F2}";
	}

	private void trackBar1_ValueChanged(object sender, EventArgs e)
	{
		SetButtonTrackText();
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
		this.btnTrack = new System.Windows.Forms.Button();
		this.trackBar1 = new System.Windows.Forms.TrackBar();
		((System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
		base.SuspendLayout();
		this.btnTrack.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnTrack.Location = new System.Drawing.Point(157, 16);
		this.btnTrack.Name = "btnTrack";
		this.btnTrack.Size = new System.Drawing.Size(45, 23);
		this.btnTrack.TabIndex = 3;
		this.btnTrack.Text = "0";
		this.btnTrack.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.btnTrack.Click += new System.EventHandler(btnTrack_Click);
		this.trackBar1.LargeChange = 50;
		this.trackBar1.Location = new System.Drawing.Point(4, 19);
		this.trackBar1.Maximum = 1000;
		this.trackBar1.Minimum = 1;
		this.trackBar1.Name = "trackBar1";
		this.trackBar1.Size = new System.Drawing.Size(147, 45);
		this.trackBar1.SmallChange = 10;
		this.trackBar1.TabIndex = 4;
		this.trackBar1.TickFrequency = 50;
		this.trackBar1.Value = 1;
		this.trackBar1.ValueChanged += new System.EventHandler(trackBar1_ValueChanged);
		base.AcceptButton = this.btnTrack;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
		base.ClientSize = new System.Drawing.Size(212, 70);
		base.Controls.Add(this.trackBar1);
		base.Controls.Add(this.btnTrack);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SliderEditorForm";
		base.ShowInTaskbar = false;
		this.Text = "SliderEditorForm";
		((System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
