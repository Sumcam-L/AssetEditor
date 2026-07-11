using System;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public abstract class DynamicFieldPropertyDescriptorBase : FieldPropertyDescriptorBase, IAdaptable
{
	private FieldPropertyDescriptorBase m_targetField;

	public override AttributeInfo AttributeInfo => m_targetField.AttributeInfo;

	public override Type ClrType => AttributeInfo.Type.ClrType;

	public FieldPropertyDescriptorBase TemplateFieldDescriptor => m_targetField;

	public DynamicFieldPropertyDescriptorBase(FieldPropertyDescriptorBase fldPropDesc)
		: base(fldPropDesc)
	{
		m_targetField = fldPropDesc;
	}

	public DynamicFieldPropertyDescriptorBase(FieldPropertyDescriptorBase fldPropDesc, object editor)
		: base(fldPropDesc, editor)
	{
		m_targetField = fldPropDesc;
	}

	public object GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(GetType()))
		{
			return this;
		}
		if (type.IsAssignableFrom(TemplateFieldDescriptor.GetType()))
		{
			return TemplateFieldDescriptor;
		}
		return null;
	}

	public override object GetEditor(Type editorBaseType)
	{
		object editor = m_targetField.GetEditor(editorBaseType);
		if (editor == null)
		{
			editor = base.GetEditor(editorBaseType);
		}
		return editor;
	}

	public abstract IFieldValueAdapter GetFieldAdapter(object component);
}
