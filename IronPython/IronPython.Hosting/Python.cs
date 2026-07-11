using System;
using System.Collections.Generic;
using IronPython.Modules;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Utils;

namespace IronPython.Hosting;

public static class Python
{
	public static ScriptRuntime CreateRuntime()
	{
		return new ScriptRuntime(CreateRuntimeSetup(null));
	}

	public static ScriptRuntime CreateRuntime(IDictionary<string, object> options)
	{
		return new ScriptRuntime(CreateRuntimeSetup(options));
	}

	public static ScriptRuntime CreateRuntime(AppDomain domain)
	{
		ContractUtils.RequiresNotNull(domain, "domain");
		return ScriptRuntime.CreateRemote(domain, CreateRuntimeSetup(null));
	}

	public static ScriptRuntime CreateRuntime(AppDomain domain, IDictionary<string, object> options)
	{
		ContractUtils.RequiresNotNull(domain, "domain");
		return ScriptRuntime.CreateRemote(domain, CreateRuntimeSetup(options));
	}

	public static ScriptEngine CreateEngine()
	{
		return GetEngine(CreateRuntime());
	}

	public static ScriptEngine CreateEngine(IDictionary<string, object> options)
	{
		return GetEngine(CreateRuntime(options));
	}

	public static ScriptEngine CreateEngine(AppDomain domain)
	{
		return GetEngine(CreateRuntime(domain));
	}

	public static ScriptEngine CreateEngine(AppDomain domain, IDictionary<string, object> options)
	{
		return GetEngine(CreateRuntime(domain, options));
	}

	public static ScriptEngine GetEngine(ScriptRuntime runtime)
	{
		return runtime.GetEngineByTypeName(typeof(PythonContext).AssemblyQualifiedName);
	}

	public static ScriptScope GetSysModule(this ScriptRuntime runtime)
	{
		ContractUtils.RequiresNotNull(runtime, "runtime");
		return GetEngine(runtime).GetSysModule();
	}

	public static ScriptScope GetSysModule(this ScriptEngine engine)
	{
		ContractUtils.RequiresNotNull(engine, "engine");
		return GetPythonService(engine).GetSystemState();
	}

	public static ScriptScope GetBuiltinModule(this ScriptRuntime runtime)
	{
		ContractUtils.RequiresNotNull(runtime, "runtime");
		return GetEngine(runtime).GetBuiltinModule();
	}

	public static ScriptScope GetBuiltinModule(this ScriptEngine engine)
	{
		ContractUtils.RequiresNotNull(engine, "engine");
		return GetPythonService(engine).GetBuiltins();
	}

	public static ScriptScope GetClrModule(this ScriptRuntime runtime)
	{
		ContractUtils.RequiresNotNull(runtime, "runtime");
		return GetEngine(runtime).GetClrModule();
	}

	public static ScriptScope GetClrModule(this ScriptEngine engine)
	{
		ContractUtils.RequiresNotNull(engine, "engine");
		return GetPythonService(engine).GetClr();
	}

	public static ScriptScope ImportModule(this ScriptRuntime runtime, string moduleName)
	{
		ContractUtils.RequiresNotNull(runtime, "runtime");
		ContractUtils.RequiresNotNull(moduleName, "moduleName");
		return GetEngine(runtime).ImportModule(moduleName);
	}

	public static ScriptScope ImportModule(this ScriptEngine engine, string moduleName)
	{
		ContractUtils.RequiresNotNull(engine, "engine");
		ContractUtils.RequiresNotNull(moduleName, "moduleName");
		return GetPythonService(engine).ImportModule(engine, moduleName);
	}

	public static void ImportModule(this ScriptScope scope, string moduleName)
	{
		ContractUtils.RequiresNotNull(scope, "scope");
		ContractUtils.RequiresNotNull(moduleName, "moduleName");
		scope.SetVariable(moduleName, scope.Engine.ImportModule(moduleName));
	}

	public static void SetHostVariables(this ScriptRuntime runtime, string prefix, string executable, string version)
	{
		ContractUtils.RequiresNotNull(runtime, "runtime");
		ContractUtils.RequiresNotNull(prefix, "prefix");
		ContractUtils.RequiresNotNull(executable, "executable");
		ContractUtils.RequiresNotNull(version, "version");
		GetPythonContext(GetEngine(runtime)).SetHostVariables(prefix, executable, version);
	}

	public static void SetHostVariables(this ScriptEngine engine, string prefix, string executable, string version)
	{
		ContractUtils.RequiresNotNull(engine, "engine");
		ContractUtils.RequiresNotNull(prefix, "prefix");
		ContractUtils.RequiresNotNull(executable, "executable");
		ContractUtils.RequiresNotNull(version, "version");
		GetPythonContext(engine).SetHostVariables(prefix, executable, version);
	}

	public static void SetTrace(this ScriptEngine engine, TracebackDelegate traceFunc)
	{
		SysModule.settrace(GetPythonContext(engine).SharedContext, traceFunc);
	}

	public static void SetTrace(this ScriptRuntime runtime, TracebackDelegate traceFunc)
	{
		GetEngine(runtime).SetTrace(traceFunc);
	}

	public static void CallTracing(this ScriptRuntime runtime, object traceFunc, params object[] args)
	{
		GetEngine(runtime).CallTracing(traceFunc, args);
	}

	public static void CallTracing(this ScriptEngine engine, object traceFunc, params object[] args)
	{
		SysModule.call_tracing(GetPythonContext(engine).SharedContext, traceFunc, PythonTuple.MakeTuple(args));
	}

	public static ScriptRuntimeSetup CreateRuntimeSetup(IDictionary<string, object> options)
	{
		ScriptRuntimeSetup scriptRuntimeSetup = new ScriptRuntimeSetup();
		scriptRuntimeSetup.LanguageSetups.Add(CreateLanguageSetup(options));
		if (options != null)
		{
			if (options.TryGetValue("Debug", out var value) && value is bool && (bool)value)
			{
				scriptRuntimeSetup.DebugMode = true;
			}
			if (options.TryGetValue("PrivateBinding", out value) && value is bool && (bool)value)
			{
				scriptRuntimeSetup.PrivateBinding = true;
			}
		}
		return scriptRuntimeSetup;
	}

	public static LanguageSetup CreateLanguageSetup(IDictionary<string, object> options)
	{
		LanguageSetup languageSetup = new LanguageSetup(typeof(PythonContext).AssemblyQualifiedName, "IronPython 2.7.3", "IronPython;Python;py".Split(';'), ".py".Split(';'));
		if (options != null)
		{
			foreach (KeyValuePair<string, object> option in options)
			{
				languageSetup.Options.Add(option.Key, option.Value);
			}
		}
		return languageSetup;
	}

	public static ScriptScope CreateModule(this ScriptEngine engine, string name)
	{
		return GetPythonService(engine).CreateModule(name, string.Empty, string.Empty);
	}

	public static ScriptScope CreateModule(this ScriptEngine engine, string name, string filename)
	{
		return GetPythonService(engine).CreateModule(name, filename, string.Empty);
	}

	public static ScriptScope CreateModule(this ScriptEngine engine, string name, string filename, string docString)
	{
		return GetPythonService(engine).CreateModule(name, filename, docString);
	}

	public static string[] GetModuleFilenames(this ScriptEngine engine)
	{
		return GetPythonService(engine).GetModuleFilenames();
	}

	private static PythonService GetPythonService(ScriptEngine engine)
	{
		return engine.GetService<PythonService>(new object[1] { engine });
	}

	private static PythonContext GetPythonContext(ScriptEngine engine)
	{
		return HostingHelpers.GetLanguageContext(engine) as PythonContext;
	}
}
