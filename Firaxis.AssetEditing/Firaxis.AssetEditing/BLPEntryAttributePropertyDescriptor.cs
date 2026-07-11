using System;
using System.ComponentModel;
using Firaxis.ATF;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class BLPEntryAttributePropertyDescriptor : AttributeFieldPropertyDescriptor
{
	public BLPEntryAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter typeConverter)
		: base(name, attributeInfo, category, description, isReadOnlyFunctor, editor, typeConverter)
	{
	}

	public BLPEntryAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor)
		: base(name, attributeInfo, category, description, isReadOnlyFunctor, editor, null)
	{
	}

	public BLPEntryAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, Func<bool> isReadOnlyFunctor)
		: base(name, attributeInfo, category, description, isReadOnlyFunctor, null, null)
	{
	}

	public override void SetValue(object component, object value)
	{
		DomNode node = GetNode(component);
		if (node != null)
		{
			node.As<BLPEntryFieldValueAdapter>();
			if (value is string)
			{
				node.SetAttribute(AttributeInfo, value);
			}
			else if (value is BLPData bLPData)
			{
				node.SetAttribute(AttributeInfo, bLPData.Name);
				node.SetAttribute(FieldSchema.BLPFieldValueType.XLPPathAttribute, bLPData.XLPPath);
				node.SetAttribute(FieldSchema.BLPFieldValueType.BLPPackageAttribute, bLPData.BLPPath);
			}
		}
	}
}
