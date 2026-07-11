using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[PythonType("bytearray")]
public class ByteArray : IList<byte>, ICollection<byte>, IEnumerable<byte>, IEnumerable, ICodeFormattable, IBufferProtocol
{
	public const object __hash__ = null;

	internal List<byte> _bytes;

	public object this[int index]
	{
		get
		{
			lock (this)
			{
				return ScriptingRuntimeHelpers.Int32ToObject(_bytes[PythonOps.FixIndex(index, _bytes.Count)]);
			}
		}
		set
		{
			lock (this)
			{
				_bytes[PythonOps.FixIndex(index, _bytes.Count)] = GetByte(value);
			}
		}
	}

	public object this[BigInteger index]
	{
		get
		{
			if (index.AsInt32(out var ret))
			{
				return this[ret];
			}
			throw PythonOps.IndexError("cannot fit long in index");
		}
		set
		{
			if (index.AsInt32(out var ret))
			{
				this[ret] = value;
				return;
			}
			throw PythonOps.IndexError("cannot fit long in index");
		}
	}

	public object this[Slice slice]
	{
		get
		{
			lock (this)
			{
				List<byte> list = _bytes.Slice(slice);
				if (list == null)
				{
					return new ByteArray();
				}
				return new ByteArray(list);
			}
		}
		set
		{
			if (slice == null)
			{
				throw PythonOps.TypeError("bytearray indices must be integer or slice, not None");
			}
			IList<byte> list = value as IList<byte>;
			if (list == null)
			{
				int? num = null;
				int ret;
				if (value is int)
				{
					num = (int)value;
				}
				else if (value is Extensible<int>)
				{
					num = ((Extensible<int>)value).Value;
				}
				else if (value is BigInteger && ((BigInteger)value).AsInt32(out ret))
				{
					num = ret;
				}
				if (num.HasValue)
				{
					List<byte> list2 = new List<byte>();
					list2.Capacity = num.Value;
					for (int i = 0; i < num; i++)
					{
						list2.Add(0);
					}
					list = list2;
				}
				else
				{
					IEnumerator enumerator = PythonOps.GetEnumerator(value);
					list = new List<byte>();
					while (enumerator.MoveNext())
					{
						list.Add(GetByte(enumerator.Current));
					}
				}
			}
			lock (this)
			{
				if (slice.step != null)
				{
					if (this == list)
					{
						value = CopyThis();
					}
					else if (list.Count == 0)
					{
						DeleteItem(slice);
						return;
					}
					IList<byte> bytes = GetBytes(value);
					slice.indices(_bytes.Count, out var ostart, out var ostop, out var ostep);
					int num2 = ((ostep > 0) ? (ostop - ostart + ostep - 1) : (ostop - ostart + ostep + 1)) / ostep;
					if (list.Count < num2)
					{
						throw PythonOps.ValueError("too few items in the enumerator. need {0} have {1}", num2, bytes.Count);
					}
					int num3 = 0;
					int num4 = ostart;
					while (num3 < bytes.Count)
					{
						if (num3 >= num2)
						{
							if (num4 == _bytes.Count)
							{
								_bytes.Add(bytes[num3]);
							}
							else
							{
								_bytes.Insert(num4, bytes[num3]);
							}
						}
						else
						{
							_bytes[num4] = bytes[num3];
						}
						num3++;
						num4 += ostep;
					}
				}
				else
				{
					slice.indices(_bytes.Count, out var ostart2, out var ostop2, out var _);
					SliceNoStep(ostart2, ostop2, list);
				}
			}
		}
	}

	byte IList<byte>.this[int index]
	{
		get
		{
			return _bytes[index];
		}
		set
		{
			_bytes[index] = value;
		}
	}

	public int Count
	{
		[PythonHidden]
		get
		{
			lock (this)
			{
				return _bytes.Count;
			}
		}
	}

	public bool IsReadOnly
	{
		[PythonHidden]
		get
		{
			return false;
		}
	}

	int IBufferProtocol.ItemCount => _bytes.Count;

	string IBufferProtocol.Format => "B";

	BigInteger IBufferProtocol.ItemSize => 1;

	BigInteger IBufferProtocol.NumberDimensions => 1;

	bool IBufferProtocol.ReadOnly => false;

	PythonTuple IBufferProtocol.Strides => PythonTuple.MakeTuple(1);

	object IBufferProtocol.SubOffsets => null;

	public ByteArray()
	{
		_bytes = new List<byte>(0);
	}

	internal ByteArray(List<byte> bytes)
	{
		_bytes = bytes;
	}

	internal ByteArray(byte[] bytes)
	{
		_bytes = new List<byte>(bytes);
	}

	public void __init__()
	{
		_bytes = new List<byte>();
	}

	public void __init__(int source)
	{
		_bytes = new List<byte>(source);
		for (int i = 0; i < source; i++)
		{
			_bytes.Add(0);
		}
	}

	public void __init__(BigInteger source)
	{
		__init__((int)source);
	}

	public void __init__([NotNull] IList<byte> source)
	{
		_bytes = new List<byte>(source);
	}

	public void __init__(object source)
	{
		__init__(GetBytes(source));
	}

	public void __init__(CodeContext context, string source, string encoding, [DefaultParameterValue("strict")] string errors)
	{
		_bytes = new List<byte>(StringOps.encode(context, source, encoding, errors).MakeByteArray());
	}

	public void append(int item)
	{
		lock (this)
		{
			_bytes.Add(item.ToByteChecked());
		}
	}

	public void append(object item)
	{
		lock (this)
		{
			_bytes.Add(GetByte(item));
		}
	}

	public void extend([NotNull] IEnumerable<byte> seq)
	{
		using (new OrderedLocker(this, seq))
		{
			_bytes.AddRange(seq);
		}
	}

	public void extend(object seq)
	{
		extend(GetBytes(seq));
	}

	public void insert(int index, int value)
	{
		lock (this)
		{
			if (index >= Count)
			{
				append(value);
				return;
			}
			index = PythonOps.FixSliceIndex(index, Count);
			_bytes.Insert(index, value.ToByteChecked());
		}
	}

	public void insert(int index, object value)
	{
		insert(index, Converter.ConvertToIndex(value));
	}

	public int pop()
	{
		lock (this)
		{
			if (Count == 0)
			{
				throw PythonOps.OverflowError("pop off of empty bytearray");
			}
			int result = _bytes[_bytes.Count - 1];
			_bytes.RemoveAt(_bytes.Count - 1);
			return result;
		}
	}

	public int pop(int index)
	{
		lock (this)
		{
			if (Count == 0)
			{
				throw PythonOps.OverflowError("pop off of empty bytearray");
			}
			index = PythonOps.FixIndex(index, Count);
			int result = _bytes[index];
			_bytes.RemoveAt(index);
			return result;
		}
	}

	public void remove(int value)
	{
		lock (this)
		{
			_bytes.RemoveAt(_bytes.IndexOfByte(value.ToByteChecked(), 0, _bytes.Count));
		}
	}

	public void remove(object value)
	{
		lock (this)
		{
			if (value is ByteArray)
			{
				throw PythonOps.TypeError("an integer or string of size 1 is required");
			}
			_bytes.RemoveAt(_bytes.IndexOfByte(GetByte(value), 0, _bytes.Count));
		}
	}

	public void reverse()
	{
		lock (this)
		{
			List<byte> list = new List<byte>();
			for (int num = _bytes.Count - 1; num >= 0; num--)
			{
				list.Add(_bytes[num]);
			}
			_bytes = list;
		}
	}

	[SpecialName]
	public ByteArray InPlaceAdd(ByteArray other)
	{
		using (new OrderedLocker(this, other))
		{
			_bytes.AddRange(other._bytes);
			return this;
		}
	}

	[SpecialName]
	public ByteArray InPlaceAdd(Bytes other)
	{
		lock (this)
		{
			_bytes.AddRange(other);
			return this;
		}
	}

	[SpecialName]
	public ByteArray InPlaceMultiply(int len)
	{
		lock (this)
		{
			_bytes = (this * len)._bytes;
			return this;
		}
	}

	public ByteArray capitalize()
	{
		lock (this)
		{
			return new ByteArray(_bytes.Capitalize());
		}
	}

	public ByteArray center(int width)
	{
		return center(width, " ");
	}

	public ByteArray center(int width, [NotNull] string fillchar)
	{
		lock (this)
		{
			List<byte> list = _bytes.TryCenter(width, fillchar.ToByte("center", 2));
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public ByteArray center(int width, [BytesConversion] IList<byte> fillchar)
	{
		lock (this)
		{
			List<byte> list = _bytes.TryCenter(width, fillchar.ToByte("center", 2));
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public int count([BytesConversion] IList<byte> sub)
	{
		return count(sub, 0, _bytes.Count);
	}

	public int count([BytesConversion] IList<byte> sub, int start)
	{
		return count(sub, start, _bytes.Count);
	}

	public int count([BytesConversion] IList<byte> ssub, int start, int end)
	{
		lock (this)
		{
			return _bytes.CountOf(ssub, start, end);
		}
	}

	public string decode(CodeContext context, [Optional] string encoding, [DefaultParameterValue("strict")] string errors)
	{
		return StringOps.decode(context, _bytes.MakeString(), encoding, errors);
	}

	public bool endswith([BytesConversion] IList<byte> suffix)
	{
		lock (this)
		{
			return _bytes.EndsWith(suffix);
		}
	}

	public bool endswith([BytesConversion] IList<byte> suffix, int start)
	{
		lock (this)
		{
			return _bytes.EndsWith(suffix, start);
		}
	}

	public bool endswith([BytesConversion] IList<byte> suffix, int start, int end)
	{
		lock (this)
		{
			return _bytes.EndsWith(suffix, start, end);
		}
	}

	public bool endswith(PythonTuple suffix)
	{
		lock (this)
		{
			return _bytes.EndsWith(suffix);
		}
	}

	public bool endswith(PythonTuple suffix, int start)
	{
		lock (this)
		{
			return _bytes.EndsWith(suffix, start);
		}
	}

	public bool endswith(PythonTuple suffix, int start, int end)
	{
		lock (this)
		{
			return _bytes.EndsWith(suffix, start, end);
		}
	}

	public ByteArray expandtabs()
	{
		return expandtabs(8);
	}

	public ByteArray expandtabs(int tabsize)
	{
		lock (this)
		{
			return new ByteArray(_bytes.ExpandTabs(tabsize));
		}
	}

	public int find([BytesConversion] IList<byte> sub)
	{
		lock (this)
		{
			return _bytes.Find(sub);
		}
	}

	public int find([BytesConversion] IList<byte> sub, int start)
	{
		lock (this)
		{
			return _bytes.Find(sub, start);
		}
	}

	public int find([BytesConversion] IList<byte> sub, int start, int end)
	{
		lock (this)
		{
			return _bytes.Find(sub, start, end);
		}
	}

	public static ByteArray fromhex(string @string)
	{
		return new ByteArray(IListOfByteOps.FromHex(@string));
	}

	public int index([BytesConversion] IList<byte> item)
	{
		return index(item, 0, _bytes.Count);
	}

	public int index([BytesConversion] IList<byte> item, int start)
	{
		return index(item, start, _bytes.Count);
	}

	public int index([BytesConversion] IList<byte> item, int start, int stop)
	{
		lock (this)
		{
			int num = find(item, start, stop);
			if (num == -1)
			{
				throw PythonOps.ValueError("bytearray.index(item): item not in bytearray");
			}
			return num;
		}
	}

	public bool isalnum()
	{
		lock (this)
		{
			return _bytes.IsAlphaNumeric();
		}
	}

	public bool isalpha()
	{
		lock (this)
		{
			return _bytes.IsLetter();
		}
	}

	public bool isdigit()
	{
		lock (this)
		{
			return _bytes.IsDigit();
		}
	}

	public bool islower()
	{
		lock (this)
		{
			return _bytes.IsLower();
		}
	}

	public bool isspace()
	{
		lock (this)
		{
			return _bytes.IsWhiteSpace();
		}
	}

	public bool istitle()
	{
		lock (this)
		{
			return _bytes.IsTitle();
		}
	}

	public bool isupper()
	{
		lock (this)
		{
			return _bytes.IsUpper();
		}
	}

	public ByteArray join(object sequence)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(sequence);
		if (!enumerator.MoveNext())
		{
			return new ByteArray();
		}
		object current = enumerator.Current;
		if (!enumerator.MoveNext())
		{
			return JoinOne(current);
		}
		List<byte> list = new List<byte>();
		ByteOps.AppendJoin(current, 0, list);
		int num = 1;
		do
		{
			list.AddRange(this);
			ByteOps.AppendJoin(enumerator.Current, num, list);
			num++;
		}
		while (enumerator.MoveNext());
		return new ByteArray(list);
	}

	public ByteArray join([NotNull] List sequence)
	{
		if (sequence.__len__() == 0)
		{
			return new ByteArray();
		}
		lock (this)
		{
			if (sequence.__len__() == 1)
			{
				return JoinOne(sequence[0]);
			}
			List<byte> list = new List<byte>();
			ByteOps.AppendJoin(sequence._data[0], 0, list);
			for (int i = 1; i < sequence._size; i++)
			{
				list.AddRange(this);
				ByteOps.AppendJoin(sequence._data[i], i, list);
			}
			return new ByteArray(list);
		}
	}

	public ByteArray ljust(int width)
	{
		return ljust(width, 32);
	}

	public ByteArray ljust(int width, [NotNull] string fillchar)
	{
		return ljust(width, fillchar.ToByte("ljust", 2));
	}

	public ByteArray ljust(int width, IList<byte> fillchar)
	{
		return ljust(width, fillchar.ToByte("ljust", 2));
	}

	private ByteArray ljust(int width, byte fillchar)
	{
		lock (this)
		{
			int num = width - _bytes.Count;
			List<byte> list = new List<byte>(width);
			list.AddRange(_bytes);
			for (int i = 0; i < num; i++)
			{
				list.Add(fillchar);
			}
			return new ByteArray(list);
		}
	}

	public ByteArray lower()
	{
		lock (this)
		{
			return new ByteArray(_bytes.ToLower());
		}
	}

	public ByteArray lstrip()
	{
		lock (this)
		{
			List<byte> list = _bytes.LeftStrip();
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public ByteArray lstrip([BytesConversionNoString] IList<byte> bytes)
	{
		lock (this)
		{
			List<byte> list = _bytes.LeftStrip(bytes);
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public PythonTuple partition(IList<byte> sep)
	{
		if (sep == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (sep.Count == 0)
		{
			throw PythonOps.ValueError("empty separator");
		}
		object[] array = new object[3]
		{
			new ByteArray(),
			new ByteArray(),
			new ByteArray()
		};
		if (_bytes.Count != 0)
		{
			int num = find(sep);
			if (num == -1)
			{
				array[0] = this;
			}
			else
			{
				array[0] = new ByteArray(_bytes.Substring(0, num));
				array[1] = sep;
				array[2] = new ByteArray(_bytes.Substring(num + sep.Count, _bytes.Count - num - sep.Count));
			}
		}
		return new PythonTuple(array);
	}

	public PythonTuple partition([NotNull] List sep)
	{
		return partition(GetBytes(sep));
	}

	public ByteArray replace([BytesConversion] IList<byte> old, [BytesConversion] IList<byte> new_)
	{
		if (old == null)
		{
			throw PythonOps.TypeError("expected bytes or bytearray, got NoneType");
		}
		return replace(old, new_, _bytes.Count);
	}

	public ByteArray replace([BytesConversion] IList<byte> old, [BytesConversion] IList<byte> new_, int maxsplit)
	{
		if (old == null)
		{
			throw PythonOps.TypeError("expected bytes or bytearray, got NoneType");
		}
		if (maxsplit == 0)
		{
			return CopyThis();
		}
		return new ByteArray(_bytes.Replace(old, new_, maxsplit));
	}

	public int rfind([BytesConversion] IList<byte> sub)
	{
		return rfind(sub, 0, _bytes.Count);
	}

	public int rfind([BytesConversion] IList<byte> sub, int start)
	{
		return rfind(sub, start, _bytes.Count);
	}

	public int rfind([BytesConversion] IList<byte> sub, int start, int end)
	{
		lock (this)
		{
			return _bytes.ReverseFind(sub, start, end);
		}
	}

	public int rindex([BytesConversion] IList<byte> sub)
	{
		return rindex(sub, 0, _bytes.Count);
	}

	public int rindex([BytesConversion] IList<byte> sub, int start)
	{
		return rindex(sub, start, _bytes.Count);
	}

	public int rindex([BytesConversion] IList<byte> sub, int start, int end)
	{
		int num = rfind(sub, start, end);
		if (num == -1)
		{
			throw PythonOps.ValueError("substring {0} not found in {1}", sub, this);
		}
		return num;
	}

	public ByteArray rjust(int width)
	{
		return rjust(width, 32);
	}

	public ByteArray rjust(int width, [NotNull] string fillchar)
	{
		return rjust(width, fillchar.ToByte("rjust", 2));
	}

	public ByteArray rjust(int width, [BytesConversion] IList<byte> fillchar)
	{
		return rjust(width, fillchar.ToByte("rjust", 2));
	}

	private ByteArray rjust(int width, int fillchar)
	{
		byte item = fillchar.ToByteChecked();
		lock (this)
		{
			int num = width - _bytes.Count;
			if (num <= 0)
			{
				return CopyThis();
			}
			List<byte> list = new List<byte>(width);
			for (int i = 0; i < num; i++)
			{
				list.Add(item);
			}
			list.AddRange(_bytes);
			return new ByteArray(list);
		}
	}

	public PythonTuple rpartition(IList<byte> sep)
	{
		if (sep == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (sep.Count == 0)
		{
			throw PythonOps.ValueError("empty separator");
		}
		lock (this)
		{
			object[] array = new object[3]
			{
				new ByteArray(),
				new ByteArray(),
				new ByteArray()
			};
			if (_bytes.Count != 0)
			{
				int num = rfind(sep);
				if (num == -1)
				{
					array[2] = this;
				}
				else
				{
					array[0] = new ByteArray(_bytes.Substring(0, num));
					array[1] = new ByteArray(new List<byte>(sep));
					array[2] = new ByteArray(_bytes.Substring(num + sep.Count, Count - num - sep.Count));
				}
			}
			return new PythonTuple(array);
		}
	}

	public PythonTuple rpartition([NotNull] List sep)
	{
		return rpartition(GetBytes(sep));
	}

	public List rsplit()
	{
		lock (this)
		{
			return ((IList<byte>)_bytes).SplitInternal((IList<byte>)null, -1, (Func<List<byte>, object>)((List<byte> x) => new ByteArray(x)));
		}
	}

	public List rsplit([BytesConversionNoString] IList<byte> sep)
	{
		return rsplit(sep, -1);
	}

	public List rsplit([BytesConversionNoString] IList<byte> sep, int maxsplit)
	{
		return _bytes.RightSplit(sep, maxsplit, (IList<byte> x) => new ByteArray(new List<byte>(x)));
	}

	public ByteArray rstrip()
	{
		lock (this)
		{
			List<byte> list = _bytes.RightStrip();
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public ByteArray rstrip([BytesConversionNoString] IList<byte> bytes)
	{
		lock (this)
		{
			List<byte> list = _bytes.RightStrip(bytes);
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public List split()
	{
		lock (this)
		{
			return ((IList<byte>)_bytes).SplitInternal((IList<byte>)null, -1, (Func<List<byte>, object>)((List<byte> x) => new ByteArray(x)));
		}
	}

	public List split([BytesConversionNoString] IList<byte> sep)
	{
		return split(sep, -1);
	}

	public List split([BytesConversionNoString] IList<byte> sep, int maxsplit)
	{
		lock (this)
		{
			return _bytes.Split(sep, maxsplit, (List<byte> x) => new ByteArray(x));
		}
	}

	public List splitlines()
	{
		return splitlines(keepends: false);
	}

	public List splitlines(bool keepends)
	{
		lock (this)
		{
			return _bytes.SplitLines(keepends, (List<byte> x) => new ByteArray(x));
		}
	}

	public bool startswith([BytesConversion] IList<byte> prefix)
	{
		lock (this)
		{
			return _bytes.StartsWith(prefix);
		}
	}

	public bool startswith([BytesConversion] IList<byte> prefix, int start)
	{
		lock (this)
		{
			int num = Count;
			if (start > num)
			{
				return false;
			}
			if (start < 0)
			{
				start += num;
				if (start < 0)
				{
					start = 0;
				}
			}
			return _bytes.Substring(start).StartsWith(prefix);
		}
	}

	public bool startswith([BytesConversion] IList<byte> prefix, int start, int end)
	{
		lock (this)
		{
			return _bytes.StartsWith(prefix, start, end);
		}
	}

	public bool startswith(PythonTuple prefix)
	{
		lock (this)
		{
			return _bytes.StartsWith(prefix);
		}
	}

	public bool startswith(PythonTuple prefix, int start)
	{
		lock (this)
		{
			return _bytes.StartsWith(prefix, start);
		}
	}

	public bool startswith(PythonTuple prefix, int start, int end)
	{
		lock (this)
		{
			return _bytes.StartsWith(prefix, start, end);
		}
	}

	public ByteArray strip()
	{
		lock (this)
		{
			List<byte> list = _bytes.Strip();
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public ByteArray strip([BytesConversionNoString] IList<byte> chars)
	{
		lock (this)
		{
			List<byte> list = _bytes.Strip(chars);
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public ByteArray swapcase()
	{
		lock (this)
		{
			return new ByteArray(_bytes.SwapCase());
		}
	}

	public ByteArray title()
	{
		lock (this)
		{
			List<byte> list = _bytes.Title();
			if (list == null)
			{
				return CopyThis();
			}
			return new ByteArray(list);
		}
	}

	public ByteArray translate([BytesConversion] IList<byte> table)
	{
		if (table == null)
		{
			throw PythonOps.TypeError("expected bytearray or bytes, got NoneType");
		}
		lock (this)
		{
			if (table.Count != 256)
			{
				throw PythonOps.ValueError("translation table must be 256 characters long");
			}
			if (Count == 0)
			{
				return CopyThis();
			}
			return new ByteArray(_bytes.Translate(table, null));
		}
	}

	public ByteArray translate([BytesConversion] IList<byte> table, [BytesConversion] IList<byte> deletechars)
	{
		if (table == null)
		{
			throw PythonOps.TypeError("expected bytearray or bytes, got NoneType");
		}
		if (deletechars == null)
		{
			throw PythonOps.TypeError("expected bytes or bytearray, got None");
		}
		lock (this)
		{
			return new ByteArray(_bytes.Translate(table, deletechars));
		}
	}

	public ByteArray upper()
	{
		lock (this)
		{
			return new ByteArray(_bytes.ToUpper());
		}
	}

	public ByteArray zfill(int width)
	{
		lock (this)
		{
			int num = width - Count;
			if (num <= 0)
			{
				return CopyThis();
			}
			return new ByteArray(_bytes.ZeroFill(width, num));
		}
	}

	public int __alloc__()
	{
		if (_bytes.Count == 0)
		{
			return 0;
		}
		return _bytes.Count + 1;
	}

	public bool __contains__([BytesConversionNoString] IList<byte> bytes)
	{
		return this.IndexOf(bytes, 0) != -1;
	}

	public bool __contains__(int value)
	{
		return IndexOf(value.ToByteChecked()) != -1;
	}

	public bool __contains__(CodeContext context, object value)
	{
		if (value is Extensible<int>)
		{
			return IndexOf(((Extensible<int>)value).Value.ToByteChecked()) != -1;
		}
		if (value is BigInteger)
		{
			return IndexOf(((BigInteger)value).ToByteChecked()) != -1;
		}
		if (value is Extensible<BigInteger>)
		{
			return IndexOf(((Extensible<BigInteger>)value).Value.ToByteChecked()) != -1;
		}
		throw PythonOps.TypeError("Type {0} doesn't support the buffer API", PythonContext.GetContext(context).PythonOptions.Python30 ? PythonTypeOps.GetOldName(value) : PythonTypeOps.GetName(value));
	}

	public PythonTuple __reduce__(CodeContext context)
	{
		return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), PythonTuple.MakeTuple(this.MakeString(), "latin-1"), (GetType() == typeof(ByteArray)) ? null : ObjectOps.ReduceProtocol0(context, this)[2]);
	}

	public virtual string __repr__(CodeContext context)
	{
		lock (this)
		{
			return "bytearray(" + _bytes.BytesRepr() + ")";
		}
	}

	public static ByteArray operator +(ByteArray self, ByteArray other)
	{
		if (self == null)
		{
			throw PythonOps.TypeError("expected ByteArray, got None");
		}
		List<byte> list;
		lock (self)
		{
			list = new List<byte>(self._bytes);
		}
		lock (other)
		{
			list.AddRange(other._bytes);
		}
		return new ByteArray(list);
	}

	public static ByteArray operator +(ByteArray self, Bytes other)
	{
		List<byte> list;
		lock (self)
		{
			list = new List<byte>(self._bytes);
		}
		list.AddRange(other);
		return new ByteArray(list);
	}

	public static ByteArray operator *(ByteArray x, int y)
	{
		lock (x)
		{
			if (y == 1)
			{
				return x.CopyThis();
			}
			return new ByteArray(x._bytes.Multiply(y));
		}
	}

	public static ByteArray operator *(int x, ByteArray y)
	{
		return y * x;
	}

	public static bool operator >(ByteArray x, ByteArray y)
	{
		if (y == null)
		{
			return true;
		}
		using (new OrderedLocker(x, y))
		{
			return x._bytes.Compare(y._bytes) > 0;
		}
	}

	public static bool operator <(ByteArray x, ByteArray y)
	{
		if (y == null)
		{
			return false;
		}
		using (new OrderedLocker(x, y))
		{
			return x._bytes.Compare(y._bytes) < 0;
		}
	}

	public static bool operator >=(ByteArray x, ByteArray y)
	{
		if (y == null)
		{
			return true;
		}
		using (new OrderedLocker(x, y))
		{
			return x._bytes.Compare(y._bytes) >= 0;
		}
	}

	public static bool operator <=(ByteArray x, ByteArray y)
	{
		if (y == null)
		{
			return false;
		}
		using (new OrderedLocker(x, y))
		{
			return x._bytes.Compare(y._bytes) <= 0;
		}
	}

	public static bool operator >(ByteArray x, Bytes y)
	{
		if (y == null)
		{
			return true;
		}
		lock (x)
		{
			return x._bytes.Compare(y) > 0;
		}
	}

	public static bool operator <(ByteArray x, Bytes y)
	{
		if (y == null)
		{
			return false;
		}
		lock (x)
		{
			return x._bytes.Compare(y) < 0;
		}
	}

	public static bool operator >=(ByteArray x, Bytes y)
	{
		if (y == null)
		{
			return true;
		}
		lock (x)
		{
			return x._bytes.Compare(y) >= 0;
		}
	}

	public static bool operator <=(ByteArray x, Bytes y)
	{
		if (y == null)
		{
			return false;
		}
		lock (x)
		{
			return x._bytes.Compare(y) <= 0;
		}
	}

	[SpecialName]
	public void DeleteItem(int index)
	{
		_bytes.RemoveAt(PythonOps.FixIndex(index, _bytes.Count));
	}

	[SpecialName]
	public void DeleteItem(Slice slice)
	{
		if (slice == null)
		{
			throw PythonOps.TypeError("list indices must be integers or slices");
		}
		lock (this)
		{
			slice.indices(_bytes.Count, out var ostart, out var ostop, out var ostep);
			if ((ostep > 0 && ostart >= ostop) || (ostep < 0 && ostart <= ostop))
			{
				return;
			}
			if (ostep == 1)
			{
				int num = ostart;
				int num2 = ostop;
				while (num2 < _bytes.Count)
				{
					_bytes[num] = _bytes[num2];
					num2++;
					num++;
				}
				_bytes.RemoveRange(num, ostop - ostart);
				return;
			}
			if (ostep == -1)
			{
				int num3 = ostop + 1;
				int num4 = ostart + 1;
				while (num4 < _bytes.Count)
				{
					_bytes[num3] = _bytes[num4];
					num4++;
					num3++;
				}
				_bytes.RemoveRange(num3, ostart - ostop);
				return;
			}
			if (ostep < 0)
			{
				int i;
				for (i = ostart; i > ostop; i += ostep)
				{
				}
				i -= ostep;
				ostop = ostart + 1;
				ostart = i;
				ostep = -ostep;
			}
			int num6;
			int num7;
			int num5 = (num6 = (num7 = ostart));
			while (num5 < ostop && num7 < ostop)
			{
				if (num7 != num6)
				{
					_bytes[num5++] = _bytes[num7];
				}
				else
				{
					num6 += ostep;
				}
				num7++;
			}
			while (ostop < _bytes.Count)
			{
				_bytes[num5++] = _bytes[ostop++];
			}
			_bytes.RemoveRange(num5, _bytes.Count - num5);
		}
	}

	private static ByteArray JoinOne(object curVal)
	{
		if (!(curVal is IList<byte>))
		{
			throw PythonOps.TypeError("can only join an iterable of bytes");
		}
		return new ByteArray(new List<byte>(curVal as IList<byte>));
	}

	private ByteArray CopyThis()
	{
		return new ByteArray(new List<byte>(_bytes));
	}

	private void SliceNoStep(int start, int stop, IList<byte> value)
	{
		IList<byte> bytes = GetBytes(value);
		lock (this)
		{
			if (start > stop)
			{
				int capacity = Count + bytes.Count;
				List<byte> list = new List<byte>(capacity);
				int num = 0;
				for (num = 0; num < start; num++)
				{
					list.Add(_bytes[num]);
				}
				for (int i = 0; i < bytes.Count; i++)
				{
					list.Add(bytes[i]);
				}
				for (; num < Count; num++)
				{
					list.Add(_bytes[num]);
				}
				_bytes = list;
			}
			else if (stop - start == bytes.Count)
			{
				for (int j = 0; j < bytes.Count; j++)
				{
					_bytes[j + start] = bytes[j];
				}
			}
			else
			{
				int capacity2 = Count - (stop - start) + bytes.Count;
				List<byte> list2 = new List<byte>(capacity2);
				for (int k = 0; k < start; k++)
				{
					list2.Add(_bytes[k]);
				}
				for (int l = 0; l < bytes.Count; l++)
				{
					list2.Add(bytes[l]);
				}
				for (int m = stop; m < Count; m++)
				{
					list2.Add(_bytes[m]);
				}
				_bytes = list2;
			}
		}
	}

	private static byte GetByte(object value)
	{
		if (value is double || value is Extensible<double> || value is float)
		{
			throw PythonOps.TypeError("an integer or string of size 1 is required");
		}
		return ByteOps.GetByteListOk(value);
	}

	private static IList<byte> GetBytes(object value)
	{
		ListGenericWrapper<byte> listGenericWrapper = value as ListGenericWrapper<byte>;
		if (listGenericWrapper == null && value is IList<byte>)
		{
			return (IList<byte>)value;
		}
		if (value is string || value is Extensible<string>)
		{
			throw PythonOps.TypeError("unicode argument without an encoding");
		}
		List<byte> list = new List<byte>();
		IEnumerator enumerator = PythonOps.GetEnumerator(value);
		while (enumerator.MoveNext())
		{
			list.Add(GetByte(enumerator.Current));
		}
		return list;
	}

	[PythonHidden]
	public int IndexOf(byte item)
	{
		lock (this)
		{
			return _bytes.IndexOf(item);
		}
	}

	[PythonHidden]
	public void Insert(int index, byte item)
	{
		_bytes.Insert(index, item);
	}

	[PythonHidden]
	public void RemoveAt(int index)
	{
		_bytes.RemoveAt(index);
	}

	[PythonHidden]
	public void Add(byte item)
	{
		lock (this)
		{
			_bytes.Add(item);
		}
	}

	[PythonHidden]
	public void Clear()
	{
		lock (this)
		{
			_bytes.Clear();
		}
	}

	[PythonHidden]
	public bool Contains(byte item)
	{
		lock (this)
		{
			return _bytes.Contains(item);
		}
	}

	[PythonHidden]
	public void CopyTo(byte[] array, int arrayIndex)
	{
		lock (this)
		{
			_bytes.CopyTo(array, arrayIndex);
		}
	}

	[PythonHidden]
	public bool Remove(byte item)
	{
		lock (this)
		{
			return _bytes.Remove(item);
		}
	}

	public IEnumerator __iter__()
	{
		return PythonOps.BytesIntEnumerator(this).Key;
	}

	[PythonHidden]
	public IEnumerator<byte> GetEnumerator()
	{
		return _bytes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _bytes.GetEnumerator();
	}

	public override int GetHashCode()
	{
		return PythonTuple.MakeTuple(_bytes.ToArray()).GetHashCode();
	}

	public override bool Equals(object other)
	{
		if (!(other is IList<byte> list) || Count != list.Count)
		{
			return false;
		}
		if (Count == 0)
		{
			return true;
		}
		using (new OrderedLocker(this, other))
		{
			for (int i = 0; i < Count; i++)
			{
				if (_bytes[i] != list[i])
				{
					return false;
				}
			}
		}
		return true;
	}

	public override string ToString()
	{
		return _bytes.MakeString();
	}

	Bytes IBufferProtocol.GetItem(int index)
	{
		lock (this)
		{
			return new Bytes(new byte[1] { _bytes[PythonOps.FixIndex(index, _bytes.Count)] });
		}
	}

	void IBufferProtocol.SetItem(int index, object value)
	{
		this[index] = value;
	}

	void IBufferProtocol.SetSlice(Slice index, object value)
	{
		this[index] = value;
	}

	IList<BigInteger> IBufferProtocol.GetShape(int start, int? end)
	{
		if (end.HasValue)
		{
			return new BigInteger[1] { (BigInteger)end.Value - (BigInteger)start };
		}
		return new BigInteger[1] { (BigInteger)_bytes.Count - (BigInteger)start };
	}

	Bytes IBufferProtocol.ToBytes(int start, int? end)
	{
		if (start == 0 && !end.HasValue)
		{
			return new Bytes(this);
		}
		return new Bytes((ByteArray)this[new Slice(start, end)]);
	}

	List IBufferProtocol.ToList(int start, int? end)
	{
		List<byte> list = _bytes.Slice(new Slice(start, end));
		if (list == null)
		{
			return new List();
		}
		return new List(list.ToArray());
	}
}
