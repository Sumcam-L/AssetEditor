using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sce.Atf;

namespace AssetEditor;

public static class Resources
{
	[IconResource("AssetEditor.ico")]
	public static readonly string AssetEditorIcon;

	static Resources()
	{
		IEnumerable<MethodInfo> enumerable = from assembly in AppDomain.CurrentDomain.GetAssemblies()
			where assembly.FullName.StartsWith("Atf.Gui.WinForms")
			from type in assembly.GetExportedTypes()
			where type.Name == "ResourceUtil"
			from methodInfo in type.GetMethods()
			where methodInfo.Name == "Register" && methodInfo.IsStatic && methodInfo.IsPublic && methodInfo.ReturnType == typeof(void) && methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(Type)
			select methodInfo;
		bool flag = false;
		foreach (MethodInfo item in enumerable)
		{
			if (flag)
			{
				throw new InvalidOperationException("More than one implementation of ResourceUtil.Register(Type type) has been found.  Only the first one will be called.");
			}
			Queue queue = new Queue(1);
			queue.Enqueue(typeof(Resources));
			item.Invoke(null, queue.ToArray());
			flag = true;
		}
	}
}
