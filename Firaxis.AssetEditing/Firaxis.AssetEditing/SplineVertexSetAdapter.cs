using System;
using System.Collections.Generic;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class SplineVertexSetAdapter : DomNodeAdapter, IObservableContext
{
	public IList<SplineVertexAdapter> Vertices { get; private set; }

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public SplineVertexSetAdapter()
	{
		if (this.ItemChanged != null)
		{
			_ = this.Reloaded;
		}
	}

	private void RegisterForDomNotifications()
	{
		UnregisterForDomNotifications();
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterForDomNotifications()
	{
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		RegisterForDomNotifications();
		Vertices = new DomNodeListAdapter<SplineVertexAdapter>(base.DomNode, EntitySchema.SplineVertexSetType.SplineVertexChild);
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.SplineVertexSetType.SplineVertexChild)
		{
			SplineAdapter splineAdapter = base.DomNode.Parent.As<SplineAdapter>();
			SplineVertexAdapter splineVertexAdapter = e.Child.As<SplineVertexAdapter>();
			if (e.Index < splineAdapter.Spline.VertexCount)
			{
				splineVertexAdapter.SplineVertex = splineAdapter.Spline.InsertVertex(e.Index, splineVertexAdapter.Position);
			}
			else
			{
				BugSubmitter.SilentAssert(e.Index == splineAdapter.Spline.VertexCount, "@assign bwhitman @summary Dom index and Native index got out of sync while adding a spline vertex");
				splineVertexAdapter.SplineVertex = splineAdapter.Spline.AppendVertex(splineVertexAdapter.Position);
			}
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child));
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.SplineVertexSetType.SplineVertexChild)
		{
			ISpline spline = base.DomNode.Parent.As<SplineAdapter>().Spline;
			this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child));
			spline.RemoveVertex(e.Index);
		}
	}

	private SplineVertexAdapter CreateSplineVertex()
	{
		DomNode domNode = new DomNode(EntitySchema.SplineVertexType.Type);
		domNode.InitializeExtensions();
		return domNode.As<SplineVertexAdapter>();
	}

	public void InitializeFromNative(IEnumerable<ISplineVertex> vertices)
	{
		UnregisterForDomNotifications();
		Vertices.Clear();
		foreach (ISplineVertex vertex in vertices)
		{
			SplineVertexAdapter splineVertexAdapter = CreateSplineVertex();
			Vertices.Add(splineVertexAdapter);
			splineVertexAdapter.InitializeFromNative(vertex);
		}
		RegisterForDomNotifications();
	}
}
