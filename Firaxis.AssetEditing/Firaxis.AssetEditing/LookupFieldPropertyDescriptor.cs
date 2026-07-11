using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LookupFieldPropertyDescriptor : DynamicFieldPropertyDescriptorBase
{
	private string m_description;

	private string m_fieldName;

	public override string Description => m_description;

	public override string DisplayName => GetDisplayName();

	public override string Name => GetName();

	public virtual string FieldName => m_fieldName;

	public LookupFieldPropertyDescriptor(FieldPropertyDescriptorBase fldPropDesc, string fieldName)
		: this(fldPropDesc, fieldName, fldPropDesc.LocalEditor)
	{
	}

	public LookupFieldPropertyDescriptor(FieldPropertyDescriptorBase fldPropDesc, string fieldName, object editor)
		: base(fldPropDesc, editor)
	{
		m_fieldName = fieldName;
		m_description = fldPropDesc.Description;
	}

	public override string GetCategoryName(object component)
	{
		string text = GetFieldAdapter(component)?.Parameter?.Category;
		if (string.IsNullOrEmpty(text))
		{
			text = base.TemplateFieldDescriptor.Category;
		}
		return text;
	}

	public override string GetDisplayName(object component)
	{
		if (base.TemplateFieldDescriptor.ShowTypeInName)
		{
			return m_fieldName + "(" + base.TemplateFieldDescriptor.DisplayName + ")";
		}
		return m_fieldName;
	}

	protected IFieldValueAdapter GetFieldAdapter(object component, string fldName)
	{
		IFieldContainerAdapter fieldContainerAdapter = component.As<IFieldContainerAdapter>();
		if (fieldContainerAdapter == null)
		{
			return null;
		}
		if (fieldContainerAdapter.Fields == null)
		{
			return null;
		}
		return fieldContainerAdapter.Fields.FirstOrDefault((IFieldValueAdapter fld) => fld.Name == fldName);
	}

	public override IFieldValueAdapter GetFieldAdapter(object component)
	{
		if (component == null)
		{
			return null;
		}
		return GetFieldAdapter(component, m_fieldName);
	}

	public override DomNode GetNode(object component)
	{
		IFieldValueAdapter fieldAdapter = GetFieldAdapter(component);
		if (fieldAdapter == null)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(fieldAdapter.Parameter.Description))
		{
			m_description = fieldAdapter.Parameter.Description;
		}
		return base.TemplateFieldDescriptor.GetNode(fieldAdapter.DomNode);
	}

	public override object GetValue(object component)
	{
		if (component == null)
		{
			return null;
		}
		object obj = null;
		IFieldValueAdapter fieldAdapter = GetFieldAdapter(component);
		if (fieldAdapter != null)
		{
			obj = base.TemplateFieldDescriptor.GetValue(fieldAdapter.DomNode);
		}
		BugSubmitter.SilentAssertOnce(obj != null, "Failed to get non-null value for property descriptor for field \"{0}\" on component \"{1}\" using template \"{2}\" @summary Failed to get non-null value for property descriptor @assign bwhitman", m_fieldName, component, base.TemplateFieldDescriptor);
		return obj;
	}

	public override void ResetValue(object component)
	{
		IFieldValueAdapter fieldAdapter = GetFieldAdapter(component);
		if (fieldAdapter != null)
		{
			base.TemplateFieldDescriptor.SetValue(fieldAdapter.DomNode, fieldAdapter.DefaultDataAsObject);
		}
	}

	public override void SetValue(object component, object value)
	{
		IFieldValueAdapter fieldAdapter = GetFieldAdapter(component);
		if (fieldAdapter != null)
		{
			base.TemplateFieldDescriptor.SetValue(fieldAdapter.DomNode, value);
		}
	}

	private string GetDisplayName()
	{
		if (base.TemplateFieldDescriptor.ShowTypeInName)
		{
			return m_fieldName + "(" + base.TemplateFieldDescriptor.DisplayName + ")";
		}
		return m_fieldName;
	}

	private string GetName()
	{
		if (base.TemplateFieldDescriptor.ShowTypeInName)
		{
			return m_fieldName + "(" + base.TemplateFieldDescriptor.DisplayName + ")";
		}
		return m_fieldName;
	}
}
