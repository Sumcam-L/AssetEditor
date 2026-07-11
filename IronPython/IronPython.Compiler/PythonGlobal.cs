using System;
using System.Diagnostics;
using System.Reflection;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Compiler;

[DebuggerDisplay("{Display}")]
public sealed class PythonGlobal
{
	private object _value;

	private ModuleGlobalCache _global;

	private string _name;

	private CodeContext _context;

	internal static PropertyInfo CurrentValueProperty = typeof(PythonGlobal).GetProperty("CurrentValue");

	internal static PropertyInfo RawValueProperty = typeof(PythonGlobal).GetProperty("RawValue");

	public object CurrentValue
	{
		get
		{
			if (_value != Uninitialized.Instance)
			{
				return _value;
			}
			return GetCachedValue(lightThrow: false);
		}
		set
		{
			if (value == Uninitialized.Instance && _value == Uninitialized.Instance)
			{
				throw PythonOps.GlobalNameError(_name);
			}
			_value = value;
		}
	}

	public object CurrentValueLightThrow
	{
		get
		{
			if (_value != Uninitialized.Instance)
			{
				return _value;
			}
			return GetCachedValue(lightThrow: true);
		}
	}

	public string Name => _name;

	public object RawValue
	{
		get
		{
			return _value;
		}
		internal set
		{
			_value = value;
		}
	}

	public string Display
	{
		get
		{
			try
			{
				return GetStringDisplay(CurrentValue);
			}
			catch (MissingMemberException)
			{
				return "<uninitialized>";
			}
		}
	}

	public PythonGlobal(CodeContext context, string name)
	{
		_value = Uninitialized.Instance;
		_context = context;
		_name = name;
	}

	private object GetCachedValue(bool lightThrow)
	{
		if (_global == null)
		{
			_global = _context.LanguageContext.GetModuleGlobalCache(_name);
		}
		object value;
		if (_global.IsCaching)
		{
			if (_global.HasValue)
			{
				return _global.Value;
			}
		}
		else if (_context.TryLookupBuiltin(_name, out value))
		{
			return value;
		}
		if (lightThrow)
		{
			return LightExceptions.Throw(PythonOps.GlobalNameError(_name));
		}
		throw PythonOps.GlobalNameError(_name);
	}

	private static string GetStringDisplay(object val)
	{
		if (val != null)
		{
			return val.ToString();
		}
		return "(null)";
	}

	public override string ToString()
	{
		return string.Format("ModuleGlobal: {0} Value: {1} ({2})", _name, _value, (RawValue == Uninitialized.Instance) ? "Module Local" : "Global");
	}
}
