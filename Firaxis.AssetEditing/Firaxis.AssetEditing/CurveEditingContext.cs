using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class CurveEditingContext : SelectionContext, ITransactionContext, IInstancingContext, ICurveEditingContext
{
	private List<CurveControlPointViewModel> m_curveControlPoints = new List<CurveControlPointViewModel>();

	private List<CurveSegmentDefinitionViewModel> m_curveSegmentDefinitions = new List<CurveSegmentDefinitionViewModel>();

	public Size ControlPointSize { get; set; } = new Size(5, 5);

	public Size MouseOverControlPointSize { get; set; } = new Size(7, 7);

	public Size SelectedControlPointSize { get; set; } = new Size(9, 9);

	public PointF MenuCurvePosition { get; set; } = PointF.Empty;

	public CurveEditingCommands CurveEditingCommands { get; private set; }

	public ICommandService CommandService { get; private set; }

	public IThemeService ThemeService { get; set; }

	public float MinY { get; set; }

	public float MaxY { get; set; } = 1f;

	public bool ClampDomain { get; set; }

	public float MinX { get; set; }

	public float MaxX { get; set; } = 1f;

	public IEnumerable<CurveControlPointViewModel> CurveControlPoints => m_curveControlPoints;

	public IEnumerable<CurveSegmentDefinitionViewModel> CurveSegmentDefinitions => m_curveSegmentDefinitions;

	public bool InTransaction => base.DomNode.GetRoot().As<HistoryContext>().InTransaction;

	public int PendingOperationCount => base.DomNode.GetRoot().As<HistoryContext>().PendingOperationCount;

	public void Begin(string transactionName)
	{
		base.DomNode.GetRoot().As<HistoryContext>().Begin(transactionName);
	}

	public void Cancel()
	{
		base.DomNode.GetRoot().As<HistoryContext>().Cancel();
	}

	public void End()
	{
		base.DomNode.GetRoot().As<HistoryContext>().End();
	}

	public TransactionSuspensionReceipt SuspendTransactions()
	{
		return base.DomNode.GetRoot().As<HistoryContext>().SuspendTransactions();
	}

	public void InitializeCommands(ICommandService commandService)
	{
		CommandService = commandService;
		if (CurveEditingCommands == null)
		{
			CurveEditingCommands = new CurveEditingCommands(commandService, this);
		}
		else
		{
			CurveEditingCommands.RegisterCommands(commandService);
		}
	}

	public IEnumerable<CommandInfo> GetValidCommandInfos_ContextMenu()
	{
		return CurveEditingCommands.Commands.Where((CommandInfo cmd) => CurveEditingCommands.CurveCommandGroup.CurveCommands.Equals(cmd.GroupTag));
	}

	public void Paint(Func<PointF, PointF> curveSpaceToDrawSpace, Graphics graphics)
	{
		foreach (CurveSegmentDefinitionViewModel curveSegmentDefinition in CurveSegmentDefinitions)
		{
			curveSegmentDefinition.PaintEndLines(curveSpaceToDrawSpace, graphics, MaxY, MinY);
			curveSegmentDefinition.PaintCurveLines(curveSpaceToDrawSpace, graphics);
		}
		foreach (CurveSegmentDefinitionViewModel curveSegmentDefinition2 in CurveSegmentDefinitions)
		{
			curveSegmentDefinition2.PaintControlPoints(curveSpaceToDrawSpace, graphics, this);
		}
	}

	public void InsertCurveSegmentDefinition(CurveSegmentType segmentType, params object[] args)
	{
		CurveSegmentDefinitionAdapter curveSegmentDefinitionAdapter = CurveSegmentDefinitionAdapter.Create(segmentType, args);
		BugSubmitter.Assert(curveSegmentDefinitionAdapter != null, "Created a new segment that ended up null.  Invalid arguments for creation?");
		bool flag = false;
		if (!InTransaction)
		{
			Begin("Insert Curve Segment.");
			flag = true;
		}
		InsertCurveSegmentDefinitionAdapter(curveSegmentDefinitionAdapter);
		if (flag)
		{
			End();
		}
	}

	private void InsertCurveSegmentDefinitionAdapter(CurveSegmentDefinitionAdapter newSegmentAdapter)
	{
		CurveAdapter curveAdapter = base.DomNode.As<CurveAdapter>();
		if (curveAdapter.CurveSegments.Count == 0)
		{
			newSegmentAdapter.StartingPoint = 0f;
		}
		int i;
		for (i = 0; i < curveAdapter.CurveSegments.Count && !(curveAdapter.CurveSegments[i].StartingPoint > newSegmentAdapter.StartingPoint); i++)
		{
		}
		if (i < m_curveSegmentDefinitions.Count)
		{
			curveAdapter.CurveSegments.Insert(i, newSegmentAdapter);
		}
		else
		{
			curveAdapter.CurveSegments.Add(newSegmentAdapter);
		}
		int num = i - 1;
		if (num >= 0)
		{
			LinearCurveSegmentAdapter linearCurveSegmentAdapter = curveAdapter.CurveSegments[num].Curve.As<LinearCurveSegmentAdapter>();
			if (linearCurveSegmentAdapter != null)
			{
				linearCurveSegmentAdapter.LastValue = newSegmentAdapter.Curve.GetCurveValue(0f);
			}
		}
	}

	public void InsertConstantCurveSegmentDefinition(float segmentBegin, float value)
	{
		InsertCurveSegmentDefinition(CurveSegmentType.CST_CONSTANT, segmentBegin, value);
	}

	public void InsertLinearCurveSegmentDefinition(float segmentBegin, float firstValue)
	{
		CurveAdapter curveAdapter = base.DomNode.As<CurveAdapter>();
		CurveSegmentDefinitionAdapter curveSegmentDefinitionAdapter = curveAdapter.CurveSegments.FirstOrDefault((CurveSegmentDefinitionAdapter seg) => seg.StartingPoint > segmentBegin);
		float lastValue = firstValue;
		if (curveSegmentDefinitionAdapter != null)
		{
			lastValue = curveSegmentDefinitionAdapter.Curve.GetCurveValue(0f);
		}
		else
		{
			CurveSegmentDefinitionAdapter curveSegmentDefinitionAdapter2 = curveAdapter.CurveSegments.LastOrDefault((CurveSegmentDefinitionAdapter seg) => seg.StartingPoint < segmentBegin);
			if (curveSegmentDefinitionAdapter2 != null)
			{
				lastValue = curveSegmentDefinitionAdapter2.Curve.GetCurveValue(1f);
			}
		}
		InsertLinearCurveSegmentDefinition(segmentBegin, firstValue, lastValue);
	}

	public void InsertLinearCurveSegmentDefinition(float segmentBegin, float firstValue, float lastValue)
	{
		InsertCurveSegmentDefinition(CurveSegmentType.CST_LINEAR, segmentBegin, firstValue, lastValue);
	}

	public void RemoveSelectedCurveSegments()
	{
		object[] source = base.Selection.ToArray();
		bool flag = false;
		if (!InTransaction)
		{
			Begin("Remove selected curve segments.");
			flag = true;
		}
		source.OfType<CurveControlPointViewModel>().ForEach(delegate(CurveControlPointViewModel pt)
		{
			RemoveCurveSegmentDefinition(pt);
		});
		source.OfType<CurveSegmentDefinitionAdapter>().ForEach(delegate(CurveSegmentDefinitionAdapter adapter)
		{
			RemoveCurveSegmentDefinition(adapter);
		});
		if (flag)
		{
			End();
		}
	}

	public void RemoveCurveSegmentDefinition(CurveControlPointViewModel controlPoint)
	{
		if (base.DomNode.As<CurveAdapter>().CurveSegments.Count != 0)
		{
			if (base.LastSelected == controlPoint)
			{
				base.Selection.Set(null);
			}
			CurveSegmentDefinitionViewModel curveSegmentDefinitionViewModel = CurveSegmentDefinitions.FirstOrDefault((CurveSegmentDefinitionViewModel vm) => vm.StartsWith(controlPoint));
			if (curveSegmentDefinitionViewModel != null)
			{
				RemoveCurveSegmentDefinition(curveSegmentDefinitionViewModel.CurveSegmentDefinition);
			}
		}
	}

	private void RemoveCurveSegmentDefinition(CurveSegmentDefinitionAdapter segmentAdapter)
	{
		if (segmentAdapter == null)
		{
			return;
		}
		if (base.LastSelected == segmentAdapter)
		{
			base.Selection.Set(null);
		}
		CurveAdapter curveAdapter = base.DomNode.As<CurveAdapter>();
		IList<CurveSegmentDefinitionAdapter> curveSegments = curveAdapter.CurveSegments;
		int num = curveSegments.IndexOf(segmentAdapter);
		if (num < 0 || num >= curveSegments.Count)
		{
			return;
		}
		bool flag = false;
		if (!InTransaction)
		{
			Begin("Remove selected curve segments.");
			flag = true;
		}
		curveSegments.RemoveAt(num);
		if (curveSegments.Count == 1 || (num == 0 && curveSegments.Count > 0))
		{
			curveAdapter.CurveSegments[0].StartingPoint = 0f;
		}
		int num2 = num - 1;
		if (num2 >= 0 && curveSegments.Count >= 2 && num < curveSegments.Count)
		{
			LinearCurveSegmentAdapter linearCurveSegmentAdapter = curveSegments[num2].Curve.As<LinearCurveSegmentAdapter>();
			if (linearCurveSegmentAdapter != null)
			{
				linearCurveSegmentAdapter.LastValue = curveSegments[num].Curve.GetCurveValue(0f);
			}
		}
		if (flag)
		{
			End();
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.ChildInserted += RebuildCollections;
		base.DomNode.ChildRemoved += RebuildCollections;
		base.DomNode.AttributeChanged += UpdateCurveScaleIfNecessary;
	}

	private void RebuildCollections(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo.Type == FieldSchema.CurveSegmentDefinitionType.Type)
		{
			CurveAdapter adapter = base.DomNode.As<CurveAdapter>();
			RebuildCurveSegmentDefinitionCollection(adapter, m_curveSegmentDefinitions);
			RebuildCurveControlPoints(m_curveSegmentDefinitions, m_curveControlPoints);
		}
	}

	private void UpdateCurveScaleIfNecessary(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo.Type == FieldSchema.ConstantCurveSegmentType.ConstantValueAttribute.Type || e.AttributeInfo.Type == FieldSchema.LinearCurveSegmentType.FirstValueAttribute.Type || e.AttributeInfo.Type == FieldSchema.LinearCurveSegmentType.LastValueAttribute.Type)
		{
			_ = (float)e.NewValue;
		}
	}

	private void RebuildCurveSegmentDefinitionCollection(CurveAdapter adapter, ICollection<CurveSegmentDefinitionViewModel> segmentViewModels)
	{
		segmentViewModels.Clear();
		for (int i = 0; i < adapter.CurveSegments.Count; i++)
		{
			CurveSegmentDefinitionViewModel curveSegmentDefinitionViewModel = CreateViewModel(adapter, i);
			if (curveSegmentDefinitionViewModel != null)
			{
				segmentViewModels.Add(curveSegmentDefinitionViewModel);
			}
		}
	}

	private CurveSegmentDefinitionViewModel CreateViewModel(CurveAdapter curve, int segmentIndex)
	{
		BugSubmitter.Assert(curve != null, "Cannot create a ViewModel for a segment with a null curve!  @assign dgurley");
		BugSubmitter.Assert(segmentIndex < curve.CurveSegments.Count, "Cannot create a ViewModel for a segment with an out-of-bounds index!  @assign dgurley");
		CurveSegmentDefinitionAdapter curveSegmentDefinitionAdapter = curve.CurveSegments[segmentIndex];
		BugSubmitter.Assert(curveSegmentDefinitionAdapter != null, "Cannot create a ViewModel for a null segment.  This should have been removed from the SegmentList.  @assign dgurley");
		if (curveSegmentDefinitionAdapter.Curve.Is<ConstantCurveSegmentAdapter>())
		{
			return new ConstantCurveSegmentDefinitionViewModel(curve, segmentIndex, this);
		}
		if (curveSegmentDefinitionAdapter.Curve.Is<LinearCurveSegmentAdapter>())
		{
			return new LinearCurveSegmentDefinitionViewModel(curve, segmentIndex, this);
		}
		BugSubmitter.Assert(condition: false, "Unkown curve type.  Update this factory function!  @assign dgurley");
		return null;
	}

	private void RebuildCurveControlPoints(IList<CurveSegmentDefinitionViewModel> segmentViewModels, List<CurveControlPointViewModel> controlPoints)
	{
		controlPoints.Clear();
		foreach (CurveSegmentDefinitionViewModel segmentViewModel in segmentViewModels)
		{
			controlPoints.AddRange(segmentViewModel.GetControlPoints());
		}
	}

	public void SelectNextPoint()
	{
		CurveControlPointViewModel lastSelected = base.Selection.GetLastSelected<CurveControlPointViewModel>();
		int num = m_curveControlPoints.IndexOf(lastSelected);
		if (num >= 0)
		{
			num = ((num != m_curveControlPoints.Count - 1) ? (num + 1) : 0);
			base.Selection.Set(m_curveControlPoints[num]);
		}
	}

	public void SelectPreviousPoint()
	{
		CurveControlPointViewModel lastSelected = base.Selection.GetLastSelected<CurveControlPointViewModel>();
		int num = m_curveControlPoints.IndexOf(lastSelected);
		if (num >= 0)
		{
			num = ((num != m_curveControlPoints.Count - 1) ? (num - 1) : 0);
			this.Set(m_curveControlPoints[num]);
		}
	}

	public void NudgeSelectedPointsRight()
	{
		this.DoTransaction(delegate
		{
			MoveSelectedPoints(new PointF(0.01f, 0f));
		}, "Nudge Right.".Localize());
	}

	public void NudgeSelectedPointsLeft()
	{
		this.DoTransaction(delegate
		{
			MoveSelectedPoints(new PointF(-0.01f, 0f));
		}, "Nudge Left.".Localize());
	}

	public void NudgeSelectedPointsUp()
	{
		this.DoTransaction(delegate
		{
			MoveSelectedPoints(new PointF(0f, 0.01f));
		}, "Nudge Up.".Localize());
	}

	public void NudgeSelectedPointsDown()
	{
		this.DoTransaction(delegate
		{
			MoveSelectedPoints(new PointF(0f, -0.01f));
		}, "Nudge Down.".Localize());
	}

	public void MoveSelectedPoints(PointF curveDelta)
	{
		CurveControlPointViewModel[] array = base.Selection.OfType<CurveControlPointViewModel>().ToArray();
		CurveControlPointViewModel[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			curveDelta = array2[i].GetActualLocationDelta(curveDelta);
		}
		Dictionary<CurveControlPointViewModel, PointF> dictionary = new Dictionary<CurveControlPointViewModel, PointF>(array.Length);
		array2 = array;
		foreach (CurveControlPointViewModel curveControlPointViewModel in array2)
		{
			dictionary[curveControlPointViewModel] = new PointF(curveControlPointViewModel.Location.X + curveDelta.X, curveControlPointViewModel.Location.Y + curveDelta.Y);
		}
		foreach (KeyValuePair<CurveControlPointViewModel, PointF> item in dictionary)
		{
			item.Key.Location = item.Value;
		}
	}

	public bool CanCopy()
	{
		return ((ISelectionContext)this).SelectionCount > 0;
	}

	public object Copy()
	{
		List<CurveSegmentDefinitionAdapter> list = new List<CurveSegmentDefinitionAdapter>();
		List<CurveSegmentDefinitionViewModel> list2 = base.Selection.OfType<CurveSegmentDefinitionViewModel>().ToList();
		foreach (CurveControlPointViewModel pointVM in GetSelection<CurveControlPointViewModel>())
		{
			CurveSegmentDefinitionViewModel curveSegmentDefinitionViewModel = CurveSegmentDefinitions.FirstOrDefault((CurveSegmentDefinitionViewModel vm) => vm.GetControlPoints().Contains(pointVM));
			if (curveSegmentDefinitionViewModel != null)
			{
				list2.Add(curveSegmentDefinitionViewModel);
			}
		}
		foreach (CurveSegmentDefinitionViewModel item in list2)
		{
			if (!list.Contains(item.CurveSegmentDefinition))
			{
				list.Add(item.CurveSegmentDefinition);
			}
		}
		return list.ToArray();
	}

	public bool CanInsert(object dataObject)
	{
		if (dataObject == null)
		{
			return false;
		}
		if (dataObject is IEnumerable<CurveSegmentDefinitionAdapter>)
		{
			return true;
		}
		return dataObject is CurveSegmentDefinitionAdapter;
	}

	public void Insert(object dataObject)
	{
		bool flag = false;
		if (!InTransaction)
		{
			Begin("Insert curve segments.");
			flag = true;
		}
		if (dataObject is IEnumerable<CurveSegmentDefinitionAdapter> enumerable)
		{
			foreach (CurveSegmentDefinitionAdapter item in enumerable)
			{
				InsertCurveSegmentDefinitionAdapter(item);
			}
		}
		else if (dataObject is CurveSegmentDefinitionAdapter newSegmentAdapter)
		{
			InsertCurveSegmentDefinitionAdapter(newSegmentAdapter);
		}
		else
		{
			BugSubmitter.Assert(false, "Insert called on CurveEditingContext with invalid dataObject.  \n\tObject: {0}  \n\n@assign dgurley @summary InvalidCurveInsert", dataObject.ToString());
		}
		if (flag)
		{
			End();
		}
	}

	public bool CanDelete()
	{
		return ((ISelectionContext)this).SelectionCount > 0;
	}

	public void Delete()
	{
		CurveSegmentDefinitionViewModel[] array = base.Selection.OfType<CurveSegmentDefinitionViewModel>().ToArray();
		CurveControlPointViewModel[] array2 = base.Selection.OfType<CurveControlPointViewModel>().ToArray();
		if (array.Length != 0 || array2.Length != 0)
		{
			this.Set(null);
			bool flag = false;
			if (!InTransaction)
			{
				Begin("Remove selected curve segments.");
				flag = true;
			}
			CurveAdapter curveAdapter = base.DomNode.As<CurveAdapter>();
			CurveSegmentDefinitionViewModel[] array3 = array;
			foreach (CurveSegmentDefinitionViewModel curveSegmentDefinitionViewModel in array3)
			{
				curveAdapter.CurveSegments.Remove(curveSegmentDefinitionViewModel.CurveSegmentDefinition);
			}
			CurveControlPointViewModel[] array4 = array2;
			foreach (CurveControlPointViewModel controlPoint in array4)
			{
				RemoveCurveSegmentDefinition(controlPoint);
			}
			if (flag)
			{
				End();
			}
		}
	}
}
