using System;
using System.ComponentModel;
using System.Globalization;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

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

	public AttributeFieldWithLabelPropertyDescriptor(string name, AttributeInfo labelAttributeInfo, AttributeInfo valueAttributeInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, TypeConverter labelTypeConverter)
		: base(name, valueAttributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_labelAttributeInfo = labelAttributeInfo;
		m_labelTypeConverter = labelTypeConverter;
	}

	public override string GetDisplayName(object component)
	{
		object attribute = component.As<DomNode>().GetAttribute(m_labelAttributeInfo);
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
