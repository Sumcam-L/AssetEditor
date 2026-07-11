using System.ComponentModel;

namespace Sce.Atf.Dom;

public class AttributePropertyWithLabelDescriptor : AttributePropertyDescriptor, ICustomPropertyDisplayName, INonCacheableDescriptor
{
	public AttributePropertyWithLabelDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly)
		: base(name, attribute, category, description, isReadOnly, null, null)
	{
	}

	public AttributePropertyWithLabelDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor)
		: base(name, attribute, category, description, isReadOnly, editor, null)
	{
	}

	public AttributePropertyWithLabelDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, attribute, category, description, isReadOnly, editor, typeConverter, null)
	{
	}

	public string GetDisplayName(object component)
	{
		return Name;
	}
}
