using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Controls;

namespace Firaxis.ATF;

public class KeyFrameControl : UserControl
{
	private enum DragMode
	{
		Idle,
		Pan,
		Select,
		KeyFrame
	}

	private DragMode m_drag;

	private Point m_drag_pt;

	private float m_dragGhostTime;

	private List<IKeyFrame> m_keyFrames = new List<IKeyFrame>();

	private TimeRulerControl m_ruler;

	private int m_selectedIndex = -1;

	private IContainer components;

	public IList<IKeyFrame> KeyFrames
	{
		get
		{
			return m_keyFrames;
		}
		set
		{
			m_keyFrames.Clear();
			m_keyFrames.AddRange(value);
		}
	}

	public TimeRulerControl Ruler
	{
		get
		{
			return m_ruler;
		}
		set
		{
			if (m_ruler != value)
			{
				if (m_ruler != null)
				{
					m_ruler.OriginChanged -= Ruler_OriginChanged;
					m_ruler.ScaleChanged -= Ruler_ScaleChanged;
				}
				m_ruler = value;
				if (m_ruler != null)
				{
					m_ruler.OriginChanged += Ruler_OriginChanged;
					m_ruler.ScaleChanged += Ruler_ScaleChanged;
				}
			}
		}
	}

	public int SelectedIndex
	{
		get
		{
			return m_selectedIndex;
		}
		set
		{
			if (m_selectedIndex != value)
			{
				if (value >= m_keyFrames.Count)
				{
					throw new InvalidOperationException("Selected index out of range");
				}
				if (m_selectedIndex < m_keyFrames.Count)
				{
					InvalidateKeyFrame(m_selectedIndex);
				}
				m_selectedIndex = value;
				InvalidateKeyFrame(m_selectedIndex);
				RaiseSelectionChanged();
			}
		}
	}

	private Color KeyFrameColor { get; set; }

	private int KeyFrameSize { get; set; }

	private Color SelectedKeyFrameColor { get; set; }

	private int SelectedKeyFrameSize { get; set; }

	public event EventHandler<KeyFrameTimeChangeArgs> KeyFrameTimeChanged;

	public event EventHandler<EventArgs> SelectionChanged;

	public KeyFrameControl()
	{
		InitializeComponent();
		KeyFrameSize = 10;
		KeyFrameColor = Color.Green;
		SelectedKeyFrameSize = 14;
		SelectedKeyFrameColor = Color.Blue;
	}

	protected void RaiseKeyFrameTimeChanged(int keyFrame, float time)
	{
		this.KeyFrameTimeChanged?.Invoke(this, new KeyFrameTimeChangeArgs(keyFrame, time));
	}

	protected void RaiseSelectionChanged()
	{
		this.SelectionChanged?.Invoke(this, new EventArgs());
	}

	private void DrawDraggingKeyFrame(Graphics graphics)
	{
		int selectedKeyFrameSize = SelectedKeyFrameSize;
		int num = (int)((double)selectedKeyFrameSize / 2.0 + 0.5);
		using Brush brush = new SolidBrush(Color.Beige);
		int num2 = Ruler.TimeToX(m_dragGhostTime);
		int halfClientHeight = GetHalfClientHeight();
		graphics.FillEllipse(brush, num2 - num, halfClientHeight - num, selectedKeyFrameSize, selectedKeyFrameSize);
	}

	private void DrawKeyFrames(Graphics graphics)
	{
		using Brush brush = new SolidBrush(KeyFrameColor);
		int keyFrameSize = KeyFrameSize;
		int num = (int)((double)keyFrameSize / 2.0 + 0.5);
		foreach (IKeyFrame keyFrame in KeyFrames)
		{
			int num2 = Ruler.TimeToX(keyFrame.Time);
			int halfClientHeight = GetHalfClientHeight();
			graphics.FillEllipse(brush, num2 - num, halfClientHeight - num, keyFrameSize, keyFrameSize);
		}
	}

	private void DrawMajorTicks(Graphics graphics)
	{
		float num = (float)Math.Truncate(Ruler.Origin);
		float num2 = Ruler.Origin + Ruler.TimeSpan;
		float num3 = 0f;
		using Pen pen = new Pen(Ruler.TickColor, 1f);
		for (num3 = num; num3 < num2; num3 += 1f)
		{
			int num4 = Ruler.TimeToX(num3);
			graphics.DrawLine(pen, num4, 0, num4, base.ClientSize.Height);
		}
	}

	private void DrawSelection(Graphics graphics)
	{
		int selectedKeyFrameSize = SelectedKeyFrameSize;
		int num = (int)((double)selectedKeyFrameSize / 2.0 + 0.5);
		using Brush brush = new SolidBrush(SelectedKeyFrameColor);
		int halfClientHeight = GetHalfClientHeight();
		int num2 = Ruler.TimeToX(KeyFrames[m_selectedIndex].Time);
		graphics.FillEllipse(brush, num2 - num, halfClientHeight - num, selectedKeyFrameSize, selectedKeyFrameSize);
	}

	private int FindKeyFrame(float time)
	{
		float num = Math.Abs(Ruler.XToTime(3) - Ruler.Origin);
		for (int i = 0; i < KeyFrames.Count; i++)
		{
			if (Math.Abs(KeyFrames[i].Time - time) < num)
			{
				return i;
			}
		}
		return -1;
	}

	private int GetHalfClientHeight()
	{
		return (int)((double)base.ClientSize.Height / 2.0 + 0.5);
	}

	private int GetKeyFrameHit(Point mousePt)
	{
		for (int i = 0; i < KeyFrames.Count; i++)
		{
			if (GetKeyFrameRect(i).Contains(mousePt))
			{
				return i;
			}
		}
		return -1;
	}

	private Rectangle GetKeyFrameRect(int idx)
	{
		IKeyFrame keyFrame = KeyFrames[idx];
		int num = Ruler.TimeToX(keyFrame.Time);
		int num2 = (int)((double)SelectedKeyFrameSize / 2.0 + 0.5);
		int halfClientHeight = GetHalfClientHeight();
		return new Rectangle(num - num2, halfClientHeight - num2, SelectedKeyFrameSize, SelectedKeyFrameSize);
	}

	private float GetRulerTime(int x_mousePosition)
	{
		float val = Math.Max(0f, Ruler.XToTime(x_mousePosition));
		float val2 = Math.Max(0f, Ruler.MaxTime);
		return Math.Min(val, val2);
	}

	private void InvalidateKeyFrame(int idx)
	{
		if (idx != -1)
		{
			Invalidate(GetKeyFrameRect(idx));
		}
	}

	private void KeyFrameControl_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Middle)
		{
			SetStateToPan(e.X);
		}
		else if (e.Button == MouseButtons.Left)
		{
			int keyFrameHit = GetKeyFrameHit(e.Location);
			if (keyFrameHit != -1)
			{
				SetStateToSelect(e.X);
			}
			else
			{
				SetStateToIdle();
			}
			SelectedIndex = keyFrameHit;
		}
	}

	private void KeyFrameControl_MouseMove(object sender, MouseEventArgs e)
	{
		switch (m_drag)
		{
		case DragMode.Pan:
		{
			float val = Ruler.Origin - (float)(e.X - m_drag_pt.X) / Ruler.MajorScale;
			Ruler.Origin = Math.Min(Math.Max(0f, val), Math.Max(0f, Ruler.MaxTime - Ruler.TimeSpan + 0.5f));
			m_drag_pt.X = e.X;
			break;
		}
		case DragMode.Select:
			if (Math.Abs(m_drag_pt.X - e.X) > 5)
			{
				m_drag = DragMode.KeyFrame;
				m_dragGhostTime = GetRulerTime(e.X);
				base.Capture = true;
			}
			break;
		case DragMode.KeyFrame:
			Cursor = CustomCursors.Get(CustomCursor.HSplit);
			m_dragGhostTime = GetRulerTime(e.X);
			Invalidate();
			break;
		case DragMode.Idle:
			Cursor = ((GetKeyFrameHit(e.Location) != -1) ? CustomCursors.Get(CustomCursor.FingerPoint) : Cursors.Default);
			break;
		}
	}

	private void KeyFrameControl_MouseUp(object sender, MouseEventArgs e)
	{
		bool num = e.Button == MouseButtons.Left;
		bool flag = num || (e.Button == MouseButtons.Middle && m_drag == DragMode.Pan);
		if (num && m_drag == DragMode.KeyFrame)
		{
			RaiseTimeChanged(e.X);
		}
		if (flag)
		{
			SetStateToIdle();
			Cursor = Cursors.Default;
		}
		if (num)
		{
			Invalidate();
		}
	}

	private void KeyFrameControl_MouseWheel(object sender, MouseEventArgs e)
	{
		int num = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
		Ruler.MajorScale += num;
	}

	private void KeyFrameControl_Paint(object sender, PaintEventArgs e)
	{
		if (Ruler != null)
		{
			DrawMajorTicks(e.Graphics);
			if (SelectionIsValid())
			{
				DrawSelection(e.Graphics);
			}
			DrawKeyFrames(e.Graphics);
			if (m_drag == DragMode.KeyFrame)
			{
				DrawDraggingKeyFrame(e.Graphics);
			}
		}
	}

	private void RaiseTimeChanged(int x_mousePosition)
	{
		float rulerTime = GetRulerTime(x_mousePosition);
		RaiseKeyFrameTimeChanged(SelectedIndex, rulerTime);
	}

	private void Ruler_OriginChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void Ruler_ScaleChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	private bool SelectionIsValid()
	{
		if (m_selectedIndex >= 0)
		{
			return m_selectedIndex < KeyFrames.Count;
		}
		return false;
	}

	private void SetStateToIdle()
	{
		m_drag = DragMode.Idle;
		base.Capture = false;
	}

	private void SetStateToPan(int x_mouseLocation)
	{
		m_drag = DragMode.Pan;
		m_drag_pt.X = x_mouseLocation;
		base.Capture = true;
		Cursor = CustomCursors.Get(CustomCursor.HandDrag);
	}

	private void SetStateToSelect(int x_mouseLocation)
	{
		m_drag = DragMode.Select;
		m_drag_pt.X = x_mouseLocation;
		base.Capture = true;
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
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Name = "KeyFrameControl";
		base.Size = new System.Drawing.Size(336, 84);
		base.Paint += new System.Windows.Forms.PaintEventHandler(KeyFrameControl_Paint);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(KeyFrameControl_MouseDown);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(KeyFrameControl_MouseMove);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(KeyFrameControl_MouseUp);
		base.MouseWheel += new System.Windows.Forms.MouseEventHandler(KeyFrameControl_MouseWheel);
		base.ResumeLayout(false);
	}
}
