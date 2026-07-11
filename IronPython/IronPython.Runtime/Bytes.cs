using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.InteropServices;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

[PythonType("bytes")]
public class Bytes : IList<byte>, ICollection<byte>, IEnumerable<byte>, IEnumerable, ICodeFormattable, IExpressionSerializable, IBufferProtocol
{
	internal byte[] _bytes;

	internal static Bytes Empty = new Bytes();

	public object this[CodeContext context, int index]
	{
		get
		{
			byte b = _bytes[PythonOps.FixIndex(index, _bytes.Length)];
			if (PythonContext.GetContext(context).PythonOptions.Python30)
			{
				return (int)b;
			}
			return new Bytes(new byte[1] { b });
		}
		[PythonHidden]
		set
		{
			throw new InvalidOperationException();
		}
	}

	public object this[CodeContext context, BigInteger index]
	{
		get
		{
			if (index.AsInt32(out var ret))
			{
				return this[context, ret];
			}
			throw PythonOps.IndexError("cannot fit long in index");
		}
	}

	public Bytes this[Slice slice]
	{
		get
		{
			List<byte> list = _bytes.Slice(slice);
			if (list == null)
			{
				return Empty;
			}
			return new Bytes(list.ToArray());
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
			throw new InvalidOperationException();
		}
	}

	public int Count
	{
		[PythonHidden]
		get
		{
			return _bytes.Length;
		}
	}

	public bool IsReadOnly
	{
		[PythonHidden]
		get
		{
			return true;
		}
	}

	int IBufferProtocol.ItemCount => _bytes.Length;

	string IBufferProtocol.Format => "B";

	BigInteger IBufferProtocol.ItemSize => 1;

	BigInteger IBufferProtocol.NumberDimensions => 1;

	bool IBufferProtocol.ReadOnly => true;

	PythonTuple IBufferProtocol.Strides => PythonTuple.MakeTuple(1);

	object IBufferProtocol.SubOffsets => null;

	public Bytes()
	{
		_bytes = new byte[0];
	}

	public Bytes([NotNull][BytesConversion] IList<byte> bytes)
	{
		_bytes = ArrayUtils.ToArray(bytes);
	}

	public Bytes([NotNull] List bytes)
	{
		_bytes = ByteOps.GetBytes(bytes, ByteOps.GetByteListOk).ToArray();
	}

	public Bytes(int size)
	{
		_bytes = new byte[size];
	}

	private Bytes(byte[] bytes)
	{
		_bytes = bytes;
	}

	public Bytes(CodeContext context, [NotNull] string unicode, [NotNull] string encoding)
	{
		_bytes = StringOps.encode(context, unicode, encoding, "strict").MakeByteArray();
	}

	internal static Bytes Make(byte[] bytes)
	{
		return new Bytes(bytes);
	}

	public Bytes capitalize()
	{
		if (Count == 0)
		{
			return this;
		}
		return new Bytes(_bytes.Capitalize());
	}

	public Bytes center(int width)
	{
		return center(width, " ");
	}

	public Bytes center(int width, [NotNull] string fillchar)
	{
		List<byte> list = _bytes.TryCenter(width, fillchar.ToByte("center", 2));
		if (list == null)
		{
			return this;
		}
		return new Bytes(list);
	}

	public Bytes center(int width, [BytesConversion] IList<byte> fillchar)
	{
		List<byte> list = _bytes.TryCenter(width, fillchar.ToByte("center", 2));
		if (list == null)
		{
			return this;
		}
		return new Bytes(list);
	}

	public ByteArray center(int width, List fillchar)
	{
		throw PythonOps.TypeError("center() argument 2 must be byte, not list");
	}

	public int count([BytesConversion] IList<byte> sub)
	{
		return count(sub, 0, Count);
	}

	public int count([BytesConversion] IList<byte> sub, int start)
	{
		return count(sub, start, Count);
	}

	public int count([BytesConversion] IList<byte> ssub, int start, int end)
	{
		return _bytes.CountOf(ssub, start, end);
	}

	public int count(List sub)
	{
		throw PythonOps.TypeError("expected bytes or bytearray, got list");
	}

	public int count(List sub, int start)
	{
		throw PythonOps.TypeError("expected bytes or bytearray, got list");
	}

	public int count(List ssub, int start, int end)
	{
		throw PythonOps.TypeError("expected bytes or bytearray, got list");
	}

	public string decode(CodeContext context, [Optional] string encoding, [DefaultParameterValue("strict")][NotNull] string errors)
	{
		return StringOps.decode(context, _bytes.MakeString(), encoding, errors);
	}

	public bool endswith([BytesConversion] IList<byte> suffix)
	{
		return _bytes.EndsWith(suffix);
	}

	public bool endswith([BytesConversion] IList<byte> suffix, int start)
	{
		return _bytes.EndsWith(suffix, start);
	}

	public bool endswith([BytesConversion] IList<byte> suffix, int start, int end)
	{
		return _bytes.EndsWith(suffix, start, end);
	}

	public bool endswith(List suffix)
	{
		throw PythonOps.TypeError("expected bytes or bytearray, got list");
	}

	public bool endswith(List suffix, int start)
	{
		throw PythonOps.TypeError("expected bytes or bytearray, got list");
	}

	public bool endswith(List suffix, int start, int end)
	{
		throw PythonOps.TypeError("expected bytes or bytearray, got list");
	}

	public bool endswith(PythonTuple suffix)
	{
		return _bytes.EndsWith(suffix);
	}

	public bool endswith(PythonTuple suffix, int start)
	{
		return _bytes.EndsWith(suffix, start);
	}

	public bool endswith(PythonTuple suffix, int start, int end)
	{
		return _bytes.EndsWith(suffix, start, end);
	}

	public Bytes expandtabs()
	{
		return expandtabs(8);
	}

	public Bytes expandtabs(int tabsize)
	{
		return new Bytes(_bytes.ExpandTabs(tabsize));
	}

	public int find([BytesConversion] IList<byte> sub)
	{
		return _bytes.Find(sub);
	}

	public int find([BytesConversion] IList<byte> sub, int? start)
	{
		return _bytes.Find(sub, start);
	}

	public int find([BytesConversion] IList<byte> sub, int? start, int? end)
	{
		return _bytes.Find(sub, start, end);
	}

	public static Bytes fromhex(string @string)
	{
		return new Bytes(IListOfByteOps.FromHex(@string).ToArray());
	}

	public int index([BytesConversion] IList<byte> item)
	{
		return index(item, 0, Count);
	}

	public int index([BytesConversion] IList<byte> item, int? start)
	{
		return index(item, start, Count);
	}

	public int index([BytesConversion] IList<byte> item, int? start, int? stop)
	{
		int num = find(item, start, stop);
		if (num == -1)
		{
			throw PythonOps.ValueError("bytes.index(item): item not in bytes");
		}
		return num;
	}

	public bool isalnum()
	{
		return _bytes.IsAlphaNumeric();
	}

	public bool isalpha()
	{
		return _bytes.IsLetter();
	}

	public bool isdigit()
	{
		return _bytes.IsDigit();
	}

	public bool islower()
	{
		return _bytes.IsLower();
	}

	public bool isspace()
	{
		return _bytes.IsWhiteSpace();
	}

	public bool istitle()
	{
		return _bytes.IsTitle();
	}

	public bool isupper()
	{
		return _bytes.IsUpper();
	}

	public Bytes join(object sequence)
	{
		IEnumerator enumerator = PythonOps.GetEnumerator(sequence);
		if (!enumerator.MoveNext())
		{
			return Empty;
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
		return new Bytes(list);
	}

	public Bytes join([NotNull] List sequence)
	{
		if (sequence.__len__() == 0)
		{
			return new Bytes();
		}
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
		return new Bytes(list);
	}

	public Bytes ljust(int width)
	{
		return ljust(width, 32);
	}

	public Bytes ljust(int width, [NotNull] string fillchar)
	{
		return ljust(width, fillchar.ToByte("ljust", 2));
	}

	public Bytes ljust(int width, [BytesConversion] IList<byte> fillchar)
	{
		return ljust(width, fillchar.ToByte("ljust", 2));
	}

	private Bytes ljust(int width, byte fillchar)
	{
		int num = width - Count;
		if (num <= 0)
		{
			return this;
		}
		List<byte> list = new List<byte>(width);
		list.AddRange(_bytes);
		for (int i = 0; i < num; i++)
		{
			list.Add(fillchar);
		}
		return new Bytes(list);
	}

	public Bytes lower()
	{
		return new Bytes(_bytes.ToLower());
	}

	public Bytes lstrip()
	{
		List<byte> list = _bytes.LeftStrip();
		if (list == null)
		{
			return this;
		}
		return new Bytes(list);
	}

	public Bytes lstrip([BytesConversion] IList<byte> bytes)
	{
		lock (this)
		{
			List<byte> list = _bytes.LeftStrip(bytes);
			if (list == null)
			{
				return this;
			}
			return new Bytes(list);
		}
	}

	public PythonTuple partition([BytesConversion] IList<byte> sep)
	{
		if (sep == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (sep.Count == 0)
		{
			throw PythonOps.ValueError("empty separator");
		}
		object[] array = new object[3] { Empty, Empty, Empty };
		if (Count != 0)
		{
			int num = find(sep);
			if (num == -1)
			{
				array[0] = this;
			}
			else
			{
				array[0] = new Bytes(_bytes.Substring(0, num));
				array[1] = sep;
				array[2] = new Bytes(_bytes.Substring(num + sep.Count, Count - num - sep.Count));
			}
		}
		return new PythonTuple(array);
	}

	public Bytes replace([BytesConversion] IList<byte> old, [BytesConversion] IList<byte> new_)
	{
		if (old == null)
		{
			throw PythonOps.TypeError("expected bytes or bytearray, got NoneType");
		}
		return replace(old, new_, _bytes.Length);
	}

	public Bytes replace([BytesConversion] IList<byte> old, [BytesConversion] IList<byte> new_, int maxsplit)
	{
		if (old == null)
		{
			throw PythonOps.TypeError("expected bytes or bytearray, got NoneType");
		}
		if (maxsplit == 0)
		{
			return this;
		}
		return new Bytes(_bytes.Replace(old, new_, maxsplit));
	}

	public int rfind([BytesConversion] IList<byte> sub)
	{
		return rfind(sub, 0, Count);
	}

	public int rfind([BytesConversion] IList<byte> sub, int? start)
	{
		return rfind(sub, start, Count);
	}

	public int rfind([BytesConversion] IList<byte> sub, int? start, int? end)
	{
		return _bytes.ReverseFind(sub, start, end);
	}

	public int rindex([BytesConversion] IList<byte> sub)
	{
		return rindex(sub, 0, Count);
	}

	public int rindex([BytesConversion] IList<byte> sub, int? start)
	{
		return rindex(sub, start, Count);
	}

	public int rindex([BytesConversion] IList<byte> sub, int? start, int? end)
	{
		int num = rfind(sub, start, end);
		if (num == -1)
		{
			throw PythonOps.ValueError("substring {0} not found in {1}", sub, this);
		}
		return num;
	}

	public Bytes rjust(int width)
	{
		return rjust(width, 32);
	}

	public Bytes rjust(int width, [NotNull] string fillchar)
	{
		return rjust(width, fillchar.ToByte("rjust", 2));
	}

	public Bytes rjust(int width, [BytesConversion] IList<byte> fillchar)
	{
		return rjust(width, fillchar.ToByte("rjust", 2));
	}

	private Bytes rjust(int width, byte fillchar)
	{
		int num = width - Count;
		if (num <= 0)
		{
			return this;
		}
		List<byte> list = new List<byte>(width);
		for (int i = 0; i < num; i++)
		{
			list.Add(fillchar);
		}
		list.AddRange(_bytes);
		return new Bytes(list);
	}

	public PythonTuple rpartition([BytesConversion] IList<byte> sep)
	{
		if (sep == null)
		{
			throw PythonOps.TypeError("expected string, got NoneType");
		}
		if (sep.Count == 0)
		{
			throw PythonOps.ValueError("empty separator");
		}
		object[] array = new object[3] { Empty, Empty, Empty };
		if (Count != 0)
		{
			int num = rfind(sep);
			if (num == -1)
			{
				array[2] = this;
			}
			else
			{
				array[0] = new Bytes(_bytes.Substring(0, num));
				array[1] = sep;
				array[2] = new Bytes(_bytes.Substring(num + sep.Count, Count - num - sep.Count));
			}
		}
		return new PythonTuple(array);
	}

	public List rsplit()
	{
		return ((IList<byte>)_bytes).SplitInternal((IList<byte>)null, -1, (Func<List<byte>, object>)((List<byte> x) => new Bytes(x)));
	}

	public List rsplit([BytesConversion] IList<byte> sep)
	{
		return rsplit(sep, -1);
	}

	public List rsplit([BytesConversion] IList<byte> sep, int maxsplit)
	{
		return _bytes.RightSplit(sep, maxsplit, (IList<byte> x) => new Bytes(new List<byte>(x)));
	}

	public Bytes rstrip()
	{
		List<byte> list = _bytes.RightStrip();
		if (list == null)
		{
			return this;
		}
		return new Bytes(list);
	}

	public Bytes rstrip([BytesConversion] IList<byte> bytes)
	{
		lock (this)
		{
			List<byte> list = _bytes.RightStrip(bytes);
			if (list == null)
			{
				return this;
			}
			return new Bytes(list);
		}
	}

	public List split()
	{
		return ((IList<byte>)_bytes).SplitInternal((IList<byte>)null, -1, (Func<List<byte>, object>)((List<byte> x) => new Bytes(x)));
	}

	public List split([BytesConversion] IList<byte> sep)
	{
		return split(sep, -1);
	}

	public List split([BytesConversion] IList<byte> sep, int maxsplit)
	{
		return _bytes.Split(sep, maxsplit, (List<byte> x) => new Bytes(x));
	}

	public List splitlines()
	{
		return splitlines(keepends: false);
	}

	public List splitlines(bool keepends)
	{
		return _bytes.SplitLines(keepends, (List<byte> x) => new Bytes(x));
	}

	public bool startswith([BytesConversion] IList<byte> prefix)
	{
		return _bytes.StartsWith(prefix);
	}

	public bool startswith([BytesConversion] IList<byte> prefix, int start)
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

	public bool startswith([BytesConversion] IList<byte> prefix, int start, int end)
	{
		return _bytes.StartsWith(prefix, start, end);
	}

	public bool startswith(PythonTuple prefix)
	{
		return _bytes.StartsWith(prefix);
	}

	public bool startswith(PythonTuple prefix, int start)
	{
		return _bytes.StartsWith(prefix, start);
	}

	public bool startswith(PythonTuple prefix, int start, int end)
	{
		return _bytes.StartsWith(prefix, start, end);
	}

	public Bytes strip()
	{
		List<byte> list = _bytes.Strip();
		if (list == null)
		{
			return this;
		}
		return new Bytes(list);
	}

	public Bytes strip([BytesConversion] IList<byte> chars)
	{
		lock (this)
		{
			List<byte> list = _bytes.Strip(chars);
			if (list == null)
			{
				return this;
			}
			return new Bytes(list);
		}
	}

	public Bytes swapcase()
	{
		return new Bytes(_bytes.SwapCase());
	}

	public Bytes title()
	{
		lock (this)
		{
			List<byte> list = _bytes.Title();
			if (list == null)
			{
				return this;
			}
			return new Bytes(list.ToArray());
		}
	}

	public Bytes translate([BytesConversion] IList<byte> table)
	{
		if (table == null)
		{
			return this;
		}
		if (table.Count != 256)
		{
			throw PythonOps.ValueError("translation table must be 256 characters long");
		}
		if (Count == 0)
		{
			return this;
		}
		return new Bytes(_bytes.Translate(table, null));
	}

	public Bytes translate([BytesConversion] IList<byte> table, [BytesConversion] IList<byte> deletechars)
	{
		if (deletechars == null)
		{
			throw PythonOps.TypeError("expected bytes or bytearray, got None");
		}
		if (Count == 0)
		{
			return this;
		}
		return new Bytes(_bytes.Translate(table, deletechars));
	}

	public Bytes upper()
	{
		return new Bytes(_bytes.ToUpper());
	}

	public Bytes zfill(int width)
	{
		int num = width - Count;
		if (num <= 0)
		{
			return this;
		}
		return new Bytes(_bytes.ZeroFill(width, num));
	}

	public bool __contains__([BytesConversion] IList<byte> bytes)
	{
		return this.IndexOf(bytes, 0) != -1;
	}

	public bool __contains__(CodeContext context, int value)
	{
		if (!PythonContext.GetContext(context).PythonOptions.Python30)
		{
			throw PythonOps.TypeError("'in <bytes>' requires string or bytes as left operand, not int");
		}
		return IndexOf(value.ToByteChecked()) != -1;
	}

	public bool __contains__(CodeContext context, object value)
	{
		if (!PythonContext.GetContext(context).PythonOptions.Python30)
		{
			throw PythonOps.TypeError("'in <bytes>' requires string or bytes as left operand, not {0}", PythonTypeOps.GetName(value));
		}
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
		throw PythonOps.TypeError("Type {0} doesn't support the buffer API", PythonTypeOps.GetOldName(value));
	}

	public PythonTuple __reduce__(CodeContext context)
	{
		return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), PythonTuple.MakeTuple(this.MakeString(), "latin-1"), (GetType() == typeof(Bytes)) ? null : ObjectOps.ReduceProtocol0(context, this)[2]);
	}

	public virtual string __repr__(CodeContext context)
	{
		return _bytes.BytesRepr();
	}

	public override string ToString()
	{
		return this.MakeString();
	}

	public static Bytes operator +(Bytes self, Bytes other)
	{
		if (self == null)
		{
			throw PythonOps.TypeError("expected bytes, got None");
		}
		List<byte> list = new List<byte>(self._bytes);
		list.AddRange(other._bytes);
		return new Bytes(list);
	}

	public static ByteArray operator +(Bytes self, ByteArray other)
	{
		List<byte> list = new List<byte>(self._bytes);
		lock (other)
		{
			list.AddRange(other);
		}
		return new ByteArray(list);
	}

	public static string operator +(Bytes self, string other)
	{
		return self.ToString() + other;
	}

	public static string operator +(string other, Bytes self)
	{
		return other + self.ToString();
	}

	public static Bytes operator *(Bytes x, int y)
	{
		if (y == 1)
		{
			return x;
		}
		return new Bytes(x._bytes.Multiply(y));
	}

	public static Bytes operator *(int x, Bytes y)
	{
		return y * x;
	}

	public static bool operator >(Bytes x, Bytes y)
	{
		if (y == null)
		{
			return true;
		}
		return x._bytes.Compare(y._bytes) > 0;
	}

	public static bool operator <(Bytes x, Bytes y)
	{
		if (y == null)
		{
			return false;
		}
		return x._bytes.Compare(y._bytes) < 0;
	}

	public static bool operator >=(Bytes x, Bytes y)
	{
		if (y == null)
		{
			return true;
		}
		return x._bytes.Compare(y._bytes) >= 0;
	}

	public static bool operator <=(Bytes x, Bytes y)
	{
		if (y == null)
		{
			return false;
		}
		return x._bytes.Compare(y._bytes) <= 0;
	}

	[PythonHidden]
	public byte[] ToByteArray()
	{
		byte[] array = null;
		if (_bytes != null)
		{
			array = new byte[_bytes.Length];
			_bytes.CopyTo(array, 0);
		}
		return array;
	}

	[PythonHidden]
	public byte[] GetUnsafeByteArray()
	{
		return _bytes;
	}

	private static Bytes JoinOne(object curVal)
	{
		if (!(curVal is IList<byte>))
		{
			throw PythonOps.TypeError("can only join an iterable of bytes");
		}
		return (curVal as Bytes) ?? new Bytes(curVal as IList<byte>);
	}

	internal static Bytes Concat(IList<Bytes> list, int length)
	{
		byte[] array = new byte[length];
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			Array.Copy(list[i]._bytes, 0, array, num, list[i]._bytes.Length);
			num += list[i]._bytes.Length;
		}
		return new Bytes(array);
	}

	[PythonHidden]
	public int IndexOf(byte item)
	{
		for (int i = 0; i < _bytes.Length; i++)
		{
			if (_bytes[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	[PythonHidden]
	public void Insert(int index, byte item)
	{
		throw new InvalidOperationException();
	}

	[PythonHidden]
	public void RemoveAt(int index)
	{
		throw new InvalidOperationException();
	}

	[PythonHidden]
	public void Add(byte item)
	{
		throw new InvalidOperationException();
	}

	[PythonHidden]
	public void Clear()
	{
		throw new InvalidOperationException();
	}

	[PythonHidden]
	public bool Contains(byte item)
	{
		return ((IList<byte>)_bytes).Contains(item);
	}

	[PythonHidden]
	public void CopyTo(byte[] array, int arrayIndex)
	{
		_bytes.CopyTo(array, arrayIndex);
	}

	[PythonHidden]
	public bool Remove(byte item)
	{
		throw new InvalidOperationException();
	}

	[PythonHidden]
	public IEnumerator<byte> GetEnumerator()
	{
		return ((IEnumerable<byte>)_bytes).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _bytes.GetEnumerator();
	}

	public override bool Equals(object obj)
	{
		if (obj is IList<byte> other)
		{
			return _bytes.Compare(other) == 0;
		}
		string text = obj as string;
		if (text == null && obj is Extensible<string> extensible)
		{
			text = extensible.Value;
		}
		if (text != null)
		{
			return ToString() == text;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	Expression IExpressionSerializable.CreateExpression()
	{
		return Expression.Call(typeof(PythonOps).GetMethod("MakeBytes"), Expression.NewArrayInit(typeof(byte), ArrayUtils.ConvertAll(_bytes, (byte b) => Expression.Constant(b))));
	}

	Bytes IBufferProtocol.GetItem(int index)
	{
		byte b = _bytes[PythonOps.FixIndex(index, _bytes.Length)];
		return new Bytes(new byte[1] { b });
	}

	void IBufferProtocol.SetItem(int index, object value)
	{
		throw new InvalidOperationException();
	}

	void IBufferProtocol.SetSlice(Slice index, object value)
	{
		throw new InvalidOperationException();
	}

	IList<BigInteger> IBufferProtocol.GetShape(int start, int? end)
	{
		if (end.HasValue)
		{
			return new BigInteger[1] { (BigInteger)end.Value - (BigInteger)start };
		}
		return new BigInteger[1] { (BigInteger)_bytes.Length - (BigInteger)start };
	}

	Bytes IBufferProtocol.ToBytes(int start, int? end)
	{
		if (start == 0 && !end.HasValue)
		{
			return this;
		}
		return this[new Slice(start, end)];
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
