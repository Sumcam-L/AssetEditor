using System;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation;

public class Adapter : IAdapter, IAdaptable, IDecoratable
{
	private object m_adaptee;

	public object Adaptee
	{
		get
		{
			return m_adaptee;
		}
		set
		{
			if (m_adaptee != value)
			{
				object adaptee = m_adaptee;
				m_adaptee = value;
				OnAdapteeChanged(adaptee);
			}
		}
	}

	public Adapter()
	{
	}

	public Adapter(object adaptee)
	{
		Adaptee = adaptee;
	}

	public object GetAdapter(Type type)
	{
		object obj = Adapt(type);
		if (obj == null)
		{
			obj = m_adaptee.As(type);
		}
		return obj;
	}

	public IEnumerable<object> GetDecorators(Type type)
	{
		object adapter = Adapt(type);
		if (adapter != null)
		{
			yield return adapter;
		}
		foreach (object obj in m_adaptee.AsAll(type))
		{
			if (obj != adapter)
			{
				yield return obj;
			}
		}
	}

	public T As<T>() where T : class
	{
		return Adapters.As<T>(this);
	}

	public T Cast<T>() where T : class
	{
		return Adapters.Cast<T>(this);
	}

	public bool Is<T>() where T : class
	{
		return Adapters.Is<T>(this);
	}

	public IEnumerable<T> AsAll<T>() where T : class
	{
		return Adapters.AsAll<T>(this);
	}

	protected virtual void OnAdapteeChanged(object oldAdaptee)
	{
	}

	protected virtual object Adapt(Type type)
	{
		if (type.IsAssignableFrom(GetType()))
		{
			return this;
		}
		return null;
	}
}
