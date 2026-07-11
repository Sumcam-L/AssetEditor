using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

internal static class SkinUtil
{
	private static Dictionary<string, Type> s_types;

	static SkinUtil()
	{
		s_types = new Dictionary<string, Type>();
		s_types.Add("string", typeof(string));
		s_types.Add("float", typeof(float));
		s_types.Add("int", typeof(int));
		s_types.Add("char", typeof(char));
		s_types.Add(typeof(FontStyle).FullName, typeof(FontStyle));
		s_types.Add(typeof(Color).FullName, typeof(Color));
		s_types.Add(typeof(Control).FullName, typeof(Control));
		s_types.Add(typeof(Font).FullName, typeof(Font));
		s_types.Add(typeof(DockColors).FullName, typeof(DockColors));
		s_types.Add(typeof(LinearGradientMode).FullName, typeof(LinearGradientMode));
		s_types.Add(typeof(ControlGradient).FullName, typeof(ControlGradient));
		s_types.Add(typeof(FlatStyle).FullName, typeof(FlatStyle));
	}

	public static Type GetType(string typeName)
	{
		if (s_types.TryGetValue(typeName, out var value))
		{
			return value;
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			value = assembly.GetType(typeName);
			if (value != null)
			{
				break;
			}
		}
		s_types.Add(typeName, value);
		return value;
	}
}
