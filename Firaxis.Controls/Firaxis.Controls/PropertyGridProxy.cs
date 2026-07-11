using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Firaxis.Controls;

public class PropertyGridProxy : ICustomTypeDescriptor
{
	private class ProxyPropertyDescriptor : PropertyDescriptor
	{
		private PropertyGridProxy proxy;

		private PropertyDescriptor desc;

		public override Type ComponentType => desc.ComponentType;

		public override bool IsReadOnly => desc.IsReadOnly;

		public override Type PropertyType => desc.PropertyType;

		public ProxyPropertyDescriptor(PropertyGridProxy proxy, PropertyDescriptor desc, Attribute[] attribs)
			: base(desc.Name, attribs)
		{
			this.proxy = proxy;
			this.desc = desc;
		}

		public override bool CanResetValue(object component)
		{
			return desc.CanResetValue(component);
		}

		public override object GetValue(object component)
		{
			return desc.GetValue(component);
		}

		public override void ResetValue(object component)
		{
			desc.ResetValue(component);
		}

		public override void SetValue(object component, object value)
		{
			proxy.OnPropertyValueChanging(EventArgs.Empty);
			if (desc.Converter != null && !desc.PropertyType.IsInstanceOfType(value))
			{
				TypeConverter typeConverter = desc.Converter;
				desc.SetValue(component, typeConverter.ConvertFrom(value));
			}
			else
			{
				desc.SetValue(component, value);
			}
			proxy.OnPropertyValueChanged(EventArgs.Empty);
		}

		public override bool ShouldSerializeValue(object component)
		{
			return desc.ShouldSerializeValue(component);
		}
	}

	private object obj;

	[Browsable(false)]
	public object Object => obj;

	public event EventHandler PropertyValueChanging;

	public event EventHandler PropertyValueChanged;

	public PropertyGridProxy(object obj)
	{
		this.obj = obj;
	}

	public PropertyDescriptorCollection GetProperties()
	{
		return GetProperties(new Attribute[0]);
	}

	public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
		foreach (PropertyDescriptor item2 in properties)
		{
			List<Attribute> list2 = new List<Attribute>();
			foreach (Attribute attribute in item2.Attributes)
			{
				list2.Add(attribute);
			}
			list.Add(new ProxyPropertyDescriptor(this, item2, list2.ToArray()));
		}
		return new PropertyDescriptorCollection(list.ToArray());
	}

	protected virtual void OnPropertyValueChanging(EventArgs e)
	{
		this.PropertyValueChanging?.Invoke(this, e);
	}

	protected virtual void OnPropertyValueChanged(EventArgs e)
	{
		this.PropertyValueChanged?.Invoke(this, e);
	}

	public AttributeCollection GetAttributes()
	{
		return TypeDescriptor.GetAttributes(obj, noCustomTypeDesc: true);
	}

	public string GetClassName()
	{
		return TypeDescriptor.GetClassName(obj, noCustomTypeDesc: true);
	}

	public string GetComponentName()
	{
		return TypeDescriptor.GetComponentName(obj, noCustomTypeDesc: true);
	}

	public TypeConverter GetConverter()
	{
		return TypeDescriptor.GetConverter(obj, noCustomTypeDesc: true);
	}

	public EventDescriptor GetDefaultEvent()
	{
		return TypeDescriptor.GetDefaultEvent(obj, noCustomTypeDesc: true);
	}

	public PropertyDescriptor GetDefaultProperty()
	{
		return TypeDescriptor.GetDefaultProperty(obj);
	}

	public object GetEditor(Type editorBaseType)
	{
		return TypeDescriptor.GetEditor(obj, editorBaseType, noCustomTypeDesc: true);
	}

	public EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		return TypeDescriptor.GetEvents(obj, attributes, noCustomTypeDesc: true);
	}

	public EventDescriptorCollection GetEvents()
	{
		return TypeDescriptor.GetEvents(obj, noCustomTypeDesc: true);
	}

	public object GetPropertyOwner(PropertyDescriptor pd)
	{
		return obj;
	}
}
