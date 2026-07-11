using System;
using System.ComponentModel;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public class AttributeFieldPropertyDescriptor : FieldPropertyDescriptorBase
{
	private AttributeInfo m_attributeInfo;

	public override AttributeInfo AttributeInfo => m_attributeInfo;

	public override Type ClrType => AttributeInfo.Type.ClrType;

	public AttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, bool isReadOnly)
		: this(name, attributeInfo, category, description, isReadOnly, null, null)
	{
	}

	public AttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, bool isReadOnly, object editor)
		: this(name, attributeInfo, category, description, isReadOnly, editor, null)
	{
	}

	public AttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnly, editor, typeConverter)
	{
		m_attributeInfo = attributeInfo;
	}
}
