using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using IronPython.Compiler;
using IronPython.Modules;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

public static class Importer
{
	private class PythonFileStreamContentProvider : StreamContentProvider
	{
		private readonly PythonFile _file;

		public PythonFileStreamContentProvider(PythonFile file)
		{
			_file = file;
		}

		public override Stream GetStream()
		{
			return _file._stream;
		}
	}

	internal const string ModuleReloadMethod = "PerformModuleReload";

	public static object Import(CodeContext context, string fullName, PythonTuple from, int level)
	{
		return LightExceptions.CheckAndThrow(ImportLightThrow(context, fullName, from, level));
	}

	[LightThrowing]
	internal static object ImportLightThrow(CodeContext context, string fullName, PythonTuple from, int level)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (level == -1)
		{
			CallSite<Func<CallSite, CodeContext, object, string, PythonDictionary, PythonDictionary, PythonTuple, object>> oldImportSite = context2.OldImportSite;
			return oldImportSite.Target(oldImportSite, context, FindImportFunction(context), fullName, Builtin.globals(context), context.Dict, from);
		}
		CallSite<Func<CallSite, CodeContext, object, string, PythonDictionary, PythonDictionary, PythonTuple, int, object>> importSite = context2.ImportSite;
		return importSite.Target(importSite, context, FindImportFunction(context), fullName, Builtin.globals(context), context.Dict, from, level);
	}

	public static object ImportFrom(CodeContext context, object from, string name)
	{
		object ret2;
		if (from is PythonModule pythonModule)
		{
			object ret;
			if (pythonModule.GetType() == typeof(PythonModule))
			{
				if (pythonModule.__dict__.TryGetValue(name, out ret))
				{
					return ret;
				}
			}
			else if (PythonOps.TryGetBoundAttr(context, pythonModule, name, out ret))
			{
				return ret;
			}
			if (pythonModule.__dict__._storage.TryGetPath(out var value) && value is List path)
			{
				return ImportNestedModule(context, pythonModule, name, path);
			}
		}
		else if (from is PythonType pythonType)
		{
			if (pythonType.TryResolveSlot(context, name, out var slot) && slot.TryGetValue(context, null, pythonType, out var value2))
			{
				return value2;
			}
		}
		else if (from is NamespaceTracker self)
		{
			object customMember = NamespaceTrackerOps.GetCustomMember(context, self, name);
			if (customMember != OperationFailed.Value)
			{
				return customMember;
			}
		}
		else if (PythonOps.TryGetBoundAttr(context, from, name, out ret2))
		{
			return ret2;
		}
		throw PythonOps.ImportError("Cannot import name {0}", name);
	}

	private static object ImportModuleFrom(CodeContext context, object from, string name)
	{
		if (from is PythonModule pythonModule && pythonModule.__dict__._storage.TryGetPath(out var value) && value is List path)
		{
			return ImportNestedModule(context, pythonModule, name, path);
		}
		if (from is NamespaceTracker namespaceTracker && namespaceTracker.TryGetValue(name, out object value2))
		{
			return MemberTrackerToPython(context, value2);
		}
		throw PythonOps.ImportError("No module named {0}", name);
	}

	public static object ImportModule(CodeContext context, object globals, string modName, bool bottom, int level)
	{
		if (modName.IndexOf(Path.DirectorySeparatorChar) != -1)
		{
			throw PythonOps.ImportError("Import by filename is not supported.", modName);
		}
		string text = null;
		if (globals is PythonDictionary pythonDictionary)
		{
			if (pythonDictionary._storage.TryGetPackage(out var value))
			{
				text = value as string;
				if (text == null && value != null)
				{
					throw PythonOps.ValueError("__package__ set to non-string");
				}
			}
			else
			{
				text = null;
				if (level > 0 && pythonDictionary._storage.TryGetName(out var value2) && value2 is string)
				{
					if (pythonDictionary._storage.TryGetPath(out var _))
					{
						pythonDictionary["__package__"] = value2;
					}
					else
					{
						pythonDictionary["__package__"] = ((string)value2).rpartition(".")[0];
					}
				}
			}
		}
		object ret = null;
		int num = modName.IndexOf('.');
		string text2 = ((num != -1) ? modName.Substring(0, num) : modName);
		string text3 = null;
		if (level != 0 && TryGetNameAndPath(context, globals, text2, level, text, out var full, out var path, out var parentMod))
		{
			text3 = full;
			if (!TryGetExistingOrMetaPathModule(context, full, path, out ret))
			{
				ret = ImportFromPath(context, text2, full, path);
				if (ret == null)
				{
					context.LanguageContext.SystemStateModules[full] = null;
				}
				else if (parentMod != null)
				{
					parentMod.__dict__[modName] = ret;
				}
			}
			else if (num == -1 && ret is NamespaceTracker)
			{
				context.ShowCls = true;
			}
		}
		if (level <= 0 && ret == null)
		{
			if (!string.IsNullOrEmpty(text) && !PythonContext.GetContext(context).SystemStateModules.TryGetValue(text, out var _))
			{
				PythonModule pythonModule = new PythonModule();
				pythonModule.__dict__["__file__"] = text;
				pythonModule.__dict__["__name__"] = text;
				ModuleContext moduleContext = new ModuleContext(pythonModule.__dict__, context.LanguageContext);
				PythonOps.Warn(moduleContext.GlobalContext, PythonExceptions.RuntimeWarning, "Parent module '{0}' not found while handling absolute import", text);
			}
			ret = ImportTopAbsolute(context, text2);
			text3 = text2;
			if (ret == null)
			{
				return null;
			}
		}
		string[] array = modName.Split('.');
		object obj = ret;
		string text4 = null;
		for (int i = 0; i < array.Length; i++)
		{
			text4 = ((i == 0) ? text3 : (text4 + "." + array[i]));
			if (TryGetExistingModule(context, text4, out var ret2))
			{
				obj = ret2;
				if (i == 0)
				{
					ret = obj;
				}
			}
			else if (i != 0)
			{
				obj = ImportModuleFrom(context, obj, array[i]);
			}
			else
			{
				ret = obj;
			}
		}
		if (!bottom)
		{
			return ret;
		}
		return obj;
	}

	private static bool TryGetNameAndPath(CodeContext context, object globals, string name, int level, string package, out string full, out List path, out PythonModule parentMod)
	{
		full = name;
		path = null;
		parentMod = null;
		if (!(globals is PythonDictionary pythonDictionary) || !pythonDictionary._storage.TryGetName(out var value))
		{
			return false;
		}
		if (!(value is string text))
		{
			return false;
		}
		string text3;
		if (package == null)
		{
			if (pythonDictionary._storage.TryGetPath(out value) && (path = value as List) != null)
			{
				if (level == -1)
				{
					full = text + "." + name;
					if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(text, out var value2))
					{
						parentMod = value2 as PythonModule;
					}
				}
				else if (string.IsNullOrEmpty(name))
				{
					full = text.rsplit(".", level - 1)[0] as string;
				}
				else
				{
					string text2 = text.rsplit(".", level - 1)[0] as string;
					full = text2 + "." + name;
					if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(text2, out var value3))
					{
						parentMod = value3 as PythonModule;
					}
				}
				return true;
			}
			int num = text.LastIndexOf('.');
			if (num == -1)
			{
				if (level > 0)
				{
					throw PythonOps.ValueError("Attempted relative import in non-package");
				}
				return false;
			}
			int num2 = level;
			while (num2 > 1 && num != -1)
			{
				num = text.LastIndexOf('.', num - 1);
				num2--;
			}
			text3 = ((num != -1) ? text.Substring(0, num) : text);
		}
		else
		{
			text3 = GetParentPackageName(level - 1, package.Split('.'));
		}
		path = GetParentPathAndModule(context, text3, out parentMod);
		if (path != null)
		{
			if (string.IsNullOrEmpty(name))
			{
				full = text3;
			}
			else
			{
				full = text3 + "." + name;
			}
			return true;
		}
		if (level > 0)
		{
			throw PythonOps.SystemError("Parent module '{0}' not loaded, cannot perform relative import", text3);
		}
		return false;
	}

	private static string GetParentPackageName(int level, string[] names)
	{
		StringBuilder stringBuilder = new StringBuilder(names[0]);
		if (level < 0)
		{
			level = 1;
		}
		for (int i = 1; i < names.Length - level; i++)
		{
			stringBuilder.Append('.');
			stringBuilder.Append(names[i]);
		}
		return stringBuilder.ToString();
	}

	public static object ReloadModule(CodeContext context, PythonModule module)
	{
		return ReloadModule(context, module, null);
	}

	internal static object ReloadModule(CodeContext context, PythonModule module, PythonFile file)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		string file2 = module.GetFile();
		if (file2 == null)
		{
			ReloadBuiltinModule(context, module);
			return module;
		}
		string name = module.GetName();
		if (name != null)
		{
			List path = null;
			int num = name.LastIndexOf('.');
			if (num != -1)
			{
				path = GetParentPathAndModule(context, name.Substring(0, num), out var _);
			}
			if (TryLoadMetaPathModule(context, module.GetName(), path, out var ret) && ret != null)
			{
				return module;
			}
			if (PythonContext.GetContext(context).TryGetSystemPath(out var path2))
			{
				object obj = ImportFromPathHook(context, name, name, path2, null);
				if (obj != null)
				{
					return obj;
				}
			}
		}
		SourceUnit sourceCode;
		if (file != null)
		{
			sourceCode = context2.CreateSourceUnit(new PythonFileStreamContentProvider(file), file2, file.Encoding, SourceCodeKind.File);
		}
		else
		{
			if (!context2.DomainManager.Platform.FileExists(file2))
			{
				throw PythonOps.SystemError("module source file not found");
			}
			sourceCode = context2.CreateFileUnit(file2, context2.DefaultEncoding, SourceCodeKind.File);
		}
		context2.GetScriptCode(sourceCode, name, ModuleOptions.None, CompilationMode.Lookup).Run(module.Scope);
		return module;
	}

	private static List GetParentPathAndModule(CodeContext context, string parentModuleName, out PythonModule parentModule)
	{
		List result = null;
		parentModule = null;
		if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(parentModuleName, out var value))
		{
			parentModule = value as PythonModule;
			if (parentModule != null && parentModule.__dict__._storage.TryGetPath(out var value2))
			{
				result = value2 as List;
			}
		}
		return result;
	}

	private static void ReloadBuiltinModule(CodeContext context, PythonModule module)
	{
		string name = module.GetName();
		PythonContext context2 = PythonContext.GetContext(context);
		if (!context2.BuiltinModules.TryGetValue(name, out var _))
		{
			throw PythonOps.ImportError("no module named {0}", module.GetName());
		}
		((ModuleDictionaryStorage)module.__dict__._storage).Reload();
	}

	private static bool TryGetExistingOrMetaPathModule(CodeContext context, string fullName, List path, out object ret)
	{
		if (TryGetExistingModule(context, fullName, out ret))
		{
			return true;
		}
		return TryLoadMetaPathModule(context, fullName, path, out ret);
	}

	private static bool TryLoadMetaPathModule(CodeContext context, string fullName, List path, out object ret)
	{
		if (PythonContext.GetContext(context).GetSystemStateValue("meta_path") is List list)
		{
			foreach (object item in (IEnumerable)list)
			{
				if (FindAndLoadModuleFromImporter(context, item, fullName, path, out ret))
				{
					return true;
				}
			}
		}
		ret = null;
		return false;
	}

	private static bool FindAndLoadModuleFromImporter(CodeContext context, object importer, string fullName, List path, out object ret)
	{
		object boundAttr = PythonOps.GetBoundAttr(context, importer, "find_module");
		PythonContext context2 = PythonContext.GetContext(context);
		object obj = context2.Call(context, boundAttr, fullName, path);
		if (obj != null)
		{
			object boundAttr2 = PythonOps.GetBoundAttr(context, obj, "load_module");
			ret = context2.Call(context, boundAttr2, fullName);
			return ret != null;
		}
		ret = null;
		return false;
	}

	internal static bool TryGetExistingModule(CodeContext context, string fullName, out object ret)
	{
		if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(fullName, out ret))
		{
			return true;
		}
		return false;
	}

	private static object ImportTopAbsolute(CodeContext context, string name)
	{
		if (TryGetExistingModule(context, name, out var ret))
		{
			if (IsReflected(ret))
			{
				ret = ImportReflected(context, name) ?? ret;
			}
			NamespaceTracker namespaceTracker = ret as NamespaceTracker;
			if (namespaceTracker != null || ret == PythonContext.GetContext(context).ClrModule)
			{
				context.ShowCls = true;
			}
			return ret;
		}
		if (TryLoadMetaPathModule(context, name, null, out ret))
		{
			return ret;
		}
		ret = ImportBuiltin(context, name);
		if (ret != null)
		{
			return ret;
		}
		if (PythonContext.GetContext(context).TryGetSystemPath(out var path))
		{
			ret = ImportFromPath(context, name, name, path);
			if (ret != null)
			{
				return ret;
			}
		}
		ret = ImportReflected(context, name);
		if (ret != null)
		{
			return ret;
		}
		return null;
	}

	private static bool TryGetNestedModule(CodeContext context, PythonModule scope, string name, out object nested)
	{
		if (scope.__dict__.TryGetValue(name, out nested))
		{
			if (nested is PythonModule)
			{
				return true;
			}
			if (nested is PythonType { IsSystemType: not false })
			{
				return true;
			}
		}
		return false;
	}

	private static object ImportNestedModule(CodeContext context, PythonModule module, string name, List path)
	{
		string text = CreateFullName(module.GetName(), name);
		if (TryGetExistingOrMetaPathModule(context, text, path, out var ret))
		{
			module.__dict__[name] = ret;
			return ret;
		}
		if (TryGetNestedModule(context, module, name, out ret))
		{
			return ret;
		}
		ImportFromPath(context, name, text, path);
		if (PythonContext.GetContext(context).SystemStateModules.TryGetValue(text, out var value))
		{
			module.__dict__[name] = value;
			return value;
		}
		throw PythonOps.ImportError("cannot import {0} from {1}", name, module.GetName());
	}

	private static object FindImportFunction(CodeContext context)
	{
		PythonDictionary pythonDictionary = context.GetBuiltinsDict() ?? PythonContext.GetContext(context).BuiltinModuleDict;
		if (pythonDictionary._storage.TryGetImport(out var value))
		{
			return value;
		}
		throw PythonOps.ImportError("cannot find __import__");
	}

	internal static object ImportBuiltin(CodeContext context, string name)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (name == "sys")
		{
			return context2.SystemState;
		}
		if (name == "clr")
		{
			context.ShowCls = true;
			context2.SystemStateModules["clr"] = context2.ClrModule;
			return context2.ClrModule;
		}
		return context2.GetBuiltinModule(name);
	}

	private static object ImportReflected(CodeContext context, string name)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (!PythonOps.ScopeTryGetMember(context, context2.DomainManager.Globals, name, out var value) && (value = context2.TopNamespace.TryGetPackageAny(name)) == null)
		{
			value = TryImportSourceFile(context2, name);
		}
		value = MemberTrackerToPython(context, value);
		if (value != null)
		{
			PythonContext.GetContext(context).SystemStateModules[name] = value;
		}
		return value;
	}

	internal static object MemberTrackerToPython(CodeContext context, object ret)
	{
		if (ret is MemberTracker memberTracker)
		{
			context.ShowCls = true;
			object obj = memberTracker;
			switch (memberTracker.MemberType)
			{
			case TrackerTypes.Type:
				obj = DynamicHelpers.GetPythonTypeFromType(((TypeTracker)memberTracker).Type);
				break;
			case TrackerTypes.Field:
				obj = PythonTypeOps.GetReflectedField(((FieldTracker)memberTracker).Field);
				break;
			case TrackerTypes.Event:
				obj = PythonTypeOps.GetReflectedEvent((EventTracker)memberTracker);
				break;
			case TrackerTypes.Method:
			{
				MethodTracker methodTracker = memberTracker as MethodTracker;
				obj = PythonTypeOps.GetBuiltinFunction(methodTracker.DeclaringType, methodTracker.Name, new MemberInfo[1] { methodTracker.Method });
				break;
			}
			}
			ret = obj;
		}
		return ret;
	}

	internal static PythonModule TryImportSourceFile(PythonContext context, string name)
	{
		SourceUnit sourceUnit = TryFindSourceFile(context, name);
		PlatformAdaptationLayer platform = context.DomainManager.Platform;
		if (sourceUnit == null || GetFullPathAndValidateCase(context, platform.CombinePaths(platform.GetDirectoryName(sourceUnit.Path), name + platform.GetExtension(sourceUnit.Path)), isDir: false) == null)
		{
			return null;
		}
		PythonModule pythonModule = ExecuteSourceUnit(context, sourceUnit);
		if (sourceUnit.LanguageContext != context)
		{
			context.SystemStateModules[name] = pythonModule;
		}
		PythonOps.ScopeSetMember(context.SharedContext, sourceUnit.LanguageContext.DomainManager.Globals, name, pythonModule);
		return pythonModule;
	}

	internal static PythonModule ExecuteSourceUnit(PythonContext context, SourceUnit sourceUnit)
	{
		ScriptCode scriptCode = sourceUnit.Compile();
		Scope scope = scriptCode.CreateScope();
		PythonModule module = ((PythonScopeExtension)context.EnsureScopeExtension(scope)).Module;
		scriptCode.Run(scope);
		return module;
	}

	internal static SourceUnit TryFindSourceFile(PythonContext context, string name)
	{
		if (!context.TryGetSystemPath(out var path))
		{
			return null;
		}
		foreach (object item in path)
		{
			if (!(item is string path2))
			{
				continue;
			}
			string text = null;
			LanguageContext languageContext = null;
			string[] fileExtensions = context.DomainManager.Configuration.GetFileExtensions();
			foreach (string text2 in fileExtensions)
			{
				string text3;
				try
				{
					text3 = context.DomainManager.Platform.CombinePaths(path2, name + text2);
				}
				catch (ArgumentException)
				{
					continue;
				}
				if (context.DomainManager.Platform.FileExists(text3))
				{
					if (text != null)
					{
						throw PythonOps.ImportError($"Found multiple modules of the same name '{name}': '{text}' and '{text3}'");
					}
					text = text3;
					languageContext = context.DomainManager.GetLanguageByExtension(text2);
				}
			}
			if (text != null)
			{
				return languageContext.CreateFileUnit(text);
			}
		}
		return null;
	}

	private static bool IsReflected(object module)
	{
		if (!(module is MemberTracker) && !(module is PythonType) && !(module is ReflectedEvent) && !(module is ReflectedField))
		{
			return module is BuiltinFunction;
		}
		return true;
	}

	private static string CreateFullName(string baseName, string name)
	{
		if (baseName == null || baseName.Length == 0 || baseName == "__main__")
		{
			return name;
		}
		return baseName + "." + name;
	}

	private static object ImportFromPath(CodeContext context, string name, string fullName, List path)
	{
		return ImportFromPathHook(context, name, fullName, path, LoadFromDisk);
	}

	private static object ImportFromPathHook(CodeContext context, string name, string fullName, List path, Func<CodeContext, string, string, string, object> defaultLoader)
	{
		if (!(PythonContext.GetContext(context).GetSystemStateValue("path_importer_cache") is IDictionary<object, object> dictionary))
		{
			return null;
		}
		foreach (object item in (IEnumerable)path)
		{
			string result = item as string;
			if (result == null && (!Converter.TryConvertToString(item, out result) || result == null))
			{
				continue;
			}
			if (!dictionary.TryGetValue(result, out var value))
			{
				value = (dictionary[result] = FindImporterForPath(context, result));
			}
			if (value != null)
			{
				if (FindAndLoadModuleFromImporter(context, value, fullName, null, out var ret))
				{
					return ret;
				}
			}
			else if (defaultLoader != null)
			{
				object obj2 = defaultLoader(context, name, fullName, result);
				if (obj2 != null)
				{
					return obj2;
				}
			}
		}
		return null;
	}

	private static object LoadFromDisk(CodeContext context, string name, string fullName, string str)
	{
		string text = context.LanguageContext.DomainManager.Platform.CombinePaths(str, name);
		PythonModule pythonModule = LoadPackageFromSource(context, fullName, text);
		if (pythonModule != null)
		{
			return pythonModule;
		}
		string path = text + ".py";
		pythonModule = LoadModuleFromSource(context, fullName, path);
		if (pythonModule != null)
		{
			return pythonModule;
		}
		return null;
	}

	private static object FindImporterForPath(CodeContext context, string dirname)
	{
		List list = PythonContext.GetContext(context).GetSystemStateValue("path_hooks") as List;
		foreach (object item in (IEnumerable)list)
		{
			try
			{
				object obj = PythonCalls.Call(context, item, dirname);
				if (obj != null)
				{
					return obj;
				}
			}
			catch (ImportException)
			{
			}
		}
		if (!context.LanguageContext.DomainManager.Platform.DirectoryExists(dirname))
		{
			return new PythonImport.NullImporter(dirname);
		}
		return null;
	}

	private static PythonModule LoadModuleFromSource(CodeContext context, string name, string path)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		string fullPathAndValidateCase = GetFullPathAndValidateCase(context2, path, isDir: false);
		if (fullPathAndValidateCase == null || !context2.DomainManager.Platform.FileExists(fullPathAndValidateCase))
		{
			return null;
		}
		SourceUnit sourceUnit = context2.CreateFileUnit(fullPathAndValidateCase, context2.DefaultEncoding, SourceCodeKind.File);
		return LoadFromSourceUnit(context, sourceUnit, name, sourceUnit.Path);
	}

	private static string GetFullPathAndValidateCase(LanguageContext context, string path, bool isDir)
	{
		PlatformAdaptationLayer platform = context.DomainManager.Platform;
		string directoryName = Path.GetDirectoryName(path);
		if (!platform.DirectoryExists(directoryName))
		{
			return null;
		}
		try
		{
			string fileName = Path.GetFileName(path);
			string[] fileSystemEntries = platform.GetFileSystemEntries(directoryName, fileName, !isDir, isDir);
			if (fileSystemEntries.Length != 1 || Path.GetFileName(fileSystemEntries[0]) != fileName)
			{
				return null;
			}
			return Path.GetFullPath(fileSystemEntries[0]);
		}
		catch (IOException)
		{
			return null;
		}
	}

	internal static PythonModule LoadPackageFromSource(CodeContext context, string name, string path)
	{
		path = GetFullPathAndValidateCase(PythonContext.GetContext(context), path, isDir: true);
		if (path == null)
		{
			return null;
		}
		return LoadModuleFromSource(context, name, context.LanguageContext.DomainManager.Platform.CombinePaths(path, "__init__.py"));
	}

	private static PythonModule LoadFromSourceUnit(CodeContext context, SourceUnit sourceCode, string name, string path)
	{
		return PythonContext.GetContext(context).CompileModule(path, name, sourceCode, ModuleOptions.Optimized | ModuleOptions.Initialize);
	}
}
