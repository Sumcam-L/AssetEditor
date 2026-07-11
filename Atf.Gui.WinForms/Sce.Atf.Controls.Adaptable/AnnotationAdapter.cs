using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable;

public class AnnotationAdapter : DraggingControlAdapter, IPickingAdapter, IPrintingAdapter, IItemDragAdapter, IDisposable
{
	public class AnnotationHitEventArgs : DiagramHitRecord
	{
		public IAnnotation Annotation => base.Item as IAnnotation;

		public DiagramLabel Label => base.Part as DiagramLabel;

		public AnnotationHitEventArgs()
		{
		}

		public AnnotationHitEventArgs(IAnnotation annotation, DiagramLabel label)
		{
			base.Item = annotation;
			base.Part = label;
		}
	}

	private DiagramTheme m_theme;

	private ITransformAdapter m_transformAdapter;

	private IAutoTranslateAdapter m_autoTranslateAdapter;

	private IAnnotatedDiagram m_annotatedDiagram;

	private IObservableContext m_observableContext;

	private ISelectionContext m_selectionContext;

	private ILayoutContext m_layoutContext;

	private AnnotationHitEventArgs m_mousePick;

	private IAnnotation[] m_draggingAnnotations;

	private HashSet<IAnnotation> m_annotationSet;

	private Point[] m_newPositions;

	private Point[] m_oldPositions;

	private bool m_initiated;

	public AnnotationAdapter(DiagramTheme theme)
	{
		m_theme = theme;
		m_theme.Redraw += theme_Redraw;
	}

	public virtual void Dispose()
	{
		if (m_theme != null)
		{
			m_theme.Redraw -= theme_Redraw;
			m_theme = null;
		}
	}

	private void theme_Redraw(object sender, EventArgs e)
	{
		base.AdaptedControl.Invalidate();
	}

	public AnnotationHitEventArgs Pick(Point p)
	{
		if (m_annotatedDiagram != null)
		{
			if (m_transformAdapter != null)
			{
				p = GdiUtil.InverseTransform(m_transformAdapter.Transform, p);
			}
			foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
			{
				Rectangle bounds = GetBounds(annotation);
				int pickTolerance = m_theme.PickTolerance;
				bounds.Inflate(pickTolerance, pickTolerance);
				if (bounds.Contains(p))
				{
					bounds.Inflate(pickTolerance * -2, pickTolerance * -2);
					DiagramLabel label = null;
					if (bounds.Contains(p))
					{
						label = new DiagramLabel(bounds, TextFormatFlags.Default);
					}
					return new AnnotationHitEventArgs(annotation, label);
				}
			}
		}
		return new AnnotationHitEventArgs();
	}

	DiagramHitRecord IPickingAdapter.Pick(Point p)
	{
		return Pick(p);
	}

	IEnumerable<object> IPickingAdapter.Pick(Region region)
	{
		if (m_annotatedDiagram == null)
		{
			return EmptyEnumerable<object>.Instance;
		}
		List<object> list = new List<object>();
		RectangleF r;
		using (Graphics g = base.AdaptedControl.CreateGraphics())
		{
			r = region.GetBounds(g);
		}
		if (m_transformAdapter != null)
		{
			r = GdiUtil.InverseTransform(m_transformAdapter.Transform, r);
		}
		foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
		{
			Rectangle bounds = GetBounds(annotation);
			if (r.IntersectsWith(bounds))
			{
				list.Add(annotation);
			}
		}
		return list;
	}

	Rectangle IPickingAdapter.GetBounds(IEnumerable<object> items)
	{
		Rectangle rectangle = default(Rectangle);
		foreach (IAnnotation item in items.AsIEnumerable<IAnnotation>())
		{
			Rectangle bounds = GetBounds(item);
			rectangle = (rectangle.IsEmpty ? bounds : Rectangle.Union(rectangle, bounds));
		}
		if (!rectangle.IsEmpty && m_transformAdapter != null)
		{
			rectangle = m_transformAdapter.TransformToClient(rectangle);
		}
		return rectangle;
	}

	void IPrintingAdapter.Print(PrintDocument printDocument, Graphics g)
	{
		PrintRange printRange = printDocument.PrinterSettings.PrintRange;
		if (printRange == PrintRange.Selection)
		{
			PrintSelection(g);
		}
		else
		{
			PrintAll(g);
		}
	}

	private void PrintSelection(Graphics g)
	{
		if (m_selectionContext == null)
		{
			return;
		}
		foreach (IAnnotation item in m_selectionContext.GetSelection<IAnnotation>())
		{
			DrawAnnotation(item, DiagramDrawingStyle.Normal, g);
		}
	}

	private void PrintAll(Graphics g)
	{
		foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
		{
			DrawAnnotation(annotation, DiagramDrawingStyle.Normal, g);
		}
	}

	void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
	{
		m_annotationSet = new HashSet<IAnnotation>();
		List<IAnnotation> list = new List<IAnnotation>();
		foreach (IAnnotation item in m_selectionContext.GetSelection<IAnnotation>())
		{
			list.Add(item);
			m_annotationSet.Add(item);
		}
		m_draggingAnnotations = list.ToArray();
		m_newPositions = new Point[m_draggingAnnotations.Length];
		m_oldPositions = new Point[m_draggingAnnotations.Length];
		for (int i = 0; i < m_draggingAnnotations.Length; i++)
		{
			Point location = m_draggingAnnotations[i].Bounds.Location;
			m_newPositions[i] = location;
			m_oldPositions[i] = location;
		}
	}

	void IItemDragAdapter.EndingDrag()
	{
	}

	void IItemDragAdapter.EndDrag()
	{
		int num = 0;
		IAnnotation[] draggingAnnotations = m_draggingAnnotations;
		foreach (IAnnotation annotation in draggingAnnotations)
		{
			MoveAnnotation(annotation, m_newPositions[num]);
			num++;
		}
		m_draggingAnnotations = null;
		m_annotationSet = null;
		m_newPositions = null;
		m_oldPositions = null;
		base.AdaptedControl.Invalidate();
	}

	protected override void Bind(AdaptableControl control)
	{
		m_transformAdapter = control.As<ITransformAdapter>();
		m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();
		control.ContextChanged += control_ContextChanged;
		control.Paint += control_Paint;
		base.Bind(control);
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.ContextChanged -= control_ContextChanged;
		control.Paint -= control_Paint;
		base.Unbind(control);
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		IAnnotatedDiagram annotatedDiagram = base.AdaptedControl.ContextAs<IAnnotatedDiagram>();
		if (m_annotatedDiagram == annotatedDiagram)
		{
			return;
		}
		if (m_annotatedDiagram != null)
		{
			if (m_observableContext != null)
			{
				m_observableContext.ItemInserted -= context_ObjectInserted;
				m_observableContext.ItemRemoved -= context_ObjectRemoved;
				m_observableContext.ItemChanged -= context_ObjectChanged;
				m_observableContext.Reloaded -= context_Reloaded;
				m_observableContext = null;
			}
			if (m_selectionContext != null)
			{
				m_selectionContext.SelectionChanged -= selection_Changed;
				m_selectionContext = null;
			}
		}
		m_annotatedDiagram = annotatedDiagram;
		if (m_annotatedDiagram != null)
		{
			m_observableContext = base.AdaptedControl.ContextAs<IObservableContext>();
			if (m_observableContext != null)
			{
				m_observableContext.ItemInserted += context_ObjectInserted;
				m_observableContext.ItemRemoved += context_ObjectRemoved;
				m_observableContext.ItemChanged += context_ObjectChanged;
				m_observableContext.Reloaded += context_Reloaded;
			}
			m_selectionContext = base.AdaptedControl.ContextAs<ISelectionContext>();
			if (m_selectionContext != null)
			{
				m_selectionContext.SelectionChanged += selection_Changed;
			}
			m_layoutContext = base.AdaptedControl.ContextAs<ILayoutContext>();
		}
	}

	private void control_Paint(object sender, PaintEventArgs e)
	{
		if (m_annotatedDiagram == null)
		{
			return;
		}
		Graphics graphics = e.Graphics;
		Matrix transform = null;
		PointF v = base.Delta;
		if (m_transformAdapter != null)
		{
			transform = e.Graphics.Transform;
			float[] elements = m_transformAdapter.Transform.Elements;
			float num = Math.Min(elements[0], elements[3]);
			Matrix matrix = (graphics.Transform = new Matrix(num, elements[1], elements[2], num, elements[4], elements[5]));
			v = GdiUtil.InverseTransformVector(matrix, v);
		}
		foreach (IAnnotation annotation2 in m_annotatedDiagram.Annotations)
		{
			DiagramDrawingStyle style = DiagramDrawingStyle.Normal;
			if (m_annotationSet != null && m_annotationSet.Contains(annotation2))
			{
				style = DiagramDrawingStyle.Ghosted;
			}
			else if (m_selectionContext.SelectionContains(annotation2))
			{
				style = DiagramDrawingStyle.Selected;
				if (m_selectionContext.GetLastSelected<IAnnotation>() == annotation2)
				{
					style = DiagramDrawingStyle.LastSelected;
				}
			}
			DrawAnnotation(annotation2, style, graphics);
		}
		if (m_draggingAnnotations != null)
		{
			for (int i = 0; i < m_draggingAnnotations.Length; i++)
			{
				IAnnotation annotation = m_draggingAnnotations[i];
				Rectangle bounds = GetBounds(annotation);
				Rectangle bounds2 = bounds;
				bounds2.X += (int)v.X;
				bounds2.Y += (int)v.Y;
				m_newPositions[i] = bounds2.Location;
				m_layoutContext.SetBounds(annotation, bounds2, BoundsSpecified.Location);
				DrawAnnotation(annotation, DiagramDrawingStyle.Normal, graphics);
				m_layoutContext.SetBounds(annotation, bounds, BoundsSpecified.Location);
			}
		}
		if (m_transformAdapter != null)
		{
			graphics.Transform = transform;
		}
	}

	protected override void OnMouseMove(object sender, MouseEventArgs e)
	{
		base.OnMouseMove(sender, e);
		if (e.Button == MouseButtons.None)
		{
			AnnotationHitEventArgs e2 = Pick(base.CurrentPoint);
			if (e2.Annotation != null && e2.Label == null && base.AdaptedControl.Cursor == Cursors.Default)
			{
				base.AdaptedControl.Cursor = Cursors.SizeAll;
			}
		}
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		if (m_layoutContext != null && m_draggingAnnotations == null && e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Alt) == 0 && !base.AdaptedControl.Capture)
		{
			m_mousePick = Pick(base.FirstPoint);
			if (m_mousePick.Item != null)
			{
				m_initiated = true;
				foreach (IItemDragAdapter item in base.AdaptedControl.AsAll<IItemDragAdapter>())
				{
					item.BeginDrag(this);
				}
				base.AdaptedControl.Capture = true;
				if (m_autoTranslateAdapter != null)
				{
					m_autoTranslateAdapter.Enabled = true;
				}
			}
		}
		if (m_draggingAnnotations != null)
		{
			base.AdaptedControl.Invalidate();
		}
	}

	protected override void OnMouseUp(object sender, MouseEventArgs e)
	{
		base.OnMouseUp(sender, e);
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		if (m_draggingAnnotations != null && m_initiated)
		{
			ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
			context.DoTransaction(delegate
			{
				foreach (IItemDragAdapter item in base.AdaptedControl.AsAll<IItemDragAdapter>())
				{
					item.EndDrag();
				}
			}, "Drag Items".Localize());
			m_initiated = false;
		}
		if (m_autoTranslateAdapter != null)
		{
			m_autoTranslateAdapter.Enabled = false;
		}
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void context_ObjectInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		Invalidate();
	}

	private void context_ObjectRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		Invalidate();
	}

	private void context_ObjectChanged(object sender, ItemChangedEventArgs<object> e)
	{
		Invalidate();
	}

	private void context_Reloaded(object sender, EventArgs e)
	{
		Invalidate();
	}

	private void Invalidate()
	{
		base.AdaptedControl.Invalidate();
	}

	private void DrawAnnotation(IAnnotation annotation, DiagramDrawingStyle style, Graphics g)
	{
		Rectangle bounds = annotation.Bounds;
		if (bounds.Size.IsEmpty)
		{
			SizeF sizeF = g.MeasureString(annotation.Text, m_theme.Font);
			Size textSize = (bounds.Size = new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height)));
			annotation.SetTextSize(textSize);
		}
		g.FillRectangle(SystemBrushes.Info, bounds);
		if ((style & DiagramDrawingStyle.Ghosted) == 0)
		{
			g.DrawString(annotation.Text, m_theme.Font, SystemBrushes.WindowText, bounds);
			Pen pen = null;
			if ((style & DiagramDrawingStyle.LastSelected) != DiagramDrawingStyle.Normal)
			{
				pen = m_theme.LastHighlightPen;
			}
			else if ((style & DiagramDrawingStyle.Selected) != DiagramDrawingStyle.Normal)
			{
				pen = m_theme.HighlightPen;
			}
			if (pen != null)
			{
				g.DrawRectangle(pen, bounds);
			}
		}
	}

	private void MoveAnnotation(IAnnotation annotation, Point location)
	{
		Rectangle bounds = new Rectangle(location.X, location.Y, 0, 0);
		m_layoutContext.SetBounds(annotation, bounds, BoundsSpecified.Location);
	}

	private Rectangle GetBounds(IAnnotation annotation)
	{
		Rectangle bounds = annotation.Bounds;
		if (bounds.Size.IsEmpty)
		{
			SizeF sizeF = TextRenderer.MeasureText(annotation.Text, m_theme.Font);
			Size size = new Size((int)sizeF.Width, (int)sizeF.Height);
			bounds.Size = size;
		}
		return bounds;
	}
}
