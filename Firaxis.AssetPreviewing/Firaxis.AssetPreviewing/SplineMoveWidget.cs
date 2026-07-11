using System;
using Firaxis.AssetEditing;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetPreviewing;

public class SplineMoveWidget : IDisposable
{
	private SplineVertexAdapter m_VertexAdapter;

	private IWidget m_VertexLocator;

	private IWidget m_MoveWidget;

	private IEntityChangeList m_changeList = Context.EnsureCreated<CivTechContext>().CreateInstance<IEntityChangeList>();

	private bool disposedValue = false;

	private IPreviewWindow PreviewWindow { get; set; }

	private CivTechContext CivTechFactory { get; set; }

	private SplineAdapter Spline { get; set; }

	public IFloatVector3 Position { get; set; }

	public ISplineVertex Vertex => m_VertexLocator.BoundObject as ISplineVertex;

	public bool IsEditing { get; set; }

	public SplineMoveWidget(SplineAdapter splineAdpt, SplineVertexAdapter vertAdpt, IWidget VertexLocator, IPreviewWindow window, bool is3D, bool isReadOnly)
	{
		CivTechFactory = Context.EnsureCreated<CivTechContext>();
		PreviewWindow = window;
		Spline = splineAdpt;
		ISplineVertex splineVertex = VertexLocator.BoundObject as ISplineVertex;
		m_VertexAdapter = vertAdpt;
		IValueSet valueSet = CivTechFactory.CreateInstance<IValueSet>();
		valueSet.Push<ICoord3DValue>("Position").ParameterValue = splineVertex.Position;
		valueSet.Push<IBoolValue>("2D").ParameterValue = !is3D;
		valueSet.Push<IBoolValue>("IsReadOnly").ParameterValue = isReadOnly;
		m_MoveWidget = PreviewWindow.CreateWidget("Move", valueSet, this);
		m_MoveWidget.OnEdit += SplineMoveWidget_OnEdit;
		m_MoveWidget.OnStartEdit += SplineMoveWidget_OnStartEdit;
		m_MoveWidget.OnFinishEdit += SplineMoveWidget_OnFinishEdit;
		m_MoveWidget.OnCancelEdit += SplineMoveWidget_OnCancelEdit;
		Position = CivTechFactory.CreateInstance<IFloatVector3>();
		Position.X = splineVertex.Position.x;
		Position.Y = splineVertex.Position.y;
		Position.Z = splineVertex.Position.z;
		m_VertexLocator = VertexLocator;
	}

	public IWidget GetLocatorWidget()
	{
		return m_VertexLocator;
	}

	public void UpdateMoveWidget(Point3F pos)
	{
		IValueSet valueSet = CivTechFactory.CreateInstance<IValueSet>();
		valueSet.Push<ICoord3DValue>("Position").ParameterValue = pos;
		m_MoveWidget.Alter(valueSet);
	}

	private void SplineMoveWidget_OnEdit(object sender, EventArgs e)
	{
		BaseEntityPropertyContext baseEntityPropertyContext = m_VertexAdapter.DomNode.GetRoot().As<BaseEntityPropertyContext>();
		BugSubmitter.Assert(baseEntityPropertyContext.InTransaction, "SplineMoveWidget_OnEdit triggered while not in a transaction!");
		IValueSet valueSet = CivTechFactory.CreateInstance<IValueSet>();
		valueSet.Push<ICoord3DValue>("Position").ParameterValue = new Point3F(Position.X, Position.Y, Position.Z);
		m_VertexLocator.Alter(valueSet);
		IBehaviorProviderAdapter behaviorProviderAdapter = m_VertexAdapter.DomNode.GetRoot().As<IBehaviorProviderAdapter>();
		int vertexIndex = Spline.SplineVertexSet.Vertices.IndexOf(m_VertexAdapter);
		ISplineVertexChanged splineVertexChanged = m_changeList.CreateSplineVertexChangedEvent(behaviorProviderAdapter.InstanceEntity, Spline.Name, vertexIndex, new float[3] { Position.X, Position.Y, Position.Z });
		PreviewWindow.UpdateAsset(m_changeList.EntityChanges, 0);
		m_changeList.Clear();
	}

	private void SplineMoveWidget_OnStartEdit(object sender, EventArgs e)
	{
		IsEditing = true;
		TransactionContext transactionContext = m_VertexAdapter.DomNode.GetRoot().As<TransactionContext>();
		transactionContext.Begin("Move Spline Vertex");
	}

	private void SplineMoveWidget_OnFinishEdit(object sender, EventArgs e)
	{
		IsEditing = false;
		TransactionContext transactionContext = m_VertexAdapter.DomNode.GetRoot().As<TransactionContext>();
		m_VertexAdapter.Position = new float[3] { Position.X, Position.Y, Position.Z };
		transactionContext.End();
	}

	private void SplineMoveWidget_OnCancelEdit(object sender, EventArgs e)
	{
		IsEditing = false;
		TransactionContext transactionContext = m_VertexAdapter.DomNode.GetRoot().As<TransactionContext>();
		transactionContext.Cancel();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				m_MoveWidget.CancelPendingEdits();
				m_MoveWidget.OnEdit -= SplineMoveWidget_OnEdit;
				m_MoveWidget.OnStartEdit -= SplineMoveWidget_OnStartEdit;
				m_MoveWidget.OnFinishEdit -= SplineMoveWidget_OnFinishEdit;
				m_MoveWidget.OnCancelEdit -= SplineMoveWidget_OnCancelEdit;
				m_MoveWidget.Dispose();
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
