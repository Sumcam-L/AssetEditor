using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing;

public class CurveCanvas : Cartesian2dCanvas
{
	private class DefaultTransactionContext : ITransactionContext
	{
		public bool InTransaction => false;

		public int PendingOperationCount => 0;

		public void Begin(string transactionName)
		{
		}

		public void Cancel()
		{
		}

		public void End()
		{
		}

		public TransactionSuspensionReceipt SuspendTransactions()
		{
			return null;
		}
	}

	private class DefaultHistoryContext : IHistoryContext
	{
		public bool CanUndo => false;

		public bool CanRedo => false;

		public string UndoDescription => string.Empty;

		public string RedoDescription => string.Empty;

		public bool Dirty
		{
			get
			{
				return false;
			}
			set
			{
				this.DirtyChanged.Raise(this, EventArgs.Empty);
			}
		}

		public event EventHandler DirtyChanged;

		public void Clear()
		{
		}

		public void Undo()
		{
		}

		public void Redo()
		{
		}
	}

	public static class ShortcutKeys
	{
		public static Keys Undo = Keys.Z | Keys.Control;

		public static Keys Redo = Keys.Y | Keys.Control;

		public static Keys Cut = Keys.X | Keys.Control;

		public static Keys Copy = Keys.C | Keys.Control;

		public static Keys Paste = Keys.V | Keys.Control;

		public static Keys Delete = Keys.Delete;

		public static Keys Move = Keys.W;

		public static Keys Scale = Keys.R;

		public static Keys Fit = Keys.F;

		public static Keys FitAll = Keys.A;

		public static Keys PanToOrigin = Keys.C;

		public static Keys Deselect = Keys.Escape;
	}

	private enum MouseEditAction
	{
		None,
		Panning,
		Zooming,
		Select,
		AddToSelection,
		RemoveFromSelection,
		ToggleSelection,
		MoveSelectionAlongX,
		MoveSelectionAlongY,
		MoveSelection,
		AddControlPoint,
		AddControlPointToEmptyCurve,
		InsertPoint,
		PivotScale,
		PivotScaleAlongX,
		PivotScaleAlongY,
		CurveLimitResize
	}

	[Flags]
	private enum CurveLimitSides
	{
		None = 0,
		Left = 1,
		Right = 2,
		Top = 4,
		Bottom = 8
	}

	[Flags]
	private enum MoveAxis
	{
		None = 0,
		X = 1,
		Y = 2
	}

	private enum MouseDownAction
	{
		None,
		SelectionRectangle,
		Pan,
		Zoom,
		FreeMove,
		ConstrainedMove,
		FreeScale,
		ConstrainedScale,
		CurveLimitResize
	}

	private enum CursorType
	{
		Default,
		QuestionMark,
		Selection,
		MoveHz,
		MoveVert,
		Move,
		CantDo,
		AddPoint,
		Pan
	}

	private HashSet<ICurve> m_editSet = new HashSet<ICurve>();

	private PointD m_zoomCenterStart;

	private readonly SortedDictionary<float, CurveLimitSides> m_limitHits = new SortedDictionary<float, CurveLimitSides>();

	private ICurve m_limitHit;

	private CurveLimitSides m_limitSide;

	private const float SnapThreshold = 16f;

	private object m_context;

	private ITransactionContext m_transactionContext = new DefaultTransactionContext();

	private IHistoryContext m_historyContext = new DefaultHistoryContext();

	private readonly ContextMenuStrip m_contextMenu;

	private readonly ToolStripMenuItem m_undoMenuItem;

	private readonly ToolStripMenuItem m_redoMenuItem;

	private readonly ToolStripMenuItem m_deleteMenuItem;

	private int m_visibleCurveCount;

	private IEnumerable<ICurve> m_pickableCurves;

	private Vec2F m_scalePivot;

	private bool m_updateCurveLimits;

	private bool m_autoPointSnap;

	private bool m_autoCurveSnap;

	private bool m_autoSnapToX;

	private bool m_autoSnapToY;

	private readonly HashSet<ICurve> m_curveSet = new HashSet<ICurve>();

	private PointF[] m_originalValues;

	private MoveAxis m_moveAxis = MoveAxis.None;

	private PointF m_selectionClickPoint;

	private MouseDownAction m_mouseDownAction = MouseDownAction.None;

	private readonly Color m_insertLineColor = Color.FromArgb(240, 240, 83);

	private bool m_drawScalePivot;

	private bool m_drawInsertLine;

	private RectangleF m_mouseRect = new RectangleF(0f, 0f, 10f, 10f);

	private Cursor m_activeCursor;

	private readonly Dictionary<CursorType, Cursor> m_cursors = new Dictionary<CursorType, Cursor>();

	private EditModes m_editMode = EditModes.Move;

	private InputModes m_inputMode = InputModes.Basic;

	private readonly Selection<IControlPoint> m_selection = new Selection<IControlPoint>();

	private readonly ReadOnlyCollection<IControlPoint> m_readonlySelection;

	private ICurve[] m_selectedCurves = new ICurve[0];

	private ReadOnlyCollection<ICurve> m_curves = s_emptyCurves;

	private static readonly ReadOnlyCollection<ICurve> s_emptyCurves;

	private readonly CurveRenderer m_renderer;

	private const float MaxAngle = 1.5706218f;

	private const float MinAngle = -1.5706218f;

	private static readonly Bitmap s_noAction;

	private static readonly Pen s_marqueePen;

	public bool Editing { get; private set; }

	public bool OnlyEditSelectedCurves { get; set; }

	public IEnumerable<int> EditableCurves
	{
		set
		{
			m_editSet.Clear();
			if (!OnlyEditSelectedCurves)
			{
				return;
			}
			value?.ForEach(delegate(int index)
			{
				m_editSet.Add(m_curves[index]);
			});
			if (m_editSet.Count > 0)
			{
				foreach (ICurve curf in m_curves)
				{
					if (!m_editSet.Contains(curf))
					{
						RemoveCurveFromSelection(curf);
					}
				}
			}
			Invalidate();
		}
	}

	public bool AllowResizeCurveLimits { get; set; }

	public bool AutoComputeCurveLimitsEnabled { get; set; }

	public bool RestrictedTranslationEnabled { get; set; }

	public ITransactionContext TransactionContext => m_transactionContext;

	public IHistoryContext HistoryContext => m_historyContext;

	public object Context
	{
		get
		{
			return m_context;
		}
		set
		{
			if (value != m_context)
			{
				m_context = value;
				m_historyContext = m_context.As<IHistoryContext>();
				if (m_historyContext == null)
				{
					m_historyContext = new DefaultHistoryContext();
				}
				m_transactionContext = m_context.As<ITransactionContext>();
				if (m_transactionContext == null)
				{
					m_transactionContext = new DefaultTransactionContext();
				}
				m_curves = s_emptyCurves;
				ClearSelection();
				EditableCurves = EmptyEnumerable<int>.Instance;
				Invalidate();
			}
		}
	}

	public bool AutoPointSnap
	{
		get
		{
			return m_autoPointSnap;
		}
		set
		{
			m_autoPointSnap = value;
		}
	}

	public bool AutoCurveSnap
	{
		get
		{
			return m_autoCurveSnap;
		}
		set
		{
			m_autoCurveSnap = value;
		}
	}

	public new ContextMenuStrip ContextMenuStrip => m_contextMenu;

	public bool AutoSnapToX
	{
		get
		{
			return m_autoSnapToX;
		}
		set
		{
			m_autoSnapToX = value;
		}
	}

	public bool AutoSnapToY
	{
		get
		{
			return m_autoSnapToY;
		}
		set
		{
			m_autoSnapToY = value;
		}
	}

	public ReadOnlyCollection<IControlPoint> Selection => m_readonlySelection;

	public ICurve[] SelectedCurves => m_selectedCurves;

	public EditModes EditMode
	{
		get
		{
			return m_editMode;
		}
		set
		{
			if (m_editMode != value)
			{
				m_editMode = value;
				SetCursor(m_editMode);
				this.EditModeChanged(this, EventArgs.Empty);
			}
		}
	}

	public InputModes InputMode
	{
		get
		{
			return m_inputMode;
		}
		set
		{
			m_inputMode = value;
			if (m_inputMode == InputModes.Basic)
			{
				m_activeCursor = m_cursors[CursorType.Selection];
				Cursor = m_activeCursor;
			}
			else if (m_inputMode == InputModes.Advanced)
			{
				SetCursor(m_editMode);
			}
		}
	}

	public ReadOnlyCollection<ICurve> Curves
	{
		get
		{
			return m_curves;
		}
		set
		{
			if (m_curves != value)
			{
				m_curves = value ?? s_emptyCurves;
				ClearSelection();
				UpdateCurveLimits();
				PanToOrigin(invalidate: false);
				this.CurvesChanged(this, EventArgs.Empty);
				EditableCurves = EmptyEnumerable<int>.Instance;
				Invalidate();
			}
		}
	}

	public event EventHandler EditModeChanged = delegate
	{
	};

	public event EventHandler SelectionChanged = delegate
	{
	};

	public event EventHandler CurvesChanged = delegate
	{
	};

	public CurveCanvas()
	{
		OnlyEditSelectedCurves = true;
		m_renderer = new CurveRenderer();
		m_renderer.SetCartesian2dCanvas(this);
		Dock = DockStyle.Fill;
		m_selection.Changed += m_selection_SelectionChanged;
		m_readonlySelection = new ReadOnlyCollection<IControlPoint>(m_selection);
		m_cursors.Add(CursorType.Default, Cursors.Default);
		m_cursors.Add(CursorType.AddPoint, Cursors.Cross);
		m_cursors.Add(CursorType.CantDo, Cursors.No);
		m_cursors.Add(CursorType.MoveHz, Cursors.SizeWE);
		m_cursors.Add(CursorType.MoveVert, Cursors.SizeNS);
		m_cursors.Add(CursorType.Move, Cursors.SizeAll);
		m_cursors.Add(CursorType.Pan, new Cursor(typeof(CurveUtils), "Resources.Panhv.cur"));
		m_cursors.Add(CursorType.Selection, new Cursor(typeof(CurveUtils), "Resources.SelectionCursor.cur"));
		m_cursors.Add(CursorType.QuestionMark, new Cursor(typeof(CurveUtils), "Resources.QuestionMark.cur"));
		Cursor = m_cursors[CursorType.Selection];
		m_activeCursor = Cursor;
		m_contextMenu = new ContextMenuStrip();
		m_contextMenu.AutoClose = true;
		m_contextMenu.Opening += menustrip_Opening;
		m_undoMenuItem = new ToolStripMenuItem("Undo".Localize());
		m_undoMenuItem.Click += delegate
		{
			Undo();
		};
		m_undoMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Undo, digitOnly: true);
		m_redoMenuItem = new ToolStripMenuItem("Redo".Localize());
		m_redoMenuItem.Click += delegate
		{
			Redo();
		};
		m_redoMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Redo, digitOnly: true);
		m_deleteMenuItem = new ToolStripMenuItem("Delete".Localize());
		m_deleteMenuItem.Click += delegate
		{
			Delete();
		};
		m_deleteMenuItem.ShortcutKeyDisplayString = KeysUtil.KeysToString(ShortcutKeys.Delete, digitOnly: true);
		m_contextMenu.Items.Add(m_undoMenuItem);
		m_contextMenu.Items.Add(m_redoMenuItem);
		m_contextMenu.Items.Add(new ToolStripSeparator());
		m_contextMenu.Items.AddRange(new ToolStripItem[1] { m_deleteMenuItem });
	}

	static CurveCanvas()
	{
		s_emptyCurves = new List<ICurve>().AsReadOnly();
		s_marqueePen = new Pen(Color.FromArgb(40, 40, 40));
		s_marqueePen.DashPattern = new float[2] { 3f, 3f };
		s_noAction = new Bitmap(typeof(CurveUtils), "Resources.NoAction.png");
	}

	public void RemoveCurveFromSelection(ICurve curve)
	{
		if (!m_curves.Contains(curve))
		{
			throw new ArgumentException("curve not found");
		}
		if (m_selection.Count <= 0)
		{
			return;
		}
		m_selection.BeginUpdate();
		IControlPoint[] snapshot = m_selection.GetSnapshot();
		IControlPoint[] array = snapshot;
		foreach (IControlPoint controlPoint in array)
		{
			if (controlPoint.Parent == curve)
			{
				m_selection.Remove(controlPoint);
				controlPoint.EditorData.SelectedRegion = PointSelectionRegions.None;
			}
		}
		m_selection.EndUpdate();
	}

	public void Undo()
	{
		if (m_historyContext.CanUndo)
		{
			m_historyContext.Undo();
			UpdateCurveLimits();
			Invalidate();
		}
	}

	public void Redo()
	{
		if (m_historyContext.CanRedo)
		{
			m_historyContext.Redo();
			UpdateCurveLimits();
			Invalidate();
		}
	}

	public void Delete()
	{
		if (m_selection.Count <= 0)
		{
			return;
		}
		m_transactionContext.DoTransaction(delegate
		{
			foreach (IControlPoint item in m_selection)
			{
				ICurve curve = item.Parent;
				curve.RemoveControlPoint(item);
			}
			ICurve[] selectedCurves = m_selectedCurves;
			foreach (ICurve curve2 in selectedCurves)
			{
				CurveUtils.ComputeTangent(curve2);
			}
		}, "Delete".Localize());
		ClearSelection();
		UpdateCurveLimits();
		Invalidate();
	}

	public void SetTangent(TangentSelection selectedTan, CurveTangentTypes tanType)
	{
		if (m_selection.Count == 0 || selectedTan == TangentSelection.None)
		{
			return;
		}
		switch (selectedTan)
		{
		case TangentSelection.TangentIn:
			if (tanType == CurveTangentTypes.Stepped || tanType == CurveTangentTypes.SteppedNext)
			{
				break;
			}
			m_transactionContext.DoTransaction(delegate
			{
				foreach (IControlPoint item in m_selection)
				{
					if (item.Parent.CurveInterpolation != InterpolationTypes.Linear && (item.EditorData.SelectedRegion == PointSelectionRegions.Point || item.EditorData.SelectedRegion == PointSelectionRegions.TangentIn))
					{
						item.TangentInType = tanType;
					}
				}
				ReComputeTangents();
			}, "Edit Tangent".Localize());
			break;
		case TangentSelection.TangentOut:
			m_transactionContext.DoTransaction(delegate
			{
				foreach (IControlPoint item2 in m_selection)
				{
					if (item2.Parent.CurveInterpolation != InterpolationTypes.Linear && (item2.EditorData.SelectedRegion == PointSelectionRegions.Point || item2.EditorData.SelectedRegion == PointSelectionRegions.TangentOut))
					{
						item2.TangentOutType = tanType;
					}
				}
				ReComputeTangents();
			}, "Edit Tangent".Localize());
			break;
		case TangentSelection.TangentInOut:
			m_transactionContext.DoTransaction(delegate
			{
				foreach (IControlPoint item3 in m_selection)
				{
					if (item3.Parent.CurveInterpolation != InterpolationTypes.Linear)
					{
						if (item3.EditorData.SelectedRegion == PointSelectionRegions.Point)
						{
							if (tanType != CurveTangentTypes.Stepped && tanType != CurveTangentTypes.SteppedNext)
							{
								item3.TangentInType = tanType;
							}
							item3.TangentOutType = tanType;
						}
						else if (item3.EditorData.SelectedRegion == PointSelectionRegions.TangentIn)
						{
							if (tanType != CurveTangentTypes.Stepped && tanType != CurveTangentTypes.SteppedNext)
							{
								item3.TangentInType = tanType;
							}
						}
						else if (item3.EditorData.SelectedRegion == PointSelectionRegions.TangentOut)
						{
							item3.TangentOutType = tanType;
						}
					}
				}
				ReComputeTangents();
			}, "Edit Tangent".Localize());
			break;
		}
		Invalidate();
	}

	public void BreakTangents(bool breaktan)
	{
		if (m_selection.Count == 0)
		{
			return;
		}
		string transactionName = (breaktan ? "Break Tangents".Localize() : "Unify Tangents".Localize());
		m_transactionContext.DoTransaction(delegate
		{
			foreach (IControlPoint item in m_selection)
			{
				item.BrokenTangents = breaktan;
			}
		}, transactionName);
		Invalidate();
	}

	public void SetPreInfinity(CurveLoopTypes infType)
	{
		if (m_editSet.Count == 0 && m_selectedCurves.Length == 0)
		{
			return;
		}
		IEnumerable<ICurve> enumerable;
		if (m_editSet.Count <= 0)
		{
			IEnumerable<ICurve> selectedCurves = m_selectedCurves;
			enumerable = selectedCurves;
		}
		else
		{
			IEnumerable<ICurve> selectedCurves = m_editSet;
			enumerable = selectedCurves;
		}
		IEnumerable<ICurve> curves = enumerable;
		m_transactionContext.DoTransaction(delegate
		{
			foreach (ICurve item in curves)
			{
				item.PreInfinity = infType;
			}
		}, "Edit Pre-Infinity".Localize());
		Invalidate();
	}

	public void SetPostInfinity(CurveLoopTypes infType)
	{
		if (m_editSet.Count == 0 && m_selectedCurves.Length == 0)
		{
			return;
		}
		IEnumerable<ICurve> enumerable;
		if (m_editSet.Count <= 0)
		{
			IEnumerable<ICurve> selectedCurves = m_selectedCurves;
			enumerable = selectedCurves;
		}
		else
		{
			IEnumerable<ICurve> selectedCurves = m_editSet;
			enumerable = selectedCurves;
		}
		IEnumerable<ICurve> curves = enumerable;
		m_transactionContext.DoTransaction(delegate
		{
			foreach (ICurve item in curves)
			{
				item.PostInfinity = infType;
			}
		}, "Edit Post-Infinity".Localize());
		Invalidate();
	}

	public void FitSelection()
	{
		RectangleF empty = RectangleF.Empty;
		if (m_selection.Count == 0)
		{
			return;
		}
		List<IControlPoint> list = new List<IControlPoint>(m_selection.Count);
		list.AddRange(m_selection.GetSnapshot());
		if (list.Count == 1)
		{
			ICurve curve = list[0].Parent;
			ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
			int num = curve.ControlPoints.IndexOf(list[0]);
			int num2 = controlPoints.Count - 1;
			if (num < num2)
			{
				list.Add(controlPoints[num + 1]);
			}
			if (num > 0)
			{
				list.Add(controlPoints[num - 1]);
			}
		}
		empty = ComputeBound(list.AsReadOnly());
		float num3 = 0.1f;
		if (empty != RectangleF.Empty)
		{
			empty.Inflate(empty.Width * num3, empty.Height * num3);
			Frame(empty);
			Invalidate();
		}
	}

	public void Fit()
	{
		if (m_selection.Count > 0)
		{
			FitSelection();
		}
		else
		{
			FitAll();
		}
	}

	public void FitAll()
	{
		if (m_curves.Count <= 0)
		{
			return;
		}
		RectangleF rectangleF = RectangleF.Empty;
		foreach (ICurve curf in m_curves)
		{
			if (curf.Visible)
			{
				RectangleF rectangleF2 = ComputeBound(curf.ControlPoints);
				if (rectangleF2 != RectangleF.Empty)
				{
					rectangleF = RectangleF.Union(rectangleF, rectangleF2);
				}
			}
		}
		float num = 0.1f;
		if (rectangleF != RectangleF.Empty)
		{
			rectangleF.Inflate(rectangleF.Width * num, rectangleF.Height * num);
			Frame(rectangleF);
			Invalidate();
		}
	}

	public void SnapToMajorTick()
	{
		if (m_selection.Count == 0)
		{
			return;
		}
		float majorTick = base.MajorTickX;
		m_transactionContext.DoTransaction(delegate
		{
			foreach (IControlPoint item in m_selection)
			{
				SnapToX(item, majorTick);
			}
		}, "Snap".Localize());
		UpdateCurveLimits();
		Invalidate();
	}

	public void SnapToMinorTick()
	{
		if (m_selection.Count == 0)
		{
			return;
		}
		float tick = base.MajorTickX / base.NumOfMinorTicks;
		m_transactionContext.DoTransaction(delegate
		{
			foreach (IControlPoint item in m_selection)
			{
				SnapToX(item, tick);
			}
		}, "Snap".Localize());
		UpdateCurveLimits();
		Invalidate();
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (keyData == (Keys.Menu | Keys.Alt))
		{
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		bool flag = true;
		if (keyData == ShortcutKeys.Undo)
		{
			Undo();
		}
		else if (keyData == ShortcutKeys.Redo)
		{
			Redo();
		}
		else if (keyData == ShortcutKeys.Fit)
		{
			Fit();
		}
		else if (keyData == ShortcutKeys.FitAll)
		{
			FitAll();
		}
		else if (keyData == ShortcutKeys.PanToOrigin)
		{
			PanToOrigin();
		}
		else if (keyData == ShortcutKeys.Scale)
		{
			EditMode = EditModes.Scale;
		}
		else if (keyData == ShortcutKeys.Delete)
		{
			Delete();
		}
		else if (keyData == ShortcutKeys.Move)
		{
			EditMode = EditModes.Move;
		}
		else if (keyData == ShortcutKeys.Deselect)
		{
			ClearSelection();
		}
		else
		{
			flag = keyData == ShortcutKeys.Cut || keyData == ShortcutKeys.Copy || keyData == ShortcutKeys.Paste;
		}
		if (!flag)
		{
			flag = base.ProcessCmdKey(ref msg, keyData);
		}
		else
		{
			Invalidate();
		}
		return flag;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.Parent.Focus();
		base.OnMouseDown(e);
		m_mouseDownAction = MouseDownAction.None;
		MouseEditAction action = MouseEditAction.None;
		m_moveAxis = MoveAxis.None;
		m_selectionClickPoint = ClickPoint;
		m_originalValues = null;
		m_scalePivot = ClickGraphPoint;
		m_limitHit = null;
		m_zoomCenterStart = new PointD(((double)ClickPoint.X - base.Pan_d.X) / base.Zoom_d.X, ((double)ClickPoint.Y - base.Pan_d.Y) / base.Zoom_d.Y);
		m_updateCurveLimits = false;
		m_visibleCurveCount = m_curves.Count((ICurve c) => c.Visible);
		m_pickableCurves = m_curves.Where((ICurve c) => c.Visible && (m_visibleCurveCount == 1 || !OnlyEditSelectedCurves || m_editSet.Count == 0 || m_editSet.Contains(c))).Reverse();
		if (m_autoSnapToX)
		{
			m_scalePivot.X = CurveUtils.SnapTo(m_scalePivot.X, base.MajorTickX);
		}
		if (m_autoSnapToY)
		{
			m_scalePivot.Y = CurveUtils.SnapTo(m_scalePivot.Y, base.MajorTickY);
		}
		if (m_inputMode == InputModes.Basic)
		{
			action = BasicOnMouseDown(e);
		}
		else if (m_inputMode == InputModes.Advanced)
		{
			action = AdvancedOnMouseDown(e);
		}
		PerformAction(action);
		Invalidate();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (base.DraggingOverThreshold)
		{
			MouseEditAction mouseEditAction = MouseEditAction.None;
			if (m_mouseDownAction == MouseDownAction.Pan)
			{
				mouseEditAction = MouseEditAction.Panning;
			}
			else if (m_mouseDownAction == MouseDownAction.Zoom)
			{
				mouseEditAction = MouseEditAction.Zooming;
			}
			else if (m_mouseDownAction == MouseDownAction.FreeMove)
			{
				mouseEditAction = MouseEditAction.MoveSelection;
			}
			else if (m_mouseDownAction == MouseDownAction.CurveLimitResize)
			{
				mouseEditAction = MouseEditAction.CurveLimitResize;
			}
			else if (m_inputMode == InputModes.Basic)
			{
				mouseEditAction = BasicOnMouseMove(e);
			}
			else if (m_inputMode == InputModes.Advanced)
			{
				mouseEditAction = AdvancedOnMouseMove(e);
			}
			if (!m_transactionContext.InTransaction)
			{
				if (m_mouseDownAction == MouseDownAction.FreeMove || m_mouseDownAction == MouseDownAction.ConstrainedMove)
				{
					m_transactionContext.Begin("Move".Localize());
				}
				else if (m_mouseDownAction == MouseDownAction.FreeScale || m_mouseDownAction == MouseDownAction.ConstrainedScale)
				{
					m_transactionContext.Begin("Scale".Localize());
				}
				else if (m_mouseDownAction == MouseDownAction.CurveLimitResize)
				{
					m_transactionContext.Begin("Resize Curve Limit".Localize("could be phrased as 'resize the boundary of the curve'"));
				}
			}
			PerformAction(mouseEditAction);
			if (mouseEditAction != MouseEditAction.None || m_drawInsertLine)
			{
				Invalidate();
			}
		}
		else if (e.Button == MouseButtons.None && m_curves.Count > 0)
		{
			if (PickCurveLimits(out m_limitSide) != null)
			{
				Cursor = ((m_limitSide == CurveLimitSides.Left || m_limitSide == CurveLimitSides.Right) ? m_cursors[CursorType.MoveHz] : m_cursors[CursorType.MoveVert]);
			}
			else if (Cursor != m_activeCursor)
			{
				Cursor = m_activeCursor;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (m_transactionContext.InTransaction)
		{
			m_transactionContext.End();
		}
		MouseEditAction action = MouseEditAction.None;
		if (m_inputMode == InputModes.Basic)
		{
			action = BasicOnMouseUp(e);
		}
		else if (m_inputMode == InputModes.Advanced)
		{
			action = AdvancedOnMouseUp(e);
		}
		if (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.None)
		{
			m_contextMenu.Show(this, e.X, e.Y);
		}
		PerformAction(action);
		Cursor = m_activeCursor;
		if (m_updateCurveLimits)
		{
			UpdateCurveLimits();
		}
		m_updateCurveLimits = false;
		m_drawInsertLine = false;
		m_drawScalePivot = false;
		m_mouseDownAction = MouseDownAction.None;
		base.OnMouseUp(e);
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		DrawCartesianGrid(e.Graphics);
		if (m_curves.Count > 0)
		{
			float num = base.ClientSize.Width;
			float num2 = base.ClientSize.Height;
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			for (int num3 = m_selection.Count - 1; num3 >= 0; num3--)
			{
				ICurve curve = m_selection[num3].Parent;
				if (curve != null && curve.Visible)
				{
					DrawXYLabel(e.Graphics, curve.XLabel, curve.YLabel, curve.CurveColor);
					break;
				}
			}
			foreach (ICurve curf in m_curves)
			{
				if (curf.Visible)
				{
					float thickness = (m_editSet.Contains(curf) ? 2.6f : 1f);
					m_renderer.DrawCurve(curf, e.Graphics, thickness);
				}
			}
			e.Graphics.SmoothingMode = SmoothingMode.None;
			foreach (ICurve curf2 in m_curves)
			{
				if (curf2.Visible)
				{
					m_renderer.DrawControlPoints(curf2, e.Graphics);
				}
			}
			Pen pen = new Pen(Color.Black, 2f);
			ICurve[] selectedCurves = m_selectedCurves;
			foreach (ICurve curve2 in selectedCurves)
			{
				if (!curve2.Visible)
				{
					continue;
				}
				Vec2F p = new Vec2F(curve2.MinX, curve2.MinY);
				Vec2F p2 = new Vec2F(curve2.MaxX, curve2.MaxY);
				p = GraphToClient(p);
				p2 = GraphToClient(p2);
				float num4 = p.X;
				float num5 = p2.X;
				float num6 = p.Y;
				float num7 = p2.Y;
				pen.Color = curve2.CurveColor;
				if ((!(num4 < 0f) || !(num5 < 0f)) && (!(num4 > num) || !(num5 > num)))
				{
					float x = Math.Max(0f, num4);
					float x2 = Math.Min(num, num5);
					if (num7 > 0f && num7 < num2)
					{
						e.Graphics.DrawLine(pen, x, num7, x2, num7);
					}
					if (num6 > 0f && num6 < num2)
					{
						e.Graphics.DrawLine(pen, x, num6, x2, num6);
					}
				}
				if ((!(num6 < 0f) || !(num7 < 0f)) && (!(num6 > num2) || !(num7 > num2)))
				{
					float y = Math.Min(num2, num6);
					float y2 = Math.Max(0f, num7);
					if (num4 > 0f && num4 < num)
					{
						e.Graphics.DrawLine(pen, num4, y2, num4, y);
					}
					if (num5 > 0f && num5 < num)
					{
						e.Graphics.DrawLine(pen, num5, y2, num5, y);
					}
				}
			}
			if (m_drawScalePivot)
			{
				Vec2F vec2F = GraphToClient(m_scalePivot);
				Rectangle rect = new Rectangle((int)vec2F.X - 4, (int)vec2F.Y - 4, 8, 8);
				e.Graphics.DrawLine(Pens.Black, 0f, vec2F.Y, base.Width, vec2F.Y);
				e.Graphics.DrawLine(Pens.Black, vec2F.X, 0f, vec2F.X, base.Height);
				e.Graphics.DrawRectangle(Pens.Black, rect);
			}
			else if (m_drawInsertLine)
			{
				using Pen pen2 = new Pen(m_insertLineColor);
				using SolidBrush brush = new SolidBrush(m_insertLineColor);
				e.Graphics.DrawLine(pen2, CurrentPoint.X, 0f, CurrentPoint.X, base.Height);
				IEnumerable<ICurve> enumerable;
				if (m_editSet.Count <= 0)
				{
					IEnumerable<ICurve> selectedCurves2 = m_selectedCurves;
					enumerable = selectedCurves2;
				}
				else
				{
					IEnumerable<ICurve> selectedCurves2 = m_editSet;
					enumerable = selectedCurves2;
				}
				IEnumerable<ICurve> enumerable2 = enumerable;
				foreach (ICurve item in enumerable2)
				{
					ICurveEvaluator curveEvaluator = CurveUtils.CreateCurveEvaluator(item);
					float num8 = curveEvaluator.Evaluate(CurrentGraphPoint.X);
					Vec2F p3 = new Vec2F(CurrentGraphPoint.X, num8);
					Vec2F vec2F2 = GraphToClient(p3);
					RectangleF mouseRect = m_mouseRect;
					mouseRect.X = vec2F2.X - mouseRect.Width / 2f;
					mouseRect.Y = vec2F2.Y - mouseRect.Height / 2f;
					bool flag = AutoComputeCurveLimitsEnabled || (p3.X >= item.MinX && p3.X <= item.MaxX && p3.Y >= item.MinY && p3.Y <= item.MaxY);
					int validInsertionIndex = CurveUtils.GetValidInsertionIndex(item, CurrentGraphPoint.X);
					if (validInsertionIndex >= 0 && flag)
					{
						e.Graphics.FillRectangle(brush, mouseRect);
						continue;
					}
					vec2F2.X -= s_noAction.Width / 2;
					vec2F2.Y -= s_noAction.Height / 2;
					e.Graphics.DrawImage(s_noAction, vec2F2);
				}
			}
			pen.Dispose();
		}
		if (SelectionRect != RectangleF.Empty)
		{
			e.Graphics.DrawRectangle(s_marqueePen, Rectangle.Truncate(SelectionRect));
		}
	}

	private MouseEditAction AdvancedOnMouseDown(MouseEventArgs e)
	{
		MouseEditAction result = MouseEditAction.None;
		bool flag = false;
		if (e.Button == MouseButtons.Left)
		{
			m_limitHit = PickCurveLimits(out m_limitSide);
			m_mouseDownAction = ((m_limitHit == null) ? MouseDownAction.SelectionRectangle : MouseDownAction.CurveLimitResize);
			if (m_limitSide != CurveLimitSides.None)
			{
				Cursor = ((m_limitSide == CurveLimitSides.Left || m_limitSide == CurveLimitSides.Right) ? m_cursors[CursorType.MoveHz] : m_cursors[CursorType.MoveVert]);
			}
		}
		else if ((e.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.None) || (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.Control))
		{
			if (m_editSet.Count > 0 || m_selection.Count > 0 || m_visibleCurveCount == 1)
			{
				if (m_editMode == EditModes.AddPoint)
				{
					result = MouseEditAction.AddControlPoint;
				}
				else if (m_editMode == EditModes.InsertPoint)
				{
					m_drawInsertLine = true;
				}
				else if (m_selection.Count > 0)
				{
					if (m_editMode == EditModes.Move)
					{
						Cursor = m_cursors[CursorType.Move];
						m_mouseDownAction = MouseDownAction.FreeMove;
						flag = true;
					}
					else if (m_editMode == EditModes.Scale)
					{
						Cursor = m_cursors[CursorType.Move];
						m_mouseDownAction = MouseDownAction.FreeScale;
						m_drawScalePivot = true;
						flag = true;
					}
				}
			}
			else if (m_curves.Count > 0 && m_editMode == EditModes.AddPoint)
			{
				result = MouseEditAction.AddControlPointToEmptyCurve;
			}
		}
		else if ((e.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.Shift) || (e.Button == MouseButtons.Right && Control.ModifierKeys == (Keys.Shift | Keys.Control)))
		{
			if (m_selection.Count > 0)
			{
				if (m_editMode == EditModes.Move)
				{
					Cursor = m_cursors[CursorType.QuestionMark];
					m_mouseDownAction = MouseDownAction.ConstrainedMove;
					flag = true;
				}
				else if (m_editMode == EditModes.Scale)
				{
					Cursor = m_cursors[CursorType.QuestionMark];
					m_mouseDownAction = MouseDownAction.ConstrainedScale;
					m_drawScalePivot = true;
					flag = true;
				}
			}
		}
		else if ((e.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.Alt) || (e.Button == MouseButtons.Right && Control.ModifierKeys == (Keys.Control | Keys.Alt)))
		{
			m_mouseDownAction = MouseDownAction.Pan;
			Cursor = m_cursors[CursorType.Pan];
		}
		else if (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.Alt)
		{
			m_mouseDownAction = MouseDownAction.Zoom;
		}
		if (flag)
		{
			m_originalValues = new PointF[m_selection.Count];
			for (int i = 0; i < m_selection.Count; i++)
			{
				IControlPoint controlPoint = m_selection[i];
				m_originalValues[i] = new PointF(controlPoint.X, controlPoint.Y);
			}
		}
		return result;
	}

	private MouseEditAction AdvancedOnMouseMove(MouseEventArgs e)
	{
		MouseEditAction result = MouseEditAction.None;
		if (m_mouseDownAction == MouseDownAction.SelectionRectangle)
		{
			if (Control.ModifierKeys == Keys.Alt)
			{
				m_selectionClickPoint.X += CurrentPoint.X - PreviousPoint.X;
				m_selectionClickPoint.Y += CurrentPoint.Y - PreviousPoint.Y;
			}
			SelectionRect = MakeRect(m_selectionClickPoint, CurrentPoint);
			Invalidate();
		}
		else if (m_mouseDownAction == MouseDownAction.ConstrainedMove)
		{
			if (m_moveAxis == MoveAxis.None)
			{
				float value = CurrentPoint.X - ClickPoint.X;
				float value2 = CurrentPoint.Y - ClickPoint.Y;
				m_moveAxis = ((Math.Abs(value) > Math.Abs(value2)) ? MoveAxis.X : MoveAxis.Y);
				if (m_moveAxis == MoveAxis.X)
				{
					Cursor = m_cursors[CursorType.MoveHz];
				}
				else
				{
					Cursor = m_cursors[CursorType.MoveVert];
				}
			}
			result = ((m_moveAxis == MoveAxis.X) ? MouseEditAction.MoveSelectionAlongX : MouseEditAction.MoveSelectionAlongY);
		}
		else if (m_mouseDownAction == MouseDownAction.FreeScale)
		{
			result = MouseEditAction.PivotScale;
		}
		else if (m_mouseDownAction == MouseDownAction.ConstrainedScale)
		{
			if (m_moveAxis == MoveAxis.None)
			{
				float value3 = CurrentPoint.X - ClickPoint.X;
				float value4 = CurrentPoint.Y - ClickPoint.Y;
				m_moveAxis = ((Math.Abs(value3) > Math.Abs(value4)) ? MoveAxis.X : MoveAxis.Y);
				if (m_moveAxis == MoveAxis.X)
				{
					Cursor = m_cursors[CursorType.MoveHz];
				}
				else
				{
					Cursor = m_cursors[CursorType.MoveVert];
				}
			}
			result = ((m_moveAxis == MoveAxis.X) ? MouseEditAction.PivotScaleAlongX : MouseEditAction.PivotScaleAlongY);
		}
		return result;
	}

	private MouseEditAction AdvancedOnMouseUp(MouseEventArgs e)
	{
		MouseEditAction result = MouseEditAction.None;
		if (e.Button == MouseButtons.Left && m_limitHit == null)
		{
			if (Control.ModifierKeys == Keys.Control)
			{
				result = MouseEditAction.RemoveFromSelection;
			}
			else if (Control.ModifierKeys == Keys.Shift)
			{
				result = MouseEditAction.ToggleSelection;
			}
			else if ((Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
			{
				result = MouseEditAction.AddToSelection;
			}
			else if (Control.ModifierKeys == Keys.None || Control.ModifierKeys == Keys.Alt)
			{
				result = MouseEditAction.Select;
			}
		}
		else if (((e.Button == MouseButtons.Middle && Control.ModifierKeys == Keys.None) || (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.Control)) && (m_editSet.Count > 0 || m_selection.Count > 0 || m_visibleCurveCount == 1) && EditMode == EditModes.InsertPoint)
		{
			result = MouseEditAction.InsertPoint;
		}
		return result;
	}

	private MouseEditAction BasicOnMouseDown(MouseEventArgs e)
	{
		MouseEditAction result = MouseEditAction.None;
		bool flag = false;
		if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)
		{
			if (Control.ModifierKeys == Keys.None && e.Button == MouseButtons.Left)
			{
				m_limitHit = PickCurveLimits(out m_limitSide);
				if (m_limitHit != null)
				{
					m_mouseDownAction = MouseDownAction.CurveLimitResize;
					Cursor = ((m_limitSide == CurveLimitSides.Left || m_limitSide == CurveLimitSides.Right) ? m_cursors[CursorType.MoveHz] : m_cursors[CursorType.MoveVert]);
				}
				else
				{
					List<IControlPoint> list = new List<IControlPoint>();
					List<PointSelectionRegions> list2 = new List<PointSelectionRegions>();
					RectangleF mouseRect = m_mouseRect;
					mouseRect.X = CurrentPoint.X - m_mouseRect.Width / 2f;
					mouseRect.Y = CurrentPoint.Y - m_mouseRect.Height / 2f;
					m_renderer.PickPoints(m_pickableCurves, mouseRect, list, list2, singlePick: true);
					IControlPoint controlPoint = ((list.Count > 0) ? list[0] : null);
					PointSelectionRegions pointSelectionRegions = ((list2.Count > 0) ? list2[0] : PointSelectionRegions.None);
					if (controlPoint != null)
					{
						if (!m_selection.Contains(controlPoint))
						{
							SetSelection(list, list2);
						}
						else if (controlPoint.EditorData.SelectedRegion != pointSelectionRegions)
						{
							SetSelection(list, list2);
						}
						m_mouseDownAction = MouseDownAction.FreeMove;
						flag = true;
					}
					else
					{
						m_mouseDownAction = MouseDownAction.SelectionRectangle;
					}
				}
			}
			else if (Control.ModifierKeys == Keys.Shift)
			{
				if (m_editSet.Count > 0 || m_selection.Count > 0 || m_visibleCurveCount == 1)
				{
					result = MouseEditAction.AddControlPoint;
				}
				else if (m_curves.Count > 0)
				{
					result = MouseEditAction.AddControlPointToEmptyCurve;
				}
			}
			else if (Control.ModifierKeys == Keys.Alt)
			{
				m_mouseDownAction = MouseDownAction.Pan;
				Cursor = m_cursors[CursorType.Pan];
			}
		}
		else if (e.Button == MouseButtons.Right && Control.ModifierKeys == Keys.Alt)
		{
			m_mouseDownAction = MouseDownAction.Zoom;
		}
		if (flag)
		{
			m_originalValues = new PointF[m_selection.Count];
			for (int i = 0; i < m_selection.Count; i++)
			{
				IControlPoint controlPoint2 = m_selection[i];
				m_originalValues[i] = new PointF(controlPoint2.X, controlPoint2.Y);
			}
		}
		return result;
	}

	private MouseEditAction BasicOnMouseMove(MouseEventArgs e)
	{
		MouseEditAction result = MouseEditAction.None;
		if (m_mouseDownAction == MouseDownAction.SelectionRectangle)
		{
			SelectionRect = MakeRect(m_selectionClickPoint, CurrentPoint);
			Invalidate();
		}
		return result;
	}

	private MouseEditAction BasicOnMouseUp(MouseEventArgs e)
	{
		MouseEditAction result = MouseEditAction.None;
		if (e.Button == MouseButtons.Left && m_limitHit == null)
		{
			if (Control.ModifierKeys == Keys.Control)
			{
				result = MouseEditAction.ToggleSelection;
			}
			else if (m_mouseDownAction == MouseDownAction.SelectionRectangle)
			{
				result = MouseEditAction.Select;
			}
		}
		return result;
	}

	private void SetCursor(EditModes mode)
	{
		switch (mode)
		{
		case EditModes.Move:
		case EditModes.Scale:
			m_activeCursor = m_cursors[CursorType.Selection];
			break;
		case EditModes.AddPoint:
		case EditModes.InsertPoint:
			m_activeCursor = m_cursors[CursorType.AddPoint];
			break;
		default:
			m_activeCursor = m_cursors[CursorType.Default];
			break;
		}
		Cursor = m_activeCursor;
	}

	private void menustrip_Opening(object sender, CancelEventArgs e)
	{
		bool enabled = m_selection.Count > 0;
		ContextMenuStrip contextMenuStrip = sender as ContextMenuStrip;
		foreach (ToolStripItem item in contextMenuStrip.Items)
		{
			item.Enabled = enabled;
		}
		m_undoMenuItem.Enabled = m_historyContext.CanUndo;
		m_undoMenuItem.Text = "Undo".Localize() + " " + m_historyContext.UndoDescription;
		m_redoMenuItem.Enabled = m_historyContext.CanRedo;
		m_redoMenuItem.Text = "Redo".Localize() + " " + m_historyContext.RedoDescription;
	}

	private RectangleF ComputeBound(ReadOnlyCollection<IControlPoint> points)
	{
		RectangleF rectangleF = RectangleF.Empty;
		if (points.Count > 0)
		{
			IControlPoint controlPoint = points[0];
			rectangleF = new RectangleF(controlPoint.X, controlPoint.Y, 0f, 0f);
			for (int i = 1; i < points.Count; i++)
			{
				controlPoint = points[i];
				rectangleF = RectangleF.Union(b: new RectangleF(controlPoint.X, controlPoint.Y, 0f, 0f), a: rectangleF);
			}
			if (rectangleF.Width == 0f)
			{
				rectangleF.Width = float.Epsilon;
			}
			if (rectangleF.Height == 0f)
			{
				rectangleF.Height = float.Epsilon;
			}
		}
		return rectangleF;
	}

	private void PerformAction(MouseEditAction action)
	{
		float num = CurrentPoint.X - ClickPoint.X;
		float num2 = CurrentPoint.Y - ClickPoint.Y;
		List<IControlPoint> points = new List<IControlPoint>();
		List<PointSelectionRegions> regions = new List<PointSelectionRegions>();
		RectangleF empty = RectangleF.Empty;
		bool singlePick = !base.DraggingOverThreshold;
		if (base.DraggingOverThreshold)
		{
			empty = SelectionRect;
		}
		else
		{
			empty = m_mouseRect;
			empty.X = CurrentPoint.X - m_mouseRect.Width / 2f;
			empty.Y = CurrentPoint.Y - m_mouseRect.Height / 2f;
		}
		switch (action)
		{
		case MouseEditAction.Panning:
			base.Pan_d = new PointD(ClickPan_d.X + (double)num, ClickPan_d.Y + (double)num2);
			break;
		case MouseEditAction.Zooming:
		{
			float num3 = 1f + 4f * Math.Abs(num / (float)base.Width);
			if (num < 0f)
			{
				num3 = 1f / num3;
			}
			float num4 = 1f + 4f * Math.Abs(num2 / (float)base.Height);
			if (num2 > 0f)
			{
				num4 = 1f / num4;
			}
			base.Zoom_d = new PointD(ClickZoom_d.X * (double)num3, ClickZoom_d.Y * (double)num4);
			base.Pan_d = new PointD((double)ClickPoint.X - m_zoomCenterStart.X * base.Zoom_d.X, (double)ClickPoint.Y - m_zoomCenterStart.Y * base.Zoom_d.Y);
			break;
		}
		case MouseEditAction.Select:
			m_renderer.Pick(m_pickableCurves, empty, points, regions, singlePick);
			SetSelection(points, regions);
			break;
		case MouseEditAction.AddToSelection:
			m_renderer.Pick(m_pickableCurves, empty, points, regions, singlePick);
			AddToSelection(points, regions);
			break;
		case MouseEditAction.RemoveFromSelection:
			m_renderer.Pick(m_pickableCurves, empty, points, regions, singlePick);
			RemoveFromSelection(points, regions);
			break;
		case MouseEditAction.ToggleSelection:
			m_renderer.Pick(m_pickableCurves, empty, points, regions, singlePick);
			ToggleSelection(points, regions);
			break;
		case MouseEditAction.MoveSelectionAlongX:
		case MouseEditAction.MoveSelectionAlongY:
		case MouseEditAction.MoveSelection:
		{
			m_updateCurveLimits = true;
			float dx = 0f;
			float dy = 0f;
			switch (action)
			{
			case MouseEditAction.MoveSelectionAlongX:
				dx = CurrentGraphPoint.X - ClickGraphPoint.X;
				break;
			case MouseEditAction.MoveSelectionAlongY:
				dy = CurrentGraphPoint.Y - ClickGraphPoint.Y;
				break;
			default:
				dx = CurrentGraphPoint.X - ClickGraphPoint.X;
				dy = CurrentGraphPoint.Y - ClickGraphPoint.Y;
				break;
			}
			Translate(dx, dy);
			break;
		}
		case MouseEditAction.PivotScale:
		case MouseEditAction.PivotScaleAlongX:
		case MouseEditAction.PivotScaleAlongY:
		{
			m_updateCurveLimits = true;
			float num5 = 10f;
			float xScale = 0f;
			float yScale = 0f;
			switch (action)
			{
			case MouseEditAction.PivotScaleAlongX:
				xScale = 1f + num5 * num / (float)base.Width;
				break;
			case MouseEditAction.PivotScaleAlongY:
				yScale = 1f + num5 * (0f - num2) / (float)base.Height;
				break;
			default:
				xScale = 1f + num5 * num / (float)base.Width;
				yScale = 1f + num5 * (0f - num2) / (float)base.Height;
				break;
			}
			ScalePoints(xScale, yScale);
			break;
		}
		case MouseEditAction.AddControlPoint:
		case MouseEditAction.InsertPoint:
		{
			m_updateCurveLimits = true;
			IEnumerable<ICurve> curves = null;
			if (m_visibleCurveCount == 1)
			{
				ICurve curve = m_curves.First((ICurve c) => c.Visible);
				curves = new ICurve[1] { curve };
			}
			else
			{
				IEnumerable<ICurve> enumerable;
				if (m_editSet.Count <= 0)
				{
					IEnumerable<ICurve> selectedCurves = m_selectedCurves;
					enumerable = selectedCurves;
				}
				else
				{
					IEnumerable<ICurve> selectedCurves = m_editSet;
					enumerable = selectedCurves;
				}
				curves = enumerable;
			}
			bool insert = action == MouseEditAction.InsertPoint;
			string transactionName = (insert ? "Insert Control Point".Localize() : "Add Control Point".Localize());
			Vec2F pt = (insert ? CurrentGraphPoint : ClickGraphPoint);
			m_transactionContext.DoTransaction(delegate
			{
				curves.ForEach(delegate(ICurve curve2)
				{
					CurveUtils.AddControlPoint(curve2, pt, insert);
				});
			}, transactionName);
			break;
		}
		case MouseEditAction.AddControlPointToEmptyCurve:
		{
			m_updateCurveLimits = true;
			if (m_curves.Count <= 0)
			{
				break;
			}
			ICurve emptyCurve = null;
			foreach (ICurve curf in m_curves)
			{
				if (curf.Visible && curf.ControlPoints.Count == 0)
				{
					emptyCurve = curf;
					break;
				}
			}
			if (emptyCurve != null)
			{
				m_transactionContext.DoTransaction(delegate
				{
					CurveUtils.AddControlPoint(emptyCurve, ClickGraphPoint, insert: false);
					regions.Add(PointSelectionRegions.Point);
					points.Add(emptyCurve.ControlPoints[0]);
					AddToSelection(points, regions);
				}, "Add Control Point".Localize());
			}
			break;
		}
		case MouseEditAction.CurveLimitResize:
			try
			{
				Editing = true;
				if (m_limitHit != null)
				{
					if (m_limitSide == CurveLimitSides.Left)
					{
						m_limitHit.MinX = CurrentGraphPoint.X;
					}
					else if (m_limitSide == CurveLimitSides.Right)
					{
						m_limitHit.MaxX = CurrentGraphPoint.X;
					}
					else if (m_limitSide == CurveLimitSides.Top)
					{
						m_limitHit.MaxY = CurrentGraphPoint.Y;
					}
					else if (m_limitSide == CurveLimitSides.Bottom)
					{
						m_limitHit.MinY = CurrentGraphPoint.Y;
					}
					ValidateCurveLimits(m_limitHit, m_limitSide);
				}
				break;
			}
			catch (InvalidTransactionException ex)
			{
				if (m_transactionContext.InTransaction)
				{
					m_transactionContext.Cancel();
				}
				if (ex.ReportError)
				{
					Outputs.WriteLine(OutputMessageType.Error, ex.Message);
				}
				break;
			}
			finally
			{
				Editing = false;
			}
		}
	}

	public void UpdateCurveLimits()
	{
		if (m_curves.Count == 0 || !AutoComputeCurveLimitsEnabled)
		{
			return;
		}
		bool editing = Editing;
		try
		{
			Editing = true;
			foreach (ICurve curf in m_curves)
			{
				RectangleF rectangleF = ComputeBound(curf.ControlPoints);
				if (rectangleF != RectangleF.Empty)
				{
					curf.MinX = rectangleF.Left;
					curf.MaxX = rectangleF.Right;
					curf.MinY = rectangleF.Top;
					curf.MaxY = rectangleF.Bottom;
				}
			}
		}
		finally
		{
			Editing = editing;
		}
	}

	private ICurve PickCurveLimits(out CurveLimitSides side)
	{
		side = CurveLimitSides.None;
		if (AutoComputeCurveLimitsEnabled || !AllowResizeCurveLimits)
		{
			return null;
		}
		ICurve result = null;
		for (int num = SelectedCurves.Length - 1; num >= 0; num--)
		{
			ICurve curve = SelectedCurves[num];
			Vec2F p = new Vec2F(curve.MinX, curve.MinY);
			Vec2F p2 = new Vec2F(curve.MaxX, curve.MaxY);
			p = GraphToClient(p);
			p2 = GraphToClient(p2);
			RectangleF rect = MakeRect(p, p2);
			RectangleF rectangleF = RectangleF.Inflate(rect, 4f, 4f);
			if (curve.Visible && rectangleF.Contains(CurrentPoint) && !RectangleF.Inflate(rect, -4f, -4f).Contains(CurrentPoint))
			{
				m_limitHits.Clear();
				m_limitHits[Math.Abs(p.X - CurrentPoint.X)] = CurveLimitSides.Left;
				m_limitHits[Math.Abs(p2.X - CurrentPoint.X)] = CurveLimitSides.Right;
				m_limitHits[Math.Abs(p.Y - CurrentPoint.Y)] = CurveLimitSides.Bottom;
				m_limitHits[Math.Abs(p2.Y - CurrentPoint.Y)] = CurveLimitSides.Top;
				foreach (KeyValuePair<float, CurveLimitSides> limitHit in m_limitHits)
				{
					if (limitHit.Key <= 4f)
					{
						side = limitHit.Value;
						result = curve;
						break;
					}
				}
				break;
			}
		}
		return result;
	}

	private void ValidateCurveLimits(ICurve curve, CurveLimitSides side)
	{
		switch (side)
		{
		case CurveLimitSides.Left:
		{
			float num3 = ((curve.ControlPoints.Count > 0) ? curve.ControlPoints[0] : null)?.X ?? (curve.MaxX - CurveUtils.Epsilone);
			if (curve.MinX > num3)
			{
				curve.MinX = num3;
			}
			break;
		}
		case CurveLimitSides.Right:
		{
			float num2 = ((curve.ControlPoints.Count > 0) ? curve.ControlPoints[curve.ControlPoints.Count - 1] : null)?.X ?? (curve.MinX + CurveUtils.Epsilone);
			if (curve.MaxX < num2)
			{
				curve.MaxX = num2;
			}
			break;
		}
		case CurveLimitSides.Top:
		{
			float num4 = curve.MinY + CurveUtils.Epsilone;
			foreach (IControlPoint controlPoint in curve.ControlPoints)
			{
				if (controlPoint.Y > num4)
				{
					num4 = controlPoint.Y;
				}
			}
			if (curve.MaxY < num4)
			{
				curve.MaxY = num4;
			}
			break;
		}
		case CurveLimitSides.Bottom:
		{
			float num = curve.MaxY - CurveUtils.Epsilone;
			foreach (IControlPoint controlPoint2 in curve.ControlPoints)
			{
				if (controlPoint2.Y < num)
				{
					num = controlPoint2.Y;
				}
			}
			if (curve.MinY > num)
			{
				curve.MinY = num;
			}
			break;
		}
		}
	}

	private void SnapPoint(IControlPoint p1, Vec2F v2, float threshold)
	{
		Vec2F vec2F = new Vec2F(p1.X, p1.Y);
		Vec2F tan = vec2F - v2;
		float length = GraphToClientTangent(tan).Length;
		if (length > threshold)
		{
			return;
		}
		p1.Y = v2.Y;
		float num = v2.X;
		ICurve curve = p1.Parent;
		int num2 = curve.ControlPoints.IndexOf(p1);
		if (num2 == -1)
		{
			throw new ArgumentException("p1");
		}
		IControlPoint controlPoint = null;
		if (p1.X > num)
		{
			controlPoint = ((num2 != 0) ? curve.ControlPoints[num2 - 1] : null);
			if (controlPoint != null && Math.Abs(controlPoint.X - num) <= CurveUtils.Epsilone)
			{
				p1.X = controlPoint.X + CurveUtils.Epsilone;
			}
			else
			{
				p1.X = num;
			}
		}
		else if (p1.X < num)
		{
			controlPoint = ((num2 + 1 < curve.ControlPoints.Count) ? curve.ControlPoints[num2 + 1] : null);
			if (controlPoint != null && Math.Abs(controlPoint.X - num) <= CurveUtils.Epsilone)
			{
				p1.X = controlPoint.X - CurveUtils.Epsilone;
			}
			else
			{
				p1.X = num;
			}
		}
	}

	private void SnapToY(IControlPoint cpt, float snapValue)
	{
		float num = CurveUtils.SnapTo(cpt.Y, snapValue);
		cpt.Y = num;
	}

	private void SnapToX(IControlPoint cpt, float snapValue)
	{
		ICurve curve = cpt.Parent;
		int num = curve.ControlPoints.IndexOf(cpt);
		if (num == -1)
		{
			throw new ArgumentException("cpt");
		}
		float num2 = CurveUtils.SnapTo(cpt.X, snapValue);
		IControlPoint controlPoint = null;
		if (cpt.X > num2)
		{
			controlPoint = ((num != 0) ? curve.ControlPoints[num - 1] : null);
			if (controlPoint != null && Math.Abs(controlPoint.X - num2) <= CurveUtils.Epsilone)
			{
				cpt.X = controlPoint.X + CurveUtils.Epsilone;
			}
			else
			{
				cpt.X = num2;
			}
		}
		else if (cpt.X < num2)
		{
			controlPoint = ((num + 1 < curve.ControlPoints.Count) ? curve.ControlPoints[num + 1] : null);
			if (controlPoint != null && Math.Abs(controlPoint.X - num2) <= CurveUtils.Epsilone)
			{
				cpt.X = controlPoint.X - CurveUtils.Epsilone;
			}
			else
			{
				cpt.X = num2;
			}
		}
	}

	private void ScalePoints(float xScale, float yScale)
	{
		if (m_selection.Count == 0)
		{
			return;
		}
		try
		{
			m_curveSet.Clear();
			for (int i = 0; i < m_selection.Count; i++)
			{
				IControlPoint controlPoint = m_selection[i];
				if (controlPoint.EditorData.SelectedRegion == PointSelectionRegions.Point)
				{
					if (yScale != 0f)
					{
						float num = m_originalValues[i].Y;
						num -= m_scalePivot.Y;
						num *= yScale;
						num += m_scalePivot.Y;
						controlPoint.Y = num;
					}
					if (xScale != 0f)
					{
						float num2 = m_originalValues[i].X;
						num2 -= m_scalePivot.X;
						num2 *= xScale;
						num2 += ClickGraphPoint.X;
						controlPoint.X = num2;
					}
				}
			}
			for (int j = 0; j < m_selection.Count; j++)
			{
				IControlPoint controlPoint2 = m_selection[j];
				if (!CurveUtils.IsSorted(controlPoint2))
				{
					m_curveSet.Add(controlPoint2.Parent);
				}
			}
			if (RestrictedTranslationEnabled)
			{
				if (m_curveSet.Count > 0)
				{
					for (int k = 0; k < m_selection.Count; k++)
					{
						IControlPoint controlPoint3 = m_selection[k];
						PointF pointF = m_originalValues[k];
						controlPoint3.X = pointF.X;
						controlPoint3.Y = pointF.Y;
					}
				}
			}
			else
			{
				foreach (ICurve item in m_curveSet)
				{
					CurveUtils.Sort(item);
				}
			}
			ICurve[] selectedCurves = m_selectedCurves;
			foreach (ICurve curve in selectedCurves)
			{
				CurveUtils.ForceMinDistance(curve, CurveUtils.Epsilone);
				CurveUtils.ComputeTangent(curve);
			}
		}
		catch (InvalidTransactionException ex)
		{
			if (m_transactionContext.InTransaction)
			{
				m_transactionContext.Cancel();
			}
			if (ex.ReportError)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			}
		}
	}

	private void Translate(float dx, float dy)
	{
		if (m_selection.Count == 0)
		{
			return;
		}
		try
		{
			Predicate<float> predicate = (float angle) => angle < -1.5706218f || angle > 1.5706218f;
			m_curveSet.Clear();
			for (int num = 0; num < m_selection.Count; num++)
			{
				IControlPoint controlPoint = m_selection[num];
				PointSelectionRegions selectedRegion = controlPoint.EditorData.SelectedRegion;
				if (selectedRegion == PointSelectionRegions.TangentIn || selectedRegion == PointSelectionRegions.TangentOut)
				{
					Vec2F tangentIn = controlPoint.TangentIn;
					Vec2F tangentOut = controlPoint.TangentOut;
					float num2 = (float)Math.Atan2(tangentIn.Y, tangentIn.X);
					float num3 = (float)Math.Atan2(tangentOut.Y, tangentOut.X);
					float num4 = 0f;
					float num5 = 0f;
					if (selectedRegion == PointSelectionRegions.TangentIn)
					{
						float num6 = (float)Math.Atan2(PreviousGraphPoint.Y - controlPoint.Y, controlPoint.X - PreviousGraphPoint.X);
						float num7 = (float)Math.Atan2(CurrentGraphPoint.Y - controlPoint.Y, controlPoint.X - CurrentGraphPoint.X);
						float num8 = num6 - num7;
						num4 = num2 + num8;
						num5 = num3 + (num4 - num2);
						if (!predicate(num4) && !predicate(num5))
						{
							controlPoint.TangentIn = new Vec2F((float)Math.Cos(num4), (float)Math.Sin(num4));
							controlPoint.TangentInType = CurveTangentTypes.Fixed;
							if (!controlPoint.BrokenTangents && controlPoint.TangentOutType != CurveTangentTypes.Stepped && controlPoint.TangentOutType != CurveTangentTypes.SteppedNext)
							{
								controlPoint.TangentOut = new Vec2F((float)Math.Cos(num5), (float)Math.Sin(num5));
								controlPoint.TangentOutType = CurveTangentTypes.Fixed;
							}
						}
						continue;
					}
					float num9 = (float)Math.Atan2(PreviousGraphPoint.Y - controlPoint.Y, PreviousGraphPoint.X - controlPoint.X);
					float num10 = (float)Math.Atan2(CurrentGraphPoint.Y - controlPoint.Y, CurrentGraphPoint.X - controlPoint.X);
					float num11 = num10 - num9;
					num5 = num3 + num11;
					num4 = num2 + (num5 - num3);
					if (!predicate(num4) && !predicate(num5))
					{
						controlPoint.TangentOut = new Vec2F((float)Math.Cos(num5), (float)Math.Sin(num5));
						controlPoint.TangentOutType = CurveTangentTypes.Fixed;
						if (!controlPoint.BrokenTangents && controlPoint.TangentInType != CurveTangentTypes.Stepped && controlPoint.TangentInType != CurveTangentTypes.SteppedNext)
						{
							controlPoint.TangentIn = new Vec2F((float)Math.Cos(num4), (float)Math.Sin(num4));
							controlPoint.TangentInType = CurveTangentTypes.Fixed;
						}
					}
				}
				else if (selectedRegion == PointSelectionRegions.Point)
				{
					PointF pointF = m_originalValues[num];
					controlPoint.X = pointF.X + dx;
					controlPoint.Y = pointF.Y + dy;
				}
			}
			for (int num12 = 0; num12 < m_selection.Count; num12++)
			{
				IControlPoint controlPoint2 = m_selection[num12];
				if (!CurveUtils.IsSorted(controlPoint2))
				{
					m_curveSet.Add(controlPoint2.Parent);
				}
			}
			if (RestrictedTranslationEnabled)
			{
				if (m_curveSet.Count > 0)
				{
					for (int num13 = 0; num13 < m_selection.Count; num13++)
					{
						IControlPoint controlPoint3 = m_selection[num13];
						PointF pointF2 = m_originalValues[num13];
						controlPoint3.X = pointF2.X;
						controlPoint3.Y = pointF2.Y;
					}
				}
			}
			else
			{
				foreach (ICurve item in m_curveSet)
				{
					CurveUtils.Sort(item);
				}
				foreach (IControlPoint item2 in m_selection)
				{
					float num14 = item2.X;
					float num15 = item2.Y;
					if (m_autoPointSnap)
					{
						foreach (ICurve curf in m_curves)
						{
							if (item2.Parent == curf)
							{
								continue;
							}
							foreach (IControlPoint controlPoint4 in curf.ControlPoints)
							{
								if (controlPoint4.EditorData.SelectedRegion == PointSelectionRegions.None)
								{
									SnapPoint(item2, new Vec2F(controlPoint4.X, controlPoint4.Y), 16f);
								}
							}
						}
					}
					if (m_autoCurveSnap)
					{
						foreach (ICurve curf2 in m_curves)
						{
							if (item2.Parent != curf2)
							{
								ICurveEvaluator curveEvaluator = CurveUtils.CreateCurveEvaluator(curf2);
								float num16 = curveEvaluator.Evaluate(item2.X);
								SnapPoint(item2, new Vec2F(item2.X, num16), 16f);
							}
						}
					}
					if (m_autoSnapToX && num14 == item2.X)
					{
						SnapToX(item2, base.MajorTickX);
					}
					if (m_autoSnapToY && num15 == item2.Y)
					{
						SnapToY(item2, base.MajorTickY);
					}
				}
				ICurve[] selectedCurves = m_selectedCurves;
				foreach (ICurve curve in selectedCurves)
				{
					CurveUtils.ForceMinDistance(curve, CurveUtils.Epsilone);
				}
			}
			ICurve[] selectedCurves2 = m_selectedCurves;
			foreach (ICurve curve2 in selectedCurves2)
			{
				CurveUtils.ComputeTangent(curve2);
			}
		}
		catch (InvalidTransactionException ex)
		{
			if (m_transactionContext.InTransaction)
			{
				m_transactionContext.Cancel();
			}
			if (ex.ReportError)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			}
		}
	}

	private void SetSelection(List<IControlPoint> points, List<PointSelectionRegions> regions)
	{
		m_selection.BeginUpdate();
		ClearSelection();
		for (int i = 0; i < points.Count; i++)
		{
			IControlPoint controlPoint = points[i];
			controlPoint.EditorData.SelectedRegion = regions[i];
			m_selection.Add(controlPoint);
		}
		m_selection.EndUpdate();
	}

	private void AddToSelection(List<IControlPoint> points, List<PointSelectionRegions> regions)
	{
		m_selection.BeginUpdate();
		for (int i = 0; i < points.Count; i++)
		{
			IControlPoint controlPoint = points[i];
			controlPoint.EditorData.SelectedRegion = regions[i];
			m_selection.Add(controlPoint);
		}
		m_selection.EndUpdate();
	}

	private void RemoveFromSelection(List<IControlPoint> points, List<PointSelectionRegions> regions)
	{
		m_selection.BeginUpdate();
		for (int i = 0; i < points.Count; i++)
		{
			IControlPoint controlPoint = points[i];
			controlPoint.EditorData.SelectedRegion = PointSelectionRegions.None;
			m_selection.Remove(controlPoint);
		}
		m_selection.EndUpdate();
	}

	private void ToggleSelection(List<IControlPoint> points, List<PointSelectionRegions> regions)
	{
		m_selection.BeginUpdate();
		for (int i = 0; i < points.Count; i++)
		{
			IControlPoint controlPoint = points[i];
			PointSelectionRegions pointSelectionRegions = regions[i];
			if (m_selection.Contains(controlPoint))
			{
				if (controlPoint.EditorData.SelectedRegion == pointSelectionRegions)
				{
					m_selection.Remove(controlPoint);
					controlPoint.EditorData.SelectedRegion = PointSelectionRegions.None;
				}
				else
				{
					controlPoint.EditorData.SelectedRegion = pointSelectionRegions;
				}
			}
			else
			{
				controlPoint.EditorData.SelectedRegion = pointSelectionRegions;
				m_selection.Add(controlPoint);
			}
		}
		m_selection.EndUpdate();
	}

	private void m_selection_SelectionChanged(object sender, EventArgs e)
	{
		Dictionary<ICurve, object> dictionary = new Dictionary<ICurve, object>();
		foreach (IControlPoint item in m_selection)
		{
			if (item.EditorData.SelectedRegion == PointSelectionRegions.None)
			{
				item.EditorData.SelectedRegion = PointSelectionRegions.Point;
			}
			dictionary[item.Parent] = null;
		}
		m_selectedCurves = new ICurve[dictionary.Count];
		dictionary.Keys.CopyTo(m_selectedCurves, 0);
		this.SelectionChanged(this, EventArgs.Empty);
	}

	private void ClearSelection()
	{
		foreach (IControlPoint item in m_selection)
		{
			item.EditorData.SelectedRegion = PointSelectionRegions.None;
		}
		m_selection.Clear();
	}

	private void ReComputeTangents()
	{
		ICurve[] selectedCurves = m_selectedCurves;
		foreach (ICurve curve in selectedCurves)
		{
			if (curve.CurveInterpolation != InterpolationTypes.Linear)
			{
				CurveUtils.ComputeTangent(curve);
			}
		}
	}
}
