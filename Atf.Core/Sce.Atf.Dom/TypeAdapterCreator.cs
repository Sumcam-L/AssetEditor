using System;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class TypeAdapterCreator<T> : IAdapterCreator where T : class
{
	public bool CanAdapt(object adaptee, Type type)
	{
		return adaptee is DomNode && type != null && type.IsAssignableFrom(typeof(T));
	}

	public object GetAdapter(object adaptee, Type type)
	{
		if (adaptee is DomNode domNode && type != null && type.IsAssignableFrom(typeof(T)))
		{
			return domNode.Type.GetTag<T>();
		}
		return null;
	}
}
