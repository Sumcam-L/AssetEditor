using System.IO;
using System.Runtime.CompilerServices;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonImport
{
	[PythonType]
	public sealed class NullImporter
	{
		public NullImporter(string path_string)
		{
		}

		public object find_module(params object[] args)
		{
			return null;
		}
	}

	public const string __doc__ = "Provides functions for programmatically creating and importing modules and packages.";

	internal const int PythonSource = 1;

	internal const int PythonCompiled = 2;

	internal const int CExtension = 3;

	internal const int PythonResource = 4;

	internal const int PackageDirectory = 5;

	internal const int CBuiltin = 6;

	internal const int PythonFrozen = 7;

	internal const int PythonCodeResource = 8;

	internal const int SearchError = 0;

	internal const int ImporterHook = 9;

	public const int PY_SOURCE = 1;

	public const int PY_COMPILED = 2;

	public const int C_EXTENSION = 3;

	public const int PY_RESOURCE = 4;

	public const int PKG_DIRECTORY = 5;

	public const int C_BUILTIN = 6;

	public const int PY_FROZEN = 7;

	public const int PY_CODERESOURCE = 8;

	public const int SEARCH_ERROR = 0;

	public const int IMP_HOOK = 9;

	private static readonly object _lockCountKey = new object();

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		if (!context.HasModuleState(_lockCountKey))
		{
			context.SetModuleState(_lockCountKey, 0L);
		}
	}

	public static string get_magic()
	{
		return "";
	}

	public static List get_suffixes()
	{
		return List.FromArrayNoCopy(PythonOps.MakeTuple(".py", "U", 1));
	}

	public static PythonTuple find_module(CodeContext context, string name)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("find_module() argument 1 must be string, not None");
		}
		return FindBuiltinOrSysPath(context, name);
	}

	public static PythonTuple find_module(CodeContext context, string name, List path)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("find_module() argument 1 must be string, not None");
		}
		if (path == null)
		{
			return FindBuiltinOrSysPath(context, name);
		}
		return FindModulePath(context, name, path);
	}

	public static object load_module(CodeContext context, string name, PythonFile file, string filename, PythonTuple description)
	{
		if (description == null)
		{
			throw PythonOps.TypeError("load_module() argument 4 must be 3-item sequence, not None");
		}
		if (description.__len__() != 3)
		{
			throw PythonOps.TypeError("load_module() argument 4 must be sequence of length 3, not {0}", description.__len__());
		}
		PythonContext context2 = PythonContext.GetContext(context);
		PythonModule moduleByName = context2.GetModuleByName(name);
		if (moduleByName != null)
		{
			Importer.ReloadModule(context, moduleByName, file);
			return moduleByName;
		}
		int num = PythonContext.GetContext(context).ConvertToInt32(description[2]);
		return num switch
		{
			1 => LoadPythonSource(context2, name, file, filename), 
			6 => LoadBuiltinModule(context, name), 
			5 => LoadPackageDirectory(context2, name, filename), 
			_ => throw PythonOps.TypeError("don't know how to import {0}, (type code {1}", name, num), 
		};
	}

	[Documentation("new_module(name) -> module\nCreates a new module without adding it to sys.modules.")]
	public static PythonModule new_module(CodeContext context, string name)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("new_module() argument 1 must be string, not None");
		}
		PythonModule pythonModule = new PythonModule();
		pythonModule.__dict__["__name__"] = name;
		pythonModule.__dict__["__doc__"] = null;
		pythonModule.__dict__["__package__"] = null;
		return pythonModule;
	}

	public static bool lock_held(CodeContext context)
	{
		return GetLockCount(context) != 0;
	}

	public static void acquire_lock(CodeContext context)
	{
		lock (_lockCountKey)
		{
			SetLockCount(context, GetLockCount(context) + 1);
		}
	}

	public static void release_lock(CodeContext context)
	{
		lock (_lockCountKey)
		{
			long lockCount = GetLockCount(context);
			if (lockCount == 0)
			{
				throw PythonOps.RuntimeError("not holding the import lock");
			}
			SetLockCount(context, lockCount - 1);
		}
	}

	public static object init_builtin(CodeContext context, string name)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("init_builtin() argument 1 must be string, not None");
		}
		return LoadBuiltinModule(context, name);
	}

	public static object init_frozen(string name)
	{
		return null;
	}

	public static object get_frozen_object(string name)
	{
		throw PythonOps.ImportError("No such frozen object named {0}", name);
	}

	public static int is_builtin(CodeContext context, string name)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("is_builtin() argument 1 must be string, not None");
		}
		if (PythonContext.GetContext(context).BuiltinModules.TryGetValue(name, out var value))
		{
			if (value.Assembly == typeof(PythonContext).Assembly)
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}

	public static bool is_frozen(string name)
	{
		return false;
	}

	public static object load_compiled(string name, string pathname)
	{
		return null;
	}

	public static object load_compiled(string name, string pathname, PythonFile file)
	{
		return null;
	}

	public static object load_dynamic(string name, string pathname)
	{
		return null;
	}

	public static object load_dynamic(string name, string pathname, PythonFile file)
	{
		return null;
	}

	public static object load_package(CodeContext context, string name, string pathname)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("load_package() argument 1 must be string, not None");
		}
		if (pathname == null)
		{
			throw PythonOps.TypeError("load_package() argument 2 must be string, not None");
		}
		return Importer.LoadPackageFromSource(context, name, pathname) ?? CreateEmptyPackage(context, name, pathname);
	}

	private static PythonModule CreateEmptyPackage(CodeContext context, string name, string pathname)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		PythonModule pythonModule = new PythonModule();
		pythonModule.__dict__["__name__"] = name;
		pythonModule.__dict__["__path__"] = pathname;
		context2.SystemStateModules[name] = pythonModule;
		return pythonModule;
	}

	public static object load_source(CodeContext context, string name, string pathname)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("load_source() argument 1 must be string, not None");
		}
		if (pathname == null)
		{
			throw PythonOps.TypeError("load_source() argument 2 must be string, not None");
		}
		PythonContext context2 = PythonContext.GetContext(context);
		if (!context2.DomainManager.Platform.FileExists(pathname))
		{
			throw PythonOps.IOError("Couldn't find file: {0}", pathname);
		}
		SourceUnit sourceCode = context2.CreateFileUnit(pathname, context2.DefaultEncoding, SourceCodeKind.File);
		return context2.CompileModule(pathname, name, sourceCode, ModuleOptions.Initialize);
	}

	public static object load_source(CodeContext context, string name, string pathname, PythonFile file)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("load_source() argument 1 must be string, not None");
		}
		if (pathname == null)
		{
			throw PythonOps.TypeError("load_source() argument 2 must be string, not None");
		}
		if (pathname == null)
		{
			throw PythonOps.TypeError("load_source() argument 3 must be file, not None");
		}
		return LoadPythonSource(PythonContext.GetContext(context), name, file, pathname);
	}

	public static object reload(CodeContext context, PythonModule scope)
	{
		return Builtin.reload(context, scope);
	}

	private static PythonTuple FindBuiltinOrSysPath(CodeContext context, string name)
	{
		if (!PythonContext.GetContext(context).TryGetSystemPath(out var path))
		{
			throw PythonOps.ImportError("sys.path must be a list of directory names");
		}
		return FindModuleBuiltinOrPath(context, name, path);
	}

	private static PythonTuple FindModulePath(CodeContext context, string name, List path)
	{
		if (name == null)
		{
			throw PythonOps.TypeError("find_module() argument 1 must be string, not None");
		}
		PlatformAdaptationLayer platform = context.LanguageContext.DomainManager.Platform;
		foreach (object item in path)
		{
			if (item is string path2)
			{
				string text = Path.Combine(path2, name);
				if (platform.DirectoryExists(text) && platform.FileExists(Path.Combine(text, "__init__.py")))
				{
					return PythonTuple.MakeTuple(null, text, PythonTuple.MakeTuple("", "", 5));
				}
				string text2 = text + ".py";
				if (platform.FileExists(text2))
				{
					Stream stream = platform.OpenInputFileStream(text2, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					PythonFile pythonFile = PythonFile.Create(context, stream, text2, "U");
					return PythonTuple.MakeTuple(pythonFile, text2, PythonTuple.MakeTuple(".py", "U", 1));
				}
			}
		}
		throw PythonOps.ImportError("No module named {0}", name);
	}

	private static PythonTuple FindModuleBuiltinOrPath(CodeContext context, string name, List path)
	{
		if (name.Equals("sys"))
		{
			return BuiltinModuleTuple(name);
		}
		if (name.Equals("clr"))
		{
			context.ShowCls = true;
			return BuiltinModuleTuple(name);
		}
		if (PythonContext.GetContext(context).BuiltinModules.TryGetValue(name, out var _))
		{
			return BuiltinModuleTuple(name);
		}
		return FindModulePath(context, name, path);
	}

	private static PythonTuple BuiltinModuleTuple(string name)
	{
		return PythonTuple.MakeTuple(null, name, PythonTuple.MakeTuple("", "", 6));
	}

	private static PythonModule LoadPythonSource(PythonContext context, string name, PythonFile file, string fileName)
	{
		SourceUnit sourceCode = context.CreateSnippet(file.read(), string.IsNullOrEmpty(fileName) ? null : fileName, SourceCodeKind.File);
		return context.CompileModule(fileName, name, sourceCode, ModuleOptions.Initialize);
	}

	private static PythonModule LoadPackageDirectory(PythonContext context, string moduleName, string path)
	{
		string text = Path.Combine(path, "__init__.py");
		SourceUnit sourceCode = context.CreateFileUnit(text, context.DefaultEncoding);
		return context.CompileModule(text, moduleName, sourceCode, ModuleOptions.Initialize);
	}

	private static object LoadBuiltinModule(CodeContext context, string name)
	{
		return Importer.ImportBuiltin(context, name);
	}

	private static long GetLockCount(CodeContext context)
	{
		return (long)PythonContext.GetContext(context).GetModuleState(_lockCountKey);
	}

	private static void SetLockCount(CodeContext context, long lockCount)
	{
		PythonContext.GetContext(context).SetModuleState(_lockCountKey, lockCount);
	}
}
