using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class ArrayModule
{
	[PythonType]
	public class array : IPythonArray, IWeakReferenceable, ICollection, ICodeFormattable, IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable, IStructuralEquatable
	{
		private abstract class ArrayData
		{
			public abstract Type StorageType { get; }

			public abstract int Length { get; set; }

			public abstract void SetData(int index, object value);

			public abstract object GetData(int index);

			public abstract void Append(object value);

			public abstract int Count(object value);

			public abstract bool CanStore(object value);

			public abstract int Index(object value);

			public abstract void Insert(int index, object value);

			public abstract void Remove(object value);

			public abstract void RemoveAt(int index);

			public abstract void Swap(int x, int y);

			public abstract void Clear();

			public abstract IntPtr GetAddress();

			public abstract ArrayData Multiply(int count);
		}

		private class ArrayData<T> : ArrayData
		{
			private T[] _data;

			private int _count;

			private GCHandle? _dataHandle;

			public T[] Data
			{
				get
				{
					return _data;
				}
				set
				{
					_data = value;
				}
			}

			public override int Length
			{
				get
				{
					return _count;
				}
				set
				{
					_count = value;
				}
			}

			public override Type StorageType => typeof(T);

			public ArrayData()
			{
				GC.SuppressFinalize(this);
				_data = new T[8];
			}

			private ArrayData(int size)
			{
				GC.SuppressFinalize(this);
				_data = new T[size];
				_count = size;
			}

			~ArrayData()
			{
				_dataHandle.Value.Free();
			}

			public override object GetData(int index)
			{
				return _data[index];
			}

			public override void SetData(int index, object value)
			{
				_data[index] = GetValue(value);
			}

			private static T GetValue(object value)
			{
				if (!(value is T))
				{
					if (!Converter.TryConvert(value, typeof(T), out var result))
					{
						if (value != null && typeof(T).IsPrimitive && typeof(T) != typeof(char))
						{
							throw PythonOps.OverflowError("couldn't convert {1} to {0}", DynamicHelpers.GetPythonTypeFromType(typeof(T)).Name, DynamicHelpers.GetPythonType(value).Name);
						}
						throw PythonOps.TypeError("expected {0}, got {1}", DynamicHelpers.GetPythonTypeFromType(typeof(T)).Name, DynamicHelpers.GetPythonType(value).Name);
					}
					value = result;
				}
				return (T)value;
			}

			public override void Append(object value)
			{
				EnsureSize(_count + 1);
				_data[_count++] = GetValue(value);
			}

			public void EnsureSize(int size)
			{
				if (_data.Length < size)
				{
					Array.Resize(ref _data, _data.Length * 2);
					if (_dataHandle.HasValue)
					{
						_dataHandle.Value.Free();
						_dataHandle = null;
						GC.SuppressFinalize(this);
					}
				}
			}

			public override int Count(object value)
			{
				T value2 = GetValue(value);
				int num = 0;
				for (int i = 0; i < _count; i++)
				{
					if (_data[i].Equals(value2))
					{
						num++;
					}
				}
				return num;
			}

			public override void Insert(int index, object value)
			{
				EnsureSize(_count + 1);
				if (index < _count)
				{
					Array.Copy(_data, index, _data, index + 1, _count - index);
				}
				_data[index] = GetValue(value);
				_count++;
			}

			public override int Index(object value)
			{
				T value2 = GetValue(value);
				for (int i = 0; i < _count; i++)
				{
					if (_data[i].Equals(value2))
					{
						return i;
					}
				}
				return -1;
			}

			public override void Remove(object value)
			{
				T value2 = GetValue(value);
				for (int i = 0; i < _count; i++)
				{
					if (_data[i].Equals(value2))
					{
						RemoveAt(i);
						return;
					}
				}
				throw PythonOps.ValueError("couldn't find value to remove");
			}

			public override void RemoveAt(int index)
			{
				_count--;
				if (index < _count)
				{
					Array.Copy(_data, index + 1, _data, index, _count - index);
				}
			}

			public override void Swap(int x, int y)
			{
				T val = _data[x];
				_data[x] = _data[y];
				_data[y] = val;
			}

			public override void Clear()
			{
				_count = 0;
			}

			public override bool CanStore(object value)
			{
				if (!(value is T) && !Converter.TryConvert(value, typeof(T), out var _))
				{
					return false;
				}
				return true;
			}

			public override IntPtr GetAddress()
			{
				if (!_dataHandle.HasValue)
				{
					_dataHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);
					GC.ReRegisterForFinalize(this);
				}
				return _dataHandle.Value.AddrOfPinnedObject();
			}

			public override ArrayData Multiply(int count)
			{
				ArrayData<T> arrayData = new ArrayData<T>(count * _count);
				if (count != 0)
				{
					Array.Copy(_data, arrayData._data, _count);
					int num = count * _count;
					int num2 = _count;
					int num3 = _count;
					while (num3 < num)
					{
						Array.Copy(arrayData._data, 0, arrayData._data, num3, Math.Min(num2, num - num3));
						num3 += num2;
						num2 *= 2;
					}
				}
				return arrayData;
			}
		}

		public const object __hash__ = null;

		private ArrayData _data;

		private char _typeCode;

		private WeakRefTracker _tracker;

		public int itemsize
		{
			get
			{
				switch (_typeCode)
				{
				case 'B':
				case 'b':
				case 'c':
				case 'p':
				case 's':
				case 'x':
					return 1;
				case 'H':
				case 'h':
				case 'u':
					return 2;
				case 'I':
				case 'L':
				case 'f':
				case 'i':
				case 'l':
					return 4;
				case 'P':
					return IntPtr.Size;
				case 'Q':
				case 'd':
				case 'q':
					return 8;
				default:
					return 0;
				}
			}
		}

		public virtual object this[int index]
		{
			get
			{
				object data = _data.GetData(PythonOps.FixIndex(index, _data.Length));
				switch (_typeCode)
				{
				case 'b':
					return (int)(sbyte)data;
				case 'B':
					return (int)(byte)data;
				case 'c':
				case 'u':
					return new string((char)data, 1);
				case 'h':
					return (int)(short)data;
				case 'H':
					return (int)(ushort)data;
				case 'l':
					return data;
				case 'i':
					return data;
				case 'L':
					return (BigInteger)(uint)data;
				case 'I':
					return (BigInteger)(uint)data;
				case 'f':
					return (double)(float)data;
				case 'd':
					return data;
				default:
					throw PythonOps.ValueError("Bad type code (expected one of 'c', 'b', 'B', 'u', 'H', 'h', 'i', 'I', 'l', 'L', 'f', 'd')");
				}
			}
			set
			{
				_data.SetData(PythonOps.FixIndex(index, _data.Length), value);
			}
		}

		public object this[Slice index]
		{
			get
			{
				if (index == null)
				{
					throw PythonOps.TypeError("expected Slice, got None");
				}
				index.indices(_data.Length, out var ostart, out var ostop, out var ostep);
				array array2 = new array(new string(_typeCode, 1), Missing.Value);
				if (ostep < 0)
				{
					for (int i = ostart; i > ostop; i += ostep)
					{
						array2._data.Append(_data.GetData(i));
					}
				}
				else
				{
					for (int j = ostart; j < ostop; j += ostep)
					{
						array2._data.Append(_data.GetData(j));
					}
				}
				return array2;
			}
			set
			{
				if (index == null)
				{
					throw PythonOps.TypeError("expected Slice, got None");
				}
				CheckSliceAssignType(value);
				if (index.step != null)
				{
					if (object.ReferenceEquals(value, this))
					{
						value = tolist();
					}
					index.DoSliceAssign(SliceAssign, _data.Length, value);
					return;
				}
				index.indices(_data.Length, out var ostart, out var ostop, out var _);
				if (ostop < ostart)
				{
					ostop = ostart;
				}
				SliceNoStep(value, ostart, ostop);
			}
		}

		public string typecode => ScriptingRuntimeHelpers.CharToString(_typeCode);

		int ICollection.Count => __len__();

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		int ICollection<object>.Count => __len__();

		bool ICollection<object>.IsReadOnly => false;

		public array([BytesConversion] string type, [Optional] object initializer)
		{
			if (type == null || type.Length != 1)
			{
				throw PythonOps.TypeError("expected character, got {0}", PythonTypeOps.GetName(type));
			}
			_typeCode = type[0];
			_data = CreateData(_typeCode);
			if (initializer != Missing.Value)
			{
				extend(initializer);
			}
		}

		private array(char typeCode, ArrayData data)
		{
			_typeCode = typeCode;
			_data = data;
		}

		private static ArrayData CreateData(char typecode)
		{
			switch (typecode)
			{
			case 'c':
				return new ArrayData<char>();
			case 'b':
				return new ArrayData<sbyte>();
			case 'B':
				return new ArrayData<byte>();
			case 'u':
				return new ArrayData<char>();
			case 'h':
				return new ArrayData<short>();
			case 'H':
				return new ArrayData<ushort>();
			case 'i':
			case 'l':
				return new ArrayData<int>();
			case 'I':
			case 'L':
				return new ArrayData<uint>();
			case 'f':
				return new ArrayData<float>();
			case 'd':
				return new ArrayData<double>();
			default:
				throw PythonOps.ValueError("Bad type code (expected one of 'c', 'b', 'B', 'u', 'H', 'h', 'i', 'I', 'l', 'L', 'f', 'd')");
			}
		}

		[SpecialName]
		public array InPlaceAdd(array other)
		{
			if (typecode != other.typecode)
			{
				throw PythonOps.TypeError("cannot add different typecodes");
			}
			if (other._data.Length != 0)
			{
				extend(other);
			}
			return this;
		}

		public static array operator +(array self, array other)
		{
			if (self.typecode != other.typecode)
			{
				throw PythonOps.TypeError("cannot add different typecodes");
			}
			array array2 = new array(self.typecode, Missing.Value);
			foreach (object item in (IEnumerable<object>)self)
			{
				array2.append(item);
			}
			foreach (object item2 in (IEnumerable<object>)other)
			{
				array2.append(item2);
			}
			return array2;
		}

		[SpecialName]
		public array InPlaceMultiply(int value)
		{
			if (value <= 0)
			{
				_data.Clear();
			}
			else
			{
				List iterable = tolist();
				for (int i = 0; i < value - 1; i++)
				{
					extend(iterable);
				}
			}
			return this;
		}

		public static array operator *(array array, int value)
		{
			if ((BigInteger)value * (BigInteger)array.__len__() * array.itemsize > 2147483647L)
			{
				throw PythonOps.MemoryError("");
			}
			if (value <= 0)
			{
				return new array(array.typecode, Missing.Value);
			}
			return new array(array._typeCode, array._data.Multiply(value));
		}

		public static array operator *(array array, BigInteger value)
		{
			if (!value.AsInt32(out var ret))
			{
				throw PythonOps.OverflowError("cannot fit 'long' into an index-sized integer");
			}
			if (value * array.__len__() * array.itemsize > 2147483647L)
			{
				throw PythonOps.MemoryError("");
			}
			return array * ret;
		}

		public static array operator *(int value, array array)
		{
			return array * value;
		}

		public static array operator *(BigInteger value, array array)
		{
			return array * value;
		}

		public void append(object iterable)
		{
			_data.Append(iterable);
		}

		internal IntPtr GetArrayAddress()
		{
			return _data.GetAddress();
		}

		public PythonTuple buffer_info()
		{
			return PythonTuple.MakeTuple(_data.GetAddress().ToPython(), _data.Length);
		}

		public void byteswap()
		{
			Stream stream = ToStream();
			byte[] array2 = new byte[stream.Length];
			stream.Read(array2, 0, array2.Length);
			byte[] array3 = new byte[itemsize];
			for (int i = 0; i < array2.Length; i += itemsize)
			{
				for (int j = 0; j < itemsize; j++)
				{
					array3[j] = array2[i + j];
				}
				for (int k = 0; k < itemsize; k++)
				{
					array2[i + k] = array3[itemsize - (k + 1)];
				}
			}
			_data.Clear();
			MemoryStream ms = new MemoryStream(array2);
			FromStream(ms);
		}

		public int count(object x)
		{
			if (x == null)
			{
				return 0;
			}
			return _data.Count(x);
		}

		public void extend(object iterable)
		{
			if (iterable is array array2 && typecode != array2.typecode)
			{
				throw PythonOps.TypeError("cannot extend with different typecode");
			}
			if (iterable is string s && _typeCode != 'u')
			{
				fromstring(s);
				return;
			}
			if (iterable is Bytes b)
			{
				fromstring(b);
				return;
			}
			if (iterable is PythonBuffer buf)
			{
				fromstring(buf);
				return;
			}
			IEnumerator enumerator = PythonOps.GetEnumerator(iterable);
			while (enumerator.MoveNext())
			{
				append(enumerator.Current);
			}
		}

		public void fromlist(object iterable)
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(iterable);
			List<object> list = new List<object>();
			while (enumerator.MoveNext())
			{
				if (!_data.CanStore(enumerator.Current))
				{
					throw PythonOps.TypeError("expected {0}, got {1}", DynamicHelpers.GetPythonTypeFromType(_data.StorageType).Name, DynamicHelpers.GetPythonType(enumerator.Current).Name);
				}
				list.Add(enumerator.Current);
			}
			extend(list);
		}

		public void fromfile(PythonFile f, int n)
		{
			int num = n * itemsize;
			string text = f.read(num);
			if (text.Length < num)
			{
				throw PythonOps.EofError("file not large enough");
			}
			fromstring(text);
		}

		public void fromstring([NotNull] Bytes b)
		{
			if (b.Count % itemsize != 0)
			{
				throw PythonOps.ValueError("string length not a multiple of itemsize");
			}
			FromStream(new MemoryStream(b._bytes, writable: false));
		}

		public void fromstring([NotNull] string s)
		{
			if (s.Length % itemsize != 0)
			{
				throw PythonOps.ValueError("string length not a multiple of itemsize");
			}
			byte[] array2 = new byte[s.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = checked((byte)s[i]);
			}
			MemoryStream ms = new MemoryStream(array2);
			FromStream(ms);
		}

		public void fromstring([NotNull] PythonBuffer buf)
		{
			if (buf.Size % itemsize != 0)
			{
				throw PythonOps.ValueError("string length not a multiple of itemsize");
			}
			FromStream(new MemoryStream(buf.byteCache, writable: false));
		}

		public void fromunicode(CodeContext context, string s)
		{
			if (s == null)
			{
				throw PythonOps.TypeError("expected string");
			}
			if (_typeCode != 'u')
			{
				throw PythonOps.ValueError("fromunicode() may only be called on type 'u' arrays");
			}
			ArrayData<char> arrayData = (ArrayData<char>)_data;
			arrayData.EnsureSize(arrayData.Length + s.Length);
			for (int i = 0; i < s.Length; i++)
			{
				arrayData.Data[i + arrayData.Length] = s[i];
			}
			arrayData.Length += s.Length;
		}

		public int index(object x)
		{
			if (x == null)
			{
				throw PythonOps.ValueError("got None, expected value");
			}
			int num = _data.Index(x);
			if (num == -1)
			{
				throw PythonOps.ValueError("x not found");
			}
			return num;
		}

		public void insert(int i, object x)
		{
			if (i > _data.Length)
			{
				i = _data.Length;
			}
			if (i < 0)
			{
				i = _data.Length + i;
			}
			if (i < 0)
			{
				i = 0;
			}
			_data.Insert(i, x);
		}

		public object pop()
		{
			return pop(-1);
		}

		public object pop(int i)
		{
			i = PythonOps.FixIndex(i, _data.Length);
			object data = _data.GetData(i);
			_data.RemoveAt(i);
			return data;
		}

		public void read(PythonFile f, int n)
		{
			fromfile(f, n);
		}

		public void remove(object value)
		{
			if (value == null)
			{
				throw PythonOps.ValueError("got None, expected value");
			}
			_data.Remove(value);
		}

		public void reverse()
		{
			for (int i = 0; i < _data.Length / 2; i++)
			{
				int x = i;
				int y = _data.Length - (i + 1);
				_data.Swap(x, y);
			}
		}

		internal byte[] RawGetItem(int index)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			switch (_typeCode)
			{
			case 'c':
				binaryWriter.Write((byte)(char)_data.GetData(index));
				break;
			case 'b':
				binaryWriter.Write((sbyte)_data.GetData(index));
				break;
			case 'B':
				binaryWriter.Write((byte)_data.GetData(index));
				break;
			case 'u':
				binaryWriter.Write((char)_data.GetData(index));
				break;
			case 'h':
				binaryWriter.Write((short)_data.GetData(index));
				break;
			case 'H':
				binaryWriter.Write((ushort)_data.GetData(index));
				break;
			case 'i':
			case 'l':
				binaryWriter.Write((int)_data.GetData(index));
				break;
			case 'I':
			case 'L':
				binaryWriter.Write((uint)_data.GetData(index));
				break;
			case 'f':
				binaryWriter.Write((float)_data.GetData(index));
				break;
			case 'd':
				binaryWriter.Write((double)_data.GetData(index));
				break;
			}
			return memoryStream.ToArray();
		}

		public void __delitem__(int index)
		{
			_data.RemoveAt(PythonOps.FixIndex(index, _data.Length));
		}

		public void __delitem__(Slice slice)
		{
			if (slice == null)
			{
				throw PythonOps.TypeError("expected Slice, got None");
			}
			slice.indices(_data.Length, out var ostart, out var ostop, out var ostep);
			if ((ostep > 0 && ostart >= ostop) || (ostep < 0 && ostart <= ostop))
			{
				return;
			}
			if (ostep == 1)
			{
				int num = ostart;
				int num2 = ostop;
				while (num2 < _data.Length)
				{
					_data.SetData(num, _data.GetData(num2));
					num2++;
					num++;
				}
				for (num = 0; num < ostop - ostart; num++)
				{
					_data.RemoveAt(_data.Length - 1);
				}
				return;
			}
			if (ostep == -1)
			{
				int num3 = ostop + 1;
				int num4 = ostart + 1;
				while (num4 < _data.Length)
				{
					_data.SetData(num3, _data.GetData(num4));
					num4++;
					num3++;
				}
				for (num3 = 0; num3 < ostop - ostart; num3++)
				{
					_data.RemoveAt(_data.Length - 1);
				}
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
					_data.SetData(num5++, _data.GetData(num7));
				}
				else
				{
					num6 += ostep;
				}
				num7++;
			}
			while (ostop < _data.Length)
			{
				_data.SetData(num5++, _data.GetData(ostop++));
			}
			while (_data.Length > num5)
			{
				_data.RemoveAt(_data.Length - 1);
			}
		}

		private void CheckSliceAssignType(object value)
		{
			if (!(value is array array2))
			{
				throw PythonOps.TypeError("can only assign array (not \"{0}\") to array slice", PythonTypeOps.GetName(value));
			}
			if (array2 != null && array2._typeCode != _typeCode)
			{
				throw PythonOps.TypeError("bad argument type for built-in operation");
			}
		}

		private void SliceNoStep(object value, int start, int stop)
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(value);
			ArrayData arrayData = CreateData(_typeCode);
			for (int i = 0; i < start; i++)
			{
				arrayData.Append(_data.GetData(i));
			}
			while (enumerator.MoveNext())
			{
				arrayData.Append(enumerator.Current);
			}
			for (int j = Math.Max(stop, start); j < _data.Length; j++)
			{
				arrayData.Append(_data.GetData(j));
			}
			_data = arrayData;
		}

		public object __getslice__(object start, object stop)
		{
			return this[new Slice(start, stop)];
		}

		public void __setslice__(int start, int stop, object value)
		{
			CheckSliceAssignType(value);
			Slice.FixSliceArguments(_data.Length, ref start, ref stop);
			SliceNoStep(value, start, stop);
		}

		public void __delslice__(object start, object stop)
		{
			__delitem__(new Slice(start, stop));
		}

		public PythonTuple __reduce__()
		{
			return PythonOps.MakeTuple(DynamicHelpers.GetPythonType(this), PythonOps.MakeTuple(typecode, ToByteArray().MakeString()), null);
		}

		public array __copy__()
		{
			return new array(typecode, this);
		}

		public array __deepcopy__()
		{
			return __copy__();
		}

		public PythonTuple __reduce_ex__(int version)
		{
			return __reduce__();
		}

		public PythonTuple __reduce_ex__()
		{
			return __reduce__();
		}

		private void SliceAssign(int index, object value)
		{
			_data.SetData(index, value);
		}

		public void tofile(PythonFile f)
		{
			f.write(tostring());
		}

		public List tolist()
		{
			List list = new List();
			for (int i = 0; i < _data.Length; i++)
			{
				list.AddNoLock(this[i]);
			}
			return list;
		}

		public string tostring()
		{
			Stream stream = ToStream();
			byte[] array2 = new byte[stream.Length];
			stream.Read(array2, 0, (int)stream.Length);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array2.Length; i++)
			{
				stringBuilder.Append((char)array2[i]);
			}
			return stringBuilder.ToString();
		}

		public string tounicode(CodeContext context)
		{
			if (_typeCode != 'u')
			{
				throw PythonOps.ValueError("only 'u' arrays can be converted to unicode");
			}
			return new string(((ArrayData<char>)_data).Data, 0, _data.Length);
		}

		public void write(PythonFile f)
		{
			tofile(f);
		}

		internal MemoryStream ToStream()
		{
			MemoryStream memoryStream = new MemoryStream();
			ToStream(memoryStream);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}

		internal void ToStream(Stream ms)
		{
			BinaryWriter binaryWriter = new BinaryWriter(ms, Encoding.Unicode);
			for (int i = 0; i < _data.Length; i++)
			{
				switch (_typeCode)
				{
				case 'c':
					binaryWriter.Write((byte)(char)_data.GetData(i));
					break;
				case 'b':
					binaryWriter.Write((sbyte)_data.GetData(i));
					break;
				case 'B':
					binaryWriter.Write((byte)_data.GetData(i));
					break;
				case 'u':
					binaryWriter.Write((char)_data.GetData(i));
					break;
				case 'h':
					binaryWriter.Write((short)_data.GetData(i));
					break;
				case 'H':
					binaryWriter.Write((ushort)_data.GetData(i));
					break;
				case 'i':
				case 'l':
					binaryWriter.Write((int)_data.GetData(i));
					break;
				case 'I':
				case 'L':
					binaryWriter.Write((uint)_data.GetData(i));
					break;
				case 'f':
					binaryWriter.Write((float)_data.GetData(i));
					break;
				case 'd':
					binaryWriter.Write((double)_data.GetData(i));
					break;
				}
			}
		}

		internal byte[] ToByteArray()
		{
			return ToStream().ToArray();
		}

		internal void Clear()
		{
			_data = CreateData(_typeCode);
		}

		internal void FromStream(Stream ms)
		{
			BinaryReader binaryReader = new BinaryReader(ms);
			for (int i = 0; i < ms.Length / itemsize; i++)
			{
				object value = _typeCode switch
				{
					'c' => (char)binaryReader.ReadByte(), 
					'b' => (sbyte)binaryReader.ReadByte(), 
					'B' => binaryReader.ReadByte(), 
					'u' => ReadBinaryChar(binaryReader), 
					'h' => binaryReader.ReadInt16(), 
					'H' => binaryReader.ReadUInt16(), 
					'i' => binaryReader.ReadInt32(), 
					'I' => binaryReader.ReadUInt32(), 
					'l' => binaryReader.ReadInt32(), 
					'L' => binaryReader.ReadUInt32(), 
					'f' => binaryReader.ReadSingle(), 
					'd' => binaryReader.ReadDouble(), 
					_ => throw new InvalidOperationException(), 
				};
				_data.Append(value);
			}
		}

		internal void FromStream(Stream ms, int index)
		{
			BinaryReader binaryReader = new BinaryReader(ms);
			for (int i = index; i < ms.Length / itemsize + index; i++)
			{
				object value = _typeCode switch
				{
					'c' => (char)binaryReader.ReadByte(), 
					'b' => (sbyte)binaryReader.ReadByte(), 
					'B' => binaryReader.ReadByte(), 
					'u' => ReadBinaryChar(binaryReader), 
					'h' => binaryReader.ReadInt16(), 
					'H' => binaryReader.ReadUInt16(), 
					'i' => binaryReader.ReadInt32(), 
					'I' => binaryReader.ReadUInt32(), 
					'l' => binaryReader.ReadInt32(), 
					'L' => binaryReader.ReadUInt32(), 
					'f' => binaryReader.ReadSingle(), 
					'd' => binaryReader.ReadDouble(), 
					_ => throw new InvalidOperationException(), 
				};
				_data.SetData(i, value);
			}
		}

		internal long FromStream(Stream ms, int index, int nbytes)
		{
			BinaryReader binaryReader = new BinaryReader(ms);
			if (nbytes <= 0)
			{
				return 0L;
			}
			int num = Math.Min((int)(ms.Length - ms.Position), nbytes);
			for (int i = index; i < num / itemsize + index; i++)
			{
				object value = _typeCode switch
				{
					'c' => (char)binaryReader.ReadByte(), 
					'b' => (sbyte)binaryReader.ReadByte(), 
					'B' => binaryReader.ReadByte(), 
					'u' => ReadBinaryChar(binaryReader), 
					'h' => binaryReader.ReadInt16(), 
					'H' => binaryReader.ReadUInt16(), 
					'i' => binaryReader.ReadInt32(), 
					'I' => binaryReader.ReadUInt32(), 
					'l' => binaryReader.ReadInt32(), 
					'L' => binaryReader.ReadUInt32(), 
					'f' => binaryReader.ReadSingle(), 
					'd' => binaryReader.ReadDouble(), 
					_ => throw new InvalidOperationException(), 
				};
				_data.SetData(i, value);
			}
			if (num % itemsize > 0)
			{
				byte[] array2 = ToBytes(num / itemsize + index);
				for (int j = 0; j < num % itemsize; j++)
				{
					array2[j] = binaryReader.ReadByte();
				}
				_data.SetData(num / itemsize + index, FromBytes(array2));
			}
			return num;
		}

		private static object ReadBinaryChar(BinaryReader br)
		{
			byte b = br.ReadByte();
			return (char)((br.ReadByte() << 8) | b);
		}

		private byte[] ToBytes(int index)
		{
			switch (_typeCode)
			{
			case 'c':
				return new byte[1] { (byte)(char)_data.GetData(index) };
			case 'b':
				return new byte[1] { (byte)(sbyte)_data.GetData(index) };
			case 'B':
				return new byte[1] { (byte)_data.GetData(index) };
			case 'u':
				return BitConverter.GetBytes((char)_data.GetData(index));
			case 'h':
				return BitConverter.GetBytes((short)_data.GetData(index));
			case 'H':
				return BitConverter.GetBytes((ushort)_data.GetData(index));
			case 'i':
			case 'l':
				return BitConverter.GetBytes((int)_data.GetData(index));
			case 'I':
			case 'L':
				return BitConverter.GetBytes((uint)_data.GetData(index));
			case 'f':
				return BitConverter.GetBytes((float)_data.GetData(index));
			case 'd':
				return BitConverter.GetBytes((double)_data.GetData(index));
			default:
				throw PythonOps.ValueError("Bad type code (expected one of 'c', 'b', 'B', 'u', 'H', 'h', 'i', 'I', 'l', 'L', 'f', 'd')");
			}
		}

		private object FromBytes(byte[] bytes)
		{
			switch (_typeCode)
			{
			case 'c':
				return (char)bytes[0];
			case 'b':
				return (sbyte)bytes[0];
			case 'B':
				return bytes[0];
			case 'u':
				return BitConverter.ToChar(bytes, 0);
			case 'h':
				return BitConverter.ToInt16(bytes, 0);
			case 'H':
				return BitConverter.ToUInt16(bytes, 0);
			case 'i':
			case 'l':
				return BitConverter.ToInt32(bytes, 0);
			case 'I':
			case 'L':
				return BitConverter.ToUInt32(bytes, 0);
			case 'f':
				return BitConverter.ToSingle(bytes, 0);
			case 'd':
				return BitConverter.ToDouble(bytes, 0);
			default:
				throw PythonOps.ValueError("Bad type code (expected one of 'c', 'b', 'B', 'u', 'H', 'h', 'i', 'I', 'l', 'L', 'f', 'd')");
			}
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			IStructuralEquatable structuralEquatable;
			switch (_typeCode)
			{
			case 'c':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<char>)_data).Data);
				break;
			case 'b':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<sbyte>)_data).Data);
				break;
			case 'B':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<byte>)_data).Data);
				break;
			case 'u':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<char>)_data).Data);
				break;
			case 'h':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<short>)_data).Data);
				break;
			case 'H':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<ushort>)_data).Data);
				break;
			case 'i':
			case 'l':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<int>)_data).Data);
				break;
			case 'I':
			case 'L':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<uint>)_data).Data);
				break;
			case 'f':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<float>)_data).Data);
				break;
			case 'd':
				structuralEquatable = PythonTuple.MakeTuple(((ArrayData<double>)_data).Data);
				break;
			default:
				throw PythonOps.ValueError("Bad type code (expected one of 'c', 'b', 'B', 'u', 'H', 'h', 'i', 'I', 'l', 'L', 'f', 'd')");
			}
			return structuralEquatable.GetHashCode(comparer);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is array array2))
			{
				return false;
			}
			if (_data.Length != array2._data.Length)
			{
				return false;
			}
			for (int i = 0; i < _data.Length; i++)
			{
				if (!comparer.Equals(_data.GetData(i), array2._data.GetData(i)))
				{
					return false;
				}
			}
			return true;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < _data.Length; i++)
			{
				yield return _data.GetData(i);
			}
		}

		public virtual string __repr__(CodeContext context)
		{
			string text = "array('" + typecode.ToString() + "'";
			if (_data.Length == 0)
			{
				return text + ")";
			}
			StringBuilder stringBuilder = new StringBuilder(text);
			if (_typeCode == 'c' || _typeCode == 'u')
			{
				char c = '\'';
				string text2 = new string(((ArrayData<char>)_data).Data, 0, _data.Length);
				if (text2.IndexOf('\'') != -1 && text2.IndexOf('"') == -1)
				{
					c = '"';
				}
				if (_typeCode == 'u')
				{
					stringBuilder.Append(", u");
				}
				else
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(c);
				bool isUnicode = false;
				stringBuilder.Append(StringOps.ReprEncode(text2, c, ref isUnicode));
				stringBuilder.Append(c);
				stringBuilder.Append(")");
			}
			else
			{
				stringBuilder.Append(", [");
				for (int i = 0; i < _data.Length; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(PythonOps.Repr(context, this[i]));
				}
				stringBuilder.Append("])");
			}
			return stringBuilder.ToString();
		}

		WeakRefTracker IWeakReferenceable.GetWeakRef()
		{
			return _tracker;
		}

		bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
		{
			_tracker = value;
			return true;
		}

		void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
		{
			_tracker = value;
		}

		public int __len__()
		{
			return _data.Length;
		}

		public bool __contains__(object value)
		{
			return _data.Index(value) != -1;
		}

		private bool TryCompare(object other, out int res)
		{
			if (!(other is array array2) || array2.typecode != typecode)
			{
				res = 0;
				return false;
			}
			if (array2._data.Length != _data.Length)
			{
				res = _data.Length - array2._data.Length;
			}
			else
			{
				res = 0;
				for (int i = 0; i < array2._data.Length; i++)
				{
					if (res != 0)
					{
						break;
					}
					res = PythonOps.Compare(_data.GetData(i), array2._data.GetData(i));
				}
			}
			return true;
		}

		[return: MaybeNotImplemented]
		public static object operator >(array self, object other)
		{
			if (!self.TryCompare(other, out var res))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(res > 0);
		}

		[return: MaybeNotImplemented]
		public static object operator <(array self, object other)
		{
			if (!self.TryCompare(other, out var res))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(res < 0);
		}

		[return: MaybeNotImplemented]
		public static object operator >=(array self, object other)
		{
			if (!self.TryCompare(other, out var res))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(res >= 0);
		}

		[return: MaybeNotImplemented]
		public static object operator <=(array self, object other)
		{
			if (!self.TryCompare(other, out var res))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(res <= 0);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		int IList<object>.IndexOf(object item)
		{
			return _data.Index(item);
		}

		void IList<object>.Insert(int index, object item)
		{
			insert(index, item);
		}

		void IList<object>.RemoveAt(int index)
		{
			__delitem__(index);
		}

		void ICollection<object>.Add(object item)
		{
			append(item);
		}

		void ICollection<object>.Clear()
		{
			__delitem__(new Slice(null, null));
		}

		bool ICollection<object>.Contains(object item)
		{
			return __contains__(item);
		}

		void ICollection<object>.CopyTo(object[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		bool ICollection<object>.Remove(object item)
		{
			try
			{
				remove(item);
				return true;
			}
			catch (ArgumentException)
			{
				return false;
			}
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			for (int i = 0; i < _data.Length; i++)
			{
				yield return _data.GetData(i);
			}
		}
	}

	public const string __doc__ = "Provides arrays for native data types.  These can be used for compact storage or native interop via ctypes";

	public static readonly PythonType ArrayType = DynamicHelpers.GetPythonTypeFromType(typeof(array));
}
