using System.Collections.Generic;
using System.Threading;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class PythonScopeExtension : ScopeExtension
{
	private readonly ModuleContext _modContext;

	private readonly PythonModule _module;

	private Dictionary<object, object> _objectKeys;

	public ModuleContext ModuleContext => _modContext;

	public PythonModule Module => _module;

	public Dictionary<object, object> ObjectKeys => _objectKeys;

	public PythonScopeExtension(PythonContext context, Scope scope)
		: base(scope)
	{
		_module = new PythonModule(context, scope);
		_modContext = new ModuleContext(_module, context);
	}

	public PythonScopeExtension(PythonContext context, PythonModule module, ModuleContext modContext)
		: base(module.Scope)
	{
		_module = module;
		_modContext = modContext;
	}

	public Dictionary<object, object> EnsureObjectKeys()
	{
		if (_objectKeys == null)
		{
			Interlocked.CompareExchange(ref _objectKeys, new Dictionary<object, object>(), null);
		}
		return _objectKeys;
	}
}
