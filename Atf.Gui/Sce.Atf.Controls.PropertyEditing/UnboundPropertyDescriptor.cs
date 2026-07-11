using System;
using System.ComponentModel;
using System.Reflection;

namespace Sce.Atf.Controls.PropertyEditing;

public class UnboundPropertyDescriptor : PropertyDescriptor
{
	private readonly Type m_type;

	private readonly PropertyInfo m_propertyInfo;

	private readonly object m_editor;

	private readonly TypeConverter m_typeConverter;

	private readonly bool m_readOnly;

	public override Type ComponentType => m_type;

	public override Type PropertyType => m_propertyInfo.PropertyType;

	public override bool IsReadOnly => m_readOnly;

	public override TypeConverter Converter
	{
		get
		{
			if (m_typeConverter != null)
			{
				return m_typeConverter;
			}
			return base.Converter;
		}
	}

	public UnboundPropertyDescriptor(Type type, string name, string displayName, string category, string description)
		: this(type, name, displayName, category, description, null, null)
	{
	}

	public UnboundPropertyDescriptor(Type type, string name, string displayName, string category, string description, object editor)
		: this(type, name, displayName, category, description, editor, null)
	{
	}

	public UnboundPropertyDescriptor(Type type, string name, string displayName, string category, string description, object editor, TypeConverter converter)
		: base(displayName, new Attribute[2]
		{
			new CategoryAttribute(category),
			new DescriptionAttribute(description)
		})
	{
		m_type = type;
		m_propertyInfo = m_type.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		if (m_propertyInfo == null)
		{
			throw new ArgumentException(name + ":  Property doesn't exist");
		}
		MethodInfo method = m_type.GetMethod("set_" + name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		m_readOnly = method == null;
		m_editor = editor;
		m_typeConverter = converter;
	}

	public override bool CanResetValue(object component)
	{
		object defaultValue = GetDefaultValue();
		return defaultValue != null && !object.Equals(GetValue(component), defaultValue);
	}

	public override void ResetValue(object component)
	{
		object defaultValue = GetDefaultValue();
		SetValue(component, defaultValue);
	}

	public override bool ShouldSerializeValue(object component)
	{
		object value = GetValue(component);
		object defaultValue = GetDefaultValue();
		if (defaultValue == null && value == null)
		{
			return false;
		}
		return !value.Equals(defaultValue);
	}

	public override object GetValue(object component)
	{
		return m_propertyInfo.GetValue(component, null);
	}

	public override void SetValue(object component, object value)
	{
		m_propertyInfo.SetValue(component, value, null);
	}

	public override object GetEditor(Type editorBaseType)
	{
		if (m_editor != null && editorBaseType.IsAssignableFrom(m_editor.GetType()))
		{
			return m_editor;
		}
		return base.GetEditor(editorBaseType);
	}

	private object GetDefaultValue()
	{
		object obj = null;
		object[] customAttributes = m_propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			obj = (customAttributes[0] as DefaultValueAttribute).Value;
			if (obj != null && obj.GetType() != m_propertyInfo.PropertyType)
			{
				obj = TypeDescriptor.GetConverter(obj).ConvertTo(obj, m_propertyInfo.PropertyType);
			}
		}
		return obj;
	}
}
