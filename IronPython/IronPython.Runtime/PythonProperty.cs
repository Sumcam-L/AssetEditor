using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

[PythonType("property")]
public class PythonProperty : PythonTypeDataSlot
{
	private object _fget;

	private object _fset;

	private object _fdel;

	private object _doc;

	internal override bool GetAlwaysSucceeds => true;

	public object fdel
	{
		get
		{
			return _fdel;
		}
		set
		{
			throw PythonOps.TypeError("readonly attribute");
		}
	}

	public object fset
	{
		get
		{
			return _fset;
		}
		set
		{
			throw PythonOps.TypeError("readonly attribute");
		}
	}

	public object fget
	{
		get
		{
			return _fget;
		}
		set
		{
			throw PythonOps.TypeError("readonly attribute");
		}
	}

	public PythonProperty()
	{
	}

	public PythonProperty(params object[] args)
	{
	}

	public PythonProperty([ParamDictionary] IDictionary<object, object> dict, params object[] args)
	{
	}

	public void __init__([DefaultParameterValue(null)] object fget, [DefaultParameterValue(null)] object fset, [DefaultParameterValue(null)] object fdel, [DefaultParameterValue(null)] object doc)
	{
		_fget = fget;
		_fset = fset;
		_fdel = fdel;
		_doc = doc;
		if (GetType() != typeof(PythonProperty) && _fget is PythonFunction)
		{
			PythonDictionary dictionary = UserTypeOps.GetDictionary((IPythonObject)this);
			if (dictionary == null)
			{
				throw PythonOps.AttributeError("{0} object has no __doc__ attribute", PythonTypeOps.GetName(this));
			}
			dictionary["__doc__"] = ((PythonFunction)_fget).__doc__;
		}
	}

	internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
	{
		value = __get__(context, instance, PythonOps.ToPythonType(owner));
		return true;
	}

	internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
	{
		if (instance == null)
		{
			return false;
		}
		__set__(context, instance, value);
		return true;
	}

	internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
	{
		if (instance == null)
		{
			return false;
		}
		__delete__(context, instance);
		return true;
	}

	[SpecialName]
	[WrapperDescriptor]
	[PropertyMethod]
	public static object Get__doc__(CodeContext context, PythonProperty self)
	{
		if (self._doc == null && PythonOps.HasAttr(context, self._fget, "__doc__"))
		{
			return PythonOps.GetBoundAttr(context, self._fget, "__doc__");
		}
		if (self._doc == null)
		{
			Console.WriteLine("No attribute __doc__");
		}
		return self._doc;
	}

	[SpecialName]
	[PropertyMethod]
	[WrapperDescriptor]
	public static void Set__doc__(PythonProperty self, object value)
	{
		throw PythonOps.TypeError("readonly attribute");
	}

	public override object __get__(CodeContext context, object instance, [DefaultParameterValue(null)] object owner)
	{
		if (instance == null)
		{
			return this;
		}
		if (fget != null)
		{
			CallSite<Func<CallSite, CodeContext, object, object, object>> propertyGetSite = PythonContext.GetContext(context).PropertyGetSite;
			return propertyGetSite.Target(propertyGetSite, context, fget, instance);
		}
		throw PythonOps.UnreadableProperty();
	}

	public override void __set__(CodeContext context, object instance, object value)
	{
		if (fset != null)
		{
			CallSite<Func<CallSite, CodeContext, object, object, object, object>> propertySetSite = PythonContext.GetContext(context).PropertySetSite;
			propertySetSite.Target(propertySetSite, context, fset, instance, value);
			return;
		}
		throw PythonOps.UnsetableProperty();
	}

	public override void __delete__(CodeContext context, object instance)
	{
		if (fdel != null)
		{
			CallSite<Func<CallSite, CodeContext, object, object, object>> propertyDeleteSite = PythonContext.GetContext(context).PropertyDeleteSite;
			propertyDeleteSite.Target(propertyDeleteSite, context, fdel, instance);
			return;
		}
		throw PythonOps.UndeletableProperty();
	}

	public PythonProperty getter(object fget)
	{
		PythonProperty pythonProperty = new PythonProperty();
		pythonProperty.__init__(fget, _fset, _fdel, _doc);
		return pythonProperty;
	}

	public PythonProperty setter(object fset)
	{
		PythonProperty pythonProperty = new PythonProperty();
		pythonProperty.__init__(_fget, fset, _fdel, _doc);
		return pythonProperty;
	}

	public PythonProperty deleter(object fdel)
	{
		PythonProperty pythonProperty = new PythonProperty();
		pythonProperty.__init__(_fget, _fset, fdel, _doc);
		return pythonProperty;
	}
}
