using System;
using Firaxis.Reflection;

namespace Firaxis.Utility;

public class Maker<T> : IMaker
{
	private object[] args;

	private Func<object[], object> maker;

	public string Name => Type.Name;

	public Type Type { get; private set; }

	public Maker()
		: this(typeof(T), (Func<object[], object>)null, (object[])null)
	{
	}

	public Maker(params object[] args)
		: this(typeof(T), (Func<object[], object>)null, args)
	{
		this.args = args;
	}

	public Maker(Type type, Func<object[], object> maker)
		: this(type, maker, (object[])null)
	{
	}

	public Maker(Type type, Func<object[], object> maker, params object[] args)
	{
		Type = type;
		this.maker = maker;
		this.args = args;
	}

	public object Make()
	{
		if (maker != null)
		{
			return maker(args);
		}
		return Activator.CreateInstance(Type, args);
	}

	public object Make(params object[] args)
	{
		return Activator.CreateInstance(Type, args);
	}

	public override string ToString()
	{
		return ReflectionHelper.GetDisplayName(Type);
	}
}
