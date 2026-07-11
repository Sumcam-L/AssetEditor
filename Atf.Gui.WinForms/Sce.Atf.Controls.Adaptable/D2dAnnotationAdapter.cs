using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Direct2D;
using Sce.Atf.DirectWrite;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable;

public class D2dAnnotationAdapter : DraggingControlAdapter, IPickingAdapter2, IItemDragAdapter, IDisposable
{
	public class AnnotationHitEventArgs : DiagramHitRecord
	{
		public IAnnotation Annotation => base.Item as IAnnotation;

		public DiagramLabel Label => base.Part as DiagramLabel;

		public DiagramBorder Border => base.Part as DiagramBorder;

		public PointF Position { get; set; }

		public AnnotationHitEventArgs()
		{
		}

		public AnnotationHitEventArgs(IAnnotation annotation, DiagramBorder border)
		{
			base.Item = annotation;
			base.Part = border;
		}

		public AnnotationHitEventArgs(IAnnotation annotation, DiagramScrollBar scrollBar)
		{
			base.Item = annotation;
			base.Part = scrollBar;
		}

		public AnnotationHitEventArgs(IAnnotation annotation, DiagramLabel label)
		{
			base.Item = annotation;
			base.Part = label;
		}

		public AnnotationHitEventArgs(IAnnotation annotation, DiagramTitleBar titleBar)
		{
			base.Item = annotation;
			base.Part = titleBar;
		}
	}

	private static readonly Padding Margin = new Padding(5, 12, 3, 3);

	private static readonly string EditAnnotation = "Edit Annotation".Localize("the name of a command");

	private const int ScrollBarWidth = 5;

	private const int ScrollBarMargin = 2;

	private const int MinimumWidth = 50;

	private int MinimumHeight = 13;

	private const int CaretWidth = 2;

	private int CaretHeight = 12;

	private readonly Dictionary<IAnnotation, TextEditor> m_annotationEditors = new Dictionary<IAnnotation, TextEditor>();

	private D2dDiagramTheme m_theme;

	private ITransformAdapter m_transformAdapter;

	private IAutoTranslateAdapter m_autoTranslateAdapter;

	private IAnnotatedDiagram m_annotatedDiagram;

	private IColoringContext m_coloringContext;

	private IObservableContext m_observableContext;

	private ISelectionContext m_selectionContext;

	private ILayoutContext m_layoutContext;

	private AnnotationHitEventArgs m_mousePick;

	private IAnnotation[] m_draggingAnnotations;

	private Point[] m_newPositions;

	private Point[] m_oldPositions;

	private Rectangle m_startBounds;

	private float m_scaleX = 1f;

	private bool m_resizing;

	private bool m_scrolling;

	private bool m_moving;

	private bool m_selecting;

	private int m_startTopLine;

	private bool m_caretCreated;

	private bool m_editingText;

	private bool m_rmbPressed;

	public D2dAnnotationAdapter(D2dDiagramTheme theme)
	{
		m_theme = theme;
		m_theme.Redraw += theme_Redraw;
	}

	public virtual void Dispose()
	{
		m_theme.Redraw -= theme_Redraw;
	}

	private void theme_Redraw(object sender, EventArgs e)
	{
		if (base.AdaptedControl != null)
		{
			base.AdaptedControl.Invalidate();
		}
	}

	public AnnotationHitEventArgs Pick(Point p)
	{
		if (m_annotatedDiagram != null)
		{
			if (m_transformAdapter != null)
			{
				p = GdiUtil.InverseTransform(m_transformAdapter.Transform, p);
			}
			foreach (IAnnotation item in m_annotatedDiagram.Annotations.Reverse())
			{
				Rectangle bounds = GetBounds(item);
				if (bounds.IsEmpty)
				{
					continue;
				}
				Rectangle rectangle = bounds;
				int pickTolerance = m_theme.PickTolerance;
				rectangle.Inflate(pickTolerance, pickTolerance);
				if (!rectangle.Contains(p) || !m_annotationEditors.ContainsKey(item))
				{
					continue;
				}
				RectangleF rectangleF = new RectangleF(bounds.Left + 2 * pickTolerance, bounds.Y - pickTolerance, bounds.Width - 4 * pickTolerance, Margin.Top + pickTolerance);
				if (rectangleF.Contains(p))
				{
					return new AnnotationHitEventArgs(item, new DiagramTitleBar(item));
				}
				TextEditor textEditor = m_annotationEditors[item];
				if (textEditor.VerticalScrollBarVisibe)
				{
					RectangleF rectangleF2 = new RectangleF(bounds.Right - Margin.Right - 5 - 4, bounds.Y, 9f, bounds.Height);
					if (rectangleF2.Contains(p))
					{
						return new AnnotationHitEventArgs(item, new DiagramScrollBar(item, Orientation.Vertical));
					}
				}
				if (new RectangleF(bounds.Right - 2 * pickTolerance, bounds.Bottom - 2 * pickTolerance, 4 * pickTolerance, 4 * pickTolerance).Contains(p))
				{
					DiagramBorder border = new DiagramBorder(item)
					{
						Border = DiagramBorder.BorderType.LowerRightCorner
					};
					return new AnnotationHitEventArgs(item, border);
				}
				if (new RectangleF(bounds.Left - 2 * pickTolerance, bounds.Top - 2 * m_theme.PickTolerance, 4 * pickTolerance, 4 * pickTolerance).Contains(p))
				{
					DiagramBorder border2 = new DiagramBorder(item)
					{
						Border = DiagramBorder.BorderType.UpperLeftCorner
					};
					return new AnnotationHitEventArgs(item, border2);
				}
				if (new RectangleF(bounds.Right - 2 * m_theme.PickTolerance, bounds.Top - 2 * m_theme.PickTolerance, 4 * pickTolerance, 4 * pickTolerance).Contains(p))
				{
					DiagramBorder border3 = new DiagramBorder(item)
					{
						Border = DiagramBorder.BorderType.UpperRightCorner
					};
					return new AnnotationHitEventArgs(item, border3);
				}
				if (new RectangleF(bounds.Left - 2 * m_theme.PickTolerance, bounds.Bottom - 2 * m_theme.PickTolerance, 4 * pickTolerance, 4 * pickTolerance).Contains(p))
				{
					DiagramBorder border4 = new DiagramBorder(item)
					{
						Border = DiagramBorder.BorderType.LowerLeftCorner
					};
					return new AnnotationHitEventArgs(item, border4);
				}
				RectangleF rectangleF3 = new RectangleF(bounds.Left - m_theme.PickTolerance, bounds.Y, 2 * m_theme.PickTolerance, bounds.Height);
				if (rectangleF3.Contains(p))
				{
					DiagramBorder border5 = new DiagramBorder(item)
					{
						Border = DiagramBorder.BorderType.Left
					};
					return new AnnotationHitEventArgs(item, border5);
				}
				rectangleF3.Offset(bounds.Width, 0f);
				if (rectangleF3.Contains(p))
				{
					DiagramBorder border6 = new DiagramBorder(item)
					{
						Border = DiagramBorder.BorderType.Right
					};
					return new AnnotationHitEventArgs(item, border6);
				}
				rectangleF3 = new RectangleF(bounds.Left, bounds.Y - m_theme.PickTolerance, bounds.Width, 2 * m_theme.PickTolerance);
				if (rectangleF3.Contains(p))
				{
					DiagramBorder border7 = new DiagramBorder(item)
					{
						Border = DiagramBorder.BorderType.Top
					};
					return new AnnotationHitEventArgs(item, border7);
				}
				rectangleF3.Offset(0f, bounds.Height);
				if (rectangleF3.Contains(p))
				{
					DiagramBorder border8 = new DiagramBorder(item)
					{
						Border = DiagramBorder.BorderType.Bottom
					};
					return new AnnotationHitEventArgs(item, border8);
				}
				rectangle.Inflate(pickTolerance, pickTolerance);
				DiagramLabel diagramLabel = null;
				if (!rectangle.Contains(p))
				{
					continue;
				}
				Rectangle labelBounds = new Rectangle(bounds.X + Margin.Left, bounds.Y + Margin.Top, (int)textEditor.TextLayout.LayoutWidth + SystemInformation.VerticalScrollBarWidth, (int)textEditor.TextLayout.LayoutHeight);
				diagramLabel = new DiagramLabel(labelBounds, TextFormatFlags.LeftAndRightPadding);
				RectangleF rectangleF4 = new RectangleF(bounds.X + Margin.Left, bounds.Y + Margin.Top, bounds.Width - Margin.Size.Width, bounds.Height - Margin.Size.Height);
				rectangleF4.Width = Math.Max(rectangleF4.Width, 50f);
				rectangleF4.Height = Math.Max(rectangleF4.Height, MinimumHeight);
				PointF pointF = new PointF(rectangleF4.Location.X, rectangleF4.Location.Y - textEditor.GetLineYOffset(textEditor.TopLine));
				AnnotationHitEventArgs e = new AnnotationHitEventArgs(item, diagramLabel);
				e.Position = new PointF((float)p.X - pointF.X, (float)p.Y - pointF.Y);
				return e;
			}
		}
		return new AnnotationHitEventArgs();
	}

	DiagramHitRecord IPickingAdapter2.Pick(Point p)
	{
		return Pick(p);
	}

	IEnumerable<object> IPickingAdapter2.Pick(Rectangle bounds)
	{
		if (m_annotatedDiagram == null)
		{
			return EmptyEnumerable<object>.Instance;
		}
		List<object> list = new List<object>();
		if (m_transformAdapter != null)
		{
			bounds = GdiUtil.InverseTransform(m_transformAdapter.Transform, bounds);
		}
		foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
		{
			Rectangle bounds2 = GetBounds(annotation);
			if (bounds.IntersectsWith(bounds2))
			{
				list.Add(annotation);
			}
		}
		return list;
	}

	Rectangle IPickingAdapter2.GetBounds(IEnumerable<object> items)
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

	void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
	{
		m_draggingAnnotations = m_selectionContext.GetSelection<IAnnotation>().ToArray();
		m_moving = true;
		m_newPositions = new Point[m_draggingAnnotations.Length];
		m_oldPositions = new Point[m_draggingAnnotations.Length];
		for (int i = 0; i < m_draggingAnnotations.Length; i++)
		{
			Point location = m_draggingAnnotations[i].Bounds.Location;
			m_newPositions[i] = location;
			m_oldPositions[i] = location;
		}
		if (m_autoTranslateAdapter != null)
		{
			m_autoTranslateAdapter.Enabled = true;
		}
	}

	void IItemDragAdapter.EndingDrag()
	{
		for (int i = 0; i < m_draggingAnnotations.Length; i++)
		{
			IAnnotation annotation = m_draggingAnnotations[i];
			MoveAnnotation(annotation, m_oldPositions[i]);
		}
	}

	void IItemDragAdapter.EndDrag()
	{
		for (int i = 0; i < m_draggingAnnotations.Length; i++)
		{
			IAnnotation annotation = m_draggingAnnotations[i];
			MoveAnnotation(annotation, m_newPositions[i]);
		}
		m_draggingAnnotations = null;
	}

	protected override void Bind(AdaptableControl control)
	{
		m_transformAdapter = control.As<ITransformAdapter>();
		m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();
		D2dAdaptableControl d2dAdaptableControl = control as D2dAdaptableControl;
		d2dAdaptableControl.ContextChanged += control_ContextChanged;
		d2dAdaptableControl.DrawingD2d += control_Paint;
		d2dAdaptableControl.KeyPress += control_KeyPress;
		d2dAdaptableControl.PreviewKeyDown += control_PreviewKeyDown;
		d2dAdaptableControl.GotFocus += control_GotFocus;
		d2dAdaptableControl.LostFocus += control_LostFocus;
		d2dAdaptableControl.DoubleClick += control_DoubleClick;
		base.Bind(control);
	}

	protected override void Unbind(AdaptableControl control)
	{
		D2dAdaptableControl d2dAdaptableControl = control as D2dAdaptableControl;
		d2dAdaptableControl.ContextChanged -= control_ContextChanged;
		d2dAdaptableControl.DrawingD2d -= control_Paint;
		d2dAdaptableControl.KeyPress -= control_KeyPress;
		d2dAdaptableControl.PreviewKeyDown -= control_PreviewKeyDown;
		d2dAdaptableControl.GotFocus -= control_GotFocus;
		d2dAdaptableControl.LostFocus -= control_LostFocus;
		m_transformAdapter = null;
		m_autoTranslateAdapter = null;
		base.Unbind(control);
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		m_annotationEditors.Clear();
		IAnnotatedDiagram annotatedDiagram = base.AdaptedControl.ContextAs<IAnnotatedDiagram>();
		m_coloringContext = base.AdaptedControl.ContextAs<IColoringContext>();
		if (m_annotatedDiagram == annotatedDiagram)
		{
			return;
		}
		if (m_annotatedDiagram != null)
		{
			if (m_observableContext != null)
			{
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
			}
			m_selectionContext = base.AdaptedControl.ContextAs<ISelectionContext>();
			if (m_selectionContext != null)
			{
				m_selectionContext.SelectionChanged += selection_Changed;
			}
			m_layoutContext = base.AdaptedControl.ContextAs<ILayoutContext>();
		}
	}

	private void control_Paint(object sender, EventArgs e)
	{
		if (m_annotatedDiagram == null)
		{
			return;
		}
		D2dAdaptableControl d2dAdaptableControl = (D2dAdaptableControl)base.AdaptedControl;
		D2dGraphics d2dGraphics = d2dAdaptableControl.D2dGraphics;
		Matrix3x2F transform = d2dGraphics.Transform;
		m_scaleX = transform.M11;
		transform.Invert();
		RectangleF graphBound = D2dUtil.Transform(transform, d2dAdaptableControl.ClientRectangle);
		D2dParagraphAlignment paragraphAlignment = m_theme.TextFormat.ParagraphAlignment;
		D2dTextAlignment textAlignment = m_theme.TextFormat.TextAlignment;
		D2dDrawTextOptions drawTextOptions = m_theme.TextFormat.DrawTextOptions;
		m_theme.TextFormat.ParagraphAlignment = D2dParagraphAlignment.Near;
		m_theme.TextFormat.TextAlignment = D2dTextAlignment.Leading;
		m_theme.TextFormat.DrawTextOptions = D2dDrawTextOptions.Clip;
		bool drawText = m_scaleX * m_theme.TextFormat.FontHeight > 5f;
		foreach (IAnnotation annotation in m_annotatedDiagram.Annotations)
		{
			Rectangle bounds = annotation.Bounds;
			if (bounds.Size.IsEmpty)
			{
				if (string.IsNullOrEmpty(annotation.Text))
				{
					annotation.SetTextSize(new Size(180, 100));
				}
				else
				{
					SizeF sizeF = d2dGraphics.MeasureText(annotation.Text, m_theme.TextFormat);
					Size size = new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));
					size.Width += 2 * Margin.Size.Width;
					size.Height += 2 * Margin.Size.Height;
					bounds.Size = size;
					annotation.SetTextSize(size);
				}
			}
			if (!graphBound.IntersectsWith(bounds))
			{
				continue;
			}
			DiagramDrawingStyle style = DiagramDrawingStyle.Normal;
			if (m_selectionContext.SelectionContains(annotation))
			{
				style = DiagramDrawingStyle.Selected;
				if (m_selectionContext.GetLastSelected<IAnnotation>() == annotation)
				{
					style = DiagramDrawingStyle.LastSelected;
				}
			}
			DrawAnnotation(annotation, style, d2dGraphics, drawText, graphBound);
		}
		m_theme.TextFormat.ParagraphAlignment = paragraphAlignment;
		m_theme.TextFormat.TextAlignment = textAlignment;
		m_theme.TextFormat.DrawTextOptions = drawTextOptions;
	}

	protected override void OnMouseMove(object sender, MouseEventArgs e)
	{
		base.OnMouseMove(sender, e);
		AnnotationHitEventArgs e2 = Pick(base.CurrentPoint);
		if (e2.Annotation != null && base.AdaptedControl.Cursor == Cursors.Default)
		{
			if (e2.Label != null)
			{
				base.AdaptedControl.Cursor = Cursors.IBeam;
			}
			else if (e2.Part.Is<DiagramTitleBar>())
			{
				base.AdaptedControl.Cursor = Cursors.SizeAll;
			}
			else if (e2.Part.Is<DiagramBorder>())
			{
				DiagramBorder diagramBorder = e2.Part.Cast<DiagramBorder>();
				base.AdaptedControl.AutoResetCursor = false;
				if (diagramBorder.Border == DiagramBorder.BorderType.Right || diagramBorder.Border == DiagramBorder.BorderType.Left)
				{
					base.AdaptedControl.Cursor = Cursors.SizeWE;
				}
				else if (diagramBorder.Border == DiagramBorder.BorderType.Bottom || diagramBorder.Border == DiagramBorder.BorderType.Top)
				{
					base.AdaptedControl.Cursor = Cursors.SizeNS;
				}
				else if (diagramBorder.Border == DiagramBorder.BorderType.LowerRightCorner)
				{
					base.AdaptedControl.Cursor = Cursors.SizeNWSE;
				}
				else if (diagramBorder.Border == DiagramBorder.BorderType.UpperLeftCorner)
				{
					base.AdaptedControl.Cursor = Cursors.SizeNWSE;
				}
				else if (diagramBorder.Border == DiagramBorder.BorderType.UpperRightCorner)
				{
					base.AdaptedControl.Cursor = Cursors.SizeNESW;
				}
				else if (diagramBorder.Border == DiagramBorder.BorderType.LowerLeftCorner)
				{
					base.AdaptedControl.Cursor = Cursors.SizeNESW;
				}
			}
			else if (!e2.Part.Is<DiagramScrollBar>())
			{
				base.AdaptedControl.Cursor = Cursors.SizeAll;
			}
			base.AdaptedControl.AutoResetCursor = false;
		}
		else
		{
			base.AdaptedControl.AutoResetCursor = true;
		}
	}

	protected override void OnMouseDown(object sender, MouseEventArgs e)
	{
		base.OnMouseDown(sender, e);
		AnnotationHitEventArgs e2 = Pick(base.CurrentPoint);
		base.AdaptedControl.HasKeyboardFocus = false;
		bool editingText = m_editingText;
		m_editingText = false;
		if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Alt) == 0)
		{
			if (e2.Annotation != null && e2.Label != null)
			{
				m_editingText = true;
				base.AdaptedControl.HasKeyboardFocus = true;
				if (m_annotationEditors.ContainsKey(e2.Annotation))
				{
					TextEditor textEditor = m_annotationEditors[e2.Annotation];
					textEditor.SetSelectionFromPoint(e2.Position.X, e2.Position.Y, extendSelection: false);
					base.AdaptedControl.Invalidate();
				}
				if (base.AdaptedControl.Cursor == Cursors.Default)
				{
					base.AdaptedControl.Cursor = Cursors.IBeam;
				}
			}
		}
		else if (e.Button == MouseButtons.Right)
		{
			m_rmbPressed = true;
			if (e2.Annotation != null && e2.Part.Is<DiagramTitleBar>())
			{
				base.AdaptedControl.HasKeyboardFocus = false;
			}
		}
		if (editingText != m_editingText)
		{
			base.AdaptedControl.Invalidate();
		}
	}

	protected override void OnMouseUp(object sender, MouseEventArgs e)
	{
		m_rmbPressed = false;
	}

	protected override void OnBeginDrag(MouseEventArgs e)
	{
		base.OnBeginDrag(e);
		if (m_layoutContext == null || e.Button != MouseButtons.Left || (Control.ModifierKeys & Keys.Alt) != Keys.None || base.AdaptedControl.Capture)
		{
			return;
		}
		m_mousePick = Pick(base.FirstPoint);
		if (m_mousePick.Item == null)
		{
			return;
		}
		foreach (IItemDragAdapter item in base.AdaptedControl.AsAll<IItemDragAdapter>())
		{
			if (item != this)
			{
				item.BeginDrag(this);
			}
		}
		m_draggingAnnotations = m_selectionContext.GetSelection<IAnnotation>().ToArray();
		if (m_mousePick.Item.Is<IAnnotation>())
		{
			if (m_mousePick.Part.Is<DiagramBorder>())
			{
				m_startBounds = m_mousePick.Item.Cast<IAnnotation>().Bounds;
				m_resizing = true;
			}
			else if (m_mousePick.Part.Is<DiagramTitleBar>())
			{
				m_moving = true;
			}
			else if (m_mousePick.Part.Is<DiagramLabel>())
			{
				m_selecting = true;
			}
			else if (m_mousePick.Part.Is<DiagramScrollBar>())
			{
				TextEditor textEditor = m_annotationEditors[m_mousePick.Item.Cast<IAnnotation>()];
				m_startTopLine = textEditor.TopLine;
				m_scrolling = true;
			}
		}
		m_newPositions = new Point[m_draggingAnnotations.Length];
		m_oldPositions = new Point[m_draggingAnnotations.Length];
		for (int i = 0; i < m_draggingAnnotations.Length; i++)
		{
			Point location = m_draggingAnnotations[i].Bounds.Location;
			m_newPositions[i] = location;
			m_oldPositions[i] = location;
		}
		base.AdaptedControl.Capture = true;
		if (m_autoTranslateAdapter != null)
		{
			m_autoTranslateAdapter.Enabled = !m_scrolling;
		}
	}

	protected override void OnDragging(MouseEventArgs e)
	{
		D2dAdaptableControl d2dAdaptableControl = base.AdaptedControl as D2dAdaptableControl;
		if (m_resizing)
		{
			ResizeAnnotation(m_mousePick.Part.As<DiagramBorder>());
			d2dAdaptableControl.Invalidate();
		}
		else if (m_scrolling)
		{
			ScrollAnnotation(m_mousePick.Part.As<DiagramScrollBar>());
			d2dAdaptableControl.Invalidate();
		}
		else
		{
			if (m_draggingAnnotations == null)
			{
				return;
			}
			if (m_moving)
			{
				Matrix3x2F mat = Matrix3x2F.Invert(d2dAdaptableControl.D2dGraphics.Transform);
				PointF pointF = Matrix3x2F.TransformVector(mat, base.Delta);
				Point point = new Point((int)pointF.X, (int)pointF.Y);
				for (int i = 0; i < m_draggingAnnotations.Length; i++)
				{
					IAnnotation annotation = m_draggingAnnotations[i];
					Rectangle bounds = GetBounds(annotation);
					bounds.X = m_oldPositions[i].X + point.X;
					bounds.Y = m_oldPositions[i].Y + point.Y;
					m_newPositions[i] = bounds.Location;
					m_layoutContext.SetBounds(annotation, bounds, BoundsSpecified.Location);
				}
			}
			else if (m_selecting)
			{
				AnnotationHitEventArgs e2 = Pick(base.CurrentPoint);
				if (e2.Annotation != null && e2.Label != null && m_annotationEditors.ContainsKey(e2.Annotation))
				{
					TextEditor textEditor = m_annotationEditors[e2.Annotation];
					textEditor.SetSelectionFromPoint(e2.Position.X, e2.Position.Y, extendSelection: true);
					base.AdaptedControl.Invalidate();
				}
			}
			d2dAdaptableControl.Invalidate();
		}
	}

	protected override void OnEndDrag(MouseEventArgs e)
	{
		base.OnEndDrag(e);
		if (m_draggingAnnotations != null)
		{
			ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
			if (m_moving)
			{
				foreach (IItemDragAdapter item in base.AdaptedControl.AsAll<IItemDragAdapter>())
				{
					item.EndingDrag();
				}
				context.DoTransaction(delegate
				{
					foreach (IItemDragAdapter item2 in base.AdaptedControl.AsAll<IItemDragAdapter>())
					{
						item2.EndDrag();
					}
				}, "Drag Items".Localize());
			}
			else if (!m_selecting && m_resizing)
			{
				context.DoTransaction(delegate
				{
					ResizeAnnotation(m_mousePick.Part.Cast<DiagramBorder>());
				}, "Resize Annotation".Localize());
			}
			if (m_autoTranslateAdapter != null)
			{
				m_autoTranslateAdapter.Enabled = false;
			}
			m_draggingAnnotations = null;
			m_newPositions = null;
			m_oldPositions = null;
			base.AdaptedControl.Invalidate();
		}
		m_resizing = false;
		m_scrolling = false;
		m_moving = false;
		m_selecting = false;
	}

	private void DrawAnnotation(IAnnotation annotation, DiagramDrawingStyle style, D2dGraphics g, bool drawText, RectangleF graphBound)
	{
		Rectangle bounds = annotation.Bounds;
		Color color = ((m_coloringContext == null) ? SystemColors.Info : m_coloringContext.GetColor(ColoringTypes.BackColor, annotation));
		Color color2 = ((m_coloringContext == null) ? SystemColors.WindowText : m_coloringContext.GetColor(ColoringTypes.ForeColor, annotation));
		float strokeWidth = 1f / m_scaleX;
		g.FillRectangle(bounds, color);
		g.DrawRectangle(bounds, ControlPaint.Dark(color), strokeWidth);
		if (style == DiagramDrawingStyle.LastSelected || style == DiagramDrawingStyle.Selected)
		{
			RectangleF rect = new RectangleF(bounds.X, bounds.Y, bounds.Width, Margin.Top - 1);
			g.FillRectangle(rect, ControlPaint.Dark(color));
		}
		g.DrawLine(bounds.X, bounds.Y + Margin.Top - 1, bounds.X + bounds.Width, bounds.Y + Margin.Top - 1, ControlPaint.Dark(color), strokeWidth);
		if (drawText)
		{
			RectangleF rectangleF = new RectangleF(bounds.X + Margin.Left, bounds.Y + Margin.Top, bounds.Width - Margin.Size.Width, bounds.Height - Margin.Size.Height);
			rectangleF.Width = Math.Max(rectangleF.Width, 50f);
			rectangleF.Height = Math.Max(rectangleF.Height, MinimumHeight);
			RectangleF rectangleF2 = rectangleF;
			int num = 0;
			D2dTextLayout d2dTextLayout = null;
			if (m_annotationEditors.ContainsKey(annotation))
			{
				TextEditor textEditor = m_annotationEditors[annotation];
				if (textEditor.TextLayout.Text != annotation.Text)
				{
					textEditor.ResetText(annotation.Text);
				}
				num = textEditor.TopLine;
				d2dTextLayout = textEditor.TextLayout;
				textEditor.VerticalScrollBarVisibe = d2dTextLayout.Height > d2dTextLayout.LayoutHeight;
				if (textEditor.VerticalScrollBarVisibe)
				{
					rectangleF2.Width -= 9f;
				}
				if ((double)(Math.Abs(d2dTextLayout.LayoutWidth - rectangleF2.Width) + Math.Abs(d2dTextLayout.LayoutHeight - rectangleF2.Height)) > 1.0)
				{
					d2dTextLayout.LayoutWidth = rectangleF2.Width;
					d2dTextLayout.LayoutHeight = rectangleF2.Height;
					textEditor.Validate();
				}
			}
			if (d2dTextLayout == null)
			{
				d2dTextLayout = D2dFactory.CreateTextLayout(annotation.Text, m_theme.TextFormat, rectangleF.Width, rectangleF.Height);
				if (m_theme.TextFormat.Underlined)
				{
					d2dTextLayout.SetUnderline(hasUnderline: true, 0, annotation.Text.Length);
				}
				if (m_theme.TextFormat.Strikeout)
				{
					d2dTextLayout.SetStrikethrough(hasStrikethrough: true, 0, annotation.Text.Length);
				}
				if (d2dTextLayout.Height > d2dTextLayout.LayoutHeight)
				{
					d2dTextLayout.LayoutWidth = rectangleF.Width - 5f - 4f;
				}
				m_annotationEditors.Add(annotation, new TextEditor
				{
					TextLayout = d2dTextLayout,
					TextFormat = m_theme.TextFormat,
					TopLine = num,
					VerticalScrollBarVisibe = (d2dTextLayout.Height > d2dTextLayout.LayoutHeight)
				});
			}
			TextEditor textEditor2 = m_annotationEditors[annotation];
			float lineYOffset = textEditor2.GetLineYOffset(num);
			PointF pointF = new PointF(rectangleF.Location.X, rectangleF.Location.Y - lineYOffset);
			g.PushAxisAlignedClip(rectangleF);
			if (textEditor2.SelectionLength > 0)
			{
				HitTestMetrics[] array = d2dTextLayout.HitTestTextRange(textEditor2.SelectionStart, textEditor2.SelectionLength, 0f, 0f);
				for (int i = 0; i < array.Length; i++)
				{
					RectangleF rect2 = new RectangleF(array[i].Point.X, array[i].Point.Y, array[i].Width, array[i].Height);
					rect2.Offset(pointF);
					g.FillRectangle(rect2, m_theme.TextHighlightBrush);
				}
			}
			if (style == DiagramDrawingStyle.Selected || style == DiagramDrawingStyle.LastSelected)
			{
				d2dTextLayout = textEditor2.TextLayout;
				RectangleF caretRect = m_annotationEditors[annotation].GetCaretRect();
				caretRect.Offset(pointF);
				if (m_editingText && rectangleF.IntersectsWith(caretRect) && graphBound.IntersectsWith(caretRect) && base.AdaptedControl.Focused)
				{
					RectangleF rectangleF3 = GdiUtil.Transform(m_transformAdapter.Transform, caretRect);
					float num2 = m_scaleX * m_theme.TextFormat.FontHeight / (float)CaretHeight;
					if (num2 > 1.1f || num2 < 0.9f)
					{
						CaretHeight = (int)(m_scaleX * m_theme.TextFormat.FontHeight);
						User32.DestroyCaret();
						User32.CreateCaret(base.AdaptedControl.Handle, IntPtr.Zero, 2, CaretHeight);
					}
					User32.SetCaretPos((int)rectangleF3.X, (int)(rectangleF3.Y + rectangleF3.Height - (float)CaretHeight));
					if (!m_rmbPressed)
					{
						base.AdaptedControl.HasKeyboardFocus = true;
					}
				}
				else
				{
					HideCaret();
				}
			}
			g.DrawTextLayout(pointF, d2dTextLayout, color2);
			g.PopAxisAlignedClip();
			if (rectangleF.Height < d2dTextLayout.Height)
			{
				float num3 = textEditor2.GetVisibleLines();
				float num4 = (float)num * rectangleF.Height / (float)d2dTextLayout.LineCount;
				float num5 = ((float)num + num3 - 1f) * rectangleF.Height / (float)d2dTextLayout.LineCount;
				if (m_scrolling)
				{
					RectangleF rect3 = new RectangleF(rectangleF.Right - 2f - 5f, rectangleF.Y, 5f, rectangleF.Height);
					g.FillRectangle(rect3, Color.Gainsboro);
				}
				RectangleF rect4 = new RectangleF(rectangleF.Right - 2f - 5f, rectangleF.Y + num4, 5f, num5 - num4);
				g.FillRectangle(rect4, Color.DimGray);
			}
		}
		else
		{
			HideCaret();
		}
	}

	private void MoveAnnotation(IAnnotation annotation, Point location)
	{
		Rectangle bounds = new Rectangle(location.X, location.Y, 0, 0);
		m_layoutContext.SetBounds(annotation, bounds, BoundsSpecified.Location);
	}

	private void ResizeAnnotation(DiagramBorder diagramBorder)
	{
		if (diagramBorder != null)
		{
			Point point = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.CurrentPoint);
			Point point2 = GdiUtil.InverseTransform(m_transformAdapter.Transform, base.FirstPoint);
			Point point3 = new Point(point.X - point2.X, point.Y - point2.Y);
			switch (diagramBorder.Border)
			{
			case DiagramBorder.BorderType.LowerRightCorner:
			{
				Rectangle annotationBounds = new Rectangle(m_startBounds.X, m_startBounds.Y, m_startBounds.Width + point3.X, m_startBounds.Height + point3.Y);
				m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(annotationBounds), BoundsSpecified.All);
				break;
			}
			case DiagramBorder.BorderType.UpperLeftCorner:
			{
				Rectangle annotationBounds = new Rectangle(point.X, point.Y, m_startBounds.Width - point3.X, m_startBounds.Height - point3.Y);
				m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(annotationBounds), BoundsSpecified.All);
				break;
			}
			case DiagramBorder.BorderType.UpperRightCorner:
			{
				Rectangle annotationBounds = new Rectangle(m_startBounds.X, point.Y, m_startBounds.Width + point3.X, m_startBounds.Height - point3.Y);
				m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(annotationBounds), BoundsSpecified.All);
				break;
			}
			case DiagramBorder.BorderType.LowerLeftCorner:
			{
				Rectangle annotationBounds = new Rectangle(point.X, m_startBounds.Y, m_startBounds.Width - point3.X, m_startBounds.Height + point3.Y);
				m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(annotationBounds), BoundsSpecified.All);
				break;
			}
			case DiagramBorder.BorderType.Left:
			{
				Rectangle annotationBounds = new Rectangle(point.X, m_startBounds.Y, m_startBounds.Width - point3.X, m_startBounds.Height);
				m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(annotationBounds), BoundsSpecified.All);
				break;
			}
			case DiagramBorder.BorderType.Right:
			{
				Rectangle annotationBounds = new Rectangle(m_startBounds.X, m_startBounds.Y, m_startBounds.Width + point3.X, m_startBounds.Height);
				m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(annotationBounds), BoundsSpecified.Size);
				break;
			}
			case DiagramBorder.BorderType.Top:
			{
				Rectangle annotationBounds = new Rectangle(m_startBounds.X, point.Y, m_startBounds.Width, m_startBounds.Height - point3.Y);
				m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(annotationBounds), BoundsSpecified.All);
				break;
			}
			case DiagramBorder.BorderType.Bottom:
			{
				Rectangle annotationBounds = new Rectangle(m_startBounds.X, m_startBounds.Y, m_startBounds.Width, m_startBounds.Height + point3.Y);
				m_layoutContext.SetBounds(diagramBorder.Item, ConstrainBounds(annotationBounds), BoundsSpecified.Size);
				break;
			}
			}
		}
	}

	private Rectangle ConstrainBounds(Rectangle annotationBounds)
	{
		return new Rectangle(annotationBounds.X, annotationBounds.Y, Math.Max(annotationBounds.Width, 50), Math.Max(annotationBounds.Height, MinimumHeight + Margin.Top));
	}

	private void ScrollAnnotation(DiagramScrollBar scrollBar)
	{
		if (scrollBar != null)
		{
			Point delta = base.Delta;
			TextEditor textEditor = m_annotationEditors[scrollBar.Item.Cast<IAnnotation>()];
			float num = textEditor.TextLayout.Height / (float)textEditor.TextLayout.LineCount;
			float num2 = (float)delta.Y / num;
			int num3 = m_startTopLine + (int)Math.Ceiling(num2);
			float num4 = textEditor.TextLayout.LayoutHeight / num;
			if (num3 < 0)
			{
				num3 = 0;
			}
			if ((int)((float)num3 + num4 - 1f) > textEditor.TextLayout.LineCount)
			{
				num3 = textEditor.TopLine;
			}
			textEditor.TopLine = ((num3 >= 0) ? num3 : 0);
		}
	}

	private Rectangle GetBounds(IAnnotation annotation)
	{
		Rectangle bounds = annotation.Bounds;
		if (bounds.Size.IsEmpty && m_theme != null)
		{
			D2dGraphics d2dGraphics = ((D2dAdaptableControl)base.AdaptedControl).D2dGraphics;
			SizeF sizeF = d2dGraphics.MeasureText(annotation.Text, m_theme.TextFormat);
			bounds.Size = new Size((int)sizeF.Width + 2 * Margin.Size.Width, (int)sizeF.Height + 2 * Margin.Size.Height);
		}
		return bounds;
	}

	private void control_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!base.AdaptedControl.HasKeyboardFocus)
		{
			return;
		}
		IAnnotation annotation = m_selectionContext.GetSelection<IAnnotation>().FirstOrDefault();
		if (annotation == null || !m_annotationEditors.ContainsKey(annotation))
		{
			return;
		}
		TextEditor annotationEditor = m_annotationEditors[annotation];
		PropertyInfo textProperty = annotation.GetType().GetProperty("Text");
		if (!textProperty.CanWrite)
		{
			return;
		}
		ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
		if (base.AdaptedControl.IsImeChar)
		{
			return;
		}
		switch (e.KeyChar)
		{
		case '\r':
			context.DoTransaction(delegate
			{
				DeleteTextSelection(annotation);
				InsertText(annotation, "\r\n");
			}, EditAnnotation);
			annotationEditor.SetSelection(TextEditor.SelectionMode.AbsoluteLeading, annotationEditor.CaretAbsolutePosition + 2, extendSelection: false, updateCaretFormat: false);
			break;
		case '\b':
			context.DoTransaction(delegate
			{
				if (annotationEditor.CaretAbsolutePosition != annotationEditor.CaretAnchorPosition)
				{
					DeleteTextSelection(annotation);
				}
				else if (annotationEditor.CaretAbsolutePosition > 0)
				{
					int num = 1;
					if (annotationEditor.CaretAbsolutePosition >= 2 && annotationEditor.CaretAbsolutePosition <= annotation.Text.Length)
					{
						char c = annotation.Text[annotationEditor.CaretAbsolutePosition - 1];
						char c2 = annotation.Text[annotationEditor.CaretAbsolutePosition - 2];
						if ((char.IsLowSurrogate(c) && char.IsHighSurrogate(c2)) || (c == '\n' && c2 == '\r'))
						{
							num = 2;
						}
					}
					annotationEditor.SetSelection(TextEditor.SelectionMode.LeftChar, num, extendSelection: false, updateCaretFormat: false);
					string value = annotationEditor.RemoveTextAt(annotation.Text, annotationEditor.CaretPosition, num);
					textProperty.SetValue(annotation, value, null);
				}
			}, EditAnnotation);
			break;
		default:
			if (e.KeyChar >= ' ')
			{
				InsertChar(annotation, annotationEditor, e.KeyChar);
				base.AdaptedControl.Invalidate();
			}
			break;
		}
	}

	private void control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (!base.AdaptedControl.HasKeyboardFocus)
		{
			return;
		}
		IAnnotation annotation = m_selectionContext.GetSelection<IAnnotation>().FirstOrDefault();
		if (annotation == null || !m_annotationEditors.ContainsKey(annotation))
		{
			return;
		}
		TextEditor annotationEditor = m_annotationEditors[annotation];
		PropertyInfo textProperty = annotation.GetType().GetProperty("Text");
		if (!textProperty.CanWrite)
		{
			return;
		}
		ITransactionContext transactionContext = base.AdaptedControl.ContextAs<ITransactionContext>();
		Keys keys = e.KeyData & Keys.KeyCode;
		bool flag = (e.KeyData & Keys.Control) == Keys.Control;
		bool extendSelection = (e.KeyData & Keys.Shift) == Keys.Shift;
		switch (keys)
		{
		case Keys.Delete:
			if (flag)
			{
				annotationEditor.SetSelection(TextEditor.SelectionMode.RightWord, 1, extendSelection: true, updateCaretFormat: false);
				transactionContext.DoTransaction(delegate
				{
					DeleteTextSelection(annotation);
				}, EditAnnotation);
			}
			else
			{
				transactionContext.DoTransaction(delegate
				{
					if (annotationEditor.CaretAbsolutePosition != annotationEditor.CaretAnchorPosition)
					{
						DeleteTextSelection(annotation);
					}
					else
					{
						HitTestMetrics hitTestMetrics = annotationEditor.TextLayout.HitTestTextPosition(annotationEditor.CaretAbsolutePosition, isTrailingHit: false);
						string value = annotationEditor.RemoveTextAt(annotation.Text, hitTestMetrics.TextPosition, hitTestMetrics.Length);
						textProperty.SetValue(annotation, value, null);
						annotationEditor.SetSelection(TextEditor.SelectionMode.AbsoluteLeading, hitTestMetrics.TextPosition, extendSelection: false, updateCaretFormat: false);
					}
				}, EditAnnotation);
			}
			base.AdaptedControl.Invalidate();
			break;
		case Keys.Tab:
			InsertChar(annotation, annotationEditor, '\t');
			base.AdaptedControl.Invalidate();
			break;
		case Keys.Left:
			annotationEditor.SetSelection(flag ? TextEditor.SelectionMode.LeftWord : TextEditor.SelectionMode.Left, 1, extendSelection, updateCaretFormat: false);
			base.AdaptedControl.Invalidate();
			break;
		case Keys.Right:
			annotationEditor.SetSelection((!flag) ? TextEditor.SelectionMode.Right : TextEditor.SelectionMode.RightWord, 1, extendSelection, updateCaretFormat: false);
			base.AdaptedControl.Invalidate();
			break;
		case Keys.Up:
			annotationEditor.SetSelection(TextEditor.SelectionMode.Up, 1, extendSelection, updateCaretFormat: false);
			base.AdaptedControl.Invalidate();
			break;
		case Keys.Down:
			annotationEditor.SetSelection(TextEditor.SelectionMode.Down, 1, extendSelection, updateCaretFormat: false);
			base.AdaptedControl.Invalidate();
			break;
		case Keys.Home:
			annotationEditor.SetSelection(flag ? TextEditor.SelectionMode.First : TextEditor.SelectionMode.Home, 0, extendSelection, updateCaretFormat: false);
			base.AdaptedControl.Invalidate();
			break;
		case Keys.End:
			annotationEditor.SetSelection(flag ? TextEditor.SelectionMode.Last : TextEditor.SelectionMode.End, 0, extendSelection, updateCaretFormat: false);
			base.AdaptedControl.Invalidate();
			break;
		case Keys.Insert:
			if (flag)
			{
				CopyToClipboard(annotation);
			}
			else
			{
				PasteFromClipboard(annotation);
			}
			break;
		}
		if (e.KeyCode == Keys.X && flag)
		{
			CopyToClipboard(annotation);
			transactionContext.DoTransaction(delegate
			{
				DeleteTextSelection(annotation);
			}, EditAnnotation);
		}
		else if (e.KeyCode == Keys.C && flag)
		{
			CopyToClipboard(annotation);
		}
		else if (e.KeyCode == Keys.V && flag)
		{
			transactionContext.DoTransaction(delegate
			{
				PasteFromClipboard(annotation);
			}, EditAnnotation);
			base.AdaptedControl.Invalidate();
		}
		else if (e.KeyCode == Keys.A && flag)
		{
			annotationEditor.SetSelection(TextEditor.SelectionMode.All, 0, extendSelection: true, updateCaretFormat: false);
			base.AdaptedControl.Invalidate();
		}
		else if (e.KeyCode == Keys.Z && flag)
		{
			IHistoryContext historyContext = transactionContext.As<IHistoryContext>();
			if (historyContext != null && historyContext.CanUndo)
			{
				historyContext.Undo();
			}
			base.AdaptedControl.Invalidate();
		}
		else if (e.KeyCode == Keys.Y && flag)
		{
			IHistoryContext historyContext2 = transactionContext.As<IHistoryContext>();
			if (historyContext2 != null && historyContext2.CanRedo)
			{
				historyContext2.Redo();
			}
			base.AdaptedControl.Invalidate();
		}
	}

	private void InsertChar(IAnnotation annotation, TextEditor annotationEditor, char charValue)
	{
		ITransactionContext context = base.AdaptedControl.ContextAs<ITransactionContext>();
		context.DoTransaction(delegate
		{
			DeleteTextSelection(annotation);
			InsertText(annotation, new string(charValue, 1));
			annotationEditor.SetSelection(TextEditor.SelectionMode.Right, 1, extendSelection: false, updateCaretFormat: false);
		}, EditAnnotation);
	}

	private void control_LostFocus(object sender, EventArgs e)
	{
		User32.DestroyCaret();
		m_caretCreated = false;
		base.AdaptedControl.HasKeyboardFocus = false;
	}

	private void control_GotFocus(object sender, EventArgs e)
	{
		CaretHeight = (int)(m_scaleX * m_theme.TextFormat.FontHeight);
		User32.CreateCaret(base.AdaptedControl.Handle, IntPtr.Zero, 2, CaretHeight);
		User32.ShowCaret(base.AdaptedControl.Handle);
		m_caretCreated = true;
		if (m_selectionContext != null)
		{
			if (m_selectionContext.GetSelection<IAnnotation>().FirstOrDefault() != null)
			{
				base.AdaptedControl.Invalidate();
			}
			else
			{
				HideCaret();
			}
		}
	}

	private void control_DoubleClick(object sender, EventArgs e)
	{
		Point p = base.AdaptedControl.PointToClient(Control.MousePosition);
		AnnotationHitEventArgs e2 = Pick(p);
		if (e2.Annotation != null && e2.Label != null && m_annotationEditors.ContainsKey(e2.Annotation))
		{
			TextEditor textEditor = m_annotationEditors[e2.Annotation];
			textEditor.SetSelection(TextEditor.SelectionMode.SingleWord, 0, extendSelection: true, updateCaretFormat: false);
			base.AdaptedControl.Invalidate();
		}
	}

	public void DeleteTextSelection(IAnnotation annotation)
	{
		if (!m_annotationEditors.ContainsKey(annotation))
		{
			return;
		}
		PropertyInfo property = annotation.GetType().GetProperty("Text");
		if (property.CanWrite)
		{
			TextEditor textEditor = m_annotationEditors[annotation];
			textEditor.UpdateSelectionRange();
			if (textEditor.SelectionLength > 0)
			{
				string value = textEditor.RemoveTextAt(annotation.Text, textEditor.SelectionStart, textEditor.SelectionLength);
				property.SetValue(annotation, value, null);
				textEditor.SetSelection(TextEditor.SelectionMode.AbsoluteLeading, textEditor.SelectionStart, extendSelection: false, updateCaretFormat: false);
			}
		}
	}

	public void PasteFromClipboard(IAnnotation annotation)
	{
		if (m_annotationEditors.ContainsKey(annotation))
		{
			TextEditor textEditor = m_annotationEditors[annotation];
			string text = Clipboard.GetText();
			DeleteTextSelection(annotation);
			InsertText(annotation, text);
			textEditor.SetSelection(TextEditor.SelectionMode.RightChar, text.Length, extendSelection: false, updateCaretFormat: false);
		}
	}

	private void CopyToClipboard(IAnnotation annotation)
	{
		string text = TextSelected(annotation);
		if (!string.IsNullOrEmpty(text))
		{
			Clipboard.SetText(text);
		}
	}

	private void InsertText(IAnnotation annotation, string text)
	{
		if (m_annotationEditors.ContainsKey(annotation))
		{
			PropertyInfo property = annotation.GetType().GetProperty("Text");
			if (property.CanWrite)
			{
				TextEditor textEditor = m_annotationEditors[annotation];
				string value = textEditor.InsertTextAt(annotation.Text, text);
				property.SetValue(annotation, value, null);
			}
		}
	}

	public bool CanDeleteTextSelection(IAnnotation annotation)
	{
		return m_selectionContext.GetSelection<IAnnotation>().Contains(annotation) && base.AdaptedControl.HasKeyboardFocus;
	}

	public bool CanInsertText(IAnnotation annotation)
	{
		return m_selectionContext.GetSelection<IAnnotation>().Contains(annotation) && base.AdaptedControl.HasKeyboardFocus;
	}

	public bool CanCopyText(IAnnotation annotation)
	{
		if (m_selectionContext.GetSelection<IAnnotation>().Contains(annotation) && base.AdaptedControl.Focused)
		{
			string value = TextSelected(annotation);
			return !string.IsNullOrEmpty(value);
		}
		return false;
	}

	public string TextSelected(IAnnotation annotation)
	{
		if (m_annotationEditors.ContainsKey(annotation))
		{
			TextEditor textEditor = m_annotationEditors[annotation];
			return annotation.Text.Substring(textEditor.SelectionStart, textEditor.SelectionLength);
		}
		return string.Empty;
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		if (m_caretCreated && m_selectionContext != null && m_selectionContext.GetSelection<IAnnotation>().FirstOrDefault() == null)
		{
			HideCaret();
		}
	}

	private void HideCaret()
	{
		User32.SetCaretPos(-10, 0);
		base.AdaptedControl.HasKeyboardFocus = false;
	}
}
