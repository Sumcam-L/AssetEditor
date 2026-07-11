using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class SplineAdapter : AssetComponentAdapterBase, IFieldContainerAdapter, IObservableContext
{
	public ISpline Spline { get; set; }

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.SplineType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SplineType.NameAttribute, value);
		}
	}

	public string ClassName
	{
		get
		{
			return GetAttribute<string>(EntitySchema.SplineType.ClassNameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SplineType.ClassNameAttribute, value);
		}
	}

	public bool ClosedLoop
	{
		get
		{
			return GetAttribute<bool>(EntitySchema.SplineType.ClosedLoopAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.SplineType.ClosedLoopAttribute, value);
		}
	}

	public SplineVertexSetAdapter SplineVertexSet { get; private set; }

	public CookParameterSetAdapter CookParameterSet { get; private set; }

	public IList<IFieldValueAdapter> Fields => CookParameterSet?.Fields;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public SplineAdapter()
	{
		if (this.ItemInserted == null && this.ItemRemoved == null)
		{
			_ = this.Reloaded;
		}
	}

	public CookParameterSetAdapter InitializeCookParameters(IClassEntity config)
	{
		CookParameterSet = CreateCookParameterSet();
		AddParametersFromConfig(config, CookParameterSet);
		return CookParameterSet;
	}

	public void RemoveCookParameters()
	{
		CookParameterSet.Fields.Clear();
	}

	public SplineVertexAdapter AppendVertex(float[] pos)
	{
		DomNode domNode = new DomNode(EntitySchema.SplineVertexType.Type);
		domNode.InitializeExtensions();
		SplineVertexAdapter splineVertexAdapter = domNode.As<SplineVertexAdapter>();
		SplineVertexSet.Vertices.Add(splineVertexAdapter);
		splineVertexAdapter.Position = pos;
		return splineVertexAdapter;
	}

	public SplineVertexAdapter InsertVertex(int index, float[] pos)
	{
		DomNode domNode = new DomNode(EntitySchema.SplineVertexType.Type);
		domNode.InitializeExtensions();
		SplineVertexAdapter splineVertexAdapter = domNode.As<SplineVertexAdapter>();
		SplineVertexSet.Vertices.Insert(index, splineVertexAdapter);
		splineVertexAdapter.Position = pos;
		return splineVertexAdapter;
	}

	private void RegisterForDomNotifications()
	{
		UnregisterForDomNotifications();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
	}

	private void UnregisterForDomNotifications()
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		base.DomNode.ChildInserted -= DomNode_ChildInserted;
		base.DomNode.ChildRemoved -= DomNode_ChildRemoved;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		SplineVertexSet = this.CreateComponentAdapter<SplineVertexSetAdapter>(EntitySchema.SplineVertexSetType.Type, EntitySchema.SplineType.VerticesChild);
		RegisterForDomNotifications();
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == EntitySchema.SplineType.NameAttribute)
		{
			Spline.Name = (string)e.NewValue;
			this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		}
		else if (e.AttributeInfo == EntitySchema.SplineType.ClosedLoopAttribute)
		{
			Spline.ClosedLoop = (bool)e.NewValue;
			this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		}
		if (base.AssetAdapter != null)
		{
			base.AssetAdapter.DomNode.As<AssetContext>().BatchChangelist?.CreateEntityChangedEvent(base.AssetAdapter.Asset.Type, base.AssetAdapter.Asset.Name);
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == EntitySchema.SplineType.CookParametersChild && e.Parent == base.DomNode)
		{
			e.Child.As<CookParameterSetAdapter>().ValueSet = Spline.CookParameters;
		}
		base.AssetAdapter.DomNode.As<AssetContext>().BatchChangelist?.CreateEntityChangedEvent(base.AssetAdapter.Asset.Type, base.AssetAdapter.Asset.Name);
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		base.AssetAdapter.DomNode.As<AssetContext>().BatchChangelist?.CreateEntityChangedEvent(base.AssetAdapter.Asset.Type, base.AssetAdapter.Asset.Name);
	}

	private SplineVertexAdapter CreateSplineVertex()
	{
		DomNode domNode = new DomNode(EntitySchema.SplineVertexType.Type);
		domNode.InitializeExtensions();
		return domNode.As<SplineVertexAdapter>();
	}

	private CookParameterSetAdapter CreateCookParameterSet()
	{
		DomNode domNode = new DomNode(EntitySchema.CookParametersSetType.Type);
		domNode.InitializeExtensions();
		CookParameterSetAdapter cookParameterSetAdapter = domNode.As<CookParameterSetAdapter>();
		cookParameterSetAdapter.EntityAdapter = base.EntityAdapter;
		base.DomNode.SetChild(EntitySchema.SplineType.CookParametersChild, domNode);
		return cookParameterSetAdapter;
	}

	private void AddParametersFromConfig(IClassEntity config, CookParameterSetAdapter cookParams)
	{
		if (!(config is ISplineClass splineClass))
		{
			return;
		}
		foreach (IParameter item in splineClass.CookParameters.Items)
		{
			cookParams.AddParameter(item, CookParameterChanged);
		}
	}

	private void CookParameterChanged(object sender, AttributeEventArgs e)
	{
	}

	public void InitializeFromNative(ISpline spline)
	{
		UnregisterForDomNotifications();
		Spline = spline;
		ClassName = spline.ClassName;
		Name = spline.Name;
		ClosedLoop = spline.ClosedLoop;
		SplineVertexSet.InitializeFromNative(Spline.Vertices);
		if (CookParameterSet == null)
		{
			CookParameterSet = CreateCookParameterSet();
		}
		if (base.EntityAdapter.CivTechService.PrimaryProject.Config.Classes.Items.FirstOrDefault((IClassEntity cls) => cls.Name == Spline.ClassName) is ISplineClass splineClass)
		{
			CookParameterSet.Update(spline.CookParameters, splineClass.CookParameters, CookParameterChanged, updateUI: false);
		}
		RegisterForDomNotifications();
	}
}
