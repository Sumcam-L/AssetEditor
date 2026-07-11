using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class BoolInputControl : UserControl
{
	private bool m_value;

	private readonly string m_trueText;

	private readonly string m_falseText;

	private TrackBar m_trackBar;

	private TextBox m_textBox;

	private readonly Container components = null;

	public bool Value
	{
		get
		{
			return m_value;
		}
		set
		{
			if (value != m_value)
			{
				m_value = value;
				if (value)
				{
					m_textBox.Text = m_trueText;
					m_trackBar.Value = 1;
				}
				else
				{
					m_textBox.Text = m_falseText;
					m_trackBar.Value = 0;
				}
				OnValueChanged();
			}
		}
	}

	public Point TrackBarButtonOffset
	{
		get
		{
			int num = m_trackBar.Location.Y + m_trackBar.Height / 2;
			int num2 = (int)((float)(m_trackBar.Location.X + 10) + (float)((m_trackBar.Width - 20) * m_trackBar.Value) / (float)(m_trackBar.Maximum - m_trackBar.Minimum));
			return new Point(num2, num);
		}
	}

	public event EventHandler ValueChanged;

	public BoolInputControl(bool value, string trueText, string falseText)
	{
		InitializeComponent();
		m_trueText = trueText;
		m_falseText = falseText;
		m_textBox.Text = falseText;
		Value = value;
		m_trackBar.ValueChanged += control_ValueChanged;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		ControlPaint.DrawBorder3D(e.Graphics, base.ClientRectangle, Border3DStyle.Flat);
	}

	protected virtual void OnValueChanged()
	{
		if (this.ValueChanged != null)
		{
			this.ValueChanged(this, EventArgs.Empty);
		}
	}

	private void control_ValueChanged(object sender, EventArgs e)
	{
		Value = m_trackBar.Value == 1;
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.BoolInputControl));
		this.m_trackBar = new System.Windows.Forms.TrackBar();
		this.m_textBox = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.m_trackBar).BeginInit();
		base.SuspendLayout();
		resources.ApplyResources(this.m_trackBar, "_trackBar");
		this.m_trackBar.BackColor = System.Drawing.SystemColors.Window;
		this.m_trackBar.Maximum = 1;
		this.m_trackBar.Name = "_trackBar";
		this.m_trackBar.TickFrequency = 0;
		this.m_textBox.AcceptsReturn = true;
		this.m_textBox.AcceptsTab = true;
		this.m_textBox.BackColor = System.Drawing.SystemColors.Window;
		this.m_textBox.HideSelection = false;
		resources.ApplyResources(this.m_textBox, "_textBox");
		this.m_textBox.Name = "_textBox";
		this.m_textBox.ReadOnly = true;
		base.Controls.Add(this.m_textBox);
		base.Controls.Add(this.m_trackBar);
		base.Name = "BoolInputControl";
		resources.ApplyResources(this, "$this");
		((System.ComponentModel.ISupportInitialize)this.m_trackBar).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
