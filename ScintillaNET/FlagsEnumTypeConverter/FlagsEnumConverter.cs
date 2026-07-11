using System;
using System.ComponentModel;
using System.Reflection;

namespace FlagsEnumTypeConverter;

internal class FlagsEnumConverter : EnumConverter
{
	protected class EnumFieldDescriptor : SimplePropertyDescriptor
	{
		private ITypeDescriptorContext fContext;

		public override AttributeCollection Attributes => new AttributeCollection(RefreshPropertiesAttribute.Repaint);

		public EnumFieldDescriptor(Type componentType, string name, ITypeDescriptorContext context)
			: base(componentType, name, typeof(bool))
		{
			fContext = context;
		}

		public override object GetValue(object component)
		{
			return ((int)component & (int)Enum.Parse(ComponentType, Name)) != 0;
		}

		public override void SetValue(object component, object value)
		{
			int num = ((!(bool)value) ? ((int)component & ~(int)Enum.Parse(ComponentType, Name)) : ((int)component | (int)Enum.Parse(ComponentType, Name)));
			FieldInfo field = component.GetType().GetField("value__", BindingFlags.Instance | BindingFlags.Public);
			field.SetValue(component, num);
			fContext.PropertyDescriptor.SetValue(fContext.Instance, component);
		}

		public override bool ShouldSerializeValue(object component)
		{
			return (bool)GetValue(component) != GetDefaultValue();
		}

		public override void ResetValue(object component)
		{
			SetValue(component, GetDefaultValue());
		}

		public override bool CanResetValue(object component)
		{
			return ShouldSerializeValue(component);
		}

		private bool GetDefaultValue()
		{
			object obj = null;
			string text = fContext.PropertyDescriptor.Name;
			Type type = fContext.PropertyDescriptor.ComponentType;
			DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)Attribute.GetCustomAttribute(type.GetProperty(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), typeof(DefaultValueAttribute));
			if (defaultValueAttribute != null)
			{
				obj = defaultValueAttribute.Value;
			}
			if (obj != null)
			{
				return ((int)obj & (int)Enum.Parse(ComponentType, Name)) != 0;
			}
			return false;
		}
	}

	public FlagsEnumConverter(Type type)
		: base(type)
	{
	}

	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		if (context != null)
		{
			Type type = value.GetType();
			string[] names = Enum.GetNames(type);
			Array array = Enum.GetValues(type);
			if (names != null)
			{
				PropertyDescriptorCollection propertyDescriptorCollection = new PropertyDescriptorCollection(null);
				for (int i = 0; i < names.Length; i++)
				{
					if ((int)array.GetValue(i) != 0 && names[i] != "All")
					{
						propertyDescriptorCollection.Add(new EnumFieldDescriptor(type, names[i], context));
					}
				}
				return propertyDescriptorCollection;
			}
		}
		return base.GetProperties(context, value, attributes);
	}

	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		if (context != null)
		{
			return true;
		}
		return base.GetPropertiesSupported(context);
	}

	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return false;
	}
}
