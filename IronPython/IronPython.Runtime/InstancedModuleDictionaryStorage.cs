using System.Collections.Generic;
using IronPython.Compiler;

namespace IronPython.Runtime;

internal class InstancedModuleDictionaryStorage : ModuleDictionaryStorage
{
	private BuiltinPythonModule _module;

	public override BuiltinPythonModule Instance => _module;

	public InstancedModuleDictionaryStorage(BuiltinPythonModule moduleInstance, Dictionary<string, PythonGlobal> globalsDict)
		: base(moduleInstance.GetType(), globalsDict)
	{
		_module = moduleInstance;
	}
}
