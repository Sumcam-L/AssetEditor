using System;
using System.Reflection;
using System.Text;
using IronPython.Runtime.Binding;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Types;

public static class NameConverter
{
	public static NameType TryGetName(PythonType dt, MethodInfo mi, out string name)
	{
		name = mi.Name;
		return GetNameFromMethod(dt, mi, NameType.Method, ref name);
	}

	public static NameType TryGetName(PythonType dt, EventInfo ei, MethodInfo eventMethod, out string name)
	{
		name = ei.Name;
		NameType res = (dt.IsPythonType ? NameType.PythonEvent : NameType.Event);
		return GetNameFromMethod(dt, eventMethod, res, ref name);
	}

	public static NameType TryGetName(PythonType dt, PropertyInfo pi, MethodInfo prop, out string name)
	{
		if (pi.IsDefined(typeof(PythonHiddenAttribute), inherit: false))
		{
			name = null;
			return NameType.None;
		}
		name = pi.Name;
		return GetNameFromMethod(dt, prop, NameType.Property, ref name);
	}

	public static string GetTypeName(Type t)
	{
		if (t.IsArray)
		{
			return "Array[" + GetTypeName(t.GetElementType()) + "]";
		}
		string text = PythonBinder.GetTypeNameInternal(t);
		if (text != t.Name)
		{
			return text;
		}
		int length;
		if ((length = text.IndexOf('`')) != -1)
		{
			text = text.Substring(0, length);
			Type[] genericArguments = t.GetGenericArguments();
			StringBuilder stringBuilder = new StringBuilder(text);
			stringBuilder.Append('[');
			bool flag = true;
			Type[] array = genericArguments;
			foreach (Type t2 in array)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(GetTypeName(t2));
			}
			stringBuilder.Append(']');
			text = stringBuilder.ToString();
		}
		return text;
	}

	internal static NameType GetNameFromMethod(PythonType dt, MethodInfo mi, NameType res, ref string name)
	{
		string text = null;
		if (mi.IsPrivate || (mi.IsAssembly && !mi.IsFamilyOrAssembly))
		{
			if (!mi.IsPrivate || !mi.IsFinal || !mi.IsHideBySig || !mi.IsVirtual)
			{
				text = "_" + dt.Name + "__";
			}
			else
			{
				int num = name.LastIndexOf('.');
				if (num != -1)
				{
					name = name.Substring(num + 1);
				}
			}
		}
		if (mi.IsDefined(typeof(ClassMethodAttribute), inherit: false))
		{
			res |= NameType.ClassMember;
		}
		if (text != null)
		{
			name = text + name;
		}
		if (mi.DeclaringType.IsDefined(typeof(PythonTypeAttribute), inherit: false) || !mi.DeclaringType.IsAssignableFrom(dt.UnderlyingSystemType))
		{
			res |= NameType.Python;
		}
		if (mi.IsDefined(typeof(PropertyMethodAttribute), inherit: false))
		{
			res = (res & ~NameType.BaseTypeMask) | NameType.Property;
		}
		return res;
	}
}
