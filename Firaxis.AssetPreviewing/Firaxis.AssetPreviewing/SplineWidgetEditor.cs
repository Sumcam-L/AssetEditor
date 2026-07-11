using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DatabaseWrapper;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetPreviewing;

public class SplineWidgetEditor : IWidgetEditor, IDisposable
{
	private SplineAdapter m_Spline;

	private SplineMoveWidget m_VertexPusher = null;

	private IPreviewWindow m_Window = null;

	private IEntityDocument m_Document = null;

	private CivTechContext m_CivTechFactory = Context.EnsureCreated<CivTechContext>();

	private List<IWidget> m_VertexLocators = new List<IWidget>();

	private IWidget m_SplineWidget;

	private IWidget m_LastWidgetHovered = null;

	private bool IsReadOnly = true;

	public static readonly string WidgetName = "Spline";

	public SplineAdapter Spline
	{
		get
		{
			return m_Spline;
		}
		set
		{
			if (m_Spline != value)
			{
				if (m_Spline != null)
				{
					UnregisterForItemChanges();
					DestroyWidgets();
				}
				m_Spline = value;
				if (m_Spline != null)
				{
					RegisterForItemChanges();
					CreateWidgets(m_Spline);
				}
			}
		}
	}

	private SplineVertexAdapter SelectedVertex { get; set; }

	private ISplineClass SplineClass
	{
		get
		{
			if (m_Spline == null)
			{
				return null;
			}
			return global::DatabaseWrapper.DatabaseWrapper.GetClass(m_Spline.EntityAdapter.CivTechService.PrimaryProject.Name, m_Spline.EntityAdapter.InstanceType, m_Spline.EntityAdapter.InstanceEntity.ClassName) as ISplineClass;
		}
	}

	public string Name => WidgetName;

	public virtual IEnumerable<IWidgetState> States => Enumerable.Empty<IWidgetState>();

	private void RegisterForItemChanges()
	{
		UnregisterForItemChanges();
		if (m_Spline == null || m_Spline.SplineVertexSet == null)
		{
			return;
		}
		m_Spline.SplineVertexSet.ItemInserted += SplineVertexSet_ItemInserted;
		m_Spline.SplineVertexSet.ItemRemoved += SplineVertexSet_ItemRemoved;
		foreach (SplineVertexAdapter vertex in m_Spline.SplineVertexSet.Vertices)
		{
			vertex.ItemChanged += SplineVertex_ItemChanged;
		}
	}

	private void UnregisterForItemChanges()
	{
		if (m_Spline == null || m_Spline.SplineVertexSet == null)
		{
			return;
		}
		m_Spline.SplineVertexSet.ItemInserted -= SplineVertexSet_ItemInserted;
		m_Spline.SplineVertexSet.ItemRemoved -= SplineVertexSet_ItemRemoved;
		foreach (SplineVertexAdapter vertex in m_Spline.SplineVertexSet.Vertices)
		{
			vertex.ItemChanged -= SplineVertex_ItemChanged;
		}
	}

	private void SplineVertexSet_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		SplineVertexAdapter sva = e.Item.As<SplineVertexAdapter>();
		if (sva == null)
		{
			return;
		}
		sva.ItemChanged -= SplineVertex_ItemChanged;
		IWidget widget = m_VertexLocators.FirstOrDefault((IWidget widget2) => widget2.BoundObject == sva.SplineVertex);
		if (widget != null)
		{
			if (m_LastWidgetHovered == widget)
			{
				SetHighlightedWidget(null);
			}
			if (m_VertexPusher != null && m_VertexPusher.Vertex == sva.SplineVertex)
			{
				DestroyVertexPusher();
			}
			m_VertexLocators.Remove(widget);
			widget.Dispose();
		}
	}

	private void SplineVertex_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		SplineVertexAdapter sva = e.Item.As<SplineVertexAdapter>();
		if (sva == null)
		{
			return;
		}
		IWidget widget = m_VertexLocators.FirstOrDefault((IWidget widget2) => widget2.BoundObject == sva.SplineVertex);
		if (widget != null)
		{
			IValueSet valueSet = m_CivTechFactory.CreateInstance<IValueSet>();
			Point3F point3F = new Point3F(sva.Position[0], sva.Position[1], sva.Position[2]);
			valueSet.Push<ICoord3DValue>("Position").ParameterValue = point3F;
			widget.Alter(valueSet);
			if (m_VertexPusher != null && m_VertexPusher.Vertex == sva.SplineVertex)
			{
				m_VertexPusher.UpdateMoveWidget(point3F);
			}
		}
	}

	private void SplineVertexSet_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		SplineVertexAdapter splineVertexAdapter = e.Item.As<SplineVertexAdapter>();
		if (splineVertexAdapter != null)
		{
			splineVertexAdapter.ItemChanged += SplineVertex_ItemChanged;
			m_VertexLocators.Add(CreateVertexLocator(splineVertexAdapter.SplineVertex));
		}
	}

	private void AddUndoTransaction(string name, DomNodeAdapter adapter, Action action)
	{
		if (m_VertexPusher == null || !m_VertexPusher.IsEditing)
		{
			TransactionContext context = adapter.DomNode.GetRoot().As<TransactionContext>();
			context.DoTransaction(action, name.Localize());
		}
	}

	public void SetTarget(IPreviewWindow window, IEntityDocument target)
	{
		m_Window = window;
		m_Document = target;
	}

	public void Activate(bool isReadOnly)
	{
		IsReadOnly = isReadOnly;
		if (m_Spline != null)
		{
			CreateWidgets(m_Spline);
			RegisterForItemChanges();
		}
	}

	public void Deactivate()
	{
		UnregisterForItemChanges();
		DestroyWidgets();
	}

	public void OnKeyDown(KeyEventArgs evt)
	{
	}

	public void OnKeyUp(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.L)
		{
			AddUndoTransaction("Toggle Closed Loop", m_Spline, delegate
			{
				Spline.ClosedLoop = !Spline.ClosedLoop;
			});
		}
		else
		{
			if (m_VertexPusher == null)
			{
				return;
			}
			if (e.KeyCode == Keys.C)
			{
				AddUndoTransaction("Toggle Sharp Corner", m_Spline, delegate
				{
					SelectedVertex.SharpCorner = !SelectedVertex.SharpCorner;
				});
			}
			else if (e.KeyCode == Keys.Delete)
			{
				AddUndoTransaction("Delete Spline Vertex", m_Spline, delegate
				{
					Spline.SplineVertexSet.Vertices.Remove(SelectedVertex);
				});
			}
		}
	}

	public void OnMouseMove(MouseEventArgs e)
	{
		if (m_Document == null || m_Document.IsReadOnly)
		{
			return;
		}
		IEnumerable<PickResult> enumerable = m_Window?.PickPoint(e.X, e.Y, 1f);
		if (enumerable == null)
		{
			return;
		}
		WidgetPickResult widgetPickResult = FindNearestWidgetPicked(enumerable);
		if (widgetPickResult == null || m_VertexLocators.Contains(widgetPickResult.Widget))
		{
			if (m_VertexPusher != null && widgetPickResult?.Widget == m_VertexPusher.GetLocatorWidget())
			{
				SetHighlightedWidget(null);
			}
			else
			{
				SetHighlightedWidget(widgetPickResult?.Widget);
			}
		}
	}

	public void OnMouseDown(MouseEventArgs e)
	{
		if (m_Document == null || m_Document.IsReadOnly || e.Button != MouseButtons.Left)
		{
			return;
		}
		IEnumerable<PickResult> picks = m_Window?.PickPoint(e.X, e.Y, 1f);
		WidgetPickResult widgetPickResult = FindNearestWidgetPicked(picks);
		if (widgetPickResult != null && widgetPickResult.Widget.BoundObject is ISplineVertex)
		{
			DestroyVertexPusher();
			if (m_Spline != null)
			{
				CreateVertexPusher(widgetPickResult.Widget);
				SetHighlightedWidget(null);
			}
		}
		else if (widgetPickResult == null)
		{
			DestroyVertexPusher();
		}
	}

	public void OnMouseUp(MouseEventArgs e)
	{
		if (m_Document == null || m_Document.IsReadOnly)
		{
			return;
		}
		IEnumerable<PickResult> enumerable = m_Window?.PickPoint(e.X, e.Y, 1f);
		if (enumerable == null)
		{
			return;
		}
		GroundPickResult groundPick = enumerable.OfType<GroundPickResult>().FirstOrDefault();
		WidgetPickResult widgetPickResult = FindNearestWidgetPicked(enumerable);
		if (e.Button != MouseButtons.Right)
		{
			return;
		}
		if (widgetPickResult != null)
		{
			if (widgetPickResult.Widget == m_SplineWidget)
			{
				if (m_Spline == null)
				{
					return;
				}
				BugSubmitter.SilentAssert(m_SplineWidget != null, "Adding control point while no spline widget is active! @assign bwhitman");
				SplineWidgetPickResult swpr = widgetPickResult as SplineWidgetPickResult;
				if (swpr == null)
				{
					BugSubmitter.SilentReport("Picked a spline widget but did not get a SplineWidgetPick in response @assign bwhitman");
					return;
				}
				BugSubmitter.SilentAssert(swpr.SplineParameter >= 0f, "Adding control point negative distance along the spline! @assign bwhitman");
				if (swpr.SplineParameter < (float)(m_Spline.SplineVertexSet.Vertices.Count - 1))
				{
					AddUndoTransaction("Insert Vertex", m_Spline, delegate
					{
						m_Spline.InsertVertex((int)Math.Ceiling(swpr.SplineParameter), swpr.Point);
					});
				}
				else
				{
					AddUndoTransaction("Append Vertex", m_Spline, delegate
					{
						m_Spline.AppendVertex(swpr.Point);
					});
				}
				return;
			}
			ISplineVertex splineVertex = m_Spline.SplineVertexSet.Vertices.FirstOrDefault()?.SplineVertex;
			if (widgetPickResult.Widget?.BoundObject is ISplineVertex splineVertex2 && splineVertex2 == splineVertex && !Spline.ClosedLoop)
			{
				AddUndoTransaction("Close Loop", m_Spline, delegate
				{
					Spline.ClosedLoop = true;
				});
			}
		}
		else if (groundPick != null && m_Spline != null)
		{
			AddUndoTransaction("Add Vertex", m_Spline, delegate
			{
				m_Spline.AppendVertex(groundPick.Point);
			});
		}
	}

	public void Dispose()
	{
		DestroyWidgets();
	}

	public void SetHighlightedWidget(IWidget w)
	{
		if (m_LastWidgetHovered != w)
		{
			if (m_LastWidgetHovered != null)
			{
				IValueSet valueSet = m_CivTechFactory.CreateInstance<IValueSet>();
				valueSet.Push<IFloatValue>("Radius").ParameterValue = 0.2f;
				valueSet.Push<IRGBValue>("Color").ParameterValue = Color.FromArgb(255, 0, 196, 196);
				m_LastWidgetHovered.Alter(valueSet);
			}
			if (w != null)
			{
				IValueSet valueSet2 = m_CivTechFactory.CreateInstance<IValueSet>();
				valueSet2.Push<IFloatValue>("Radius").ParameterValue = 0.3f;
				valueSet2.Push<IRGBValue>("Color").ParameterValue = Color.FromArgb(255, 255, 0, 0);
				w.Alter(valueSet2);
			}
			m_LastWidgetHovered = w;
		}
	}

	public IWidget FindNearestVertexWidgetPicked(IEnumerable<PickResult> picks)
	{
		WidgetPickResult widgetPickResult = null;
		foreach (WidgetPickResult item in picks.OfType<WidgetPickResult>())
		{
			if (m_VertexLocators.Contains(item.Widget) && (widgetPickResult == null || item.DistanceToRayImpact < widgetPickResult.DistanceToRayImpact))
			{
				widgetPickResult = item;
			}
		}
		return widgetPickResult?.Widget;
	}

	public WidgetPickResult FindNearestWidgetPicked(IEnumerable<PickResult> picks)
	{
		WidgetPickResult widgetPickResult = null;
		foreach (WidgetPickResult item in picks.OfType<WidgetPickResult>())
		{
			if (widgetPickResult == null || item.DistanceToRayImpact < widgetPickResult.DistanceToRayImpact)
			{
				widgetPickResult = item;
			}
		}
		return widgetPickResult;
	}

	private void CreateWidgets(SplineAdapter splineAdapter)
	{
		if (m_Window == null)
		{
			return;
		}
		IValueSet valueSet = m_CivTechFactory.CreateInstance<IValueSet>();
		valueSet.Push<IStringValue>("SplineName").ParameterValue = splineAdapter.Name;
		m_SplineWidget = m_Window.CreateWidget("Spline", valueSet, splineAdapter.Spline);
		foreach (SplineVertexAdapter vertex in splineAdapter.SplineVertexSet.Vertices)
		{
			vertex.ItemChanged -= SplineVertex_ItemChanged;
			vertex.ItemChanged += SplineVertex_ItemChanged;
			m_VertexLocators.Add(CreateVertexLocator(vertex.SplineVertex));
		}
	}

	private IWidget CreateVertexLocator(ISplineVertex vert)
	{
		IValueSet valueSet = m_CivTechFactory.CreateInstance<IValueSet>();
		valueSet.Push<ICoord3DValue>("Position").ParameterValue = vert.Position;
		valueSet.Push<IFloatValue>("Radius").ParameterValue = 0.2f;
		valueSet.Push<IRGBValue>("Color").ParameterValue = Color.FromArgb(255, 0, 196, 196);
		return m_Window?.CreateWidget("Locator", valueSet, vert);
	}

	private void DestroyWidgets()
	{
		DestroyVertexPusher();
		SetHighlightedWidget(null);
		foreach (IWidget vertexLocator in m_VertexLocators)
		{
			vertexLocator?.Dispose();
		}
		m_SplineWidget?.Dispose();
		m_VertexLocators.Clear();
		m_SplineWidget = null;
	}

	private void CreateVertexPusher(IWidget locator)
	{
		BugSubmitter.SilentAssert(m_VertexPusher == null, "SplineWidgetEditor attempted to create vertex pusher when one was already active! @assign bwhitman");
		BugSubmitter.SilentAssert(m_Window != null, "SplineWidgetEditor attempted to create vertex pusher and no previewer window is present! @assign bwhitman");
		ISplineVertex boundVertex = locator.BoundObject as ISplineVertex;
		BugSubmitter.SilentAssert(boundVertex != null, "SplineWidgetEditor attempted to create vertex pusher but locator widget is not bound to a spline vertex! @assign bwhitman");
		SelectedVertex = Spline.SplineVertexSet.Vertices.FirstOrDefault((SplineVertexAdapter vert) => vert.SplineVertex == boundVertex);
		BugSubmitter.SilentAssert(SelectedVertex != null, "SplineWidgetEditor attempted to create vertex pusher but ISplineVertex bound to widget was not found in dom adapters! @assign bwhitman");
		m_VertexPusher = new SplineMoveWidget(Spline, SelectedVertex, locator, m_Window, is3D: true, IsReadOnly);
	}

	private void DestroyVertexPusher()
	{
		if (m_VertexPusher == null)
		{
			BugSubmitter.SilentAssert(SelectedVertex == null, "Orphaned SelectedVertex in SplineWidgetEditor!");
			return;
		}
		SelectedVertex = null;
		m_VertexPusher.Dispose();
		m_VertexPusher = null;
	}
}
