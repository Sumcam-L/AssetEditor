using System;
using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public sealed class MultiSelectProxyDescriptor : Sce.Atf.Dom.PropertyDescriptor, IAdaptable
{
	public System.ComponentModel.PropertyDescriptor ProxyTarget { get; set; }

	public MultiSelectProxyDescriptor(System.ComponentModel.PropertyDescriptor proxy)
		: base(proxy.Name, proxy.ComponentType, proxy.Category, proxy.Description, proxy.IsReadOnly, proxy.GetEditor(typeof(object)), proxy.Converter)
	{
		ProxyTarget = proxy;
	}

	public override bool CanResetValue(object component)
	{
		return ProxyTarget.CanResetValue(component);
	}

	public override object GetValue(object component)
	{
		return null;
	}

	public override void ResetValue(object component)
	{
		ProxyTarget.ResetValue(component);
	}

	public override void SetValue(object component, object value)
	{
		ProxyTarget.SetValue(component, value);
	}

	public object GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(GetType()))
		{
			return this;
		}
		if (type.IsAssignableFrom(ProxyTarget.GetType()))
		{
			return ProxyTarget;
		}
		return null;
	}
}
