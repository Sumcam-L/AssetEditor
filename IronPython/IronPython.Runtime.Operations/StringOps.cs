using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public static class StringOps
{
	private static class CodecsInfo
	{
		public static readonly Dictionary<string, EncodingInfoWrapper> Codecs = MakeCodecsDict();

		private static Dictionary<string, EncodingInfoWrapper> MakeCodecsDict()
		{
			Dictionary<string, EncodingInfoWrapper> dictionary = new Dictionary<string, EncodingInfoWrapper>();
			EncodingInfo[] encodings = Encoding.GetEncodings();
			for (int i = 0; i < encodings.Length; i++)
			{
				string text = NormalizeEncodingName(encodings[i].Name);
				switch (text)
				{
				case "us_ascii":
				{
					string key = "cp" + encodings[i].CodePage;
					EncodingInfoWrapper encodingInfoWrapper24 = (dictionary["us_ascii"] = new AsciiEncodingInfoWrapper());
					EncodingInfoWrapper encodingInfoWrapper26 = (dictionary["646"] = encodingInfoWrapper24);
					EncodingInfoWrapper encodingInfoWrapper28 = (dictionary["ascii"] = encodingInfoWrapper26);
					EncodingInfoWrapper encodingInfoWrapper30 = (dictionary["us"] = encodingInfoWrapper28);
					EncodingInfoWrapper value6 = (dictionary[text] = encodingInfoWrapper30);
					dictionary[key] = value6;
					continue;
				}
				case "iso_8859_1":
				{
					EncodingInfoWrapper encodingInfoWrapper7 = (dictionary["l1"] = encodings[i]);
					EncodingInfoWrapper encodingInfoWrapper9 = (dictionary["latin1"] = encodingInfoWrapper7);
					EncodingInfoWrapper encodingInfoWrapper11 = (dictionary["latin"] = encodingInfoWrapper9);
					EncodingInfoWrapper encodingInfoWrapper13 = (dictionary["819"] = encodingInfoWrapper11);
					EncodingInfoWrapper encodingInfoWrapper15 = (dictionary["cp819"] = encodingInfoWrapper13);
					EncodingInfoWrapper encodingInfoWrapper17 = (dictionary["iso8859_1"] = encodingInfoWrapper15);
					EncodingInfoWrapper encodingInfoWrapper19 = (dictionary["iso 8859_1"] = encodingInfoWrapper17);
					EncodingInfoWrapper encodingInfoWrapper21 = (dictionary["latin1"] = encodingInfoWrapper19);
					EncodingInfoWrapper value5 = (dictionary["latin_1"] = encodingInfoWrapper21);
					dictionary["8859"] = value5;
					break;
				}
				case "utf_7":
				{
					EncodingInfoWrapper value4 = (dictionary["unicode-1-1-utf-7"] = encodings[i]);
					dictionary["u7"] = value4;
					break;
				}
				case "utf_8":
				{
					dictionary["utf_8_sig"] = encodings[i];
					EncodingInfoWrapper encodingInfoWrapper3 = (dictionary["u8"] = new EncodingInfoWrapper(encodings[i], new byte[0]));
					EncodingInfoWrapper value3 = (dictionary["utf8"] = encodingInfoWrapper3);
					dictionary["utf_8"] = value3;
					continue;
				}
				case "utf_16":
				{
					EncodingInfoWrapper value2 = (dictionary["utf_16le"] = new EncodingInfoWrapper(encodings[i], new byte[0]));
					dictionary["utf_16_le"] = value2;
					dictionary["utf16"] = new EncodingInfoWrapper(encodings[i], encodings[i].GetEncoding().GetPreamble());
					break;
				}
				case "unicodefffe":
				{
					EncodingInfoWrapper value = (dictionary["utf_16be"] = new EncodingInfoWrapper(encodings[i], new byte[0]));
					dictionary["utf_16_be"] = value;
					break;
				}
				}
				dictionary[text] = encodings[i];
				dictionary["windows-" + encodings[i].GetEncoding().WindowsCodePage] = encodings[i];
				string key2 = "cp" + encodings[i].CodePage;
				EncodingInfoWrapper value7 = (dictionary[encodings[i].CodePage.ToString()] = encodings[i]);
				dictionary[key2] = value7;
			}
			dictionary["raw_unicode_escape"] = new EncodingInfoWrapper(new UnicodeEscapeEncoding(raw: true));
			dictionary["unicode_escape"] = new EncodingInfoWrapper(new UnicodeEscapeEncoding(raw: false));
			return dictionary;
		}
	}

	private class EncodingInfoWrapper
	{
		private EncodingInfo _info;

		private Encoding _encoding;

		private byte[] _preamble;

		public EncodingInfoWrapper(Encoding enc)
		{
			_encoding = enc;
		}

		public EncodingInfoWrapper(EncodingInfo info)
		{
			_info = info;
		}

		public EncodingInfoWrapper(EncodingInfo info, byte[] preamble)
		{
			_info = info;
			_preamble = preamble;
		}

		public virtual Encoding GetEncoding()
		{
			if (_encoding != null)
			{
				return _encoding;
			}
			if (_preamble == null)
			{
				return _info.GetEncoding();
			}
			return new EncodingWrapper(_info.GetEncoding(), _preamble);
		}

		public static implicit operator EncodingInfoWrapper(EncodingInfo info)
		{
			return new EncodingInfoWrapper(info);
		}
	}

	private class AsciiEncodingInfoWrapper : EncodingInfoWrapper
	{
		public AsciiEncodingInfoWrapper()
			: base((EncodingInfo)null)
		{
		}

		public override Encoding GetEncoding()
		{
			return PythonAsciiEncoding.Instance;
		}
	}

	private class EncodingWrapper : Encoding
	{
		private byte[] _preamble;

		private Encoding _encoding;

		public EncodingWrapper(Encoding encoding, byte[] preamable)
		{
			_preamble = preamable;
			_encoding = encoding;
		}

		private void SetEncoderFallback()
		{
			_encoding.EncoderFallback = base.EncoderFallback;
		}

		private void SetDecoderFallback()
		{
			_encoding.DecoderFallback = base.DecoderFallback;
		}

		public override int GetByteCount(char[] chars, int index, int count)
		{
			SetEncoderFallback();
			return _encoding.GetByteCount(chars, index, count);
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			SetEncoderFallback();
			return _encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			SetDecoderFallback();
			return _encoding.GetCharCount(bytes, index, count);
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			SetDecoderFallback();
			return _encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
		}

		public override int GetMaxByteCount(int charCount)
		{
			SetEncoderFallback();
			return _encoding.GetMaxByteCount(charCount);
		}

		public override int GetMaxCharCount(int byteCount)
		{
			SetDecoderFallback();
			return _encoding.GetMaxCharCount(byteCount);
		}

		public override byte[] GetPreamble()
		{
			return _preamble;
		}

		public override Encoder GetEncoder()
		{
			SetEncoderFallback();
			return _encoding.GetEncoder();
		}

		public override Decoder GetDecoder()
		{
			SetDecoderFallback();
			return _encoding.GetDecoder();
		}

		public override object Clone()
		{
			EncodingWrapper encodingWrapper = (EncodingWrapper)base.Clone();
			encodingWrapper._encoding = (Encoding)_encoding.Clone();
			return encodingWrapper;
		}
	}

	[PythonType("str_iterator")]
	private class PythonStringEnumerable : IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
	{
		private readonly string _s;

		private int _index;

		public string Current
		{
			get
			{
				if (_index < 0)
				{
					throw PythonOps.SystemError("Enumeration has not started. Call MoveNext.");
				}
				if (_index >= _s.Length)
				{
					throw PythonOps.SystemError("Enumeration already finished.");
				}
				return ScriptingRuntimeHelpers.CharToString(_s[_index]);
			}
		}

		object IEnumerator.Current => ((IEnumerator<string>)this).Current;

		public PythonStringEnumerable(string s)
		{
			_index = -1;
			_s = s;
		}

		public IEnumerator GetEnumerator()
		{
			return this;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_index >= _s.Length)
			{
				return false;
			}
			_index++;
			return _index != _s.Length;
		}

		public void Reset()
		{
			_index = -1;
		}
	}

	private class PythonEncoderFallbackBuffer : EncoderFallbackBuffer
	{
		private object _function;

		private string _encoding;

		private string _strData;

		private string _buffer;

		private int _bufferIndex;

		public override int Remaining
		{
			get
			{
				if (_buffer == null)
				{
					return 0;
				}
				return _buffer.Length - _bufferIndex;
			}
		}

		public PythonEncoderFallbackBuffer(string encoding, string str, object callable)
		{
			_function = callable;
			_strData = str;
			_encoding = encoding;
		}

		public override bool Fallback(char charUnknown, int index)
		{
			return DoPythonFallback(index, 1);
		}

		public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
		{
			return DoPythonFallback(index, 2);
		}

		public override char GetNextChar()
		{
			if (_buffer == null || _bufferIndex >= _buffer.Length)
			{
				return '\0';
			}
			return _buffer[_bufferIndex++];
		}

		public override bool MovePrevious()
		{
			if (_bufferIndex > 0)
			{
				_bufferIndex--;
				return true;
			}
			return false;
		}

		public override void Reset()
		{
			_buffer = null;
			_bufferIndex = 0;
			base.Reset();
		}

		private bool DoPythonFallback(int index, int length)
		{
			if (_function != null)
			{
				PythonExceptions._UnicodeEncodeError unicodeEncodeError = new PythonExceptions._UnicodeEncodeError();
				unicodeEncodeError.__init__(_encoding, _strData, index, index + length, "unexpected code byte");
				object res = PythonCalls.Call(_function, unicodeEncodeError);
				string buffer = PythonDecoderFallbackBuffer.CheckReplacementTuple(res, "encoding");
				_buffer = buffer;
				_bufferIndex = 0;
				return true;
			}
			return false;
		}
	}

	private class PythonEncoderFallback : EncoderFallback
	{
		private object _function;

		private string _str;

		private string _enc;

		public override int MaxCharCount => int.MaxValue;

		public PythonEncoderFallback(string encoding, string data, object callable)
		{
			_function = callable;
			_str = data;
			_enc = encoding;
		}

		public override EncoderFallbackBuffer CreateFallbackBuffer()
		{
			return new PythonEncoderFallbackBuffer(_enc, _str, _function);
		}
	}

	private class PythonDecoderFallbackBuffer : DecoderFallbackBuffer
	{
		private object _function;

		private string _encoding;

		private string _strData;

		private string _buffer;

		private int _bufferIndex;

		public override int Remaining
		{
			get
			{
				if (_buffer == null)
				{
					return 0;
				}
				return _buffer.Length - _bufferIndex;
			}
		}

		public PythonDecoderFallbackBuffer(string encoding, string str, object callable)
		{
			_encoding = encoding;
			_strData = str;
			_function = callable;
		}

		public override char GetNextChar()
		{
			if (_buffer == null || _bufferIndex >= _buffer.Length)
			{
				return '\0';
			}
			return _buffer[_bufferIndex++];
		}

		public override bool MovePrevious()
		{
			if (_bufferIndex > 0)
			{
				_bufferIndex--;
				return true;
			}
			return false;
		}

		public override void Reset()
		{
			_buffer = null;
			_bufferIndex = 0;
			base.Reset();
		}

		public override bool Fallback(byte[] bytesUnknown, int index)
		{
			if (_function != null)
			{
				PythonExceptions._UnicodeDecodeError unicodeDecodeError = new PythonExceptions._UnicodeDecodeError();
				unicodeDecodeError.__init__(_encoding, _strData, index, index + bytesUnknown.Length, "unexpected code byte");
				object res = PythonCalls.Call(_function, unicodeDecodeError);
				string buffer = CheckReplacementTuple(res, "decoding");
				_buffer = buffer;
				_bufferIndex = 0;
				return true;
			}
			return false;
		}

		internal static string CheckReplacementTuple(object res, string encodeOrDecode)
		{
			bool flag = true;
			string result = null;
			if (res is PythonTuple pythonTuple && pythonTuple.__len__() == 2)
			{
				if (!Converter.TryConvertToString(pythonTuple[0], out result))
				{
					flag = false;
				}
				if (flag && !Converter.TryConvertToInt32(pythonTuple[1], out var _))
				{
					flag = false;
				}
			}
			else
			{
				flag = false;
			}
			if (!flag)
			{
				throw PythonOps.TypeError("{1} error handler must return tuple containing (str, int), got {0}", PythonOps.GetPythonTypeName(res), encodeOrDecode);
			}
			return result;
		}
	}

	private class PythonDecoderFallback : DecoderFallback
	{
		private object function;

		private string str;

		private string enc;

		public override int MaxCharCount
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public PythonDecoderFallback(string encoding, string data, object callable)
		{
			function = callable;
			str = data;
			enc = encoding;
		}

		public override DecoderFallbackBuffer CreateFallbackBuffer()
		{
			return new PythonDecoderFallbackBuffer(enc, str, function);
		}
	}

	private class BackslashEncoderReplaceFallback : EncoderFallback
	{
		private class BackslashReplaceFallbackBuffer : EncoderFallbackBuffer
		{
			private List<char> _buffer = new List<char>();

			private int _index;

			public override int Remaining => _buffer.Count - _index;

			public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
			{
				return false;
			}

			public override bool Fallback(char charUnknown, int index)
			{
				_buffer.Add('\\');
				if (charUnknown > 'ÿ')
				{
					_buffer.Add('u');
					AddCharacter((int)charUnknown >> 8);
					AddCharacter(charUnknown & 0xFF);
				}
				else
				{
					_buffer.Add('x');
					AddCharacter(charUnknown);
				}
				return true;
			}

			private void AddCharacter(int val)
			{
				AddOneDigit((val & 0xF0) >> 4);
				AddOneDigit(val & 0xF);
			}

			private void AddOneDigit(int val)
			{
				if (val > 9)
				{
					_buffer.Add((char)(97 + val - 10));
				}
				else
				{
					_buffer.Add((char)(48 + val));
				}
			}

			public override char GetNextChar()
			{
				if (_index == _buffer.Count)
				{
					return '\0';
				}
				return _buffer[_index++];
			}

			public override bool MovePrevious()
			{
				if (_index > 0)
				{
					_index--;
					return true;
				}
				return false;
			}
		}

		public override int MaxCharCount
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override EncoderFallbackBuffer CreateFallbackBuffer()
		{
			return new BackslashReplaceFallbackBuffer();
		}
	}

	private class XmlCharRefEncoderReplaceFallback : EncoderFallback
	{
		private class XmlCharRefEncoderReplaceFallbackBuffer : EncoderFallbackBuffer
		{
			private List<char> _buffer = new List<char>();

			private int _index;

			public override int Remaining => _buffer.Count - _index;

			public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
			{
				return false;
			}

			public override bool Fallback(char charUnknown, int index)
			{
				_buffer.Add('&');
				_buffer.Add('#');
				int num = charUnknown;
				string text = num.ToString();
				foreach (char item in text)
				{
					_buffer.Add(item);
				}
				_buffer.Add(';');
				return true;
			}

			public override char GetNextChar()
			{
				if (_index == _buffer.Count)
				{
					return '\0';
				}
				return _buffer[_index++];
			}

			public override bool MovePrevious()
			{
				if (_index > 0)
				{
					_index--;
					return true;
				}
				return false;
			}
		}

		public override int MaxCharCount
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override EncoderFallbackBuffer CreateFallbackBuffer()
		{
			return new XmlCharRefEncoderReplaceFallbackBuffer();
		}
	}

	private class UnicodeEscapeEncoding : Encoding
	{
		private bool _raw;

		public UnicodeEscapeEncoding(bool raw)
		{
			_raw = raw;
		}

		public override int GetByteCount(char[] chars, int index, int count)
		{
			return EscapeEncode(chars, index, count).Length;
		}

		private string EscapeEncode(char[] chars, int index, int count)
		{
			if (_raw)
			{
				return RawUnicodeEscapeEncode(new string(chars, index, count));
			}
			bool isUnicode = false;
			return ReprEncode(new string(chars, index, count), ref isUnicode);
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			if (_raw)
			{
				string text = RawUnicodeEscapeEncode(new string(chars, charIndex, charCount));
				for (int i = 0; i < text.Length; i++)
				{
					bytes[i + byteIndex] = (_raw ? ((byte)text[i]) : ((byte)chars[i]));
				}
				return text.Length;
			}
			for (int j = 0; j < charCount; j++)
			{
				bytes[j + byteIndex] = (byte)chars[j + charIndex];
			}
			return charCount;
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			char[] array = new char[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = (char)bytes[i + index];
			}
			return LiteralParser.ParseString(array, 0, array.Length, _raw, isUni: true, normalizeLineEndings: false).Length;
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			char[] array = new char[byteCount];
			for (int i = 0; i < byteCount; i++)
			{
				chars[i] = (char)bytes[i + byteIndex];
			}
			string text = LiteralParser.ParseString(array, 0, array.Length, _raw, isUni: true, normalizeLineEndings: false);
			for (int j = 0; j < text.Length; j++)
			{
				chars[j + charIndex] = text[j];
			}
			return text.Length;
		}

		public override int GetMaxByteCount(int charCount)
		{
			return charCount * 5;
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return byteCount;
		}
	}

	internal const int LowestUnicodeValue = 127;

	private static readonly char[] Whitespace = new char[5] { ' ', '\t', '\n', '\r', '\f' };

	internal static object FastNew(CodeContext context, object x)
	{
		if (x == null)
		{
			return "None";
		}
		if (x is string)
		{
			return CheckAsciiString(context, (string)x);
		}
		object obj = PythonContext.InvokeUnaryOperator(context, UnaryOperators.String, x);
		if (obj is string || obj is Extensible<string>)
		{
			return obj;
		}
		throw PythonOps.TypeError("expected str, got {0} from __str__", DynamicHelpers.GetPythonType(obj).Name);
	}

	internal static string FastNewUnicode(CodeContext context, object value, object encoding, object errors)
	{
		if (!(errors is string errors2))
		{
			throw PythonOps.TypeError("unicode() argument 3 must be string, not {0}", PythonTypeOps.GetName(errors));
		}
		if (value != null)
		{
			if (value is string s)
			{
				return RawDecode(context, s, encoding, errors2);
			}
			if (value is Extensible<string> extensible)
			{
				return RawDecode(context, extensible.Value, encoding, errors2);
			}
			if (value is Bytes bytes)
			{
				return RawDecode(context, bytes.ToString(), encoding, errors2);
			}
			if (value is PythonBuffer pythonBuffer)
			{
				return RawDecode(context, pythonBuffer.ToString(), encoding, errors2);
			}
		}
		throw PythonOps.TypeError("coercing to Unicode: need string or buffer, {0} found", PythonTypeOps.GetName(value));
	}

	internal static object FastNewUnicode(CodeContext context, object value, object encoding)
	{
		return FastNewUnicode(context, value, encoding, "strict");
	}

	internal static object FastNewUnicode(CodeContext context, object value)
	{
		if (value == null)
		{
			return "None";
		}
		if (value is string)
		{
			return value;
		}
		if (value is OldInstance oldInstance && (oldInstance.TryGetBoundCustomMember(context, "__unicode__", out var value2) || oldInstance.TryGetBoundCustomMember(context, "__str__", out value2)))
		{
			value2 = context.LanguageContext.Call(context, value2);
			if (value2 is string || value2 is Extensible<string>)
			{
				return value2;
			}
			throw PythonOps.TypeError("coercing to Unicode: expected string, got {0}", PythonTypeOps.GetName(value));
		}
		if (PythonTypeOps.TryInvokeUnaryOperator(context, value, "__unicode__", out value2) || PythonTypeOps.TryInvokeUnaryOperator(context, value, "__str__", out value2))
		{
			if (value2 is string || value2 is Extensible<string>)
			{
				return value2;
			}
			throw PythonOps.TypeError("coercing to Unicode: expected string, got {0}", PythonTypeOps.GetName(value));
		}
		return FastNewUnicode(context, value, context.LanguageContext.DefaultEncoding.WebName, "strict");
	}

	private static object CheckAsciiString(CodeContext context, string s)
	{
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] > '\u0080')
			{
				return __new__(context, DynamicHelpers.GetPythonTypeFromType(typeof(string)), s, null, "strict");
			}
		}
		return s;
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls)
	{
		if (cls == TypeCache.String)
		{
			return "";
		}
		return cls.CreateInstance(context);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, object @object)
	{
		if (cls == TypeCache.String)
		{
			return FastNew(context, @object);
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, [NotNull] string @object)
	{
		if (cls == TypeCache.String)
		{
			return CheckAsciiString(context, @object);
		}
		return cls.CreateInstance(context, @object);
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, [NotNull] ExtensibleString @object)
	{
		if (cls == TypeCache.String)
		{
			return FastNew(context, @object);
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, char @object)
	{
		if (cls == TypeCache.String)
		{
			return CheckAsciiString(context, ScriptingRuntimeHelpers.CharToString(@object));
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, [NotNull] BigInteger @object)
	{
		if (cls == TypeCache.String)
		{
			return @object.ToString();
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, [NotNull] Extensible<BigInteger> @object)
	{
		if (cls == TypeCache.String)
		{
			return FastNew(context, @object);
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, int @object)
	{
		if (cls == TypeCache.String)
		{
			return @object.ToString();
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, bool @object)
	{
		if (cls == TypeCache.String)
		{
			return @object.ToString();
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, double @object)
	{
		if (cls == TypeCache.String)
		{
			return DoubleOps.__str__(context, @object);
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, Extensible<double> @object)
	{
		if (cls == TypeCache.String)
		{
			return FastNew(context, @object);
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, float @object)
	{
		if (cls == TypeCache.String)
		{
			return SingleOps.__str__(context, @object);
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, @object));
	}

	[StaticExtensionMethod]
	public static object __new__(CodeContext context, PythonType cls, object @string, [DefaultParameterValue(null)] string encoding, [DefaultParameterValue("strict")] string errors)
	{
		if (!(@string is string text))
		{
			throw PythonOps.TypeError("converting to unicode: need string, got {0}", DynamicHelpers.GetPythonType(@string).Name);
		}
		if (cls == TypeCache.String)
		{
			return decode(context, text, encoding ?? PythonContext.GetContext(context).GetDefaultEncodingName(), errors);
		}
		return cls.CreateInstance(context, __new__(context, TypeCache.String, text, encoding, errors));
	}

	public static bool __contains__(string s, string item)
	{
		return s.Contains(item);
	}

	public static bool __contains__(string s, char item)
	{
		return s.IndexOf(item) != -1;
	}

	public static string __format__(CodeContext context, string self, string formatSpec)
	{
		return ObjectOps.__format__(context, self, formatSpec);
	}

	public static int __len__(string s)
	{
		return s.Length;
	}

	[SpecialName]
	public static string GetItem(string s, int index)
	{
		return ScriptingRuntimeHelpers.CharToString(s[PythonOps.FixIndex(index, s.Length)]);
	}

	[SpecialName]
	public static string GetItem(string s, object index)
	{
		return GetItem(s, Converter.ConvertToIndex(index));
	}

	[SpecialName]
	public static string GetItem(string s, Slice slice)
	{
		if (slice == null)
		{
			throw PythonOps.TypeError("string indices must be slices or integers");
		}
		slice.indices(s.Length, out var ostart, out var ostop, out var ostep);
		if (ostep == 1)
		{
			if (ostop <= ostart)
			{
				return string.Empty;
			}
			return s.Substring(ostart, ostop - ostart);
		}
		int num = 0;
		char[] array;
		if (ostep > 0)
		{
			if (ostart > ostop)
			{
				return string.Empty;
			}
			int num2 = (ostop - ostart + ostep - 1) / ostep;
			array = new char[num2];
			for (int i = ostart; i < ostop; i += ostep)
			{
				array[num++] = s[i];
			}
		}
		else
		{
			if (ostart < ostop)
			{
				return string.Empty;
			}
			int num3 = (ostop - ostart + ostep + 1) / ostep;
			array = new char[num3];
			for (int j = ostart; j > ostop; j += ostep)
			{
				array[num++] = s[j];
			}
		}
		return new string(array);
	}

	public static string __getslice__(string self, int x, int y)
	{
		Slice.FixSliceArguments(self.Length, ref x, ref y);
		if (x >= y)
		{
			return string.Empty;
		}
		return self.Substring(x, y - x);
	}

	public static string capitalize(this string self)
	{
		if (self.Length == 0)
		{
			return self;
		}
		return char.ToUpper(self[0], CultureInfo.InvariantCulture) + self.Substring(1).ToLower(CultureInfo.InvariantCulture);
	}

	public static string center(this string self, int width)
	{
		return self.center(width, ' ');
	}

	public static string center(this string self, int width, char fillchar)
	{
		int num = width - self.Length;
		if (num <= 0)
		{
			return self;
		}
		StringBuilder stringBuilder = new StringBuilder(width);
		stringBuilder.Append(fillchar, num / 2);
		stringBuilder.Append(self);
		stringBuilder.Append(fillchar, (num + 1) / 2);
		return stringBuilder.ToString();
	}

	public static int count(this string self, string sub)
	{
		return self.count(sub, 0, self.Length);
	}

	public static int count(this string self, string sub, int start)
	{
		return self.count(sub, start, self.Length);
	}

	public static int count(this string self, string ssub, int start, int end)
	{
		if (ssub == null)
		{
			throw PythonOps.TypeError("expected string for 'sub' argument, got NoneType");
		}
		if (start > self.Length)
		{
			return 0;
		}
		start = PythonOps.FixSliceIndex(start, self.Length);
		end = PythonOps.FixSliceIndex(end, self.Length);
		if (ssub.Length == 0)
		{
			return Math.Max(end - start + 1, 0);
		}
		int num = 0;
		CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
		while (end > start)
		{
			int num2 = compareInfo.IndexOf(self, ssub, start, end - start, CompareOptions.Ordinal);
			if (num2 == -1)
			{
				break;
			}
			num++;
			start = num2 + ssub.Length;
		}
		return num;
	}

	public static string decode(CodeContext context, string s)
	{
		return decode(context, s, Missing.Value, "strict");
	}

	public static string decode(CodeContext context, string s, [Optional] object encoding, [DefaultParameterValue("strict")] string errors)
	{
		return RawDecode(context, s, encoding, errors);
	}

	public static string encode(CodeContext context, string s, [Optional] object encoding, [DefaultParameterValue("strict")] string errors)
	{
		return RawEncode(context, s, encoding, errors);
	}

	private static string CastString(object o)
	{
		if (o is string result)
		{
			return result;
		}
		return ((Extensible<string>)o).Value;
	}

	internal static string AsString(object o)
	{
		if (o is string result)
		{
			return result;
		}
		if (o is Extensible<string> extensible)
		{
			return extensible.Value;
		}
		return null;
	}

	public static bool endswith(this string self, object suffix)
	{
		TryStringOrTuple(suffix);
		if (suffix is PythonTuple)
		{
			return endswith(self, (PythonTuple)suffix);
		}
		return endswith(self, CastString(suffix));
	}

	public static bool endswith(this string self, object suffix, int start)
	{
		TryStringOrTuple(suffix);
		if (suffix is PythonTuple)
		{
			return endswith(self, (PythonTuple)suffix, start);
		}
		return endswith(self, CastString(suffix), start);
	}

	public static bool endswith(this string self, object suffix, int start, int end)
	{
		TryStringOrTuple(suffix);
		if (suffix is PythonTuple)
		{
			return endswith(self, (PythonTuple)suffix, start, end);
		}
		return endswith(self, CastString(suffix), start, end);
	}

	public static string expandtabs(string self)
	{
		return self.expandtabs(8);
	}

	public static string expandtabs(this string self, int tabsize)
	{
		StringBuilder stringBuilder = new StringBuilder(self.Length * 2);
		int num = 0;
		foreach (char c in self)
		{
			switch (c)
			{
			case '\n':
			case '\r':
				num = 0;
				stringBuilder.Append(c);
				break;
			case '\t':
				if (tabsize > 0)
				{
					int num2 = tabsize - num % tabsize;
					int capacity = stringBuilder.Capacity;
					stringBuilder.Capacity = checked(capacity + num2);
					stringBuilder.Append(' ', num2);
					num = 0;
				}
				break;
			default:
				num++;
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public static int find(this string self, string sub)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (sub.Length == 1)
		{
			return self.IndexOf(sub[0]);
		}
		CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
		return compareInfo.IndexOf(self, sub, CompareOptions.Ordinal);
	}

	public static int find(this string self, string sub, int start)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > self.Length)
		{
			return -1;
		}
		start = PythonOps.FixSliceIndex(start, self.Length);
		CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
		return compareInfo.IndexOf(self, sub, start, CompareOptions.Ordinal);
	}

	public static int find(this string self, string sub, BigInteger start)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > self.Length)
		{
			return -1;
		}
		return self.find(sub, (int)start);
	}

	public static int find(this string self, string sub, int start, int end)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > self.Length)
		{
			return -1;
		}
		start = PythonOps.FixSliceIndex(start, self.Length);
		end = PythonOps.FixSliceIndex(end, self.Length);
		if (end < start)
		{
			return -1;
		}
		CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
		return compareInfo.IndexOf(self, sub, start, end - start, CompareOptions.Ordinal);
	}

	public static int find(this string self, string sub, BigInteger start, BigInteger end)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > self.Length)
		{
			return -1;
		}
		return self.find(sub, (int)start, (int)end);
	}

	public static int find(this string self, string sub, object start, [DefaultParameterValue(null)] object end)
	{
		return self.find(sub, CheckIndex(start, 0), CheckIndex(end, self.Length));
	}

	public static int index(this string self, string sub)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		return self.index(sub, 0, self.Length);
	}

	public static int index(this string self, string sub, int start)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		return self.index(sub, start, self.Length);
	}

	public static int index(this string self, string sub, int start, int end)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		int num = self.find(sub, start, end);
		if (num == -1)
		{
			throw PythonOps.ValueError("substring {0} not found in {1}", sub, self);
		}
		return num;
	}

	public static int index(this string self, string sub, object start, [DefaultParameterValue(null)] object end)
	{
		return self.index(sub, CheckIndex(start, 0), CheckIndex(end, self.Length));
	}

	public static bool isalnum(this string self)
	{
		if (self.Length == 0)
		{
			return false;
		}
		for (int num = self.Length - 1; num >= 0; num--)
		{
			if (!char.IsLetterOrDigit(self, num))
			{
				return false;
			}
		}
		return true;
	}

	public static bool isalpha(this string self)
	{
		if (self.Length == 0)
		{
			return false;
		}
		for (int num = self.Length - 1; num >= 0; num--)
		{
			if (!char.IsLetter(self, num))
			{
				return false;
			}
		}
		return true;
	}

	public static bool isdigit(this string self)
	{
		if (self.Length == 0)
		{
			return false;
		}
		for (int num = self.Length - 1; num >= 0; num--)
		{
			if (!char.IsDigit(self, num) && (self[num] < '①' || self[num] > '⑨'))
			{
				return false;
			}
		}
		return true;
	}

	public static bool isspace(this string self)
	{
		if (self.Length == 0)
		{
			return false;
		}
		for (int num = self.Length - 1; num >= 0; num--)
		{
			if (!char.IsWhiteSpace(self, num))
			{
				return false;
			}
		}
		return true;
	}

	public static bool isdecimal(this string self)
	{
		return self.isnumeric();
	}

	public static bool isnumeric(this string self)
	{
		if (string.IsNullOrEmpty(self))
		{
			return false;
		}
		foreach (char c in self)
		{
			if (!char.IsDigit(c))
			{
				return false;
			}
		}
		return true;
	}

	public static bool islower(this string self)
	{
		if (self.Length == 0)
		{
			return false;
		}
		bool flag = false;
		for (int num = self.Length - 1; num >= 0; num--)
		{
			if (!flag && char.IsLower(self, num))
			{
				flag = true;
			}
			if (char.IsUpper(self, num))
			{
				return false;
			}
		}
		return flag;
	}

	public static bool isupper(this string self)
	{
		if (self.Length == 0)
		{
			return false;
		}
		bool flag = false;
		for (int num = self.Length - 1; num >= 0; num--)
		{
			if (!flag && char.IsUpper(self, num))
			{
				flag = true;
			}
			if (char.IsLower(self, num))
			{
				return false;
			}
		}
		return flag;
	}

	public static bool istitle(this string self)
	{
		if (self == null || self.Length == 0)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool result = false;
		for (int i = 0; i < self.Length; i++)
		{
			if (char.IsUpper(self, i) || char.GetUnicodeCategory(self, i) == UnicodeCategory.TitlecaseLetter)
			{
				result = true;
				if (flag)
				{
					return false;
				}
				flag2 = true;
			}
			else if (char.IsLower(self, i))
			{
				if (!flag)
				{
					return false;
				}
				flag2 = true;
			}
			else
			{
				flag2 = false;
			}
			flag = flag2;
		}
		return result;
	}

	public static bool isunicode(this string self)
	{
		foreach (char c in self)
		{
			if (c >= '\u007f')
			{
				return true;
			}
		}
		return false;
	}

	public static string join(this string self, object sequence)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(sequence);
		if (!enumerator.MoveNext())
		{
			return "";
		}
		object current = enumerator.Current;
		if (!enumerator.MoveNext())
		{
			return Converter.ConvertToString(current);
		}
		StringBuilder stringBuilder = new StringBuilder();
		AppendJoin(current, 0, stringBuilder);
		int num = 1;
		do
		{
			stringBuilder.Append(self);
			AppendJoin(enumerator.Current, num, stringBuilder);
			num++;
		}
		while (enumerator.MoveNext());
		return stringBuilder.ToString();
	}

	public static string join(this string self, [NotNull] List sequence)
	{
		if (sequence.__len__() == 0)
		{
			return string.Empty;
		}
		lock (sequence)
		{
			if (sequence.__len__() == 1)
			{
				return Converter.ConvertToString(sequence[0]);
			}
			StringBuilder stringBuilder = new StringBuilder();
			AppendJoin(sequence._data[0], 0, stringBuilder);
			for (int i = 1; i < sequence._size; i++)
			{
				if (!string.IsNullOrEmpty(self))
				{
					stringBuilder.Append(self);
				}
				AppendJoin(sequence._data[i], i, stringBuilder);
			}
			return stringBuilder.ToString();
		}
	}

	public static string ljust(this string self, int width)
	{
		return self.ljust(width, ' ');
	}

	public static string ljust(this string self, int width, char fillchar)
	{
		if (width < 0)
		{
			return self;
		}
		int num = width - self.Length;
		if (num <= 0)
		{
			return self;
		}
		StringBuilder stringBuilder = new StringBuilder(width);
		stringBuilder.Append(self);
		stringBuilder.Append(fillchar, num);
		return stringBuilder.ToString();
	}

	public static string lower(this string self)
	{
		for (int i = 0; i < self.Length; i++)
		{
			if (self[i] >= 'A' && self[i] <= 'Z')
			{
				return self.ToLower(CultureInfo.InvariantCulture);
			}
		}
		return self;
	}

	public static string lstrip(this string self)
	{
		return self.TrimStart(Whitespace);
	}

	public static string lstrip(this string self, string chars)
	{
		if (chars == null)
		{
			return self.lstrip();
		}
		return self.TrimStart(chars.ToCharArray());
	}

	[return: SequenceTypeInfo(new Type[] { typeof(string) })]
	public static PythonTuple partition(this string self, string sep)
	{
		if (sep == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (sep.Length == 0)
		{
			throw PythonOps.ValueError("empty separator");
		}
		object[] array = new object[3] { "", "", "" };
		if (self.Length != 0)
		{
			int num = self.find(sep);
			if (num == -1)
			{
				array[0] = self;
			}
			else
			{
				array[0] = self.Substring(0, num);
				array[1] = sep;
				array[2] = self.Substring(num + sep.Length, self.Length - num - sep.Length);
			}
		}
		return new PythonTuple(array);
	}

	private static string StringOrBuffer(object input)
	{
		if (input is string result)
		{
			return result;
		}
		if (input is PythonBuffer pythonBuffer)
		{
			return pythonBuffer.ToString();
		}
		throw PythonOps.TypeError("expected a character buffer object");
	}

	public static string replace(this string self, object old, object new_, [DefaultParameterValue(-1)] int maxsplit)
	{
		string text = StringOrBuffer(old);
		string text2 = StringOrBuffer(new_);
		if (text.Length == 0)
		{
			return ReplaceEmpty(self, text2, maxsplit);
		}
		int num = self.count(text);
		num = ((maxsplit < 0 || maxsplit > num) ? num : maxsplit);
		int length = self.Length;
		length -= num * text.Length;
		length = checked(length + num * text2.Length);
		StringBuilder stringBuilder = new StringBuilder(length);
		int num2 = 0;
		int num3;
		while (maxsplit != 0 && (num3 = self.IndexOf(text, num2, StringComparison.Ordinal)) != -1)
		{
			stringBuilder.Append(self, num2, num3 - num2);
			stringBuilder.Append(text2);
			num2 = num3 + text.Length;
			maxsplit--;
		}
		stringBuilder.Append(self.Substring(num2));
		return stringBuilder.ToString();
	}

	public static int rfind(this string self, string sub)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		return self.rfind(sub, 0, self.Length);
	}

	public static int rfind(this string self, string sub, int start)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > self.Length)
		{
			return -1;
		}
		return self.rfind(sub, start, self.Length);
	}

	public static int rfind(this string self, string sub, BigInteger start)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > self.Length)
		{
			return -1;
		}
		return self.rfind(sub, (int)start, self.Length);
	}

	public static int rfind(this string self, string sub, int start, int end)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > self.Length)
		{
			return -1;
		}
		start = PythonOps.FixSliceIndex(start, self.Length);
		end = PythonOps.FixSliceIndex(end, self.Length);
		if (start > end)
		{
			return -1;
		}
		if (sub.Length == 0)
		{
			return end;
		}
		if (end == 0)
		{
			return -1;
		}
		CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
		return compareInfo.LastIndexOf(self, sub, end - 1, end - start, CompareOptions.Ordinal);
	}

	public static int rfind(this string self, string sub, BigInteger start, BigInteger end)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > self.Length)
		{
			return -1;
		}
		return self.rfind(sub, (int)start, (int)end);
	}

	public static int rfind(this string self, string sub, object start, [DefaultParameterValue(null)] object end)
	{
		return self.rfind(sub, CheckIndex(start, 0), CheckIndex(end, self.Length));
	}

	public static int rindex(this string self, string sub)
	{
		return self.rindex(sub, 0, self.Length);
	}

	public static int rindex(this string self, string sub, int start)
	{
		return self.rindex(sub, start, self.Length);
	}

	public static int rindex(this string self, string sub, int start, int end)
	{
		int num = self.rfind(sub, start, end);
		if (num == -1)
		{
			throw PythonOps.ValueError("substring {0} not found in {1}", sub, self);
		}
		return num;
	}

	public static int rindex(this string self, string sub, object start, [DefaultParameterValue(null)] object end)
	{
		return self.rindex(sub, CheckIndex(start, 0), CheckIndex(end, self.Length));
	}

	public static string rjust(this string self, int width)
	{
		return self.rjust(width, ' ');
	}

	public static string rjust(this string self, int width, char fillchar)
	{
		int num = width - self.Length;
		if (num <= 0)
		{
			return self;
		}
		StringBuilder stringBuilder = new StringBuilder(width);
		stringBuilder.Append(fillchar, num);
		stringBuilder.Append(self);
		return stringBuilder.ToString();
	}

	[return: SequenceTypeInfo(new Type[] { typeof(string) })]
	public static PythonTuple rpartition(this string self, string sep)
	{
		if (sep == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (sep.Length == 0)
		{
			throw PythonOps.ValueError("empty separator");
		}
		object[] array = new object[3] { "", "", "" };
		if (self.Length != 0)
		{
			int num = self.rfind(sep);
			if (num == -1)
			{
				array[2] = self;
			}
			else
			{
				array[0] = self.Substring(0, num);
				array[1] = sep;
				array[2] = self.Substring(num + sep.Length, self.Length - num - sep.Length);
			}
		}
		return new PythonTuple(array);
	}

	public static List rsplit(this string self)
	{
		return SplitInternal(self, (char[])null, -1);
	}

	public static List rsplit(this string self, string sep)
	{
		return self.rsplit(sep, -1);
	}

	public static List rsplit(this string self, string sep, int maxsplit)
	{
		string self2 = Reverse(self);
		if (sep != null)
		{
			sep = Reverse(sep);
		}
		List list = null;
		List list2 = null;
		list = self2.split(sep, maxsplit);
		list.reverse();
		int num = list.__len__();
		if (num != 0)
		{
			list2 = new List(num);
			foreach (string item in list)
			{
				list2.AddNoLock(Reverse(item));
			}
		}
		else
		{
			list2 = list;
		}
		return list2;
	}

	public static string rstrip(this string self)
	{
		return self.TrimEnd(Whitespace);
	}

	public static string rstrip(this string self, string chars)
	{
		if (chars == null)
		{
			return self.rstrip();
		}
		return self.TrimEnd(chars.ToCharArray());
	}

	public static List split(this string self)
	{
		return SplitInternal(self, (char[])null, -1);
	}

	public static List split(this string self, string sep)
	{
		return self.split(sep, -1);
	}

	public static List split(this string self, string sep, int maxsplit)
	{
		if (sep == null)
		{
			if (maxsplit == 0)
			{
				List list = PythonOps.MakeEmptyList(1);
				list.AddNoLock(self.TrimStart());
				return list;
			}
			return SplitInternal(self, (char[])null, maxsplit);
		}
		if (sep.Length == 0)
		{
			throw PythonOps.ValueError("empty separator");
		}
		if (sep.Length == 1)
		{
			return SplitInternal(self, new char[1] { sep[0] }, maxsplit);
		}
		return SplitInternal(self, sep, maxsplit);
	}

	public static List splitlines(this string self)
	{
		return self.splitlines(keepends: false);
	}

	public static List splitlines(this string self, bool keepends)
	{
		List list = new List();
		int i = 0;
		int num = 0;
		for (; i < self.Length; i++)
		{
			if (self[i] != '\n' && self[i] != '\r' && self[i] != '\u2028')
			{
				continue;
			}
			if (i < self.Length - 1 && self[i] == '\r' && self[i + 1] == '\n')
			{
				if (keepends)
				{
					list.AddNoLock(self.Substring(num, i - num + 2));
				}
				else
				{
					list.AddNoLock(self.Substring(num, i - num));
				}
				num = i + 2;
				i++;
			}
			else
			{
				if (keepends)
				{
					list.AddNoLock(self.Substring(num, i - num + 1));
				}
				else
				{
					list.AddNoLock(self.Substring(num, i - num));
				}
				num = i + 1;
			}
		}
		if (i - num != 0)
		{
			list.AddNoLock(self.Substring(num, i - num));
		}
		return list;
	}

	public static bool startswith(this string self, object prefix)
	{
		TryStringOrTuple(prefix);
		if (prefix is PythonTuple)
		{
			return startswith(self, (PythonTuple)prefix);
		}
		return startswith(self, CastString(prefix));
	}

	public static bool startswith(this string self, object prefix, int start)
	{
		TryStringOrTuple(prefix);
		if (prefix is PythonTuple)
		{
			return startswith(self, (PythonTuple)prefix, start);
		}
		return startswith(self, CastString(prefix), start);
	}

	public static bool startswith(this string self, object prefix, int start, int end)
	{
		TryStringOrTuple(prefix);
		if (prefix is PythonTuple)
		{
			return startswith(self, (PythonTuple)prefix, start, end);
		}
		return startswith(self, CastString(prefix), start, end);
	}

	public static string strip(this string self)
	{
		return self.Trim();
	}

	public static string strip(this string self, string chars)
	{
		if (chars == null)
		{
			return self.strip();
		}
		return self.Trim(chars.ToCharArray());
	}

	public static string swapcase(this string self)
	{
		StringBuilder stringBuilder = new StringBuilder(self);
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			char c = stringBuilder[i];
			if (char.IsUpper(c))
			{
				stringBuilder[i] = char.ToLower(c, CultureInfo.InvariantCulture);
			}
			else if (char.IsLower(c))
			{
				stringBuilder[i] = char.ToUpper(c, CultureInfo.InvariantCulture);
			}
		}
		return stringBuilder.ToString();
	}

	public static string title(this string self)
	{
		if (self == null || self.Length == 0)
		{
			return self;
		}
		char[] array = self.ToCharArray();
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		do
		{
			if (char.IsUpper(array[num]) || char.IsLower(array[num]))
			{
				if (!flag)
				{
					array[num] = char.ToUpper(array[num], CultureInfo.InvariantCulture);
				}
				else
				{
					array[num] = char.ToLower(array[num], CultureInfo.InvariantCulture);
				}
				flag2 = true;
			}
			else
			{
				flag2 = false;
			}
			num++;
			flag = flag2;
		}
		while (num < array.Length);
		return new string(array);
	}

	public static string translate(this string self, [NotNull] PythonDictionary table)
	{
		if (table == null || self.Length == 0)
		{
			return self;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		int num = 0;
		for (; i < self.Length; i++)
		{
			num = self[i];
			if (table.__contains__(num))
			{
				stringBuilder.Append((string)table[num]);
			}
			else
			{
				stringBuilder.Append(self[i]);
			}
		}
		return stringBuilder.ToString();
	}

	public static string translate(this string self, string table)
	{
		return self.translate(table, null);
	}

	public static string translate(this string self, string table, string deletechars)
	{
		if (table != null && table.Length != 256)
		{
			throw PythonOps.ValueError("translation table must be 256 characters long");
		}
		if (self.Length == 0)
		{
			return self;
		}
		List<char> list = new List<char>();
		for (int i = 0; i < self.Length; i++)
		{
			if (deletechars != null && deletechars.Contains(char.ToString(self[i])))
			{
				continue;
			}
			if (table != null)
			{
				int num = self[i];
				if (num >= 0 && num < 256)
				{
					list.Add(table[num]);
				}
			}
			else
			{
				list.Add(self[i]);
			}
		}
		return new string(list.ToArray());
	}

	public static string upper(this string self)
	{
		return self.ToUpper(CultureInfo.InvariantCulture);
	}

	public static string zfill(this string self, int width)
	{
		int num = width - self.Length;
		if (num <= 0)
		{
			return self;
		}
		StringBuilder stringBuilder = new StringBuilder(width);
		if (self.Length > 0 && IsSign(self[0]))
		{
			stringBuilder.Append(self[0]);
			stringBuilder.Append('0', num);
			stringBuilder.Append(self.Substring(1));
		}
		else
		{
			stringBuilder.Append('0', num);
			stringBuilder.Append(self);
		}
		return stringBuilder.ToString();
	}

	public static string format(CodeContext context, string format_string, [NotNull] params object[] args)
	{
		return NewStringFormatter.FormatString(PythonContext.GetContext(context), format_string, PythonTuple.MakeTuple(args), new PythonDictionary());
	}

	public static string format(CodeContext context, string format_string, [ParamDictionary] IDictionary<object, object> kwargs, params object[] args)
	{
		return NewStringFormatter.FormatString(PythonContext.GetContext(context), format_string, PythonTuple.MakeTuple(args), kwargs);
	}

	public static IEnumerable<PythonTuple> _formatter_parser(this string self)
	{
		return NewStringFormatter.GetFormatInfo(self);
	}

	public static PythonTuple _formatter_field_name_split(this string self)
	{
		return NewStringFormatter.GetFieldNameInfo(self);
	}

	[SpecialName]
	public static string Add([NotNull] string self, [NotNull] string other)
	{
		return self + other;
	}

	[SpecialName]
	public static string Add([NotNull] string self, char other)
	{
		return self + other;
	}

	[SpecialName]
	public static string Add(char self, [NotNull] string other)
	{
		return self + other;
	}

	[SpecialName]
	public static string Mod(CodeContext context, string self, object other)
	{
		return new StringFormatter(context, self, other).Format();
	}

	[SpecialName]
	[return: MaybeNotImplemented]
	public static object Mod(CodeContext context, object other, string self)
	{
		if (other is string str)
		{
			return new StringFormatter(context, str, self).Format();
		}
		if (other is Extensible<string> extensible)
		{
			return new StringFormatter(context, extensible.Value, self).Format();
		}
		return NotImplementedType.Value;
	}

	[SpecialName]
	public static string Multiply(string s, int count)
	{
		if (count <= 0)
		{
			return string.Empty;
		}
		if (count == 1)
		{
			return s;
		}
		long num = (long)s.Length * (long)count;
		if (num > int.MaxValue)
		{
			throw PythonOps.OverflowError("repeated string is too long");
		}
		int length = s.Length;
		if (length == 1)
		{
			return new string(s[0], count);
		}
		StringBuilder stringBuilder = new StringBuilder(length * count);
		stringBuilder.Insert(0, s, count);
		return stringBuilder.ToString();
	}

	[SpecialName]
	public static string Multiply(int other, string self)
	{
		return Multiply(self, other);
	}

	[SpecialName]
	public static object Multiply(string self, [NotNull] Index count)
	{
		return PythonOps.MultiplySequence(Multiply, self, count, isForward: true);
	}

	[SpecialName]
	public static object Multiply([NotNull] Index count, string self)
	{
		return PythonOps.MultiplySequence(Multiply, self, count, isForward: false);
	}

	[SpecialName]
	public static object Multiply(string self, object count)
	{
		if (Converter.TryConvertToIndex(count, out int num))
		{
			return Multiply(self, num);
		}
		throw PythonOps.TypeErrorForUnIndexableObject(count);
	}

	[SpecialName]
	public static object Multiply(object count, string self)
	{
		if (Converter.TryConvertToIndex(count, out int other))
		{
			return Multiply(other, self);
		}
		throw PythonOps.TypeErrorForUnIndexableObject(count);
	}

	[SpecialName]
	public static bool GreaterThan(string x, string y)
	{
		return string.CompareOrdinal(x, y) > 0;
	}

	[SpecialName]
	public static bool LessThan(string x, string y)
	{
		return string.CompareOrdinal(x, y) < 0;
	}

	[SpecialName]
	public static bool LessThanOrEqual(string x, string y)
	{
		return string.CompareOrdinal(x, y) <= 0;
	}

	[SpecialName]
	public static bool GreaterThanOrEqual(string x, string y)
	{
		return string.CompareOrdinal(x, y) >= 0;
	}

	[SpecialName]
	public static bool Equals(string x, string y)
	{
		return string.Equals(x, y);
	}

	[SpecialName]
	public static bool NotEquals(string x, string y)
	{
		return !string.Equals(x, y);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static string ConvertFromChar(char c)
	{
		return ScriptingRuntimeHelpers.CharToString(c);
	}

	[SpecialName]
	[ExplicitConversionMethod]
	public static char ConvertToChar(string s)
	{
		if (s.Length == 1)
		{
			return s[0];
		}
		throw PythonOps.TypeErrorForTypeMismatch("char", s);
	}

	[SpecialName]
	[ImplicitConversionMethod]
	public static IEnumerable ConvertToIEnumerable(string s)
	{
		return new PythonStringEnumerable(s);
	}

	internal static int Compare(string self, string obj)
	{
		int num = string.CompareOrdinal(self, obj);
		if (num != 0)
		{
			if (num >= 0)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public static object __getnewargs__(CodeContext context, string self)
	{
		if (!object.ReferenceEquals(self, null))
		{
			return PythonTuple.MakeTuple(__new__(context, TypeCache.String, (object)self));
		}
		throw PythonOps.TypeErrorForBadInstance("__getnewargs__ requires a 'str' object but received a '{0}'", self);
	}

	public static string __str__(string self)
	{
		return self;
	}

	public static Extensible<string> __str__(ExtensibleString self)
	{
		return self;
	}

	internal static string Quote(string s)
	{
		bool isUnicode = false;
		StringBuilder stringBuilder = new StringBuilder(s.Length + 5);
		char c = '\'';
		if (s.IndexOf('\'') != -1 && s.IndexOf('"') == -1)
		{
			c = '"';
		}
		stringBuilder.Append(c);
		stringBuilder.Append(ReprEncode(s, c, ref isUnicode));
		stringBuilder.Append(c);
		if (isUnicode)
		{
			return "u" + stringBuilder.ToString();
		}
		return stringBuilder.ToString();
	}

	internal static string ReprEncode(string s, ref bool isUnicode)
	{
		return ReprEncode(s, '\0', ref isUnicode);
	}

	internal static bool TryGetEncoding(string name, out Encoding encoding)
	{
		name = NormalizeEncodingName(name);
		if (CodecsInfo.Codecs.TryGetValue(name, out var value))
		{
			encoding = (Encoding)value.GetEncoding().Clone();
			return true;
		}
		encoding = null;
		return false;
	}

	internal static string RawUnicodeEscapeEncode(string s)
	{
		StringBuilder sb = null;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (c > 'ÿ')
			{
				ReprInit(ref sb, s, i);
				sb.AppendFormat("\\u{0:x4}", (int)c);
			}
			else
			{
				sb?.Append(c);
			}
		}
		if (sb == null)
		{
			return s;
		}
		return sb.ToString();
	}

	private static int CheckIndex(object index, int defaultValue)
	{
		if (index == null)
		{
			return defaultValue;
		}
		if (!Converter.TryConvertToIndex(index, out int result))
		{
			throw PythonOps.TypeError("slice indices must be integers or None or have an __index__ method");
		}
		return result;
	}

	private static void AppendJoin(object value, int index, StringBuilder sb)
	{
		string result;
		if ((result = value as string) != null)
		{
			sb.Append(result);
			return;
		}
		if (Converter.TryConvertToString(value, out result) && result != null)
		{
			sb.Append(result);
			return;
		}
		throw PythonOps.TypeError("sequence item {0}: expected string, {1} found", index.ToString(), PythonOps.GetPythonTypeName(value));
	}

	private static string ReplaceEmpty(string self, string new_, int maxsplit)
	{
		if (maxsplit == 0)
		{
			return self;
		}
		if (maxsplit < 0)
		{
			maxsplit = self.Length + 1;
		}
		else if (maxsplit > self.Length + 1)
		{
			maxsplit = checked(self.Length + 1);
		}
		int capacity = checked(self.Length + new_.Length * maxsplit);
		int num = Math.Min(self.Length, maxsplit);
		StringBuilder stringBuilder = new StringBuilder(capacity);
		for (int i = 0; i < num; i++)
		{
			stringBuilder.Append(new_);
			stringBuilder.Append(self[i]);
		}
		if (maxsplit > num)
		{
			stringBuilder.Append(new_);
		}
		else
		{
			stringBuilder.Append(self, num, self.Length - num);
		}
		return stringBuilder.ToString();
	}

	private static string Reverse(string s)
	{
		if (s.Length == 0 || s.Length == 1)
		{
			return s;
		}
		char[] array = new char[s.Length];
		int num = s.Length - 1;
		int num2 = 0;
		while (num >= 0)
		{
			array[num2] = s[num];
			num--;
			num2++;
		}
		return new string(array);
	}

	internal static string ReprEncode(string s, char quote, ref bool isUnicode)
	{
		StringBuilder sb = null;
		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (c >= '\u007f')
			{
				isUnicode = true;
			}
			switch (c)
			{
			case '\\':
				ReprInit(ref sb, s, i);
				sb.Append("\\\\");
				continue;
			case '\t':
				ReprInit(ref sb, s, i);
				sb.Append("\\t");
				continue;
			case '\n':
				ReprInit(ref sb, s, i);
				sb.Append("\\n");
				continue;
			case '\r':
				ReprInit(ref sb, s, i);
				sb.Append("\\r");
				continue;
			}
			if (quote != 0 && c == quote)
			{
				ReprInit(ref sb, s, i);
				sb.Append('\\');
				sb.Append(c);
			}
			else if (c < ' ' || (c >= '\u007f' && c <= 'ÿ'))
			{
				ReprInit(ref sb, s, i);
				sb.AppendFormat("\\x{0:x2}", (int)c);
			}
			else if (c > 'ÿ')
			{
				ReprInit(ref sb, s, i);
				sb.AppendFormat("\\u{0:x4}", (int)c);
			}
			else
			{
				sb?.Append(c);
			}
		}
		if (sb == null)
		{
			return s;
		}
		return sb.ToString();
	}

	private static void ReprInit(ref StringBuilder sb, string s, int c)
	{
		if (sb == null)
		{
			sb = new StringBuilder(s, 0, c, s.Length);
		}
	}

	private static bool IsSign(char ch)
	{
		if (ch != '+')
		{
			return ch == '-';
		}
		return true;
	}

	internal static string GetEncodingName(Encoding encoding)
	{
		string text = null;
		if (encoding.CodePage != 0)
		{
			if (encoding.IsBrowserDisplay)
			{
				text = encoding.WebName;
			}
			if (text == null && encoding.IsMailNewsDisplay)
			{
				text = encoding.HeaderName;
			}
			if (text == null)
			{
				text = "cp" + encoding.CodePage;
			}
		}
		if (text == null)
		{
			text = encoding.EncodingName;
		}
		return NormalizeEncodingName(text);
	}

	internal static string NormalizeEncodingName(string name)
	{
		return name?.ToLower(CultureInfo.InvariantCulture).Replace('-', '_').Replace(' ', '_');
	}

	private static string RawDecode(CodeContext context, string s, object encodingType, string errors)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		Encoding encoding = null;
		string text = encodingType as string;
		if (text == null)
		{
			encoding = encodingType as Encoding;
			if (encoding == null)
			{
				if (encodingType != Missing.Value)
				{
					throw PythonOps.TypeError("decode() expected string, got '{0}'", DynamicHelpers.GetPythonType(encodingType).Name);
				}
				text = context2.GetDefaultEncodingName();
			}
		}
		if (encoding == null)
		{
			string text2 = NormalizeEncodingName(text);
			if ("raw_unicode_escape" == text2)
			{
				return LiteralParser.ParseString(s, isRaw: true, isUni: true);
			}
			if ("unicode_escape" == text2)
			{
				return LiteralParser.ParseString(s, isRaw: false, isUni: true);
			}
			if ("string_escape" == text2)
			{
				return LiteralParser.ParseString(s, isRaw: false, isUni: false);
			}
		}
		if (encoding != null || TryGetEncoding(text, out encoding))
		{
			return DoDecode(context, s, errors, text, encoding);
		}
		PythonTuple pythonTuple = PythonOps.LookupEncoding(context, text);
		if (pythonTuple != null)
		{
			return UserDecodeOrEncode(pythonTuple[1], s);
		}
		throw PythonOps.LookupError("unknown encoding: {0}", text);
	}

	internal static string DoDecode(CodeContext context, string s, string errors, string encoding, Encoding e)
	{
		e = (Encoding)e.Clone();
		switch (errors)
		{
		case "backslashreplace":
		case "xmlcharrefreplace":
		case "strict":
			e.DecoderFallback = DecoderFallback.ExceptionFallback;
			break;
		case "replace":
			e.DecoderFallback = DecoderFallback.ReplacementFallback;
			break;
		case "ignore":
			e.DecoderFallback = new PythonDecoderFallback(encoding, s, null);
			break;
		default:
			e.DecoderFallback = new PythonDecoderFallback(encoding, s, LightExceptions.CheckAndThrow(PythonOps.LookupEncodingError(context, errors)));
			break;
		}
		byte[] array = s.MakeByteArray();
		int startingOffset = GetStartingOffset(e, array);
		return e.GetString(array, startingOffset, array.Length - startingOffset);
	}

	private static int GetStartingOffset(Encoding e, byte[] bytes)
	{
		byte[] preamble = e.GetPreamble();
		int result = 0;
		if (bytes.Length >= preamble.Length)
		{
			bool flag = false;
			for (int i = 0; i < preamble.Length; i++)
			{
				if (bytes[i] != preamble[i])
				{
					flag = true;
				}
			}
			if (!flag)
			{
				result = preamble.Length;
			}
		}
		return result;
	}

	private static string RawEncode(CodeContext context, string s, object encodingType, string errors)
	{
		string text = encodingType as string;
		Encoding encoding = null;
		if (text == null)
		{
			encoding = encodingType as Encoding;
			if (encoding == null)
			{
				if (encodingType != Missing.Value)
				{
					throw PythonOps.TypeError("encode() expected string, got '{0}'", DynamicHelpers.GetPythonType(encodingType).Name);
				}
				text = PythonContext.GetContext(context).GetDefaultEncodingName();
			}
		}
		if (encoding == null)
		{
			string text2 = NormalizeEncodingName(text);
			if ("raw_unicode_escape" == text2)
			{
				return RawUnicodeEscapeEncode(s);
			}
			if ("unicode_escape" == text2 || "string_escape" == text2)
			{
				bool isUnicode = false;
				return ReprEncode(s, '\'', ref isUnicode);
			}
		}
		if (encoding != null || TryGetEncoding(text, out encoding))
		{
			return DoEncode(context, s, errors, text, encoding);
		}
		PythonTuple pythonTuple = PythonOps.LookupEncoding(context, text);
		if (pythonTuple != null)
		{
			return UserDecodeOrEncode(pythonTuple[0], s);
		}
		throw PythonOps.LookupError("unknown encoding: {0}", text);
	}

	internal static string DoEncode(CodeContext context, string s, string errors, string encoding, Encoding e)
	{
		e = (Encoding)e.Clone();
		switch (errors)
		{
		case "strict":
			e.EncoderFallback = EncoderFallback.ExceptionFallback;
			break;
		case "replace":
			e.EncoderFallback = EncoderFallback.ReplacementFallback;
			break;
		case "backslashreplace":
			e.EncoderFallback = new BackslashEncoderReplaceFallback();
			break;
		case "xmlcharrefreplace":
			e.EncoderFallback = new XmlCharRefEncoderReplaceFallback();
			break;
		case "ignore":
			e.EncoderFallback = new PythonEncoderFallback(encoding, s, null);
			break;
		default:
			e.EncoderFallback = new PythonEncoderFallback(encoding, s, LightExceptions.CheckAndThrow(PythonOps.LookupEncodingError(context, errors)));
			break;
		}
		return e.GetPreamble().MakeString(e.GetBytes(s));
	}

	private static string UserDecodeOrEncode(object function, string data)
	{
		object obj = PythonCalls.Call(function, data);
		string text = AsString(obj);
		if (text != null)
		{
			return text;
		}
		if (!(obj is PythonTuple pythonTuple))
		{
			throw PythonOps.TypeErrorForBadInstance("expected tuple, but found {0}", obj);
		}
		return Converter.ConvertToString(pythonTuple[0]);
	}

	private static List SplitEmptyString(bool separators)
	{
		List list = PythonOps.MakeEmptyList(1);
		if (separators)
		{
			list.AddNoLock(string.Empty);
		}
		return list;
	}

	private static List SplitInternal(string self, char[] seps, int maxsplit)
	{
		if (string.IsNullOrEmpty(self))
		{
			return SplitEmptyString(seps != null);
		}
		string[] array = StringUtils.Split(self, seps, (maxsplit < 0) ? int.MaxValue : (maxsplit + 1), (seps == null) ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
		List list = PythonOps.MakeEmptyList(array.Length);
		string[] array2 = array;
		foreach (string item in array2)
		{
			list.AddNoLock(item);
		}
		return list;
	}

	private static List SplitInternal(string self, string separator, int maxsplit)
	{
		if (string.IsNullOrEmpty(self))
		{
			return SplitEmptyString(separator != null);
		}
		string[] array = StringUtils.Split(self, separator, (maxsplit < 0) ? int.MaxValue : (maxsplit + 1), StringSplitOptions.None);
		List list = PythonOps.MakeEmptyList(array.Length);
		string[] array2 = array;
		foreach (string item in array2)
		{
			list.AddNoLock(item);
		}
		return list;
	}

	private static void TryStringOrTuple(object prefix)
	{
		if (prefix == null)
		{
			throw PythonOps.TypeError("expected string or Tuple, got NoneType");
		}
		if (!(prefix is string) && !(prefix is PythonTuple) && !(prefix is Extensible<string>))
		{
			throw PythonOps.TypeError("expected string or Tuple, got {0} Type", prefix.GetType());
		}
	}

	private static string GetString(object obj)
	{
		string text = AsString(obj);
		if (text == null)
		{
			throw PythonOps.TypeError("expected string, got {0}", DynamicHelpers.GetPythonType(obj).Name);
		}
		return text;
	}

	private static bool endswith(string self, string suffix)
	{
		return self.EndsWith(suffix);
	}

	private static bool endswith(string self, string suffix, int start)
	{
		int length = self.Length;
		if (start > length)
		{
			return false;
		}
		if (start < 0)
		{
			start += length;
			if (start < 0)
			{
				start = 0;
			}
		}
		return self.Substring(start).EndsWith(suffix);
	}

	private static bool endswith(string self, string suffix, int start, int end)
	{
		int length = self.Length;
		if (start > length)
		{
			return false;
		}
		if (start < 0)
		{
			start += length;
			if (start < 0)
			{
				start = 0;
			}
		}
		if (end >= length)
		{
			return self.Substring(start).EndsWith(suffix);
		}
		if (end < 0)
		{
			end += length;
			if (end < 0)
			{
				return false;
			}
		}
		if (end < start)
		{
			return false;
		}
		return self.Substring(start, end - start).EndsWith(suffix);
	}

	private static bool endswith(string self, PythonTuple suffix)
	{
		foreach (object item in suffix)
		{
			if (self.EndsWith(GetString(item)))
			{
				return true;
			}
		}
		return false;
	}

	private static bool endswith(string self, PythonTuple suffix, int start)
	{
		foreach (object item in suffix)
		{
			if (endswith(self, GetString(item), start))
			{
				return true;
			}
		}
		return false;
	}

	private static bool endswith(string self, PythonTuple suffix, int start, int end)
	{
		foreach (object item in suffix)
		{
			if (endswith(self, GetString(item), start, end))
			{
				return true;
			}
		}
		return false;
	}

	private static bool startswith(string self, string prefix)
	{
		return self.StartsWith(prefix);
	}

	private static bool startswith(string self, string prefix, int start)
	{
		int length = self.Length;
		if (start > length)
		{
			return false;
		}
		if (start < 0)
		{
			start += length;
			if (start < 0)
			{
				start = 0;
			}
		}
		return self.Substring(start).StartsWith(prefix);
	}

	private static bool startswith(string self, string prefix, int start, int end)
	{
		int length = self.Length;
		if (start > length)
		{
			return false;
		}
		if (start < 0)
		{
			start += length;
			if (start < 0)
			{
				start = 0;
			}
		}
		if (end >= length)
		{
			return self.Substring(start).StartsWith(prefix);
		}
		if (end < 0)
		{
			end += length;
			if (end < 0)
			{
				return false;
			}
		}
		if (end < start)
		{
			return false;
		}
		return self.Substring(start, end - start).StartsWith(prefix);
	}

	private static bool startswith(string self, PythonTuple prefix)
	{
		foreach (object item in prefix)
		{
			if (self.StartsWith(GetString(item)))
			{
				return true;
			}
		}
		return false;
	}

	private static bool startswith(string self, PythonTuple prefix, int start)
	{
		foreach (object item in prefix)
		{
			if (startswith(self, GetString(item), start))
			{
				return true;
			}
		}
		return false;
	}

	private static bool startswith(string self, PythonTuple prefix, int start, int end)
	{
		foreach (object item in prefix)
		{
			if (startswith(self, GetString(item), start, end))
			{
				return true;
			}
		}
		return false;
	}

	internal static IEnumerable StringEnumerable(string str)
	{
		return new PythonStringEnumerable(str);
	}

	internal static IEnumerator<string> StringEnumerator(string str)
	{
		return new PythonStringEnumerable(str);
	}

	public static string __repr__(string self)
	{
		return Quote(self);
	}
}
