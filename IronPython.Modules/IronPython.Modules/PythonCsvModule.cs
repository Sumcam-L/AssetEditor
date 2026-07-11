using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public static class PythonCsvModule
{
	[PythonType]
	[Documentation("CSV dialect\r\nThe Dialect type records CSV parsing and generation options.")]
	public class Dialect
	{
		private string _delimiter = ",";

		private string _escapechar;

		private bool _skipinitialspace;

		private bool _doublequote = true;

		private bool _strict;

		private int _quoting;

		private string _quotechar = "\"";

		private string _lineterminator = "\r\n";

		private static readonly string[] VALID_KWARGS = new string[9] { "dialect", "delimiter", "doublequote", "escapechar", "lineterminator", "quotechar", "quoting", "skipinitialspace", "strict" };

		public string escapechar => _escapechar;

		public string delimiter => _delimiter;

		public bool skipinitialspace => _skipinitialspace;

		public bool doublequote => _doublequote;

		public string lineterminator => _lineterminator;

		public bool strict => _strict;

		public int quoting => _quoting;

		public string quotechar => _quotechar;

		private Dialect()
		{
		}

		public static Dialect Create(CodeContext context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args)
		{
			object value = null;
			object value2 = null;
			object value3 = null;
			object value4 = null;
			object value5 = null;
			object value6 = null;
			object value7 = null;
			object value8 = null;
			object value9 = null;
			Dictionary<string, Dialect> dialects = GetDialects(context);
			if (args.Length > 0 && args[0] != null)
			{
				value = args[0];
			}
			if (value == null)
			{
				kwArgs.TryGetValue("dialect", out value);
			}
			kwArgs.TryGetValue("delimiter", out value2);
			kwArgs.TryGetValue("doublequote", out value3);
			kwArgs.TryGetValue("escapechar", out value4);
			kwArgs.TryGetValue("lineterminator", out value5);
			kwArgs.TryGetValue("quotechar", out value6);
			kwArgs.TryGetValue("quoting", out value7);
			kwArgs.TryGetValue("skipinitialspace", out value8);
			kwArgs.TryGetValue("strict", out value9);
			if (value != null)
			{
				if (value is string)
				{
					string key = (string)value;
					if (dialects.ContainsKey(key))
					{
						return dialects[key];
					}
					throw MakeError("unknown dialect");
				}
				if (value is Dialect && value2 == null && value3 == null && value4 == null && value5 == null && value6 == null && value7 == null && value8 == null && value9 == null)
				{
					return value as Dialect;
				}
			}
			return (value != null) ? new Dialect(context, kwArgs, value) : new Dialect(context, kwArgs);
		}

		[SpecialName]
		public void DeleteMember(CodeContext context, string name)
		{
			if (string.Compare(name, "delimiter") == 0 || string.Compare(name, "skipinitialspace") == 0 || string.Compare(name, "doublequote") == 0 || string.Compare(name, "strict") == 0)
			{
				throw PythonOps.TypeError("readonly attribute");
			}
			if (string.Compare(name, "escapechar") == 0 || string.Compare(name, "lineterminator") == 0 || string.Compare(name, "quotechar") == 0 || string.Compare(name, "quoting") == 0)
			{
				throw PythonOps.AttributeError("attribute '{0}' of '_csv.Dialect' objects is not writable", name);
			}
			throw PythonOps.AttributeError("'_csv.Dialect' object has no attribute '{0}'", name);
		}

		[SpecialName]
		public void SetMember(CodeContext context, string name, object value)
		{
			if (string.Compare(name, "delimiter") == 0 || string.Compare(name, "skipinitialspace") == 0 || string.Compare(name, "doublequote") == 0 || string.Compare(name, "strict") == 0)
			{
				throw PythonOps.TypeError("readonly attribute");
			}
			if (string.Compare(name, "escapechar") == 0 || string.Compare(name, "lineterminator") == 0 || string.Compare(name, "quotechar") == 0 || string.Compare(name, "quoting") == 0)
			{
				throw PythonOps.AttributeError("attribute '{0}' of '_csv.Dialect' objects is not writable", name);
			}
			throw PythonOps.AttributeError("'_csv.Dialect' object has no attribute '{0}'", name);
		}

		private static int SetInt(string name, object src, bool found, int @default)
		{
			int result = @default;
			if (found)
			{
				if (!(src is int))
				{
					throw PythonOps.TypeError("\"{0}\" must be an integer", name);
				}
				result = (int)src;
			}
			return result;
		}

		private static bool SetBool(string name, object src, bool found, bool @default)
		{
			bool result = @default;
			if (found)
			{
				result = PythonOps.IsTrue(src);
			}
			return result;
		}

		private static string SetChar(string name, object src, bool found, string @default)
		{
			string result = @default;
			if (found)
			{
				if (src == null)
				{
					result = null;
				}
				else
				{
					if (!(src is string))
					{
						throw PythonOps.TypeError("\"{0}\" must be a 1-character string", name);
					}
					string text = src as string;
					if (text.Length == 0)
					{
						result = null;
					}
					else
					{
						if (text.Length != 1)
						{
							throw PythonOps.TypeError("\"{0}\" must be a 1-character string", name);
						}
						result = text.Substring(0, 1);
					}
				}
			}
			return result;
		}

		private static string SetString(string name, object src, bool found, string @default)
		{
			string result = @default;
			if (found)
			{
				if (src == null)
				{
					result = null;
				}
				else
				{
					if (!(src is string))
					{
						throw PythonOps.TypeError("\"{0}\" must be an string", name);
					}
					result = src as string;
				}
			}
			return result;
		}

		public Dialect(CodeContext context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args)
		{
			object value = null;
			object value2 = null;
			object value3 = null;
			object value4 = null;
			object value5 = null;
			object value6 = null;
			object value7 = null;
			object value8 = null;
			object value9 = null;
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			foreach (object key in kwArgs.Keys)
			{
				if (Array.IndexOf(VALID_KWARGS, key) < 0)
				{
					throw PythonOps.TypeError("'{0}' is an invalid keyword argument for this function", key);
				}
			}
			if (args.Length > 0 && args[0] != null)
			{
				value = args[0];
				dictionary["dialect"] = true;
			}
			if (value == null)
			{
				dictionary["dialect"] = kwArgs.TryGetValue("dialect", out value);
			}
			dictionary["delimiter"] = kwArgs.TryGetValue("delimiter", out value2);
			dictionary["doublequote"] = kwArgs.TryGetValue("doublequote", out value3);
			dictionary["escapechar"] = kwArgs.TryGetValue("escapechar", out value4);
			dictionary["lineterminator"] = kwArgs.TryGetValue("lineterminator", out value5);
			dictionary["quotechar"] = kwArgs.TryGetValue("quotechar", out value6);
			dictionary["quoting"] = kwArgs.TryGetValue("quoting", out value7);
			dictionary["skipinitialspace"] = kwArgs.TryGetValue("skipinitialspace", out value8);
			dictionary["strict"] = kwArgs.TryGetValue("strict", out value9);
			if (value != null)
			{
				if (!dictionary["delimiter"] && value2 == null)
				{
					dictionary["delimiter"] = PythonOps.TryGetBoundAttr(value, "delimiter", out value2);
				}
				if (!dictionary["doublequote"] && value3 == null)
				{
					dictionary["doublequote"] = PythonOps.TryGetBoundAttr(value, "doublequote", out value3);
				}
				if (!dictionary["escapechar"] && value4 == null)
				{
					dictionary["escapechar"] = PythonOps.TryGetBoundAttr(value, "escapechar", out value4);
				}
				if (!dictionary["lineterminator"] && value5 == null)
				{
					dictionary["lineterminator"] = PythonOps.TryGetBoundAttr(value, "lineterminator", out value5);
				}
				if (!dictionary["quotechar"] && value6 == null)
				{
					dictionary["quotechar"] = PythonOps.TryGetBoundAttr(value, "quotechar", out value6);
				}
				if (!dictionary["quoting"] && value7 == null)
				{
					dictionary["quoting"] = PythonOps.TryGetBoundAttr(value, "quoting", out value7);
				}
				if (!dictionary["skipinitialspace"] && value8 == null)
				{
					dictionary["skipinitialspace"] = PythonOps.TryGetBoundAttr(value, "skipinitialspace", out value8);
				}
				if (!dictionary["strict"] && value9 == null)
				{
					dictionary["strict"] = PythonOps.TryGetBoundAttr(value, "strict", out value9);
				}
			}
			_delimiter = SetChar("delimiter", value2, dictionary["delimiter"], ",");
			_doublequote = SetBool("doublequote", value3, dictionary["doublequote"], @default: true);
			_escapechar = SetString("escapechar", value4, dictionary["escapechar"], null);
			_lineterminator = SetString("lineterminator", value5, dictionary["lineterminator"], "\r\n");
			_quotechar = SetChar("quotechar", value6, dictionary["quotechar"], "\"");
			_quoting = SetInt("quoting", value7, dictionary["quoting"], 0);
			_skipinitialspace = SetBool("skipinitialspace", value8, dictionary["skipinitialspace"], @default: false);
			_strict = SetBool("strict", value9, dictionary["strict"], @default: false);
			if (_quoting < 0 || _quoting > 3)
			{
				throw PythonOps.TypeError("bad \"quoting\" value");
			}
			if (string.IsNullOrEmpty(_delimiter))
			{
				throw PythonOps.TypeError("delimiter must be set");
			}
			if (dictionary["quotechar"] && value6 == null && value7 == null)
			{
				_quoting = 3;
			}
			if (_quoting != 3 && string.IsNullOrEmpty(_quotechar))
			{
				throw PythonOps.TypeError("quotechar must be set if quoting enabled");
			}
			if (_lineterminator == null)
			{
				throw PythonOps.TypeError("lineterminator must be set");
			}
		}
	}

	[Documentation("CSV reader\r\n\r\nReader objects are responsible for reading and parsing tabular data\r\nin CSV format.")]
	[PythonType]
	public class Reader : IEnumerable
	{
		private sealed class ReaderIterator : IEnumerator, IEnumerable
		{
			private enum State
			{
				StartRecord,
				StartField,
				EscapedChar,
				InField,
				InQuotedField,
				EscapeInQuotedField,
				QuoteInQuotedField,
				EatCrNl
			}

			private CodeContext _context;

			private Reader _reader;

			private List _fields = new List();

			private bool _is_numeric_field;

			private State _state;

			private StringBuilder _field = new StringBuilder();

			private IEnumerator _iterator;

			public object Current => new List(_fields);

			public ReaderIterator(CodeContext context, Reader reader)
			{
				_context = context;
				_reader = reader;
				_iterator = _reader._input_iter;
			}

			public bool MoveNext()
			{
				bool flag = false;
				Reset();
				do
				{
					object obj = null;
					if (!_iterator.MoveNext())
					{
						if (_field.Length != 0)
						{
							throw MakeError("newline inside string");
						}
						return false;
					}
					obj = _iterator.Current;
					_reader._line_num++;
					if (obj is char)
					{
						obj = obj.ToString();
					}
					if (!(obj is string))
					{
						throw PythonOps.TypeError("expected string or Unicode object, {0} found", PythonType.GetPythonType(obj.GetType()));
					}
					string text = obj as string;
					if (!string.IsNullOrEmpty(text))
					{
						foreach (char c in text)
						{
							if (c == '\0')
							{
								throw MakeError("line contains NULL byte");
							}
							ProcessChar(c);
						}
					}
					ProcessChar('\0');
					flag = true;
				}
				while (_state != State.StartRecord);
				return flag;
			}

			public void Reset()
			{
				_state = State.StartRecord;
				_fields.Clear();
				_is_numeric_field = false;
				_field.Clear();
			}

			public IEnumerator GetEnumerator()
			{
				return this;
			}

			private void ProcessChar(char c)
			{
				Dialect dialect = _reader._dialect;
				switch (_state)
				{
				case State.StartRecord:
					switch (c)
					{
					case '\0':
						return;
					case '\n':
					case '\r':
						_state = State.EatCrNl;
						return;
					}
					_state = State.StartField;
					goto case State.StartField;
				case State.StartField:
					if (c == '\n' || c == '\r' || c == '\0')
					{
						ParseSaveField();
						_state = ((c != 0) ? State.EatCrNl : State.StartRecord);
					}
					else if (!string.IsNullOrEmpty(dialect.quotechar) && c == dialect.quotechar[0] && dialect.quoting != 3)
					{
						_state = State.InQuotedField;
					}
					else if (!string.IsNullOrEmpty(dialect.escapechar) && c == dialect.escapechar[0])
					{
						_state = State.EscapedChar;
					}
					else
					{
						if (c == ' ' && dialect.skipinitialspace)
						{
							break;
						}
						if (c == dialect.delimiter[0])
						{
							ParseSaveField();
							break;
						}
						if (dialect.quoting == 2)
						{
							_is_numeric_field = true;
						}
						ParseAddChar(c);
						_state = State.InField;
					}
					break;
				case State.EscapedChar:
					if (c == '\0')
					{
						c = '\n';
					}
					ParseAddChar(c);
					_state = State.InField;
					break;
				case State.InField:
					if (c == '\n' || c == '\r' || c == '\0')
					{
						ParseSaveField();
						_state = ((c != 0) ? State.EatCrNl : State.StartRecord);
					}
					else if (!string.IsNullOrEmpty(dialect.escapechar) && c == dialect.escapechar[0])
					{
						_state = State.EscapedChar;
					}
					else if (c == dialect.delimiter[0])
					{
						ParseSaveField();
						_state = State.StartField;
					}
					else
					{
						ParseAddChar(c);
					}
					break;
				case State.InQuotedField:
					if (c == '\0')
					{
						break;
					}
					if (!string.IsNullOrEmpty(dialect.escapechar) && c == dialect.escapechar[0])
					{
						_state = State.EscapeInQuotedField;
					}
					else if (!string.IsNullOrEmpty(dialect.quotechar) && c == dialect.quotechar[0] && dialect.quoting != 3)
					{
						if (dialect.doublequote)
						{
							_state = State.QuoteInQuotedField;
						}
						else
						{
							_state = State.InField;
						}
					}
					else
					{
						ParseAddChar(c);
					}
					break;
				case State.EscapeInQuotedField:
					if (c == '\0')
					{
						c = '\n';
					}
					ParseAddChar(c);
					_state = State.InQuotedField;
					break;
				case State.QuoteInQuotedField:
					if (dialect.quoting != 3 && c == dialect.quotechar[0])
					{
						ParseAddChar(c);
						_state = State.InQuotedField;
						break;
					}
					if (c == dialect.delimiter[0])
					{
						ParseSaveField();
						_state = State.StartField;
						break;
					}
					if (c == '\n' || c == '\r' || c == '\0')
					{
						ParseSaveField();
						_state = ((c != 0) ? State.EatCrNl : State.StartRecord);
						break;
					}
					if (!dialect.strict)
					{
						ParseAddChar(c);
						_state = State.InField;
						break;
					}
					throw MakeError("'{0}' expected after '{1}'", dialect.delimiter, dialect.quotechar);
				case State.EatCrNl:
					switch (c)
					{
					case '\n':
					case '\r':
						break;
					case '\0':
						_state = State.StartRecord;
						break;
					default:
						throw MakeError("new-line character seen in unquoted field - do you need to open the file in universal-newline mode?");
					}
					break;
				}
			}

			private void ParseAddChar(char c)
			{
				int fieldSizeLimit = GetFieldSizeLimit(_context);
				if (_field.Length >= fieldSizeLimit)
				{
					throw MakeError($"field larger than field limit ({fieldSizeLimit})");
				}
				_field.Append(c);
			}

			private void ParseSaveField()
			{
				string text = _field.ToString();
				if (_is_numeric_field)
				{
					_is_numeric_field = false;
					if (!double.TryParse(text, out var result))
					{
						throw PythonOps.ValueError("invalid literal for float(): {0}", text);
					}
					if (text.Contains("."))
					{
						_fields.Add(result);
					}
					else
					{
						_fields.Add((int)result);
					}
				}
				else
				{
					_fields.Add(text);
				}
				_field.Clear();
			}
		}

		private IEnumerator _input_iter;

		private Dialect _dialect;

		private int _line_num;

		private ReaderIterator _iterator;

		public object dialect => _dialect;

		public int line_num => _line_num;

		public Reader(CodeContext context, IEnumerator input_iter, Dialect dialect)
		{
			_input_iter = input_iter;
			_dialect = dialect;
			_iterator = new ReaderIterator(context, this);
		}

		public object next()
		{
			if (!_iterator.MoveNext())
			{
				throw PythonOps.StopIteration();
			}
			return _iterator.Current;
		}

		public IEnumerator GetEnumerator()
		{
			return _iterator;
		}
	}

	[PythonType]
	[Documentation("CSV writer\r\n\r\nWriter objects are responsible for generating tabular data\r\nin CSV format from sequence input.")]
	public class Writer
	{
		private Dialect _dialect;

		private object _writeline;

		private List<string> _rec = new List<string>();

		private int _num_fields;

		public object dialect => _dialect;

		public Writer(CodeContext context, object output_file, Dialect dialect)
		{
			_dialect = dialect;
			if (!PythonOps.TryGetBoundAttr(output_file, "write", out _writeline) || _writeline == null || !PythonOps.IsCallable(context, _writeline))
			{
				throw PythonOps.TypeError("argument 1 must have a \"write\" method");
			}
		}

		[Documentation("writerow(sequence)\r\n\r\nConstruct and write a CSV record from a sequence of fields.  Non-string\r\nelements will be converted to string.")]
		public void writerow(CodeContext context, object sequence)
		{
			IEnumerator enumerator = null;
			if (!PythonOps.TryGetEnumerator(context, sequence, out enumerator))
			{
				throw MakeError("sequence expected");
			}
			int num = PythonOps.Length(sequence);
			JoinReset();
			while (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				bool quoted = false;
				switch (_dialect.quoting)
				{
				case 2:
					quoted = !PythonOps.CheckingConvertToFloat(current) && !PythonOps.CheckingConvertToInt(current) && !PythonOps.CheckingConvertToLong(current);
					break;
				case 1:
					quoted = true;
					break;
				}
				if (current is string)
				{
					JoinAppend((string)current, quoted, num == 1);
				}
				else if (current is double)
				{
					JoinAppend(DoubleOps.__str__(context, (double)current), quoted, num == 1);
				}
				else if (current is float)
				{
					JoinAppend(SingleOps.__str__(context, (float)current), quoted, num == 1);
				}
				else if (current == null)
				{
					JoinAppend(string.Empty, quoted, num == 1);
				}
				else
				{
					JoinAppend(current.ToString(), quoted, num == 1);
				}
			}
			_rec.Add(_dialect.lineterminator);
			PythonOps.CallWithContext(context, _writeline, string.Join("", _rec.ToArray()));
		}

		[Documentation("writerows(sequence of sequences)\r\n\r\nConstruct and write a series of sequences to a csv file.  Non-string \r\nelements will be converted to string.")]
		public void writerows(CodeContext context, object sequence)
		{
			IEnumerator enumerator = null;
			if (!PythonOps.TryGetEnumerator(context, sequence, out enumerator))
			{
				throw PythonOps.TypeError("writerows() argument must be iterable");
			}
			while (enumerator.MoveNext())
			{
				writerow(context, enumerator.Current);
			}
		}

		private void JoinReset()
		{
			_num_fields = 0;
			_rec.Clear();
		}

		private void JoinAppend(string field, bool quoted, bool quote_empty)
		{
			if (_num_fields > 0)
			{
				_rec.Add(_dialect.delimiter);
			}
			List<char> list = new List<char>();
			if (_dialect.quoting == 3)
			{
				list.AddRange(_dialect.lineterminator.ToCharArray());
				if (!string.IsNullOrEmpty(_dialect.escapechar))
				{
					list.Add(_dialect.escapechar[0]);
				}
				if (!string.IsNullOrEmpty(_dialect.delimiter))
				{
					list.Add(_dialect.delimiter[0]);
				}
				if (!string.IsNullOrEmpty(_dialect.quotechar))
				{
					list.Add(_dialect.quotechar[0]);
				}
			}
			else
			{
				List<char> list2 = new List<char>();
				list2.AddRange(_dialect.lineterminator.ToCharArray());
				if (!string.IsNullOrEmpty(_dialect.delimiter))
				{
					list2.Add(_dialect.delimiter[0]);
				}
				if (!string.IsNullOrEmpty(_dialect.escapechar))
				{
					list2.Add(_dialect.escapechar[0]);
				}
				if (field.IndexOfAny(list2.ToArray()) >= 0)
				{
					quoted = true;
				}
				list.Clear();
				if (!string.IsNullOrEmpty(_dialect.quotechar) && field.Contains(_dialect.quotechar))
				{
					if (_dialect.doublequote)
					{
						field = field.Replace(_dialect.quotechar, _dialect.quotechar + _dialect.quotechar);
						quoted = true;
					}
					else
					{
						list.Add(_dialect.quotechar[0]);
					}
				}
			}
			foreach (char item in list)
			{
				if (field.IndexOf(item) >= 0)
				{
					if (string.IsNullOrEmpty(_dialect.escapechar))
					{
						throw MakeError("need to escape, but no escapechar set");
					}
					field = field.Replace(item.ToString(), _dialect.escapechar + item);
				}
			}
			if (string.IsNullOrEmpty(field) && quote_empty)
			{
				if (_dialect.quoting == 3)
				{
					throw MakeError("single empty field record must be quoted");
				}
				quoted = true;
			}
			if (quoted)
			{
				field = _dialect.quotechar + field + _dialect.quotechar;
			}
			_rec.Add(field);
			_num_fields++;
		}
	}

	public const string __doc__ = "";

	public const string __version__ = "1.0";

	public const int QUOTE_MINIMAL = 0;

	public const int QUOTE_ALL = 1;

	public const int QUOTE_NONNUMERIC = 2;

	public const int QUOTE_NONE = 3;

	private const int FieldSizeLimit = 131072;

	private static readonly object _fieldSizeLimitKey = new object();

	private static readonly object _dialectRegistryKey = new object();

	public static PythonType Error;

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		if (!context.HasModuleState(_fieldSizeLimitKey))
		{
			context.SetModuleState(_fieldSizeLimitKey, 131072);
		}
		if (!context.HasModuleState(_dialectRegistryKey))
		{
			context.SetModuleState(_dialectRegistryKey, new Dictionary<string, Dialect>());
		}
		InitModuleExceptions(context, dict);
	}

	public static int field_size_limit(CodeContext context, int new_limit)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		int result = (int)context2.GetModuleState(_fieldSizeLimitKey);
		context2.SetModuleState(_fieldSizeLimitKey, new_limit);
		return result;
	}

	public static int field_size_limit(CodeContext context)
	{
		return (int)PythonContext.GetContext(context).GetModuleState(_fieldSizeLimitKey);
	}

	[Documentation("Create a mapping from a string name to a dialect class.\r\ndialect = csv.register_dialect(name, dialect)")]
	public static void register_dialect(CodeContext context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args)
	{
		string text = null;
		object obj = null;
		Dialect dialect = null;
		if (args.Length < 1)
		{
			throw PythonOps.TypeError("expected at least 1 arguments, got {0}", args.Length);
		}
		if (args.Length > 2)
		{
			throw PythonOps.TypeError("expected at most 2 arguments, got {0}", args.Length);
		}
		if (!(args[0] is string key))
		{
			throw PythonOps.TypeError("dialect name must be a string or unicode");
		}
		if (args.Length > 1)
		{
			obj = args[1];
		}
		dialect = ((obj != null) ? Dialect.Create(context, kwArgs, obj) : Dialect.Create(context, kwArgs));
		if (dialect != null)
		{
			GetDialects(context)[key] = dialect;
		}
	}

	private static Dictionary<string, Dialect> GetDialects(CodeContext context)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (!context2.HasModuleState(_dialectRegistryKey))
		{
			context2.SetModuleState(_dialectRegistryKey, new Dictionary<string, Dialect>());
		}
		return (Dictionary<string, Dialect>)context2.GetModuleState(_dialectRegistryKey);
	}

	private static int GetFieldSizeLimit(CodeContext context)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		if (!context2.HasModuleState(_fieldSizeLimitKey))
		{
			context2.SetModuleState(_fieldSizeLimitKey, 131072);
		}
		return (int)context2.GetModuleState(_fieldSizeLimitKey);
	}

	[Documentation("Delete the name/dialect mapping associated with a string name.\\n\r\n    csv.unregister_dialect(name)")]
	public static void unregister_dialect(CodeContext context, string name)
	{
		Dictionary<string, Dialect> dialects = GetDialects(context);
		if (name == null || !dialects.ContainsKey(name))
		{
			throw MakeError("unknown dialect");
		}
		if (dialects.ContainsKey(name))
		{
			dialects.Remove(name);
		}
	}

	[Documentation("Return the dialect instance associated with name.\r\n    dialect = csv.get_dialect(name)")]
	public static object get_dialect(CodeContext context, string name)
	{
		Dictionary<string, Dialect> dialects = GetDialects(context);
		if (name == null || !dialects.ContainsKey(name))
		{
			throw MakeError("unknown dialect");
		}
		return dialects[name];
	}

	[Documentation("Return a list of all know dialect names\r\n    names = csv.list_dialects()")]
	public static List list_dialects(CodeContext context)
	{
		return new List(GetDialects(context).Keys);
	}

	[Documentation("csv_reader = reader(iterable [, dialect='excel']\r\n                       [optional keyword args])\r\n    for row in csv_reader:\r\n        process(row)\r\n\r\n    The \"iterable\" argument can be any object that returns a line\r\n    of input for each iteration, such as a file object or a list.  The\r\n    optional \"dialect\" parameter is discussed below.  The function\r\n    also accepts optional keyword arguments which override settings\r\n    provided by the dialect.\r\n\r\n    The returned object is an iterator.  Each iteration returns a row\r\n    of the CSV file (which can span multiple input lines)")]
	public static object reader(CodeContext context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args)
	{
		object obj = null;
		Dialect dialect = null;
		IEnumerator enumerator = null;
		Dictionary<string, Dialect> dialects = GetDialects(context);
		if (args.Length < 1)
		{
			throw PythonOps.TypeError("expected at least 1 arguments, got {0}", args.Length);
		}
		if (args.Length > 2)
		{
			throw PythonOps.TypeError("expected at most 2 arguments, got {0}", args.Length);
		}
		if (!PythonOps.TryGetEnumerator(context, args[0], out enumerator))
		{
			throw PythonOps.TypeError("argument 1 must be an iterator");
		}
		if (args.Length > 1)
		{
			obj = args[1];
		}
		if (obj is string && !dialects.ContainsKey((string)obj))
		{
			throw MakeError("unknown dialect");
		}
		if (obj is string)
		{
			dialect = dialects[(string)obj];
			obj = dialect;
		}
		dialect = ((obj != null) ? Dialect.Create(context, kwArgs, obj) : Dialect.Create(context, kwArgs));
		return new Reader(context, enumerator, dialect);
	}

	public static object writer(CodeContext context, [ParamDictionary] IDictionary<object, object> kwArgs, params object[] args)
	{
		object obj = null;
		object obj2 = null;
		Dialect dialect = null;
		Dictionary<string, Dialect> dialects = GetDialects(context);
		if (args.Length < 1)
		{
			throw PythonOps.TypeError("expected at least 1 arguments, got {0}", args.Length);
		}
		if (args.Length > 2)
		{
			throw PythonOps.TypeError("expected at most 2 arguments, got {0}", args.Length);
		}
		obj = args[0];
		if (args.Length > 1)
		{
			obj2 = args[1];
		}
		if (obj2 is string && !dialects.ContainsKey((string)obj2))
		{
			throw MakeError("unknown dialect");
		}
		if (obj2 is string)
		{
			dialect = dialects[(string)obj2];
			obj2 = dialect;
		}
		dialect = ((obj2 != null) ? Dialect.Create(context, kwArgs, obj2) : Dialect.Create(context, kwArgs));
		return new Writer(context, obj, dialect);
	}

	internal static Exception MakeError(params object[] args)
	{
		return PythonOps.CreateThrowable(Error, args);
	}

	private static void InitModuleExceptions(PythonContext context, PythonDictionary dict)
	{
		Error = context.EnsureModuleException("csv.Error", PythonExceptions.StandardError, dict, "Error", "_csv");
	}
}
