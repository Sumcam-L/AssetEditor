using System;
using System.Collections.Generic;
using System.Reflection;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

internal class PythonDocumentationProvider : DocumentationProvider
{
	private readonly PythonContext _context;

	public PythonDocumentationProvider(PythonContext context)
	{
		_context = context;
	}

	public override ICollection<MemberDoc> GetMembers(object value)
	{
		List<MemberDoc> list = new List<MemberDoc>();
		if (value is PythonModule pythonModule)
		{
			{
				foreach (KeyValuePair<object, object> item in pythonModule.__dict__)
				{
					AddMember(list, item, fromClass: false);
				}
				return list;
			}
		}
		if (value is NamespaceTracker namespaceTracker)
		{
			foreach (KeyValuePair<string, object> item2 in namespaceTracker)
			{
				AddMember(list, new KeyValuePair<object, object>(item2.Key, Importer.MemberTrackerToPython(_context.SharedClsContext, item2.Value)), fromClass: false);
			}
		}
		else if (value is OldInstance oldInstance)
		{
			foreach (KeyValuePair<object, object> item3 in oldInstance.Dictionary)
			{
				AddMember(list, item3, fromClass: false);
			}
			AddOldClassMembers(list, oldInstance._class);
		}
		else
		{
			if (value is PythonType pythonType)
			{
				foreach (PythonType item4 in pythonType.ResolutionOrder)
				{
					foreach (KeyValuePair<object, object> item5 in item4.GetMemberDictionary(_context.SharedContext))
					{
						AddMember(list, item5, fromClass: true);
					}
				}
			}
			else if (value is OldClass oc)
			{
				AddOldClassMembers(list, oc);
			}
			else
			{
				PythonType pythonType2 = DynamicHelpers.GetPythonType(value);
				foreach (KeyValuePair<object, object> item6 in pythonType2.GetMemberDictionary(_context.SharedContext))
				{
					AddMember(list, item6, fromClass: true);
				}
			}
			if (value is IPythonObject { Dict: not null } pythonObject)
			{
				foreach (KeyValuePair<object, object> item7 in pythonObject.Dict)
				{
					AddMember(list, item7, fromClass: false);
				}
			}
		}
		return list.ToArray();
	}

	private void AddOldClassMembers(List<MemberDoc> res, OldClass oc)
	{
		foreach (KeyValuePair<object, object> item in oc._dict)
		{
			AddMember(res, item, fromClass: true);
		}
		foreach (OldClass baseClass in oc.BaseClasses)
		{
			AddOldClassMembers(res, baseClass);
		}
	}

	private static void AddMember(List<MemberDoc> res, KeyValuePair<object, object> member, bool fromClass)
	{
		if (member.Key is string name)
		{
			res.Add(MakeMemberDoc(name, member.Value, fromClass));
		}
	}

	private static MemberDoc MakeMemberDoc(string name, object value, bool fromClass)
	{
		MemberKind kind = MemberKind.None;
		if (value is BuiltinFunction)
		{
			kind = MemberKind.Function;
		}
		else if (value is NamespaceTracker)
		{
			kind = MemberKind.Namespace;
		}
		else if (value is PythonFunction)
		{
			kind = ((!fromClass) ? MemberKind.Function : MemberKind.Method);
		}
		else if (value is BuiltinMethodDescriptor || value is Method)
		{
			kind = MemberKind.Method;
		}
		else if (value is PythonType)
		{
			PythonType pythonType = value as PythonType;
			kind = ((!pythonType.IsSystemType || !pythonType.UnderlyingSystemType.IsEnum()) ? MemberKind.Class : MemberKind.Enum);
		}
		else if (value is Delegate)
		{
			kind = MemberKind.Delegate;
		}
		else if (value is ReflectedProperty || value is ReflectedExtensionProperty)
		{
			kind = MemberKind.Property;
		}
		else if (value is ReflectedEvent)
		{
			kind = MemberKind.Event;
		}
		else if (value is ReflectedField)
		{
			kind = MemberKind.Field;
		}
		else if (value != null && value.GetType().IsEnum())
		{
			kind = MemberKind.EnumMember;
		}
		else if (value is PythonType || value is OldClass)
		{
			kind = MemberKind.Class;
		}
		else if (value is IPythonObject || value is OldInstance)
		{
			kind = MemberKind.Instance;
		}
		return new MemberDoc(name, kind);
	}

	public override ICollection<OverloadDoc> GetOverloads(object value)
	{
		if (value is BuiltinFunction bf)
		{
			return GetBuiltinFunctionOverloads(bf);
		}
		if (value is BuiltinMethodDescriptor builtinMethodDescriptor)
		{
			return GetBuiltinFunctionOverloads(builtinMethodDescriptor.Template);
		}
		if (value is PythonFunction pythonFunction)
		{
			return new OverloadDoc[1]
			{
				new OverloadDoc(pythonFunction.__name__, pythonFunction.__doc__ as string, GetParameterDocs(pythonFunction))
			};
		}
		if (value is Method method)
		{
			return GetOverloads(method.__func__);
		}
		if (value is Delegate obj)
		{
			return new OverloadDoc[1] { DocBuilder.GetOverloadDoc(obj.GetType().GetMethod("Invoke"), obj.GetType().Name, 0, includeSelf: false) };
		}
		return new OverloadDoc[0];
	}

	private static ICollection<ParameterDoc> GetParameterDocs(PythonFunction pf)
	{
		ParameterDoc[] array = new ParameterDoc[pf.ArgNames.Length];
		for (int i = 0; i < array.Length; i++)
		{
			ParameterFlags parameterFlags = ParameterFlags.None;
			if (i == pf.ExpandDictPosition)
			{
				parameterFlags |= ParameterFlags.ParamsDict;
			}
			else if (i == pf.ExpandListPosition)
			{
				parameterFlags |= ParameterFlags.ParamsArray;
			}
			array[i] = new ParameterDoc(pf.ArgNames[i], parameterFlags);
		}
		return array;
	}

	private static ICollection<OverloadDoc> GetBuiltinFunctionOverloads(BuiltinFunction bf)
	{
		OverloadDoc[] array = new OverloadDoc[bf.Targets.Count];
		for (int i = 0; i < bf.Targets.Count; i++)
		{
			array[i] = GetOverloadDoc(bf.__name__, bf.Targets[i]);
		}
		return array;
	}

	private static OverloadDoc GetOverloadDoc(string name, MethodBase method)
	{
		return DocBuilder.GetOverloadDoc(method, name, 0);
	}
}
