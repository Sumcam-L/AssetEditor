using System;
using System.ComponentModel;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefElementAttributePropertyDescriptor : AttributePropertyDescriptor
{
	public ArtDefElementAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly)
		: base(name, attribute, category, description, isReadOnly)
	{
	}

	public ArtDefElementAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, Attribute[] attributes)
		: base(name, attribute, category, description, isReadOnly, editor, typeConverter, attributes)
	{
	}
}
