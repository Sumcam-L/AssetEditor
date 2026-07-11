using System;
using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class EntityNameAttributePropertyDescriptor : AttributePropertyDescriptor
{
	public EntityNameAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly)
		: this(name, attribute, category, description, isReadOnly, null, null)
	{
	}

	public EntityNameAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor)
		: this(name, attribute, category, description, isReadOnly, editor, null)
	{
	}

	public EntityNameAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: this(name, attribute, category, description, isReadOnly, editor, typeConverter, null)
	{
	}

	public EntityNameAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, Attribute[] attributes)
		: base(name, attribute, category, description, isReadOnly, editor, typeConverter, attributes)
	{
	}

	public override void SetValue(object component, object value)
	{
		DomNode node = GetNode(component);
		if (node != null)
		{
			InstanceEntityAdapter instanceEntityAdapter = node.As<InstanceEntityAdapter>();
			string text = (string)value;
			string oldName = instanceEntityAdapter.Name;
			if (!instanceEntityAdapter.PreNameChange(text))
			{
				MessageBoxes.Show("Can not set the name of the entity to \"" + text + "\". Please pick a new name.", "Failed to change name", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			node.SetAttribute(base.AttributeInfo, value);
			instanceEntityAdapter.PostNameChange(oldName, text);
		}
	}
}
