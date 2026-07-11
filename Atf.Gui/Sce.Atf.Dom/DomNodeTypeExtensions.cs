using System.ComponentModel;
using System.Linq;

namespace Sce.Atf.Dom;

public static class DomNodeTypeExtensions
{
	public static void RegisterDescriptor(this DomNodeType type, System.ComponentModel.PropertyDescriptor descriptor)
	{
		PropertyDescriptorCollection propertyDescriptorCollection = type.GetTagLocal<PropertyDescriptorCollection>();
		if (propertyDescriptorCollection == null)
		{
			propertyDescriptorCollection = new PropertyDescriptorCollection(EmptyArray<PropertyDescriptor>.Instance);
			type.SetTag(propertyDescriptorCollection);
		}
		if (!(descriptor is AttributePropertyDescriptor) || !propertyDescriptorCollection.OfType<AttributePropertyDescriptor>().Any((AttributePropertyDescriptor x) => Equivalent(x, (AttributePropertyDescriptor)descriptor)))
		{
			propertyDescriptorCollection.Add(descriptor);
		}
	}

	private static bool Equivalent(AttributePropertyDescriptor x, AttributePropertyDescriptor y)
	{
		if (!x.AttributeInfo.Equivalent(y.AttributeInfo))
		{
			return false;
		}
		ChildAttributePropertyDescriptor childAttributePropertyDescriptor = x as ChildAttributePropertyDescriptor;
		ChildAttributePropertyDescriptor childAttributePropertyDescriptor2 = y as ChildAttributePropertyDescriptor;
		if (childAttributePropertyDescriptor == null || childAttributePropertyDescriptor2 == null)
		{
			return false;
		}
		ChildInfo[] array = childAttributePropertyDescriptor.Path.ToArray();
		ChildInfo[] array2 = childAttributePropertyDescriptor2.Path.ToArray();
		if (array.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (!array2[i].IsEquivalent(array[i]))
			{
				return false;
			}
		}
		return true;
	}
}
