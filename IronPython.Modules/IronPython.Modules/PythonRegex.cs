using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonRegex
{
	[PythonType]
	public class RE_Pattern : IWeakReferenceable
	{
		internal Regex _re;

		private PythonDictionary _groups;

		private int _compileFlags;

		private WeakRefTracker _weakRefTracker;

		internal ParsedRegex _pre;

		public int flags => _compileFlags;

		public PythonDictionary groupindex
		{
			get
			{
				if (_groups == null)
				{
					PythonDictionary pythonDictionary = new PythonDictionary();
					string[] groupNames = _re.GetGroupNames();
					int[] groupNumbers = _re.GetGroupNumbers();
					for (int i = 1; i < groupNames.Length; i++)
					{
						if (!char.IsDigit(groupNames[i][0]) && !groupNames[i].StartsWith("___PyRegexNameMangled"))
						{
							pythonDictionary[groupNames[i]] = groupNumbers[i];
						}
					}
					_groups = pythonDictionary;
				}
				return _groups;
			}
		}

		public int groups => _re.GetGroupNumbers().Length - 1;

		public string pattern => _pre.UserPattern;

		internal RE_Pattern(CodeContext context, object pattern)
			: this(context, pattern, 0)
		{
		}

		internal RE_Pattern(CodeContext context, object pattern, int flags)
			: this(context, pattern, flags, compiled: false)
		{
		}

		internal RE_Pattern(CodeContext context, object pattern, int flags, bool compiled)
		{
			_pre = PreParseRegex(context, ValidatePatternAsString(pattern));
			try
			{
				flags |= OptionToFlags(_pre.Options);
				RegexOptions regexOptions = FlagsToOption(flags);
				_re = new Regex(_pre.Pattern, (RegexOptions)((int)regexOptions | (compiled ? 8 : 0)));
			}
			catch (ArgumentException ex)
			{
				throw PythonExceptions.CreateThrowable(error(context), ex.Message);
			}
			_compileFlags = flags;
		}

		public RE_Match match(object text)
		{
			string text2 = ValidateString(text, "text");
			return RE_Match.makeMatch(_re.Match(text2), this, text2, 0, text2.Length);
		}

		private static int FixPosition(string text, int position)
		{
			if (position < 0)
			{
				return 0;
			}
			if (position > text.Length)
			{
				return text.Length;
			}
			return position;
		}

		public RE_Match match(object text, int pos)
		{
			string text2 = ValidateString(text, "text");
			pos = FixPosition(text2, pos);
			return RE_Match.makeMatch(_re.Match(text2, pos), this, text2, pos, text2.Length);
		}

		public RE_Match match(object text, [DefaultParameterValue(0)] int pos, int endpos)
		{
			string text2 = ValidateString(text, "text");
			pos = FixPosition(text2, pos);
			endpos = FixPosition(text2, endpos);
			return RE_Match.makeMatch(_re.Match(text2.Substring(0, endpos), pos), this, text2, pos, endpos);
		}

		public RE_Match search(object text)
		{
			string input = ValidateString(text, "text");
			return RE_Match.make(_re.Match(input), this, input);
		}

		public RE_Match search(object text, int pos)
		{
			string text2 = ValidateString(text, "text");
			return RE_Match.make(_re.Match(text2, pos, text2.Length - pos), this, text2);
		}

		public RE_Match search(object text, int pos, int endpos)
		{
			string text2 = ValidateString(text, "text");
			return RE_Match.make(_re.Match(text2, pos, Math.Min(Math.Max(endpos - pos, 0), text2.Length - pos)), this, text2);
		}

		public object findall(CodeContext context, string @string)
		{
			return findall(context, @string, 0, null);
		}

		public object findall(CodeContext context, string @string, int pos)
		{
			return findall(context, @string, pos, null);
		}

		public object findall(CodeContext context, object @string, int pos, object endpos)
		{
			MatchCollection mc = FindAllWorker(context, ValidateString(@string, "text"), pos, endpos);
			return FixFindAllMatch(this, mc, FindMaker(@string));
		}

		internal MatchCollection FindAllWorker(CodeContext context, string str, int pos, object endpos)
		{
			string text = str;
			if (endpos != null)
			{
				int val = PythonContext.GetContext(context).ConvertToInt32(endpos);
				text = text.Substring(0, Math.Max(val, 0));
			}
			return _re.Matches(text, pos);
		}

		internal MatchCollection FindAllWorker(CodeContext context, IList<byte> str, int pos, object endpos)
		{
			string text = str.MakeString();
			if (endpos != null)
			{
				int val = PythonContext.GetContext(context).ConvertToInt32(endpos);
				text = text.Substring(0, Math.Max(val, 0));
			}
			return _re.Matches(text, pos);
		}

		public object finditer(CodeContext context, object @string)
		{
			string text = ValidateString(@string, "string");
			return MatchIterator(FindAllWorker(context, text, 0, text.Length), this, text);
		}

		public object finditer(CodeContext context, object @string, int pos)
		{
			string text = ValidateString(@string, "string");
			return MatchIterator(FindAllWorker(context, text, pos, text.Length), this, text);
		}

		public object finditer(CodeContext context, object @string, int pos, int endpos)
		{
			string text = ValidateString(@string, "string");
			return MatchIterator(FindAllWorker(context, text, pos, endpos), this, text);
		}

		[return: SequenceTypeInfo(new Type[] { typeof(string) })]
		public List split(string @string)
		{
			return split(@string, 0);
		}

		[return: SequenceTypeInfo(new Type[] { typeof(string) })]
		public List split(object @string, int maxsplit)
		{
			List list = new List();
			if (maxsplit < 0)
			{
				list.AddNoLock(ValidateString(@string, "string"));
			}
			else
			{
				string text = ValidateString(@string, "string");
				MatchCollection matchCollection = _re.Matches(text);
				int num = 0;
				int num2 = 0;
				foreach (Match item in matchCollection)
				{
					if (item.Length <= 0)
					{
						continue;
					}
					list.AddNoLock(text.Substring(num, item.Index - num));
					if (item.Groups.Count > 1)
					{
						for (int i = 1; i < item.Groups.Count; i++)
						{
							if (item.Groups[i].Success)
							{
								list.AddNoLock(item.Groups[i].Value);
							}
							else
							{
								list.AddNoLock(null);
							}
						}
					}
					num = item.Index + item.Length;
					num2++;
					if (num2 == maxsplit)
					{
						break;
					}
				}
				list.AddNoLock(text.Substring(num));
			}
			return list;
		}

		public string sub(CodeContext context, object repl, object @string)
		{
			return sub(context, repl, ValidateString(@string, "string"), int.MaxValue);
		}

		public string sub(CodeContext context, object repl, object @string, int count)
		{
			if (repl == null)
			{
				throw PythonOps.TypeError("NoneType is not valid repl");
			}
			if (count == 0)
			{
				count = int.MaxValue;
			}
			string replacement = repl as string;
			if (replacement == null)
			{
				if (repl is ExtensibleString)
				{
					replacement = ((ExtensibleString)repl).Value;
				}
				else if (repl is Bytes)
				{
					replacement = ((Bytes)repl).ToString();
				}
			}
			Match prev = null;
			string input = ValidateString(@string, "string");
			return _re.Replace(input, delegate(Match match)
			{
				if (string.IsNullOrEmpty(match.Value) && prev != null && prev.Index + prev.Length == match.Index)
				{
					return "";
				}
				prev = match;
				return (replacement != null) ? UnescapeGroups(match, replacement) : (PythonCalls.Call(context, repl, RE_Match.make(match, this, input)) as string);
			}, count);
		}

		public object subn(CodeContext context, object repl, string @string)
		{
			return subn(context, repl, @string, int.MaxValue);
		}

		public object subn(CodeContext context, object repl, object @string, int count)
		{
			if (repl == null)
			{
				throw PythonOps.TypeError("NoneType is not valid repl");
			}
			if (count == 0)
			{
				count = int.MaxValue;
			}
			int totalCount = 0;
			string replacement = repl as string;
			if (replacement == null)
			{
				if (repl is ExtensibleString)
				{
					replacement = ((ExtensibleString)repl).Value;
				}
				else if (repl is Bytes)
				{
					replacement = ((Bytes)repl).ToString();
				}
			}
			Match prev = null;
			string input = ValidateString(@string, "string");
			string text = _re.Replace(input, delegate(Match match)
			{
				if (string.IsNullOrEmpty(match.Value) && prev != null && prev.Index + prev.Length == match.Index)
				{
					return "";
				}
				prev = match;
				totalCount++;
				return (replacement != null) ? UnescapeGroups(match, replacement) : (PythonCalls.Call(context, repl, RE_Match.make(match, this, input)) as string);
			}, count);
			return PythonTuple.MakeTuple(text, totalCount);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is RE_Pattern rE_Pattern))
			{
				return false;
			}
			if (rE_Pattern.pattern == pattern)
			{
				return rE_Pattern.flags == flags;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return pattern.GetHashCode() ^ flags;
		}

		WeakRefTracker IWeakReferenceable.GetWeakRef()
		{
			return _weakRefTracker;
		}

		bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
		{
			_weakRefTracker = value;
			return true;
		}

		void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
		{
			((IWeakReferenceable)this).SetWeakRef(value);
		}
	}

	[PythonType]
	public class RE_Match
	{
		private RE_Pattern _pattern;

		private Match _m;

		private string _text;

		private int _lastindex = -1;

		private int _pos;

		private int _endPos;

		public int pos => _pos;

		public int endpos => _endPos;

		public string @string => _text;

		public PythonTuple regs
		{
			get
			{
				object[] array = new object[_m.Groups.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = PythonTuple.MakeTuple(start(i), end(i));
				}
				return PythonTuple.MakeTuple(array);
			}
		}

		public RE_Pattern re => _pattern;

		public object lastindex
		{
			get
			{
				if (_lastindex == -1)
				{
					int i = 1;
					while (i < _m.Groups.Count)
					{
						if (_m.Groups[i].Success)
						{
							_lastindex = i;
							int index = _m.Groups[i].Index;
							int num = index + _m.Groups[i].Length;
							for (i++; i < _m.Groups.Count && _m.Groups[i].Index < num; i++)
							{
							}
						}
						else
						{
							i++;
						}
					}
					if (_lastindex == -1)
					{
						_lastindex = 0;
					}
				}
				if (_lastindex == 0)
				{
					return null;
				}
				return _lastindex;
			}
		}

		public string lastgroup
		{
			get
			{
				if (lastindex == null)
				{
					return null;
				}
				return _pattern._re.GroupNameFromNumber((int)lastindex);
			}
		}

		internal static RE_Match make(Match m, RE_Pattern pattern, string input)
		{
			if (m.Success)
			{
				return new RE_Match(m, pattern, input, 0, input.Length);
			}
			return null;
		}

		internal static RE_Match make(Match m, RE_Pattern pattern, string input, int offset, int endpos)
		{
			if (m.Success)
			{
				return new RE_Match(m, pattern, input, offset, endpos);
			}
			return null;
		}

		internal static RE_Match makeMatch(Match m, RE_Pattern pattern, string input, int offset, int endpos)
		{
			if (m.Success && m.Index == offset)
			{
				return new RE_Match(m, pattern, input, offset, endpos);
			}
			return null;
		}

		public RE_Match(Match m, RE_Pattern pattern, string text)
		{
			_m = m;
			_pattern = pattern;
			_text = text;
		}

		public RE_Match(Match m, RE_Pattern pattern, string text, int pos, int endpos)
		{
			_m = m;
			_pattern = pattern;
			_text = text;
			_pos = pos;
			_endPos = endpos;
		}

		public int end()
		{
			return _m.Index + _m.Length;
		}

		public int start()
		{
			return _m.Index;
		}

		public int start(object group)
		{
			int groupIndex = GetGroupIndex(group);
			if (!_m.Groups[groupIndex].Success)
			{
				return -1;
			}
			return _m.Groups[groupIndex].Index;
		}

		public int end(object group)
		{
			int groupIndex = GetGroupIndex(group);
			if (!_m.Groups[groupIndex].Success)
			{
				return -1;
			}
			return _m.Groups[groupIndex].Index + _m.Groups[groupIndex].Length;
		}

		public object group(object index, params object[] additional)
		{
			if (additional.Length == 0)
			{
				return group(index);
			}
			object[] array = new object[additional.Length + 1];
			array[0] = (_m.Groups[GetGroupIndex(index)].Success ? _m.Groups[GetGroupIndex(index)].Value : null);
			for (int i = 1; i < array.Length; i++)
			{
				int groupIndex = GetGroupIndex(additional[i - 1]);
				array[i] = (_m.Groups[groupIndex].Success ? _m.Groups[groupIndex].Value : null);
			}
			return PythonTuple.MakeTuple(array);
		}

		public string group(object index)
		{
			int groupIndex = GetGroupIndex(index);
			Group obj = _m.Groups[groupIndex];
			if (!obj.Success)
			{
				return null;
			}
			return obj.Value;
		}

		public string group()
		{
			return group(0);
		}

		[return: SequenceTypeInfo(new Type[] { typeof(string) })]
		public PythonTuple groups()
		{
			return groups(null);
		}

		public PythonTuple groups(object @default)
		{
			object[] array = new object[_m.Groups.Count - 1];
			for (int i = 1; i < _m.Groups.Count; i++)
			{
				if (!_m.Groups[i].Success)
				{
					array[i - 1] = @default;
				}
				else
				{
					array[i - 1] = _m.Groups[i].Value;
				}
			}
			return PythonTuple.MakeTuple(array);
		}

		public string expand(object template)
		{
			string text = ValidateString(template, "template");
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] != '\\')
				{
					stringBuilder.Append(text[i]);
					continue;
				}
				if (++i == text.Length)
				{
					stringBuilder.Append(text[i - 1]);
					continue;
				}
				if (char.IsDigit(text[i]))
				{
					AppendGroup(stringBuilder, text[i] - 48);
					continue;
				}
				if (text[i] == 'g')
				{
					if (++i == text.Length)
					{
						stringBuilder.Append("\\g");
						return stringBuilder.ToString();
					}
					if (text[i] != '<')
					{
						stringBuilder.Append("\\g<");
						continue;
					}
					StringBuilder stringBuilder2 = new StringBuilder();
					i++;
					while (text[i] != '>' && i < text.Length)
					{
						stringBuilder2.Append(text[i++]);
					}
					AppendGroup(stringBuilder, _pattern._re.GroupNumberFromName(stringBuilder2.ToString()));
					continue;
				}
				switch (text[i])
				{
				case 'n':
					stringBuilder.Append('\n');
					break;
				case 'r':
					stringBuilder.Append('\r');
					break;
				case 't':
					stringBuilder.Append('\t');
					break;
				case '\\':
					stringBuilder.Append('\\');
					break;
				}
			}
			return stringBuilder.ToString();
		}

		[return: DictionaryTypeInfo(typeof(string), typeof(string))]
		public PythonDictionary groupdict()
		{
			return groupdict(null);
		}

		private static bool IsGroupNumber(string name)
		{
			foreach (char c in name)
			{
				if (!char.IsNumber(c))
				{
					return false;
				}
			}
			return true;
		}

		[return: DictionaryTypeInfo(typeof(string), typeof(string))]
		public PythonDictionary groupdict([NotNull] string value)
		{
			return groupdict((object)value);
		}

		[return: DictionaryTypeInfo(typeof(string), typeof(object))]
		public PythonDictionary groupdict(object value)
		{
			string[] groupNames = _pattern._re.GetGroupNames();
			PythonDictionary pythonDictionary = new PythonDictionary();
			for (int i = 0; i < groupNames.Length; i++)
			{
				if (!IsGroupNumber(groupNames[i]))
				{
					if (_m.Groups[i].Captures.Count != 0)
					{
						pythonDictionary[groupNames[i]] = _m.Groups[i].Value;
					}
					else
					{
						pythonDictionary[groupNames[i]] = value;
					}
				}
			}
			return pythonDictionary;
		}

		[return: SequenceTypeInfo(new Type[] { typeof(int) })]
		public PythonTuple span()
		{
			return PythonTuple.MakeTuple(start(), end());
		}

		[return: SequenceTypeInfo(new Type[] { typeof(int) })]
		public PythonTuple span(object group)
		{
			return PythonTuple.MakeTuple(start(group), end(group));
		}

		private void AppendGroup(StringBuilder sb, int index)
		{
			sb.Append(_m.Groups[index].Value);
		}

		private int GetGroupIndex(object group)
		{
			if (!Converter.TryConvertToInt32(group, out var result))
			{
				result = _pattern._re.GroupNumberFromName(ValidateString(group, "group"));
			}
			if (result < 0 || result >= _m.Groups.Count)
			{
				throw PythonOps.IndexError("no such group");
			}
			return result;
		}
	}

	internal class ParsedRegex
	{
		public string UserPattern;

		public string Pattern;

		public RegexOptions Options = RegexOptions.CultureInvariant;

		public ParsedRegex(string pattern)
		{
			UserPattern = pattern;
		}
	}

	private class PatternKey : IEquatable<PatternKey>
	{
		public string Pattern;

		public int Flags;

		public PatternKey(string pattern, int flags)
		{
			Pattern = pattern;
			Flags = flags;
		}

		public override bool Equals(object obj)
		{
			if (obj is PatternKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Pattern.GetHashCode() ^ Flags;
		}

		public bool Equals(PatternKey other)
		{
			if (other.Pattern == Pattern)
			{
				return other.Flags == Flags;
			}
			return false;
		}
	}

	public const int I = 2;

	public const int L = 4;

	public const int M = 8;

	public const int S = 16;

	public const int U = 32;

	public const int X = 64;

	public const int IGNORECASE = 2;

	public const int LOCALE = 4;

	public const int MULTILINE = 8;

	public const int DOTALL = 16;

	public const int UNICODE = 32;

	public const int VERBOSE = 64;

	public const string engine = "cli reg ex";

	private const string _mangledNamedGroup = "___PyRegexNameMangled";

	private static CacheDict<PatternKey, RE_Pattern> _cachedPatterns = new CacheDict<PatternKey, RE_Pattern>(100);

	private static readonly Random r = new Random(DateTime.Now.Millisecond);

	private static char[] _preParsedChars = new char[4] { '(', '{', '[', ']' };

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.EnsureModuleException("reerror", dict, "error", "re");
		PythonCopyReg.GetDispatchTable(context.SharedContext)[DynamicHelpers.GetPythonTypeFromType(typeof(RE_Pattern))] = dict["_pickle"];
	}

	public static RE_Pattern compile(CodeContext context, object pattern)
	{
		try
		{
			return new RE_Pattern(context, ValidatePattern(pattern), 0, compiled: true);
		}
		catch (ArgumentException ex)
		{
			throw PythonExceptions.CreateThrowable(error(context), ex.Message);
		}
	}

	public static RE_Pattern compile(CodeContext context, object pattern, object flags)
	{
		try
		{
			return new RE_Pattern(context, ValidatePattern(pattern), PythonContext.GetContext(context).ConvertToInt32(flags), compiled: true);
		}
		catch (ArgumentException ex)
		{
			throw PythonExceptions.CreateThrowable(error(context), ex.Message);
		}
	}

	public static string escape(string text)
	{
		if (text == null)
		{
			throw PythonOps.TypeError("text must not be None");
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (char.IsLetterOrDigit(text[i]))
			{
				continue;
			}
			StringBuilder stringBuilder = new StringBuilder(text, 0, i, text.Length);
			char c = text[i];
			do
			{
				stringBuilder.Append('\\');
				stringBuilder.Append(c);
				i++;
				int num = i;
				for (; i < text.Length; i++)
				{
					c = text[i];
					if (!char.IsLetterOrDigit(c))
					{
						break;
					}
				}
				stringBuilder.Append(text, num, i - num);
			}
			while (i < text.Length);
			return stringBuilder.ToString();
		}
		return text;
	}

	public static List findall(CodeContext context, object pattern, string @string)
	{
		return findall(context, pattern, @string, 0);
	}

	public static List findall(CodeContext context, object pattern, string @string, int flags)
	{
		RE_Pattern pattern2 = GetPattern(context, ValidatePattern(pattern), flags);
		ValidateString(@string, "string");
		MatchCollection mc = pattern2.FindAllWorker(context, @string, 0, @string.Length);
		return FixFindAllMatch(pattern2, mc, null);
	}

	public static List findall(CodeContext context, object pattern, IList<byte> @string)
	{
		return findall(context, pattern, @string, 0);
	}

	public static List findall(CodeContext context, object pattern, IList<byte> @string, int flags)
	{
		RE_Pattern pattern2 = GetPattern(context, ValidatePattern(pattern), flags);
		ValidateString(@string, "string");
		MatchCollection mc = pattern2.FindAllWorker(context, @string, 0, @string.Count);
		return FixFindAllMatch(pattern2, mc, FindMaker(@string));
	}

	private static Func<string, object> FindMaker(object input)
	{
		Func<string, object> result = null;
		if (input is ByteArray)
		{
			result = (string x) => new ByteArray(x.MakeByteArray());
		}
		return result;
	}

	private static List FixFindAllMatch(RE_Pattern pat, MatchCollection mc, Func<string, object> maker)
	{
		object[] array = new object[mc.Count];
		int num = pat._re.GetGroupNumbers().Length;
		for (int i = 0; i < mc.Count; i++)
		{
			if (num > 2)
			{
				int num2 = 0;
				List<object> list = new List<object>();
				foreach (Group group in mc[i].Groups)
				{
					if (num2++ != 0)
					{
						list.Add((maker != null) ? maker(group.Value) : group.Value);
					}
				}
				array[i] = PythonTuple.Make(list);
			}
			else if (num == 2)
			{
				array[i] = ((maker != null) ? maker(mc[i].Groups[1].Value) : mc[i].Groups[1].Value);
			}
			else
			{
				array[i] = ((maker != null) ? maker(mc[i].Value) : mc[i].Value);
			}
		}
		return List.FromArrayNoCopy(array);
	}

	public static object finditer(CodeContext context, object pattern, object @string)
	{
		return finditer(context, pattern, @string, 0);
	}

	public static object finditer(CodeContext context, object pattern, object @string, int flags)
	{
		RE_Pattern pattern2 = GetPattern(context, ValidatePattern(pattern), flags);
		string text = ValidateString(@string, "string");
		return MatchIterator(pattern2.FindAllWorker(context, text, 0, text.Length), pattern2, text);
	}

	public static RE_Match match(CodeContext context, object pattern, object @string)
	{
		return match(context, pattern, @string, 0);
	}

	public static RE_Match match(CodeContext context, object pattern, object @string, int flags)
	{
		return GetPattern(context, ValidatePattern(pattern), flags).match(ValidateString(@string, "string"));
	}

	public static RE_Match search(CodeContext context, object pattern, object @string)
	{
		return search(context, pattern, @string, 0);
	}

	public static RE_Match search(CodeContext context, object pattern, object @string, int flags)
	{
		return GetPattern(context, ValidatePattern(pattern), flags).search(ValidateString(@string, "string"));
	}

	[return: SequenceTypeInfo(new Type[] { typeof(string) })]
	public static List split(CodeContext context, object pattern, object @string)
	{
		return split(context, ValidatePattern(pattern), ValidateString(@string, "string"), 0);
	}

	[return: SequenceTypeInfo(new Type[] { typeof(string) })]
	public static List split(CodeContext context, object pattern, object @string, int maxsplit)
	{
		return GetPattern(context, ValidatePattern(pattern), 0).split(ValidateString(@string, "string"), maxsplit);
	}

	public static string sub(CodeContext context, object pattern, object repl, object @string)
	{
		return sub(context, pattern, repl, @string, int.MaxValue);
	}

	public static string sub(CodeContext context, object pattern, object repl, object @string, int count)
	{
		return GetPattern(context, ValidatePattern(pattern), 0).sub(context, repl, ValidateString(@string, "string"), count);
	}

	public static object subn(CodeContext context, object pattern, object repl, object @string)
	{
		return subn(context, pattern, repl, @string, int.MaxValue);
	}

	public static object subn(CodeContext context, object pattern, object repl, object @string, int count)
	{
		return GetPattern(context, ValidatePattern(pattern), 0).subn(context, repl, ValidateString(@string, "string"), count);
	}

	public static void purge()
	{
		_cachedPatterns = new CacheDict<PatternKey, RE_Pattern>(100);
	}

	public static PythonTuple _pickle(CodeContext context, RE_Pattern pattern)
	{
		object obj = Importer.ImportModule(context, new PythonDictionary(), "re", bottom: false, 0);
		if (obj is PythonModule && ((PythonModule)obj).__dict__.TryGetValue("compile", out var value))
		{
			return PythonTuple.MakeTuple(value, PythonTuple.MakeTuple(pattern.pattern, pattern.flags));
		}
		throw new InvalidOperationException("couldn't find compile method");
	}

	private static RE_Pattern GetPattern(CodeContext context, object pattern, int flags)
	{
		RE_Pattern value = pattern as RE_Pattern;
		if (value != null)
		{
			return value;
		}
		string pattern2 = ValidatePatternAsString(pattern);
		PatternKey key = new PatternKey(pattern2, flags);
		lock (_cachedPatterns)
		{
			if (_cachedPatterns.TryGetValue(new PatternKey(pattern2, flags), out value))
			{
				return value;
			}
			value = new RE_Pattern(context, pattern2, flags);
			_cachedPatterns[key] = value;
			return value;
		}
	}

	private static IEnumerator MatchIterator(MatchCollection matches, RE_Pattern pattern, string input)
	{
		for (int i = 0; i < matches.Count; i++)
		{
			yield return RE_Match.make(matches[i], pattern, input, 0, input.Length);
		}
	}

	private static RegexOptions FlagsToOption(int flags)
	{
		RegexOptions regexOptions = RegexOptions.None;
		if ((flags & 2) != 0)
		{
			regexOptions |= RegexOptions.IgnoreCase;
		}
		if ((flags & 8) != 0)
		{
			regexOptions |= RegexOptions.Multiline;
		}
		if ((flags & 4) == 0)
		{
			regexOptions &= ~RegexOptions.CultureInvariant;
		}
		if ((flags & 0x10) != 0)
		{
			regexOptions |= RegexOptions.Singleline;
		}
		if ((flags & 0x40) != 0)
		{
			regexOptions |= RegexOptions.IgnorePatternWhitespace;
		}
		return regexOptions;
	}

	private static int OptionToFlags(RegexOptions options)
	{
		int num = 0;
		if ((options & RegexOptions.IgnoreCase) != RegexOptions.None)
		{
			num |= 2;
		}
		if ((options & RegexOptions.Multiline) != RegexOptions.None)
		{
			num |= 8;
		}
		if ((options & RegexOptions.CultureInvariant) == 0)
		{
			num |= 4;
		}
		if ((options & RegexOptions.Singleline) != RegexOptions.None)
		{
			num |= 0x10;
		}
		if ((options & RegexOptions.IgnorePatternWhitespace) != RegexOptions.None)
		{
			num |= 0x40;
		}
		return num;
	}

	private static ParsedRegex PreParseRegex(CodeContext context, string pattern)
	{
		ParsedRegex parsedRegex = new ParsedRegex(pattern);
		int num = 0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		while (true)
		{
			int nameIndex = pattern.IndexOfAny(_preParsedChars, num);
			if (nameIndex > 0 && pattern[nameIndex - 1] == '\\')
			{
				int num3 = nameIndex - 2;
				int num4 = 1;
				while (num3 >= 0 && pattern[num3] == '\\')
				{
					num4++;
					num3--;
				}
				if ((num4 & 1) != 0)
				{
					num++;
					continue;
				}
			}
			if (nameIndex == -1 || nameIndex == pattern.Length - 1)
			{
				break;
			}
			switch (pattern[nameIndex])
			{
			case '{':
				if (pattern[++nameIndex] == ',')
				{
					pattern = pattern.Insert(nameIndex, "0");
				}
				break;
			case '[':
				nameIndex++;
				flag = true;
				break;
			case ']':
				nameIndex++;
				flag = false;
				break;
			case '(':
				if (!flag)
				{
					char c = pattern[++nameIndex];
					if (c == '?')
					{
						if (nameIndex == pattern.Length - 1)
						{
							throw PythonExceptions.CreateThrowable(error(context), "unexpected end of regex");
						}
						switch (pattern[++nameIndex])
						{
						case 'P':
							if (nameIndex + 1 < pattern.Length && pattern[nameIndex + 1] == '=')
							{
								pattern = pattern.Remove(nameIndex - 2, 4);
								pattern = pattern.Insert(nameIndex - 2, "\\k<");
								int i;
								for (i = nameIndex; i < pattern.Length && pattern[i] != ')'; i++)
								{
								}
								if (i == pattern.Length)
								{
									throw PythonExceptions.CreateThrowable(error(context), "unexpected end of regex");
								}
								pattern = pattern.Substring(0, i) + ">" + pattern.Substring(i + 1);
							}
							else
							{
								flag2 = true;
								pattern = pattern.Remove(nameIndex, 1);
							}
							break;
						case 'i':
							parsedRegex.Options |= RegexOptions.IgnoreCase;
							break;
						case 'L':
							parsedRegex.Options &= ~RegexOptions.CultureInvariant;
							RemoveOption(ref pattern, ref nameIndex);
							break;
						case 'm':
							parsedRegex.Options |= RegexOptions.Multiline;
							break;
						case 's':
							parsedRegex.Options |= RegexOptions.Singleline;
							break;
						case 'u':
							RemoveOption(ref pattern, ref nameIndex);
							break;
						case 'x':
							parsedRegex.Options |= RegexOptions.IgnorePatternWhitespace;
							break;
						case '(':
							nameIndex++;
							break;
						default:
							throw PythonExceptions.CreateThrowable(error(context), "Unrecognized extension " + pattern[nameIndex]);
						case '!':
						case '#':
						case ':':
						case '<':
						case '=':
							break;
						}
					}
					else
					{
						num2++;
						if (flag2)
						{
							pattern = pattern.Insert(nameIndex, "?<___PyRegexNameMangled" + GetRandomString() + ">");
						}
					}
				}
				else
				{
					nameIndex++;
				}
				break;
			}
			num = nameIndex;
		}
		num = 0;
		do
		{
			int nameIndex = pattern.IndexOf('\\', num);
			if (nameIndex == -1 || nameIndex == pattern.Length - 1)
			{
				break;
			}
			num = ++nameIndex;
			char c2 = pattern[num];
			switch (c2)
			{
			case 'A':
			case 'D':
			case 'P':
			case 'S':
			case 'W':
			case 'Z':
			case '\\':
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'k':
			case 'n':
			case 'p':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
				continue;
			}
			switch (char.GetUnicodeCategory(c2))
			{
			case UnicodeCategory.UppercaseLetter:
			case UnicodeCategory.LowercaseLetter:
			case UnicodeCategory.TitlecaseLetter:
			case UnicodeCategory.ModifierLetter:
			case UnicodeCategory.OtherLetter:
			case UnicodeCategory.LetterNumber:
			case UnicodeCategory.OtherNumber:
			case UnicodeCategory.ConnectorPunctuation:
				pattern = pattern.Remove(nameIndex - 1, 1);
				num--;
				break;
			}
		}
		while (++num < pattern.Length);
		parsedRegex.Pattern = pattern;
		return parsedRegex;
	}

	private static void RemoveOption(ref string pattern, ref int nameIndex)
	{
		if (pattern[nameIndex - 1] == '?' && nameIndex < pattern.Length - 1 && pattern[nameIndex + 1] == ')')
		{
			pattern = pattern.Remove(nameIndex - 2, 4);
			nameIndex -= 2;
		}
		else
		{
			pattern = pattern.Remove(nameIndex--, 1);
		}
	}

	private static string GetRandomString()
	{
		return r.Next(1073741823, int.MaxValue).ToString();
	}

	private static string UnescapeGroups(Match m, string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != '\\')
			{
				continue;
			}
			StringBuilder stringBuilder = new StringBuilder(text, 0, i, text.Length);
			do
			{
				if (text[i] == '\\')
				{
					i++;
					if (i == text.Length)
					{
						stringBuilder.Append('\\');
						break;
					}
					switch (text[i])
					{
					case 'n':
						stringBuilder.Append('\n');
						continue;
					case 'r':
						stringBuilder.Append('\r');
						continue;
					case 't':
						stringBuilder.Append('\t');
						continue;
					case '\\':
						stringBuilder.Append('\\');
						continue;
					case '\'':
						stringBuilder.Append('\'');
						continue;
					case 'b':
						stringBuilder.Append('\b');
						continue;
					case 'g':
						if (text[i + 1] == '<')
						{
							int num = i + 1;
							int num2 = text.IndexOf('>', i + 2);
							if (num2 == -1)
							{
								continue;
							}
							int length = num2 - (num + 1);
							string text2 = text.Substring(num + 1, length);
							if (StringUtils.TryParseInt32(text2, out var result))
							{
								Group obj = m.Groups[result];
								if (string.IsNullOrEmpty(obj.Value))
								{
									throw PythonOps.IndexError("unknown group reference");
								}
								stringBuilder.Append(obj.Value);
							}
							else
							{
								Group obj = m.Groups[text2];
								if (string.IsNullOrEmpty(obj.Value))
								{
									throw PythonOps.IndexError("unknown group reference");
								}
								stringBuilder.Append(obj.Value);
							}
							i = num2;
						}
						else
						{
							stringBuilder.Append('\\');
							stringBuilder.Append(text[i]);
						}
						continue;
					}
					if (char.IsDigit(text[i]) && text[i] <= '7')
					{
						int num3 = 0;
						int num4 = 0;
						for (; i < text.Length && char.IsDigit(text[i]) && text[i] <= '7'; i++)
						{
							num4++;
							num3 += num3 * 8 + (text[i] - 48);
						}
						i--;
						if (num4 == 1 && num3 > 0 && num3 < m.Groups.Count)
						{
							stringBuilder.Append(m.Groups[num3].Value);
						}
						else
						{
							stringBuilder.Append((char)num3);
						}
					}
					else
					{
						stringBuilder.Append('\\');
						stringBuilder.Append(text[i]);
					}
				}
				else
				{
					stringBuilder.Append(text[i]);
				}
			}
			while (++i < text.Length);
			return stringBuilder.ToString();
		}
		return text;
	}

	private static object ValidatePattern(object pattern)
	{
		if (pattern is string)
		{
			return pattern as string;
		}
		if (pattern is ExtensibleString extensibleString)
		{
			return extensibleString.Value;
		}
		if (pattern is Bytes bytes)
		{
			return bytes.ToString();
		}
		if (pattern is RE_Pattern result)
		{
			return result;
		}
		throw PythonOps.TypeError("pattern must be a string or compiled pattern");
	}

	private static string ValidatePatternAsString(object pattern)
	{
		if (pattern is string)
		{
			return pattern as string;
		}
		if (pattern is ExtensibleString extensibleString)
		{
			return extensibleString.Value;
		}
		if (pattern is Bytes bytes)
		{
			return bytes.ToString();
		}
		if (pattern is RE_Pattern rE_Pattern)
		{
			return rE_Pattern._pre.UserPattern;
		}
		throw PythonOps.TypeError("pattern must be a string or compiled pattern");
	}

	private static string ValidateString(object str, string param)
	{
		if (str is string)
		{
			return str as string;
		}
		if (str is ExtensibleString extensibleString)
		{
			return extensibleString.Value;
		}
		if (str is PythonBuffer pythonBuffer)
		{
			return pythonBuffer.ToString();
		}
		if (str is Bytes bytes)
		{
			return bytes.ToString();
		}
		if (str is ByteArray bytes2)
		{
			return bytes2.MakeString();
		}
		if (str is MmapModule.mmap mmap)
		{
			return mmap.GetSearchString();
		}
		throw PythonOps.TypeError("expected string for parameter '{0}' but got '{1}'", param, PythonOps.GetPythonTypeName(str));
	}

	private static PythonType error(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("reerror");
	}
}
