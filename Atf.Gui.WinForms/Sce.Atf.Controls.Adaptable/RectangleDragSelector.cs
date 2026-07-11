using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable;

public class RectangleDragSelector : DraggingControlAdapter, IDragSelector
{
	private Keys m_modifierKeys = Keys.Shift | Keys.Control;

	private IAutoTranslateAdapter m_autoTranslateAdapter;

	private ITransformAdapter m_transformAdapter;

	private Cursor m_saveCursor;

	private Keys m_modifiers;

	private PointF m_firstCanvasPoint;

	private PointF m_currentCanvasPoint;

	private bool m_isMultiSelecting;

	private bool m_controlRepainted;

	public Keys ModifierKeys
	{
		get
		{
			return m_modifierKeys;
		}
		set
		{
			m_modifierKeys = value;
		}
	}

	public event EventHandler<DragSelectionEventArgs> Selected;

	protected virtual void OnSelected(DragSelectionEventArgs e)
	{
	}

	protected override void Bind(AdaptableControl control)
	{
		m_transformAdapter = control.As<ITransformAdapter>();
		m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();
		control.Paint += control_Paint;
		base.Bind(control);
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.Paint -= control_Paint;
		base.Unbind(control);
	}

	private void control_Paint(object sender, PaintEventArgs e)
	{
		m_controlRepainted = true;
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		m_modifiers = Control.ModifierKeys;
		if (e.Button == MouseButtons.Left && (m_modifiers & ~m_modifierKeys) == 0 && !base.AdaptedControl.Capture)
		{
			m_firstCanvasPoint = (m_currentCanvasPoint = ClientToCanvas(base.FirstPoint));
			m_isMultiSelecting = true;
			base.AdaptedControl.Capture = true;
			m_saveCursor = base.AdaptedControl.Cursor;
			base.AdaptedControl.Cursor = Cursors.Cross;
			if (m_autoTranslateAdapter != null)
			{
				m_autoTranslateAdapter.Enabled = true;
			}
		}
		if (m_isMultiSelecting)
		{
			ClearFrame();
			m_currentCanvasPoint = ClientToCanvas(base.CurrentPoint);
			Rectangle rect = MakeSelectionRect(m_currentCanvasPoint, m_firstCanvasPoint);
			DrawReversibleFrame(rect);
		}
	}

	protected override void OnMouseUp(object sender, MouseEventArgs e)
	{
		base.OnMouseUp(sender, e);
		if (m_isMultiSelecting)
		{
			ClearFrame();
			Rectangle bounds = MakeSelectionRect(m_currentCanvasPoint, m_firstCanvasPoint);
			RaiseSelected(bounds, m_modifiers);
			m_isMultiSelecting = false;
			base.AdaptedControl.Cursor = m_saveCursor;
			base.AdaptedControl.Capture = false;
			if (m_autoTranslateAdapter != null)
			{
				m_autoTranslateAdapter.Enabled = false;
			}
		}
	}

	private void RaiseSelected(Rectangle bounds, Keys modifiers)
	{
		DragSelectionEventArgs e = new DragSelectionEventArgs(bounds, modifiers);
		OnSelected(e);
		this.Selected.Raise(this, e);
	}

	private void ClearFrame()
	{
		if (!m_controlRepainted)
		{
			Rectangle rect = MakeSelectionRect(m_currentCanvasPoint, m_firstCanvasPoint);
			DrawReversibleFrame(rect);
		}
		else
		{
			m_controlRepainted = false;
		}
	}

	private void DrawReversibleFrame(Rectangle rect)
	{
		rect.Intersect(base.AdaptedControl.ClientRectangle);
		rect = base.AdaptedControl.RectangleToScreen(rect);
		ControlPaint.DrawReversibleFrame(rect, base.AdaptedControl.BackColor, FrameStyle.Dashed);
	}

	private Rectangle MakeSelectionRect(PointF p1, PointF p2)
	{
		Rectangle rectangle = GdiUtil.MakeRectangle(new Point((int)p1.X, (int)p1.Y), new Point((int)p2.X, (int)p2.Y));
		if (m_transformAdapter != null)
		{
			rectangle = GdiUtil.Transform(m_transformAdapter.Transform, rectangle);
		}
		return rectangle;
	}

	private PointF ClientToCanvas(PointF p)
	{
		if (m_transformAdapter != null)
		{
			p = GdiUtil.InverseTransform(m_transformAdapter.Transform, p);
		}
		return p;
	}
}
