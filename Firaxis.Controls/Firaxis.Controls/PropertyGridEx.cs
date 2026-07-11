using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using Firaxis.Utility;

namespace Firaxis.Controls;

public class PropertyGridEx : PropertyGrid, IServiceProviderProvider
{
	private bool readOnly = false;

	private string filter = "";

	private bool inSelected = false;

	private List<object> pushSelected = new List<object>();

	[Category("Behavior")]
	[Description("Set to True for the Property Grid disable all editing")]
	public bool ReadOnly
	{
		get
		{
			return readOnly;
		}
		set
		{
			readOnly = value;
			UpdateReadOnly();
		}
	}

	[Category("Behavior")]
	[Description("Filter property values separated by semicolons")]
	public string Filter
	{
		get
		{
			return filter;
		}
		set
		{
			filter = value;
			Refresh();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IServiceProvider ServiceProvider { get; set; }

	protected override void OnSelectedObjectsChanged(EventArgs e)
	{
		UpdateReadOnly();
		base.OnSelectedObjectsChanged(e);
	}

	public void BeginSelected()
	{
		inSelected = true;
		pushSelected.Clear();
	}

	public void PushSelected(IEnumerable enumerable)
	{
		foreach (object item in enumerable)
		{
			PushSelected(item);
		}
	}

	public void PushSelected(object obj)
	{
		if (inSelected)
		{
			if (obj is IPropertyGridDescriptor propertyGridDescriptor)
			{
				propertyGridDescriptor.OnEnterDescriptor(this);
			}
			else
			{
				pushSelected.Add(obj);
			}
		}
	}

	public void EndSelected()
	{
		inSelected = false;
		base.SelectedObjects = pushSelected.ToArray();
		UpdateReadOnly();
		Refresh();
	}

	private void UpdateReadOnly()
	{
		object[] selectedObjects = base.SelectedObjects;
		if (selectedObjects == null)
		{
			return;
		}
		object[] array = selectedObjects;
		foreach (object obj in array)
		{
			object obj2 = ((obj is PropertyGridProxy) ? ((PropertyGridProxy)obj).Object : obj);
			Type type = obj2.GetType();
			if (type.IsDefined(typeof(ReadOnlyAttribute), inherit: true))
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(ReadOnlyAttribute), inherit: true);
				if (customAttributes != null && customAttributes.Length != 0)
				{
					FieldInfo field = customAttributes[0].GetType().GetField("isReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
					field.SetValue(customAttributes[0], readOnly, BindingFlags.Instance | BindingFlags.NonPublic, null, null);
				}
			}
			else
			{
				try
				{
					TypeDescriptor.AddAttributes(obj2, new ReadOnlyAttribute(readOnly));
				}
				catch
				{
				}
			}
		}
	}

	public T GetService<T>()
	{
		if (ServiceProvider != null)
		{
			return (T)ServiceProvider.GetService(typeof(T));
		}
		return default(T);
	}
}
