using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.AssetEditing;

public class CurveValueEditor : IPropertyEditor
{
	private class CurveValueEditorControl : Control
	{
		private class CurveEditorControl : Control
		{
			private readonly IContextRegistry m_contextRegistry;

			private CurveAdapter _curve;

			private CurveEditingContext _curveEditingContext;

			private bool _bCapturedMouseDown;

			private PointF _oldMouseLocation = PointF.Empty;

			private ToolTip TextTooltip { get; set; }

			private Rectangle CurveCanvasRectangle { get; set; }

			private IThemeService ThemeService { get; }

			public CurveAdapter Curve
			{
				get
				{
					return _curve;
				}
				set
				{
					if (_curve != value)
					{
						if (_curveEditingContext != null)
						{
							_curveEditingContext.Selection.ItemsChanged -= HandleSelectedItemsChanged;
							m_contextRegistry.ActiveContext = (_curveEditingContext = null);
						}
						_curve = value;
						if (_curve != null)
						{
							_curveEditingContext = _curve.As<CurveEditingContext>();
							_curveEditingContext.Selection.ItemsChanged += HandleSelectedItemsChanged;
							m_contextRegistry.ActiveContext = _curveEditingContext;
						}
					}
				}
			}

			public CurveEditorControl(CurveAdapter curve, IContextRegistry contextRegistry, IThemeService themeService)
			{
				SetStyle(ControlStyles.ResizeRedraw, value: true);
				SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
				SetStyle(ControlStyles.UserPaint, value: true);
				SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
				ThemeService = themeService;
				base.Visible = true;
				m_contextRegistry = contextRegistry;
				Curve = curve;
				TextTooltip = new ToolTip();
				TextTooltip.AutoPopDelay = 3000;
				TextTooltip.InitialDelay = 1000;
				TextTooltip.ReshowDelay = 500;
				TextTooltip.ShowAlways = false;
				TextTooltip.SetToolTip(this, "<Loading...>");
				TextTooltip.Popup += TextTooltip_Popup;
			}

			private void TextTooltip_Popup(object sender, PopupEventArgs e)
			{
				Point point = PointToClient(Control.MousePosition);
				PointF pointF = DrawSpaceToCurveSpace(point);
				string caption = pointF.X.ToString("0.000") + ", " + pointF.Y.ToString("0.000");
				TextTooltip.SetToolTip(this, caption);
			}

			protected override void OnSizeChanged(EventArgs e)
			{
				base.OnSizeChanged(e);
				CurveCanvasRectangle = new Rectangle(new Point(30, 0), new Size(base.ClientSize.Width - 35, base.ClientSize.Height - 15));
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				if (_curveEditingContext != null)
				{
					PaintBackground(e);
					_curveEditingContext.Paint(CurveSpaceToDrawSpace, e.Graphics);
					PaintLabels(e.Graphics);
				}
			}

			private void PaintBackground(PaintEventArgs pevent)
			{
				ThemeBase activeTheme = ThemeService.ActiveTheme;
				IPaintingService paintingService = activeTheme.PaintingService;
				SolidBrush brush = paintingService.GetBrush(activeTheme.ColorPalette.MainWindowActive.Background);
				pevent.Graphics.FillRectangle(brush, pevent.ClipRectangle);
				using (Pen pen = new Pen(Color.FromArgb(64, Color.Green)))
				{
					pen.DashStyle = DashStyle.Dash;
					float gridIncrement = GetGridIncrement(_curveEditingContext.MinX, _curveEditingContext.MaxX, CurveCanvasRectangle.Width);
					for (float num = _curveEditingContext.MinX; num < _curveEditingContext.MaxX; num += gridIncrement)
					{
						PointF pt = CurveSpaceToDrawSpace(new PointF(num, _curveEditingContext.MinY));
						PointF pt2 = CurveSpaceToDrawSpace(new PointF(num, _curveEditingContext.MaxY));
						pevent.Graphics.DrawLine(pen, pt, pt2);
					}
					float gridIncrement2 = GetGridIncrement(_curveEditingContext.MinY, _curveEditingContext.MaxY, CurveCanvasRectangle.Height);
					for (float num2 = _curveEditingContext.MinY; num2 < _curveEditingContext.MaxY; num2 += gridIncrement2)
					{
						PointF pt3 = CurveSpaceToDrawSpace(new PointF(_curveEditingContext.MinX, num2));
						PointF pt4 = CurveSpaceToDrawSpace(new PointF(_curveEditingContext.MaxX, num2));
						pevent.Graphics.DrawLine(pen, pt3, pt4);
					}
				}
				Pen pen2 = paintingService.GetPen(activeTheme.ColorPalette.ToolWindowBorder, 2);
				pevent.Graphics.DrawLine(pen2, new Point(CurveCanvasRectangle.Left, CurveCanvasRectangle.Top), new Point(CurveCanvasRectangle.Left, CurveCanvasRectangle.Bottom));
				pevent.Graphics.DrawLine(pen2, new Point(CurveCanvasRectangle.Left, CurveCanvasRectangle.Bottom), new Point(CurveCanvasRectangle.Right, CurveCanvasRectangle.Bottom));
				pevent.Graphics.DrawLine(pen2, new Point(CurveCanvasRectangle.Right - 1, CurveCanvasRectangle.Top), new Point(CurveCanvasRectangle.Right - 1, CurveCanvasRectangle.Bottom));
			}

			private void PaintLabels(Graphics graphics)
			{
				ThemeBase activeTheme = ThemeService.ActiveTheme;
				SolidBrush brush = activeTheme.PaintingService.GetBrush(activeTheme.ColorPalette.ToolWindowTabSelectedActive.Text);
				float minY = _curveEditingContext.MinY;
				float maxY = _curveEditingContext.MaxY;
				PointF pointF = CurveSpaceToDrawSpace(new PointF(0f, minY));
				PointF originalOrigin = CurveSpaceToDrawSpace(new PointF(0.5f, minY));
				PointF originalOrigin2 = CurveSpaceToDrawSpace(new PointF(1f, minY));
				DrawStringCentered(graphics, "0.0", Font, brush, pointF);
				DrawStringCentered(graphics, "0.5", Font, brush, originalOrigin);
				DrawStringLeftAligned(graphics, "1.0", Font, brush, originalOrigin2);
				PointF pointF2 = CurveSpaceToDrawSpace(new PointF(0f, 0f));
				if (pointF2 != pointF)
				{
					pointF2.X -= 3f;
					DrawStringLeftAligned(graphics, "0.0", Font, brush, pointF2);
				}
				if (minY != 0f)
				{
					PointF originalOrigin3 = CurveSpaceToDrawSpace(new PointF(0f, minY));
					string s = minY.ToString("0.0");
					originalOrigin3.X -= graphics.MeasureString(s, Font).Width;
					originalOrigin3.X -= 3f;
					DrawStringLeftAligned(graphics, s, Font, brush, originalOrigin3);
				}
				if (maxY != 0f)
				{
					PointF originalOrigin4 = CurveSpaceToDrawSpace(new PointF(0f, maxY));
					originalOrigin4.X -= 3f;
					DrawStringLeftAligned(graphics, maxY.ToString("0.0"), Font, brush, originalOrigin4);
				}
			}

			private void DrawStringCentered(Graphics graphics, string s, Font font, Brush brush, PointF originalOrigin)
			{
				SizeF sizeF = graphics.MeasureString(s, font);
				graphics.DrawString(s, font, brush, new PointF(originalOrigin.X - sizeF.Width / 2f, originalOrigin.Y));
			}

			private void DrawStringLeftAligned(Graphics graphics, string s, Font font, Brush brush, PointF originalOrigin)
			{
				SizeF sizeF = graphics.MeasureString(s, font);
				graphics.DrawString(s, font, brush, new PointF(originalOrigin.X - sizeF.Width, originalOrigin.Y));
			}

			private PointF CurveSpaceToDrawSpace(PointF point)
			{
				float num = _curveEditingContext.MaxX - _curveEditingContext.MinX;
				float num2 = (point.X - _curveEditingContext.MinX) / num;
				float num3 = _curveEditingContext.MaxY - _curveEditingContext.MinY;
				float num4 = (point.Y - _curveEditingContext.MinY) / num3;
				float num5 = num2 * (float)CurveCanvasRectangle.Width + (float)CurveCanvasRectangle.Left - 1f;
				float num6 = (float)CurveCanvasRectangle.Height - num4 * (float)CurveCanvasRectangle.Height;
				return new PointF(num5, num6);
			}

			private PointF DrawSpaceToCurveSpace(Point point)
			{
				float num = Convert.ToSingle(point.X - CurveCanvasRectangle.Left) / (float)CurveCanvasRectangle.Width;
				float num2 = CurveCanvasRectangle.Bottom;
				float num3 = Convert.ToSingle(((float)point.Y - num2) * -1f / (float)CurveCanvasRectangle.Height);
				num = _curveEditingContext.MinX + (_curveEditingContext.MaxX - _curveEditingContext.MinX) * num;
				num3 = _curveEditingContext.MinY + (_curveEditingContext.MaxY - _curveEditingContext.MinY) * num3;
				return new PointF(num, num3);
			}

			private float GetGridIncrement(float minValue, float maxValue, float displayRange)
			{
				float num = Math.Min(displayRange / 20f, 10f);
				return (maxValue - minValue) / num;
			}

			protected override void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);
				_oldMouseLocation = DrawSpaceToCurveSpace(e.Location);
				EndOrCancelCurrentTransaction();
				UpdateSelection(e.Location);
				if (e.Button == MouseButtons.Right)
				{
					_curveEditingContext.MenuCurvePosition = _oldMouseLocation;
					ShowContextMenu(e.Location);
				}
				else if (e.Button == MouseButtons.Left)
				{
					_bCapturedMouseDown = true;
					base.Capture = true;
				}
			}

			private void UpdateSelection(Point mouseLocation)
			{
				List<CurveControlPointViewModel> list = new List<CurveControlPointViewModel>(GetOverlappedControlPoints(mouseLocation));
				if (KeyHelper.ShiftPressed)
				{
					CurveControlPointViewModel lastSelected = _curveEditingContext.GetLastSelected<CurveControlPointViewModel>();
					if (lastSelected == null)
					{
						list.Clear();
					}
					if (list.Count > 0)
					{
						List<CurveControlPointViewModel> list2 = new List<CurveControlPointViewModel>();
						bool flag = false;
						int num = 0;
						foreach (CurveControlPointViewModel curveControlPoint in _curveEditingContext.CurveControlPoints)
						{
							if (curveControlPoint == lastSelected)
							{
								flag = true;
								list2.Add(curveControlPoint);
								continue;
							}
							if (list.Contains(curveControlPoint))
							{
								num++;
								list2.Add(curveControlPoint);
							}
							if (flag && num == list.Count)
							{
								break;
							}
							if (flag || num > 0)
							{
								list2.Add(curveControlPoint);
							}
						}
						list = list2;
					}
				}
				if (KeyHelper.CtrlPressed)
				{
					HashSet<CurveControlPointViewModel> currentSelection = new HashSet<CurveControlPointViewModel>(_curveEditingContext.GetSelection<CurveControlPointViewModel>());
					if (KeyHelper.ShiftPressed || !list.Any((CurveControlPointViewModel x) => currentSelection.Contains(x)))
					{
						_curveEditingContext.AddRange(list);
					}
					else
					{
						_curveEditingContext.RemoveRange(list);
					}
				}
				else
				{
					_curveEditingContext.SetRange(list);
				}
				if (((ISelectionContext)_curveEditingContext).SelectionCount == 0)
				{
					PointF curveLocation = DrawSpaceToCurveSpace(mouseLocation);
					CurveSegmentDefinitionViewModel curveSegmentDefinitionViewModel = _curveEditingContext.CurveSegmentDefinitions.LastOrDefault((CurveSegmentDefinitionViewModel vm) => vm.CurveSegmentDefinition.StartingPoint <= curveLocation.X);
					if (curveSegmentDefinitionViewModel != null)
					{
						_curveEditingContext.Selection.Set(curveSegmentDefinitionViewModel.CurveSegmentDefinition);
					}
				}
			}

			private void HandleSelectedItemsChanged(object sender, ItemsChangedEventArgs<object> e)
			{
				RectangleF invalidRectangle = default(RectangleF);
				foreach (CurveControlPointViewModel item in e.AddedItems.Union(e.ChangedItems).Union(e.RemovedItems).OfType<CurveControlPointViewModel>())
				{
					PointF pt = CurveSpaceToDrawSpace(item.Location);
					if (!invalidRectangle.Contains(pt))
					{
						invalidRectangle.Inflate(pt.X - invalidRectangle.Right + 5f, pt.Y - invalidRectangle.Bottom + 5f);
					}
				}
				Action action = delegate
				{
					Invalidate(ToRectangle(invalidRectangle));
				};
				if (base.InvokeRequired)
				{
					BeginInvoke(action);
				}
				else
				{
					action();
				}
			}

			private static Rectangle ToRectangle(RectangleF rect)
			{
				return new Rectangle(Convert.ToInt32(rect.Location.X), Convert.ToInt32(rect.Location.Y), Convert.ToInt32(rect.Width), Convert.ToInt32(rect.Height));
			}

			private void ShowContextMenu(Point location)
			{
				IEnumerable<object> commandTags = from cmd in _curveEditingContext.GetValidCommandInfos_ContextMenu()
					orderby cmd.GroupTag.ToString()
					select cmd.CommandTag;
				_curveEditingContext.CommandService.RunContextMenu(commandTags, PointToScreen(location));
			}

			protected override void OnMouseMove(MouseEventArgs e)
			{
				base.OnMouseMove(e);
				if (_bCapturedMouseDown)
				{
					if (!_curveEditingContext.InTransaction)
					{
						_curveEditingContext.Begin("Move Curve Points.");
					}
					if (_curveEditingContext.GetSelection<CurveControlPointViewModel>().Any())
					{
						Cursor = Cursors.SizeAll;
					}
					PointF oldMouseLocation = DrawSpaceToCurveSpace(e.Location);
					PointF curveDelta = new PointF(oldMouseLocation.X - _oldMouseLocation.X, oldMouseLocation.Y - _oldMouseLocation.Y);
					_curveEditingContext.MoveSelectedPoints(curveDelta);
					_oldMouseLocation = oldMouseLocation;
				}
				else if (GetOverlappedControlPoints(e.Location).Any())
				{
					Cursor = Cursors.Hand;
				}
				else
				{
					Cursor = DefaultCursor;
				}
			}

			private IEnumerable<CurveControlPointViewModel> GetOverlappedControlPoints(Point mouseLocation)
			{
				if (mouseLocation.X < 0 || mouseLocation.Y < 0 || _curveEditingContext == null)
				{
					yield break;
				}
				RectangleF hitTestRectangle = new RectangleF(0f, 0f, _curveEditingContext.SelectedControlPointSize.Width, _curveEditingContext.SelectedControlPointSize.Height);
				foreach (CurveControlPointViewModel curveControlPoint in _curveEditingContext.CurveControlPoints)
				{
					PointF location = CurveSpaceToDrawSpace(curveControlPoint.Location);
					hitTestRectangle.Location = location;
					if (hitTestRectangle.Contains(mouseLocation))
					{
						yield return curveControlPoint;
					}
				}
			}

			private CurveSegmentDefinitionViewModel GetOverlappedSegmentDefinition(Point mouseLocation)
			{
				if (mouseLocation.X < 0 || mouseLocation.Y < 0 || _curveEditingContext == null)
				{
					return null;
				}
				PointF curveSpaceLocation = DrawSpaceToCurveSpace(mouseLocation);
				return _curveEditingContext.CurveSegmentDefinitions.FirstOrDefault((CurveSegmentDefinitionViewModel vm) => vm.CurveSegmentDefinition.StartingPoint > curveSpaceLocation.X);
			}

			protected override void OnMouseUp(MouseEventArgs e)
			{
				base.OnMouseUp(e);
				base.Capture = false;
				_bCapturedMouseDown = false;
				EndOrCancelCurrentTransaction();
			}

			private void EndOrCancelCurrentTransaction()
			{
				if (_curveEditingContext.InTransaction)
				{
					if (_curveEditingContext.PendingOperationCount > 0)
					{
						_curveEditingContext.End();
					}
					else
					{
						_curveEditingContext.Cancel();
					}
				}
			}

			protected override void OnMouseEnter(EventArgs e)
			{
				base.OnMouseEnter(e);
				if (m_contextRegistry.ActiveContext != _curveEditingContext)
				{
					m_contextRegistry.ActiveContext = _curveEditingContext;
				}
			}

			protected override void OnMouseLeave(EventArgs e)
			{
				base.OnMouseLeave(e);
				if (m_contextRegistry.ActiveContext == _curveEditingContext)
				{
					object fallbackContext = GetFallbackContext();
					if (fallbackContext != null)
					{
						m_contextRegistry.ActiveContext = fallbackContext;
					}
				}
			}

			private object GetFallbackContext()
			{
				DomNode domNode = _curveEditingContext?.DomNode;
				if (domNode == null)
				{
					return _curveEditingContext;
				}
				EditingContext editingContext = null;
				while (editingContext == null && domNode != null)
				{
					editingContext = domNode.Parent.As<EditingContext>();
					domNode = domNode.Parent;
				}
				return editingContext;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && TextTooltip != null)
				{
					TextTooltip.Popup -= TextTooltip_Popup;
					TextTooltip.Dispose();
					TextTooltip = null;
				}
				base.Dispose(disposing);
			}
		}

		public class CurveEditorControlParent : Control
		{
			private readonly SplitContainer m_editingSplitter;

			private readonly CurveEditorControl m_curveEditor;

			private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_segmentEditor;

			private readonly ISelectionContext m_selectionContext;

			public CurveEditorControlParent(CurveAdapter curve, IContextRegistry contextRegistry, IThemeService themeService)
			{
				SetStyle(ControlStyles.ResizeRedraw, value: true);
				m_selectionContext = curve.As<ISelectionContext>();
				m_selectionContext.SelectionChanged += Context_SelectionChanged;
				m_editingSplitter = new SplitContainer();
				m_editingSplitter.Dock = DockStyle.Fill;
				m_editingSplitter.Orientation = Orientation.Vertical;
				m_editingSplitter.FixedPanel = FixedPanel.Panel2;
				m_curveEditor = new CurveEditorControl(curve, contextRegistry, themeService);
				m_curveEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
				m_curveEditor.Dock = DockStyle.Fill;
				m_editingSplitter.Panel1.Controls.Add(m_curveEditor);
				m_segmentEditor = new Sce.Atf.Controls.PropertyEditing.PropertyGrid(PropertyGridMode.DisplayTooltips | PropertyGridMode.DisableSearchControls | PropertyGridMode.HideResetAllButton);
				m_segmentEditor.Dock = DockStyle.Fill;
				m_segmentEditor.Bind(m_selectionContext.LastSelected);
				m_editingSplitter.Panel2.Controls.Add(m_segmentEditor);
				Dock = DockStyle.Fill;
				base.Visible = true;
				base.Controls.Add(m_editingSplitter);
			}

			private void Context_SelectionChanged(object sender, EventArgs e)
			{
				m_segmentEditor.Bind(m_selectionContext.LastSelected);
			}

			protected override void OnVisibleChanged(EventArgs e)
			{
				base.OnVisibleChanged(e);
				if (base.Visible)
				{
					try
					{
						m_editingSplitter.Panel2MinSize = 200;
					}
					catch (InvalidOperationException)
					{
					}
				}
			}
		}

		private CurveEditorControlParent CurveEditorParent { get; }

		private ICommandService CommandService { get; }

		private PropertyEditorControlContext Context { get; }

		private IContextRegistry ContextRegistry { get; }

		private IThemeService ThemeService { get; }

		public CurveValueEditorControl(PropertyEditorControlContext context, IContextRegistry contextRegistry, ICommandService commandService, IThemeService themeService)
		{
			SetStyle(ControlStyles.ResizeRedraw, value: true);
			Context = context;
			ContextRegistry = contextRegistry;
			CommandService = commandService;
			ThemeService = themeService;
			CurveFieldValueAdapter editingAdapter = GetEditingAdapter();
			CurveAdapter curveAdapter = editingAdapter.CurveAdapter;
			CurveEditingContext curveEditingContext = curveAdapter.As<CurveEditingContext>();
			if (editingAdapter.Parameter is ICurveParameter curveParameter)
			{
				curveEditingContext.ClampDomain = curveParameter.ClampDomain;
				if (curveParameter.ClampDomain)
				{
					curveEditingContext.MinY = curveParameter.DomainMinValue;
					curveEditingContext.MaxY = curveParameter.DomainMaxValue;
				}
			}
			curveEditingContext.ThemeService = ThemeService;
			curveEditingContext.InitializeCommands(CommandService);
			CurveEditorParent = new CurveEditorControlParent(curveAdapter, ContextRegistry, ThemeService);
			base.Controls.Add(CurveEditorParent);
			base.Height = 120;
			base.Visible = true;
		}

		private CurveFieldValueAdapter GetEditingAdapter()
		{
			return Context.GetValue().As<CurveFieldValueAdapter>();
		}
	}

	private IContextRegistry ContextRegistry { get; }

	private ICommandService CommandService { get; }

	private IThemeService ThemeService { get; }

	public CurveValueEditor(IContextRegistry contextRegistry, ICommandService commandService, IThemeService themeService)
	{
		ContextRegistry = contextRegistry;
		CommandService = commandService;
		ThemeService = themeService;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(600f, 60f);
	}

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		return new CurveValueEditorControl(context, ContextRegistry, CommandService, ThemeService);
	}
}
