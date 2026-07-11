using System;
using System.Reflection;

namespace Sce.Atf;

public static class AttributeUtils
{
	public static T GetAttribute<T>(MemberInfo info, bool inherit = false) where T : Attribute
	{
		object[] customAttributes = info.GetCustomAttributes(typeof(T), inherit);
		return (customAttributes.Length != 0) ? ((T)customAttributes[0]) : null;
	}
}
