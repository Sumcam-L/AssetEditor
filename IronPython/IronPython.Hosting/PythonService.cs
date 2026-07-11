using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;

namespace IronPython.Hosting;

public sealed class PythonService : MarshalByRefObject
{
	private readonly ScriptEngine _engine;

	private readonly PythonContext _context;

	private ScriptScope _sys;

	private ScriptScope _builtins;

	private ScriptScope _clr;

	public PythonService(PythonContext context, ScriptEngine engine)
	{
		_context = context;
		_engine = engine;
	}

	public ScriptScope GetSystemState()
	{
		if (_sys == null)
		{
			Interlocked.CompareExchange(ref _sys, HostingHelpers.CreateScriptScope(_engine, _context.SystemState.Scope), null);
		}
		return _sys;
	}

	public ScriptScope GetBuiltins()
	{
		if (_builtins == null)
		{
			Interlocked.CompareExchange(ref _builtins, HostingHelpers.CreateScriptScope(_engine, _context.BuiltinModuleInstance.Scope), null);
		}
		return _builtins;
	}

	public ScriptScope GetClr()
	{
		if (_clr == null)
		{
			Interlocked.CompareExchange(ref _clr, HostingHelpers.CreateScriptScope(_engine, _context.ClrModule.Scope), null);
		}
		return _clr;
	}

	public ScriptScope CreateModule(string name, string filename, string docString)
	{
		PythonModule pythonModule = new PythonModule();
		_context.PublishModule(name, pythonModule);
		pythonModule.__init__(name, docString);
		pythonModule.__dict__["__file__"] = filename;
		return HostingHelpers.CreateScriptScope(_engine, pythonModule.Scope);
	}

	public ScriptScope ImportModule(ScriptEngine engine, string name)
	{
		if (Importer.ImportModule(_context.SharedClsContext, _context.SharedClsContext.GlobalDict, name, bottom: false, -1) is PythonModule pythonModule)
		{
			return HostingHelpers.CreateScriptScope(engine, pythonModule.Scope);
		}
		throw PythonOps.ImportError("no module named {0}", name);
	}

	public string[] GetModuleFilenames()
	{
		List<string> list = new List<string>();
		if (_engine.GetSysModule().GetVariable("modules") is PythonDictionary pythonDictionary)
		{
			foreach (KeyValuePair<object, object> item in pythonDictionary)
			{
				string text = item.Key as string;
				PythonModule pythonModule = item.Value as PythonModule;
				if (text != null && pythonModule != null)
				{
					PythonDictionary dict__ = pythonModule.Get__dict__();
					if (dict__.TryGetValue("__file__", out var value) && value != null)
					{
						list.Add(text);
					}
				}
			}
		}
		return list.ToArray();
	}

	public void DispatchCommand(Action command)
	{
		_context.DispatchCommand(command);
	}

	public ObjectHandle GetSetCommandDispatcher(ObjectHandle dispatcher)
	{
		Action<Action> setCommandDispatcher = _context.GetSetCommandDispatcher((Action<Action>)dispatcher.Unwrap());
		if (setCommandDispatcher != null)
		{
			return new ObjectHandle(setCommandDispatcher);
		}
		return null;
	}

	public ObjectHandle GetLocalCommandDispatcher()
	{
		return new ObjectHandle((Action<Action>)delegate(Action action)
		{
			_context.DispatchCommand(action);
		});
	}

	public override object InitializeLifetimeService()
	{
		return _engine.InitializeLifetimeService();
	}
}
