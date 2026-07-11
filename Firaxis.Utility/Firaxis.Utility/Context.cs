using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Firaxis.Utility;

public sealed class Context : ISite, IServiceProvider
{
	private static Context self = new Context();

	private List<object> contexts;

	private string name;

	private bool cachedDesignMode;

	private bool inDesignMode;

	public static ISite Site => self;

	public IComponent Component
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IContainer Container
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public static bool InDesignMode => self.DesignMode;

	public bool DesignMode
	{
		get
		{
			if (!cachedDesignMode)
			{
				cachedDesignMode = true;
				if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				{
					inDesignMode = true;
				}
				else if (Process.GetCurrentProcess().ProcessName.ToUpper().Equals("DEVENV"))
				{
					inDesignMode = true;
				}
			}
			return inDesignMode;
		}
	}

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	private Context()
	{
		contexts = new List<object>();
		name = "";
	}

	public static object Get(Type type)
	{
		object obj = Find(type);
		if (obj == null)
		{
			throw new ArgumentException($"Type ({type.ToString()}) does not exist in context");
		}
		return obj;
	}

	public static object Get(string type)
	{
		object type2 = Type.GetType(type, throwOnError: false);
		if (type2 == null)
		{
			return null;
		}
		return Get((Type)type2);
	}

	public static T Get<T>()
	{
		return (T)Get(typeof(T));
	}

	public static bool TryGet<T>(out T service)
	{
		return (service = (T)Find(typeof(T))) != null;
	}

	public static bool Has(Type type)
	{
		return Find(type) != null;
	}

	private static object Find(Type type)
	{
		if (type.IsInstanceOfType(self))
		{
			return self;
		}
		foreach (object context in self.contexts)
		{
			if (type.IsInstanceOfType(context))
			{
				return context;
			}
		}
		return null;
	}

	public static bool Has<T>()
	{
		return Has(typeof(T));
	}

	public static T EnsureCreated<T>()
	{
		if (Has<T>())
		{
			return Get<T>();
		}
		lock (self.contexts)
		{
			T val = Activator.CreateInstance<T>();
			self.contexts.Add(val);
			return val;
		}
	}

	public static T EnsureCreated<T>(params object[] args)
	{
		if (Has<T>())
		{
			return Get<T>();
		}
		T val = (T)Activator.CreateInstance(typeof(T), args);
		self.contexts.Add(val);
		return val;
	}

	public static void Add(object o)
	{
		self.contexts.Add(o);
	}

	public static void Remove(object o)
	{
		self.contexts.Remove(o);
	}

	public static void Remove<T>()
	{
		object o;
		while ((o = GetService<T>()) != null)
		{
			Remove(o);
		}
	}

	public static void Clear()
	{
		self.contexts.Clear();
	}

	public static T GetService<T>()
	{
		return (T)self.GetService(typeof(T));
	}

	public object GetService(Type serviceType)
	{
		if (serviceType.IsInstanceOfType(self))
		{
			return self;
		}
		foreach (object context in self.contexts)
		{
			if (serviceType.IsInstanceOfType(context))
			{
				return context;
			}
		}
		return null;
	}
}
