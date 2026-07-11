using System;
using System.ComponentModel;
using Firaxis.Error;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public abstract class FieldPropertyDescriptorBase : Sce.Atf.Dom.PropertyDescriptor, ICustomPropertyDisplayName, INonCacheableDescriptor, ICustomPropertyCatergoryName
{
	public abstract AttributeInfo AttributeInfo { get; }

	public abstract Type ClrType { get; }

	public bool ShowTypeInName { get; set; }

	public FieldPropertyDescriptorBase(FieldPropertyDescriptorBase fldPropDesc)
		: this(fldPropDesc.Name, fldPropDesc.AttributeInfo, fldPropDesc.Category, fldPropDesc.Description, fldPropDesc.IsReadOnly, fldPropDesc.LocalEditor, fldPropDesc.Converter)
	{
	}

	public FieldPropertyDescriptorBase(FieldPropertyDescriptorBase fldPropDesc, object editor)
		: this(fldPropDesc.Name, fldPropDesc.AttributeInfo, fldPropDesc.Category, fldPropDesc.Description, fldPropDesc.IsReadOnly, editor, fldPropDesc.Converter)
	{
	}

	public FieldPropertyDescriptorBase(string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, (attribute != null) ? attribute.Type.ClrType : typeof(object), category, description, isReadOnly, editor, typeConverter, null)
	{
		ShowTypeInName = false;
	}

	public FieldPropertyDescriptorBase(string name, AttributeInfo attribute, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter typeConverter)
		: base(name, (attribute != null) ? attribute.Type.ClrType : typeof(object), category, description, isReadOnlyFunctor, editor, typeConverter, null)
	{
		ShowTypeInName = false;
	}

	public virtual string GetCategoryName(object component)
	{
		return Category;
	}

	public virtual string GetDisplayName(object component)
	{
		return Name;
	}

	public override bool CanResetValue(object component)
	{
		if (AttributeInfo == null)
		{
			return false;
		}
		if (IsReadOnly)
		{
			return false;
		}
		DomNode node = GetNode(component);
		if (node != null && AttributeInfo.Equivalent(node.Type.IdAttribute))
		{
			return false;
		}
		object value = GetValue(component);
		if (value == null || value.Equals(AttributeInfo.DefaultValue))
		{
			if (value == null)
			{
				return AttributeInfo.DefaultValue != null;
			}
			return false;
		}
		return true;
	}

	public override object GetValue(object component)
	{
		object obj = null;
		DomNode node = GetNode(component);
		if (node != null && AttributeInfo != null)
		{
			obj = node.GetAttribute(AttributeInfo);
		}
		return obj ?? node;
	}

	public override void ResetValue(object component)
	{
		PlatformAssert.If(AttributeInfo == null);
		SetValue(component, AttributeInfo.DefaultValue);
	}

	public override void SetValue(object component, object value)
	{
		PlatformAssert.If(AttributeInfo == null);
		GetNode(component)?.SetAttribute(AttributeInfo, value);
	}
}
