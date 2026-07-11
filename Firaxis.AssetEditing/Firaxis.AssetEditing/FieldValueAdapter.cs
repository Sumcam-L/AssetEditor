using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public abstract class FieldValueAdapter : CustomTypeDescriptorNodeAdapter, IFieldValueAdapter, INamedAdapter, IObservableContext
{
	protected Func<bool> ReadOnlyFunctor = () => false;

	protected bool CollectionOwned;

	public virtual object DefaultDataAsObject => null;

	public virtual string Name
	{
		get
		{
			return GetAttribute<string>(FieldSchema.FieldValueType.NameAttribute);
		}
		set
		{
			SetAttribute(FieldSchema.FieldValueType.NameAttribute, value);
		}
	}

	public IParameter Parameter { get; internal set; }

	public abstract IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors { get; }

	public IValue Value { get; internal set; }

	public virtual object ValueDataAsObject
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	DomNode IFieldValueAdapter.DomNode => base.DomNode;

	IParameter IFieldValueAdapter.Parameter => Parameter;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler Reloaded;

	public FieldValueAdapter()
	{
	}

	public virtual void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		Value = FieldValueHelper.NativeAddElementField(valSet, valParam);
	}

	public virtual void AssignDefaultValue()
	{
	}

	public virtual void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		ReadOnlyFunctor = readOnlyFunctor;
	}

	public virtual bool RequiresUpdate(IValue val)
	{
		return Name != val.ParameterName;
	}

	public virtual void UpdateDomFromNative(IValue val)
	{
		Value = val;
		if (Name != val.ParameterName)
		{
			Name = val.ParameterName;
		}
	}

	public virtual void CopyValue(IValue val)
	{
		BugSubmitter.Assert(Value != null, "CopyValue is being called on an invalid adapter.");
		BugSubmitter.Assert(Parameter != null, "CopyValue is being called on an invalid adapter.");
		BugSubmitter.Assert(val.ParameterType == Value.ParameterType, "The source value is a different type than the destination value!");
	}

	public virtual void UpdateNativeFromDom()
	{
	}

	public virtual void Initialize(IParameter param)
	{
		if (param != null)
		{
			Name = param.Name;
		}
		Parameter = param;
	}

	protected ITransactionContext GetRootTransactionContext()
	{
		return base.DomNode.GetRoot().As<ITransactionContext>();
	}

	protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
	{
		return PropertyDescriptors.OfType<FieldPropertyDescriptorBase>().ToArray();
	}

	protected virtual void OnItemChanged(object item)
	{
		this.ItemChanged?.Invoke(this, new ItemChangedEventArgs<object>(item));
	}

	protected virtual void OnItemInserted(int index, object item)
	{
		this.ItemInserted?.Invoke(this, new ItemInsertedEventArgs<object>(index, item));
	}

	protected virtual void OnItemRemoved(int index, object item)
	{
		this.ItemRemoved?.Invoke(this, new ItemRemovedEventArgs<object>(index, item));
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		CollectionOwned = base.DomNode.Parent.Is<CollectionFieldValueAdapter>();
		InitializeEditor(() => false);
	}

	protected string BindDynamicValueOrDefault(string dynValue, string staticValue)
	{
		if (string.IsNullOrEmpty(dynValue))
		{
			return staticValue;
		}
		return dynValue;
	}

	protected bool HasNamedEmtries()
	{
		DomNode parent = base.DomNode.Parent;
		BugSubmitter.SilentAssert(parent != null || parent.Is<IDocument>(), "Parent node not set by InitializeEditor time!");
		CollectionFieldValueAdapter collectionFieldValueAdapter = parent.As<CollectionFieldValueAdapter>();
		if (collectionFieldValueAdapter == null)
		{
			return false;
		}
		if (collectionFieldValueAdapter.CollectionParameter != null)
		{
			return collectionFieldValueAdapter.CollectionParameter.HasNamedEntries;
		}
		return false;
	}

	protected bool IsCollectionOwned()
	{
		DomNode parent = base.DomNode.Parent;
		BugSubmitter.SilentAssert(parent != null || parent.Is<IDocument>(), "Parent node not set by InitializeEditor time!");
		return parent.Is<CollectionFieldValueAdapter>();
	}

	protected bool IsArtDefElementOwned()
	{
		DomNode parent = base.DomNode.Parent;
		BugSubmitter.SilentAssert(parent != null || parent.Is<IDocument>(), "Parent node not set by InitializeEditor time!");
		while (parent?.Parent != null)
		{
			if (parent.Is<ArtDefElementAdapter>())
			{
				return true;
			}
			parent = parent.Parent;
		}
		return false;
	}

	protected virtual FieldPropertyDescriptorBase CreateProxyPropertyDescriptorIfNeeded(FieldPropertyDescriptorBase proxyDescr, string fldName)
	{
		return CreateProxyPropertyDescriptorIfNeeded(proxyDescr, fldName, null);
	}

	protected virtual FieldPropertyDescriptorBase CreateProxyPropertyDescriptorIfNeeded(FieldPropertyDescriptorBase proxyDescr, string fldName, object editor)
	{
		if (!IsArtDefElementOwned())
		{
			return new ProxyPropertyDescriptor(proxyDescr, fldName, editor);
		}
		if (!IsCollectionOwned())
		{
			return new LookupFieldPropertyDescriptor(proxyDescr, fldName, editor);
		}
		return proxyDescr;
	}

	protected virtual void OnReloaded()
	{
		this.Reloaded?.Invoke(this, EventArgs.Empty);
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo != FieldSchema.FieldValueType.NameAttribute)
		{
			OnItemChanged(e.DomNode);
		}
	}
}
