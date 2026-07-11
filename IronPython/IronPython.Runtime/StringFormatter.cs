using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

internal class StringFormatter
{
	[Flags]
	internal enum FormatOptions
	{
		ZeroPad = 1,
		LeftAdj = 2,
		AltForm = 4,
		Space = 8,
		SignChar = 0x10
	}

	internal struct FormatSettings
	{
		internal FormatOptions Options;

		internal int FieldWidth;

		internal int Precision;

		internal object Value;

		public bool ZeroPad
		{
			get
			{
				return (Options & FormatOptions.ZeroPad) != 0;
			}
			set
			{
				if (value)
				{
					Options |= FormatOptions.ZeroPad;
				}
				else
				{
					Options &= ~FormatOptions.ZeroPad;
				}
			}
		}

		public bool LeftAdj
		{
			get
			{
				return (Options & FormatOptions.LeftAdj) != 0;
			}
			set
			{
				if (value)
				{
					Options |= FormatOptions.LeftAdj;
				}
				else
				{
					Options &= ~FormatOptions.LeftAdj;
				}
			}
		}

		public bool AltForm
		{
			get
			{
				return (Options & FormatOptions.AltForm) != 0;
			}
			set
			{
				if (value)
				{
					Options |= FormatOptions.AltForm;
				}
				else
				{
					Options &= ~FormatOptions.AltForm;
				}
			}
		}

		public bool Space
		{
			get
			{
				return (Options & FormatOptions.Space) != 0;
			}
			set
			{
				if (value)
				{
					Options |= FormatOptions.Space;
				}
				else
				{
					Options &= ~FormatOptions.Space;
				}
			}
		}

		public bool SignChar
		{
			get
			{
				return (Options & FormatOptions.SignChar) != 0;
			}
			set
			{
				if (value)
				{
					Options |= FormatOptions.SignChar;
				}
				else
				{
					Options &= ~FormatOptions.SignChar;
				}
			}
		}
	}

	private const int UnspecifiedPrecision = -1;

	private readonly CodeContext _context;

	private readonly bool _isUnicodeString;

	private object _data;

	private int _dataIndex;

	private string _str;

	private int _index;

	private char _curCh;

	internal FormatSettings _opts;

	internal bool _TrailingZeroAfterWholeFloat;

	private StringBuilder _buf;

	[ThreadStatic]
	private static NumberFormatInfo NumberFormatInfoForThread;

	private static readonly char[] zero = new char[1] { '0' };

	internal static NumberFormatInfo nfi
	{
		get
		{
			if (NumberFormatInfoForThread == null)
			{
				NumberFormatInfo numberFormat = ((CultureInfo)CultureInfo.InvariantCulture.Clone()).NumberFormat;
				numberFormat.PositiveInfinitySymbol = "inf";
				numberFormat.NegativeInfinitySymbol = "-inf";
				numberFormat.NaNSymbol = "nan";
				NumberFormatInfoForThread = numberFormat;
			}
			return NumberFormatInfoForThread;
		}
	}

	public StringFormatter(CodeContext context, string str, object data)
		: this(context, str, data, isUnicode: false)
	{
	}

	public StringFormatter(CodeContext context, string str, object data, bool isUnicode)
	{
		_str = str;
		_data = data;
		_context = context;
		_isUnicodeString = isUnicode;
	}

	public string Format()
	{
		_index = 0;
		_buf = new StringBuilder(_str.Length * 2);
		int num;
		while ((num = _str.IndexOf('%', _index)) != -1)
		{
			_buf.Append(_str, _index, num - _index);
			_index = num + 1;
			DoFormatCode();
		}
		_buf.Append(_str, _index, _str.Length - _index);
		CheckDataUsed();
		return _buf.ToString();
	}

	private void DoFormatCode()
	{
		if (_index == _str.Length)
		{
			throw PythonOps.ValueError("incomplete format, expected format character at index {0}", _index);
		}
		_curCh = _str[_index++];
		if (_curCh == '%')
		{
			_buf.Append('%');
			return;
		}
		string text = ReadMappingKey();
		_opts = default(FormatSettings);
		ReadConversionFlags();
		ReadMinimumFieldWidth();
		ReadPrecision();
		ReadLengthModifier();
		object value = ((text != null) ? GetKey(text) : GetData(_dataIndex++));
		_opts.Value = value;
		WriteConversion();
	}

	private string ReadMappingKey()
	{
		if (_curCh != '(')
		{
			return null;
		}
		int num = 1;
		int index = _index;
		for (int i = index; i < _str.Length; i++)
		{
			if (_str[i] == '(')
			{
				num++;
			}
			else if (_str[i] == ')')
			{
				num--;
			}
			if (num == 0)
			{
				string result = _str.Substring(_index, i - index);
				_index = i + 1;
				if (_index == _str.Length)
				{
					throw PythonOps.ValueError("incomplete format");
				}
				_curCh = _str[_index++];
				return result;
			}
		}
		throw PythonOps.ValueError("incomplete format key");
	}

	private void ReadConversionFlags()
	{
		bool flag;
		do
		{
			flag = true;
			switch (_curCh)
			{
			case '#':
				_opts.AltForm = true;
				break;
			case '-':
				_opts.LeftAdj = true;
				_opts.ZeroPad = false;
				break;
			case '0':
				if (!_opts.LeftAdj)
				{
					_opts.ZeroPad = true;
				}
				break;
			case '+':
				_opts.SignChar = true;
				_opts.Space = false;
				break;
			case ' ':
				if (!_opts.SignChar)
				{
					_opts.Space = true;
				}
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				_curCh = _str[_index++];
			}
		}
		while (flag);
	}

	private int ReadNumberOrStar()
	{
		return ReadNumberOrStar(0);
	}

	private int ReadNumberOrStar(int noValSpecified)
	{
		int num = noValSpecified;
		if (_curCh == '*')
		{
			if (!(_data is PythonTuple))
			{
				throw PythonOps.TypeError("* requires a tuple for values");
			}
			_curCh = _str[_index++];
			num = PythonContext.GetContext(_context).ConvertToInt32(GetData(_dataIndex++));
		}
		else if (char.IsDigit(_curCh))
		{
			num = 0;
			while (char.IsDigit(_curCh) && _index < _str.Length)
			{
				num = num * 10 + (_curCh - 48);
				_curCh = _str[_index++];
			}
		}
		return num;
	}

	private void ReadMinimumFieldWidth()
	{
		int num = ReadNumberOrStar();
		if (num < 0)
		{
			_opts.FieldWidth = num * -1;
			_opts.LeftAdj = true;
		}
		else
		{
			_opts.FieldWidth = num;
		}
		if (_opts.FieldWidth == int.MaxValue)
		{
			throw PythonOps.MemoryError("not enough memory for field width");
		}
	}

	private void ReadPrecision()
	{
		if (_curCh == '.')
		{
			_curCh = _str[_index++];
			_opts.Precision = ReadNumberOrStar();
			if (_opts.Precision > 116)
			{
				throw PythonOps.OverflowError("formatted integer is too long (precision too large?)");
			}
		}
		else
		{
			_opts.Precision = -1;
		}
	}

	private void ReadLengthModifier()
	{
		char curCh = _curCh;
		if (curCh == 'L' || curCh == 'h' || curCh == 'l')
		{
			_curCh = _str[_index++];
		}
	}

	private void WriteConversion()
	{
		switch (_curCh)
		{
		case 'd':
		case 'i':
			AppendInt();
			return;
		case 'o':
			AppendOctal();
			return;
		case 'u':
			AppendInt();
			return;
		case 'x':
			AppendHex(_curCh);
			return;
		case 'X':
			AppendHex(_curCh);
			return;
		case 'E':
		case 'F':
		case 'e':
		case 'f':
			AppendFloat(_curCh);
			return;
		case 'G':
		case 'g':
			AppendFloat(_curCh);
			return;
		case 'c':
			AppendChar();
			return;
		case 'r':
			AppendRepr();
			return;
		case 's':
			AppendString();
			return;
		}
		if (_curCh > 'ÿ')
		{
			throw PythonOps.ValueError("unsupported format character '{0}' (0x{1:X}) at index {2}", '?', (int)_curCh, _index - 1);
		}
		throw PythonOps.ValueError("unsupported format character '{0}' (0x{1:X}) at index {2}", _curCh, (int)_curCh, _index - 1);
	}

	private object GetData(int index)
	{
		if (_data is PythonTuple pythonTuple)
		{
			if (index < pythonTuple.__len__())
			{
				return pythonTuple[index];
			}
		}
		else if (index == 0)
		{
			return _data;
		}
		throw PythonOps.TypeError("not enough arguments for format string");
	}

	private object GetKey(string key)
	{
		object value2;
		if (!(_data is IDictionary<object, object> dictionary))
		{
			if (!(_data is PythonDictionary pythonDictionary))
			{
				if (PythonOps.IsMappingType(DefaultContext.Default, _data) == ScriptingRuntimeHelpers.True)
				{
					return PythonOps.GetIndex(_context, _data, key);
				}
				throw PythonOps.TypeError("format requires a mapping");
			}
			if (pythonDictionary.TryGetValue(key, out var value))
			{
				return value;
			}
		}
		else if (dictionary.TryGetValue(key, out value2))
		{
			return value2;
		}
		throw PythonOps.KeyError(key);
	}

	private object GetIntegerValue(out bool fPos)
	{
		object result;
		if (PythonContext.GetContext(_context).TryConvertToInt32(_opts.Value, out var res))
		{
			result = res;
			fPos = res >= 0;
		}
		else
		{
			if (!Converter.TryConvertToBigInteger(_opts.Value, out var result2))
			{
				throw PythonOps.TypeError("int argument required");
			}
			result = result2;
			fPos = result2 >= BigInteger.Zero;
		}
		return result;
	}

	private void AppendChar()
	{
		char value = Converter.ExplicitConvertToChar(_opts.Value);
		if (_opts.FieldWidth > 1)
		{
			if (!_opts.LeftAdj)
			{
				_buf.Append(' ', _opts.FieldWidth - 1);
			}
			_buf.Append(value);
			if (_opts.LeftAdj)
			{
				_buf.Append(' ', _opts.FieldWidth - 1);
			}
		}
		else
		{
			_buf.Append(value);
		}
	}

	private void CheckDataUsed()
	{
		if (PythonOps.IsMappingType(DefaultContext.Default, _data) == ScriptingRuntimeHelpers.False && ((!(_data is PythonTuple) && _dataIndex != 1) || (_data is PythonTuple && _dataIndex != ((PythonTuple)_data).__len__())))
		{
			throw PythonOps.TypeError("not all arguments converted during string formatting");
		}
	}

	private void AppendInt()
	{
		bool fPos;
		object integerValue = GetIntegerValue(out fPos);
		if (_opts.LeftAdj)
		{
			AppendLeftAdj(integerValue, fPos, 'D');
		}
		else if (_opts.Precision > 0)
		{
			_opts.FieldWidth = ((_opts.Space || _opts.SignChar) ? (_opts.Precision + 1) : _opts.Precision);
			AppendZeroPad(integerValue, fPos, 'D');
		}
		else if (_opts.ZeroPad)
		{
			AppendZeroPad(integerValue, fPos, 'D');
		}
		else
		{
			AppendNumeric(integerValue, fPos, 'D');
		}
	}

	private char AdjustForG(char type, double v)
	{
		if (type != 'G' && type != 'g')
		{
			return type;
		}
		if (double.IsNaN(v) || double.IsInfinity(v))
		{
			return type;
		}
		double num = Math.Abs(v);
		if ((v != 0.0 && num < 0.0001) || num >= Math.Pow(10.0, _opts.Precision))
		{
			int num2 = _opts.Precision - 1;
			string text = num.ToString("E" + num2, CultureInfo.InvariantCulture);
			string text2 = text.Substring(0, text.IndexOf('E')).TrimEnd(zero);
			_opts.Precision = text2.Length - 2;
			type = ((type == 'G') ? 'E' : 'e');
		}
		else
		{
			int num3 = _opts.Precision;
			if (num < 0.001)
			{
				num3 += 3;
			}
			else if (num < 0.01)
			{
				num3 += 2;
			}
			else if (num < 0.1)
			{
				num3++;
			}
			string text3 = num.ToString("F" + num3, CultureInfo.InvariantCulture).TrimEnd(zero);
			bool flag = true;
			if (num3 > 15)
			{
				string text4 = num.ToString("G" + num3, CultureInfo.InvariantCulture).TrimEnd(zero);
				if (text4.Length > text3.Length)
				{
					text3 = text4;
					flag = false;
				}
			}
			string text5 = text3.Substring(text3.IndexOf('.') + 1);
			if (num < 1.0)
			{
				_opts.Precision = text5.Length;
			}
			else
			{
				int num4 = 1 + (int)Math.Log10(num);
				_opts.Precision = Math.Min(_opts.Precision - num4, text5.Length);
			}
			if (flag)
			{
				type = ((type == 'G') ? 'F' : 'f');
			}
		}
		return type;
	}

	private void AppendFloat(char type)
	{
		if (!Converter.TryConvertToDouble(_opts.Value, out var result))
		{
			throw PythonOps.TypeError("float argument required");
		}
		bool flag = false;
		if (_opts.Precision != -1)
		{
			if (_opts.Precision == 0 && _opts.AltForm)
			{
				flag = true;
			}
			if (_opts.Precision > 50)
			{
				_opts.Precision = 50;
			}
		}
		else if (_opts.AltForm)
		{
			_opts.Precision = 0;
			flag = true;
		}
		else
		{
			_opts.Precision = 6;
		}
		type = AdjustForG(type, result);
		nfi.NumberDecimalDigits = _opts.Precision;
		if (_opts.LeftAdj)
		{
			AppendLeftAdj(result, DoubleOps.Sign(result) >= 0, type);
		}
		else if (_opts.ZeroPad)
		{
			AppendZeroPadFloat(result, type);
		}
		else
		{
			AppendNumeric(result, DoubleOps.Sign(result) >= 0, type);
		}
		if (DoubleOps.Sign(result) < 0 && result > -1.0 && _buf[0] != '-')
		{
			FixupFloatMinus();
		}
		if (flag)
		{
			FixupAltFormDot(result);
		}
	}

	private void FixupAltFormDot(double v)
	{
		if (!double.IsInfinity(v) && !double.IsNaN(v))
		{
			_buf.Append('.');
		}
		if (_opts.FieldWidth == 0)
		{
			return;
		}
		for (int i = 0; i < _buf.Length; i++)
		{
			if (_buf[i] == ' ' || _buf[i] == '0')
			{
				_buf.Remove(i, 1);
				break;
			}
			if (_buf[i] != '-' && _buf[i] != '+')
			{
				break;
			}
		}
	}

	private void FixupFloatMinus()
	{
		bool flag = true;
		for (int i = 0; i < _buf.Length; i++)
		{
			if (_buf[i] != '.' && _buf[i] != '0' && _buf[i] != ' ')
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		if (_opts.FieldWidth != 0)
		{
			if (_buf[_buf.Length - 1] == ' ')
			{
				_buf.Insert(0, "-");
				_buf.Remove(_buf.Length - 1, 1);
				return;
			}
			int j;
			for (j = 0; _buf[j] == ' '; j++)
			{
			}
			if (j > 0)
			{
				j--;
			}
			_buf[j] = '-';
		}
		else
		{
			_buf.Insert(0, "-");
		}
	}

	private void AppendZeroPad(object val, bool fPos, char format)
	{
		if (fPos && (_opts.SignChar || _opts.Space))
		{
			string text = string.Format(nfi, "{0:" + format + _opts.FieldWidth.ToString() + "}", new object[1] { val });
			char c = (_opts.SignChar ? '+' : ' ');
			text = ((text[0] != '0' || text.Length <= 1) ? (c + text) : (c + text.Substring(1)));
			_buf.Append(text);
			return;
		}
		string text2 = string.Format(nfi, "{0:" + format + _opts.FieldWidth.ToString() + "}", new object[1] { val });
		if (text2[0] == '-')
		{
			_buf.Append("-");
			if (text2[1] != '0')
			{
				_buf.Append(text2.Substring(1));
			}
			else
			{
				_buf.Append(text2.Substring(2));
			}
		}
		else
		{
			_buf.Append(text2);
		}
	}

	private void AppendZeroPadFloat(double val, char format)
	{
		if (val >= 0.0)
		{
			StringBuilder stringBuilder = new StringBuilder(val.ToString(format.ToString(), nfi));
			if (stringBuilder.Length < _opts.FieldWidth)
			{
				stringBuilder.Insert(0, new string('0', _opts.FieldWidth - stringBuilder.Length));
			}
			if (_opts.SignChar || _opts.Space)
			{
				char value = (_opts.SignChar ? '+' : ' ');
				if (stringBuilder[0] == '0' && stringBuilder[1] != '.')
				{
					stringBuilder[0] = value;
				}
				else
				{
					stringBuilder.Insert(0, value.ToString());
				}
			}
			_buf.Append(stringBuilder);
		}
		else
		{
			StringBuilder stringBuilder2 = new StringBuilder(val.ToString(format.ToString(), nfi));
			if (stringBuilder2.Length < _opts.FieldWidth)
			{
				stringBuilder2.Insert(1, new string('0', _opts.FieldWidth - stringBuilder2.Length));
			}
			_buf.Append(stringBuilder2);
		}
	}

	private void AppendNumeric(object val, bool fPos, char format)
	{
		bool flag = format == 'e' || format == 'E';
		if (fPos && (_opts.SignChar || _opts.Space))
		{
			string text = (_opts.SignChar ? "+" : " ") + string.Format(nfi, "{0:" + format + "}", new object[1] { val });
			if (text.Length < _opts.FieldWidth)
			{
				_buf.Append(' ', _opts.FieldWidth - text.Length);
			}
			_buf.Append(text);
		}
		else if (_opts.Precision == -1)
		{
			_buf.AppendFormat(nfi, "{0," + _opts.FieldWidth + ":" + format + "}", new object[1] { val });
		}
		else if (_opts.Precision < 100)
		{
			string text2 = string.Format(nfi, "{0," + _opts.FieldWidth + ":" + format + _opts.Precision + "}", new object[1] { val });
			if (flag)
			{
				_buf.Append(removeExponentePaddingZero(text2));
			}
			else
			{
				_buf.Append(text2);
			}
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0:" + format + "}", val);
			if (stringBuilder.Length < _opts.Precision)
			{
				stringBuilder.Insert(0, new string('0', _opts.Precision - stringBuilder.Length));
			}
			if (stringBuilder.Length < _opts.FieldWidth)
			{
				stringBuilder.Insert(0, new string(' ', _opts.FieldWidth - stringBuilder.Length));
			}
			_buf.Append(stringBuilder.ToString());
		}
		if (_TrailingZeroAfterWholeFloat && (format == 'f' || format == 'F') && _opts.Precision == 0)
		{
			_buf.Append(".0");
		}
	}

	private static string removeExponentePaddingZero(string val)
	{
		if (val[val.Length - 3] == '0' && (((val[val.Length - 5] == 'e' || val[val.Length - 5] == 'E') && (val[val.Length - 4] == '+' || val[val.Length - 4] == '-')) || val[val.Length - 4] == 'e' || val[val.Length - 4] == 'E'))
		{
			return val.Substring(0, val.Length - 3) + val.Substring(val.Length - 2, 2);
		}
		return val;
	}

	private void AppendLeftAdj(object val, bool fPos, char type)
	{
		string text = string.Format(nfi, "{0:" + type + "}", new object[1] { val });
		if (fPos)
		{
			if (_opts.SignChar)
			{
				text = '+' + text;
			}
			else if (_opts.Space)
			{
				text = ' ' + text;
			}
		}
		_buf.Append(text);
		if (text.Length < _opts.FieldWidth)
		{
			_buf.Append(' ', _opts.FieldWidth - text.Length);
		}
	}

	private static bool NeedsAltForm(char format, char last)
	{
		if (format == 'X' || format == 'x')
		{
			return true;
		}
		if (last == '0')
		{
			return false;
		}
		return true;
	}

	private static string GetAltFormPrefixForRadix(char format, int radix)
	{
		return radix switch
		{
			8 => "0", 
			16 => format + "0", 
			_ => "", 
		};
	}

	private void AppendBase(char format, int radix)
	{
		bool fPos;
		object integerValue = GetIntegerValue(out fPos);
		if (integerValue is BigInteger)
		{
			AppendBaseBigInt((BigInteger)integerValue, format, radix);
			return;
		}
		int num = (int)integerValue;
		int num2 = num;
		if (num2 < 0)
		{
			num2 *= -1;
			_opts.Space = false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (num2 == 0)
		{
			stringBuilder.Append('0');
		}
		while (num2 != 0)
		{
			int num3 = num2 % radix;
			if (num3 < 10)
			{
				stringBuilder.Append((char)(num3 + 48));
			}
			else if (char.IsLower(format))
			{
				stringBuilder.Append((char)(num3 - 10 + 97));
			}
			else
			{
				stringBuilder.Append((char)(num3 - 10 + 65));
			}
			num2 /= radix;
		}
		if (stringBuilder.Length < _opts.Precision)
		{
			int repeatCount = _opts.Precision - stringBuilder.Length;
			stringBuilder.Append('0', repeatCount);
		}
		if (_opts.FieldWidth != 0)
		{
			int num4 = ((num < 0 || _opts.SignChar) ? 1 : 0);
			int num5 = (_opts.Space ? 1 : 0);
			int num6 = _opts.FieldWidth - (stringBuilder.Length + num4 + num5);
			if (num6 > 0)
			{
				if (_opts.AltForm && NeedsAltForm(format, (!_opts.LeftAdj && _opts.ZeroPad) ? '0' : stringBuilder[stringBuilder.Length - 1]))
				{
					num6 -= GetAltFormPrefixForRadix(format, radix).Length;
				}
				if (num6 > 0)
				{
					if (_opts.LeftAdj)
					{
						stringBuilder.Insert(0, " ", num6);
					}
					else if (_opts.ZeroPad)
					{
						stringBuilder.Append('0', num6);
					}
					else
					{
						_buf.Append(' ', num6);
					}
				}
			}
		}
		if (_opts.AltForm && NeedsAltForm(format, stringBuilder[stringBuilder.Length - 1]))
		{
			stringBuilder.Append(GetAltFormPrefixForRadix(format, radix));
		}
		if (num < 0)
		{
			_buf.Append('-');
		}
		else if (_opts.SignChar)
		{
			_buf.Append('+');
		}
		else if (_opts.Space)
		{
			_buf.Append(' ');
		}
		for (int num7 = stringBuilder.Length - 1; num7 >= 0; num7--)
		{
			_buf.Append(stringBuilder[num7]);
		}
	}

	private void AppendBaseBigInt(BigInteger origVal, char format, int radix)
	{
		BigInteger bigInteger = origVal;
		if (bigInteger < 0L)
		{
			bigInteger *= (BigInteger)(-1);
		}
		StringBuilder stringBuilder = new StringBuilder();
		switch (radix)
		{
		case 16:
			AppendNumberReversed(stringBuilder, char.IsLower(format) ? bigInteger.ToString("x") : bigInteger.ToString("X"));
			break;
		case 10:
			AppendNumberReversed(stringBuilder, bigInteger.ToString());
			break;
		default:
			if (bigInteger == 0L)
			{
				stringBuilder.Append('0');
			}
			for (; bigInteger != 0L; bigInteger /= (BigInteger)radix)
			{
				int num = (int)(bigInteger % radix);
				if (num < 10)
				{
					stringBuilder.Append((char)(num + 48));
				}
				else if (char.IsLower(format))
				{
					stringBuilder.Append((char)(num - 10 + 97));
				}
				else
				{
					stringBuilder.Append((char)(num - 10 + 65));
				}
			}
			break;
		}
		if (stringBuilder.Length < _opts.Precision)
		{
			int repeatCount = _opts.Precision - stringBuilder.Length;
			stringBuilder.Append('0', repeatCount);
		}
		if (_opts.FieldWidth != 0)
		{
			int num2 = ((origVal < 0L || _opts.SignChar) ? 1 : 0);
			int num3 = _opts.FieldWidth - (stringBuilder.Length + num2);
			if (num3 > 0)
			{
				if (_opts.AltForm && NeedsAltForm(format, (!_opts.LeftAdj && _opts.ZeroPad) ? '0' : stringBuilder[stringBuilder.Length - 1]))
				{
					num3 -= GetAltFormPrefixForRadix(format, radix).Length;
				}
				if (num3 > 0)
				{
					if (_opts.LeftAdj)
					{
						stringBuilder.Insert(0, " ", num3);
					}
					else if (_opts.ZeroPad)
					{
						stringBuilder.Append('0', num3);
					}
					else
					{
						_buf.Append(' ', num3);
					}
				}
			}
		}
		if (_opts.AltForm && NeedsAltForm(format, stringBuilder[stringBuilder.Length - 1]))
		{
			stringBuilder.Append(GetAltFormPrefixForRadix(format, radix));
		}
		if (origVal < 0L)
		{
			_buf.Append('-');
		}
		else if (_opts.SignChar)
		{
			_buf.Append('+');
		}
		else if (_opts.Space)
		{
			_buf.Append(' ');
		}
		for (int num4 = stringBuilder.Length - 1; num4 >= 0; num4--)
		{
			_buf.Append(stringBuilder[num4]);
		}
	}

	private static void AppendNumberReversed(StringBuilder str, string res)
	{
		int i;
		for (i = 0; i < res.Length - 1 && res[i] == '0'; i++)
		{
		}
		for (int num = res.Length - 1; num >= i; num--)
		{
			str.Append(res[num]);
		}
	}

	private void AppendHex(char format)
	{
		AppendBase(format, 16);
	}

	private void AppendOctal()
	{
		AppendBase('o', 8);
	}

	private void AppendString()
	{
		string text;
		if (!_isUnicodeString)
		{
			text = PythonOps.ToString(_context, _opts.Value);
		}
		else
		{
			object obj = StringOps.FastNewUnicode(_context, _opts.Value);
			text = obj as string;
			if (text == null)
			{
				text = ((Extensible<string>)obj).Value;
			}
		}
		if (text == null)
		{
			text = "None";
		}
		AppendString(text);
	}

	private void AppendRepr()
	{
		AppendString(PythonOps.Repr(_context, _opts.Value));
	}

	private void AppendString(string s)
	{
		if (_opts.Precision != -1 && s.Length > _opts.Precision)
		{
			s = s.Substring(0, _opts.Precision);
		}
		if (!_opts.LeftAdj && _opts.FieldWidth > s.Length)
		{
			_buf.Append(' ', _opts.FieldWidth - s.Length);
		}
		_buf.Append(s);
		if (_opts.LeftAdj && _opts.FieldWidth > s.Length)
		{
			_buf.Append(' ', _opts.FieldWidth - s.Length);
		}
	}
}
