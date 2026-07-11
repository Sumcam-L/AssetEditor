using System;
using System.ComponentModel;
using System.Globalization;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class ChildAttributePropertyWithLabelDescriptor : ChildAttributePropertyDescriptor, ICustomPropertyDisplayName, INonCacheableDescriptor
{
	private AttributeInfo m_labelAttributeInfo;

	private ChildInfo m_labelChildInfo;

	private TypeConverter m_labelTypeConverter;

	public ChildAttributePropertyWithLabelDescriptor(string name, AttributeInfo valueAttributeInfo, ChildInfo valueChildInfo, AttributeInfo labelAttributeInfo, ChildInfo labelChildInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, valueAttributeInfo, valueChildInfo, labelAttributeInfo, labelChildInfo, category, description, isReadOnly, editor, null, null)
	{
	}

	public ChildAttributePropertyWithLabelDescriptor(string name, AttributeInfo valueAttributeInfo, ChildInfo valueChildInfo, AttributeInfo labelAttributeInfo, ChildInfo labelChildInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, TypeConverter labelTypeConverter)
		: base(name, valueAttributeInfo, valueChildInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_labelAttributeInfo = labelAttributeInfo;
		m_labelChildInfo = labelChildInfo;
		m_labelTypeConverter = labelTypeConverter;
	}

	public virtual string GetDisplayName(object component)
	{
		DomNode labelNode = GetLabelNode(component);
		object attribute = labelNode.GetAttribute(m_labelAttributeInfo);
		string text = "";
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

	private DomNode GetLabelNode(object component)
	{
		DomNode child = component.As<DomNode>().GetChild(m_labelChildInfo);
		if (!child.Type.IsValid(m_labelAttributeInfo))
		{
			return null;
		}
		return child;
	}
}
