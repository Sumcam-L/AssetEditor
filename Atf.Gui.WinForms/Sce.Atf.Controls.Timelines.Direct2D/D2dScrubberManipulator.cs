using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D;

public class D2dScrubberManipulator : IEvent, ITimelineObject
{
	private class HitRecordObject
	{
		public override string ToString()
		{
			return "drag left or right to reposition Scrubber";
		}
	}

	public readonly D2dTimelineControl Owner;

	private readonly HitRecordObject m_handleHitObject = new HitRecordObject();

	private RectangleF m_handleRect;

	private bool m_isMoving;

	private float m_position;

	private static Color s_color = Color.Black;

	private static readonly PointF[] s_arrow = new PointF[9];

	public float Position
	{
		get
		{
			return m_position;
		}
		set
		{
			value = ValidatePosition(value);
			if (m_position != value)
			{
				m_position = value;
				Owner.Invalidate();
				OnMoved(EventArgs.Empty);
			}
		}
	}

	public bool IsMoving => m_isMoving;

	[DefaultValue(typeof(Color), "Black")]
	public static Color Color
	{
		get
		{
			return s_color;
		}
		set
		{
			s_color = value;
		}
	}

	float IEvent.Start
	{
		get
		{
			return Position;
		}
		set
		{
			Position = value;
		}
	}

	float IEvent.Length
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	Color IEvent.Color
	{
		get
		{
			return Color;
		}
		set
		{
			Color = value;
		}
	}

	string IEvent.Name
	{
		get
		{
			return "Scrubber";
		}
		set
		{
		}
	}

	public static event EventHandler Moved;

	public D2dScrubberManipulator(D2dTimelineControl owner)
	{
		Owner = owner;
		Owner.MouseDownPicked += owner_MouseDownPicked;
		Owner.MouseMovePicked += owner_MouseMovePicked;
		Owner.Picking += owner_Picking;
		Owner.MouseMove += MouseMoveHandler;
		Owner.MouseUp += owner_MouseUp;
		Owner.DrawingD2d += owner_DrawingD2d;
		Owner.BoundingRectUpdating += owner_BoundingRectUpdating;
	}

	protected virtual float ValidatePosition(float position)
	{
		return Owner.ConstrainFrameOffset(position);
	}

	protected virtual void OnMoved(EventArgs e)
	{
		if (D2dScrubberManipulator.Moved != null)
		{
			D2dScrubberManipulator.Moved(this, e);
		}
	}

	protected virtual void DrawManipulator(D2dGraphics g, out RectangleF handleRect)
	{
		Matrix transform = Owner.Transform;
		float num = GdiUtil.Transform(transform, Position);
		Rectangle visibleClientRectangle = Owner.VisibleClientRectangle;
		handleRect = new RectangleF(num - 5f, visibleClientRectangle.Top, 10f, 7f);
		g.DrawLine(num, visibleClientRectangle.Top, num, visibleClientRectangle.Bottom, s_color);
		Color color = (m_isMoving ? Color.Tomato : s_color);
		float num2 = num;
		float num3 = visibleClientRectangle.Top + 5;
		s_arrow[0] = new PointF(num2 - 4f, num3 - 5f);
		s_arrow[1] = new PointF(num2 - 4f, num3);
		s_arrow[2] = new PointF(num2 - 5f, num3 + 1f);
		s_arrow[3] = new PointF(num2 - 5f, num3 + 2f);
		s_arrow[4] = new PointF(num2, num3 + 7f);
		s_arrow[5] = new PointF(num2 + 5f, num3 + 2f);
		s_arrow[6] = new PointF(num2 + 5f, num3 + 1f);
		s_arrow[7] = new PointF(num2 + 4f, num3);
		s_arrow[8] = new PointF(num2 + 4f, num3 - 5f);
		g.DrawLines(s_arrow, color, 2f);
		string text = Position.ToString(CultureInfo.CurrentCulture);
		g.DrawText(text, Owner.Renderer.TextFormat, new PointF(num2 + 6f, visibleClientRectangle.Top), SystemColors.WindowText);
	}

	private void owner_DrawingD2d(object sender, EventArgs e)
	{
		D2dGraphics d2dGraphics = Owner.D2dGraphics;
		Rectangle visibleClientRectangle = Owner.VisibleClientRectangle;
		try
		{
			d2dGraphics.PushAxisAlignedClip(visibleClientRectangle);
			DrawManipulator(d2dGraphics, out m_handleRect);
		}
		finally
		{
			d2dGraphics.PopAxisAlignedClip();
		}
	}

	private void owner_BoundingRectUpdating(object sender, D2dTimelineControl.BoundingRectEventArgs e)
	{
		e.NewClientRect = m_handleRect;
	}

	private void owner_MouseDownPicked(object sender, HitEventArgs e)
	{
		if (e.MouseEvent.Button == MouseButtons.Left && e.HitRecord.HitObject == m_handleHitObject)
		{
			m_isMoving = true;
			Owner.Cursor = Cursors.SizeWE;
			e.Handled = true;
			Owner.Invalidate();
		}
		else if (e.HitRecord.Type == HitType.TimeScale)
		{
			m_isMoving = false;
			Matrix transform = Owner.Transform;
			Position = GdiUtil.InverseTransform(transform, ((PointF)e.MouseEvent.Location).X);
		}
	}

	private void owner_MouseMovePicked(object sender, HitEventArgs e)
	{
		if (e.MouseEvent.Button == MouseButtons.None && e.HitRecord.HitObject == m_handleHitObject)
		{
			Owner.Cursor = Cursors.SizeWE;
			e.Handled = true;
		}
	}

	private void owner_Picking(object sender, HitEventArgs e)
	{
		Rectangle visibleClientRectangle = Owner.VisibleClientRectangle;
		if (m_handleRect.IntersectsWith(visibleClientRectangle) && e.PickRectangle.IntersectsWith(m_handleRect))
		{
			e.HitRecord = new HitRecord(HitType.Custom, m_handleHitObject);
		}
	}

	protected virtual void MouseMoveHandler(object sender, MouseEventArgs e)
	{
		if (IsMoving)
		{
			Matrix transform = Owner.Transform;
			float num = GdiUtil.InverseTransform(transform, e.Location.X);
			if (Control.ModifierKeys == Keys.Shift)
			{
				float[] movingPoints = new float[1] { num };
				D2dTimelineControl.SnapOptions snapOptions = new D2dTimelineControl.SnapOptions();
				snapOptions.IncludeScrubber = false;
				snapOptions.CheckModifierKeys = false;
				float num2 = Owner.GetSnapOffset(movingPoints, snapOptions);
				num += num2;
			}
			Position = num;
		}
	}

	private void owner_MouseUp(object sender, MouseEventArgs e)
	{
		if (m_isMoving)
		{
			m_isMoving = false;
			Owner.Invalidate();
		}
	}
}
