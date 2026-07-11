using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using IronPython.Zlib;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public static class ZipImportModule
{
	[PythonType]
	public class zipimporter
	{
		[Flags]
		private enum ModuleCodeType
		{
			Source = 0,
			ByteCode = 1,
			Package = 2
		}

		private enum ModuleStatus
		{
			Error,
			NotFound,
			Module,
			Package
		}

		private const int MAXPATHLEN = 256;

		private string _archive;

		private string _prefix;

		private PythonDictionary __files;

		private static readonly Dictionary<string, ModuleCodeType> _search_order;

		public PythonDictionary _files
		{
			get
			{
				return __files;
			}
			private set
			{
				__files = value;
			}
		}

		public string archive => _archive;

		public string prefix => _prefix;

		static zipimporter()
		{
			_search_order = new Dictionary<string, ModuleCodeType>
			{
				{
					Path.DirectorySeparatorChar + "__init__.py",
					ModuleCodeType.Package
				},
				{
					".py",
					ModuleCodeType.Source
				}
			};
		}

		public zipimporter(CodeContext context, object pathObj, [ParamDictionary] IDictionary<object, object> kwArgs)
		{
			PlatformAdaptationLayer platform = context.LanguageContext.DomainManager.Platform;
			if (pathObj == null)
			{
				throw PythonOps.TypeError("must be string, not None");
			}
			if (!(pathObj is string))
			{
				throw PythonOps.TypeError("must be string, not {0}", pathObj.GetType());
			}
			if (kwArgs.Count > 0)
			{
				throw PythonOps.TypeError("zipimporter() does not take keyword arguments");
			}
			string text = pathObj as string;
			string text2 = text;
			if (text.Length == 0)
			{
				throw MakeError("archive path is empty");
			}
			if (text.Length > 256)
			{
				throw MakeError("archive path too long");
			}
			string text3 = text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			text = string.Empty;
			_ = string.Empty;
			while (!string.IsNullOrEmpty(text3))
			{
				if (platform.FileExists(text3))
				{
					text = text3;
					break;
				}
				text3 = Path.GetDirectoryName(text3);
			}
			if (!string.IsNullOrEmpty(text))
			{
				PythonDictionary pythonDictionary = context.LanguageContext.GetModuleState(_zip_directory_cache_key) as PythonDictionary;
				if (pythonDictionary != null && pythonDictionary.ContainsKey(text))
				{
					_files = pythonDictionary[text] as PythonDictionary;
				}
				else
				{
					_files = ReadDirectory(text);
					pythonDictionary.Add(text, _files);
				}
				_prefix = text2.Replace(text, string.Empty);
				if (!string.IsNullOrEmpty(_prefix) && !_prefix.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					_prefix = _prefix.Substring(1);
					_prefix += Path.DirectorySeparatorChar;
				}
				_archive = text;
				return;
			}
			throw MakeError("not a Zip file");
		}

		public string __repr__()
		{
			string arg = (string.IsNullOrEmpty(_archive) ? "???" : _archive);
			string text = (string.IsNullOrEmpty(_prefix) ? string.Empty : _prefix);
			string empty = string.Empty;
			if (!string.IsNullOrEmpty(text))
			{
				return $"<zipimporter object \"{arg}{Path.DirectorySeparatorChar}{text}\">";
			}
			return $"<zipimporter object \"{arg}\">";
		}

		[Documentation("find_module(fullname, path=None) -> self or None.\r\n\r\nSearch for a module specified by 'fullname'. 'fullname' must be the\r\nfully qualified (dotted) module name. It returns the zipimporter\r\ninstance itself if the module was found, or None if it wasn't.\r\nThe optional 'path' argument is ignored -- it's there for compatibility\r\nwith the importer protocol.")]
		public object find_module(CodeContext context, string fullname, params object[] args)
		{
			ModuleStatus moduleInfo = GetModuleInfo(context, fullname);
			if (moduleInfo == ModuleStatus.Error || moduleInfo == ModuleStatus.NotFound)
			{
				return null;
			}
			return this;
		}

		[Documentation("load_module(fullname) -> module.\r\n\r\nLoad the module specified by 'fullname'. 'fullname' must be the\r\nfully qualified (dotted) module name. It returns the imported\r\nmodule, or raises ZipImportError if it wasn't found.")]
		public object load_module(CodeContext context, string fullname)
		{
			PythonContext context2 = PythonContext.GetContext(context);
			ScriptCode scriptCode = null;
			bool ispackage;
			string modpath;
			string moduleCode = GetModuleCode(context, fullname, out ispackage, out modpath);
			if (moduleCode == null)
			{
				return null;
			}
			PythonModule pythonModule = context2.CompileModule(modpath, fullname, new SourceUnit(context2, new SourceStringContentProvider(moduleCode), modpath, SourceCodeKind.File), ModuleOptions.None, out scriptCode);
			PythonDictionary _dict__ = pythonModule.__dict__;
			_dict__.Add("__name__", fullname);
			_dict__.Add("__loader__", this);
			_dict__.Add("__package__", null);
			if (ispackage)
			{
				string subName = GetSubName(fullname);
				string text = $"{_archive}{Path.DirectorySeparatorChar}{((_prefix.Length > 0) ? _prefix : string.Empty)}{subName}";
				List value = PythonOps.MakeList(text);
				_dict__.Add("__path__", value);
			}
			scriptCode.Run(pythonModule.Scope);
			return pythonModule;
		}

		[Documentation("get_filename(fullname) -> filename string.\r\n\r\nReturn the filename for the specified module.")]
		public string get_filename(CodeContext context, string fullname)
		{
			bool ispackage;
			string modpath;
			string moduleCode = GetModuleCode(context, fullname, out ispackage, out modpath);
			if (moduleCode == null)
			{
				return null;
			}
			return modpath;
		}

		[Documentation("is_package(fullname) -> bool.\r\n\r\nReturn True if the module specified by fullname is a package.\r\nRaise ZipImportError if the module couldn't be found.")]
		public bool is_package(CodeContext context, string fullname)
		{
			ModuleStatus moduleInfo = GetModuleInfo(context, fullname);
			if (moduleInfo == ModuleStatus.NotFound)
			{
				throw MakeError("can't find module '{0}'", fullname);
			}
			return moduleInfo == ModuleStatus.Package;
		}

		[Documentation("get_data(pathname) -> string with file data.\r\n\r\nReturn the data associated with 'pathname'. Raise IOError if\r\nthe file wasn't found.")]
		public string get_data(CodeContext context, string path)
		{
			if (path.Length >= 256)
			{
				throw MakeError("path too long");
			}
			path = path.Replace(_archive, string.Empty).TrimStart(Path.DirectorySeparatorChar);
			if (!__files.ContainsKey(path))
			{
				throw PythonOps.IOError(path);
			}
			byte[] data = GetData(_archive, __files[path] as PythonTuple);
			return PythonAsciiEncoding.Instance.GetString(data, 0, data.Length);
		}

		[Documentation("get_code(fullname) -> code object.\r\n\r\nReturn the code object for the specified module. Raise ZipImportError\r\nif the module couldn't be found.")]
		public string get_code(CodeContext context, string fullname)
		{
			return string.Empty;
		}

		[Documentation("get_source(fullname) -> source string.\r\n\r\nReturn the source code for the specified module. Raise ZipImportError\r\nif the module couldn't be found, return None if the archive does\r\ncontain the module, but has no source for it.")]
		public string get_source(CodeContext context, string fullname)
		{
			ModuleStatus moduleInfo = GetModuleInfo(context, fullname);
			string result = null;
			PythonContext context2 = PythonContext.GetContext(context);
			switch (moduleInfo)
			{
			case ModuleStatus.Error:
				return null;
			case ModuleStatus.NotFound:
				throw MakeError("can't find module '{0}'", fullname);
			default:
			{
				string subName = GetSubName(fullname);
				string text = MakeFilename(_prefix, subName);
				if (string.IsNullOrEmpty(text))
				{
					return null;
				}
				text = ((moduleInfo != ModuleStatus.Package) ? (text + ".py") : (text + Path.DirectorySeparatorChar + "__init__.py"));
				if (__files.ContainsKey(text))
				{
					byte[] data = GetData(_archive, __files[text] as PythonTuple);
					result = context2.DefaultEncoding.GetString(data, 0, data.Length);
				}
				return result;
			}
			}
		}

		private string GetModuleCode(CodeContext context, string fullname, out bool ispackage, out string modpath)
		{
			PythonTuple pythonTuple = null;
			string subName = GetSubName(fullname);
			string text = MakeFilename(_prefix, subName);
			string text2 = null;
			ispackage = false;
			modpath = string.Empty;
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			foreach (KeyValuePair<string, ModuleCodeType> item in _search_order)
			{
				string key = text + item.Key;
				if (__files.ContainsKey(key))
				{
					pythonTuple = (PythonTuple)__files[key];
					ispackage = (item.Value & ModuleCodeType.Package) == ModuleCodeType.Package;
					_ = item.Value;
					text2 = GetCodeFromData(context, ispackage, isbytecode: false, 0, pythonTuple);
					if (text2 != null)
					{
						modpath = (string)pythonTuple[0];
						return text2;
					}
				}
			}
			throw MakeError("can't find module '{0}'", fullname);
		}

		private byte[] GetData(string archive, PythonTuple toc_entry)
		{
			int num = (int)toc_entry[4];
			int num2 = (int)toc_entry[2];
			int num3 = (int)toc_entry[1];
			BinaryReader binaryReader = null;
			byte[] result = null;
			try
			{
				try
				{
					binaryReader = new BinaryReader(new FileStream(archive, FileMode.Open, FileAccess.Read));
				}
				catch
				{
					throw PythonOps.IOError("zipimport: can not open file {0}", archive);
				}
				binaryReader.BaseStream.Seek(num, SeekOrigin.Begin);
				int num4 = binaryReader.ReadInt32();
				if (num4 != 67324752)
				{
					throw MakeError("bad local file header in {0}", archive);
				}
				binaryReader.BaseStream.Seek(num + 26, SeekOrigin.Begin);
				num4 = 30 + binaryReader.ReadInt16() + binaryReader.ReadInt16();
				num += num4;
				binaryReader.BaseStream.Seek(num, SeekOrigin.Begin);
				byte[] array;
				try
				{
					array = binaryReader.ReadBytes((num3 == 0) ? num2 : (num2 + 1));
				}
				catch
				{
					throw PythonOps.IOError("zipimport: can't read data");
				}
				if (num3 != 0)
				{
					array[num2] = 90;
					num2++;
				}
				result = ((num3 != 0) ? ZlibModule.Decompress(array, -15) : array);
			}
			catch
			{
				throw;
			}
			finally
			{
				binaryReader?.Close();
			}
			return result;
		}

		private string GetCodeFromData(CodeContext context, bool ispackage, bool isbytecode, int mtime, PythonTuple toc_entry)
		{
			byte[] data = GetData(_archive, toc_entry);
			_ = (string)toc_entry[0];
			string result = null;
			if (data != null && !isbytecode)
			{
				result = context.LanguageContext.DefaultEncoding.GetString(data, 0, data.Length);
			}
			return result;
		}

		private PythonDictionary ReadDirectory(string archive)
		{
			string empty = string.Empty;
			BinaryReader binaryReader = null;
			PythonDictionary pythonDictionary = null;
			byte[] array = new byte[22];
			if (archive.Length > 256)
			{
				throw PythonOps.OverflowError("Zip path name is too long");
			}
			string text = archive;
			try
			{
				try
				{
					binaryReader = new BinaryReader(new FileStream(archive, FileMode.Open, FileAccess.Read));
				}
				catch
				{
					throw MakeError("can't open Zip file: '{0}'", archive);
				}
				if (binaryReader.BaseStream.Length < 2)
				{
					throw MakeError("can't read Zip file: '{0}'", archive);
				}
				binaryReader.BaseStream.Seek(-22L, SeekOrigin.End);
				int num = (int)binaryReader.BaseStream.Position;
				if (binaryReader.Read(array, 0, 22) != 22)
				{
					throw MakeError("can't read Zip file: '{0}'", archive);
				}
				if (BitConverter.ToUInt32(array, 0) != 101010256)
				{
					binaryReader.Close();
					throw MakeError("not a Zip file: '{0}'", archive);
				}
				int num2 = BitConverter.ToInt32(array, 12);
				int num3 = BitConverter.ToInt32(array, 16);
				int num4 = num - num3 - num2;
				num3 += num4;
				pythonDictionary = new PythonDictionary();
				text += Path.DirectorySeparatorChar;
				int num5 = 0;
				while (true)
				{
					empty = string.Empty;
					binaryReader.BaseStream.Seek(num3, SeekOrigin.Begin);
					int num6 = binaryReader.ReadInt32();
					if (num6 != 33639248)
					{
						break;
					}
					binaryReader.BaseStream.Seek(num3 + 10, SeekOrigin.Begin);
					int num7 = binaryReader.ReadInt16();
					int num8 = binaryReader.ReadInt16();
					int num9 = binaryReader.ReadInt16();
					int num10 = binaryReader.ReadInt32();
					int num11 = binaryReader.ReadInt32();
					int num12 = binaryReader.ReadInt32();
					int num13 = binaryReader.ReadInt16();
					num2 = 46 + num13 + binaryReader.ReadInt16() + binaryReader.ReadInt16();
					binaryReader.BaseStream.Seek(num3 + 42, SeekOrigin.Begin);
					int num14 = binaryReader.ReadInt32() + num4;
					if (num13 > 256)
					{
						num13 = 256;
					}
					for (int i = 0; i < num13; i++)
					{
						char c = binaryReader.ReadChar();
						if (c == '/')
						{
							c = Path.DirectorySeparatorChar;
						}
						empty += c;
					}
					num3 += num2;
					PythonTuple value = PythonOps.MakeTuple(text + empty, num7, num11, num12, num14, num8, num9, num10);
					pythonDictionary.Add(empty, value);
					num5++;
				}
				return pythonDictionary;
			}
			catch
			{
				throw;
			}
			finally
			{
				binaryReader?.Close();
			}
		}

		private string GetSubName(string fullname)
		{
			string[] array = fullname.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			return array[array.Length - 1];
		}

		private string MakeFilename(string prefix, string name)
		{
			if (prefix.Length + name.Length + 13 >= 256)
			{
				throw MakeError("path to long");
			}
			return (prefix + name).Replace('.', Path.DirectorySeparatorChar);
		}

		private ModuleStatus GetModuleInfo(CodeContext context, string fullname)
		{
			string subName = GetSubName(fullname);
			string text = MakeFilename(_prefix, subName);
			if (string.IsNullOrEmpty(text))
			{
				return ModuleStatus.Error;
			}
			foreach (KeyValuePair<string, ModuleCodeType> item in _search_order)
			{
				string key = text + item.Key;
				if (_files.ContainsKey(key))
				{
					if ((item.Value & ModuleCodeType.Package) == ModuleCodeType.Package)
					{
						return ModuleStatus.Package;
					}
					return ModuleStatus.Module;
				}
			}
			return ModuleStatus.NotFound;
		}
	}

	[Serializable]
	internal sealed class SourceStringContentProvider : TextContentProvider
	{
		private readonly string _code;

		internal SourceStringContentProvider(string code)
		{
			ContractUtils.RequiresNotNull(code, "code");
			_code = NormalizeLineEndings(code);
		}

		public override SourceCodeReader GetReader()
		{
			return new SourceCodeReader(new StringReader(_code), null);
		}

		private string NormalizeLineEndings(string input)
		{
			return input.Replace("\r\n", "\n") + "\n";
		}
	}

	public const string __doc__ = "zipimport provides support for importing Python modules from Zip archives.\r\n\r\nThis module exports three objects:\r\n- zipimporter: a class; its constructor takes a path to a Zip archive.\r\n- ZipImportError: exception raised by zipimporter objects. It's a\r\nsubclass of ImportError, so it can be caught as ImportError, too.\r\n- _zip_directory_cache: a dict, mapping archive paths to zip directory\r\ninfo dicts, as used in zipimporter._files.\r\n\r\nIt is usually not needed to use the zipimport module explicitly; it is\r\nused by the builtin import mechanism for sys.path items that are paths\r\nto Zip archives.";

	private static readonly object _zip_directory_cache_key = new object();

	public static PythonType ZipImportError;

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		if (!context.HasModuleState(_zip_directory_cache_key))
		{
			context.SetModuleState(_zip_directory_cache_key, new PythonDictionary());
		}
		dict["_zip_directory_cache"] = context.GetModuleState(_zip_directory_cache_key);
		InitModuleExceptions(context, dict);
	}

	internal static Exception MakeError(params object[] args)
	{
		return PythonOps.CreateThrowable(ZipImportError, args);
	}

	private static void InitModuleExceptions(PythonContext context, PythonDictionary dict)
	{
		ZipImportError = context.EnsureModuleException("zipimport.ZipImportError", PythonExceptions.ImportError, typeof(PythonExceptions.BaseException), dict, "ZipImportError", "zipimport", (string msg) => new ImportException(msg));
	}
}
