using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable;

public class MouseLayoutManipulator : DraggingControlAdapter
{
	[Flags]
	private enum Direction
	{
		None = 0,
		Left = 1,
		Top = 2,
		Right = 4,
		Bottom = 8,
		TopLeft = 3,
		TopRight = 6,
		BottomRight = 0xC,
		BottomLeft = 9
	}

	private const int HandleSize = 4;

	private readonly ITransformAdapter m_transformAdapter;

	private IAutoTranslateAdapter m_autoTranslateAdapter;

	private ILayoutContext m_layoutContext;

	private ISelectionContext m_selectionContext;

	private Direction m_direction = Direction.None;

	private object[] m_draggingItems;

	private Rectangle[] m_originalBounds;

	private Rectangle m_startingBounds;

	private const BoundsSpecified HorizontallySizeable = BoundsSpecified.X | BoundsSpecified.Width;

	private const BoundsSpecified VerticallySizeable = BoundsSpecified.Y | BoundsSpecified.Height;

	public MouseLayoutManipulator(ITransformAdapter transformAdapter)
	{
		m_transformAdapter = transformAdapter;
	}

	protected override void Bind(AdaptableControl control)
	{
		m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();
		control.ContextChanged += control_ContextChanged;
		if (control is D2dAdaptableControl d2dAdaptableControl)
		{
			d2dAdaptableControl.DrawingD2d += d2dControl_DrawingD2d;
		}
		else
		{
			control.Paint += control_Paint;
		}
		base.Bind(control);
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.ContextChanged -= control_ContextChanged;
		if (control is D2dAdaptableControl d2dAdaptableControl)
		{
			d2dAdaptableControl.DrawingD2d -= d2dControl_DrawingD2d;
		}
		else
		{
			control.Paint -= control_Paint;
		}
		base.Unbind(control);
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanged -= selectionContext_SelectionChanged;
		}
		m_layoutContext = base.AdaptedControl.ContextAs<ILayoutContext>();
		m_selectionContext = base.AdaptedControl.ContextAs<ISelectionContext>();
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanged += selectionContext_SelectionChanged;
		}
	}

	private void selectionContext_SelectionChanged(object sender, EventArgs e)
	{
		base.AdaptedControl.Invalidate();
	}

	private void control_Paint(object sender, PaintEventArgs e)
	{
		if (Dragging() || (DragPossible() && !base.AdaptedControl.Capture))
		{
			Rectangle itemBounds = GetItemBounds();
			if (!itemBounds.IsEmpty)
			{
				Matrix transform = m_transformAdapter.Transform;
				itemBounds = GdiUtil.Transform(transform, itemBounds);
				itemBounds.Inflate(-4, -4);
				e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(itemBounds, Direction.TopLeft));
				e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(itemBounds, Direction.Top));
				e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(itemBounds, Direction.TopRight));
				e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(itemBounds, Direction.Right));
				e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(itemBounds, Direction.BottomRight));
				e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(itemBounds, Direction.Bottom));
				e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(itemBounds, Direction.BottomLeft));
				e.Graphics.FillRectangle(Brushes.Gray, GetHandleRect(itemBounds, Direction.Left));
			}
		}
	}

	private void d2dControl_DrawingD2d(object sender, EventArgs e)
	{
		if (Dragging() || (DragPossible() && !base.AdaptedControl.Capture))
		{
			D2dAdaptableControl d2dAdaptableControl = base.AdaptedControl as D2dAdaptableControl;
			D2dGraphics d2dGraphics = d2dAdaptableControl.D2dGraphics;
			Rectangle itemBounds = GetItemBounds();
			if (!itemBounds.IsEmpty)
			{
				Matrix transform = m_transformAdapter.Transform;
				itemBounds = GdiUtil.Transform(transform, itemBounds);
				itemBounds.Inflate(-4, -4);
				d2dGraphics.Transform = Matrix3x2F.Identity;
				d2dGraphics.FillRectangle(GetHandleRect(itemBounds, Direction.TopLeft), Color.Gray);
				d2dGraphics.FillRectangle(GetHandleRect(itemBounds, Direction.Top), Color.Gray);
				d2dGraphics.FillRectangle(GetHandleRect(itemBounds, Direction.TopRight), Color.Gray);
				d2dGraphics.FillRectangle(GetHandleRect(itemBounds, Direction.Right), Color.Gray);
				d2dGraphics.FillRectangle(GetHandleRect(itemBounds, Direction.BottomRight), Color.Gray);
				d2dGraphics.FillRectangle(GetHandleRect(itemBounds, Direction.Bottom), Color.Gray);
				d2dGraphics.FillRectangle(GetHandleRect(itemBounds, Direction.BottomLeft), Color.Gray);
				d2dGraphics.FillRectangle(GetHandleRect(itemBounds, Direction.Left), Color.Gray);
				d2dGraphics.Transform = transform;
			}
		}
	}

	protected override void OnMouseDown(object sender, MouseEventArgs e)
	{
		base.OnMouseDown(sender, e);
		if (!DragPossible() || e.Button != MouseButtons.Left || (Control.ModifierKeys & Keys.Alt) != Keys.None || base.AdaptedControl.Capture)
		{
			return;
		}
		m_direction = GetHitDirection(base.FirstPoint);
		if (m_direction != Direction.None)
		{
			SetCursor(m_direction);
			m_startingBounds = GetItemBounds();
			m_draggingItems = GetItems();
			m_originalBounds = new Rectangle[m_draggingItems.Length];
			for (int i = 0; i < m_draggingItems.Length; i++)
			{
				m_layoutContext.GetBounds(m_draggingItems[i], out m_originalBounds[i]);
			}
			if (m_autoTranslateAdapter != null)
			{
				m_autoTranslateAdapter.Enabled = true;
			}
			base.AdaptedControl.Capture = true;
		}
	}

	protected override void OnMouseMove(object sender, MouseEventArgs e)
	{
		base.OnMouseMove(sender, e);
		if (DragPossible() && e.Button == MouseButtons.None && base.AdaptedControl.Focused && base.AdaptedControl.Cursor == Cursors.Default)
		{
			Direction hitDirection = GetHitDirection(new Point(e.X, e.Y));
			SetCursor(hitDirection);
		}
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		if (Dragging())
		{
			UpdateBounds();
			D2dAdaptableControl d2dAdaptableControl = base.AdaptedControl as D2dAdaptableControl;
			d2dAdaptableControl.DrawD2d();
		}
	}

	private void UpdateBounds()
	{
		Matrix transform = GetTransform();
		for (int i = 0; i < m_draggingItems.Length; i++)
		{
			Rectangle bounds = GdiUtil.Transform(transform, m_originalBounds[i]);
			m_layoutContext.SetBounds(m_draggingItems[i], bounds, BoundsSpecified.All);
		}
	}

	protected override void OnMouseUp(object sender, MouseEventArgs e)
	{
		base.OnMouseUp(sender, e);
		if (e.Button == MouseButtons.Left)
		{
			if (Dragging())
			{
				for (int i = 0; i < m_draggingItems.Length; i++)
				{
					m_layoutContext.SetBounds(m_draggingItems[i], m_originalBounds[i], BoundsSpecified.All);
				}
				ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
				context.DoTransaction(UpdateBounds, "Resize States".Localize());
				base.AdaptedControl.Invalidate();
			}
			if (m_autoTranslateAdapter != null)
			{
				m_autoTranslateAdapter.Enabled = false;
			}
		}
		m_draggingItems = null;
		m_direction = Direction.None;
	}

	private void SetCursor(Direction direction)
	{
		switch (direction)
		{
		case Direction.Left:
		case Direction.Right:
			base.AdaptedControl.Cursor = Cursors.SizeWE;
			break;
		case Direction.Top:
		case Direction.Bottom:
			base.AdaptedControl.Cursor = Cursors.SizeNS;
			break;
		case Direction.TopLeft:
		case Direction.BottomRight:
			base.AdaptedControl.Cursor = Cursors.SizeNWSE;
			break;
		case Direction.TopRight:
		case Direction.BottomLeft:
			base.AdaptedControl.Cursor = Cursors.SizeNESW;
			break;
		case Direction.Left | Direction.Right:
		case Direction.TopLeft | Direction.Right:
		case Direction.Top | Direction.Bottom:
		case Direction.TopLeft | Direction.Bottom:
			break;
		}
	}

	private Matrix GetTransform()
	{
		Matrix transform = m_transformAdapter.Transform;
		PointF pointF = GdiUtil.InverseTransformVector(transform, base.Delta);
		if (m_transformAdapter.UniformScale)
		{
			PointF pointF2 = default(PointF);
			switch (m_direction)
			{
			case Direction.Left:
				pointF2 = new PointF(1f, 0f);
				break;
			case Direction.Right:
				pointF2 = new PointF(-1f, 0f);
				break;
			case Direction.Top:
				pointF2 = new PointF(0f, 1f);
				break;
			case Direction.Bottom:
				pointF2 = new PointF(0f, -1f);
				break;
			case Direction.TopLeft:
				pointF2 = new PointF(0.707f, 0.707f);
				break;
			case Direction.BottomRight:
				pointF2 = new PointF(-0.707f, -0.707f);
				break;
			case Direction.TopRight:
				pointF2 = new PointF(-0.707f, 0.707f);
				break;
			case Direction.BottomLeft:
				pointF2 = new PointF(0.707f, -0.707f);
				break;
			}
			float num = pointF.X * pointF2.X + pointF.Y * pointF2.Y;
			pointF = new PointF(pointF2.X * num, pointF2.Y * num);
		}
		RectangleF rectangleF = m_startingBounds;
		switch (m_direction)
		{
		case Direction.Left:
			rectangleF.X += pointF.X;
			rectangleF.Width -= pointF.X;
			if (m_transformAdapter.UniformScale)
			{
				rectangleF.Y += pointF.X * 0.5f;
				rectangleF.Height -= pointF.X;
			}
			break;
		case Direction.Right:
			rectangleF.Width += pointF.X;
			if (m_transformAdapter.UniformScale)
			{
				rectangleF.Y -= pointF.X * 0.5f;
				rectangleF.Height += pointF.X;
			}
			break;
		case Direction.Top:
			rectangleF.Y += pointF.Y;
			rectangleF.Height -= pointF.Y;
			if (m_transformAdapter.UniformScale)
			{
				rectangleF.X += pointF.Y * 0.5f;
				rectangleF.Width -= pointF.Y;
			}
			break;
		case Direction.Bottom:
			rectangleF.Height += pointF.Y;
			if (m_transformAdapter.UniformScale)
			{
				rectangleF.X -= pointF.Y * 0.5f;
				rectangleF.Width += pointF.Y;
			}
			break;
		case Direction.TopLeft:
			rectangleF.X += pointF.X;
			rectangleF.Width -= pointF.X;
			rectangleF.Y += pointF.Y;
			rectangleF.Height -= pointF.Y;
			break;
		case Direction.BottomRight:
			rectangleF.Width += pointF.X;
			rectangleF.Height += pointF.Y;
			break;
		case Direction.TopRight:
			rectangleF.Width += pointF.X;
			rectangleF.Y += pointF.Y;
			rectangleF.Height -= pointF.Y;
			break;
		case Direction.BottomLeft:
			rectangleF.X += pointF.X;
			rectangleF.Width -= pointF.X;
			rectangleF.Height += pointF.Y;
			break;
		}
		rectangleF.X = Math.Min(rectangleF.Left, m_startingBounds.Right - 1);
		rectangleF.Y = Math.Min(rectangleF.Top, m_startingBounds.Bottom - 1);
		rectangleF.Width = Math.Max(rectangleF.Width, 1f);
		rectangleF.Height = Math.Max(rectangleF.Height, 1f);
		return new Matrix(m_startingBounds, new PointF[3]
		{
			rectangleF.Location,
			new PointF(rectangleF.Right, rectangleF.Top),
			new PointF(rectangleF.Left, rectangleF.Bottom)
		});
	}

	private Direction GetHitDirection(Point point)
	{
		Rectangle itemBounds = GetItemBounds();
		if (!itemBounds.IsEmpty)
		{
			Matrix transform = m_transformAdapter.Transform;
			itemBounds = GdiUtil.Transform(transform, itemBounds);
			itemBounds.Inflate(-4, -4);
			if (GetHandleRect(itemBounds, Direction.TopLeft).Contains(point))
			{
				return Direction.TopLeft;
			}
			if (GetHandleRect(itemBounds, Direction.Top).Contains(point))
			{
				return Direction.Top;
			}
			if (GetHandleRect(itemBounds, Direction.TopRight).Contains(point))
			{
				return Direction.TopRight;
			}
			if (GetHandleRect(itemBounds, Direction.Right).Contains(point))
			{
				return Direction.Right;
			}
			if (GetHandleRect(itemBounds, Direction.BottomRight).Contains(point))
			{
				return Direction.BottomRight;
			}
			if (GetHandleRect(itemBounds, Direction.Bottom).Contains(point))
			{
				return Direction.Bottom;
			}
			if (GetHandleRect(itemBounds, Direction.BottomLeft).Contains(point))
			{
				return Direction.BottomLeft;
			}
			if (GetHandleRect(itemBounds, Direction.Left).Contains(point))
			{
				return Direction.Left;
			}
		}
		return Direction.None;
	}

	private Rectangle GetItemBounds()
	{
		IEnumerable<object> items = (Dragging() ? m_draggingItems : GetItems());
		LayoutContexts.GetBounds(m_layoutContext, items, out var bounds);
		return bounds;
	}

	private bool Dragging()
	{
		return m_draggingItems != null;
	}

	private bool DragPossible()
	{
		return CanSetBounds() != BoundsSpecified.None;
	}

	private BoundsSpecified CanSetBounds()
	{
		BoundsSpecified boundsSpecified = BoundsSpecified.None;
		if (m_selectionContext != null && m_layoutContext != null)
		{
			boundsSpecified = BoundsSpecified.All;
			foreach (object item in m_selectionContext.Selection)
			{
				BoundsSpecified boundsSpecified2 = m_layoutContext.CanSetBounds(item);
				if (CanSetBounds(boundsSpecified2))
				{
					boundsSpecified &= boundsSpecified2;
				}
			}
		}
		return boundsSpecified;
	}

	private bool CanSetBounds(BoundsSpecified specified)
	{
		return (specified & (BoundsSpecified.X | BoundsSpecified.Width)) == (BoundsSpecified.X | BoundsSpecified.Width) || (specified & (BoundsSpecified.Y | BoundsSpecified.Height)) == (BoundsSpecified.Y | BoundsSpecified.Height);
	}

	private object[] GetItems()
	{
		List<object> list = new List<object>();
		if (m_selectionContext != null && m_layoutContext != null)
		{
			foreach (object item in m_selectionContext.Selection)
			{
				BoundsSpecified specified = m_layoutContext.CanSetBounds(item);
				if (CanSetBounds(specified))
				{
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}

	private Rectangle GetHandleRect(Rectangle bounds, Direction direction)
	{
		Rectangle result = new Rectangle(bounds.X + bounds.Width / 2 - 4, bounds.Y + bounds.Height / 2 - 4, 8, 8);
		if ((direction & Direction.Left) != Direction.None)
		{
			result.X = bounds.Left - 4;
		}
		else if ((direction & Direction.Right) != Direction.None)
		{
			result.X = bounds.Right - 4;
		}
		if ((direction & Direction.Top) != Direction.None)
		{
			result.Y = bounds.Top - 4;
		}
		else if ((direction & Direction.Bottom) != Direction.None)
		{
			result.Y = bounds.Bottom - 4;
		}
		return result;
	}
}
