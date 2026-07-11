using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public sealed class ModuleContext
{
	private readonly PythonContext _pyContext;

	private readonly PythonDictionary _globals;

	private readonly CodeContext _globalContext;

	private readonly PythonModule _module;

	private ExtensionMethodSet _extensionMethods = ExtensionMethodSet.Empty;

	private ModuleOptions _features;

	public PythonDictionary Globals => _globals;

	public PythonContext Context => _pyContext;

	public Scope GlobalScope => _module.Scope;

	public CodeContext GlobalContext => _globalContext;

	public PythonModule Module => _module;

	public ModuleOptions Features
	{
		get
		{
			return _features;
		}
		set
		{
			_features = value;
		}
	}

	public bool ShowCls
	{
		get
		{
			return (_features & ModuleOptions.ShowClsMethods) != 0;
		}
		set
		{
			if (value)
			{
				_features |= ModuleOptions.ShowClsMethods;
			}
			else
			{
				_features &= ~ModuleOptions.ShowClsMethods;
			}
		}
	}

	internal ExtensionMethodSet ExtensionMethods
	{
		get
		{
			return _extensionMethods;
		}
		set
		{
			_extensionMethods = value;
		}
	}

	public ModuleContext(PythonDictionary globals, PythonContext creatingContext)
	{
		ContractUtils.RequiresNotNull(globals, "globals");
		ContractUtils.RequiresNotNull(creatingContext, "creatingContext");
		_globals = globals;
		_pyContext = creatingContext;
		_globalContext = new CodeContext(globals, this);
		_module = new PythonModule(globals);
		_module.Scope.SetExtension(_pyContext.ContextId, new PythonScopeExtension(_pyContext, _module, this));
	}

	public ModuleContext(PythonModule module, PythonContext creatingContext)
	{
		ContractUtils.RequiresNotNull(module, "module");
		ContractUtils.RequiresNotNull(creatingContext, "creatingContext");
		_globals = module.__dict__;
		_pyContext = creatingContext;
		_globalContext = new CodeContext(_globals, this);
		_module = module;
	}

	internal void InitializeBuiltins(bool moduleBuiltins)
	{
		if (!Globals.ContainsKey("__builtins__"))
		{
			if (moduleBuiltins)
			{
				Globals["__builtins__"] = Context.BuiltinModuleInstance;
			}
			else
			{
				Globals["__builtins__"] = Context.BuiltinModuleDict;
			}
		}
	}
}
