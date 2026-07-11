using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

[Documentation("Provides global reduction-function registration for pickling and copying objects.")]
public static class PythonCopyReg
{
	private static readonly object _dispatchTableKey = new object();

	private static readonly object _extensionRegistryKey = new object();

	private static readonly object _invertedRegistryKey = new object();

	private static readonly object _extensionCacheKey = new object();

	internal static PythonDictionary GetDispatchTable(CodeContext context)
	{
		EnsureModuleInitialized(context);
		return (PythonDictionary)PythonContext.GetContext(context).GetModuleState(_dispatchTableKey);
	}

	internal static PythonDictionary GetExtensionRegistry(CodeContext context)
	{
		EnsureModuleInitialized(context);
		return (PythonDictionary)PythonContext.GetContext(context).GetModuleState(_extensionRegistryKey);
	}

	internal static PythonDictionary GetInvertedRegistry(CodeContext context)
	{
		EnsureModuleInitialized(context);
		return (PythonDictionary)PythonContext.GetContext(context).GetModuleState(_invertedRegistryKey);
	}

	internal static PythonDictionary GetExtensionCache(CodeContext context)
	{
		EnsureModuleInitialized(context);
		return (PythonDictionary)PythonContext.GetContext(context).GetModuleState(_extensionCacheKey);
	}

	[Documentation("pickle(type, function[, constructor]) -> None\n\nAssociate function with type, indicating that function should be used to\n\"reduce\" objects of the given type when pickling. function should behave as\nspecified by the \"Extended __reduce__ API\" section of PEP 307.\n\nReduction functions registered by calling pickle() can be retrieved later\nthrough copy_reg.dispatch_table[type].\n\nNote that calling pickle() will overwrite any previous association for the\ngiven type.\n\nThe constructor argument is ignored, and exists only for backwards\ncompatibility.")]
	public static void pickle(CodeContext context, object type, object function, [DefaultParameterValue(null)] object ctor)
	{
		EnsureCallable(context, function, "reduction functions must be callable");
		if (ctor != null)
		{
			constructor(context, ctor);
		}
		GetDispatchTable(context)[type] = function;
	}

	[Documentation("constructor(object) -> None\n\nRaise TypeError if object isn't callable. This function exists only for\nbackwards compatibility; for details, see\nhttp://mail.python.org/pipermail/python-dev/2006-June/066831.html.")]
	public static void constructor(CodeContext context, object callable)
	{
		EnsureCallable(context, callable, "constructors must be callable");
	}

	private static void EnsureCallable(CodeContext context, object @object, string message)
	{
		if (!PythonOps.IsCallable(context, @object))
		{
			throw PythonOps.TypeError(message);
		}
	}

	[Documentation("pickle_complex(complex_number) -> (<type 'complex'>, (real, imag))\n\nReduction function for pickling complex numbers.")]
	public static PythonTuple pickle_complex(CodeContext context, object complex)
	{
		return PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(typeof(Complex)), PythonTuple.MakeTuple(PythonOps.GetBoundAttr(context, complex, "real"), PythonOps.GetBoundAttr(context, complex, "imag")));
	}

	public static void clear_extension_cache(CodeContext context)
	{
		GetExtensionCache(context).clear();
	}

	[Documentation("Register an extension code.")]
	public static void add_extension(CodeContext context, object moduleName, object objectName, object value)
	{
		PythonTuple pythonTuple = PythonTuple.MakeTuple(moduleName, objectName);
		int code = GetCode(context, value);
		bool flag = GetExtensionRegistry(context).__contains__(pythonTuple);
		bool flag2 = GetInvertedRegistry(context).__contains__(code);
		if (!flag && !flag2)
		{
			GetExtensionRegistry(context)[pythonTuple] = code;
			GetInvertedRegistry(context)[code] = pythonTuple;
		}
		else if (!flag || !flag2 || !PythonOps.EqualRetBool(context, GetExtensionRegistry(context)[pythonTuple], code) || !PythonOps.EqualRetBool(context, GetInvertedRegistry(context)[code], pythonTuple))
		{
			if (flag)
			{
				throw PythonOps.ValueError("key {0} is already registered with code {1}", PythonOps.Repr(context, pythonTuple), PythonOps.Repr(context, GetExtensionRegistry(context)[pythonTuple]));
			}
			throw PythonOps.ValueError("code {0} is already in use for key {1}", PythonOps.Repr(context, code), PythonOps.Repr(context, GetInvertedRegistry(context)[code]));
		}
	}

	[Documentation("Unregister an extension code. (only for testing)")]
	public static void remove_extension(CodeContext context, object moduleName, object objectName, object value)
	{
		PythonTuple pythonTuple = PythonTuple.MakeTuple(moduleName, objectName);
		int code = GetCode(context, value);
		if (((IDictionary<object, object>)GetExtensionRegistry(context)).TryGetValue((object)pythonTuple, out object value2) && ((IDictionary<object, object>)GetInvertedRegistry(context)).TryGetValue((object)code, out object value3) && PythonOps.EqualRetBool(context, value2, code) && PythonOps.EqualRetBool(context, value3, pythonTuple))
		{
			GetExtensionRegistry(context).__delitem__(pythonTuple);
			GetInvertedRegistry(context).__delitem__(code);
			return;
		}
		throw PythonOps.ValueError("key {0} is not registered with code {1}", PythonOps.Repr(context, pythonTuple), PythonOps.Repr(context, code));
	}

	[Documentation("__newobj__(cls, *args) -> cls.__new__(cls, *args)\n\nHelper function for unpickling. Creates a new object of a given class.\nSee PEP 307 section \"The __newobj__ unpickling function\" for details.")]
	public static object __newobj__(CodeContext context, object cls, params object[] args)
	{
		object[] array = new object[1 + args.Length];
		array[0] = cls;
		for (int i = 0; i < args.Length; i++)
		{
			array[i + 1] = args[i];
		}
		return PythonOps.Invoke(context, cls, "__new__", array);
	}

	[Documentation("_reconstructor(basetype, objtype, basestate) -> object\n\nHelper function for unpickling. Creates and initializes a new object of a given\nclass. See PEP 307 section \"Case 2: pickling new-style class instances using\nprotocols 0 or 1\" for details.")]
	public static object _reconstructor(CodeContext context, object objType, object baseType, object baseState)
	{
		object obj;
		if (baseState == null)
		{
			obj = PythonOps.Invoke(context, baseType, "__new__", objType);
			PythonOps.Invoke(context, baseType, "__init__", obj);
		}
		else
		{
			obj = PythonOps.Invoke(context, baseType, "__new__", objType, baseState);
			PythonOps.Invoke(context, baseType, "__init__", obj, baseState);
		}
		return obj;
	}

	private static int GetCode(CodeContext context, object value)
	{
		try
		{
			int num = PythonContext.GetContext(context).ConvertToInt32(value);
			if (num > 0)
			{
				return num;
			}
		}
		catch (OverflowException)
		{
		}
		throw PythonOps.ValueError("code out of range");
	}

	private static void EnsureModuleInitialized(CodeContext context)
	{
		if (!PythonContext.GetContext(context).HasModuleState(_dispatchTableKey))
		{
			Importer.ImportBuiltin(context, "copy_reg");
		}
	}

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.NewObject = (BuiltinFunction)dict["__newobj__"];
		context.PythonReconstructor = (BuiltinFunction)dict["_reconstructor"];
		PythonDictionary pythonDictionary = new PythonDictionary();
		pythonDictionary[TypeCache.Complex] = dict["pickle_complex"];
		object dispatchTableKey = _dispatchTableKey;
		object value = (dict["dispatch_table"] = pythonDictionary);
		context.SetModuleState(dispatchTableKey, value);
		object extensionRegistryKey = _extensionRegistryKey;
		object value2 = (dict["_extension_registry"] = new PythonDictionary());
		context.SetModuleState(extensionRegistryKey, value2);
		object invertedRegistryKey = _invertedRegistryKey;
		object value3 = (dict["_inverted_registry"] = new PythonDictionary());
		context.SetModuleState(invertedRegistryKey, value3);
		object extensionCacheKey = _extensionCacheKey;
		object value4 = (dict["_extension_cache"] = new PythonDictionary());
		context.SetModuleState(extensionCacheKey, value4);
	}
}
