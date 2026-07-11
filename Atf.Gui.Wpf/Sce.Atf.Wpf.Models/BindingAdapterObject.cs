using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Models;

internal class BindingAdapterObject : BindingAdapterObjectBase
{
	private class BindingAdapterPropertyDescriptor : PropertyDescriptor
	{
		private Type m_type;

		public override Type ComponentType => typeof(BindingAdapterObject);

		public override Type PropertyType => m_type;

		public override bool IsReadOnly => true;

		public BindingAdapterPropertyDescriptor(Type type)
			: base(type.Name, null)
		{
			m_type = type;
		}

		public override bool CanResetValue(object component)
		{
			return false;
		}

		public override void ResetValue(object component)
		{
			throw new NotImplementedException();
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		public override object GetValue(object component)
		{
			return ((BindingAdapterObject)component).Adaptee.As(m_type);
		}

		public override void SetValue(object component, object value)
		{
			throw new NotSupportedException();
		}
	}

	private static Dictionary<Type, BindingAdapterPropertyDescriptor[]> s_baseTypesLookup = new Dictionary<Type, BindingAdapterPropertyDescriptor[]>();

	public BindingAdapterObject(object adaptee)
		: base(adaptee)
	{
	}

	protected override PropertyDescriptorCollection GenerateDescriptors()
	{
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		foreach (object item in base.Adaptee.AsAll<object>())
		{
			Type type = item.GetType();
			BindingAdapterPropertyDescriptor[] descriptorsFromBaseTypes = GetDescriptorsFromBaseTypes(type);
			BindingAdapterObjectBase.MergeDescriptors(list, descriptorsFromBaseTypes);
		}
		return new PropertyDescriptorCollection(list.ToArray());
	}

	private static BindingAdapterPropertyDescriptor[] GetDescriptorsFromBaseTypes(Type adapterType)
	{
		if (!s_baseTypesLookup.TryGetValue(adapterType, out var value))
		{
			List<Type> list = new List<Type>(adapterType.GetInterfaces());
			Type type = adapterType;
			while (type != typeof(object))
			{
				list.Add(type);
				type = type.BaseType;
			}
			List<BindingAdapterPropertyDescriptor> list2 = new List<BindingAdapterPropertyDescriptor>();
			foreach (Type baseType in list)
			{
				if (!list2.Any((BindingAdapterPropertyDescriptor x) => x.Name == baseType.Name))
				{
					list2.Add(new BindingAdapterPropertyDescriptor(baseType));
				}
			}
			value = list2.ToArray();
			lock (s_baseTypesLookup)
			{
				if (!s_baseTypesLookup.ContainsKey(adapterType))
				{
					s_baseTypesLookup.Add(adapterType, value);
				}
			}
		}
		return s_baseTypesLookup[adapterType];
	}
}
