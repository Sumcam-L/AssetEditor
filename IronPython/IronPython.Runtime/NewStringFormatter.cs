using System.Collections.Generic;
using System.Text;
using IronPython.Modules;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

internal sealed class NewStringFormatter
{
	private struct StringFormatParser
	{
		private readonly string _str;

		private int _index;

		private StringFormatParser(string text)
		{
			_str = text;
			_index = 0;
		}

		public static IEnumerable<PythonTuple> Parse(string text)
		{
			return new StringFormatParser(text).Parse();
		}

		private IEnumerable<PythonTuple> Parse()
		{
			while (_index != _str.Length)
			{
				int lastTextStart = _index;
				_index = _str.IndexOfAny(_brackets, _index);
				if (_index == -1)
				{
					yield return PythonTuple.MakeTuple(_str.Substring(lastTextStart, _str.Length - lastTextStart), null, null, null);
					break;
				}
				yield return ParseFormat(lastTextStart);
			}
		}

		private PythonTuple ParseFormat(int lastTextStart)
		{
			if (ParseDoubleBracket(lastTextStart, out var text))
			{
				return PythonTuple.MakeTuple(text, null, null, null);
			}
			int depth = 1;
			char? c = null;
			string text2 = string.Empty;
			string text3 = ParseFieldName(ref depth);
			bool flag = CheckEnd();
			if (!flag && _str[_index] == '!')
			{
				c = ParseConversion();
			}
			flag = flag || CheckEnd();
			if (!flag && _str[_index] == ':')
			{
				text2 = ParseFormatSpec(ref depth);
			}
			if (!flag && !CheckEnd())
			{
				throw PythonOps.ValueError("expected ':' after format specifier");
			}
			return PythonTuple.MakeTuple(text, text3, text2, c.HasValue ? c.ToString() : null);
		}

		private bool ParseDoubleBracket(int lastTextStart, out string text)
		{
			if (_str[_index] == '}')
			{
				_index++;
				if (_index == _str.Length || _str[_index] != '}')
				{
					throw PythonOps.ValueError("Single '}}' encountered in format string");
				}
				text = _str.Substring(lastTextStart, _index - lastTextStart);
				_index++;
				return true;
			}
			if (_index == _str.Length - 1)
			{
				throw PythonOps.ValueError("Single '{{' encountered in format string");
			}
			if (_str[_index + 1] == '{')
			{
				text = _str.Substring(lastTextStart, ++_index - lastTextStart);
				_index++;
				return true;
			}
			text = _str.Substring(lastTextStart, _index++ - lastTextStart);
			return false;
		}

		private char ParseConversion()
		{
			_index++;
			if (CheckEnd())
			{
				throw PythonOps.ValueError("end of format while looking for conversion specifier");
			}
			return _str[_index++];
		}

		private bool CheckEnd()
		{
			if (_index == _str.Length)
			{
				throw PythonOps.ValueError("unmatched '{{' in format");
			}
			if (_str[_index] == '}')
			{
				_index++;
				return true;
			}
			return false;
		}

		private string ParseFormatSpec(ref int depth)
		{
			_index++;
			return ParseFieldOrSpecWorker(_brackets, ref depth);
		}

		private string ParseFieldName(ref int depth)
		{
			return ParseFieldOrSpecWorker(_fieldNameEnd, ref depth);
		}

		private string ParseFieldOrSpecWorker(char[] ends, ref int depth)
		{
			int num = _index - 1;
			bool flag = false;
			do
			{
				num = _str.IndexOfAny(ends, num + 1);
				if (num == -1)
				{
					throw PythonOps.ValueError("unmatched '{{' in format");
				}
				switch (_str[num])
				{
				case '{':
					depth++;
					break;
				case '}':
					depth--;
					break;
				default:
					flag = true;
					break;
				}
			}
			while (!flag && depth != 0);
			string result = _str.Substring(_index, num - _index);
			_index = num;
			return result;
		}
	}

	private class Formatter
	{
		private readonly PythonContext _context;

		private readonly PythonTuple _args;

		private readonly IDictionary<object, object> _kwArgs;

		private readonly int _depth;

		private int _autoNumberedIndex;

		private Formatter(PythonContext context, PythonTuple args, IDictionary<object, object> kwArgs, int depth)
			: this(context, args, kwArgs)
		{
			_depth = depth;
		}

		private Formatter(PythonContext context, PythonTuple args, IDictionary<object, object> kwArgs)
		{
			_context = context;
			_args = args;
			_kwArgs = kwArgs;
		}

		public static string FormatString(PythonContext context, string format, PythonTuple args, IDictionary<object, object> kwArgs)
		{
			return new Formatter(context, args, kwArgs).ReplaceText(format);
		}

		public static string FormatString(PythonContext context, string format, PythonTuple args, IDictionary<object, object> kwArgs, int depth)
		{
			if (depth == 2)
			{
				throw PythonOps.ValueError("Max string recursion exceeded");
			}
			return new Formatter(context, args, kwArgs, depth).ReplaceText(format);
		}

		private string ReplaceText(string format)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (PythonTuple item in StringFormatParser.Parse(format))
			{
				string value = (string)item[0];
				string text = (string)item[1];
				string formatSpec = (string)item[2];
				string text2 = (string)item[3];
				char? conversion = ((text2 != null && text2.Length > 0) ? new char?(text2[0]) : ((char?)null));
				stringBuilder.Append(value);
				if (text != null)
				{
					object argumentValue = GetArgumentValue(ParseFieldName(text, reportErrors: true));
					argumentValue = ApplyConversion(conversion, argumentValue);
					formatSpec = ReplaceComputedFormats(formatSpec);
					stringBuilder.Append(Builtin.format(_context.SharedContext, argumentValue, formatSpec));
				}
			}
			return stringBuilder.ToString();
		}

		private string ReplaceComputedFormats(string formatSpec)
		{
			int num = formatSpec.IndexOf('{');
			if (num != -1)
			{
				formatSpec = FormatString(_context, formatSpec, _args, _kwArgs, _depth + 1);
			}
			return formatSpec;
		}

		private object GetArgumentValue(FieldName fieldName)
		{
			return DoAccessors(fieldName, GetUnaccessedObject(fieldName));
		}

		private object ApplyConversion(char? conversion, object argValue)
		{
			switch (conversion)
			{
			case 'r':
				argValue = PythonOps.Repr(_context.SharedContext, argValue);
				break;
			case 's':
				argValue = PythonOps.ToString(_context.SharedContext, argValue);
				break;
			default:
				throw PythonOps.ValueError("Unknown conversion specifier {0}", conversion.Value);
			case null:
				break;
			}
			return argValue;
		}

		private object GetUnaccessedObject(FieldName fieldName)
		{
			if (fieldName.ArgumentName.Length == 0)
			{
				if (_autoNumberedIndex == -1)
				{
					throw PythonOps.ValueError("cannot switch from manual field specification to automatic field numbering");
				}
				return _args[_autoNumberedIndex++];
			}
			if (int.TryParse(fieldName.ArgumentName, out var result))
			{
				if (_autoNumberedIndex > 0)
				{
					throw PythonOps.ValueError("cannot switch from automatic field numbering to manual field specification");
				}
				_autoNumberedIndex = -1;
				return _args[result];
			}
			return _kwArgs[fieldName.ArgumentName];
		}

		private object DoAccessors(FieldName fieldName, object argValue)
		{
			foreach (FieldAccessor accessor in fieldName.Accessors)
			{
				argValue = ((!accessor.IsField) ? ((!int.TryParse(accessor.AttributeName, out var result)) ? PythonOps.GetIndex(_context.SharedContext, argValue, accessor.AttributeName) : PythonOps.GetIndex(_context.SharedContext, argValue, ScriptingRuntimeHelpers.Int32ToObject(result))) : PythonOps.GetBoundAttr(_context.SharedContext, argValue, accessor.AttributeName));
			}
			return argValue;
		}
	}

	private struct FieldName
	{
		public readonly string ArgumentName;

		public readonly IEnumerable<FieldAccessor> Accessors;

		public FieldName(string argumentName, IEnumerable<FieldAccessor> accessors)
		{
			ArgumentName = argumentName;
			Accessors = accessors;
		}
	}

	private struct FieldAccessor
	{
		public readonly string AttributeName;

		public readonly bool IsField;

		public FieldAccessor(string attributeName, bool isField)
		{
			AttributeName = attributeName;
			IsField = isField;
		}
	}

	private static readonly char[] _brackets = new char[2] { '{', '}' };

	private static readonly char[] _fieldNameEnd = new char[4] { '{', '}', '!', ':' };

	public static string FormatString(PythonContext context, string format, PythonTuple args, IDictionary<object, object> kwArgs)
	{
		ContractUtils.RequiresNotNull(context, "context");
		ContractUtils.RequiresNotNull(format, "format");
		ContractUtils.RequiresNotNull(args, "args");
		ContractUtils.RequiresNotNull(kwArgs, "kwArgs");
		return Formatter.FormatString(context, format, args, kwArgs);
	}

	public static IEnumerable<PythonTuple> GetFormatInfo(string format)
	{
		ContractUtils.RequiresNotNull(format, "format");
		return StringFormatParser.Parse(format);
	}

	public static PythonTuple GetFieldNameInfo(string name)
	{
		ContractUtils.RequiresNotNull(name, "name");
		FieldName fieldName = ParseFieldName(name, reportErrors: false);
		if (string.IsNullOrEmpty(fieldName.ArgumentName))
		{
			throw PythonOps.ValueError("empty field name");
		}
		object obj = fieldName.ArgumentName;
		if (int.TryParse(fieldName.ArgumentName, out var result))
		{
			obj = ScriptingRuntimeHelpers.Int32ToObject(result);
		}
		return PythonTuple.MakeTuple(obj, AccessorsToPython(fieldName.Accessors));
	}

	private static FieldName ParseFieldName(string str, bool reportErrors)
	{
		int index = 0;
		string argumentName = ParseIdentifier(str, isIndex: false, ref index);
		return new FieldName(argumentName, ParseFieldAccessors(str, index, reportErrors));
	}

	private static IEnumerable<FieldAccessor> ParseFieldAccessors(string str, int index, bool reportErrors)
	{
		while (index != str.Length && str[index] != '}')
		{
			char accessType = str[index];
			if (accessType == '.' || accessType == '[')
			{
				index++;
				bool isIndex = accessType == '[';
				string identifier = ParseIdentifier(str, isIndex, ref index);
				if (isIndex)
				{
					if (index == str.Length || str[index] != ']')
					{
						throw PythonOps.ValueError("Missing ']' in format string");
					}
					index++;
				}
				if (identifier.Length == 0)
				{
					throw PythonOps.ValueError("Empty attribute in format string");
				}
				yield return new FieldAccessor(identifier, !isIndex);
				continue;
			}
			if (reportErrors)
			{
				throw PythonOps.ValueError("Only '.' and '[' are valid in format field specifier, got {0}", accessType);
			}
			break;
		}
	}

	private static IEnumerable<PythonTuple> AccessorsToPython(IEnumerable<FieldAccessor> accessors)
	{
		foreach (FieldAccessor accessor in accessors)
		{
			object attrName = accessor.AttributeName;
			if (int.TryParse(accessor.AttributeName, out var val))
			{
				attrName = ScriptingRuntimeHelpers.Int32ToObject(val);
			}
			yield return PythonTuple.MakeTuple(ScriptingRuntimeHelpers.BooleanToObject(accessor.IsField), attrName);
		}
	}

	private static string ParseIdentifier(string str, bool isIndex, ref int index)
	{
		int num = index;
		while (index < str.Length && str[index] != '.' && (isIndex || str[index] != '[') && (!isIndex || str[index] != ']'))
		{
			index++;
		}
		return str.Substring(num, index - num);
	}
}
