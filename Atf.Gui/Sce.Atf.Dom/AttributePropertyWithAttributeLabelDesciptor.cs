using System;
using System.ComponentModel;
using System.Globalization;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class AttributePropertyWithAttributeLabelDesciptor : AttributePropertyDescriptor, ICustomPropertyDisplayName, INonCacheableDescriptor
{
	private AttributeInfo m_labelAttributeInfo;

	private TypeConverter m_labelTypeConverter;

	public AttributePropertyWithAttributeLabelDesciptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, bool isReadOnly)
		: this(name, labelAttributeInfo, valueAttributeInfo, category, description, isReadOnly, null)
	{
	}

	public AttributePropertyWithAttributeLabelDesciptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, labelAttributeInfo, valueAttributeInfo, category, description, isReadOnly, editor, null)
	{
	}

	public AttributePropertyWithAttributeLabelDesciptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: this(name, labelAttributeInfo, valueAttributeInfo, category, description, isReadOnly, editor, typeConverter, null)
	{
	}

	public AttributePropertyWithAttributeLabelDesciptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, TypeConverter labelTypeConverter)
		: base(name, valueAttributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_labelAttributeInfo = labelAttributeInfo;
		m_labelTypeConverter = labelTypeConverter;
	}

	public virtual string GetDisplayName(object component)
	{
		DomNode domNode = component.As<DomNode>();
		object attribute = domNode.GetAttribute(m_labelAttributeInfo);
		string text = string.Empty;
		if (attribute != null)
		{
			TypeConverter labelTypeConverter = m_labelTypeConverter;
			if (labelTypeConverter != null && labelTypeConverter.CanConvertTo(typeof(string)))
			{
				text = labelTypeConverter.ConvertTo(attribute, typeof(string)) as string;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = ((!(attribute is IFormattable formattable)) ? attribute.ToString() : formattable.ToString(null, CultureInfo.CurrentUICulture));
			}
		}
		return text;
	}
}
