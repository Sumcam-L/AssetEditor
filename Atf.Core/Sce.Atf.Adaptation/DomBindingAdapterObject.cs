using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Dom;

namespace Sce.Atf.Adaptation;

public class DomBindingAdapterObject : BindingAdapterObject
{
	private static readonly Dictionary<DomNodeType, PropertyDescriptorCollection> s_cachedPropertyDescriptors = new Dictionary<DomNodeType, PropertyDescriptorCollection>();

	public bool EnableNodeTypeExtensionOptimisation { get; private set; }

	public DomBindingAdapterObject(DomNode adaptee, bool enableNodeTypeExtensionOptimisation)
		: base(adaptee)
	{
		EnableNodeTypeExtensionOptimisation = enableNodeTypeExtensionOptimisation;
	}

	protected override PropertyDescriptorCollection GenerateDescriptors()
	{
		PropertyDescriptorCollection value = null;
		if (EnableNodeTypeExtensionOptimisation)
		{
			DomNode domNode = base.Adaptee.Cast<DomNode>();
			if (!s_cachedPropertyDescriptors.TryGetValue(domNode.Type, out value))
			{
				value = base.GenerateDescriptors();
				lock (s_cachedPropertyDescriptors)
				{
					if (!s_cachedPropertyDescriptors.ContainsKey(domNode.Type))
					{
						s_cachedPropertyDescriptors.Add(domNode.Type, value);
					}
				}
			}
		}
		else
		{
			value = base.GenerateDescriptors();
		}
		return value;
	}
}
