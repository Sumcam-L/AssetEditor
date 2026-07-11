using System.Diagnostics;
using IronPython.Compiler;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[DebuggerDisplay("module: {ModuleName}", Type = "module")]
[DebuggerTypeProxy(typeof(DebugProxy))]
public sealed class CodeContext
{
	internal class DebugProxy
	{
		private readonly CodeContext _context;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public PythonModule Members => _context.Module;

		public DebugProxy(CodeContext context)
		{
			_context = context;
		}
	}

	private readonly ModuleContext _modContext;

	private readonly PythonDictionary _dict;

	public ModuleContext ModuleContext => _modContext;

	public Scope GlobalScope => _modContext.GlobalScope;

	public PythonContext LanguageContext => _modContext.Context;

	internal PythonDictionary GlobalDict => _modContext.Globals;

	internal bool ShowCls
	{
		get
		{
			return ModuleContext.ShowCls;
		}
		set
		{
			ModuleContext.ShowCls = value;
		}
	}

	internal PythonDictionary Dict => _dict;

	internal bool IsTopLevel => Dict != ModuleContext.Globals;

	internal PythonModule Module => _modContext.Module;

	internal string ModuleName => Module.GetName();

	public CodeContext(PythonDictionary dict, ModuleContext moduleContext)
	{
		ContractUtils.RequiresNotNull(dict, "dict");
		ContractUtils.RequiresNotNull(moduleContext, "moduleContext");
		_dict = dict;
		_modContext = moduleContext;
	}

	internal bool TryLookupName(string name, out object value)
	{
		if (_dict.TryGetValue(name, out value))
		{
			return true;
		}
		return _modContext.Globals.TryGetValue(name, out value);
	}

	internal bool TryLookupBuiltin(string name, out object value)
	{
		if (!GlobalDict.TryGetValue("__builtins__", out var value2))
		{
			value = null;
			return false;
		}
		if (value2 is PythonModule pythonModule && pythonModule.__dict__.TryGetValue(name, out value))
		{
			return true;
		}
		if (value2 is PythonDictionary pythonDictionary && pythonDictionary.TryGetValue(name, out value))
		{
			return true;
		}
		value = null;
		return false;
	}

	internal bool TryGetVariable(string name, out object value)
	{
		return Dict.TryGetValue(name, out value);
	}

	internal bool TryRemoveVariable(string name)
	{
		return Dict.Remove(name);
	}

	internal void SetVariable(string name, object value)
	{
		Dict.Add(name, value);
	}

	internal bool TryGetGlobalVariable(string name, out object res)
	{
		return GlobalDict.TryGetValue(name, out res);
	}

	internal void SetGlobalVariable(string name, object value)
	{
		GlobalDict.Add(name, value);
	}

	internal bool TryRemoveGlobalVariable(string name)
	{
		return GlobalDict.Remove(name);
	}

	internal PythonGlobal[] GetGlobalArray()
	{
		return ((GlobalDictionaryStorage)_dict._storage).Data;
	}

	internal PythonDictionary GetBuiltinsDict()
	{
		if (GlobalDict._storage.TryGetBuiltins(out var value))
		{
			if (value is PythonModule pythonModule)
			{
				return pythonModule.__dict__;
			}
			return value as PythonDictionary;
		}
		return null;
	}
}
