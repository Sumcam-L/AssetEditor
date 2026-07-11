using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Timelines;

public class ScrubberManipulator : IEvent, ITimelineObject
{
	private class HitRecordObject
	{
		public override string ToString()
		{
			return "drag left or right to reposition Scrubber";
		}
	}

	public readonly TimelineControl Owner;

	private readonly HitRecordObject m_handleHitObject = new HitRecordObject();

	private RectangleF m_handleRect;

	private bool m_isMoving;

	private float m_position;

	private static Color s_color = Color.Black;

	private static readonly Pen s_grayPen = new Pen(Color.FromArgb(116, 114, 106));

	private static readonly Point[] s_arrow = new Point[9];

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

	public ScrubberManipulator(TimelineControl owner)
	{
		Owner = owner;
		Owner.MouseDownPicked += owner_MouseDownPicked;
		Owner.MouseMovePicked += owner_MouseMovePicked;
		Owner.Picking += owner_Picking;
		Owner.MouseMove += MouseMoveHandler;
		Owner.MouseUp += owner_MouseUp;
		Owner.Paint += owner_Paint;
		Owner.BoundingRectUpdating += owner_BoundingRectUpdating;
	}

	protected virtual float ValidatePosition(float position)
	{
		return Owner.ConstrainFrameOffset(position);
	}

	protected virtual void OnMoved(EventArgs e)
	{
		if (ScrubberManipulator.Moved != null)
		{
			ScrubberManipulator.Moved(this, e);
		}
	}

	protected virtual void DrawManipulator(Graphics g, out RectangleF handleRect)
	{
		Matrix transform = Owner.Transform;
		float num = GdiUtil.Transform(transform, Position);
		Rectangle visibleClientRectangle = Owner.VisibleClientRectangle;
		handleRect = new RectangleF(num - 5f, visibleClientRectangle.Top, 10f, 7f);
		using (Pen pen = new Pen(s_color))
		{
			g.DrawLine(pen, num, visibleClientRectangle.Top, num, visibleClientRectangle.Bottom);
		}
		Color color = (m_isMoving ? Color.Tomato : s_color);
		using Brush brush = new SolidBrush(color);
		int num2 = Convert.ToInt32(num);
		int num3 = Convert.ToInt32(visibleClientRectangle.Top + 5);
		s_arrow[0] = new Point(num2 - 4, num3 - 5);
		s_arrow[1] = new Point(num2 - 4, num3);
		s_arrow[2] = new Point(num2 - 5, num3 + 1);
		s_arrow[3] = new Point(num2 - 5, num3 + 2);
		s_arrow[4] = new Point(num2, num3 + 7);
		s_arrow[5] = new Point(num2 + 5, num3 + 2);
		s_arrow[6] = new Point(num2 + 5, num3 + 1);
		s_arrow[7] = new Point(num2 + 4, num3);
		s_arrow[8] = new Point(num2 + 4, num3 - 5);
		g.FillPolygon(brush, s_arrow);
		g.DrawPolygon(s_grayPen, s_arrow);
		string s = Position.ToString(CultureInfo.CurrentCulture);
		g.DrawString(s, Owner.Font, SystemBrushes.WindowText, num2 + 6, visibleClientRectangle.Top);
	}

	private void owner_Paint(object sender, PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Region clip = graphics.Clip;
		Rectangle visibleClientRectangle = Owner.VisibleClientRectangle;
		graphics.Clip = new Region(visibleClientRectangle);
		DrawManipulator(graphics, out m_handleRect);
		graphics.Clip = clip;
	}

	private void owner_BoundingRectUpdating(object sender, TimelineControl.BoundingRectEventArgs e)
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
			Position = GdiUtil.InverseTransform(transform, e.MouseEvent.Location.X);
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
				TimelineControl.SnapOptions snapOptions = new TimelineControl.SnapOptions();
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
