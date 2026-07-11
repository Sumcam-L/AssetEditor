using Firaxis.ATF;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class BoundFieldPropertyDescriptor : DynamicFieldPropertyDescriptorBase, INonCacheableDescriptor
{
	public override string Description => GetDescription();

	public override string DisplayName => GetDisplayName();

	public override string Name => GetName();

	private IFieldValueAdapter FieldValueAdapter { get; set; }

	public BoundFieldPropertyDescriptor(FieldPropertyDescriptorBase fldPropDesc, IFieldValueAdapter fldAdapter)
		: this(fldPropDesc, fldAdapter, fldPropDesc.LocalEditor)
	{
	}

	public BoundFieldPropertyDescriptor(FieldPropertyDescriptorBase fldPropDesc, IFieldValueAdapter fldAdapter, object editor)
		: base(fldPropDesc, editor)
	{
		FieldValueAdapter = fldAdapter;
	}

	public override string GetCategoryName(object component)
	{
		string text = FieldValueAdapter.Parameter.Category;
		if (string.IsNullOrEmpty(text))
		{
			text = base.TemplateFieldDescriptor.Category;
		}
		return text;
	}

	public override string GetDisplayName(object component)
	{
		return GetDisplayName();
	}

	public override IFieldValueAdapter GetFieldAdapter(object component)
	{
		return FieldValueAdapter;
	}

	public override DomNode GetNode(object component)
	{
		return FieldValueAdapter.DomNode;
	}

	public override object GetValue(object component)
	{
		return base.TemplateFieldDescriptor.GetValue(FieldValueAdapter.DomNode);
	}

	public override void ResetValue(object component)
	{
		DomNode domNode;
		if ((domNode = component.As<DomNode>()) == null)
		{
			domNode = component.As<DomNodeAdapter>()?.DomNode;
		}
		DomNode domNode2 = domNode;
		object value = domNode2.As<IFieldValueAdapter>()?.DefaultDataAsObject ?? FieldValueAdapter.DefaultDataAsObject;
		base.TemplateFieldDescriptor.SetValue(domNode2, value);
	}

	public override void SetValue(object component, object value)
	{
		DomNode domNode;
		if ((domNode = component.As<DomNode>()) == null)
		{
			domNode = component.As<DomNodeAdapter>()?.DomNode;
		}
		DomNode component2 = domNode;
		base.TemplateFieldDescriptor.SetValue(component2, value);
	}

	private string GetDescription()
	{
		if (!string.IsNullOrEmpty(FieldValueAdapter.Parameter.Description))
		{
			return FieldValueAdapter.Parameter.Description;
		}
		return base.TemplateFieldDescriptor.Description;
	}

	private string GetDisplayName()
	{
		if (base.TemplateFieldDescriptor.ShowTypeInName)
		{
			return FieldValueAdapter.Name + "(" + base.TemplateFieldDescriptor.DisplayName + ")";
		}
		return FieldValueAdapter.Name;
	}

	private string GetName()
	{
		if (base.TemplateFieldDescriptor.ShowTypeInName)
		{
			return FieldValueAdapter.Name + "(" + base.TemplateFieldDescriptor.DisplayName + ")";
		}
		return FieldValueAdapter.Name;
	}
}
