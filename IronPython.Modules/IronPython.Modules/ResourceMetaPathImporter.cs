using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Zlib;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

[PythonType]
public class ResourceMetaPathImporter
{
	[Flags]
	private enum ModuleCodeType
	{
		Source = 0,
		Package = 1
	}

	private struct PackedResourceInfo
	{
		private int _fileSize;

		public string FullName;

		public int Compress;

		public int DataSize;

		public int FileOffset;

		public static PackedResourceInfo Create(string fullName, int compress, int dataSize, int fileSize, int fileOffset)
		{
			PackedResourceInfo result = default(PackedResourceInfo);
			result.FullName = fullName;
			result.Compress = compress;
			result.DataSize = dataSize;
			result._fileSize = fileSize;
			result.FileOffset = fileOffset;
			return result;
		}
	}

	private class PackedResourceLoader
	{
		private const int MaxPathLen = 256;

		private readonly Assembly _fromAssembly;

		private readonly string _resourceNameBase;

		public PackedResourceLoader(Assembly fromAssembly, string resourceName)
		{
			_fromAssembly = fromAssembly;
			_resourceNameBase = resourceName;
		}

		public bool LoadZipDirectory(out IDictionary<string, PackedResourceInfo> files, out IDictionary<string, PackedResourceInfo[]> modules, out string unpackingError)
		{
			if (!ReadZipDirectory(out files, out unpackingError))
			{
				modules = null;
				return false;
			}
			try
			{
				var source = from entry in files.Values
					let isPyFile = entry.FullName.EndsWith(".py", StringComparison.OrdinalIgnoreCase)
					where isPyFile
					let name = entry.FullName.Substring(0, entry.FullName.Length - 3)
					let dottedName = name.Replace('\\', '.').Replace('/', '.')
					let lineage = dottedName.Split('.')
					let fileName = lineage.Last()
					let path = lineage.Take(lineage.Length - 1).ToArray()
					orderby fileName
					select new
					{
						name = fileName,
						path = path,
						dottedPath = string.Join(".", path),
						entry = entry
					};
				var source2 = from anon in source
					orderby anon.dottedPath
					group anon by anon.dottedPath into moduleGroup
					select new
					{
						Key = moduleGroup.Key,
						Items = moduleGroup.Select(item => item.entry).ToArray()
					};
				modules = source2.ToDictionary(moduleGroup => moduleGroup.Key, moduleGroup => moduleGroup.Items);
				return true;
			}
			catch (Exception ex)
			{
				files = null;
				modules = null;
				unpackingError = $"{ex.GetType().Name}: {ex.Message}";
				return false;
			}
		}

		private Stream GetZipArchive()
		{
			string compareName = _resourceNameBase.ToLowerInvariant();
			IEnumerable<string> source = from name in _fromAssembly.GetManifestResourceNames()
				where name.ToLowerInvariant().EndsWith(compareName)
				select name;
			string text = source.FirstOrDefault();
			if (!string.IsNullOrEmpty(text))
			{
				return _fromAssembly.GetManifestResourceStream(text);
			}
			return null;
		}

		private bool ReadZipDirectory(out IDictionary<string, PackedResourceInfo> result, out string unpackingError)
		{
			unpackingError = null;
			result = null;
			try
			{
				Stream zipArchive = GetZipArchive();
				if (zipArchive == null)
				{
					unpackingError = "Resource not found.";
					return false;
				}
				using BinaryReader binaryReader = new BinaryReader(zipArchive);
				if (binaryReader.BaseStream.Length < 2)
				{
					unpackingError = "Can't read ZIP resource: Empty Resource.";
					return false;
				}
				byte[] array = new byte[22];
				binaryReader.BaseStream.Seek(-22L, SeekOrigin.End);
				int num = (int)binaryReader.BaseStream.Position;
				if (binaryReader.Read(array, 0, 22) != 22)
				{
					unpackingError = "Can't read ZIP resource: Invalid ZIP Directory.";
					return false;
				}
				if (BitConverter.ToUInt32(array, 0) != 101010256)
				{
					unpackingError = "Can't read ZIP resource: Not a ZIP file.";
					return false;
				}
				int num2 = BitConverter.ToInt32(array, 12);
				int num3 = BitConverter.ToInt32(array, 16);
				int num4 = num - num3 - num2;
				num3 += num4;
				IEnumerable<PackedResourceInfo> source = ReadZipDirectory(binaryReader, num3, num4);
				result = source.OrderBy((PackedResourceInfo entry) => entry.FullName).ToDictionary((PackedResourceInfo entry) => entry.FullName);
				return true;
			}
			catch (Exception ex)
			{
				unpackingError = $"{ex.GetType().Name}: {ex.Message}";
				return false;
			}
		}

		private static IEnumerable<PackedResourceInfo> ReadZipDirectory(BinaryReader reader, int headerOffset, int arcoffset)
		{
			while (true)
			{
				string name = string.Empty;
				reader.BaseStream.Seek(headerOffset, SeekOrigin.Begin);
				int l = reader.ReadInt32();
				if (l != 33639248)
				{
					break;
				}
				reader.BaseStream.Seek(headerOffset + 10, SeekOrigin.Begin);
				short compress = reader.ReadInt16();
				reader.ReadInt16();
				reader.ReadInt16();
				reader.ReadInt32();
				int dataSize = reader.ReadInt32();
				int fileSize = reader.ReadInt32();
				short nameSize = reader.ReadInt16();
				int headerSize = 46 + nameSize + reader.ReadInt16() + reader.ReadInt16();
				reader.BaseStream.Seek(headerOffset + 42, SeekOrigin.Begin);
				int fileOffset = reader.ReadInt32() + arcoffset;
				if (nameSize > 256)
				{
					nameSize = 256;
				}
				for (int i = 0; i < nameSize; i++)
				{
					char c = reader.ReadChar();
					if (c == '/')
					{
						c = Path.DirectorySeparatorChar;
					}
					name += c;
				}
				headerOffset += headerSize;
				yield return PackedResourceInfo.Create(name, compress, dataSize, fileSize, fileOffset);
			}
		}

		public bool GetData(PackedResourceInfo tocEntry, out byte[] result, out string unpackingError)
		{
			unpackingError = null;
			result = null;
			int fileOffset = tocEntry.FileOffset;
			int dataSize = tocEntry.DataSize;
			int compress = tocEntry.Compress;
			try
			{
				Stream zipArchive = GetZipArchive();
				if (zipArchive == null)
				{
					unpackingError = "Resource not found.";
					return false;
				}
				using BinaryReader binaryReader = new BinaryReader(zipArchive);
				binaryReader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
				int num = binaryReader.ReadInt32();
				if (num != 67324752)
				{
					unpackingError = "Bad local file header in ZIP resource.";
					return false;
				}
				binaryReader.BaseStream.Seek(fileOffset + 26, SeekOrigin.Begin);
				num = 30 + binaryReader.ReadInt16() + binaryReader.ReadInt16();
				fileOffset += num;
				binaryReader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
				byte[] array;
				try
				{
					array = binaryReader.ReadBytes((compress == 0) ? dataSize : (dataSize + 1));
				}
				catch
				{
					unpackingError = "Can't read data";
					return false;
				}
				if (compress != 0)
				{
					array[dataSize] = 90;
				}
				result = ((compress == 0) ? array : ZlibModule.Decompress(array, -15));
				return true;
			}
			catch (Exception ex)
			{
				unpackingError = $"{ex.GetType().Name}: {ex.Message}";
				return false;
			}
		}
	}

	private readonly PackedResourceLoader _loader;

	private readonly IDictionary<string, PackedResourceInfo> _unpackedLibrary;

	private readonly IDictionary<string, PackedResourceInfo[]> _unpackedModules;

	private readonly string _unpackingError;

	private static readonly Dictionary<string, ModuleCodeType> SearchOrder;

	static ResourceMetaPathImporter()
	{
		SearchOrder = new Dictionary<string, ModuleCodeType>
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

	public ResourceMetaPathImporter(Assembly fromAssembly, string resourceName)
	{
		_loader = new PackedResourceLoader(fromAssembly, resourceName);
		if (!_loader.LoadZipDirectory(out _unpackedLibrary, out _unpackedModules, out _unpackingError))
		{
			_unpackedLibrary = new Dictionary<string, PackedResourceInfo>();
			_unpackedModules = new Dictionary<string, PackedResourceInfo[]>();
			if (!string.IsNullOrEmpty(_unpackingError))
			{
				throw MakeError("meta_path importer initialization error: {0}", _unpackingError);
			}
		}
	}

	[Documentation("find_module(fullname, path=None) -> self or None.\r\n\r\nSearch for a module specified by 'fullname'. 'fullname' must be the\r\nfully qualified (dotted) module name. It returns the importer\r\ninstance itself if the module was found, or None if it wasn't.\r\nThe optional 'path' argument is ignored -- it's there for compatibility\r\nwith the importer protocol.")]
	public object find_module(CodeContext context, string fullname, params object[] args)
	{
		string text = MakeFilename(fullname);
		foreach (KeyValuePair<string, ModuleCodeType> item in SearchOrder)
		{
			string key = text + item.Key;
			if (_unpackedLibrary.ContainsKey(key))
			{
				return this;
			}
		}
		return null;
	}

	[Documentation("load_module(fullname) -> module.\r\n\r\nLoad the module specified by 'fullname'. 'fullname' must be the\r\nfully qualified (dotted) module name. It returns the imported\r\nmodule, or raises ResourceImportError if it wasn't found.")]
	public object load_module(CodeContext context, string fullname)
	{
		IDictionary<object, object> systemStateModules = context.LanguageContext.SystemStateModules;
		if (systemStateModules.ContainsKey(fullname))
		{
			return systemStateModules[fullname];
		}
		bool ispackage;
		string modpath;
		string moduleCode = GetModuleCode(context, fullname, out ispackage, out modpath);
		if (moduleCode == null)
		{
			return null;
		}
		PythonContext languageContext = context.LanguageContext;
		ScriptCode scriptCode;
		PythonModule pythonModule = languageContext.CompileModule(modpath, fullname, new SourceUnit(languageContext, new ZipImportModule.SourceStringContentProvider(moduleCode), modpath, SourceCodeKind.File), ModuleOptions.None, out scriptCode);
		PythonDictionary _dict__ = pythonModule.__dict__;
		_dict__.Add("__name__", fullname);
		_dict__.Add("__loader__", this);
		_dict__.Add("__package__", null);
		_dict__.Add("__file__", "<resource>");
		if (ispackage)
		{
			List value = PythonOps.MakeList();
			_dict__.Add("__path__", value);
		}
		systemStateModules.Add(fullname, pythonModule);
		try
		{
			scriptCode.Run(pythonModule.Scope);
			return pythonModule;
		}
		catch (Exception)
		{
			systemStateModules.Remove(fullname);
			throw;
		}
	}

	private string GetModuleCode(CodeContext context, string fullname, out bool ispackage, out string modpath)
	{
		string text = MakeFilename(fullname);
		ispackage = false;
		modpath = string.Empty;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		foreach (KeyValuePair<string, ModuleCodeType> item in SearchOrder)
		{
			string key = text + item.Key;
			if (_unpackedLibrary.ContainsKey(key))
			{
				PackedResourceInfo tocEntry = _unpackedLibrary[key];
				ispackage = (item.Value & ModuleCodeType.Package) == ModuleCodeType.Package;
				string codeFromData = GetCodeFromData(context, isbytecode: false, tocEntry);
				if (codeFromData != null)
				{
					modpath = tocEntry.FullName;
					return codeFromData;
				}
			}
		}
		throw MakeError("can't find module '{0}'", fullname);
	}

	private string GetCodeFromData(CodeContext context, bool isbytecode, PackedResourceInfo tocEntry)
	{
		byte[] data = GetData(tocEntry);
		string result = null;
		if (data != null && !isbytecode)
		{
			result = context.LanguageContext.DefaultEncoding.GetString(data, 0, data.Length);
		}
		return result;
	}

	private byte[] GetData(PackedResourceInfo tocEntry)
	{
		if (!_loader.GetData(tocEntry, out var result, out var unpackingError))
		{
			throw MakeError(unpackingError);
		}
		return result;
	}

	private static Exception MakeError(params object[] args)
	{
		return PythonOps.CreateThrowable(PythonExceptions.ImportError, args);
	}

	private static string MakeFilename(string name)
	{
		return name.Replace('.', Path.DirectorySeparatorChar);
	}
}
