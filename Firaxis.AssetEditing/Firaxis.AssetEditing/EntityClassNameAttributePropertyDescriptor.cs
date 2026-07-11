using System;
using System.ComponentModel;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class EntityClassNameAttributePropertyDescriptor : AttributePropertyDescriptor
{
	private bool m_onlyNewCanChange;

	public EntityClassNameAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, bool onlyNewCanChange)
		: this(name, attribute, category, description, isReadOnly, null, null, onlyNewCanChange)
	{
	}

	public EntityClassNameAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, bool onlyNewCanChange)
		: this(name, attribute, category, description, isReadOnly, editor, null, onlyNewCanChange)
	{
	}

	public EntityClassNameAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, bool onlyNewCanChange)
		: this(name, attribute, category, description, isReadOnly, editor, typeConverter, null, onlyNewCanChange)
	{
	}

	public EntityClassNameAttributePropertyDescriptor(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter, Attribute[] attributes, bool onlyNewCanChange)
		: base(name, attribute, category, description, isReadOnly, editor, typeConverter, attributes)
	{
		m_onlyNewCanChange = onlyNewCanChange;
	}

	public override void SetValue(object component, object value)
	{
		DomNode node = GetNode(component);
		if (node != null)
		{
			InstanceEntityAdapter instanceEntityAdapter = node.As<InstanceEntityAdapter>();
			if (m_onlyNewCanChange && !instanceEntityAdapter.CanChangeClass)
			{
				Outputs.WriteLine(OutputMessageType.Warning, "Cannot change the class of an entity that already exists in the cloud.  Create a new entity.");
			}
			else if (instanceEntityAdapter.CanChangeClass || instanceEntityAdapter.ShouldChangeClass || MessageBox.Show("This entity is currently being used by one or more other entities.\nIf you change this class you will most likely break all those entities.\nIt is recommended that you remove those references first and then change the entity's class.\n\nAre you sure you want to change the class and most likely breaking other entities?", "Entity used by others", MessageBoxButtons.YesNo) != DialogResult.No)
			{
				instanceEntityAdapter?.PreClassNameChange();
				node.SetAttribute(base.AttributeInfo, value);
				instanceEntityAdapter?.PostClassNameChange();
			}
		}
	}
}
