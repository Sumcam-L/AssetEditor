using System;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class SplineVertexAdapter : DomNodeAdapter, IObservableContext
{
	public ISplineVertex SplineVertex { get; set; }

	public float[] Position
	{
		get
		{
			return GetAttribute<float[]>(EntitySchema.SplineVertexType.PositionAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SplineVertexType.PositionAttribute, value);
		}
	}

	public float[] LeftUserTangent
	{
		get
		{
			return GetAttribute<float[]>(EntitySchema.SplineVertexType.LeftUserTangentAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SplineVertexType.LeftUserTangentAttribute, value);
		}
	}

	public float[] RightUserTangent
	{
		get
		{
			return GetAttribute<float[]>(EntitySchema.SplineVertexType.RightUserTangentAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SplineVertexType.RightUserTangentAttribute, value);
		}
	}

	public bool SharpCorner
	{
		get
		{
			return GetAttribute<bool>(EntitySchema.SplineVertexType.SharpCornerAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SplineVertexType.SharpCornerAttribute, value);
		}
	}

	public bool UseUserTangents
	{
		get
		{
			return GetAttribute<bool>(EntitySchema.SplineVertexType.UseUserTangentsAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SplineVertexType.UseUserTangentsAttribute, value);
		}
	}

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public SplineVertexAdapter()
	{
		if (this.ItemRemoved != null && this.ItemInserted != null)
		{
			_ = this.Reloaded;
		}
	}

	private void RegisterForDomNotifications()
	{
		UnregisterForDomNotifications();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	private void UnregisterForDomNotifications()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		RegisterForDomNotifications();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.SplineVertexType.PositionAttribute)
		{
			float[] array = (float[])e.NewValue;
			SplineVertex.Position = new Point3F(array[0], array[1], array[2]);
			this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		}
		else if (e.AttributeInfo != EntitySchema.SplineVertexType.LeftUserTangentAttribute && e.AttributeInfo != EntitySchema.SplineVertexType.RightUserTangentAttribute && e.AttributeInfo != EntitySchema.SplineVertexType.UseUserTangentsAttribute && e.AttributeInfo == EntitySchema.SplineVertexType.SharpCornerAttribute)
		{
			SplineVertex.SharpCorner = (bool)e.NewValue;
		}
	}

	public void InitializeFromNative(ISplineVertex splineVert)
	{
		UnregisterForDomNotifications();
		SplineVertex = splineVert;
		Position = new float[3]
		{
			splineVert.Position.x,
			splineVert.Position.y,
			splineVert.Position.z
		};
		SharpCorner = splineVert.SharpCorner;
		RegisterForDomNotifications();
	}
}
