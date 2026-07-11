using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Types;

internal sealed class NewTypeMaker
{
	public const string VtableNamesField = "#VTableNames#";

	public const string TypePrefix = "IronPython.NewTypes.";

	public const string BaseMethodPrefix = "#base#";

	public const string FieldGetterPrefix = "#field_get#";

	public const string FieldSetterPrefix = "#field_set#";

	public const string ClassFieldName = ".class";

	public const string DictFieldName = ".dict";

	public const string SlotsAndWeakRefFieldName = ".slots_and_weakref";

	private const string _constructorTypeName = "PythonCachedTypeConstructor";

	private const string _constructorMethodName = "GetTypeInfo";

	private const MethodAttributes MethodAttributesToEraseInOveride = MethodAttributes.ReservedMask | MethodAttributes.Abstract;

	private Type _baseType;

	private IList<Type> _interfaceTypes;

	private TypeBuilder _tg;

	private FieldInfo _typeField;

	private FieldInfo _dictField;

	private FieldInfo _slotsField;

	private FieldInfo _explicitMO;

	private ILGen _cctor;

	private int _site;

	private static int _typeCount;

	internal static readonly Publisher<NewTypeInfo, Type> _newTypes = new Publisher<NewTypeInfo, Type>();

	private static readonly Dictionary<Type, Dictionary<string, List<MethodInfo>>> _overriddenMethods = new Dictionary<Type, Dictionary<string, List<MethodInfo>>>();

	private static readonly Dictionary<Type, Dictionary<string, List<ExtensionPropertyTracker>>> _overriddenProperties = new Dictionary<Type, Dictionary<string, List<ExtensionPropertyTracker>>>();

	private bool NeedsPythonObject => !typeof(IPythonObject).IsAssignableFrom(_baseType);

	private NewTypeMaker(NewTypeInfo typeInfo)
	{
		_baseType = typeInfo.BaseType;
		_interfaceTypes = typeInfo.InterfaceTypes;
	}

	public static Type GetNewType(string typeName, PythonTuple bases)
	{
		NewTypeInfo typeInfo = NewTypeInfo.GetTypeInfo(typeName, bases);
		if (typeInfo.BaseType.IsValueType())
		{
			throw PythonOps.TypeError("cannot derive from {0} because it is a value type", typeInfo.BaseType.FullName);
		}
		if (typeInfo.BaseType.IsSealed())
		{
			throw PythonOps.TypeError("cannot derive from {0} because it is sealed", typeInfo.BaseType.FullName);
		}
		return _newTypes.GetOrCreateValue(typeInfo, delegate
		{
			if (typeInfo.InterfaceTypes.Count == 0)
			{
				object[] customAttributes = typeInfo.BaseType.GetCustomAttributes(typeof(DynamicBaseTypeAttribute), inherit: false);
				if (customAttributes.Length > 0)
				{
					return typeInfo.BaseType;
				}
			}
			return new NewTypeMaker(typeInfo).CreateNewType();
		});
	}

	public static void SaveNewTypes(string assemblyName, IList<PythonTuple> types)
	{
		AssemblyGen assemblyGen = new AssemblyGen(new AssemblyName(assemblyName), ".", ".dll", isDebuggable: false);
		TypeBuilder typeBuilder = assemblyGen.DefinePublicType("PythonCachedTypeConstructor", typeof(object), preserveName: true);
		typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(PythonCachedTypeInfoAttribute).GetConstructor(ReflectionUtils.EmptyTypes), new object[0]));
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("GetTypeInfo", MethodAttributes.Public | MethodAttributes.Static, typeof(CachedNewTypeInfo[]), ReflectionUtils.EmptyTypes);
		ILGenerator iLGenerator = methodBuilder.GetILGenerator();
		EmitInt(iLGenerator, types.Count);
		iLGenerator.Emit(OpCodes.Newarr, typeof(CachedNewTypeInfo));
		int num = 0;
		foreach (PythonTuple type in types)
		{
			NewTypeInfo typeInfo = NewTypeInfo.GetTypeInfo(string.Empty, type);
			KeyValuePair<Type, Dictionary<string, string[]>> keyValuePair = new NewTypeMaker(typeInfo).SaveType(assemblyGen, "Python" + _typeCount++ + "$" + typeInfo.BaseType.Name);
			iLGenerator.Emit(OpCodes.Dup);
			EmitInt(iLGenerator, num++);
			iLGenerator.Emit(OpCodes.Ldtoken, keyValuePair.Key);
			iLGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
			iLGenerator.Emit(OpCodes.Newobj, typeof(Dictionary<string, string[]>).GetConstructor(new Type[0]));
			foreach (KeyValuePair<string, string[]> item in keyValuePair.Value)
			{
				iLGenerator.Emit(OpCodes.Dup);
				iLGenerator.Emit(OpCodes.Ldstr, item.Key);
				int iVal = item.Value.Length;
				EmitInt(iLGenerator, iVal);
				iLGenerator.Emit(OpCodes.Newarr, typeof(string));
				for (int i = 0; i < item.Value.Length; i++)
				{
					iLGenerator.Emit(OpCodes.Dup);
					EmitInt(iLGenerator, i);
					iLGenerator.Emit(OpCodes.Ldstr, item.Value[0]);
					iLGenerator.Emit(OpCodes.Stelem_Ref);
				}
				iLGenerator.Emit(OpCodes.Call, typeof(Dictionary<string, string[]>).GetMethod("set_Item"));
			}
			if (typeInfo.InterfaceTypes.Count != 0)
			{
				EmitInt(iLGenerator, typeInfo.InterfaceTypes.Count);
				iLGenerator.Emit(OpCodes.Newarr, typeof(Type));
				for (int j = 0; j < typeInfo.InterfaceTypes.Count; j++)
				{
					iLGenerator.Emit(OpCodes.Dup);
					EmitInt(iLGenerator, j);
					iLGenerator.Emit(OpCodes.Ldtoken, typeInfo.InterfaceTypes[j]);
					iLGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
					iLGenerator.Emit(OpCodes.Stelem_Ref);
				}
			}
			else
			{
				iLGenerator.Emit(OpCodes.Ldnull);
			}
			iLGenerator.Emit(OpCodes.Newobj, typeof(CachedNewTypeInfo).GetConstructors()[0]);
			iLGenerator.Emit(OpCodes.Stelem_Ref);
		}
		iLGenerator.Emit(OpCodes.Ret);
		typeBuilder.CreateType();
		assemblyGen.SaveAssembly();
	}

	public static void LoadNewTypes(Assembly asm)
	{
		Type type = asm.GetType("PythonCachedTypeConstructor");
		if (type == null || !type.IsDefined(typeof(PythonCachedTypeInfoAttribute), inherit: false))
		{
			return;
		}
		MethodInfo method = type.GetMethod("GetTypeInfo");
		CachedNewTypeInfo[] array = (CachedNewTypeInfo[])method.Invoke(null, new object[0]);
		CachedNewTypeInfo[] array2 = array;
		CachedNewTypeInfo v;
		for (int i = 0; i < array2.Length; i++)
		{
			v = array2[i];
			_newTypes.GetOrCreateValue(new NewTypeInfo(v.Type.GetBaseType(), v.InterfaceTypes), delegate
			{
				AddBaseMethods(v.Type, v.SpecialNames);
				return v.Type;
			});
		}
	}

	public static bool IsInstanceType(Type type)
	{
		if (type.FullName.IndexOf("IronPython.NewTypes.") != 0)
		{
			if (type.GetBaseType() != null)
			{
				return IsInstanceType(type.GetBaseType());
			}
			return false;
		}
		return true;
	}

	private Type CreateNewType()
	{
		string name = GetName();
		_tg = Snippets.Shared.DefinePublicType("IronPython.NewTypes." + name, _baseType);
		Dictionary<string, string[]> specialNames = ImplementType();
		Type type = FinishType();
		AddBaseMethods(type, specialNames);
		return type;
	}

	private string GetName()
	{
		StringBuilder stringBuilder = new StringBuilder(_baseType.Namespace);
		stringBuilder.Append('.');
		stringBuilder.Append(_baseType.Name);
		foreach (Type interfaceType in _interfaceTypes)
		{
			stringBuilder.Append("#");
			stringBuilder.Append(interfaceType.Name);
		}
		stringBuilder.Append("_");
		stringBuilder.Append(Interlocked.Increment(ref _typeCount));
		return stringBuilder.ToString();
	}

	private Dictionary<string, string[]> ImplementType()
	{
		DefineInterfaces();
		ImplementPythonObject();
		ImplementConstructors();
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();
		OverrideMethods(_baseType, dictionary);
		ImplementProtectedFieldAccessors(dictionary);
		Dictionary<Type, bool> doneTypes = new Dictionary<Type, bool>();
		foreach (Type interfaceType in _interfaceTypes)
		{
			DoInterfaceType(interfaceType, doneTypes, dictionary);
		}
		return dictionary;
	}

	private void DefineInterfaces()
	{
		foreach (Type interfaceType in _interfaceTypes)
		{
			ImplementInterface(interfaceType);
		}
	}

	private void ImplementInterface(Type interfaceType)
	{
		_tg.AddInterfaceImplementation(interfaceType);
	}

	private void ImplementPythonObject()
	{
		ImplementIPythonObject();
		ImplementDynamicObject();
		ImplementCustomTypeDescriptor();
		ImplementWeakReference();
		AddDebugView();
	}

	private void AddDebugView()
	{
		_tg.SetCustomAttribute(new CustomAttributeBuilder(typeof(DebuggerTypeProxyAttribute).GetConstructor(new Type[1] { typeof(Type) }), new object[1] { typeof(UserTypeDebugView) }));
		_tg.SetCustomAttribute(new CustomAttributeBuilder(typeof(DebuggerDisplayAttribute).GetConstructor(new Type[1] { typeof(string) }), new object[1] { "{get_PythonType().GetTypeDebuggerDisplay()}" }));
	}

	private void EmitGetDict(ILGen gen)
	{
		gen.EmitFieldGet(_dictField);
	}

	private void EmitSetDict(ILGen gen)
	{
		gen.EmitFieldSet(_dictField);
	}

	private ParameterInfo[] GetOverrideCtorSignature(ParameterInfo[] original)
	{
		if (typeof(IPythonObject).IsAssignableFrom(_baseType))
		{
			return original;
		}
		ParameterInfo[] array = new ParameterInfo[original.Length + 1];
		if (original.Length == 0 || original[0].ParameterType != typeof(CodeContext))
		{
			array[0] = new ParameterInfoWrapper(typeof(PythonType), "cls");
			Array.Copy(original, 0, array, 1, array.Length - 1);
		}
		else
		{
			array[0] = original[0];
			array[1] = new ParameterInfoWrapper(typeof(PythonType), "cls");
			Array.Copy(original, 1, array, 2, array.Length - 2);
		}
		return array;
	}

	private void ImplementConstructors()
	{
		ConstructorInfo[] constructors = _baseType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		ConstructorInfo[] array = constructors;
		foreach (ConstructorInfo constructorInfo in array)
		{
			if (constructorInfo.IsPublic || constructorInfo.IsProtected())
			{
				OverrideConstructor(constructorInfo);
			}
		}
	}

	private static bool CanOverrideMethod(MethodInfo mi)
	{
		return true;
	}

	private void DoInterfaceType(Type interfaceType, Dictionary<Type, bool> doneTypes, Dictionary<string, string[]> specialNames)
	{
		if (!(interfaceType == typeof(IDynamicMetaObjectProvider)) && !doneTypes.ContainsKey(interfaceType))
		{
			doneTypes.Add(interfaceType, value: true);
			OverrideMethods(interfaceType, specialNames);
			Type[] interfaces = interfaceType.GetInterfaces();
			foreach (Type interfaceType2 in interfaces)
			{
				DoInterfaceType(interfaceType2, doneTypes, specialNames);
			}
		}
	}

	private void OverrideConstructor(ConstructorInfo parentConstructor)
	{
		ParameterInfo[] parameters = parentConstructor.GetParameters();
		if (parameters.Length == 0 && typeof(IPythonObject).IsAssignableFrom(_baseType))
		{
			return;
		}
		ParameterInfo[] overrideCtorSignature = GetOverrideCtorSignature(parameters);
		Type[] array = new Type[overrideCtorSignature.Length];
		string[] array2 = new string[overrideCtorSignature.Length];
		for (int i = 0; i < overrideCtorSignature.Length; i++)
		{
			array[i] = overrideCtorSignature[i].ParameterType;
			array2[i] = overrideCtorSignature[i].Name;
		}
		ConstructorBuilder constructorBuilder = _tg.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, array);
		for (int j = 0; j < overrideCtorSignature.Length; j++)
		{
			ParameterBuilder parameterBuilder = constructorBuilder.DefineParameter(j + 1, overrideCtorSignature[j].Attributes, overrideCtorSignature[j].Name);
			int originalIndex = GetOriginalIndex(parameters, overrideCtorSignature, j);
			if (originalIndex < 0)
			{
				continue;
			}
			ParameterInfo parameterInfo = parameters[originalIndex];
			if (parameterInfo.IsDefined(typeof(ParamArrayAttribute), inherit: false))
			{
				parameterBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(ParamArrayAttribute).GetConstructor(ReflectionUtils.EmptyTypes), ArrayUtils.EmptyObjects));
			}
			else if (parameterInfo.IsDefined(typeof(ParamDictionaryAttribute), inherit: false))
			{
				parameterBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(ParamDictionaryAttribute).GetConstructor(ReflectionUtils.EmptyTypes), ArrayUtils.EmptyObjects));
			}
			if ((parameterInfo.Attributes & ParameterAttributes.HasDefault) != ParameterAttributes.None)
			{
				if (parameterInfo.DefaultValue == null || parameterInfo.ParameterType.IsAssignableFrom(parameterInfo.DefaultValue.GetType()))
				{
					parameterBuilder.SetConstant(parameterInfo.DefaultValue);
				}
				else
				{
					parameterBuilder.SetConstant(Convert.ChangeType(parameterInfo.DefaultValue, parameterInfo.ParameterType, Thread.CurrentThread.CurrentCulture));
				}
			}
		}
		ILGen iLGen = new ILGen(constructorBuilder.GetILGenerator());
		int index = ((parameters.Length == 0 || parameters[0].ParameterType != typeof(CodeContext)) ? 1 : 2);
		if (!typeof(IPythonObject).IsAssignableFrom(_baseType))
		{
			iLGen.EmitLoadArg(0);
			iLGen.EmitLoadArg(index);
			iLGen.EmitFieldSet(_typeField);
		}
		if (_explicitMO != null)
		{
			iLGen.Emit(OpCodes.Ldarg_0);
			iLGen.EmitNew(_explicitMO.FieldType.GetConstructor(ReflectionUtils.EmptyTypes));
			iLGen.Emit(OpCodes.Stfld, _explicitMO);
		}
		MethodInfo method = typeof(PythonOps).GetMethod("InitializeUserTypeSlots");
		iLGen.EmitLoadArg(0);
		iLGen.EmitLoadArg(index);
		iLGen.EmitCall(method);
		iLGen.EmitFieldSet(_slotsField);
		CallBaseConstructor(parentConstructor, parameters, overrideCtorSignature, iLGen);
	}

	private static int GetOriginalIndex(ParameterInfo[] pis, ParameterInfo[] overrideParams, int i)
	{
		if (pis.Length == 0 || pis[0].ParameterType != typeof(CodeContext))
		{
			return i - (overrideParams.Length - pis.Length);
		}
		return i switch
		{
			1 => -1, 
			0 => 0, 
			_ => i - (overrideParams.Length - pis.Length), 
		};
	}

	private static void CallBaseConstructor(ConstructorInfo parentConstructor, ParameterInfo[] pis, ParameterInfo[] overrideParams, ILGen il)
	{
		il.EmitLoadArg(0);
		for (int i = 0; i < overrideParams.Length; i++)
		{
			int originalIndex = GetOriginalIndex(pis, overrideParams, i);
			if (originalIndex >= 0)
			{
				il.EmitLoadArg(i + 1);
			}
		}
		il.Emit(OpCodes.Call, parentConstructor);
		il.Emit(OpCodes.Ret);
	}

	private ILGen GetCCtor()
	{
		if (_cctor == null)
		{
			ConstructorBuilder constructorBuilder = _tg.DefineTypeInitializer();
			_cctor = new ILGen(constructorBuilder.GetILGenerator());
		}
		return _cctor;
	}

	private void ImplementCustomTypeDescriptor()
	{
		ImplementInterface(typeof(ICustomTypeDescriptor));
		MethodInfo[] methods = typeof(ICustomTypeDescriptor).GetMethods();
		foreach (MethodInfo m in methods)
		{
			ImplementCTDOverride(m);
		}
	}

	private void ImplementCTDOverride(MethodInfo m)
	{
		MethodBuilder builder;
		ILGen iLGen = DefineExplicitInterfaceImplementation(m, out builder);
		iLGen.EmitLoadArg(0);
		ParameterInfo[] parameters = m.GetParameters();
		Type[] array = new Type[parameters.Length + 1];
		array[0] = typeof(object);
		for (int i = 0; i < parameters.Length; i++)
		{
			iLGen.EmitLoadArg(i + 1);
			array[i + 1] = parameters[i].ParameterType;
		}
		iLGen.EmitCall(typeof(CustomTypeDescHelpers), m.Name, array);
		iLGen.EmitBoxing(m.ReturnType);
		iLGen.Emit(OpCodes.Ret);
		_tg.DefineMethodOverride(builder, m);
	}

	private void ImplementDynamicObject()
	{
		bool flag = false;
		foreach (Type interfaceType in _interfaceTypes)
		{
			if (interfaceType == typeof(IDynamicMetaObjectProvider))
			{
				flag = true;
				break;
			}
		}
		bool flag2 = typeof(IDynamicMetaObjectProvider).IsAssignableFrom(_baseType);
		if (flag2 && _baseType.GetInterfaceMap(typeof(IDynamicMetaObjectProvider)).TargetMethods[0].IsPrivate)
		{
			if (_baseType.IsDefined(typeof(DynamicBaseTypeAttribute), inherit: true))
			{
				return;
			}
			flag2 = false;
		}
		ImplementInterface(typeof(IDynamicMetaObjectProvider));
		MethodInfo decl;
		MethodBuilder impl;
		ILGen iLGen = DefineMethodOverride(MethodAttributes.Private, typeof(IDynamicMetaObjectProvider), "GetMetaObject", out decl, out impl);
		MethodInfo method = typeof(UserTypeOps).GetMethod("GetMetaObjectHelper");
		LocalBuilder localBuilder = iLGen.DeclareLocal(typeof(DynamicMetaObject));
		Label label = iLGen.DefineLabel();
		if (flag)
		{
			_explicitMO = _tg.DefineField("__gettingMO", typeof(Microsoft.Scripting.Utils.ThreadLocal<bool>), FieldAttributes.Private | FieldAttributes.InitOnly);
			Label label2 = iLGen.DefineLabel();
			Label label3 = iLGen.DefineLabel();
			Label label4 = iLGen.DefineLabel();
			PropertyInfo declaredProperty = typeof(Microsoft.Scripting.Utils.ThreadLocal<bool>).GetDeclaredProperty("Value");
			iLGen.Emit(OpCodes.Ldarg_0);
			iLGen.Emit(OpCodes.Ldfld, _explicitMO);
			iLGen.EmitPropertyGet(declaredProperty);
			iLGen.Emit(OpCodes.Brtrue, label2);
			iLGen.Emit(OpCodes.Ldarg_0);
			iLGen.Emit(OpCodes.Ldfld, _explicitMO);
			iLGen.Emit(OpCodes.Ldc_I4_1);
			iLGen.EmitPropertySet(declaredProperty);
			iLGen.BeginExceptionBlock();
			LocalBuilder callTarget = EmitNonInheritedMethodLookup("GetMetaObject", iLGen);
			iLGen.Emit(OpCodes.Brfalse, label3);
			EmitClrCallStub(iLGen, typeof(IDynamicMetaObjectProvider).GetMethod("GetMetaObject"), callTarget);
			iLGen.Emit(OpCodes.Dup);
			iLGen.Emit(OpCodes.Ldnull);
			iLGen.Emit(OpCodes.Beq, label4);
			iLGen.Emit(OpCodes.Stloc_S, localBuilder.LocalIndex);
			iLGen.Emit(OpCodes.Leave, label);
			iLGen.MarkLabel(label4);
			iLGen.Emit(OpCodes.Pop);
			iLGen.MarkLabel(label3);
			iLGen.BeginFinallyBlock();
			iLGen.Emit(OpCodes.Ldarg_0);
			iLGen.Emit(OpCodes.Ldfld, _explicitMO);
			iLGen.Emit(OpCodes.Ldc_I4_0);
			iLGen.EmitPropertySet(typeof(Microsoft.Scripting.Utils.ThreadLocal<bool>).GetDeclaredProperty("Value"));
			iLGen.EndExceptionBlock();
			iLGen.MarkLabel(label2);
		}
		iLGen.EmitLoadArg(0);
		iLGen.EmitLoadArg(1);
		if (flag2)
		{
			InterfaceMapping interfaceMap = _baseType.GetInterfaceMap(typeof(IDynamicMetaObjectProvider));
			iLGen.EmitLoadArg(0);
			iLGen.EmitLoadArg(1);
			iLGen.EmitCall(interfaceMap.TargetMethods[0]);
		}
		else
		{
			iLGen.EmitNull();
		}
		iLGen.EmitCall(method);
		iLGen.Emit(OpCodes.Stloc, localBuilder.LocalIndex);
		iLGen.MarkLabel(label);
		iLGen.Emit(OpCodes.Ldloc, localBuilder.LocalIndex);
		iLGen.Emit(OpCodes.Ret);
		_tg.DefineMethodOverride(impl, decl);
	}

	private void ImplementIPythonObject()
	{
		MethodInfo decl;
		MethodBuilder impl;
		ILGen iLGen;
		if (NeedsPythonObject)
		{
			_typeField = _tg.DefineField(".class", typeof(PythonType), FieldAttributes.Public);
			_dictField = _tg.DefineField(".dict", typeof(PythonDictionary), FieldAttributes.Public);
			ImplementInterface(typeof(IPythonObject));
			MethodAttributes extra = MethodAttributes.Private;
			iLGen = DefineMethodOverride(extra, typeof(IPythonObject), "get_Dict", out decl, out impl);
			iLGen.EmitLoadArg(0);
			EmitGetDict(iLGen);
			iLGen.Emit(OpCodes.Ret);
			_tg.DefineMethodOverride(impl, decl);
			iLGen = DefineMethodOverride(extra, typeof(IPythonObject), "ReplaceDict", out decl, out impl);
			iLGen.EmitLoadArg(0);
			iLGen.EmitLoadArg(1);
			EmitSetDict(iLGen);
			iLGen.EmitBoolean(value: true);
			iLGen.Emit(OpCodes.Ret);
			_tg.DefineMethodOverride(impl, decl);
			iLGen = DefineMethodOverride(extra, typeof(IPythonObject), "SetDict", out decl, out impl);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldAddress(_dictField);
			iLGen.EmitLoadArg(1);
			iLGen.EmitCall(typeof(UserTypeOps), "SetDictHelper");
			iLGen.Emit(OpCodes.Ret);
			_tg.DefineMethodOverride(impl, decl);
			iLGen = DefineMethodOverride(extra, typeof(IPythonObject), "get_PythonType", out decl, out impl);
			iLGen.EmitLoadArg(0);
			iLGen.EmitFieldGet(_typeField);
			iLGen.Emit(OpCodes.Ret);
			_tg.DefineMethodOverride(impl, decl);
			iLGen = DefineMethodOverride(extra, typeof(IPythonObject), "SetPythonType", out decl, out impl);
			iLGen.EmitLoadArg(0);
			iLGen.EmitLoadArg(1);
			iLGen.EmitFieldSet(_typeField);
			iLGen.Emit(OpCodes.Ret);
			_tg.DefineMethodOverride(impl, decl);
		}
		_slotsField = _tg.DefineField(".slots_and_weakref", typeof(object[]), FieldAttributes.Public);
		iLGen = DefineMethodOverride(MethodAttributes.Private, typeof(IPythonObject), "GetSlots", out decl, out impl);
		iLGen.EmitLoadArg(0);
		iLGen.EmitFieldGet(_slotsField);
		iLGen.Emit(OpCodes.Ret);
		_tg.DefineMethodOverride(impl, decl);
		iLGen = DefineMethodOverride(MethodAttributes.Private, typeof(IPythonObject), "GetSlotsCreate", out decl, out impl);
		iLGen.EmitLoadArg(0);
		iLGen.EmitLoadArg(0);
		iLGen.EmitFieldAddress(_slotsField);
		iLGen.EmitCall(typeof(UserTypeOps).GetMethod("GetSlotsCreate"));
		iLGen.Emit(OpCodes.Ret);
		_tg.DefineMethodOverride(impl, decl);
	}

	private void DefineHelperInterface(Type intf)
	{
		ImplementInterface(intf);
		MethodInfo[] methods = intf.GetMethods();
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			MethodBuilder builder;
			ILGen iLGen = DefineExplicitInterfaceImplementation(methodInfo, out builder);
			ParameterInfo[] parameters = methodInfo.GetParameters();
			MethodInfo method = typeof(UserTypeOps).GetMethod(methodInfo.Name + "Helper");
			int num = 0;
			if (parameters.Length > 0 && parameters[0].ParameterType == typeof(CodeContext))
			{
				num = 1;
				iLGen.EmitLoadArg(1);
			}
			iLGen.EmitLoadArg(0);
			for (int j = num; j < parameters.Length; j++)
			{
				iLGen.EmitLoadArg(j + 1);
			}
			iLGen.EmitCall(method);
			iLGen.Emit(OpCodes.Ret);
			_tg.DefineMethodOverride(builder, methodInfo);
		}
	}

	private void ImplementWeakReference()
	{
		if (!typeof(IWeakReferenceable).IsAssignableFrom(_baseType))
		{
			DefineHelperInterface(typeof(IWeakReferenceable));
		}
	}

	private void ImplementProtectedFieldAccessors(Dictionary<string, string[]> specialNames)
	{
		bool flattenHierarchy = true;
		foreach (FieldInfo inheritedField in _baseType.GetInheritedFields(null, flattenHierarchy))
		{
			if (!inheritedField.IsProtected())
			{
				continue;
			}
			List<string> list = new List<string>();
			PropertyBuilder propertyBuilder = _tg.DefineProperty(inheritedField.Name, PropertyAttributes.None, inheritedField.FieldType, ReflectionUtils.EmptyTypes);
			MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			if (inheritedField.IsStatic)
			{
				methodAttributes |= MethodAttributes.Static;
			}
			MethodBuilder methodBuilder = _tg.DefineMethod("#field_get#" + inheritedField.Name, methodAttributes, inheritedField.FieldType, ReflectionUtils.EmptyTypes);
			ILGen iLGen = new ILGen(methodBuilder.GetILGenerator());
			if (!inheritedField.IsStatic)
			{
				iLGen.EmitLoadArg(0);
			}
			if (inheritedField.IsLiteral)
			{
				object rawConstantValue = inheritedField.GetRawConstantValue();
				switch (inheritedField.FieldType.GetTypeCode())
				{
				case TypeCode.Boolean:
					if ((bool)rawConstantValue)
					{
						iLGen.Emit(OpCodes.Ldc_I4_1);
					}
					else
					{
						iLGen.Emit(OpCodes.Ldc_I4_0);
					}
					break;
				case TypeCode.Byte:
					iLGen.Emit(OpCodes.Ldc_I4, (byte)rawConstantValue);
					break;
				case TypeCode.Char:
					iLGen.Emit(OpCodes.Ldc_I4, (char)rawConstantValue);
					break;
				case TypeCode.Double:
					iLGen.Emit(OpCodes.Ldc_R8, (double)rawConstantValue);
					break;
				case TypeCode.Int16:
					iLGen.Emit(OpCodes.Ldc_I4, (short)rawConstantValue);
					break;
				case TypeCode.Int32:
					iLGen.Emit(OpCodes.Ldc_I4, (int)rawConstantValue);
					break;
				case TypeCode.Int64:
					iLGen.Emit(OpCodes.Ldc_I8, (long)rawConstantValue);
					break;
				case TypeCode.SByte:
					iLGen.Emit(OpCodes.Ldc_I4, (sbyte)rawConstantValue);
					break;
				case TypeCode.Single:
					iLGen.Emit(OpCodes.Ldc_R4, (float)rawConstantValue);
					break;
				case TypeCode.String:
					iLGen.Emit(OpCodes.Ldstr, (string)rawConstantValue);
					break;
				case TypeCode.UInt16:
					iLGen.Emit(OpCodes.Ldc_I4, (ushort)rawConstantValue);
					break;
				case TypeCode.UInt32:
					iLGen.Emit(OpCodes.Ldc_I4, (uint)rawConstantValue);
					break;
				case TypeCode.UInt64:
					iLGen.Emit(OpCodes.Ldc_I8, (ulong)rawConstantValue);
					break;
				}
			}
			else
			{
				iLGen.EmitFieldGet(inheritedField);
			}
			iLGen.Emit(OpCodes.Ret);
			propertyBuilder.SetGetMethod(methodBuilder);
			list.Add(methodBuilder.Name);
			if (!inheritedField.IsLiteral && !inheritedField.IsInitOnly)
			{
				methodBuilder = _tg.DefineMethod("#field_set#" + inheritedField.Name, methodAttributes, null, new Type[1] { inheritedField.FieldType });
				methodBuilder.DefineParameter(1, ParameterAttributes.None, "value");
				iLGen = new ILGen(methodBuilder.GetILGenerator());
				iLGen.EmitLoadArg(0);
				if (!inheritedField.IsStatic)
				{
					iLGen.EmitLoadArg(1);
				}
				iLGen.EmitFieldSet(inheritedField);
				iLGen.Emit(OpCodes.Ret);
				propertyBuilder.SetSetMethod(methodBuilder);
				list.Add(methodBuilder.Name);
			}
			specialNames[inheritedField.Name] = list.ToArray();
		}
	}

	private void OverrideMethods(Type type, Dictionary<string, string[]> specialNames)
	{
		Dictionary<KeyValuePair<string, MethodSignatureInfo>, MethodInfo> dictionary = new Dictionary<KeyValuePair<string, MethodSignatureInfo>, MethodInfo>();
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			KeyValuePair<string, MethodSignatureInfo> key = new KeyValuePair<string, MethodSignatureInfo>(methodInfo.Name, new MethodSignatureInfo(methodInfo));
			if (!dictionary.TryGetValue(key, out var value))
			{
				dictionary[key] = methodInfo;
			}
			else if (value.DeclaringType.IsAssignableFrom(methodInfo.DeclaringType))
			{
				dictionary[key] = methodInfo;
			}
		}
		if (type.IsAbstract() && !type.IsInterface())
		{
			Type[] interfaces = type.GetInterfaces();
			foreach (Type interfaceType in interfaces)
			{
				InterfaceMapping interfaceMap = type.GetInterfaceMap(interfaceType);
				for (int k = 0; k < interfaceMap.TargetMethods.Length; k++)
				{
					if (interfaceMap.TargetMethods[k] == null)
					{
						MethodInfo methodInfo2 = interfaceMap.InterfaceMethods[k];
						KeyValuePair<string, MethodSignatureInfo> key2 = new KeyValuePair<string, MethodSignatureInfo>(methodInfo2.Name, new MethodSignatureInfo(methodInfo2));
						dictionary[key2] = methodInfo2;
					}
				}
			}
		}
		Dictionary<PropertyInfo, PropertyBuilder> overridden = new Dictionary<PropertyInfo, PropertyBuilder>();
		foreach (MethodInfo value2 in dictionary.Values)
		{
			if (CanOverrideMethod(value2) && (value2.IsPublic || value2.IsProtected()))
			{
				if (value2.IsSpecialName)
				{
					OverrideSpecialName(value2, specialNames, overridden);
				}
				else
				{
					OverrideBaseMethod(value2, specialNames);
				}
			}
		}
	}

	private void OverrideSpecialName(MethodInfo mi, Dictionary<string, string[]> specialNames, Dictionary<PropertyInfo, PropertyBuilder> overridden)
	{
		if (!mi.IsVirtual || mi.IsFinal)
		{
			if ((!mi.IsProtected() && !mi.IsSpecialName) || (!mi.Name.StartsWith("get_") && !mi.Name.StartsWith("set_")))
			{
				return;
			}
			specialNames[mi.Name] = new string[1] { mi.Name };
			MethodBuilder mb = CreateSuperCallHelper(mi);
			PropertyInfo[] properties = mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetGetMethod(nonPublic: true).MemberEquals(mi) || propertyInfo.GetSetMethod(nonPublic: true).MemberEquals(mi))
				{
					AddPublicProperty(mi, overridden, mb, propertyInfo);
					break;
				}
			}
		}
		else
		{
			if (TryOverrideProperty(mi, specialNames, overridden))
			{
				return;
			}
			EventInfo[] events = mi.DeclaringType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			EventInfo[] array = events;
			foreach (EventInfo eventInfo in array)
			{
				string name;
				if (eventInfo.GetAddMethod().MemberEquals(mi))
				{
					if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), eventInfo, mi, out name) != NameType.None)
					{
						CreateVTableEventOverride(mi, mi.Name);
					}
					return;
				}
				if (eventInfo.GetRemoveMethod().MemberEquals(mi))
				{
					if (NameConverter.TryGetName(DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType), eventInfo, mi, out name) != NameType.None)
					{
						CreateVTableEventOverride(mi, mi.Name);
					}
					return;
				}
			}
			OverrideBaseMethod(mi, specialNames);
		}
	}

	private bool TryOverrideProperty(MethodInfo mi, Dictionary<string, string[]> specialNames, Dictionary<PropertyInfo, PropertyBuilder> overridden)
	{
		PropertyInfo[] properties = mi.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		specialNames[mi.Name] = new string[1] { mi.Name };
		MethodBuilder mb = null;
		PropertyInfo propertyInfo = null;
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo2 in array)
		{
			if (propertyInfo2.GetIndexParameters().Length > 0)
			{
				if (mi.MemberEquals(propertyInfo2.GetGetMethod(nonPublic: true)))
				{
					mb = CreateVTableMethodOverride(mi, "__getitem__");
					if (!mi.IsAbstract)
					{
						CreateSuperCallHelper(mi);
					}
					propertyInfo = propertyInfo2;
					break;
				}
				if (mi.MemberEquals(propertyInfo2.GetSetMethod(nonPublic: true)))
				{
					mb = CreateVTableMethodOverride(mi, "__setitem__");
					if (!mi.IsAbstract)
					{
						CreateSuperCallHelper(mi);
					}
					propertyInfo = propertyInfo2;
					break;
				}
				continue;
			}
			string name;
			if (mi.MemberEquals(propertyInfo2.GetGetMethod(nonPublic: true)))
			{
				if (mi.Name != "get_PythonType")
				{
					if (NameConverter.TryGetName(GetBaseTypeForMethod(mi), propertyInfo2, mi, out name) == NameType.None)
					{
						return true;
					}
					mb = CreateVTableGetterOverride(mi, name);
					if (!mi.IsAbstract)
					{
						CreateSuperCallHelper(mi);
					}
				}
				propertyInfo = propertyInfo2;
				break;
			}
			if (!mi.MemberEquals(propertyInfo2.GetSetMethod(nonPublic: true)))
			{
				continue;
			}
			if (NameConverter.TryGetName(GetBaseTypeForMethod(mi), propertyInfo2, mi, out name) == NameType.None)
			{
				return true;
			}
			mb = CreateVTableSetterOverride(mi, name);
			if (!mi.IsAbstract)
			{
				CreateSuperCallHelper(mi);
			}
			propertyInfo = propertyInfo2;
			break;
		}
		if (propertyInfo != null)
		{
			AddPublicProperty(mi, overridden, mb, propertyInfo);
			return true;
		}
		return false;
	}

	private void AddPublicProperty(MethodInfo mi, Dictionary<PropertyInfo, PropertyBuilder> overridden, MethodBuilder mb, PropertyInfo foundProperty)
	{
		MethodInfo getMethod = foundProperty.GetGetMethod(nonPublic: true);
		MethodInfo setMethod = foundProperty.GetSetMethod(nonPublic: true);
		if ((!(getMethod != null) || !getMethod.IsProtected()) && (!(setMethod != null) || !setMethod.IsProtected()))
		{
			return;
		}
		if (!overridden.TryGetValue(foundProperty, out var value))
		{
			ParameterInfo[] indexParameters = foundProperty.GetIndexParameters();
			Type[] array = new Type[indexParameters.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = indexParameters[i].ParameterType;
			}
			value = (overridden[foundProperty] = _tg.DefineProperty(foundProperty.Name, foundProperty.Attributes, foundProperty.PropertyType, array));
		}
		if (foundProperty.GetGetMethod(nonPublic: true).MemberEquals(mi))
		{
			value.SetGetMethod(mb);
		}
		else if (foundProperty.GetSetMethod(nonPublic: true).MemberEquals(mi))
		{
			value.SetSetMethod(mb);
		}
	}

	private static void EmitBaseMethodDispatch(MethodInfo mi, ILGen il)
	{
		if (!mi.IsAbstract)
		{
			int num = 0;
			if (!mi.IsStatic)
			{
				il.EmitLoadArg(0);
				num = 1;
			}
			ParameterInfo[] parameters = mi.GetParameters();
			for (int i = 0; i < parameters.Length; i++)
			{
				il.EmitLoadArg(i + num);
			}
			il.EmitCall(OpCodes.Call, mi, null);
			il.Emit(OpCodes.Ret);
		}
		else
		{
			il.EmitLoadArg(0);
			il.EmitString(mi.Name);
			il.EmitCall(typeof(PythonOps), "MissingInvokeMethodException");
			il.Emit(OpCodes.Throw);
		}
	}

	private void OverrideBaseMethod(MethodInfo mi, Dictionary<string, string[]> specialNames)
	{
		if ((!mi.IsVirtual || mi.IsFinal) && !mi.IsProtected())
		{
			return;
		}
		PythonType baseTypeForMethod = GetBaseTypeForMethod(mi);
		string name = null;
		if (NameConverter.TryGetName(baseTypeForMethod, mi, out name) != NameType.None && (!(mi.DeclaringType == typeof(object)) || !(mi.Name == "Finalize")))
		{
			specialNames[mi.Name] = new string[1] { mi.Name };
			if (!mi.IsStatic)
			{
				CreateVTableMethodOverride(mi, name);
			}
			if (!mi.IsAbstract)
			{
				CreateSuperCallHelper(mi);
			}
		}
	}

	private PythonType GetBaseTypeForMethod(MethodInfo mi)
	{
		if (_baseType == mi.DeclaringType || _baseType.IsSubclassOf(mi.DeclaringType))
		{
			return DynamicHelpers.GetPythonTypeFromType(_baseType);
		}
		return DynamicHelpers.GetPythonTypeFromType(mi.DeclaringType);
	}

	private LocalBuilder EmitBaseClassCallCheckForProperties(ILGen il, MethodInfo baseMethod, string name)
	{
		Label label = il.DefineLabel();
		LocalBuilder localBuilder = il.DeclareLocal(typeof(object));
		il.EmitLoadArg(0);
		il.EmitString(name);
		il.Emit(OpCodes.Ldloca, localBuilder);
		il.EmitCall(typeof(UserTypeOps), "TryGetNonInheritedValueHelper");
		il.Emit(OpCodes.Brtrue, label);
		LocalBuilder callTarget = EmitNonInheritedMethodLookup(baseMethod.Name, il);
		Label label2 = il.DefineLabel();
		il.Emit(OpCodes.Brtrue, label2);
		EmitBaseMethodDispatch(baseMethod, il);
		il.MarkLabel(label2);
		EmitClrCallStub(il, baseMethod, callTarget);
		il.Emit(OpCodes.Ret);
		il.MarkLabel(label);
		return localBuilder;
	}

	private MethodBuilder CreateVTableGetterOverride(MethodInfo mi, string name)
	{
		DefineVTableMethodOverride(mi, out var impl, out var il);
		LocalBuilder local = EmitBaseClassCallCheckForProperties(il, mi, name);
		il.Emit(OpCodes.Ldloc, local);
		il.EmitLoadArg(0);
		il.EmitString(name);
		il.EmitCall(typeof(UserTypeOps), "GetPropertyHelper");
		if (!il.TryEmitImplicitCast(typeof(object), mi.ReturnType))
		{
			EmitConvertFromObject(il, mi.ReturnType);
		}
		il.Emit(OpCodes.Ret);
		_tg.DefineMethodOverride(impl, mi);
		return impl;
	}

	private static void EmitConvertFromObject(ILGen il, Type toType)
	{
		if (toType == typeof(object))
		{
			return;
		}
		if (toType.IsGenericParameter)
		{
			il.EmitCall(typeof(PythonOps).GetMethod("ConvertFromObject").MakeGenericMethod(toType));
			return;
		}
		MethodInfo fastConvertMethod = PythonBinder.GetFastConvertMethod(toType);
		if (fastConvertMethod != null)
		{
			il.EmitCall(fastConvertMethod);
			return;
		}
		if (toType == typeof(void))
		{
			il.Emit(OpCodes.Pop);
			return;
		}
		if (typeof(Delegate).IsAssignableFrom(toType))
		{
			il.EmitType(toType);
			il.EmitCall(typeof(Converter), "ConvertToDelegate");
			il.Emit(OpCodes.Castclass, toType);
			return;
		}
		Label label = il.DefineLabel();
		il.Emit(OpCodes.Dup);
		il.Emit(OpCodes.Isinst, toType);
		il.Emit(OpCodes.Brtrue_S, label);
		il.Emit(OpCodes.Ldtoken, toType);
		il.EmitCall(PythonBinder.GetGenericConvertMethod(toType));
		il.MarkLabel(label);
		il.Emit(OpCodes.Unbox_Any, toType);
	}

	private MethodBuilder CreateVTableSetterOverride(MethodInfo mi, string name)
	{
		DefineVTableMethodOverride(mi, out var impl, out var il);
		LocalBuilder local = EmitBaseClassCallCheckForProperties(il, mi, name);
		il.Emit(OpCodes.Ldloc, local);
		il.EmitLoadArg(0);
		il.EmitLoadArg(1);
		il.EmitBoxing(mi.GetParameters()[0].ParameterType);
		il.EmitString(name);
		il.EmitCall(typeof(UserTypeOps), "SetPropertyHelper");
		il.Emit(OpCodes.Ret);
		_tg.DefineMethodOverride(impl, mi);
		return impl;
	}

	private void CreateVTableEventOverride(MethodInfo mi, string name)
	{
		MethodBuilder impl;
		ILGen iLGen = DefineMethodOverride(mi, out impl);
		LocalBuilder local = EmitBaseClassCallCheckForEvents(iLGen, mi, name);
		iLGen.Emit(OpCodes.Ldloc, local);
		iLGen.EmitLoadArg(0);
		iLGen.EmitLoadArg(1);
		iLGen.EmitBoxing(mi.GetParameters()[0].ParameterType);
		iLGen.EmitString(name);
		iLGen.EmitCall(typeof(UserTypeOps), "AddRemoveEventHelper");
		iLGen.Emit(OpCodes.Ret);
		_tg.DefineMethodOverride(impl, mi);
	}

	private static LocalBuilder EmitBaseClassCallCheckForEvents(ILGen il, MethodInfo baseMethod, string name)
	{
		Label label = il.DefineLabel();
		LocalBuilder localBuilder = il.DeclareLocal(typeof(object));
		il.EmitLoadArg(0);
		il.EmitString(name);
		il.Emit(OpCodes.Ldloca, localBuilder);
		il.EmitCall(typeof(UserTypeOps), "TryGetNonInheritedValueHelper");
		il.Emit(OpCodes.Brtrue, label);
		EmitBaseMethodDispatch(baseMethod, il);
		il.MarkLabel(label);
		return localBuilder;
	}

	private MethodBuilder CreateVTableMethodOverride(MethodInfo mi, string name)
	{
		DefineVTableMethodOverride(mi, out var impl, out var il);
		LocalBuilder callTarget = EmitNonInheritedMethodLookup(name, il);
		Label label = il.DefineLabel();
		il.Emit(OpCodes.Brtrue, label);
		EmitBaseMethodDispatch(mi, il);
		il.MarkLabel(label);
		EmitClrCallStub(il, mi, callTarget);
		il.Emit(OpCodes.Ret);
		if (mi.IsVirtual && !mi.IsFinal)
		{
			_tg.DefineMethodOverride(impl, mi);
		}
		return impl;
	}

	private void DefineVTableMethodOverride(MethodInfo mi, out MethodBuilder impl, out ILGen il)
	{
		if (mi.IsVirtual && !mi.IsFinal)
		{
			il = DefineMethodOverride(MethodAttributes.Public, mi, out impl);
			return;
		}
		impl = _tg.DefineMethod(mi.Name, mi.IsVirtual ? (mi.Attributes | MethodAttributes.VtableLayoutMask) : ((mi.Attributes & ~MethodAttributes.MemberAccessMask) | MethodAttributes.Public), mi.CallingConvention);
		ReflectionUtils.CopyMethodSignature(mi, impl, substituteDeclaringType: false);
		il = new ILGen(impl.GetILGenerator());
	}

	private LocalBuilder EmitNonInheritedMethodLookup(string name, ILGen il)
	{
		LocalBuilder localBuilder = il.DeclareLocal(typeof(object));
		il.EmitLoadArg(0);
		if (typeof(IPythonObject).IsAssignableFrom(_baseType))
		{
			il.EmitPropertyGet(TypeInfo._IPythonObject.PythonType);
		}
		else
		{
			il.EmitFieldGet(_typeField);
		}
		il.EmitLoadArg(0);
		il.EmitString(name);
		il.Emit(OpCodes.Ldloca, localBuilder);
		il.EmitCall(typeof(UserTypeOps), "TryGetNonInheritedMethodHelper");
		return localBuilder;
	}

	private MethodBuilder CreateSuperCallHelper(MethodInfo mi)
	{
		MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
		if (mi.IsStatic)
		{
			methodAttributes |= MethodAttributes.Static;
		}
		MethodBuilder methodBuilder = _tg.DefineMethod("#base#" + mi.Name, methodAttributes, mi.CallingConvention);
		ReflectionUtils.CopyMethodSignature(mi, methodBuilder, substituteDeclaringType: true);
		EmitBaseMethodDispatch(mi, new ILGen(methodBuilder.GetILGenerator()));
		return methodBuilder;
	}

	private KeyValuePair<Type, Dictionary<string, string[]>> SaveType(AssemblyGen ag, string name)
	{
		_tg = ag.DefinePublicType("IronPython.NewTypes." + name, _baseType, preserveName: true);
		Dictionary<string, string[]> value = ImplementType();
		Type key = FinishType();
		return new KeyValuePair<Type, Dictionary<string, string[]>>(key, value);
	}

	private Type FinishType()
	{
		if (_cctor != null)
		{
			_cctor.Emit(OpCodes.Ret);
		}
		return _tg.CreateType();
	}

	private ILGen DefineExplicitInterfaceImplementation(MethodInfo baseMethod, out MethodBuilder builder)
	{
		MethodAttributes methodAttributes = baseMethod.Attributes & ~(MethodAttributes.Public | MethodAttributes.Abstract);
		methodAttributes |= MethodAttributes.Final | MethodAttributes.VtableLayoutMask;
		Type[] parameterTypes = ReflectionUtils.GetParameterTypes(baseMethod.GetParameters());
		builder = _tg.DefineMethod(baseMethod.DeclaringType.Name + "." + baseMethod.Name, methodAttributes, baseMethod.ReturnType, parameterTypes);
		return new ILGen(builder.GetILGenerator());
	}

	private ILGen DefineMethodOverride(MethodAttributes extra, Type type, string name, out MethodInfo decl, out MethodBuilder impl)
	{
		decl = type.GetMethod(name);
		return DefineMethodOverride(extra, decl, out impl);
	}

	private ILGen DefineMethodOverride(MethodInfo decl, out MethodBuilder impl)
	{
		return DefineMethodOverride(MethodAttributes.PrivateScope, decl, out impl);
	}

	private ILGen DefineMethodOverride(MethodAttributes extra, MethodInfo decl, out MethodBuilder impl)
	{
		impl = ReflectionUtils.DefineMethodOverride(_tg, extra, decl);
		return new ILGen(impl.GetILGenerator());
	}

	private void EmitClrCallStub(ILGen il, MethodInfo mi, LocalBuilder callTarget)
	{
		int num = 0;
		bool flag = false;
		bool context = false;
		ParameterInfo[] parameters = mi.GetParameters();
		if (parameters.Length > 0)
		{
			if (parameters[0].ParameterType == typeof(CodeContext))
			{
				num = 1;
				context = true;
			}
			if (parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), inherit: false))
			{
				flag = true;
			}
		}
		ParameterInfo[] array = parameters;
		int num2 = array.Length - num;
		Type[] genericArguments = mi.GetGenericArguments();
		ILGen cCtor = GetCCtor();
		if (flag || genericArguments.Length > 0)
		{
			cCtor.EmitInt(num2);
			cCtor.EmitBoolean(flag);
			cCtor.EmitInt(genericArguments.Length);
			cCtor.Emit(OpCodes.Newarr, typeof(string));
			for (int i = 0; i < genericArguments.Length; i++)
			{
				cCtor.Emit(OpCodes.Dup);
				cCtor.EmitInt(i);
				cCtor.Emit(OpCodes.Ldelema, typeof(string));
				cCtor.Emit(OpCodes.Ldstr, genericArguments[i].Name);
				cCtor.Emit(OpCodes.Stobj, typeof(string));
			}
			cCtor.EmitCall(typeof(PythonOps).GetMethod("MakeComplexCallAction"));
		}
		else
		{
			cCtor.EmitInt(num2);
			cCtor.EmitCall(typeof(PythonOps).GetMethod("MakeSimpleCallAction"));
		}
		Type type = CompilerHelpers.MakeCallSiteType(MakeSiteSignature(num2 + genericArguments.Length));
		FieldBuilder fi = _tg.DefineField("site$" + _site++, type, FieldAttributes.Private | FieldAttributes.Static);
		cCtor.EmitCall(type.GetMethod("Create"));
		cCtor.EmitFieldSet(fi);
		List<ReturnFixer> list = new List<ReturnFixer>(0);
		il.EmitFieldGet(fi);
		FieldInfo declaredField = type.GetDeclaredField("Target");
		il.EmitFieldGet(declaredField);
		il.EmitFieldGet(fi);
		EmitCodeContext(il, context);
		il.Emit(OpCodes.Ldloc, callTarget);
		for (int j = num; j < array.Length; j++)
		{
			ReturnFixer returnFixer = ReturnFixer.EmitArgument(il, array[j], j + 1);
			if (returnFixer != null)
			{
				list.Add(returnFixer);
			}
		}
		for (int k = 0; k < genericArguments.Length; k++)
		{
			il.EmitType(genericArguments[k]);
			il.EmitCall(typeof(DynamicHelpers).GetMethod("GetPythonTypeFromType"));
		}
		il.EmitCall(declaredField.FieldType, "Invoke");
		foreach (ReturnFixer item in list)
		{
			item.FixReturn(il);
		}
		EmitConvertFromObject(il, mi.ReturnType);
	}

	private static void EmitCodeContext(ILGen il, bool context)
	{
		if (context)
		{
			il.EmitLoadArg(1);
		}
		else
		{
			il.EmitPropertyGet(typeof(DefaultContext).GetProperty("Default"));
		}
	}

	private static void EmitInt(ILGenerator ilg, int iVal)
	{
		switch (iVal)
		{
		case 0:
			ilg.Emit(OpCodes.Ldc_I4_0);
			break;
		case 1:
			ilg.Emit(OpCodes.Ldc_I4_1);
			break;
		case 2:
			ilg.Emit(OpCodes.Ldc_I4_2);
			break;
		case 3:
			ilg.Emit(OpCodes.Ldc_I4_3);
			break;
		case 4:
			ilg.Emit(OpCodes.Ldc_I4_4);
			break;
		case 5:
			ilg.Emit(OpCodes.Ldc_I4_5);
			break;
		case 6:
			ilg.Emit(OpCodes.Ldc_I4_6);
			break;
		case 7:
			ilg.Emit(OpCodes.Ldc_I4_7);
			break;
		case 8:
			ilg.Emit(OpCodes.Ldc_I4_8);
			break;
		default:
			ilg.Emit(OpCodes.Ldc_I4, iVal);
			break;
		}
	}

	private static Type[] MakeSiteSignature(int nargs)
	{
		Type[] array = new Type[nargs + 4];
		array[0] = typeof(CallSite);
		array[1] = typeof(CodeContext);
		for (int i = 2; i < array.Length; i++)
		{
			array[i] = typeof(object);
		}
		return array;
	}

	private static void AddBaseMethods(Type finishedType, Dictionary<string, string[]> specialNames)
	{
		MethodInfo[] methods = finishedType.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			if (IsInstanceType(finishedType.BaseType) && IsInstanceType(methodInfo.DeclaringType))
			{
				continue;
			}
			string name = methodInfo.Name;
			if (!name.StartsWith("#base#") && !name.StartsWith("#field_get#") && !name.StartsWith("#field_set#"))
			{
				continue;
			}
			foreach (string item in GetBaseName(methodInfo, specialNames))
			{
				if (methodInfo.IsSpecialName && (item.StartsWith("get_") || item.StartsWith("set_")))
				{
					string text = item.Substring(4);
					MemberInfo[] defaultMembers = finishedType.BaseType.GetDefaultMembers();
					if (defaultMembers.Length > 0)
					{
						MemberInfo[] array = defaultMembers;
						foreach (MemberInfo memberInfo in array)
						{
							if (memberInfo.Name == text)
							{
								StoreOverriddenMethod(methodInfo, item);
								break;
							}
						}
					}
					StoreOverriddenProperty(methodInfo, item);
				}
				else if (methodInfo.IsSpecialName && (item.StartsWith("#field_get#") || item.StartsWith("#field_set#")))
				{
					StoreOverriddenField(methodInfo, item);
				}
				else
				{
					StoreOverriddenMethod(methodInfo, item);
				}
			}
		}
	}

	private static void StoreOverriddenField(MethodInfo mi, string newName)
	{
		Type baseType = mi.DeclaringType.GetBaseType();
		string text = newName.Substring("#field_get#".Length);
		lock (PythonTypeOps._propertyCache)
		{
			FieldInfo[] fields = baseType.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.Name == text)
				{
					if (newName.StartsWith("#field_get#"))
					{
						AddPropertyInfo(fieldInfo.DeclaringType, text, mi, null);
					}
					else if (newName.StartsWith("#field_set#"))
					{
						AddPropertyInfo(fieldInfo.DeclaringType, text, null, mi);
					}
				}
			}
		}
	}

	private static ExtensionPropertyTracker AddPropertyInfo(Type baseType, string propName, MethodInfo get, MethodInfo set)
	{
		MethodInfo methodInfo = get ?? set;
		if (!_overriddenProperties.TryGetValue(baseType, out var value))
		{
			value = (_overriddenProperties[baseType] = new Dictionary<string, List<ExtensionPropertyTracker>>());
		}
		if (!value.TryGetValue(propName, out var value2))
		{
			value2 = (value[propName] = new List<ExtensionPropertyTracker>());
		}
		for (int i = 0; i < value2.Count; i++)
		{
			if (value2[i].DeclaringType == methodInfo.DeclaringType)
			{
				return value2[i] = new ExtensionPropertyTracker(propName, get ?? value2[i].GetGetMethod(), set ?? value2[i].GetSetMethod(), null, methodInfo.DeclaringType);
			}
		}
		ExtensionPropertyTracker result;
		value2.Add(result = new ExtensionPropertyTracker(propName, methodInfo, null, null, methodInfo.DeclaringType));
		return result;
	}

	private static void StoreOverriddenMethod(MethodInfo mi, string newName)
	{
		Type baseType = mi.DeclaringType.GetBaseType();
		MemberInfo[] member = baseType.GetMember(newName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		Type declaringType = ((MethodInfo)member[0]).GetBaseDefinition().DeclaringType;
		string text = newName;
		switch (newName)
		{
		case "get_Item":
			text = "__getitem__";
			break;
		case "set_Item":
			text = "__setitem__";
			break;
		}
		lock (PythonTypeOps._functions)
		{
			foreach (BuiltinFunction value3 in PythonTypeOps._functions.Values)
			{
				if (value3.Name == text && value3.DeclaringType == declaringType)
				{
					value3.AddMethod(mi);
					break;
				}
			}
			if (!_overriddenMethods.TryGetValue(declaringType, out var value))
			{
				value = (_overriddenMethods[declaringType] = new Dictionary<string, List<MethodInfo>>());
			}
			if (!value.TryGetValue(text, out var value2))
			{
				value2 = (value[text] = new List<MethodInfo>());
			}
			value2.Add(mi);
		}
	}

	private static void StoreOverriddenProperty(MethodInfo mi, string newName)
	{
		Type baseType = mi.DeclaringType.GetBaseType();
		lock (PythonTypeOps._propertyCache)
		{
			string text = newName.Substring(4);
			ExtensionPropertyTracker extensionPropertyTracker = null;
			PropertyInfo[] properties = baseType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.Name == text)
				{
					Type declaringType = propertyInfo.DeclaringType;
					if (newName.StartsWith("get_"))
					{
						extensionPropertyTracker = AddPropertyInfo(declaringType, text, mi, null);
					}
					else if (newName.StartsWith("set_"))
					{
						extensionPropertyTracker = AddPropertyInfo(declaringType, text, null, mi);
					}
				}
			}
			if (extensionPropertyTracker == null)
			{
				return;
			}
			foreach (ReflectedGetterSetter value in PythonTypeOps._propertyCache.Values)
			{
				if (!(value.DeclaringType != baseType) && !(value.__name__ != extensionPropertyTracker.Name))
				{
					if (extensionPropertyTracker.GetGetMethod(privateMembers: true) != null)
					{
						value.AddGetter(extensionPropertyTracker.GetGetMethod(privateMembers: true));
					}
					if (extensionPropertyTracker.GetSetMethod(privateMembers: true) != null)
					{
						value.AddSetter(extensionPropertyTracker.GetSetMethod(privateMembers: true));
					}
				}
			}
		}
	}

	private static IEnumerable<string> GetBaseName(MethodInfo mi, Dictionary<string, string[]> specialNames)
	{
		string key;
		if (mi.Name.StartsWith("#base#"))
		{
			key = mi.Name.Substring("#base#".Length);
		}
		else if (mi.Name.StartsWith("#field_get#"))
		{
			key = mi.Name.Substring("#field_get#".Length);
		}
		else
		{
			if (!mi.Name.StartsWith("#field_set#"))
			{
				throw new InvalidOperationException();
			}
			key = mi.Name.Substring("#field_set#".Length);
		}
		return specialNames[key];
	}

	internal static IList<MethodInfo> GetOverriddenMethods(Type type, string name)
	{
		List<MethodInfo> list = null;
		Type type2 = type;
		while (type2 != null)
		{
			if (_overriddenMethods.TryGetValue(type2, out var value) && value.TryGetValue(name, out var value2))
			{
				if (list == null)
				{
					list = new List<MethodInfo>(value2.Count);
				}
				foreach (MethodInfo item in value2)
				{
					if (type.IsAssignableFrom(item.DeclaringType))
					{
						list.Add(item);
					}
				}
			}
			type2 = type2.GetBaseType();
		}
		if (list != null)
		{
			return list;
		}
		return new MethodInfo[0];
	}

	internal static IList<ExtensionPropertyTracker> GetOverriddenProperties(Type type, string name)
	{
		lock (_overriddenProperties)
		{
			if (_overriddenProperties.TryGetValue(type, out var value) && value.TryGetValue(name, out var value2))
			{
				return value2;
			}
		}
		return new ExtensionPropertyTracker[0];
	}
}
