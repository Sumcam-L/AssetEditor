using System.ComponentModel;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

internal class SkinSetterAttributePropertyDescriptor : AttributePropertyDescriptor
{
	private DomNode m_domObject;

	public SkinSetterAttributePropertyDescriptor(DomNode domObj, string name, AttributeInfo attribute, string category, string description, bool isReadOnly, object editor, TypeConverter typeConverter)
		: base(name, attribute, category, description, isReadOnly, editor, typeConverter)
	{
		m_domObject = domObj;
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	public override void ResetValue(object component)
	{
	}

	public override DomNode GetNode(object component)
	{
		return m_domObject;
	}

	public override bool Equals(object obj)
	{
		SkinSetterAttributePropertyDescriptor skinSetterAttributePropertyDescriptor = obj as SkinSetterAttributePropertyDescriptor;
		if (!base.Equals((object)skinSetterAttributePropertyDescriptor))
		{
			return false;
		}
		if (m_domObject != skinSetterAttributePropertyDescriptor.m_domObject)
		{
			return false;
		}
		if (Name != skinSetterAttributePropertyDescriptor.Name)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return m_domObject.GetHashCode();
	}
}
