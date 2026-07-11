using System;
using System.Reflection;

namespace IronPython.Runtime.Types;

public class ExtensionPropertyInfo
{
	private MethodInfo _getter;

	private MethodInfo _setter;

	private MethodInfo _deleter;

	private Type _declaringType;

	public MethodInfo Getter => _getter;

	public MethodInfo Setter => _setter;

	public MethodInfo Deleter => _deleter;

	public Type DeclaringType => _declaringType;

	public string Name
	{
		get
		{
			if (_getter != null)
			{
				return _getter.Name.Substring(3);
			}
			return _setter.Name.Substring(3);
		}
	}

	public ExtensionPropertyInfo(Type logicalDeclaringType, MethodInfo mi)
	{
		_declaringType = logicalDeclaringType;
		string text = mi.Name;
		string prefix = "";
		if (text.StartsWith("#base#"))
		{
			text = text.Substring("#base#".Length);
			prefix = "#base#";
		}
		if (text.StartsWith("Get") || text.StartsWith("Set"))
		{
			GetPropertyMethods(mi, text, prefix, "Get", "Set", "Delete");
		}
		else if (text.StartsWith("get_") || text.StartsWith("set_"))
		{
			GetPropertyMethods(mi, text, prefix, "get_", "set_", null);
		}
		else if (text.StartsWith("#field_get#") || text.StartsWith("#field_set#"))
		{
			GetPropertyMethods(mi, text, prefix, "#field_get#", "#field_set#", null);
		}
	}

	private void GetPropertyMethods(MethodInfo mi, string methodName, string prefix, string get, string set, string delete)
	{
		string text = methodName.Substring(get.Length);
		if (string.Compare(mi.Name, 0, get, 0, get.Length) == 0)
		{
			_getter = mi;
			_setter = mi.DeclaringType.GetMethod(prefix + set + text);
		}
		else
		{
			_getter = mi.DeclaringType.GetMethod(prefix + get + text);
			_setter = mi;
		}
		if (delete != null)
		{
			_deleter = mi.DeclaringType.GetMethod(prefix + delete + text);
		}
	}
}
