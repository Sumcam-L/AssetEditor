using System;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonGC
{
	public const string __doc__ = "Provides functions for inspecting, configuring, and forcing garbage collection.";

	public const int DEBUG_STATS = 1;

	public const int DEBUG_COLLECTABLE = 2;

	public const int DEBUG_UNCOLLECTABLE = 4;

	public const int DEBUG_INSTANCES = 8;

	public const int DEBUG_OBJECTS = 16;

	public const int DEBUG_SAVEALL = 32;

	public const int DEBUG_LEAK = 62;

	private static readonly object _threadholdKey = new object();

	public static List garbage => new List();

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.SetModuleState(_threadholdKey, PythonTuple.MakeTuple(65536, 262144, 1048576));
	}

	public static void enable()
	{
	}

	public static void disable(CodeContext context)
	{
		PythonOps.Warn(context, PythonExceptions.RuntimeWarning, "IronPython has no support for disabling the GC");
	}

	public static object isenabled()
	{
		return ScriptingRuntimeHelpers.True;
	}

	public static int collect(CodeContext context, int generation)
	{
		return PythonContext.GetContext(context).Collect(generation);
	}

	public static int collect(CodeContext context)
	{
		return collect(context, GC.MaxGeneration);
	}

	public static void set_debug(object o)
	{
		throw PythonOps.NotImplementedError("gc.set_debug isn't implemented");
	}

	public static object get_debug()
	{
		return null;
	}

	public static object[] get_objects()
	{
		throw PythonOps.NotImplementedError("gc.get_objects isn't implemented");
	}

	public static void set_threshold(CodeContext context, params object[] args)
	{
		SetThresholds(context, PythonTuple.MakeTuple(args));
	}

	public static PythonTuple get_threshold(CodeContext context)
	{
		return GetThresholds(context);
	}

	public static object[] get_referrers(params object[] objs)
	{
		throw PythonOps.NotImplementedError("gc.get_referrers isn't implemented");
	}

	public static object[] get_referents(params object[] objs)
	{
		throw PythonOps.NotImplementedError("gc.get_referents isn't implemented");
	}

	private static PythonTuple GetThresholds(CodeContext context)
	{
		return (PythonTuple)PythonContext.GetContext(context).GetModuleState(_threadholdKey);
	}

	private static void SetThresholds(CodeContext context, PythonTuple thresholds)
	{
		PythonContext.GetContext(context).SetModuleState(_threadholdKey, thresholds);
	}
}
