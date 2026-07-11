using System;

namespace Sce.Atf.Adaptation;

public class AdapterCreator<T> : IAdapterCreator where T : class, IAdapter, new()
{
	public bool CanAdapt(object adaptee, Type type)
	{
		return adaptee != null && type != null && type.IsAssignableFrom(typeof(T));
	}

	public object GetAdapter(object adaptee, Type type)
	{
		if (type != null && type.IsAssignableFrom(typeof(T)))
		{
			return new T
			{
				Adaptee = adaptee
			};
		}
		return null;
	}
}
