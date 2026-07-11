using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions.Calls;

namespace IronPython.Runtime.Operations;

public static class CustomTypeDescHelpers
{
	private class SuperDynamicObjectPropertyDescriptor : PropertyDescriptor
	{
		private string _name;

		private Type _propertyType;

		private Type _componentType;

		public override Type ComponentType => _componentType;

		public override bool IsReadOnly => false;

		public override Type PropertyType => _propertyType;

		internal SuperDynamicObjectPropertyDescriptor(string name, Type propertyType, Type componentType)
			: base(name, null)
		{
			_name = name;
			_propertyType = propertyType;
			_componentType = componentType;
		}

		public override object GetValue(object component)
		{
			return PythonOps.GetBoundAttr(DefaultContext.DefaultCLS, component, _name);
		}

		public override void SetValue(object component, object value)
		{
			PythonOps.SetAttr(DefaultContext.DefaultCLS, component, _name, value);
		}

		public override bool CanResetValue(object component)
		{
			return true;
		}

		public override void ResetValue(object component)
		{
			PythonOps.DeleteAttr(DefaultContext.DefaultCLS, component, _name);
		}

		public override bool ShouldSerializeValue(object component)
		{
			object ret;
			return PythonOps.TryGetBoundAttr(component, _name, out ret);
		}
	}

	private class TypeConv : TypeConverter
	{
		private object convObj;

		public TypeConv(object self)
		{
			convObj = self;
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			object result;
			return Converter.TryConvert(convObj, destinationType, out result);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return Converter.CanConvertFrom(sourceType, convObj.GetType(), NarrowingLevel.All);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return Converter.Convert(value, convObj.GetType());
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			return Converter.Convert(convObj, destinationType);
		}

		public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return false;
		}

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			object result;
			return Converter.TryConvert(value, convObj.GetType(), out result);
		}
	}

	public static AttributeCollection GetAttributes(object self)
	{
		return AttributeCollection.Empty;
	}

	public static string GetClassName(object self)
	{
		if (PythonOps.TryGetBoundAttr(DefaultContext.DefaultCLS, self, "__class__", out var ret))
		{
			return PythonOps.GetBoundAttr(DefaultContext.DefaultCLS, ret, "__name__").ToString();
		}
		return null;
	}

	public static string GetComponentName(object self)
	{
		return null;
	}

	public static TypeConverter GetConverter(object self)
	{
		return new TypeConv(self);
	}

	public static EventDescriptor GetDefaultEvent(object self)
	{
		return null;
	}

	public static PropertyDescriptor GetDefaultProperty(object self)
	{
		return null;
	}

	public static object GetEditor(object self, Type editorBaseType)
	{
		return null;
	}

	public static EventDescriptorCollection GetEvents(object self, Attribute[] attributes)
	{
		if (attributes == null || attributes.Length == 0)
		{
			return GetEvents(self);
		}
		return EventDescriptorCollection.Empty;
	}

	public static EventDescriptorCollection GetEvents(object self)
	{
		return EventDescriptorCollection.Empty;
	}

	public static PropertyDescriptorCollection GetProperties(object self)
	{
		return new PropertyDescriptorCollection(GetPropertiesImpl(self, new Attribute[0]));
	}

	public static PropertyDescriptorCollection GetProperties(object self, Attribute[] attributes)
	{
		return new PropertyDescriptorCollection(GetPropertiesImpl(self, attributes));
	}

	private static PropertyDescriptor[] GetPropertiesImpl(object self, Attribute[] attributes)
	{
		IList<object> attrNames = PythonOps.GetAttrNames(DefaultContext.DefaultCLS, self);
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		if (attrNames != null)
		{
			foreach (object item in attrNames)
			{
				if (!(item is string text))
				{
					continue;
				}
				PythonTypeSlot slot = null;
				object ret;
				if (self is OldInstance)
				{
					if (((OldInstance)self)._class.TryLookupSlot(text, out ret))
					{
						slot = ret as PythonTypeSlot;
					}
					else if (!((OldInstance)self).Dictionary.TryGetValue(text, out ret))
					{
						ret = ObjectOps.__getattribute__(DefaultContext.DefaultCLS, self, text);
					}
				}
				else
				{
					PythonType pythonType = DynamicHelpers.GetPythonType(self);
					pythonType.TryResolveSlot(DefaultContext.DefaultCLS, text, out slot);
					ret = ObjectOps.__getattribute__(DefaultContext.DefaultCLS, self, text);
				}
				Type propertyType = ((ret == null) ? typeof(NoneTypeOps) : ret.GetType());
				if ((slot != null && ShouldIncludeProperty(slot, attributes)) || (slot == null && ShouldIncludeInstanceMember(text, attributes)))
				{
					list.Add(new SuperDynamicObjectPropertyDescriptor(text, propertyType, self.GetType()));
				}
			}
		}
		return list.ToArray();
	}

	private static bool ShouldIncludeInstanceMember(string memberName, Attribute[] attributes)
	{
		bool result = true;
		foreach (Attribute attribute in attributes)
		{
			if (attribute.GetType() == typeof(BrowsableAttribute))
			{
				if (memberName.StartsWith("__") && memberName.EndsWith("__"))
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	private static bool ShouldIncludeProperty(PythonTypeSlot attrSlot, Attribute[] attributes)
	{
		bool flag = true;
		foreach (Attribute attribute in attributes)
		{
			if (attrSlot is ReflectedProperty reflectedProperty && reflectedProperty.Info != null)
			{
				flag &= reflectedProperty.Info.IsDefined(attribute.GetType(), inherit: true);
			}
			else if (attribute.GetType() == typeof(BrowsableAttribute))
			{
				if (!(attrSlot is PythonTypeUserDescriptorSlot pythonTypeUserDescriptorSlot))
				{
					if (!(attrSlot is PythonProperty))
					{
						flag = false;
					}
				}
				else if (!(pythonTypeUserDescriptorSlot.Value is IPythonObject))
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
		}
		return flag;
	}

	public static object GetPropertyOwner(object self, PropertyDescriptor pd)
	{
		return self;
	}
}
