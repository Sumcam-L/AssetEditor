using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class ListEditingContext : EditingContext, IInstancingContext, ILastHitAware, IObservableContext, INotifyPropertyChanged
{
	public object LastHit { get; set; }

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public event PropertyChangedEventHandler PropertyChanged;

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoving += DomNode_ChildRemoving;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		this.Reloaded.Raise(this, EventArgs.Empty);
	}

	public virtual bool CanCopy()
	{
		return false;
	}

	public virtual object Copy()
	{
		throw new NotSupportedException();
	}

	public virtual bool CanInsert(object dataObject)
	{
		return false;
	}

	public virtual void Insert(object dataObject)
	{
		throw new NotSupportedException();
	}

	public virtual bool CanDelete()
	{
		return base.Selection != null && base.Selection.Count > 0;
	}

	public virtual void Delete()
	{
		if (base.Selection != null)
		{
			DomNode[] array = GetSelection<DomNode>().ToArray();
			foreach (DomNode domNode in array)
			{
				domNode.RemoveFromParent();
			}
		}
	}

	protected void RaisePropertyChanged(string propertyName)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		this.PropertyChanged?.Invoke(this, e);
	}

	protected virtual int GetChildIndex(ChildEventArgs e)
	{
		return e.Index;
	}

	protected virtual void OnObjectInserted(ItemInsertedEventArgs<object> e)
	{
		this.ItemInserted.Raise(this, e);
	}

	protected virtual void OnObjectRemoving(ItemRemovedEventArgs<object> e)
	{
	}

	protected virtual void OnObjectRemoved(ItemRemovedEventArgs<object> e)
	{
		this.ItemRemoved.Raise(this, e);
	}

	protected virtual void OnObjectChanged(ItemChangedEventArgs<object> e)
	{
		this.ItemChanged.Raise(this, e);
	}

	protected virtual void OnReloaded(EventArgs e)
	{
		this.Reloaded.Raise(this, e);
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		OnObjectChanged(new ItemChangedEventArgs<object>(e.DomNode));
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		OnObjectInserted(new ItemInsertedEventArgs<object>(GetChildIndex(e), e.Child, e.Parent));
	}

	private void DomNode_ChildRemoving(object sender, ChildEventArgs e)
	{
		OnObjectRemoving(new ItemRemovedEventArgs<object>(GetChildIndex(e), e.Child, e.Parent));
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		OnObjectRemoved(new ItemRemovedEventArgs<object>(GetChildIndex(e), e.Child, e.Parent));
	}

	[Conditional("DEBUG")]
	private void CheckPropertyName(string propertyName)
	{
		System.ComponentModel.PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(this)[propertyName];
		if (propertyDescriptor == null)
		{
			throw new InvalidOperationException(string.Format(null, "The property with the propertyName '{0}' doesn't exist.", new object[1] { propertyName }));
		}
	}
}
