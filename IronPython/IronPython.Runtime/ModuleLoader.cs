using IronPython.Compiler;

namespace IronPython.Runtime;

public sealed class ModuleLoader
{
	private readonly OnDiskScriptCode _sc;

	private readonly string _parentName;

	private readonly string _name;

	internal ModuleLoader(OnDiskScriptCode sc, string parentName, string name)
	{
		_sc = sc;
		_parentName = parentName;
		_name = name;
	}

	public PythonModule load_module(CodeContext context, string fullName)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		CodeContext codeContext = _sc.CreateContext();
		codeContext.ModuleContext.InitializeBuiltins(moduleBuiltins: false);
		context2.InitializeModule(_sc.SourceUnit.Path, codeContext.ModuleContext, _sc, ModuleOptions.Initialize);
		if (_parentName != null && context2.SystemStateModules.TryGetValue(_parentName, out var value) && value is PythonModule pythonModule)
		{
			pythonModule.__dict__[_name] = codeContext.ModuleContext.Module;
		}
		return codeContext.ModuleContext.Module;
	}
}
