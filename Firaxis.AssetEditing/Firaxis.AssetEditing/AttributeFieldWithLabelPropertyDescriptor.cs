using System;
using System.ComponentModel;
using System.Globalization;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AttributeFieldWithLabelPropertyDescriptor : AttributeFieldPropertyDescriptor
{
	private AttributeInfo m_labelAttributeInfo;

	private TypeConverter m_labelTypeConverter;

	public AttributeFieldWithLabelPropertyDescriptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, bool isReadOnly)
		: this(name, labelAttributeInfo, valueAttributeInfo, category, description, isReadOnly, null, null, null)
	{
	}

	public AttributeFieldWithLabelPropertyDescriptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, labelAttributeInfo, valueAttributeInfo, category, description, isReadOnly, editor, null, null)
	{
	}

	public AttributeFieldWithLabelPropertyDescriptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor)
		: this(name, labelAttributeInfo, valueAttributeInfo, category, description, isReadOnlyFunctor, editor, null, null)
	{
	}

	public AttributeFieldWithLabelPropertyDescriptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, TypeConverter labelTypeConverter)
		: base(name, valueAttributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_labelAttributeInfo = labelAttributeInfo;
		m_labelTypeConverter = labelTypeConverter;
	}

	public AttributeFieldWithLabelPropertyDescriptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter typeConverter, TypeConverter labelTypeConverter)
		: base(name, valueAttributeInfo, category, description, isReadOnlyFunctor, editor, typeConverter)
	{
		m_labelAttributeInfo = labelAttributeInfo;
		m_labelTypeConverter = labelTypeConverter;
	}

	public override string GetDisplayName(object component)
	{
		if (component == null)
		{
			return string.Empty;
		}
		DomNode domNode = component.As<DomNode>();
		if (domNode == null)
		{
			return string.Empty;
		}
		object attribute = domNode.GetAttribute(m_labelAttributeInfo);
		if (attribute == null)
		{
			return string.Empty;
		}
		return ConvertToStringIfPossible(attribute) ?? FormatAsStringIfPossible(attribute);
	}

	private string ConvertToStringIfPossible(object value)
	{
		if (m_labelTypeConverter == null)
		{
			return null;
		}
		if (!m_labelTypeConverter.CanConvertTo(typeof(string)))
		{
			return null;
		}
		string text = m_labelTypeConverter.ConvertTo(value, typeof(string)) as string;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return text;
	}

	private string FormatAsStringIfPossible(object value)
	{
		if (value is IFormattable formattable)
		{
			return formattable.ToString(null, CultureInfo.CurrentUICulture);
		}
		return value.ToString();
	}
}
