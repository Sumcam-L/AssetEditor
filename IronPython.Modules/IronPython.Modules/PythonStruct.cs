using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonStruct
{
	[Documentation("Represents a compiled struct pattern")]
	[PythonType]
	public class Struct : IWeakReferenceable
	{
		private string _formatString;

		private Format[] _formats;

		private bool _isStandardized;

		private bool _isLittleEndian;

		private int _encodingCount = -1;

		private int _encodingSize = -1;

		private WeakRefTracker _tracker;

		[Documentation("gets the current format string for the compiled Struct")]
		public string format => _formatString;

		[Documentation("gets the number of bytes that the serialized string will occupy or are required to deserialize the data")]
		public int size => _encodingSize;

		private void Initialize(Struct s)
		{
			_formatString = s._formatString;
			_formats = s._formats;
			_isStandardized = s._isStandardized;
			_isLittleEndian = s._isLittleEndian;
			_encodingCount = s._encodingCount;
			_encodingSize = s._encodingSize;
			_tracker = s._tracker;
		}

		internal Struct(CodeContext context, [NotNull] string fmt)
		{
			__init__(context, fmt);
		}

		[Documentation("creates a new uninitialized struct object - all arguments are ignored")]
		public Struct(params object[] args)
		{
		}

		[Documentation("creates a new uninitialized struct object - all arguments are ignored")]
		public Struct([ParamDictionary] IDictionary<object, object> kwArgs, params object[] args)
		{
		}

		[Documentation("initializes or re-initializes the compiled struct object with a new format")]
		public void __init__(CodeContext context, [NotNull] string fmt)
		{
			ContractUtils.RequiresNotNull(fmt, "fmt");
			_formatString = fmt;
			if (_cache.TryGetValue(_formatString, out var value))
			{
				Initialize(value);
			}
			else
			{
				Compile(context, fmt);
			}
		}

		[Documentation("returns a string consisting of the values serialized according to the format of the struct object")]
		public string pack(CodeContext context, params object[] values)
		{
			if (values.Length != _encodingCount)
			{
				throw Error(context, $"pack requires exactly {_encodingCount} arguments");
			}
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder(_encodingSize);
			for (int i = 0; i < _formats.Length; i++)
			{
				Format format = _formats[i];
				if (!_isStandardized)
				{
					int nativeSize = format.NativeSize;
					int num2 = Align(stringBuilder.Length, nativeSize);
					int num3 = num2 - stringBuilder.Length;
					for (int j = 0; j < num3; j++)
					{
						stringBuilder.Append('\0');
					}
				}
				switch (format.Type)
				{
				case FormatType.PadByte:
					stringBuilder.Append('\0', format.Count);
					break;
				case FormatType.Bool:
					stringBuilder.Append(GetBoolValue(context, num++, values) ? '\u0001' : '\0');
					break;
				case FormatType.Char:
				{
					for (int num8 = 0; num8 < format.Count; num8++)
					{
						stringBuilder.Append(GetCharValue(context, num++, values));
					}
					break;
				}
				case FormatType.SignedChar:
				{
					for (int l = 0; l < format.Count; l++)
					{
						stringBuilder.Append((char)(byte)GetSByteValue(context, num++, values));
					}
					break;
				}
				case FormatType.UnsignedChar:
				{
					for (int num10 = 0; num10 < format.Count; num10++)
					{
						stringBuilder.Append((char)GetByteValue(context, num++, values));
					}
					break;
				}
				case FormatType.Short:
				{
					for (int num6 = 0; num6 < format.Count; num6++)
					{
						WriteShort(stringBuilder, _isLittleEndian, GetShortValue(context, num++, values));
					}
					break;
				}
				case FormatType.UnsignedShort:
				{
					for (int n = 0; n < format.Count; n++)
					{
						WriteUShort(stringBuilder, _isLittleEndian, GetUShortValue(context, num++, values));
					}
					break;
				}
				case FormatType.Int:
				{
					for (int num12 = 0; num12 < format.Count; num12++)
					{
						WriteInt(stringBuilder, _isLittleEndian, GetIntValue(context, num++, values));
					}
					break;
				}
				case FormatType.UnsignedInt:
				{
					for (int num11 = 0; num11 < format.Count; num11++)
					{
						WriteUInt(stringBuilder, _isLittleEndian, GetULongValue(context, _isStandardized, num++, values, "unsigned int"));
					}
					break;
				}
				case FormatType.UnsignedLong:
				{
					for (int num9 = 0; num9 < format.Count; num9++)
					{
						WriteUInt(stringBuilder, _isLittleEndian, GetULongValue(context, _isStandardized, num++, values, "unsigned long"));
					}
					break;
				}
				case FormatType.Pointer:
				{
					for (int num7 = 0; num7 < format.Count; num7++)
					{
						WritePointer(stringBuilder, _isLittleEndian, GetPointer(context, num++, values));
					}
					break;
				}
				case FormatType.LongLong:
				{
					for (int num5 = 0; num5 < format.Count; num5++)
					{
						WriteLong(stringBuilder, _isLittleEndian, GetLongValue(context, num++, values));
					}
					break;
				}
				case FormatType.UnsignedLongLong:
				{
					for (int num4 = 0; num4 < format.Count; num4++)
					{
						WriteULong(stringBuilder, _isLittleEndian, GetULongLongValue(context, num++, values));
					}
					break;
				}
				case FormatType.Double:
				{
					for (int m = 0; m < format.Count; m++)
					{
						WriteDouble(stringBuilder, _isLittleEndian, GetDoubleValue(context, num++, values));
					}
					break;
				}
				case FormatType.Float:
				{
					for (int k = 0; k < format.Count; k++)
					{
						WriteFloat(stringBuilder, _isLittleEndian, (float)GetDoubleValue(context, num++, values));
					}
					break;
				}
				case FormatType.CString:
					WriteString(stringBuilder, format.Count, GetStringValue(context, num++, values));
					break;
				case FormatType.PascalString:
					WritePascalString(stringBuilder, format.Count - 1, GetStringValue(context, num++, values));
					break;
				default:
					throw Error(context, "bad format string");
				}
			}
			return stringBuilder.ToString();
		}

		[Documentation("Stores the deserialized data into the provided array")]
		public void pack_into(CodeContext context, [NotNull] ArrayModule.array buffer, int offset, params object[] args)
		{
			byte[] array = buffer.ToByteArray();
			if (offset + size > array.Length)
			{
				throw Error(context, $"pack_into requires a buffer of at least {size} bytes");
			}
			string text = pack(context, args);
			for (int i = 0; i < text.Length; i++)
			{
				array[i + offset] = (byte)text[i];
			}
			buffer.Clear();
			buffer.FromStream(new MemoryStream(array));
		}

		[Documentation("deserializes the string using the structs specified format")]
		public PythonTuple unpack(CodeContext context, [NotNull] string @string)
		{
			if (@string.Length != size)
			{
				throw Error(context, $"unpack requires a string argument of length {size}");
			}
			int index = 0;
			List<object> list = new List<object>(_encodingCount);
			for (int i = 0; i < _formats.Length; i++)
			{
				Format format = _formats[i];
				if (!_isStandardized)
				{
					int nativeSize = format.NativeSize;
					if (nativeSize > 0)
					{
						index = Align(index, nativeSize);
					}
				}
				switch (format.Type)
				{
				case FormatType.PadByte:
					index += format.Count;
					break;
				case FormatType.Bool:
				{
					for (int m = 0; m < format.Count; m++)
					{
						list.Add(CreateBoolValue(context, ref index, @string));
					}
					break;
				}
				case FormatType.Char:
				{
					for (int num3 = 0; num3 < format.Count; num3++)
					{
						list.Add(CreateCharValue(context, ref index, @string).ToString());
					}
					break;
				}
				case FormatType.SignedChar:
				{
					for (int num8 = 0; num8 < format.Count; num8++)
					{
						list.Add((int)(sbyte)CreateCharValue(context, ref index, @string));
					}
					break;
				}
				case FormatType.UnsignedChar:
				{
					for (int num5 = 0; num5 < format.Count; num5++)
					{
						list.Add((int)CreateCharValue(context, ref index, @string));
					}
					break;
				}
				case FormatType.Short:
				{
					for (int num = 0; num < format.Count; num++)
					{
						list.Add((int)CreateShortValue(context, ref index, _isLittleEndian, @string));
					}
					break;
				}
				case FormatType.UnsignedShort:
				{
					for (int k = 0; k < format.Count; k++)
					{
						list.Add((int)CreateUShortValue(context, ref index, _isLittleEndian, @string));
					}
					break;
				}
				case FormatType.Int:
				{
					for (int num7 = 0; num7 < format.Count; num7++)
					{
						list.Add(CreateIntValue(context, ref index, _isLittleEndian, @string));
					}
					break;
				}
				case FormatType.UnsignedInt:
				case FormatType.UnsignedLong:
				{
					for (int num6 = 0; num6 < format.Count; num6++)
					{
						list.Add(BigIntegerOps.__int__(CreateUIntValue(context, ref index, _isLittleEndian, @string)));
					}
					break;
				}
				case FormatType.Pointer:
				{
					for (int num4 = 0; num4 < format.Count; num4++)
					{
						if (IntPtr.Size == 4)
						{
							list.Add(CreateIntValue(context, ref index, _isLittleEndian, @string));
						}
						else
						{
							list.Add(BigIntegerOps.__int__(CreateLongValue(context, ref index, _isLittleEndian, @string)));
						}
					}
					break;
				}
				case FormatType.LongLong:
				{
					for (int num2 = 0; num2 < format.Count; num2++)
					{
						list.Add(BigIntegerOps.__int__(CreateLongValue(context, ref index, _isLittleEndian, @string)));
					}
					break;
				}
				case FormatType.UnsignedLongLong:
				{
					for (int n = 0; n < format.Count; n++)
					{
						list.Add(BigIntegerOps.__int__(CreateULongValue(context, ref index, _isLittleEndian, @string)));
					}
					break;
				}
				case FormatType.Float:
				{
					for (int l = 0; l < format.Count; l++)
					{
						list.Add((double)CreateFloatValue(context, ref index, _isLittleEndian, @string));
					}
					break;
				}
				case FormatType.Double:
				{
					for (int j = 0; j < format.Count; j++)
					{
						list.Add(CreateDoubleValue(context, ref index, _isLittleEndian, @string));
					}
					break;
				}
				case FormatType.CString:
					list.Add(CreateString(context, ref index, format.Count, @string));
					break;
				case FormatType.PascalString:
					list.Add(CreatePascalString(context, ref index, format.Count - 1, @string));
					break;
				}
			}
			return new PythonTuple(list);
		}

		public PythonTuple unpack(CodeContext context, [NotNull] ArrayModule.array buffer)
		{
			return unpack_from(context, buffer, 0);
		}

		public PythonTuple unpack(CodeContext context, [NotNull] PythonBuffer buffer)
		{
			return unpack_from(context, buffer, 0);
		}

		[Documentation("reads the current format from the specified array")]
		public PythonTuple unpack_from(CodeContext context, [NotNull] ArrayModule.array buffer, [DefaultParameterValue(0)] int offset)
		{
			return unpack_from(context, buffer.ToByteArray().MakeString(), offset);
		}

		[Documentation("reads the current format from the specified string")]
		public PythonTuple unpack_from(CodeContext context, [NotNull] string buffer, [DefaultParameterValue(0)] int offset)
		{
			int num = buffer.Length - offset;
			if (num < size)
			{
				throw Error(context, $"unpack_from requires a buffer of at least {size} bytes");
			}
			return unpack(context, buffer.Substring(offset, size));
		}

		[Documentation("reads the current format from the specified buffer object")]
		public PythonTuple unpack_from(CodeContext context, [NotNull] PythonBuffer buffer, [DefaultParameterValue(0)] int offset)
		{
			return unpack_from(context, buffer.ToString(), offset);
		}

		WeakRefTracker IWeakReferenceable.GetWeakRef()
		{
			return _tracker;
		}

		bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
		{
			return Interlocked.CompareExchange(ref _tracker, value, null) == null;
		}

		void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
		{
			_tracker = value;
		}

		private void Compile(CodeContext context, string fmt)
		{
			List<Format> list = new List<Format>();
			int num = 1;
			bool isLittleEndian = BitConverter.IsLittleEndian;
			bool isStandardized = false;
			for (int i = 0; i < fmt.Length; i++)
			{
				switch (fmt[i])
				{
				case 'x':
					list.Add(new Format(FormatType.PadByte, num));
					num = 1;
					continue;
				case '?':
					list.Add(new Format(FormatType.Bool, num));
					num = 1;
					continue;
				case 'c':
					list.Add(new Format(FormatType.Char, num));
					num = 1;
					continue;
				case 'b':
					list.Add(new Format(FormatType.SignedChar, num));
					num = 1;
					continue;
				case 'B':
					list.Add(new Format(FormatType.UnsignedChar, num));
					num = 1;
					continue;
				case 'h':
					list.Add(new Format(FormatType.Short, num));
					num = 1;
					continue;
				case 'H':
					list.Add(new Format(FormatType.UnsignedShort, num));
					num = 1;
					continue;
				case 'i':
				case 'l':
					list.Add(new Format(FormatType.Int, num));
					num = 1;
					continue;
				case 'I':
					list.Add(new Format(FormatType.UnsignedInt, num));
					num = 1;
					continue;
				case 'L':
					list.Add(new Format(FormatType.UnsignedLong, num));
					num = 1;
					continue;
				case 'q':
					list.Add(new Format(FormatType.LongLong, num));
					num = 1;
					continue;
				case 'Q':
					list.Add(new Format(FormatType.UnsignedLongLong, num));
					num = 1;
					continue;
				case 'f':
					list.Add(new Format(FormatType.Float, num));
					num = 1;
					continue;
				case 'd':
					list.Add(new Format(FormatType.Double, num));
					num = 1;
					continue;
				case 's':
					list.Add(new Format(FormatType.CString, num));
					num = 1;
					continue;
				case 'p':
					list.Add(new Format(FormatType.PascalString, num));
					num = 1;
					continue;
				case 'P':
					list.Add(new Format(FormatType.Pointer, num));
					num = 1;
					continue;
				case '=':
					if (i != 0)
					{
						throw Error(context, "unexpected byte order");
					}
					isStandardized = true;
					continue;
				case '@':
					if (i != 0)
					{
						throw Error(context, "unexpected byte order");
					}
					continue;
				case '<':
					if (i != 0)
					{
						throw Error(context, "unexpected byte order");
					}
					isLittleEndian = true;
					isStandardized = true;
					continue;
				case '!':
				case '>':
					if (i != 0)
					{
						throw Error(context, "unexpected byte order");
					}
					isLittleEndian = false;
					isStandardized = true;
					continue;
				case '\t':
				case ' ':
					continue;
				}
				if (char.IsDigit(fmt[i]))
				{
					num = 0;
					for (; char.IsDigit(fmt[i]); i++)
					{
						num = num * 10 + (fmt[i] - 48);
					}
					if (char.IsWhiteSpace(fmt[i]))
					{
						Error(context, "white space not allowed between count and format");
					}
					i--;
					continue;
				}
				throw Error(context, "bad format string");
			}
			_formats = list.ToArray();
			_isStandardized = isStandardized;
			_isLittleEndian = isLittleEndian;
			_encodingSize = (_encodingCount = 0);
			for (int j = 0; j < _formats.Length; j++)
			{
				if (_formats[j].Type != FormatType.PadByte)
				{
					if (_formats[j].Type != FormatType.CString && _formats[j].Type != FormatType.PascalString)
					{
						_encodingCount += _formats[j].Count;
					}
					else
					{
						_encodingCount++;
					}
				}
				if (!_isStandardized)
				{
					_encodingSize = Align(_encodingSize, _formats[j].NativeSize);
				}
				_encodingSize += GetNativeSize(_formats[j].Type) * _formats[j].Count;
			}
			_cache.Add(fmt, this);
		}

		internal static Struct Create(string format)
		{
			Struct obj = new Struct();
			obj.__init__(DefaultContext.Default, format);
			return obj;
		}
	}

	private enum FormatType
	{
		None,
		PadByte,
		Bool,
		Char,
		SignedChar,
		UnsignedChar,
		Short,
		UnsignedShort,
		Int,
		UnsignedInt,
		UnsignedLong,
		Float,
		LongLong,
		UnsignedLongLong,
		Double,
		CString,
		PascalString,
		Pointer
	}

	private struct Format
	{
		public FormatType Type;

		public int Count;

		public int NativeSize => GetNativeSize(Type);

		public Format(FormatType type, int count)
		{
			Type = type;
			Count = count;
		}
	}

	public const string __doc__ = null;

	public const string __version__ = "0.2";

	public const int _PY_STRUCT_FLOAT_COERCE = 0;

	public const int _PY_STRUCT_OVERFLOW_MASKING = 0;

	public const int _PY_STRUCT_RANGE_CHECKING = 0;

	private const int MAX_CACHE_SIZE = 1024;

	private static CacheDict<string, Struct> _cache = new CacheDict<string, Struct>(1024);

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.EnsureModuleException("structerror", dict, "error", "struct");
	}

	private static int GetNativeSize(FormatType c)
	{
		switch (c)
		{
		case FormatType.PadByte:
		case FormatType.Bool:
		case FormatType.Char:
		case FormatType.SignedChar:
		case FormatType.UnsignedChar:
		case FormatType.CString:
		case FormatType.PascalString:
			return 1;
		case FormatType.Short:
		case FormatType.UnsignedShort:
			return 2;
		case FormatType.Int:
		case FormatType.UnsignedInt:
		case FormatType.UnsignedLong:
		case FormatType.Float:
			return 4;
		case FormatType.LongLong:
		case FormatType.UnsignedLongLong:
		case FormatType.Double:
			return 8;
		case FormatType.Pointer:
			return IntPtr.Size;
		default:
			throw new InvalidOperationException(c.ToString());
		}
	}

	[Documentation("Clear the internal cache.")]
	public static void _clearcache()
	{
		_cache = new CacheDict<string, Struct>(1024);
	}

	[Documentation("int(x[, base]) -> integer\n\nConvert a string or number to an integer, if possible.  A floating point\nargument will be truncated towards zero (this does not include a string\nrepresentation of a floating point number!)  When converting a string, use\nthe optional base.  It is an error to supply a base when converting a\nnon-string.  If base is zero, the proper base is guessed based on the\nstring content.  If the argument is outside the integer range a\nlong object will be returned instead.")]
	public static int calcsize(CodeContext context, [NotNull] string fmt)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		return value.size;
	}

	[Documentation("Return string containing values v1, v2, ... packed according to fmt.")]
	public static string pack(CodeContext context, [NotNull] string fmt, params object[] values)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		return value.pack(context, values);
	}

	[Documentation("Pack the values v1, v2, ... according to fmt.\nWrite the packed bytes into the writable buffer buf starting at offset.")]
	public static void pack_into(CodeContext context, [NotNull] string fmt, [NotNull] ArrayModule.array buffer, int offset, params object[] args)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		value.pack_into(context, buffer, offset, args);
	}

	[Documentation("Unpack the string containing packed C structure data, according to fmt.\nRequires len(string) == calcsize(fmt).")]
	public static PythonTuple unpack(CodeContext context, [NotNull] string fmt, [NotNull] string @string)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		return value.unpack(context, @string);
	}

	[Documentation("Unpack the string containing packed C structure data, according to fmt.\nRequires len(string) == calcsize(fmt).")]
	public static PythonTuple unpack(CodeContext context, [NotNull] string fmt, [NotNull] ArrayModule.array buffer)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		return value.unpack(context, buffer);
	}

	[Documentation("Unpack the string containing packed C structure data, according to fmt.\nRequires len(string) == calcsize(fmt).")]
	public static PythonTuple unpack(CodeContext context, [NotNull] string fmt, [NotNull] PythonBuffer buffer)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		return value.unpack(context, buffer);
	}

	[Documentation("Unpack the buffer, containing packed C structure data, according to\nfmt, starting at offset. Requires len(buffer[offset:]) >= calcsize(fmt).")]
	public static PythonTuple unpack_from(CodeContext context, [NotNull] string fmt, [NotNull] ArrayModule.array buffer, [DefaultParameterValue(0)] int offset)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		return value.unpack_from(context, buffer, offset);
	}

	[Documentation("Unpack the buffer, containing packed C structure data, according to\nfmt, starting at offset. Requires len(buffer[offset:]) >= calcsize(fmt).")]
	public static PythonTuple unpack_from(CodeContext context, [NotNull] string fmt, [NotNull] string buffer, [DefaultParameterValue(0)] int offset)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		return value.unpack_from(context, buffer, offset);
	}

	[Documentation("Unpack the buffer, containing packed C structure data, according to\nfmt, starting at offset. Requires len(buffer[offset:]) >= calcsize(fmt).")]
	public static PythonTuple unpack_from(CodeContext context, [NotNull] string fmt, [NotNull] PythonBuffer buffer, [DefaultParameterValue(0)] int offset)
	{
		if (!_cache.TryGetValue(fmt, out var value))
		{
			value = new Struct(context, fmt);
		}
		return value.unpack_from(context, buffer, offset);
	}

	private static void WriteShort(StringBuilder res, bool fLittleEndian, short val)
	{
		if (fLittleEndian)
		{
			res.Append((char)(val & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
		}
		else
		{
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)(val & 0xFF));
		}
	}

	private static void WriteUShort(StringBuilder res, bool fLittleEndian, ushort val)
	{
		if (fLittleEndian)
		{
			res.Append((char)(val & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
		}
		else
		{
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)(val & 0xFF));
		}
	}

	private static void WriteInt(StringBuilder res, bool fLittleEndian, int val)
	{
		if (fLittleEndian)
		{
			res.Append((char)(val & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)((val >> 16) & 0xFF));
			res.Append((char)((val >> 24) & 0xFF));
		}
		else
		{
			res.Append((char)((val >> 24) & 0xFF));
			res.Append((char)((val >> 16) & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)(val & 0xFF));
		}
	}

	private static void WriteUInt(StringBuilder res, bool fLittleEndian, uint val)
	{
		if (fLittleEndian)
		{
			res.Append((char)(val & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)((val >> 16) & 0xFF));
			res.Append((char)((val >> 24) & 0xFF));
		}
		else
		{
			res.Append((char)((val >> 24) & 0xFF));
			res.Append((char)((val >> 16) & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)(val & 0xFF));
		}
	}

	private static void WritePointer(StringBuilder res, bool fLittleEndian, IntPtr val)
	{
		if (IntPtr.Size == 4)
		{
			WriteInt(res, fLittleEndian, val.ToInt32());
		}
		else
		{
			WriteLong(res, fLittleEndian, val.ToInt64());
		}
	}

	private static void WriteFloat(StringBuilder res, bool fLittleEndian, float val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		if (fLittleEndian)
		{
			res.Append((char)bytes[0]);
			res.Append((char)bytes[1]);
			res.Append((char)bytes[2]);
			res.Append((char)bytes[3]);
		}
		else
		{
			res.Append((char)bytes[3]);
			res.Append((char)bytes[2]);
			res.Append((char)bytes[1]);
			res.Append((char)bytes[0]);
		}
	}

	private static void WriteLong(StringBuilder res, bool fLittleEndian, long val)
	{
		if (fLittleEndian)
		{
			res.Append((char)(val & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)((val >> 16) & 0xFF));
			res.Append((char)((val >> 24) & 0xFF));
			res.Append((char)((val >> 32) & 0xFF));
			res.Append((char)((val >> 40) & 0xFF));
			res.Append((char)((val >> 48) & 0xFF));
			res.Append((char)((val >> 56) & 0xFF));
		}
		else
		{
			res.Append((char)((val >> 56) & 0xFF));
			res.Append((char)((val >> 48) & 0xFF));
			res.Append((char)((val >> 40) & 0xFF));
			res.Append((char)((val >> 32) & 0xFF));
			res.Append((char)((val >> 24) & 0xFF));
			res.Append((char)((val >> 16) & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)(val & 0xFF));
		}
	}

	private static void WriteULong(StringBuilder res, bool fLittleEndian, ulong val)
	{
		if (fLittleEndian)
		{
			res.Append((char)(val & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)((val >> 16) & 0xFF));
			res.Append((char)((val >> 24) & 0xFF));
			res.Append((char)((val >> 32) & 0xFF));
			res.Append((char)((val >> 40) & 0xFF));
			res.Append((char)((val >> 48) & 0xFF));
			res.Append((char)((val >> 56) & 0xFF));
		}
		else
		{
			res.Append((char)((val >> 56) & 0xFF));
			res.Append((char)((val >> 48) & 0xFF));
			res.Append((char)((val >> 40) & 0xFF));
			res.Append((char)((val >> 32) & 0xFF));
			res.Append((char)((val >> 24) & 0xFF));
			res.Append((char)((val >> 16) & 0xFF));
			res.Append((char)((val >> 8) & 0xFF));
			res.Append((char)(val & 0xFF));
		}
	}

	private static void WriteDouble(StringBuilder res, bool fLittleEndian, double val)
	{
		byte[] bytes = BitConverter.GetBytes(val);
		if (fLittleEndian)
		{
			res.Append((char)bytes[0]);
			res.Append((char)bytes[1]);
			res.Append((char)bytes[2]);
			res.Append((char)bytes[3]);
			res.Append((char)bytes[4]);
			res.Append((char)bytes[5]);
			res.Append((char)bytes[6]);
			res.Append((char)bytes[7]);
		}
		else
		{
			res.Append((char)bytes[7]);
			res.Append((char)bytes[6]);
			res.Append((char)bytes[5]);
			res.Append((char)bytes[4]);
			res.Append((char)bytes[3]);
			res.Append((char)bytes[2]);
			res.Append((char)bytes[1]);
			res.Append((char)bytes[0]);
		}
	}

	private static void WriteString(StringBuilder res, int len, string val)
	{
		for (int i = 0; i < val.Length && i < len; i++)
		{
			res.Append(val[i]);
		}
		for (int j = val.Length; j < len; j++)
		{
			res.Append('\0');
		}
	}

	private static void WritePascalString(StringBuilder res, int len, string val)
	{
		int num = Math.Min(255, Math.Min(val.Length, len));
		res.Append((char)num);
		for (int i = 0; i < val.Length && i < len; i++)
		{
			res.Append(val[i]);
		}
		for (int j = val.Length; j < len; j++)
		{
			res.Append('\0');
		}
	}

	internal static bool GetBoolValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvert(value, typeof(bool), out var result))
		{
			return (bool)result;
		}
		throw Error(context, "expected bool value got " + value.ToString());
	}

	internal static char GetCharValue(CodeContext context, int index, object[] args)
	{
		if (!(GetValue(context, index, args) is string { Length: 1 } text))
		{
			throw Error(context, "char format requires string of length 1");
		}
		return text[0];
	}

	internal static sbyte GetSByteValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToSByte(value, out var result))
		{
			return result;
		}
		throw Error(context, "expected sbyte value got " + value.ToString());
	}

	internal static byte GetByteValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToByte(value, out var result))
		{
			return result;
		}
		if (Converter.TryConvertToChar(value, out var result2))
		{
			return (byte)result2;
		}
		throw Error(context, "expected byte value got " + value.ToString());
	}

	internal static short GetShortValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToInt16(value, out var result))
		{
			return result;
		}
		throw Error(context, "expected short value");
	}

	internal static ushort GetUShortValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToUInt16(value, out var result))
		{
			return result;
		}
		throw Error(context, "expected ushort value");
	}

	internal static int GetIntValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToInt32(value, out var result))
		{
			return result;
		}
		throw Error(context, "expected int value");
	}

	internal static uint GetUIntValue(CodeContext context, bool isStandardized, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToUInt32(value, out var result))
		{
			return result;
		}
		if (isStandardized)
		{
			throw Error(context, "expected unsigned long value");
		}
		PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "'I' format requires 0 <= number <= 4294967295");
		return 0u;
	}

	internal static uint GetULongValue(CodeContext context, bool isStandardized, int index, object[] args, string type)
	{
		object value = GetValue(context, index, args);
		uint result;
		if (value is int)
		{
			result = (uint)(int)value;
			WarnRange(context, (int)value, isStandardized, type);
		}
		else if (value is BigInteger)
		{
			result = (uint)((BigInteger)value & uint.MaxValue);
			WarnRange(context, (BigInteger)value, isStandardized, type);
		}
		else if (value is Extensible<int>)
		{
			result = (uint)((Extensible<int>)value).Value;
			WarnRange(context, ((Extensible<int>)value).Value, isStandardized, type);
		}
		else if (value is Extensible<BigInteger>)
		{
			result = (uint)(((Extensible<BigInteger>)value).Value & uint.MaxValue);
			BigInteger value2 = ((Extensible<BigInteger>)value).Value;
			WarnRange(context, value2, isStandardized, type);
		}
		else
		{
			PythonOps.Warn(context, PythonExceptions.DeprecationWarning, "struct integer overflow masking is deprecated");
			object value3 = PythonContext.GetContext(context).Operation(PythonOperationKind.BitwiseAnd, value, (BigInteger)uint.MaxValue);
			if (!Converter.TryConvertToUInt32(value3, out result))
			{
				throw PythonOps.OverflowError("can't convert to " + type);
			}
		}
		return result;
	}

	private static void WarnRange(CodeContext context, int val, bool isStandardized, string type)
	{
		if (val < 0)
		{
			WarnRange(context, isStandardized, type);
		}
	}

	private static void WarnRange(CodeContext context, BigInteger bi, bool isStandardized, string type)
	{
		if (bi < 0L || bi > 4294967295L)
		{
			WarnRange(context, isStandardized, type);
		}
	}

	private static void WarnRange(CodeContext context, bool isStandardized, string type)
	{
		if (isStandardized)
		{
			throw Error(context, "expected " + type + " value");
		}
		PythonOps.Warn(context, PythonExceptions.DeprecationWarning, ((type == "unsigned long") ? "'L'" : "'I'") + " format requires 0 <= number <= 4294967295");
	}

	internal static IntPtr GetPointer(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		long result2;
		if (IntPtr.Size == 4)
		{
			if (Converter.TryConvertToUInt32(value, out var result))
			{
				return new IntPtr(result);
			}
		}
		else if (Converter.TryConvertToInt64(value, out result2))
		{
			return new IntPtr(result2);
		}
		throw Error(context, "expected pointer value");
	}

	internal static long GetLongValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToInt64(value, out var result))
		{
			return result;
		}
		throw Error(context, "expected long value");
	}

	internal static ulong GetULongLongValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToUInt64(value, out var result))
		{
			return result;
		}
		throw Error(context, "expected ulong value");
	}

	internal static double GetDoubleValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToDouble(value, out var result))
		{
			return result;
		}
		throw Error(context, "expected double value");
	}

	internal static string GetStringValue(CodeContext context, int index, object[] args)
	{
		object value = GetValue(context, index, args);
		if (Converter.TryConvertToString(value, out var result))
		{
			return result;
		}
		throw Error(context, "expected string value");
	}

	internal static object GetValue(CodeContext context, int index, object[] args)
	{
		if (index >= args.Length)
		{
			throw Error(context, "not enough arguments");
		}
		return args[index];
	}

	internal static bool CreateBoolValue(CodeContext context, ref int index, string data)
	{
		return ReadData(context, ref index, data) != '\0';
	}

	internal static char CreateCharValue(CodeContext context, ref int index, string data)
	{
		return ReadData(context, ref index, data);
	}

	internal static short CreateShortValue(CodeContext context, ref int index, bool fLittleEndian, string data)
	{
		byte b = (byte)ReadData(context, ref index, data);
		byte b2 = (byte)ReadData(context, ref index, data);
		if (fLittleEndian)
		{
			return (short)((b2 << 8) | b);
		}
		return (short)((b << 8) | b2);
	}

	internal static ushort CreateUShortValue(CodeContext context, ref int index, bool fLittleEndian, string data)
	{
		byte b = (byte)ReadData(context, ref index, data);
		byte b2 = (byte)ReadData(context, ref index, data);
		if (fLittleEndian)
		{
			return (ushort)((b2 << 8) | b);
		}
		return (ushort)((b << 8) | b2);
	}

	internal static float CreateFloatValue(CodeContext context, ref int index, bool fLittleEndian, string data)
	{
		byte[] array = new byte[4];
		if (fLittleEndian)
		{
			array[0] = (byte)ReadData(context, ref index, data);
			array[1] = (byte)ReadData(context, ref index, data);
			array[2] = (byte)ReadData(context, ref index, data);
			array[3] = (byte)ReadData(context, ref index, data);
		}
		else
		{
			array[3] = (byte)ReadData(context, ref index, data);
			array[2] = (byte)ReadData(context, ref index, data);
			array[1] = (byte)ReadData(context, ref index, data);
			array[0] = (byte)ReadData(context, ref index, data);
		}
		float num = BitConverter.ToSingle(array, 0);
		if (PythonContext.GetContext(context).FloatFormat == FloatFormat.Unknown && (float.IsNaN(num) || float.IsInfinity(num)))
		{
			throw PythonOps.ValueError("can't unpack IEEE 754 special value on non-IEEE platform");
		}
		return num;
	}

	internal static int CreateIntValue(CodeContext context, ref int index, bool fLittleEndian, string data)
	{
		byte b = (byte)ReadData(context, ref index, data);
		byte b2 = (byte)ReadData(context, ref index, data);
		byte b3 = (byte)ReadData(context, ref index, data);
		byte b4 = (byte)ReadData(context, ref index, data);
		if (fLittleEndian)
		{
			return (b4 << 24) | (b3 << 16) | (b2 << 8) | b;
		}
		return (b << 24) | (b2 << 16) | (b3 << 8) | b4;
	}

	internal static uint CreateUIntValue(CodeContext context, ref int index, bool fLittleEndian, string data)
	{
		byte b = (byte)ReadData(context, ref index, data);
		byte b2 = (byte)ReadData(context, ref index, data);
		byte b3 = (byte)ReadData(context, ref index, data);
		byte b4 = (byte)ReadData(context, ref index, data);
		if (fLittleEndian)
		{
			return (uint)((b4 << 24) | (b3 << 16) | (b2 << 8) | b);
		}
		return (uint)((b << 24) | (b2 << 16) | (b3 << 8) | b4);
	}

	internal static long CreateLongValue(CodeContext context, ref int index, bool fLittleEndian, string data)
	{
		long num = (byte)ReadData(context, ref index, data);
		long num2 = (byte)ReadData(context, ref index, data);
		long num3 = (byte)ReadData(context, ref index, data);
		long num4 = (byte)ReadData(context, ref index, data);
		long num5 = (byte)ReadData(context, ref index, data);
		long num6 = (byte)ReadData(context, ref index, data);
		long num7 = (byte)ReadData(context, ref index, data);
		long num8 = (byte)ReadData(context, ref index, data);
		if (fLittleEndian)
		{
			return (num8 << 56) | (num7 << 48) | (num6 << 40) | (num5 << 32) | (num4 << 24) | (num3 << 16) | (num2 << 8) | num;
		}
		return (num << 56) | (num2 << 48) | (num3 << 40) | (num4 << 32) | (num5 << 24) | (num6 << 16) | (num7 << 8) | num8;
	}

	internal static ulong CreateULongValue(CodeContext context, ref int index, bool fLittleEndian, string data)
	{
		ulong num = (byte)ReadData(context, ref index, data);
		ulong num2 = (byte)ReadData(context, ref index, data);
		ulong num3 = (byte)ReadData(context, ref index, data);
		ulong num4 = (byte)ReadData(context, ref index, data);
		ulong num5 = (byte)ReadData(context, ref index, data);
		ulong num6 = (byte)ReadData(context, ref index, data);
		ulong num7 = (byte)ReadData(context, ref index, data);
		ulong num8 = (byte)ReadData(context, ref index, data);
		if (fLittleEndian)
		{
			return (num8 << 56) | (num7 << 48) | (num6 << 40) | (num5 << 32) | (num4 << 24) | (num3 << 16) | (num2 << 8) | num;
		}
		return (num << 56) | (num2 << 48) | (num3 << 40) | (num4 << 32) | (num5 << 24) | (num6 << 16) | (num7 << 8) | num8;
	}

	internal static double CreateDoubleValue(CodeContext context, ref int index, bool fLittleEndian, string data)
	{
		byte[] array = new byte[8];
		if (fLittleEndian)
		{
			array[0] = (byte)ReadData(context, ref index, data);
			array[1] = (byte)ReadData(context, ref index, data);
			array[2] = (byte)ReadData(context, ref index, data);
			array[3] = (byte)ReadData(context, ref index, data);
			array[4] = (byte)ReadData(context, ref index, data);
			array[5] = (byte)ReadData(context, ref index, data);
			array[6] = (byte)ReadData(context, ref index, data);
			array[7] = (byte)ReadData(context, ref index, data);
		}
		else
		{
			array[7] = (byte)ReadData(context, ref index, data);
			array[6] = (byte)ReadData(context, ref index, data);
			array[5] = (byte)ReadData(context, ref index, data);
			array[4] = (byte)ReadData(context, ref index, data);
			array[3] = (byte)ReadData(context, ref index, data);
			array[2] = (byte)ReadData(context, ref index, data);
			array[1] = (byte)ReadData(context, ref index, data);
			array[0] = (byte)ReadData(context, ref index, data);
		}
		double num = BitConverter.ToDouble(array, 0);
		if (PythonContext.GetContext(context).DoubleFormat == FloatFormat.Unknown && (double.IsNaN(num) || double.IsInfinity(num)))
		{
			throw PythonOps.ValueError("can't unpack IEEE 754 special value on non-IEEE platform");
		}
		return num;
	}

	internal static string CreateString(CodeContext context, ref int index, int count, string data)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Append(ReadData(context, ref index, data));
		}
		return stringBuilder.ToString();
	}

	internal static string CreatePascalString(CodeContext context, ref int index, int count, string data)
	{
		int num = ReadData(context, ref index, data);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(ReadData(context, ref index, data));
		}
		for (int j = num; j < count; j++)
		{
			ReadData(context, ref index, data);
		}
		return stringBuilder.ToString();
	}

	private static char ReadData(CodeContext context, ref int index, string data)
	{
		if (index >= data.Length)
		{
			throw Error(context, "not enough data while reading");
		}
		return data[index++];
	}

	internal static int Align(int length, int size)
	{
		return (length + (size - 1)) & ~(size - 1);
	}

	private static Exception Error(CodeContext context, string msg)
	{
		return PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState("structerror"), msg);
	}
}
