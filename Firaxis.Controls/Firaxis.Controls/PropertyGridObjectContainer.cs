using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class PropertyGridObjectContainer
{
	private class PGOCProxy : PropertyGridProxy
	{
		private PropertyGridObjectContainer container;

		public PGOCProxy(PropertyGridObjectContainer container, object obj)
			: base(obj)
		{
			this.container = container;
		}

		protected override void OnPropertyValueChanging(EventArgs e)
		{
			container.OnPropertyValueChanging(e);
			base.OnPropertyValueChanging(e);
		}

		protected override void OnPropertyValueChanged(EventArgs e)
		{
			container.OnPropertyValueChanged(e);
			base.OnPropertyValueChanged(e);
		}
	}

	private object[] objects;

	private List<PropertyGridProxy> proxies;

	public bool AutoPromote { get; set; }

	public PropertyGrid PropertyGrid { get; set; }

	public object SelectedObject
	{
		get
		{
			if (objects != null && objects.Length != 0)
			{
				return objects[0];
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				SelectedObjects = new object[0];
				return;
			}
			SelectedObjects = new object[1] { value };
		}
	}

	public object[] SelectedObjects
	{
		get
		{
			if (objects == null)
			{
				return new object[0];
			}
			return (object[])objects.Clone();
		}
		set
		{
			if (value == null)
			{
				objects = new object[0];
			}
			else
			{
				objects = (object[])value.Clone();
			}
			if (AutoPromote)
			{
				PromoteToGrid();
			}
		}
	}

	public event EventHandler PropertyValueChanging;

	public event EventHandler PropertyValueChanged;

	public PropertyGridObjectContainer()
		: this(null)
	{
	}

	public PropertyGridObjectContainer(PropertyGrid propertyGrid)
	{
		proxies = new List<PropertyGridProxy>();
		PropertyGrid = propertyGrid;
		AutoPromote = true;
	}

	protected virtual void OnPropertyValueChanging(EventArgs e)
	{
		this.PropertyValueChanging?.Invoke(this, e);
	}

	protected virtual void OnPropertyValueChanged(EventArgs e)
	{
		this.PropertyValueChanged?.Invoke(this, e);
	}

	public void PromoteToGrid()
	{
		if (PropertyGrid == null)
		{
			throw new NullReferenceException("PropertyGrid must be assigned");
		}
		BuildProxyList();
		if (PropertyGrid is PropertyGridEx)
		{
			PropertyGridEx propertyGridEx = (PropertyGridEx)PropertyGrid;
			propertyGridEx.BeginSelected();
			propertyGridEx.PushSelected(proxies);
			propertyGridEx.EndSelected();
		}
		else
		{
			PropertyGrid.SelectedObjects = proxies.ToArray();
		}
	}

	private void BuildProxyList()
	{
		proxies.Clear();
		object[] array = objects;
		foreach (object obj in array)
		{
			proxies.Add(new PGOCProxy(this, obj));
		}
	}
}
