using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

internal static class IListOfByteOps
{
	[PythonType("bytes_iterator")]
	private class PythonBytesEnumerator<T> : IEnumerable, IEnumerator<T>, IDisposable, IEnumerator
	{
		private readonly IList<byte> _bytes;

		private readonly Func<byte, T> _conversion;

		private int _index;

		public T Current
		{
			get
			{
				if (_index < 0)
				{
					throw PythonOps.SystemError("Enumeration has not started. Call MoveNext.");
				}
				if (_index >= _bytes.Count)
				{
					throw PythonOps.SystemError("Enumeration already finished.");
				}
				return _conversion(_bytes[_index]);
			}
		}

		object IEnumerator.Current => ((IEnumerator<T>)this).Current;

		public PythonBytesEnumerator(IList<byte> bytes, Func<byte, T> conversion)
		{
			_bytes = bytes;
			_conversion = conversion;
			_index = -1;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_index >= _bytes.Count)
			{
				return false;
			}
			_index++;
			return _index != _bytes.Count;
		}

		public void Reset()
		{
			_index = -1;
		}

		public IEnumerator GetEnumerator()
		{
			return this;
		}
	}

	internal static int Compare(this IList<byte> self, IList<byte> other)
	{
		for (int i = 0; i < self.Count && i < other.Count; i++)
		{
			if (self[i] != other[i])
			{
				if (self[i] > other[i])
				{
					return 1;
				}
				return -1;
			}
		}
		if (self.Count == other.Count)
		{
			return 0;
		}
		if (self.Count <= other.Count)
		{
			return -1;
		}
		return 1;
	}

	internal static int Compare(this IList<byte> self, string other)
	{
		for (int i = 0; i < self.Count && i < other.Length; i++)
		{
			if (self[i] != other[i])
			{
				if (self[i] > other[i])
				{
					return 1;
				}
				return -1;
			}
		}
		if (self.Count == other.Length)
		{
			return 0;
		}
		if (self.Count <= other.Length)
		{
			return -1;
		}
		return 1;
	}

	internal static bool EndsWith(this IList<byte> self, IList<byte> suffix)
	{
		if (self.Count < suffix.Count)
		{
			return false;
		}
		int num = self.Count - suffix.Count;
		for (int i = 0; i < suffix.Count; i++)
		{
			if (suffix[i] != self[i + num])
			{
				return false;
			}
		}
		return true;
	}

	internal static bool EndsWith(this IList<byte> bytes, IList<byte> suffix, int start)
	{
		int count = bytes.Count;
		if (start > count)
		{
			return false;
		}
		if (start < 0)
		{
			start += count;
			if (start < 0)
			{
				start = 0;
			}
		}
		return bytes.Substring(start).EndsWith(suffix);
	}

	internal static bool EndsWith(this IList<byte> bytes, IList<byte> suffix, int start, int end)
	{
		int count = bytes.Count;
		if (start > count)
		{
			return false;
		}
		if (start < 0)
		{
			start += count;
			if (start < 0)
			{
				start = 0;
			}
		}
		if (end >= count)
		{
			return bytes.Substring(start).EndsWith(suffix);
		}
		if (end < 0)
		{
			end += count;
			if (end < 0)
			{
				return false;
			}
		}
		if (end < start)
		{
			return false;
		}
		return bytes.Substring(start, end - start).EndsWith(suffix);
	}

	internal static bool EndsWith(this IList<byte> bytes, PythonTuple suffix)
	{
		foreach (object item in suffix)
		{
			if (bytes.EndsWith(ByteOps.CoerceBytes(item)))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool EndsWith(this IList<byte> bytes, PythonTuple suffix, int start)
	{
		int count = bytes.Count;
		if (start > count)
		{
			return false;
		}
		if (start < 0)
		{
			start += count;
			if (start < 0)
			{
				start = 0;
			}
		}
		foreach (object item in suffix)
		{
			if (bytes.Substring(start).EndsWith(ByteOps.CoerceBytes(item)))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool EndsWith(this IList<byte> bytes, PythonTuple suffix, int start, int end)
	{
		int count = bytes.Count;
		if (start > count)
		{
			return false;
		}
		if (start < 0)
		{
			start += count;
			if (start < 0)
			{
				start = 0;
			}
		}
		if (end >= count)
		{
			end = count;
		}
		else if (end < 0)
		{
			end += count;
			if (end < 0)
			{
				return false;
			}
		}
		if (end < start)
		{
			return false;
		}
		foreach (object item in suffix)
		{
			if (bytes.Substring(start, end - start).EndsWith(ByteOps.CoerceBytes(item)))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool StartsWith(this IList<byte> self, IList<byte> prefix)
	{
		if (self.Count < prefix.Count)
		{
			return false;
		}
		for (int i = 0; i < prefix.Count; i++)
		{
			if (prefix[i] != self[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static int IndexOfAny(this IList<byte> str, IList<byte> separators, int i)
	{
		while (i < str.Count)
		{
			for (int j = 0; j < separators.Count; j++)
			{
				if (str[i] == separators[j])
				{
					return i;
				}
			}
			i++;
		}
		return -1;
	}

	internal static int IndexOf(this IList<byte> bytes, IList<byte> sub, int start)
	{
		return bytes.IndexOf(sub, start, bytes.Count - start);
	}

	internal static int IndexOf(this IList<byte> self, IList<byte> ssub, int start, int length)
	{
		if (ssub == null)
		{
			throw PythonOps.TypeError("cannot do None in bytes or bytearray");
		}
		if (ssub.Count == 0)
		{
			return 0;
		}
		byte b = ssub[0];
		for (int i = start; i < start + length; i++)
		{
			if (self[i] != b)
			{
				continue;
			}
			bool flag = false;
			for (int j = 1; j < ssub.Count; j++)
			{
				if (j + i == start + length || ssub[j] != self[i + j])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return i;
			}
		}
		return -1;
	}

	internal static bool IsTitle(this IList<byte> bytes)
	{
		if (bytes.Count == 0)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool result = false;
		for (int i = 0; i < bytes.Count; i++)
		{
			if (bytes[i].IsUpper())
			{
				result = true;
				if (flag)
				{
					return false;
				}
				flag2 = true;
			}
			else if (bytes[i].IsLower())
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

	internal static bool IsUpper(this IList<byte> bytes)
	{
		bool flag = false;
		foreach (byte @byte in bytes)
		{
			flag = flag || @byte.IsUpper();
			if (@byte.IsLower())
			{
				return false;
			}
		}
		return flag;
	}

	internal static List<byte> Title(this IList<byte> self)
	{
		if (self.Count == 0)
		{
			return null;
		}
		List<byte> list = new List<byte>(self);
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		do
		{
			if (list[num].IsUpper() || list[num].IsLower())
			{
				if (!flag)
				{
					list[num] = list[num].ToUpper();
				}
				else
				{
					list[num] = list[num].ToLower();
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
		while (num < list.Count);
		return list;
	}

	internal static int LastIndexOf(this IList<byte> self, IList<byte> sub, int start, int length)
	{
		byte b = sub[sub.Count - 1];
		for (int num = start - 1; num >= start - length; num--)
		{
			if (self[num] == b)
			{
				bool flag = false;
				if (sub.Count != 1)
				{
					int num2 = sub.Count - 2;
					int num3 = 1;
					while (num2 >= 0)
					{
						if (sub[num2] != self[num - num3])
						{
							flag = true;
							break;
						}
						num2--;
						num3++;
					}
				}
				if (!flag)
				{
					return num - sub.Count + 1;
				}
			}
		}
		return -1;
	}

	internal static List<byte>[] Split(this IList<byte> str, IList<byte> separators, int maxComponents, StringSplitOptions options)
	{
		ContractUtils.RequiresNotNull(str, "str");
		bool flag = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;
		if (separators == null)
		{
			return str.SplitOnWhiteSpace(maxComponents);
		}
		List<List<byte>> list = new List<List<byte>>((maxComponents == int.MaxValue) ? 1 : (maxComponents + 1));
		int num = 0;
		int num2;
		while (maxComponents > 1 && num < str.Count && (num2 = str.IndexOfAny(separators, num)) != -1)
		{
			if (num2 > num || flag)
			{
				list.Add(str.Substring(num, num2 - num));
				maxComponents--;
			}
			num = num2 + separators.Count;
		}
		if (num < str.Count || flag)
		{
			list.Add(str.Substring(num));
		}
		return list.ToArray();
	}

	internal static List<byte>[] SplitOnWhiteSpace(this IList<byte> str, int maxComponents)
	{
		ContractUtils.RequiresNotNull(str, "str");
		List<List<byte>> list = new List<List<byte>>((maxComponents == int.MaxValue) ? 1 : (maxComponents + 1));
		int i = 0;
		int num;
		while (maxComponents > 1 && i < str.Count && (num = str.IndexOfWhiteSpace(i)) != -1)
		{
			if (num > i)
			{
				list.Add(str.Substring(i, num - i));
				maxComponents--;
			}
			i = num + 1;
		}
		if (i < str.Count)
		{
			for (; i < str.Count && str[i].IsWhiteSpace(); i++)
			{
			}
			if (i < str.Count)
			{
				list.Add(str.Substring(i));
			}
		}
		return list.ToArray();
	}

	internal static bool StartsWith(this IList<byte> bytes, IList<byte> prefix, int start, int end)
	{
		int count = bytes.Count;
		if (start > count)
		{
			return false;
		}
		if (start < 0)
		{
			start += count;
			if (start < 0)
			{
				start = 0;
			}
		}
		if (end >= count)
		{
			return bytes.Substring(start).StartsWith(prefix);
		}
		if (end < 0)
		{
			end += count;
			if (end < 0)
			{
				return false;
			}
		}
		if (end < start)
		{
			return false;
		}
		return bytes.Substring(start, end - start).StartsWith(prefix);
	}

	internal static List<byte> Replace(this IList<byte> bytes, IList<byte> old, IList<byte> new_, int maxsplit)
	{
		if (new_ == null)
		{
			throw PythonOps.TypeError("expected bytes or bytearray, got NoneType");
		}
		if (maxsplit == -1)
		{
			maxsplit = old.Count + 1;
		}
		if (old.Count == 0)
		{
			return bytes.ReplaceEmpty(new_, maxsplit);
		}
		List<byte> list = new List<byte>(bytes.Count);
		int num = 0;
		int num2;
		while (maxsplit > 0 && (num2 = bytes.IndexOf(old, num)) != -1)
		{
			list.AddRange(bytes.Substring(num, num2 - num));
			list.AddRange(new_);
			num = num2 + old.Count;
			maxsplit--;
		}
		list.AddRange(bytes.Substring(num));
		return list;
	}

	private static List<byte> ReplaceEmpty(this IList<byte> self, IList<byte> new_, int maxsplit)
	{
		int num = ((maxsplit > self.Count) ? self.Count : maxsplit);
		List<byte> list = new List<byte>(self.Count * (new_.Count + 1));
		for (int i = 0; i < num; i++)
		{
			list.AddRange(new_);
			list.Add(self[i]);
		}
		for (int j = num; j < self.Count; j++)
		{
			list.Add(self[j]);
		}
		if (maxsplit > num)
		{
			list.AddRange(new_);
		}
		return list;
	}

	internal static int ReverseFind(this IList<byte> bytes, IList<byte> sub, int? start, int? end)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (start > bytes.Count)
		{
			return -1;
		}
		int num = FixStart(bytes, start);
		int num2 = FixEnd(bytes, end);
		if (num > num2)
		{
			return -1;
		}
		if (sub.Count == 0)
		{
			return num2;
		}
		int? num3 = end;
		if (num3.GetValueOrDefault() == 0 && num3.HasValue)
		{
			return -1;
		}
		return bytes.LastIndexOf(sub, num2, num2 - num);
	}

	internal static List RightSplit(this IList<byte> bytes, IList<byte> sep, int maxsplit, Func<IList<byte>, IList<byte>> ctor)
	{
		IList<byte> arg = bytes.ReverseBytes();
		if (sep != null)
		{
			sep = sep.ReverseBytes();
		}
		List list = null;
		List list2 = null;
		list = ctor(arg).Split(sep, maxsplit, (List<byte> x) => ctor(x));
		list.reverse();
		int num = list.__len__();
		if (num != 0)
		{
			list2 = new List(num);
			foreach (IList<byte> item in list)
			{
				list2.AddNoLock(ctor(item.ReverseBytes()));
			}
		}
		else
		{
			list2 = list;
		}
		return list2;
	}

	internal static int IndexOfWhiteSpace(this IList<byte> str, int start)
	{
		while (start < str.Count && !str[start].IsWhiteSpace())
		{
			start++;
		}
		if (start != str.Count)
		{
			return start;
		}
		return -1;
	}

	internal static byte[] ReverseBytes(this IList<byte> s)
	{
		byte[] array = new byte[s.Count];
		int num = s.Count - 1;
		int num2 = 0;
		while (num >= 0)
		{
			array[num2] = s[num];
			num--;
			num2++;
		}
		return array;
	}

	internal static List<byte> Substring(this IList<byte> bytes, int start)
	{
		return bytes.Substring(start, bytes.Count - start);
	}

	internal static List<byte> Substring(this IList<byte> bytes, int start, int len)
	{
		List<byte> list = new List<byte>();
		for (int i = start; i < start + len; i++)
		{
			list.Add(bytes[i]);
		}
		return list;
	}

	internal static List<byte> Multiply(this IList<byte> self, int count)
	{
		if (count <= 0)
		{
			return new List<byte>();
		}
		List<byte> list = new List<byte>(checked(self.Count * count));
		for (int i = 0; i < count; i++)
		{
			list.AddRange(self);
		}
		return list;
	}

	internal static List<byte> Capitalize(this IList<byte> bytes)
	{
		List<byte> list = new List<byte>(bytes);
		if (list.Count > 0)
		{
			list[0] = list[0].ToUpper();
			for (int i = 1; i < list.Count; i++)
			{
				list[i] = list[i].ToLower();
			}
		}
		return list;
	}

	internal static List<byte> TryCenter(this IList<byte> bytes, int width, int fillchar)
	{
		int num = width - bytes.Count;
		if (num <= 0)
		{
			return null;
		}
		byte item = fillchar.ToByteChecked();
		List<byte> list = new List<byte>();
		for (int i = 0; i < num / 2; i++)
		{
			list.Add(item);
		}
		list.AddRange(bytes);
		for (int j = 0; j < (num + 1) / 2; j++)
		{
			list.Add(item);
		}
		return list;
	}

	internal static int CountOf(this IList<byte> bytes, IList<byte> ssub, int start, int end)
	{
		if (ssub == null)
		{
			throw PythonOps.TypeError("expected bytes or byte array, got NoneType");
		}
		if (start > bytes.Count)
		{
			return 0;
		}
		start = PythonOps.FixSliceIndex(start, bytes.Count);
		end = PythonOps.FixSliceIndex(end, bytes.Count);
		if (ssub.Count == 0)
		{
			return Math.Max(end - start + 1, 0);
		}
		int num = 0;
		while (end > start)
		{
			int num2 = bytes.IndexOf(ssub, start, end - start);
			if (num2 == -1)
			{
				break;
			}
			num++;
			start = num2 + ssub.Count;
		}
		return num;
	}

	internal static List<byte> ExpandTabs(this IList<byte> bytes, int tabsize)
	{
		List<byte> list = new List<byte>(bytes.Count * 2);
		int num = 0;
		for (int i = 0; i < bytes.Count; i++)
		{
			byte b = bytes[i];
			switch (b)
			{
			case 10:
			case 13:
				num = 0;
				list.Add(b);
				break;
			case 9:
				if (tabsize > 0)
				{
					int num2 = tabsize - num % tabsize;
					int capacity = list.Capacity;
					list.Capacity = checked(capacity + num2);
					for (int j = 0; j < num2; j++)
					{
						list.Add(32);
					}
					num = 0;
				}
				break;
			default:
				num++;
				list.Add(b);
				break;
			}
		}
		return list;
	}

	internal static int IndexOfByte(this IList<byte> bytes, int item, int start, int stop)
	{
		start = PythonOps.FixSliceIndex(start, bytes.Count);
		stop = PythonOps.FixSliceIndex(stop, bytes.Count);
		for (int i = start; i < Math.Min(stop, bytes.Count); i++)
		{
			if (bytes[i] == item)
			{
				return i;
			}
		}
		throw PythonOps.ValueError("bytearray.index(item): item not in bytearray");
	}

	internal static string BytesRepr(this IList<byte> bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("b'");
		for (int i = 0; i < bytes.Count; i++)
		{
			byte b = bytes[i];
			switch (b)
			{
			case 92:
				stringBuilder.Append("\\\\");
				continue;
			case 9:
				stringBuilder.Append("\\t");
				continue;
			case 10:
				stringBuilder.Append("\\n");
				continue;
			case 13:
				stringBuilder.Append("\\r");
				continue;
			case 39:
				stringBuilder.Append('\\');
				stringBuilder.Append('\'');
				continue;
			}
			if (b < 32 || (b >= 127 && b <= byte.MaxValue))
			{
				stringBuilder.AppendFormat("\\x{0:x2}", b);
			}
			else
			{
				stringBuilder.Append((char)b);
			}
		}
		stringBuilder.Append("'");
		return stringBuilder.ToString();
	}

	internal static List<byte> ZeroFill(this IList<byte> bytes, int width, int spaces)
	{
		List<byte> list = new List<byte>(width);
		if (bytes.Count > 0 && bytes[0].IsSign())
		{
			list.Add(bytes[0]);
			for (int i = 0; i < spaces; i++)
			{
				list.Add(48);
			}
			for (int j = 1; j < bytes.Count; j++)
			{
				list.Add(bytes[j]);
			}
		}
		else
		{
			for (int k = 0; k < spaces; k++)
			{
				list.Add(48);
			}
			list.AddRange(bytes);
		}
		return list;
	}

	internal static List<byte> ToLower(this IList<byte> bytes)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < bytes.Count; i++)
		{
			list.Add(bytes[i].ToLower());
		}
		return list;
	}

	internal static List<byte> ToUpper(this IList<byte> bytes)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < bytes.Count; i++)
		{
			list.Add(bytes[i].ToUpper());
		}
		return list;
	}

	internal static List<byte> Translate(this IList<byte> bytes, IList<byte> table, IList<byte> deletechars)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < bytes.Count; i++)
		{
			if (deletechars == null || !deletechars.Contains(bytes[i]))
			{
				if (table == null)
				{
					list.Add(bytes[i]);
				}
				else
				{
					list.Add(table[bytes[i]]);
				}
			}
		}
		return list;
	}

	internal static List<byte> RightStrip(this IList<byte> bytes)
	{
		int num = bytes.Count - 1;
		while (num >= 0 && bytes[num].IsWhiteSpace())
		{
			num--;
		}
		if (num == bytes.Count - 1)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i <= num; i++)
		{
			list.Add(bytes[i]);
		}
		return list;
	}

	internal static List<byte> RightStrip(this IList<byte> bytes, IList<byte> chars)
	{
		int num = bytes.Count - 1;
		while (num >= 0 && chars.Contains(bytes[num]))
		{
			num--;
		}
		if (num == bytes.Count - 1)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i <= num; i++)
		{
			list.Add(bytes[i]);
		}
		return list;
	}

	internal static List SplitLines(this IList<byte> bytes, bool keepends, Func<List<byte>, object> ctor)
	{
		List list = new List();
		int i = 0;
		int num = 0;
		for (; i < bytes.Count; i++)
		{
			if (bytes[i] != 10 && bytes[i] != 13)
			{
				continue;
			}
			if (i < bytes.Count - 1 && bytes[i] == 13 && bytes[i + 1] == 10)
			{
				if (keepends)
				{
					list.AddNoLock(ctor(bytes.Substring(num, i - num + 2)));
				}
				else
				{
					list.AddNoLock(ctor(bytes.Substring(num, i - num)));
				}
				num = i + 2;
				i++;
			}
			else
			{
				if (keepends)
				{
					list.AddNoLock(ctor(bytes.Substring(num, i - num + 1)));
				}
				else
				{
					list.AddNoLock(ctor(bytes.Substring(num, i - num)));
				}
				num = i + 1;
			}
		}
		if (i - num != 0)
		{
			list.AddNoLock(ctor(bytes.Substring(num, i - num)));
		}
		return list;
	}

	internal static List<byte> LeftStrip(this IList<byte> bytes)
	{
		int i;
		for (i = 0; i < bytes.Count && bytes[i].IsWhiteSpace(); i++)
		{
		}
		if (i == 0)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		for (; i < bytes.Count; i++)
		{
			list.Add(bytes[i]);
		}
		return list;
	}

	internal static List<byte> LeftStrip(this IList<byte> bytes, IList<byte> chars)
	{
		int i;
		for (i = 0; i < bytes.Count && chars.Contains(bytes[i]); i++)
		{
		}
		if (i == 0)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		for (; i < bytes.Count; i++)
		{
			list.Add(bytes[i]);
		}
		return list;
	}

	internal static List Split(this IList<byte> bytes, IList<byte> sep, int maxsplit, Func<List<byte>, object> ctor)
	{
		if (sep == null)
		{
			if (maxsplit == 0)
			{
				List list = PythonOps.MakeEmptyList(1);
				list.AddNoLock(ctor(bytes.LeftStrip() ?? (bytes as List<byte>) ?? new List<byte>(bytes)));
				return list;
			}
			return SplitInternal(bytes, null, maxsplit, ctor);
		}
		if (sep.Count == 0)
		{
			throw PythonOps.ValueError("empty separator");
		}
		if (sep.Count == 1)
		{
			return SplitInternal(bytes, new byte[1] { sep[0] }, maxsplit, ctor);
		}
		return bytes.SplitInternal(sep, maxsplit, ctor);
	}

	internal static List SplitInternal(IList<byte> bytes, byte[] seps, int maxsplit, Func<List<byte>, object> ctor)
	{
		if (bytes.Count == 0)
		{
			return SplitEmptyString(seps != null, ctor);
		}
		List<byte>[] array = null;
		array = bytes.Split(seps, (maxsplit < 0) ? int.MaxValue : (maxsplit + 1), GetStringSplitOptions(seps));
		List list = PythonOps.MakeEmptyList(array.Length);
		List<byte>[] array2 = array;
		foreach (List<byte> arg in array2)
		{
			list.AddNoLock(ctor(arg));
		}
		return list;
	}

	private static StringSplitOptions GetStringSplitOptions(IList<byte> seps)
	{
		if (seps != null)
		{
			return StringSplitOptions.None;
		}
		return StringSplitOptions.RemoveEmptyEntries;
	}

	internal static List SplitInternal(this IList<byte> bytes, IList<byte> separator, int maxsplit, Func<List<byte>, object> ctor)
	{
		if (bytes.Count == 0)
		{
			return SplitEmptyString(separator != null, ctor);
		}
		List<byte>[] array = bytes.Split(separator, (maxsplit < 0) ? int.MaxValue : (maxsplit + 1), GetStringSplitOptions(separator));
		List list = PythonOps.MakeEmptyList(array.Length);
		List<byte>[] array2 = array;
		foreach (List<byte> arg in array2)
		{
			list.AddNoLock(ctor(arg));
		}
		return list;
	}

	private static List SplitEmptyString(bool separators, Func<List<byte>, object> ctor)
	{
		List list = PythonOps.MakeEmptyList(1);
		if (separators)
		{
			list.AddNoLock(ctor(new List<byte>(0)));
		}
		return list;
	}

	internal static List<byte> Strip(this IList<byte> bytes)
	{
		int i;
		for (i = 0; i < bytes.Count && bytes[i].IsWhiteSpace(); i++)
		{
		}
		int num = bytes.Count - 1;
		while (num >= 0 && bytes[num].IsWhiteSpace())
		{
			num--;
		}
		if (i == 0 && num == bytes.Count - 1)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		for (int j = i; j <= num; j++)
		{
			list.Add(bytes[j]);
		}
		return list;
	}

	internal static List<byte> Strip(this IList<byte> bytes, IList<byte> chars)
	{
		int i;
		for (i = 0; i < bytes.Count && chars.Contains(bytes[i]); i++)
		{
		}
		int num = bytes.Count - 1;
		while (num >= 0 && chars.Contains(bytes[num]))
		{
			num--;
		}
		if (i == 0 && num == bytes.Count - 1)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		for (int j = i; j <= num; j++)
		{
			list.Add(bytes[j]);
		}
		return list;
	}

	internal static List<byte> Slice(this IList<byte> bytes, Slice slice)
	{
		if (slice == null)
		{
			throw PythonOps.TypeError("indices must be slices or integers");
		}
		slice.indices(bytes.Count, out var ostart, out var ostop, out var ostep);
		if (ostep == 1)
		{
			if (ostop <= ostart)
			{
				return null;
			}
			return bytes.Substring(ostart, ostop - ostart);
		}
		List<byte> list;
		if (ostep > 0)
		{
			if (ostart > ostop)
			{
				return null;
			}
			int capacity = (ostop - ostart + ostep - 1) / ostep;
			list = new List<byte>(capacity);
			for (int i = ostart; i < ostop; i += ostep)
			{
				list.Add(bytes[i]);
			}
		}
		else
		{
			if (ostart < ostop)
			{
				return null;
			}
			int capacity2 = (ostop - ostart + ostep + 1) / ostep;
			list = new List<byte>(capacity2);
			for (int j = ostart; j > ostop; j += ostep)
			{
				list.Add(bytes[j]);
			}
		}
		return list;
	}

	internal static List<byte> SwapCase(this IList<byte> bytes)
	{
		List<byte> list = new List<byte>(bytes);
		for (int i = 0; i < bytes.Count; i++)
		{
			byte p = list[i];
			if (p.IsUpper())
			{
				list[i] = p.ToLower();
			}
			else if (p.IsLower())
			{
				list[i] = p.ToUpper();
			}
		}
		return list;
	}

	internal static bool StartsWith(this IList<byte> bytes, PythonTuple prefix, int start, int end)
	{
		int count = bytes.Count;
		if (start > count)
		{
			return false;
		}
		if (start < 0)
		{
			start += count;
			if (start < 0)
			{
				start = 0;
			}
		}
		if (end >= count)
		{
			end = count;
		}
		else if (end < 0)
		{
			end += count;
			if (end < 0)
			{
				return false;
			}
		}
		if (end < start)
		{
			return false;
		}
		foreach (object item in prefix)
		{
			if (bytes.Substring(start, end - start).StartsWith(ByteOps.CoerceBytes(item)))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool StartsWith(this IList<byte> bytes, PythonTuple prefix, int start)
	{
		int count = bytes.Count;
		if (start > count)
		{
			return false;
		}
		if (start < 0)
		{
			start += count;
			if (start < 0)
			{
				start = 0;
			}
		}
		foreach (object item in prefix)
		{
			if (bytes.Substring(start).StartsWith(ByteOps.CoerceBytes(item)))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool StartsWith(this IList<byte> bytes, PythonTuple prefix)
	{
		foreach (object item in prefix)
		{
			if (bytes.StartsWith(ByteOps.CoerceBytes(item)))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsWhiteSpace(this IList<byte> bytes)
	{
		if (bytes.Count == 0)
		{
			return false;
		}
		foreach (byte @byte in bytes)
		{
			if (!@byte.IsWhiteSpace())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsLower(this IList<byte> bytes)
	{
		bool flag = false;
		foreach (byte @byte in bytes)
		{
			flag = flag || @byte.IsLower();
			if (@byte.IsUpper())
			{
				return false;
			}
		}
		return flag;
	}

	internal static bool IsDigit(this IList<byte> bytes)
	{
		if (bytes.Count == 0)
		{
			return false;
		}
		foreach (byte @byte in bytes)
		{
			if (!@byte.IsDigit())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsLetter(this IList<byte> bytes)
	{
		if (bytes.Count == 0)
		{
			return false;
		}
		foreach (byte @byte in bytes)
		{
			if (!@byte.IsLetter())
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsAlphaNumeric(this IList<byte> bytes)
	{
		if (bytes.Count == 0)
		{
			return false;
		}
		foreach (byte @byte in bytes)
		{
			if (!@byte.IsDigit() && !@byte.IsLetter())
			{
				return false;
			}
		}
		return true;
	}

	internal static int Find(this IList<byte> bytes, IList<byte> sub)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected byte or byte array, got NoneType");
		}
		return bytes.IndexOf(sub, 0);
	}

	internal static int Find(this IList<byte> bytes, IList<byte> sub, int? start)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected byte or byte array, got NoneType");
		}
		if (start > bytes.Count)
		{
			return -1;
		}
		int start2 = (start.HasValue ? PythonOps.FixSliceIndex(start.Value, bytes.Count) : 0);
		return bytes.IndexOf(sub, start2);
	}

	internal static int Find(this IList<byte> bytes, IList<byte> sub, int? start, int? end)
	{
		if (sub == null)
		{
			throw PythonOps.TypeError("expected byte or byte array, got NoneType");
		}
		if (start > bytes.Count)
		{
			return -1;
		}
		int num = FixStart(bytes, start);
		int num2 = FixEnd(bytes, end);
		if (num2 < num)
		{
			return -1;
		}
		return bytes.IndexOf(sub, num, num2 - num);
	}

	private static int FixEnd(IList<byte> bytes, int? end)
	{
		if (end.HasValue)
		{
			return PythonOps.FixSliceIndex(end.Value, bytes.Count);
		}
		return bytes.Count;
	}

	private static int FixStart(IList<byte> bytes, int? start)
	{
		if (start.HasValue)
		{
			return PythonOps.FixSliceIndex(start.Value, bytes.Count);
		}
		return 0;
	}

	internal static byte ToByte(this string self, string name, int pos)
	{
		if (self.Length != 1 || self[0] >= 'Ā')
		{
			throw PythonOps.TypeError(name + "() argument " + pos + " must be char < 256, not string");
		}
		return (byte)self[0];
	}

	internal static byte ToByte(this IList<byte> self, string name, int pos)
	{
		if (self == null)
		{
			throw PythonOps.TypeError(name + "() argument " + pos + " must be char < 256, not None");
		}
		if (self.Count != 1)
		{
			throw PythonOps.TypeError(name + "() argument " + pos + " must be char < 256, not bytearray or bytes");
		}
		return self[0];
	}

	internal static List<byte> FromHex(string @string)
	{
		if (@string == null)
		{
			throw PythonOps.TypeError("expected str, got NoneType");
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i < @string.Length; i++)
		{
			char c = @string[i];
			int num = 0;
			if (char.IsDigit(c))
			{
				num = (c - 48) * 16;
			}
			else if (c >= 'A' && c <= 'F')
			{
				num = (c - 65 + 10) * 16;
			}
			else
			{
				if (c < 'a' || c > 'f')
				{
					if (c != ' ')
					{
						throw PythonOps.ValueError("non-hexadecimal number found in fromhex() arg at position {0}", i);
					}
					continue;
				}
				num = (c - 97 + 10) * 16;
			}
			i++;
			if (i == @string.Length)
			{
				throw PythonOps.ValueError("non-hexadecimal number found in fromhex() arg at position {0}", i - 1);
			}
			c = @string[i];
			if (char.IsDigit(c))
			{
				num += c - 48;
			}
			else if (c >= 'A' && c <= 'F')
			{
				num += c - 65 + 10;
			}
			else
			{
				if (c < 'a' || c > 'f')
				{
					throw PythonOps.ValueError("non-hexadecimal number found in fromhex() arg at position {0}", i);
				}
				num += c - 97 + 10;
			}
			list.Add((byte)num);
		}
		return list;
	}

	internal static IEnumerable BytesEnumerable(IList<byte> bytes)
	{
		return new PythonBytesEnumerator<Bytes>(bytes, (byte b) => Bytes.Make(new byte[1] { b }));
	}

	internal static IEnumerable BytesIntEnumerable(IList<byte> bytes)
	{
		return new PythonBytesEnumerator<int>(bytes, (byte b) => b);
	}

	internal static IEnumerator<Bytes> BytesEnumerator(IList<byte> bytes)
	{
		return new PythonBytesEnumerator<Bytes>(bytes, (byte b) => Bytes.Make(new byte[1] { b }));
	}

	internal static IEnumerator<int> BytesIntEnumerator(IList<byte> bytes)
	{
		return new PythonBytesEnumerator<int>(bytes, (byte b) => b);
	}
}
