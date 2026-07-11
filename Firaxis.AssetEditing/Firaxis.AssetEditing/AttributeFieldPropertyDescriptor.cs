using System;
using System.ComponentModel;
using Firaxis.ATF;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

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

	public AttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnlyFunctor, editor, typeConverter)
	{
		m_attributeInfo = attributeInfo;
	}

	public AttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor)
		: base(name, attributeInfo, category, description, isReadOnlyFunctor, editor, null)
	{
		m_attributeInfo = attributeInfo;
	}

	public AttributeFieldPropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, Func<bool> isReadOnlyFunctor)
		: base(name, attributeInfo, category, description, isReadOnlyFunctor, null, null)
	{
		m_attributeInfo = attributeInfo;
	}

	public override string GetCategoryName(object component)
	{
		return base.GetCategoryName(component);
	}

	public override string GetDisplayName(object component)
	{
		return base.GetDisplayName(component);
	}
}
