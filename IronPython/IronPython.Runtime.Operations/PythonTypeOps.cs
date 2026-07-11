using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Interpreter;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

internal static class PythonTypeOps
{
	internal struct BuiltinFunctionKey
	{
		private Type DeclaringType;

		private ReflectionCache.MethodBaseCache Cache;

		private FunctionType FunctionType;

		public BuiltinFunctionKey(Type declaringType, ReflectionCache.MethodBaseCache cache, FunctionType funcType)
		{
			Cache = cache;
			FunctionType = funcType;
			DeclaringType = declaringType;
		}
	}

	private static readonly Dictionary<FieldInfo, PythonTypeSlot> _fieldCache = new Dictionary<FieldInfo, PythonTypeSlot>();

	private static readonly Dictionary<BuiltinFunction, BuiltinMethodDescriptor> _methodCache = new Dictionary<BuiltinFunction, BuiltinMethodDescriptor>();

	private static readonly Dictionary<BuiltinFunction, ClassMethodDescriptor> _classMethodCache = new Dictionary<BuiltinFunction, ClassMethodDescriptor>();

	internal static readonly Dictionary<BuiltinFunctionKey, BuiltinFunction> _functions = new Dictionary<BuiltinFunctionKey, BuiltinFunction>();

	private static readonly Dictionary<ReflectionCache.MethodBaseCache, ConstructorFunction> _ctors = new Dictionary<ReflectionCache.MethodBaseCache, ConstructorFunction>();

	private static readonly Dictionary<EventTracker, ReflectedEvent> _eventCache = new Dictionary<EventTracker, ReflectedEvent>();

	internal static readonly Dictionary<PropertyTracker, ReflectedGetterSetter> _propertyCache = new Dictionary<PropertyTracker, ReflectedGetterSetter>();

	internal static PythonTuple MroToPython(IList<PythonType> types)
	{
		List<object> list = new List<object>(types.Count);
		foreach (PythonType type in types)
		{
			if (!(type.UnderlyingSystemType == typeof(ValueType)))
			{
				if (type.OldClass != null)
				{
					list.Add(type.OldClass);
				}
				else
				{
					list.Add(type);
				}
			}
		}
		return PythonTuple.Make(list);
	}

	internal static string GetModuleName(CodeContext context, Type type)
	{
		Type type2 = type;
		while (type2 != null)
		{
			if (PythonContext.GetContext(context).BuiltinModuleNames.TryGetValue(type2, out var value))
			{
				return value;
			}
			type2 = type2.DeclaringType;
		}
		FieldInfo field = type.GetField("__module__");
		if (field != null && field.IsLiteral && field.FieldType == typeof(string))
		{
			return (string)field.GetRawConstantValue();
		}
		return "__builtin__";
	}

	internal static object CallParams(CodeContext context, PythonType cls, params object[] argsτ)
	{
		if (argsτ == null)
		{
			argsτ = ArrayUtils.EmptyObjects;
		}
		return CallWorker(context, cls, argsτ);
	}

	internal static object CallWorker(CodeContext context, PythonType dt, object[] args)
	{
		object obj = PythonOps.CallWithContext(context, GetTypeNew(context, dt), ArrayUtils.Insert(dt, args));
		if (ShouldInvokeInit(dt, DynamicHelpers.GetPythonType(obj), args.Length))
		{
			PythonOps.CallWithContext(context, GetInitMethod(context, dt, obj), args);
			AddFinalizer(context, dt, obj);
		}
		return obj;
	}

	internal static object CallWorker(CodeContext context, PythonType dt, IDictionary<string, object> kwArgs, object[] args)
	{
		object[] array = ArrayOps.CopyArray(args, kwArgs.Count + args.Length);
		string[] array2 = new string[kwArgs.Count];
		int num = args.Length;
		foreach (KeyValuePair<string, object> kwArg in kwArgs)
		{
			array[num] = kwArg.Value;
			array2[num++ - args.Length] = kwArg.Key;
		}
		return CallWorker(context, dt, new KwCallInfo(array, array2));
	}

	internal static object CallWorker(CodeContext context, PythonType dt, KwCallInfo args)
	{
		object[] args2 = ArrayUtils.Insert(dt, args.Arguments);
		object obj = PythonOps.CallWithKeywordArgs(context, GetTypeNew(context, dt), args2, args.Names);
		if (obj == null)
		{
			return null;
		}
		if (ShouldInvokeInit(dt, DynamicHelpers.GetPythonType(obj), args.Arguments.Length))
		{
			PythonOps.CallWithKeywordArgs(context, GetInitMethod(context, dt, obj), args.Arguments, args.Names);
			AddFinalizer(context, dt, obj);
		}
		return obj;
	}

	private static object GetInitMethod(CodeContext context, PythonType dt, object newObject)
	{
		for (int i = 0; i < dt.ResolutionOrder.Count; i++)
		{
			PythonType pythonType = dt.ResolutionOrder[i];
			if (pythonType.IsOldClass && PythonOps.ToPythonType(pythonType) is OldClass oldClass && oldClass.TryGetBoundCustomMember(context, "__init__", out var value))
			{
				return oldClass.GetOldStyleDescriptor(context, value, newObject, oldClass);
			}
			if (pythonType.TryLookupSlot(context, "__init__", out var slot) && slot.TryGetValue(context, newObject, dt, out value))
			{
				return value;
			}
		}
		return null;
	}

	private static void AddFinalizer(CodeContext context, PythonType dt, object newObject)
	{
		if (dt.TryResolveSlot(context, "__del__", out var _))
		{
			IWeakReferenceable weakReferenceable = newObject as IWeakReferenceable;
			InstanceFinalizer instanceFinalizer = new InstanceFinalizer(context, newObject);
			weakReferenceable.SetFinalizer(new WeakRefTracker(instanceFinalizer, instanceFinalizer));
		}
	}

	private static object GetTypeNew(CodeContext context, PythonType dt)
	{
		if (!dt.TryResolveSlot(context, "__new__", out var slot))
		{
			throw PythonOps.TypeError("cannot create instances of {0}", dt.Name);
		}
		slot.TryGetValue(context, dt, dt, out var value);
		return value;
	}

	internal static bool IsRuntimeAssembly(Assembly assembly)
	{
		if (assembly == typeof(PythonOps).Assembly || assembly == typeof(LightCompiler).Assembly || assembly == typeof(DynamicMetaObject).Assembly)
		{
			return true;
		}
		AssemblyName assemblyName = new AssemblyName(assembly.FullName);
		if (assemblyName.Name.Equals("IronPython.Modules"))
		{
			return true;
		}
		return false;
	}

	private static bool ShouldInvokeInit(PythonType cls, PythonType newObjectType, int argCnt)
	{
		if ((!cls.IsSystemType || cls.IsPythonType) && newObjectType.IsSubclassOf(cls))
		{
			if (cls == TypeCache.PythonType)
			{
				return argCnt > 1;
			}
			return true;
		}
		return false;
	}

	internal static string GetName(object o)
	{
		return DynamicHelpers.GetPythonType(o).Name;
	}

	internal static string GetOldName(object o)
	{
		if (!(o is OldInstance))
		{
			return GetName(o);
		}
		return GetOldName((OldInstance)o);
	}

	internal static string GetOldName(OldInstance instance)
	{
		return instance._class.Name;
	}

	internal static PythonType[] ObjectTypes(object[] args)
	{
		PythonType[] array = new PythonType[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			array[i] = DynamicHelpers.GetPythonType(args[i]);
		}
		return array;
	}

	internal static Type[] ConvertToTypes(PythonType[] pythonTypes)
	{
		Type[] array = new Type[pythonTypes.Length];
		for (int i = 0; i < pythonTypes.Length; i++)
		{
			array[i] = ConvertToType(pythonTypes[i]);
		}
		return array;
	}

	private static Type ConvertToType(PythonType pythonType)
	{
		if (pythonType.IsNull)
		{
			return typeof(DynamicNull);
		}
		return pythonType.UnderlyingSystemType;
	}

	internal static TrackerTypes GetMemberType(MemberGroup members)
	{
		TrackerTypes trackerTypes = TrackerTypes.All;
		for (int i = 0; i < members.Count; i++)
		{
			MemberTracker memberTracker = members[i];
			if (memberTracker.MemberType != trackerTypes)
			{
				if (trackerTypes != TrackerTypes.All)
				{
					return TrackerTypes.All;
				}
				trackerTypes = memberTracker.MemberType;
			}
		}
		return trackerTypes;
	}

	internal static PythonTypeSlot GetSlot(MemberGroup group, string name, bool privateBinding)
	{
		if (group.Count == 0)
		{
			return null;
		}
		group = FilterNewSlots(group);
		TrackerTypes memberType = GetMemberType(group);
		switch (memberType)
		{
		case TrackerTypes.Method:
		{
			bool flag = false;
			List<MemberInfo> list = new List<MemberInfo>();
			foreach (MemberTracker item in group)
			{
				MethodTracker methodTracker = (MethodTracker)item;
				list.Add(methodTracker.Method);
				flag |= methodTracker.IsStatic;
			}
			Type declaringType = group[0].DeclaringType;
			MemberInfo[] array = list.ToArray();
			FunctionType methodFunctionType = GetMethodFunctionType(declaringType, array, flag);
			return GetFinalSlotForFunction(GetBuiltinFunction(declaringType, group[0].Name, name, methodFunctionType, array));
		}
		case TrackerTypes.Field:
			return GetReflectedField(((FieldTracker)group[0]).Field);
		case TrackerTypes.Property:
			return GetReflectedProperty((PropertyTracker)group[0], group, privateBinding);
		case TrackerTypes.Event:
			return GetReflectedEvent((EventTracker)group[0]);
		case TrackerTypes.Type:
		{
			TypeTracker typeTracker = (TypeTracker)group[0];
			for (int i = 1; i < group.Count; i++)
			{
				typeTracker = TypeGroup.UpdateTypeEntity(typeTracker, (TypeTracker)group[i]);
			}
			if (typeTracker is TypeGroup)
			{
				return new PythonTypeUserDescriptorSlot(typeTracker, isntDescriptor: true);
			}
			return new PythonTypeUserDescriptorSlot(DynamicHelpers.GetPythonTypeFromType(typeTracker.Type), isntDescriptor: true);
		}
		case TrackerTypes.Constructor:
			return GetConstructor(group[0].DeclaringType, privateBinding);
		case TrackerTypes.Custom:
			return ((PythonCustomTracker)group[0]).GetSlot();
		default:
			throw new InvalidOperationException($"Bad member type {memberType.ToString()} on {group[0].DeclaringType}.{name}");
		}
	}

	internal static MemberGroup FilterNewSlots(MemberGroup group)
	{
		if (GetMemberType(group) == TrackerTypes.All)
		{
			Type declaringType = group[0].DeclaringType;
			for (int i = 1; i < group.Count; i++)
			{
				if (group[i].DeclaringType != declaringType && group[i].DeclaringType.IsSubclassOf(declaringType))
				{
					declaringType = group[i].DeclaringType;
				}
			}
			List<MemberTracker> list = new List<MemberTracker>();
			for (int j = 0; j < group.Count; j++)
			{
				if (group[j].DeclaringType == declaringType)
				{
					list.Add(group[j]);
				}
			}
			if (list.Count != group.Count)
			{
				return new MemberGroup(list.ToArray());
			}
		}
		return group;
	}

	private static BuiltinFunction GetConstructor(Type t, bool privateBinding)
	{
		BuiltinFunction nonDefaultNewInst = InstanceOps.NonDefaultNewInst;
		MethodBase[] constructors = CompilerHelpers.GetConstructors(t, privateBinding, includeProtected: true);
		return GetConstructor(t, nonDefaultNewInst, constructors);
	}

	internal static bool IsDefaultNew(MethodBase[] targets)
	{
		if (targets.Length == 1)
		{
			ParameterInfo[] parameters = targets[0].GetParameters();
			if (parameters.Length == 0)
			{
				return true;
			}
			if (parameters.Length == 1 && parameters[0].ParameterType == typeof(CodeContext))
			{
				return true;
			}
		}
		return false;
	}

	internal static BuiltinFunction GetConstructorFunction(Type type, string name)
	{
		List<MethodBase> list = new List<MethodBase>();
		bool flag = false;
		ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			if (constructorInfo.IsPublic)
			{
				if (constructorInfo.GetParameters().Length == 0)
				{
					flag = true;
				}
				list.Add(constructorInfo);
			}
		}
		if (type.IsValueType && !flag && type != typeof(void))
		{
			try
			{
				list.Add(typeof(ScriptingRuntimeHelpers).GetMethod("CreateInstance", ReflectionUtils.EmptyTypes).MakeGenericMethod(type));
			}
			catch (BadImageFormatException)
			{
			}
		}
		if (list.Count > 0)
		{
			return BuiltinFunction.MakeFunction(name, list.ToArray(), type);
		}
		return null;
	}

	internal static ReflectedEvent GetReflectedEvent(EventTracker tracker)
	{
		ReflectedEvent value;
		lock (_eventCache)
		{
			if (!_eventCache.TryGetValue(tracker, out value))
			{
				value = (PythonBinder.IsExtendedType(tracker.DeclaringType) ? (_eventCache[tracker] = new ReflectedEvent(tracker, clsOnly: true)) : (_eventCache[tracker] = new ReflectedEvent(tracker, clsOnly: false)));
			}
		}
		return value;
	}

	internal static PythonTypeSlot GetFinalSlotForFunction(BuiltinFunction func)
	{
		if ((func.FunctionType & FunctionType.Method) != FunctionType.None)
		{
			lock (_methodCache)
			{
				if (!_methodCache.TryGetValue(func, out var value))
				{
					value = (_methodCache[func] = new BuiltinMethodDescriptor(func));
				}
				return value;
			}
		}
		if (func.Targets[0].IsDefined(typeof(ClassMethodAttribute), inherit: true))
		{
			lock (_classMethodCache)
			{
				if (!_classMethodCache.TryGetValue(func, out var value2))
				{
					value2 = (_classMethodCache[func] = new ClassMethodDescriptor(func));
				}
				return value2;
			}
		}
		return func;
	}

	internal static BuiltinFunction GetBuiltinFunction(Type type, string name, MemberInfo[] mems)
	{
		return GetBuiltinFunction(type, name, null, mems);
	}

	public static MethodBase[] GetNonBaseHelperMethodInfos(MemberInfo[] members)
	{
		List<MethodBase> list = new List<MethodBase>();
		foreach (MemberInfo memberInfo in members)
		{
			MethodBase methodBase = memberInfo as MethodBase;
			if (methodBase != null && !methodBase.Name.StartsWith("#base#"))
			{
				list.Add(methodBase);
			}
		}
		return list.ToArray();
	}

	public static MemberInfo[] GetNonBaseHelperMemberInfos(MemberInfo[] members)
	{
		List<MemberInfo> list = new List<MemberInfo>(members.Length);
		foreach (MemberInfo memberInfo in members)
		{
			MethodBase methodBase = memberInfo as MethodBase;
			if (methodBase == null || !methodBase.Name.StartsWith("#base#"))
			{
				list.Add(memberInfo);
			}
		}
		return list.ToArray();
	}

	internal static BuiltinFunction GetBuiltinFunction(Type type, string name, FunctionType? funcType, params MemberInfo[] mems)
	{
		return GetBuiltinFunction(type, name, name, funcType, mems);
	}

	internal static BuiltinFunction GetBuiltinFunction(Type type, string cacheName, string pythonName, FunctionType? funcType, params MemberInfo[] mems)
	{
		BuiltinFunction value = null;
		if (mems.Length != 0)
		{
			FunctionType functionType = funcType ?? GetMethodFunctionType(type, mems);
			type = GetBaseDeclaringType(type, mems);
			BuiltinFunctionKey key = new BuiltinFunctionKey(type, new ReflectionCache.MethodBaseCache(cacheName, GetNonBaseHelperMethodInfos(mems)), functionType);
			lock (_functions)
			{
				if (!_functions.TryGetValue(key, out value))
				{
					if (GetFinalSystemType(type) == type)
					{
						IList<MethodInfo> overriddenMethods = NewTypeMaker.GetOverriddenMethods(type, cacheName);
						if (overriddenMethods.Count > 0)
						{
							List<MemberInfo> list = new List<MemberInfo>(mems);
							foreach (MethodInfo item in overriddenMethods)
							{
								list.Add(item);
							}
							mems = list.ToArray();
						}
					}
					value = (_functions[key] = BuiltinFunction.MakeMethod(pythonName, ReflectionUtils.GetMethodInfos(mems), type, functionType));
				}
			}
		}
		return value;
	}

	private static Type GetCommonBaseType(Type xType, Type yType)
	{
		if (xType.IsSubclassOf(yType))
		{
			return yType;
		}
		if (yType.IsSubclassOf(xType))
		{
			return xType;
		}
		if (xType == yType)
		{
			return xType;
		}
		Type baseType = xType.BaseType;
		Type baseType2 = yType.BaseType;
		if (baseType != null)
		{
			Type commonBaseType = GetCommonBaseType(baseType, yType);
			if (commonBaseType != null)
			{
				return commonBaseType;
			}
		}
		if (baseType2 != null)
		{
			Type commonBaseType2 = GetCommonBaseType(xType, baseType2);
			if (commonBaseType2 != null)
			{
				return commonBaseType2;
			}
		}
		return null;
	}

	private static Type GetBaseDeclaringType(Type type, MemberInfo[] mems)
	{
		Array.Sort(mems, delegate(MemberInfo x, MemberInfo y)
		{
			if (x.DeclaringType.IsSubclassOf(y.DeclaringType))
			{
				return -1;
			}
			if (y.DeclaringType.IsSubclassOf(x.DeclaringType))
			{
				return 1;
			}
			if (x.DeclaringType == y.DeclaringType)
			{
				return 0;
			}
			type = GetCommonBaseType(x.DeclaringType, y.DeclaringType) ?? typeof(object);
			if (x.DeclaringType.FullName == null)
			{
				return -1;
			}
			return (y.DeclaringType.FullName == null) ? 1 : x.DeclaringType.FullName.CompareTo(y.DeclaringType.FullName);
		});
		foreach (MemberInfo memberInfo in mems)
		{
			if (memberInfo.DeclaringType.IsAssignableFrom(type) && (type == memberInfo.DeclaringType || type.IsSubclassOf(memberInfo.DeclaringType)))
			{
				type = memberInfo.DeclaringType;
				break;
			}
		}
		return type;
	}

	internal static ConstructorFunction GetConstructor(Type type, BuiltinFunction realTarget, params MethodBase[] mems)
	{
		ConstructorFunction value = null;
		if (mems.Length != 0)
		{
			ReflectionCache.MethodBaseCache key = new ReflectionCache.MethodBaseCache("__new__", mems);
			lock (_ctors)
			{
				if (!_ctors.TryGetValue(key, out value))
				{
					value = (_ctors[key] = new ConstructorFunction(realTarget, mems));
				}
			}
		}
		return value;
	}

	internal static FunctionType GetMethodFunctionType(Type type, MemberInfo[] methods)
	{
		return GetMethodFunctionType(type, methods, checkStatic: true);
	}

	internal static FunctionType GetMethodFunctionType(Type type, MemberInfo[] methods, bool checkStatic)
	{
		FunctionType functionType = FunctionType.None;
		for (int i = 0; i < methods.Length; i++)
		{
			MethodInfo methodInfo = (MethodInfo)methods[i];
			if (methodInfo.IsStatic && methodInfo.IsSpecialName)
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if ((parameters.Length == 2 && parameters[0].ParameterType != typeof(CodeContext)) || (parameters.Length == 3 && parameters[0].ParameterType == typeof(CodeContext)))
				{
					functionType |= FunctionType.BinaryOperator;
					if (parameters[parameters.Length - 2].ParameterType != type && parameters[parameters.Length - 1].ParameterType == type)
					{
						functionType |= FunctionType.ReversedOperator;
					}
				}
			}
			functionType = ((!checkStatic || !IsStaticFunction(type, methodInfo)) ? (functionType | FunctionType.Method) : (functionType | FunctionType.Function));
		}
		if (IsMethodAlwaysVisible(type, methods))
		{
			functionType |= FunctionType.AlwaysVisible;
		}
		return functionType;
	}

	private static bool IsMethodAlwaysVisible(Type type, MemberInfo[] methods)
	{
		bool result = true;
		if (PythonBinder.IsPythonType(type))
		{
			for (int i = 0; i < methods.Length; i++)
			{
				MethodInfo methodInfo = (MethodInfo)methods[i];
				if (PythonBinder.IsExtendedType(methodInfo.DeclaringType) || PythonBinder.IsExtendedType(methodInfo.GetBaseDefinition().DeclaringType) || methodInfo.IsDefined(typeof(PythonHiddenAttribute), inherit: false))
				{
					result = false;
					break;
				}
			}
		}
		else if (typeof(IPythonObject).IsAssignableFrom(type))
		{
			for (int j = 0; j < methods.Length; j++)
			{
				MethodInfo methodInfo2 = (MethodInfo)methods[j];
				if (PythonBinder.IsExtendedType(methodInfo2.DeclaringType))
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private static bool IsStaticFunction(Type type, MethodInfo mi)
	{
		if (mi.IsStatic && !mi.IsDefined(typeof(WrapperDescriptorAttribute), inherit: false))
		{
			if (!mi.DeclaringType.IsAssignableFrom(type))
			{
				return mi.IsDefined(typeof(StaticExtensionMethodAttribute), inherit: false);
			}
			return true;
		}
		return false;
	}

	internal static PythonTypeSlot GetReflectedField(FieldInfo info)
	{
		NameType nameType = NameType.Field;
		if (!PythonBinder.IsExtendedType(info.DeclaringType) && !info.IsDefined(typeof(PythonHiddenAttribute), inherit: false))
		{
			nameType |= NameType.PythonField;
		}
		PythonTypeSlot value;
		lock (_fieldCache)
		{
			if (!_fieldCache.TryGetValue(info, out value))
			{
				value = ((nameType != NameType.PythonField || !info.IsLiteral) ? ((PythonTypeSlot)new ReflectedField(info, nameType)) : ((PythonTypeSlot)((info.FieldType == typeof(int)) ? new PythonTypeUserDescriptorSlot(ScriptingRuntimeHelpers.Int32ToObject((int)info.GetRawConstantValue()), isntDescriptor: true) : ((!(info.FieldType == typeof(bool))) ? new PythonTypeUserDescriptorSlot(info.GetValue(null), isntDescriptor: true) : new PythonTypeUserDescriptorSlot(ScriptingRuntimeHelpers.BooleanToObject((bool)info.GetRawConstantValue()), isntDescriptor: true)))));
				_fieldCache[info] = value;
			}
		}
		return value;
	}

	internal static string GetDocumentation(Type type)
	{
		object[] customAttributes = type.GetCustomAttributes(typeof(DocumentationAttribute), inherit: false);
		if (customAttributes != null && customAttributes.Length > 0)
		{
			return ((DocumentationAttribute)customAttributes[0]).Documentation;
		}
		if (type == typeof(DynamicNull))
		{
			return null;
		}
		string text = DocBuilder.CreateAutoDoc(type);
		text = ((text != null) ? (text + Environment.NewLine + Environment.NewLine) : string.Empty);
		ConstructorInfo[] constructors = type.GetConstructors();
		ConstructorInfo[] array = constructors;
		foreach (ConstructorInfo info in array)
		{
			text = text + FixCtorDoc(type, DocBuilder.CreateAutoDoc(info, DynamicHelpers.GetPythonTypeFromType(type).Name, 0)) + Environment.NewLine;
		}
		return text;
	}

	private static string FixCtorDoc(Type type, string autoDoc)
	{
		return autoDoc.Replace("__new__(cls)", DynamicHelpers.GetPythonTypeFromType(type).Name + "()").Replace("__new__(cls, ", DynamicHelpers.GetPythonTypeFromType(type).Name + "(");
	}

	internal static ReflectedGetterSetter GetReflectedProperty(PropertyTracker pt, MemberGroup allProperties, bool privateBinding)
	{
		lock (_propertyCache)
		{
			if (_propertyCache.TryGetValue(pt, out var value))
			{
				return value;
			}
			NameType nt = NameType.PythonProperty;
			MethodInfo methodInfo = FilterProtectedGetterOrSetter(pt.GetGetMethod(privateMembers: true), privateBinding);
			MethodInfo methodInfo2 = FilterProtectedGetterOrSetter(pt.GetSetMethod(privateMembers: true), privateBinding);
			if ((methodInfo != null && methodInfo.IsDefined(typeof(PythonHiddenAttribute), inherit: true)) || (methodInfo2 != null && methodInfo2.IsDefined(typeof(PythonHiddenAttribute), inherit: true)))
			{
				nt = NameType.Property;
			}
			if (!(pt is ExtensionPropertyTracker))
			{
				ReflectedPropertyTracker reflectedPropertyTracker = pt as ReflectedPropertyTracker;
				if (PythonBinder.IsExtendedType(pt.DeclaringType) || reflectedPropertyTracker.Property.IsDefined(typeof(PythonHiddenAttribute), inherit: true))
				{
					nt = NameType.Property;
				}
				if (pt.GetIndexParameters().Length == 0)
				{
					List<MethodInfo> list = new List<MethodInfo>();
					List<MethodInfo> list2 = new List<MethodInfo>();
					IList<ExtensionPropertyTracker> overriddenProperties = NewTypeMaker.GetOverriddenProperties((methodInfo ?? methodInfo2).DeclaringType, pt.Name);
					foreach (ExtensionPropertyTracker item in overriddenProperties)
					{
						MethodInfo getMethod = item.GetGetMethod(privateBinding);
						if (getMethod != null)
						{
							list.Add(getMethod);
						}
						getMethod = item.GetSetMethod(privateBinding);
						if (getMethod != null)
						{
							list2.Add(getMethod);
						}
					}
					foreach (PropertyTracker allProperty in allProperties)
					{
						MethodInfo getMethod2 = allProperty.GetGetMethod(privateBinding);
						if (getMethod2 != null)
						{
							list.Add(getMethod2);
						}
						getMethod2 = allProperty.GetSetMethod(privateBinding);
						if (getMethod2 != null)
						{
							list2.Add(getMethod2);
						}
					}
					value = new ReflectedProperty(reflectedPropertyTracker.Property, list.ToArray(), list2.ToArray(), nt);
				}
				else
				{
					value = new ReflectedIndexer(((ReflectedPropertyTracker)pt).Property, NameType.Property, privateBinding);
				}
			}
			else
			{
				value = new ReflectedExtensionProperty(new ExtensionPropertyInfo(pt.DeclaringType, methodInfo ?? methodInfo2), nt);
			}
			_propertyCache[pt] = value;
			return value;
		}
	}

	private static MethodInfo FilterProtectedGetterOrSetter(MethodInfo info, bool privateBinding)
	{
		if (info != null)
		{
			if (privateBinding || info.IsPublic)
			{
				return info;
			}
			if (info.IsProtected())
			{
				return info;
			}
		}
		return null;
	}

	internal static bool TryInvokeUnaryOperator(CodeContext context, object o, string name, out object value)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(o);
		if (pythonType.TryResolveMixedSlot(context, name, out var slot) && slot.TryGetValue(context, o, pythonType, out var value2))
		{
			value = PythonCalls.Call(context, value2);
			return true;
		}
		value = null;
		return false;
	}

	internal static bool TryInvokeBinaryOperator(CodeContext context, object o, object arg1, string name, out object value)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(o);
		if (pythonType.TryResolveMixedSlot(context, name, out var slot) && slot.TryGetValue(context, o, pythonType, out var value2))
		{
			value = PythonCalls.Call(context, value2, arg1);
			return true;
		}
		value = null;
		return false;
	}

	internal static bool TryInvokeTernaryOperator(CodeContext context, object o, object arg1, object arg2, string name, out object value)
	{
		PythonType pythonType = DynamicHelpers.GetPythonType(o);
		if (pythonType.TryResolveMixedSlot(context, name, out var slot) && slot.TryGetValue(context, o, pythonType, out var value2))
		{
			value = PythonCalls.Call(context, value2, arg1, arg2);
			return true;
		}
		value = null;
		return false;
	}

	internal static PythonTuple EnsureBaseType(PythonTuple bases)
	{
		bool flag = false;
		foreach (object basis in bases)
		{
			if (!(basis is OldClass))
			{
				PythonType pythonType = basis as PythonType;
				if (!pythonType.UnderlyingSystemType.IsInterface)
				{
					return bases;
				}
				flag = true;
			}
		}
		if (flag || bases.Count == 0)
		{
			return new PythonTuple(bases, TypeCache.Object);
		}
		throw PythonOps.TypeError("a new-style class can't have only classic bases");
	}

	internal static Type GetFinalSystemType(Type type)
	{
		while (typeof(IPythonObject).IsAssignableFrom(type) && !type.IsDefined(typeof(DynamicBaseTypeAttribute), inherit: false))
		{
			type = type.BaseType;
		}
		return type;
	}
}
