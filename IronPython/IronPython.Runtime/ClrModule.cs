using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using IronPython.Compiler;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.ComInterop;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public static class ClrModule
{
	public sealed class ReferencesList : List<Assembly>, ICodeFormattable
	{
		public new void Add(Assembly other)
		{
			base.Add(other);
		}

		[SpecialName]
		public ReferencesList Add(object other)
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(other);
			while (enumerator.MoveNext())
			{
				Assembly assembly = enumerator.Current as Assembly;
				if (assembly == null)
				{
					throw PythonOps.TypeError("non-assembly added to references list");
				}
				base.Add(assembly);
			}
			return this;
		}

		public string __repr__(CodeContext context)
		{
			StringBuilder stringBuilder = new StringBuilder("(");
			string value = "";
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Assembly current = enumerator.Current;
					stringBuilder.Append(value);
					stringBuilder.Append('<');
					stringBuilder.Append(current.FullName);
					stringBuilder.Append('>');
					value = "," + Environment.NewLine;
				}
			}
			stringBuilder.AppendLine(")");
			return stringBuilder.ToString();
		}
	}

	public class ArgChecker
	{
		private object[] expected;

		public ArgChecker(object[] prms)
		{
			expected = prms;
		}

		[SpecialName]
		public object Call(CodeContext context, object func)
		{
			return new RuntimeArgChecker(func, expected);
		}
	}

	public class RuntimeArgChecker : PythonTypeSlot
	{
		private object[] _expected;

		private object _func;

		private object _inst;

		internal override bool GetAlwaysSucceeds => true;

		public RuntimeArgChecker(object function, object[] expectedArgs)
		{
			_expected = expectedArgs;
			_func = function;
		}

		public RuntimeArgChecker(object instance, object function, object[] expectedArgs)
			: this(function, expectedArgs)
		{
			_inst = instance;
		}

		private void ValidateArgs(object[] args)
		{
			int num = 0;
			if (_inst != null)
			{
				num = 1;
			}
			for (int i = num; i < args.Length + num; i++)
			{
				PythonType pythonType = DynamicHelpers.GetPythonType(args[i - num]);
				PythonType pythonType2 = _expected[i] as PythonType;
				if (pythonType2 == null)
				{
					pythonType2 = ((OldClass)_expected[i]).TypeObject;
				}
				if (pythonType != _expected[i] && !pythonType.IsSubclassOf(pythonType2))
				{
					throw PythonOps.AssertionError("argument {0} has bad value (got {1}, expected {2})", i, pythonType, _expected[i]);
				}
			}
		}

		[SpecialName]
		public object Call(CodeContext context, params object[] args)
		{
			ValidateArgs(args);
			if (_inst != null)
			{
				return PythonOps.CallWithContext(context, _func, ArrayUtils.Insert(_inst, args));
			}
			return PythonOps.CallWithContext(context, _func, args);
		}

		internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
		{
			value = new RuntimeArgChecker(instance, _func, _expected);
			return true;
		}

		[SpecialName]
		public object Call(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
		{
			ValidateArgs(args);
			if (_inst != null)
			{
				return PythonCalls.CallWithKeywordArgs(context, _func, ArrayUtils.Insert(_inst, args), dict);
			}
			return PythonCalls.CallWithKeywordArgs(context, _func, args, dict);
		}
	}

	public class ReturnChecker
	{
		public object retType;

		public ReturnChecker(object returnType)
		{
			retType = returnType;
		}

		[SpecialName]
		public object Call(CodeContext context, object func)
		{
			return new RuntimeReturnChecker(func, retType);
		}
	}

	public class RuntimeReturnChecker : PythonTypeSlot
	{
		private object _retType;

		private object _func;

		private object _inst;

		internal override bool GetAlwaysSucceeds => true;

		public RuntimeReturnChecker(object function, object expectedReturn)
		{
			_retType = expectedReturn;
			_func = function;
		}

		public RuntimeReturnChecker(object instance, object function, object expectedReturn)
			: this(function, expectedReturn)
		{
			_inst = instance;
		}

		private void ValidateReturn(object ret)
		{
			if (ret == null && _retType == null)
			{
				return;
			}
			PythonType pythonType = DynamicHelpers.GetPythonType(ret);
			if (pythonType != _retType)
			{
				PythonType pythonType2 = _retType as PythonType;
				if (pythonType2 == null)
				{
					pythonType2 = ((OldClass)_retType).TypeObject;
				}
				if (!pythonType.IsSubclassOf(pythonType2))
				{
					throw PythonOps.AssertionError("bad return value returned (expected {0}, got {1})", _retType, pythonType);
				}
			}
		}

		[SpecialName]
		public object Call(CodeContext context, params object[] args)
		{
			object obj = ((_inst == null) ? PythonOps.CallWithContext(context, _func, args) : PythonOps.CallWithContext(context, _func, ArrayUtils.Insert(_inst, args)));
			ValidateReturn(obj);
			return obj;
		}

		public object GetAttribute(object instance, object owner)
		{
			return new RuntimeReturnChecker(instance, _func, _retType);
		}

		internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
		{
			value = GetAttribute(instance, owner);
			return true;
		}

		[SpecialName]
		public object Call(CodeContext context, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
		{
			if (_inst != null)
			{
				object obj = PythonCalls.CallWithKeywordArgs(context, _func, ArrayUtils.Insert(_inst, args), dict);
				ValidateReturn(obj);
				return obj;
			}
			return PythonCalls.CallWithKeywordArgs(context, _func, args, dict);
		}
	}

	[Serializable]
	internal sealed class FileStreamContentProvider : StreamContentProvider
	{
		[Serializable]
		private class PALHolder : MarshalByRefObject
		{
			[NonSerialized]
			private readonly PlatformAdaptationLayer _pal;

			internal PALHolder(PlatformAdaptationLayer pal)
			{
				_pal = pal;
			}

			internal Stream GetStream(string path)
			{
				return _pal.OpenInputFileStream(path);
			}
		}

		private readonly string _path;

		private readonly PALHolder _pal;

		internal string Path => _path;

		internal FileStreamContentProvider(PlatformAdaptationLayer manager, string path)
		{
			ContractUtils.RequiresNotNull(path, "path");
			_path = path;
			_pal = new PALHolder(manager);
		}

		public override Stream GetStream()
		{
			return _pal.GetStream(Path);
		}
	}

	private static PythonType _strongBoxType;

	public static PythonType Reference => StrongBox;

	public static PythonType StrongBox
	{
		get
		{
			if (_strongBoxType == null)
			{
				_strongBoxType = DynamicHelpers.GetPythonTypeFromType(typeof(StrongBox<>));
			}
			return _strongBoxType;
		}
	}

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		if (!dict.ContainsKey("References"))
		{
			dict["References"] = context.ReferencedAssemblies;
		}
	}

	public static ScriptDomainManager GetCurrentRuntime(CodeContext context)
	{
		return context.LanguageContext.DomainManager;
	}

	[Documentation("Adds a reference to a .NET assembly.  Parameters can be an already loaded\r\nAssembly object, a full assembly name, or a partial assembly name. After the\r\nload the assemblies namespaces and top-level types will be available via \r\nimport Namespace.")]
	public static void AddReference(CodeContext context, params object[] references)
	{
		if (references == null)
		{
			throw new TypeErrorException("Expected string or Assembly, got NoneType");
		}
		if (references.Length == 0)
		{
			throw new ValueErrorException("Expected at least one name, got none");
		}
		ContractUtils.RequiresNotNull(context, "context");
		foreach (object reference in references)
		{
			AddReference(context, reference);
		}
	}

	[Documentation("Adds a reference to a .NET assembly.  One or more assembly names can\r\nbe provided.  The assembly is searched for in the directories specified in \r\nsys.path and dependencies will be loaded from sys.path as well.  The assembly \r\nname should be the filename on disk without a directory specifier and \r\noptionally including the .EXE or .DLL extension. After the load the assemblies \r\nnamespaces and top-level types will be available via import Namespace.")]
	public static void AddReferenceToFile(CodeContext context, params string[] files)
	{
		if (files == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		if (files.Length == 0)
		{
			throw new ValueErrorException("Expected at least one name, got none");
		}
		ContractUtils.RequiresNotNull(context, "context");
		foreach (string file in files)
		{
			AddReferenceToFile(context, file);
		}
	}

	[Documentation("Adds a reference to a .NET assembly.  Parameters are an assembly name. \r\nAfter the load the assemblies namespaces and top-level types will be available via \r\nimport Namespace.")]
	public static void AddReferenceByName(CodeContext context, params string[] names)
	{
		if (names == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		if (names.Length == 0)
		{
			throw new ValueErrorException("Expected at least one name, got none");
		}
		ContractUtils.RequiresNotNull(context, "context");
		foreach (string name in names)
		{
			AddReferenceByName(context, name);
		}
	}

	[Documentation("Adds a reference to a .NET assembly.  Parameters are a partial assembly name. \r\nAfter the load the assemblies namespaces and top-level types will be available via \r\nimport Namespace.")]
	public static void AddReferenceByPartialName(CodeContext context, params string[] names)
	{
		if (names == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		if (names.Length == 0)
		{
			throw new ValueErrorException("Expected at least one name, got none");
		}
		ContractUtils.RequiresNotNull(context, "context");
		foreach (string name in names)
		{
			AddReferenceByPartialName(context, name);
		}
	}

	[Documentation("Adds a reference to a .NET assembly.  Parameters are a full path to an. \r\nassembly on disk. After the load the assemblies namespaces and top-level types \r\nwill be available via import Namespace.")]
	public static Assembly LoadAssemblyFromFileWithPath(CodeContext context, string file)
	{
		if (file == null)
		{
			throw new TypeErrorException("LoadAssemblyFromFileWithPath: arg 1 must be a string.");
		}
		if (!context.LanguageContext.TryLoadAssemblyFromFileWithPath(file, out var res))
		{
			if (!Path.IsPathRooted(file))
			{
				throw new ValueErrorException("LoadAssemblyFromFileWithPath: path must be rooted");
			}
			if (!File.Exists(file))
			{
				throw new ValueErrorException("LoadAssemblyFromFileWithPath: file not found");
			}
			throw new ValueErrorException("LoadAssemblyFromFileWithPath: error loading assembly");
		}
		return res;
	}

	[Documentation("Loads an assembly from the specified filename and returns the assembly\r\nobject.  Namespaces or types in the assembly can be accessed directly from \r\nthe assembly object.")]
	public static Assembly LoadAssemblyFromFile(CodeContext context, string file)
	{
		if (file == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		if (file.Length == 0)
		{
			throw new ValueErrorException("assembly name must not be empty string");
		}
		ContractUtils.RequiresNotNull(context, "context");
		if (file.IndexOf(Path.DirectorySeparatorChar) != -1)
		{
			throw new ValueErrorException("filenames must not contain full paths, first add the path to sys.path");
		}
		return context.LanguageContext.LoadAssemblyFromFile(file);
	}

	[Documentation("Loads an assembly from the specified partial assembly name and returns the \r\nassembly object.  Namespaces or types in the assembly can be accessed directly \r\nfrom the assembly object.")]
	public static Assembly LoadAssemblyByPartialName(string name)
	{
		if (name == null)
		{
			throw new TypeErrorException("LoadAssemblyByPartialName: arg 1 must be a string");
		}
		return Assembly.LoadWithPartialName(name);
	}

	[Documentation("Loads an assembly from the specified assembly name and returns the assembly\r\nobject.  Namespaces or types in the assembly can be accessed directly from \r\nthe assembly object.")]
	public static Assembly LoadAssemblyByName(CodeContext context, string name)
	{
		if (name == null)
		{
			throw new TypeErrorException("LoadAssemblyByName: arg 1 must be a string");
		}
		return PythonContext.GetContext(context).DomainManager.Platform.LoadAssembly(name);
	}

	public static object Use(CodeContext context, string name)
	{
		ContractUtils.RequiresNotNull(context, "context");
		if (name == null)
		{
			throw new TypeErrorException("Use: arg 1 must be a string");
		}
		PythonModule pythonModule = Importer.TryImportSourceFile(PythonContext.GetContext(context), name);
		if (pythonModule == null)
		{
			throw new ValueErrorException($"couldn't find module {name} to use");
		}
		return pythonModule;
	}

	public static object Use(CodeContext context, string path, string language)
	{
		ContractUtils.RequiresNotNull(context, "context");
		if (path == null)
		{
			throw new TypeErrorException("Use: arg 1 must be a string");
		}
		if (language == null)
		{
			throw new TypeErrorException("Use: arg 2 must be a string");
		}
		ScriptDomainManager domainManager = context.LanguageContext.DomainManager;
		if (!domainManager.Platform.FileExists(path))
		{
			throw new ValueErrorException($"couldn't load module at path '{path}' in language '{language}'");
		}
		SourceUnit sourceUnit = domainManager.GetLanguageByName(language).CreateFileUnit(path);
		return Importer.ExecuteSourceUnit(context.LanguageContext, sourceUnit);
	}

	public static Action<Action> SetCommandDispatcher(CodeContext context, Action<Action> dispatcher)
	{
		ContractUtils.RequiresNotNull(context, "context");
		return context.LanguageContext.GetSetCommandDispatcher(dispatcher);
	}

	public static void ImportExtensions(CodeContext context, PythonType type)
	{
		if (type == null)
		{
			throw PythonOps.TypeError("type must not be None");
		}
		if (!type.IsSystemType)
		{
			throw PythonOps.ValueError("type must be .NET type");
		}
		lock (context.ModuleContext)
		{
			context.ModuleContext.ExtensionMethods = ExtensionMethodSet.AddType(context.LanguageContext, context.ModuleContext.ExtensionMethods, type);
		}
	}

	public static void ImportExtensions(CodeContext context, [NotNull] NamespaceTracker @namespace)
	{
		lock (context.ModuleContext)
		{
			context.ModuleContext.ExtensionMethods = ExtensionMethodSet.AddNamespace(context.LanguageContext, context.ModuleContext.ExtensionMethods, @namespace);
		}
	}

	public static ComTypeLibInfo LoadTypeLibrary(CodeContext context, object rcw)
	{
		return ComTypeLibDesc.CreateFromObject(rcw);
	}

	public static ComTypeLibInfo LoadTypeLibrary(CodeContext context, Guid typeLibGuid)
	{
		return ComTypeLibDesc.CreateFromGuid(typeLibGuid);
	}

	public static void AddReferenceToTypeLibrary(CodeContext context, object rcw)
	{
		ComTypeLibInfo comTypeLibInfo = ComTypeLibDesc.CreateFromObject(rcw);
		PublishTypeLibDesc(context, comTypeLibInfo.TypeLibDesc);
	}

	public static void AddReferenceToTypeLibrary(CodeContext context, Guid typeLibGuid)
	{
		ComTypeLibInfo comTypeLibInfo = ComTypeLibDesc.CreateFromGuid(typeLibGuid);
		PublishTypeLibDesc(context, comTypeLibInfo.TypeLibDesc);
	}

	private static void PublishTypeLibDesc(CodeContext context, ComTypeLibDesc typeLibDesc)
	{
		PythonOps.ScopeSetMember(context, context.LanguageContext.DomainManager.Globals, typeLibDesc.Name, typeLibDesc);
	}

	private static void AddReference(CodeContext context, object reference)
	{
		Assembly assembly = reference as Assembly;
		if (assembly != null)
		{
			AddReference(context, assembly);
			return;
		}
		if (reference is string name)
		{
			AddReference(context, name);
			return;
		}
		throw new TypeErrorException($"Invalid assembly type. Expected string or Assembly, got {reference}.");
	}

	private static void AddReference(CodeContext context, Assembly assembly)
	{
		context.LanguageContext.DomainManager.LoadAssembly(assembly);
	}

	private static void AddReference(CodeContext context, string name)
	{
		if (name == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		Assembly assembly = null;
		try
		{
			assembly = LoadAssemblyByName(context, name);
		}
		catch
		{
		}
		if (assembly == null)
		{
			assembly = LoadAssemblyByPartialName(name);
		}
		if (assembly == null)
		{
			throw new IOException($"Could not add reference to assembly {name}");
		}
		AddReference(context, assembly);
	}

	private static void AddReferenceToFile(CodeContext context, string file)
	{
		if (file == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		Assembly assembly = LoadAssemblyFromFile(context, file);
		if (assembly == null)
		{
			throw new IOException($"Could not add reference to assembly {file}");
		}
		AddReference(context, assembly);
	}

	private static void AddReferenceByPartialName(CodeContext context, string name)
	{
		if (name == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		ContractUtils.RequiresNotNull(context, "context");
		Assembly assembly = LoadAssemblyByPartialName(name);
		if (assembly == null)
		{
			throw new IOException($"Could not add reference to assembly {name}");
		}
		AddReference(context, assembly);
	}

	private static void AddReferenceByName(CodeContext context, string name)
	{
		if (name == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		Assembly assembly = LoadAssemblyByName(context, name);
		if (assembly == null)
		{
			throw new IOException($"Could not add reference to assembly {name}");
		}
		AddReference(context, assembly);
	}

	[Documentation("Adds a reference to a .NET assembly.  One or more assembly names can\r\nbe provided which are fully qualified names to the file on disk.  The \r\ndirectory is added to sys.path and AddReferenceToFile is then called. After the \r\nload the assemblies namespaces and top-level types will be available via \r\nimport Namespace.")]
	public static void AddReferenceToFileAndPath(CodeContext context, params string[] files)
	{
		if (files == null)
		{
			throw new TypeErrorException("Expected string, got NoneType");
		}
		ContractUtils.RequiresNotNull(context, "context");
		foreach (string file in files)
		{
			AddReferenceToFileAndPath(context, file);
		}
	}

	private static void AddReferenceToFileAndPath(CodeContext context, string file)
	{
		if (file == null)
		{
			throw PythonOps.TypeError("Expected string, got NoneType");
		}
		string directoryName = Path.GetDirectoryName(Path.GetFullPath(file));
		PythonContext context2 = PythonContext.GetContext(context);
		if (!context2.TryGetSystemPath(out var path))
		{
			throw PythonOps.TypeError("cannot update path, it is not a list");
		}
		path.append(directoryName);
		Assembly assembly = context2.LoadAssemblyFromFile(file);
		if (assembly == null)
		{
			throw PythonOps.IOError("file does not exist: {0}", file);
		}
		AddReference(context, assembly);
	}

	public static Type GetClrType(Type type)
	{
		return type;
	}

	public static PythonType GetPythonType(Type t)
	{
		return DynamicHelpers.GetPythonTypeFromType(t);
	}

	[Obsolete("Call clr.GetPythonType instead")]
	public static PythonType GetDynamicType(Type t)
	{
		return DynamicHelpers.GetPythonTypeFromType(t);
	}

	public static object accepts(params object[] types)
	{
		return new ArgChecker(types);
	}

	public static object returns(object type)
	{
		return new ReturnChecker(type);
	}

	public static object Self()
	{
		return null;
	}

	public static List Dir(object o)
	{
		IList<object> attrNames = PythonOps.GetAttrNames(DefaultContext.Default, o);
		List list = new List(attrNames);
		list.sort(DefaultContext.Default);
		return list;
	}

	public static List DirClr(object o)
	{
		IList<object> attrNames = PythonOps.GetAttrNames(DefaultContext.DefaultCLS, o);
		List list = new List(attrNames);
		list.sort(DefaultContext.DefaultCLS);
		return list;
	}

	public static object Convert(CodeContext context, object o, Type toType)
	{
		return Converter.Convert(o, toType);
	}

	public static void CompileModules(CodeContext context, string assemblyName, [ParamDictionary] IDictionary<string, object> kwArgs, params string[] filenames)
	{
		ContractUtils.RequiresNotNull(assemblyName, "assemblyName");
		ContractUtils.RequiresNotNullItems(filenames, "filenames");
		PythonContext context2 = PythonContext.GetContext(context);
		for (int i = 0; i < filenames.Length; i++)
		{
			filenames[i] = Path.GetFullPath(filenames[i]);
		}
		Dictionary<string, string> dictionary = BuildPackageMap(filenames);
		List<SavableScriptCode> list = new List<SavableScriptCode>();
		foreach (string text in filenames)
		{
			if (!context2.DomainManager.Platform.FileExists(text))
			{
				throw PythonOps.IOError("Couldn't find file for compilation: {0}", text);
			}
			string directoryName = Path.GetDirectoryName(text);
			string text2 = "";
			string text3;
			if (Path.GetFileName(text) == "__init__.py")
			{
				directoryName = Path.GetDirectoryName(directoryName);
				text3 = ((!string.IsNullOrEmpty(directoryName)) ? Path.GetFileNameWithoutExtension(Path.GetDirectoryName(text)) : Path.GetDirectoryName(text));
				text2 = Path.DirectorySeparatorChar + "__init__.py";
			}
			else
			{
				text3 = Path.GetFileNameWithoutExtension(text);
			}
			if (dictionary.TryGetValue(directoryName, out var value))
			{
				text3 = value + "." + text3;
			}
			text2 = text3.Replace('.', Path.DirectorySeparatorChar) + text2;
			SourceUnit sourceCode = context2.CreateSourceUnit(new FileStreamContentProvider(context.LanguageContext.DomainManager.Platform, text), text2, context2.DefaultEncoding, SourceCodeKind.File);
			ScriptCode scriptCode = PythonContext.GetContext(context).GetScriptCode(sourceCode, text3, ModuleOptions.Initialize, CompilationMode.ToDisk);
			list.Add((SavableScriptCode)scriptCode);
		}
		if (kwArgs != null && kwArgs.TryGetValue("mainModule", out var value2) && value2 is string text4)
		{
			if (!context2.DomainManager.Platform.FileExists(text4))
			{
				throw PythonOps.IOError("Couldn't find main file for compilation: {0}", text4);
			}
			SourceUnit sourceCode2 = context2.CreateFileUnit(text4, context2.DefaultEncoding, SourceCodeKind.File);
			list.Add((SavableScriptCode)PythonContext.GetContext(context).GetScriptCode(sourceCode2, "__main__", ModuleOptions.Initialize, CompilationMode.ToDisk));
		}
		SavableScriptCode.SaveToAssembly(assemblyName, list.ToArray());
	}

	public static void CompileSubclassTypes(string assemblyName, params object[] newTypes)
	{
		if (assemblyName == null)
		{
			throw PythonOps.TypeError("CompileTypes expected str for assemblyName, got NoneType");
		}
		List<PythonTuple> list = new List<PythonTuple>();
		foreach (object obj in newTypes)
		{
			if (obj is PythonType)
			{
				list.Add(PythonTuple.MakeTuple(obj));
			}
			else
			{
				list.Add(PythonTuple.Make(obj));
			}
		}
		NewTypeMaker.SaveNewTypes(assemblyName, list);
	}

	public static PythonTuple GetSubclassedTypes()
	{
		List<object> list = new List<object>();
		foreach (NewTypeInfo key in NewTypeMaker._newTypes.Keys)
		{
			Type type = key.BaseType;
			Type type2 = type;
			while (type2 != null)
			{
				if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(Extensible<>))
				{
					type = type2.GetGenericArguments()[0];
					break;
				}
				type2 = type2.BaseType;
			}
			PythonType pythonTypeFromType = DynamicHelpers.GetPythonTypeFromType(type);
			if (key.InterfaceTypes.Count == 0)
			{
				list.Add(pythonTypeFromType);
			}
			else if (key.InterfaceTypes.Count > 0)
			{
				PythonType[] array = new PythonType[key.InterfaceTypes.Count + 1];
				array[0] = pythonTypeFromType;
				for (int i = 0; i < key.InterfaceTypes.Count; i++)
				{
					array[i + 1] = DynamicHelpers.GetPythonTypeFromType(key.InterfaceTypes[i]);
				}
				list.Add(PythonTuple.MakeTuple(array));
			}
		}
		return PythonTuple.MakeTuple(list.ToArray());
	}

	private static Dictionary<string, string> BuildPackageMap(string[] filenames)
	{
		List<string> list = new List<string>();
		foreach (string text in filenames)
		{
			if (text.EndsWith("__init__.py"))
			{
				list.Add(text);
			}
		}
		SortModules(list);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string item in list)
		{
			string directoryName = Path.GetDirectoryName(item);
			string value = string.Empty;
			string text2 = Path.GetFileName(Path.GetDirectoryName(item));
			if (dictionary.TryGetValue(Path.GetDirectoryName(directoryName), out value))
			{
				text2 = value + "." + text2;
			}
			dictionary[Path.GetDirectoryName(item)] = text2;
		}
		return dictionary;
	}

	private static void SortModules(List<string> modules)
	{
		modules.Sort((string x, string y) => x.Length - y.Length);
	}

	public static PythonTuple GetProfilerData(CodeContext context, [DefaultParameterValue(false)] bool includeUnused)
	{
		return new PythonTuple(Profiler.GetProfiler(PythonContext.GetContext(context)).GetProfile(includeUnused));
	}

	public static void ClearProfilerData(CodeContext context)
	{
		Profiler.GetProfiler(PythonContext.GetContext(context)).Reset();
	}

	public static void EnableProfiler(CodeContext context, bool enable)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		PythonOptions pythonOptions = context2.Options as PythonOptions;
		pythonOptions.EnableProfiler = enable;
	}

	public static PythonTuple Serialize(object self)
	{
		if (self == null)
		{
			return PythonTuple.MakeTuple(null, string.Empty);
		}
		string text;
		string text2;
		switch (CompilerHelpers.GetType(self).GetTypeCode())
		{
		case TypeCode.DBNull:
		case TypeCode.Char:
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
		case TypeCode.Single:
		case TypeCode.Decimal:
			text = self.ToString();
			text2 = CompilerHelpers.GetType(self).FullName;
			break;
		default:
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, self);
			text = memoryStream.ToArray().MakeString();
			text2 = null;
			break;
		}
		}
		return PythonTuple.MakeTuple(text2, text);
	}

	public static object Deserialize(string serializationFormat, [NotNull] string data)
	{
		if (serializationFormat != null)
		{
			return serializationFormat switch
			{
				"System.Byte" => byte.Parse(data), 
				"System.Char" => char.Parse(data), 
				"System.DBNull" => DBNull.Value, 
				"System.Decimal" => decimal.Parse(data), 
				"System.Int16" => short.Parse(data), 
				"System.Int64" => long.Parse(data), 
				"System.SByte" => sbyte.Parse(data), 
				"System.Single" => float.Parse(data), 
				"System.UInt16" => ushort.Parse(data), 
				"System.UInt32" => uint.Parse(data), 
				"System.UInt64" => ulong.Parse(data), 
				_ => throw PythonOps.ValueError("unknown serialization format: {0}", serializationFormat), 
			};
		}
		if (string.IsNullOrEmpty(data))
		{
			return null;
		}
		MemoryStream serializationStream = new MemoryStream(data.MakeByteArray());
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		return binaryFormatter.Deserialize(serializationStream);
	}
}
