using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing;

public class CollectionEditingControl : ListBox, ICompositePropertyControl, ICacheablePropertyControl
{
	private readonly PropertyEditorControlContext m_context;

	private Point m_mouseDownLocation;

	public PropertyEditorControlContext Context => m_context;

	public virtual bool Cacheable => true;

	public event EventHandler<CompositeOpenedEventArgs> CompositeOpened;

	public CollectionEditingControl(PropertyEditorControlContext context)
	{
		m_context = context;
		AllowDrop = true;
		SelectionMode = SelectionMode.One;
		base.BorderStyle = BorderStyle.None;
		DoubleBuffered = true;
		RefreshList();
	}

	public int GetItemIndex(int y)
	{
		int val = y / ItemHeight + base.TopIndex;
		return Math.Min(val, base.Items.Count - 1);
	}

	public int GetInsertionIndex(int y)
	{
		int val = (y + ItemHeight / 2) / ItemHeight + base.TopIndex;
		return Math.Min(val, base.Items.Count);
	}

	public override void Refresh()
	{
		RefreshList();
		base.Refresh();
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		object itemObject = GetItemObject(e.Y);
		if (itemObject != null)
		{
			PropertyDescriptor[] descriptors = GetDescriptors(itemObject);
			OnPartOpened(new CompositeOpenedEventArgs(itemObject, descriptors));
		}
		base.OnMouseDoubleClick(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		m_mouseDownLocation = e.Location;
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		Size dragSize = SystemInformation.DragSize;
		if ((e.Button & MouseButtons.Left) != MouseButtons.None && (dragSize.Width < Math.Abs(m_mouseDownLocation.X - e.X) || dragSize.Height < Math.Abs(m_mouseDownLocation.Y - e.Y)))
		{
			object itemObject = GetItemObject(m_mouseDownLocation.Y);
			if (itemObject != null)
			{
				itemObject = ConvertDragDropItem(itemObject);
				if (itemObject != null)
				{
					DoDragDrop(itemObject, DragDropEffects.All | DragDropEffects.Link);
				}
			}
		}
		base.OnMouseMove(e);
	}

	protected virtual string GetItemString(object item)
	{
		return GetItemText(item);
	}

	protected virtual PropertyDescriptor[] GetDescriptors(object item)
	{
		return PropertyUtils.GetDefaultProperties2(item);
	}

	protected virtual object ConvertDragDropItem(object item)
	{
		return item;
	}

	protected virtual void OnPartOpened(CompositeOpenedEventArgs e)
	{
		this.CompositeOpened?.Invoke(this, e);
	}

	private void RefreshList()
	{
		object value = m_context.GetValue();
		if (value == null)
		{
			return;
		}
		ICollection collection = (ICollection)value;
		base.Items.Clear();
		foreach (object item in collection)
		{
			base.Items.Add(GetItemString(item));
		}
	}

	private object GetItemObject(int y)
	{
		object value = m_context.GetValue();
		if (value == null)
		{
			return null;
		}
		ICollection collection = (ICollection)value;
		int num = GetItemIndex(y);
		foreach (object item in collection)
		{
			if (num == 0)
			{
				return item;
			}
			num--;
		}
		return null;
	}
}
