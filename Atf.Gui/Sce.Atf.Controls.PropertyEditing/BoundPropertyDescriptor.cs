using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Sce.Atf.Controls.PropertyEditing;

public class BoundPropertyDescriptor : PropertyDescriptor
{
	private object m_owner;

	private Type m_ownerType;

	private PropertyInfo m_propertyInfo;

	private bool m_readOnly;

	private object m_editor;

	private TypeConverter m_typeConverter;

	public object Owner => m_owner;

	public override Type ComponentType => m_ownerType;

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

	public BoundPropertyDescriptor(object owner, Expression<Func<object>> expression, string displayName, string category, string description)
		: this(displayName, category, description)
	{
		PropertyInfo propertyInfo = GetPropertyInfo(expression);
		Init(owner, null, null, propertyInfo, null, null);
	}

	public BoundPropertyDescriptor(Type ownerType, Expression<Func<object>> expression, string displayName, string category, string description)
		: this(displayName, category, description)
	{
		PropertyInfo propertyInfo = GetPropertyInfo(expression);
		Init(null, ownerType, null, propertyInfo, null, null);
	}

	public BoundPropertyDescriptor(object owner, Expression<Func<object>> expression, string displayName, string category, string description, object editor, TypeConverter converter)
		: this(displayName, category, description)
	{
		PropertyInfo propertyInfo = GetPropertyInfo(expression);
		Init(owner, null, null, propertyInfo, editor, converter);
	}

	public BoundPropertyDescriptor(Type ownerType, Expression<Func<object>> expression, string displayName, string category, string description, object editor, TypeConverter converter)
		: this(displayName, category, description)
	{
		PropertyInfo propertyInfo = GetPropertyInfo(expression);
		Init(null, ownerType, null, propertyInfo, editor, converter);
	}

	public BoundPropertyDescriptor(object owner, string name, string displayName, string category, string description)
		: this(displayName, category, description)
	{
		Init(owner, null, name, null, null, null);
	}

	public BoundPropertyDescriptor(object owner, string name, string displayName, string category, string description, object editor)
		: this(displayName, category, description)
	{
		Init(owner, null, name, null, editor, null);
	}

	public BoundPropertyDescriptor(object owner, string name, string displayName, string category, string description, object editor, TypeConverter converter)
		: this(displayName, category, description)
	{
		Init(owner, null, name, null, editor, converter);
	}

	public BoundPropertyDescriptor(Type ownerType, string name, string displayName, string category, string description)
		: this(displayName, category, description)
	{
		Init(null, ownerType, name, null, null, null);
	}

	public BoundPropertyDescriptor(Type ownerType, string name, string displayName, string category, string description, object editor)
		: this(displayName, category, description)
	{
		Init(null, ownerType, name, null, editor, null);
	}

	public BoundPropertyDescriptor(Type ownerType, string name, string displayName, string category, string description, object editor, TypeConverter converter)
		: this(displayName, category, description)
	{
		Init(null, ownerType, name, null, editor, converter);
	}

	private void Init(object owner, Type ownerType, string name, PropertyInfo propertyInfo, object editor, TypeConverter converter)
	{
		m_owner = owner;
		if (owner != null)
		{
			ownerType = owner.GetType();
		}
		m_ownerType = ownerType;
		if (string.IsNullOrEmpty(name))
		{
			if (propertyInfo == null)
			{
				throw new ArgumentException("either 'name' or 'propertyInfo' must be non-null");
			}
			name = propertyInfo.Name;
		}
		m_propertyInfo = propertyInfo;
		if (m_propertyInfo == null)
		{
			m_propertyInfo = m_ownerType.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			if (m_propertyInfo == null)
			{
				throw new ArgumentException(name + ":  Property doesn't exist");
			}
		}
		MethodInfo method = m_ownerType.GetMethod("set_" + name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
		m_readOnly = method == null;
		m_editor = editor;
		m_typeConverter = converter;
	}

	private BoundPropertyDescriptor(string displayName, string category, string description)
		: base(displayName, new Attribute[2]
		{
			new CategoryAttribute(category),
			new DescriptionAttribute(description)
		})
	{
	}

	public override bool CanResetValue(object component)
	{
		object result;
		return GetDefaultValue(out result) && !object.Equals(GetValue(null), result);
	}

	public override void ResetValue(object component)
	{
		GetDefaultValue(out var result);
		SetValue(component, result);
	}

	public override bool ShouldSerializeValue(object component)
	{
		object value = GetValue(component);
		if (!GetDefaultValue(out var result) && value == null)
		{
			return false;
		}
		return !value.Equals(result);
	}

	public override object GetValue(object component)
	{
		if (m_owner != null)
		{
			component = m_owner;
		}
		return m_propertyInfo.GetValue(component, null);
	}

	public override void SetValue(object component, object value)
	{
		if (m_owner != null)
		{
			component = m_owner;
		}
		m_propertyInfo.SetValue(component, value, null);
	}

	public virtual bool GetDefaultValue(out object result)
	{
		bool result2 = false;
		result = null;
		object[] customAttributes = m_propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			result2 = true;
			result = (customAttributes[0] as DefaultValueAttribute).Value;
			if (result != null && result.GetType() != m_propertyInfo.PropertyType)
			{
				TypeConverter typeConverter = TypeDescriptor.GetConverter(result);
				if (typeConverter.CanConvertTo(m_propertyInfo.PropertyType))
				{
					result = typeConverter.ConvertTo(result, m_propertyInfo.PropertyType);
				}
				else
				{
					typeConverter = TypeDescriptor.GetConverter(m_propertyInfo.PropertyType);
					if (typeConverter.CanConvertFrom(result.GetType()))
					{
						result = typeConverter.ConvertFrom(result);
					}
				}
			}
		}
		return result2;
	}

	public override object GetEditor(Type editorBaseType)
	{
		if (m_editor != null && editorBaseType.IsInstanceOfType(m_editor))
		{
			return m_editor;
		}
		return base.GetEditor(editorBaseType);
	}

	private static PropertyInfo GetPropertyInfo(Expression<Func<object>> expression)
	{
		PropertyInfo propertyInfo = null;
		if (expression.Body is MemberExpression memberExpression)
		{
			propertyInfo = memberExpression.Member as PropertyInfo;
		}
		else if (expression.Body is UnaryExpression { Operand: MemberExpression operand })
		{
			propertyInfo = operand.Member as PropertyInfo;
		}
		if (propertyInfo == null)
		{
			throw new ArgumentException("lambda expression was not properly formed. Should be \"() => myObject.MyProperty\" or \"() => MyClass.MyProperty\"");
		}
		return propertyInfo;
	}
}
