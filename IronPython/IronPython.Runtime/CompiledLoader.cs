using System.Collections.Generic;
using System.IO;
using IronPython.Compiler;
using Microsoft.Scripting;

namespace IronPython.Runtime;

public class CompiledLoader
{
	private Dictionary<string, OnDiskScriptCode> _codes = new Dictionary<string, OnDiskScriptCode>();

	internal void AddScriptCode(ScriptCode code)
	{
		if (!(code is OnDiskScriptCode onDiskScriptCode))
		{
			return;
		}
		if (onDiskScriptCode.ModuleName == "__main__")
		{
			_codes["__main__"] = onDiskScriptCode;
			return;
		}
		string path = code.SourceUnit.Path;
		path = path.Replace(Path.DirectorySeparatorChar, '.');
		if (path.EndsWith("__init__.py"))
		{
			path = path.Substring(0, path.Length - ".__init__.py".Length);
		}
		_codes[path] = onDiskScriptCode;
	}

	public ModuleLoader find_module(CodeContext context, string fullname, List path)
	{
		if (_codes.TryGetValue(fullname, out var value))
		{
			int num = fullname.LastIndexOf('.');
			string name = fullname;
			string parentName = null;
			if (num != -1)
			{
				parentName = fullname.Substring(0, num);
				name = fullname.Substring(num + 1);
			}
			return new ModuleLoader(value, parentName, name);
		}
		return null;
	}
}
