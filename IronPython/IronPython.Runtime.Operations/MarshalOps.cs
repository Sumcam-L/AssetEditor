using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public class MarshalOps
{
	private class MarshalWriter
	{
		private readonly List<byte> _bytes;

		private readonly int _version;

		private readonly Dictionary<string, int> _strings;

		public MarshalWriter(int version)
		{
			_bytes = new List<byte>();
			_version = version;
			if (_version > 0)
			{
				_strings = new Dictionary<string, int>();
			}
		}

		public void WriteObject(object o)
		{
			List<object> reprInfinite = PythonOps.GetReprInfinite();
			if (reprInfinite.Contains(o))
			{
				throw PythonOps.ValueError("Marshaled data contains infinite cycle");
			}
			int count = reprInfinite.Count;
			reprInfinite.Add(o);
			try
			{
				if (o == null)
				{
					_bytes.Add(78);
					return;
				}
				if (o == ScriptingRuntimeHelpers.True)
				{
					_bytes.Add(84);
					return;
				}
				if (o == ScriptingRuntimeHelpers.False)
				{
					_bytes.Add(70);
					return;
				}
				if (o is string)
				{
					WriteString(o as string);
					return;
				}
				if (o is int)
				{
					WriteInt((int)o);
					return;
				}
				if (o is float)
				{
					WriteFloat((float)o);
					return;
				}
				if (o is double)
				{
					WriteFloat((double)o);
					return;
				}
				if (o is long)
				{
					WriteLong((long)o);
					return;
				}
				if (o.GetType() == typeof(List))
				{
					WriteList(o);
					return;
				}
				if (o.GetType() == typeof(PythonDictionary))
				{
					WriteDict(o);
					return;
				}
				if (o.GetType() == typeof(PythonTuple))
				{
					WriteTuple(o);
					return;
				}
				if (o.GetType() == typeof(SetCollection))
				{
					WriteSet(o);
					return;
				}
				if (o.GetType() == typeof(FrozenSetCollection))
				{
					WriteFrozenSet(o);
					return;
				}
				if (o is BigInteger)
				{
					WriteInteger((BigInteger)o);
					return;
				}
				if (o is Complex)
				{
					WriteComplex((Complex)o);
					return;
				}
				if (o is PythonBuffer)
				{
					WriteBuffer((PythonBuffer)o);
					return;
				}
				if (o == PythonExceptions.StopIteration)
				{
					WriteStopIteration();
					return;
				}
				throw PythonOps.ValueError("unmarshallable object");
			}
			finally
			{
				reprInfinite.RemoveAt(count);
			}
		}

		private void WriteFloat(float f)
		{
			if (_version > 1)
			{
				_bytes.Add(103);
				_bytes.AddRange(BitConverter.GetBytes((double)f));
			}
			else
			{
				_bytes.Add(102);
				WriteDoubleString(f);
			}
		}

		private void WriteFloat(double f)
		{
			if (_version > 1)
			{
				_bytes.Add(103);
				_bytes.AddRange(BitConverter.GetBytes(f));
			}
			else
			{
				_bytes.Add(102);
				WriteDoubleString(f);
			}
		}

		private void WriteDoubleString(double d)
		{
			string text = DoubleOps.__repr__(DefaultContext.Default, d);
			_bytes.Add((byte)text.Length);
			for (int i = 0; i < text.Length; i++)
			{
				_bytes.Add((byte)text[i]);
			}
		}

		private void WriteInteger(BigInteger val)
		{
			_bytes.Add(108);
			int num = 0;
			int num2;
			if (val < BigInteger.Zero)
			{
				val *= (BigInteger)(-1);
				num2 = -1;
			}
			else
			{
				num2 = 1;
			}
			List<byte> list = new List<byte>();
			while (val != BigInteger.Zero)
			{
				int num3 = (int)(val & 32767);
				val >>= 15;
				list.Add((byte)(num3 & 0xFF));
				list.Add((byte)((num3 >> 8) & 0xFF));
				num += num2;
			}
			WriteInt32(num);
			_bytes.AddRange(list);
		}

		private void WriteBuffer(PythonBuffer b)
		{
			_bytes.Add(115);
			List<byte> list = new List<byte>();
			for (int i = 0; i < b.Size; i++)
			{
				if (b[i] is string)
				{
					string text = b[i] as string;
					byte[] bytes = Encoding.UTF8.GetBytes(text);
					if (bytes.Length != text.Length)
					{
						list.AddRange(bytes);
						continue;
					}
					byte[] bytes2 = PythonAsciiEncoding.Instance.GetBytes(text);
					list.AddRange(bytes2);
				}
				else
				{
					list.Add((byte)b[i]);
				}
			}
			WriteInt32(list.Count);
			_bytes.AddRange(list);
		}

		private void WriteLong(long l)
		{
			_bytes.Add(73);
			for (int i = 0; i < 8; i++)
			{
				_bytes.Add((byte)(l & 0xFF));
				l >>= 8;
			}
		}

		private void WriteComplex(Complex val)
		{
			_bytes.Add(120);
			WriteDoubleString(val.Real);
			WriteDoubleString(val.Imaginary());
		}

		private void WriteStopIteration()
		{
			_bytes.Add(83);
		}

		private void WriteInt(int val)
		{
			_bytes.Add(105);
			WriteInt32(val);
		}

		private void WriteInt32(int val)
		{
			_bytes.Add((byte)(val & 0xFF));
			_bytes.Add((byte)((val >> 8) & 0xFF));
			_bytes.Add((byte)((val >> 16) & 0xFF));
			_bytes.Add((byte)((val >> 24) & 0xFF));
		}

		private void WriteString(string s)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			if (bytes.Length != s.Length)
			{
				_bytes.Add(117);
				WriteInt32(bytes.Length);
				for (int i = 0; i < bytes.Length; i++)
				{
					_bytes.Add(bytes[i]);
				}
				return;
			}
			if (_strings != null && _strings.TryGetValue(s, out var value))
			{
				_bytes.Add(82);
				WriteInt32(value);
				return;
			}
			byte[] bytes2 = PythonAsciiEncoding.Instance.GetBytes(s);
			if (_strings != null)
			{
				_bytes.Add(116);
			}
			else
			{
				_bytes.Add(115);
			}
			WriteInt32(bytes2.Length);
			for (int j = 0; j < bytes2.Length; j++)
			{
				_bytes.Add(bytes2[j]);
			}
			if (_strings != null)
			{
				_strings[s] = _strings.Count;
			}
		}

		private void WriteList(object o)
		{
			List list = o as List;
			_bytes.Add(91);
			WriteInt32(list.__len__());
			for (int i = 0; i < list.__len__(); i++)
			{
				WriteObject(list[i]);
			}
		}

		private void WriteDict(object o)
		{
			PythonDictionary pythonDictionary = o as PythonDictionary;
			_bytes.Add(123);
			IEnumerator<KeyValuePair<object, object>> enumerator = ((IEnumerable<KeyValuePair<object, object>>)pythonDictionary).GetEnumerator();
			while (enumerator.MoveNext())
			{
				WriteObject(enumerator.Current.Key);
				WriteObject(enumerator.Current.Value);
			}
			_bytes.Add(48);
		}

		private void WriteTuple(object o)
		{
			PythonTuple pythonTuple = o as PythonTuple;
			_bytes.Add(40);
			WriteInt32(pythonTuple.__len__());
			for (int i = 0; i < pythonTuple.__len__(); i++)
			{
				WriteObject(pythonTuple[i]);
			}
		}

		private void WriteSet(object set)
		{
			SetCollection setCollection = set as SetCollection;
			_bytes.Add(60);
			WriteInt32(setCollection.__len__());
			foreach (object item in (IEnumerable<object>)setCollection)
			{
				WriteObject(item);
			}
		}

		private void WriteFrozenSet(object set)
		{
			FrozenSetCollection frozenSetCollection = set as FrozenSetCollection;
			_bytes.Add(62);
			WriteInt32(frozenSetCollection.__len__());
			foreach (object item in (IEnumerable<object>)frozenSetCollection)
			{
				WriteObject(item);
			}
		}

		public byte[] GetBytes()
		{
			return _bytes.ToArray();
		}
	}

	private class MarshalReader
	{
		private enum StackType
		{
			Tuple,
			Dict,
			List,
			Set,
			FrozenSet
		}

		private class ProcStack
		{
			public StackType StackType;

			public object StackObj;

			public int StackCount;

			public bool HaveKey;

			public object Key;
		}

		private IEnumerator<byte> _myBytes;

		private Stack<ProcStack> _stack;

		private readonly Dictionary<int, string> _strings;

		private object _result;

		public MarshalReader(IEnumerator<byte> bytes)
		{
			_myBytes = bytes;
			_strings = new Dictionary<int, string>();
		}

		public object ReadObject()
		{
			while (_myBytes.MoveNext())
			{
				switch ((char)_myBytes.Current)
				{
				case '(':
					PushStack(StackType.Tuple);
					break;
				case '[':
					PushStack(StackType.List);
					break;
				case '{':
					PushStack(StackType.Dict);
					break;
				case '<':
					PushStack(StackType.Set);
					break;
				case '>':
					PushStack(StackType.FrozenSet);
					break;
				case '0':
					if (_stack == null || _stack.Count == 0)
					{
						throw PythonOps.ValueError("bad marshal data");
					}
					_stack.Peek().StackCount = 0;
					break;
				default:
				{
					object obj = YieldSimple();
					if (_stack == null)
					{
						return obj;
					}
					do
					{
						obj = UpdateStack(obj);
					}
					while (obj != null && _stack.Count > 0);
					if (_stack.Count != 0)
					{
						continue;
					}
					return _result;
				}
				}
				if (_stack != null && _stack.Count > 0 && _stack.Peek().StackCount == 0)
				{
					ProcStack procStack = _stack.Pop();
					object obj = procStack.StackObj;
					if (procStack.StackType == StackType.Tuple)
					{
						obj = PythonTuple.Make(obj);
					}
					else if (procStack.StackType == StackType.FrozenSet)
					{
						obj = FrozenSetCollection.Make(TypeCache.FrozenSet, obj);
					}
					if (_stack.Count <= 0)
					{
						_result = obj;
						break;
					}
					do
					{
						obj = UpdateStack(obj);
					}
					while (obj != null && _stack.Count > 0);
					if (_stack.Count == 0)
					{
						break;
					}
				}
			}
			return _result;
		}

		private void PushStack(StackType type)
		{
			ProcStack procStack = new ProcStack();
			procStack.StackType = type;
			switch (type)
			{
			case StackType.Dict:
				procStack.StackObj = new PythonDictionary();
				procStack.StackCount = -1;
				break;
			case StackType.List:
				procStack.StackObj = new List();
				procStack.StackCount = ReadInt32();
				break;
			case StackType.Tuple:
				procStack.StackCount = ReadInt32();
				procStack.StackObj = new List<object>(procStack.StackCount);
				break;
			case StackType.Set:
				procStack.StackObj = new SetCollection();
				procStack.StackCount = ReadInt32();
				break;
			case StackType.FrozenSet:
				procStack.StackCount = ReadInt32();
				procStack.StackObj = new List<object>(procStack.StackCount);
				break;
			}
			if (_stack == null)
			{
				_stack = new Stack<ProcStack>();
			}
			_stack.Push(procStack);
		}

		private object UpdateStack(object res)
		{
			ProcStack procStack = _stack.Peek();
			switch (procStack.StackType)
			{
			case StackType.Dict:
			{
				PythonDictionary pythonDictionary = procStack.StackObj as PythonDictionary;
				if (procStack.HaveKey)
				{
					pythonDictionary[procStack.Key] = res;
					procStack.HaveKey = false;
				}
				else
				{
					procStack.HaveKey = true;
					procStack.Key = res;
				}
				break;
			}
			case StackType.Tuple:
			{
				List<object> list3 = procStack.StackObj as List<object>;
				list3.Add(res);
				procStack.StackCount--;
				if (procStack.StackCount == 0)
				{
					_stack.Pop();
					object result2 = PythonTuple.Make(list3);
					if (_stack.Count == 0)
					{
						_result = result2;
					}
					return result2;
				}
				break;
			}
			case StackType.List:
			{
				List list2 = procStack.StackObj as List;
				list2.AddNoLock(res);
				procStack.StackCount--;
				if (procStack.StackCount == 0)
				{
					_stack.Pop();
					if (_stack.Count == 0)
					{
						_result = list2;
					}
					return list2;
				}
				break;
			}
			case StackType.Set:
			{
				SetCollection setCollection = procStack.StackObj as SetCollection;
				setCollection.add(res);
				procStack.StackCount--;
				if (procStack.StackCount == 0)
				{
					_stack.Pop();
					if (_stack.Count == 0)
					{
						_result = setCollection;
					}
					return setCollection;
				}
				break;
			}
			case StackType.FrozenSet:
			{
				List<object> list = procStack.StackObj as List<object>;
				list.Add(res);
				procStack.StackCount--;
				if (procStack.StackCount == 0)
				{
					_stack.Pop();
					object result = FrozenSetCollection.Make(TypeCache.FrozenSet, list);
					if (_stack.Count == 0)
					{
						_result = result;
					}
					return result;
				}
				break;
			}
			}
			return null;
		}

		private object YieldSimple()
		{
			return (char)_myBytes.Current switch
			{
				'i' => ReadInt(), 
				'l' => ReadBigInteger(), 
				'T' => ScriptingRuntimeHelpers.True, 
				'F' => ScriptingRuntimeHelpers.False, 
				'f' => ReadFloat(), 
				't' => ReadAsciiString(), 
				'u' => ReadUnicodeString(), 
				'S' => PythonExceptions.StopIteration, 
				'N' => null, 
				'x' => ReadComplex(), 
				's' => ReadBuffer(), 
				'I' => ReadLong(), 
				'R' => _strings[ReadInt32()], 
				'g' => ReadBinaryFloat(), 
				_ => throw PythonOps.ValueError("bad marshal data"), 
			};
		}

		private byte[] ReadBytes(int len)
		{
			byte[] array = new byte[len];
			for (int i = 0; i < len; i++)
			{
				if (!_myBytes.MoveNext())
				{
					throw PythonOps.ValueError("bad marshal data");
				}
				array[i] = _myBytes.Current;
			}
			return array;
		}

		private int ReadInt32()
		{
			byte[] array = ReadBytes(4);
			return array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24);
		}

		private double ReadFloatStr()
		{
			MoveNext();
			string s = DecodeString(PythonAsciiEncoding.Instance, ReadBytes(_myBytes.Current));
			double result = 0.0;
			if (double.TryParse(s, out result))
			{
				return result;
			}
			return 0.0;
		}

		private void MoveNext()
		{
			if (!_myBytes.MoveNext())
			{
				throw PythonOps.EofError("EOF read where object expected");
			}
		}

		private string DecodeString(Encoding enc, byte[] bytes)
		{
			return enc.GetString(bytes, 0, bytes.Length);
		}

		private object ReadInt()
		{
			byte b = ReadIntPart();
			byte b2 = ReadIntPart();
			byte b3 = ReadIntPart();
			byte b4 = ReadIntPart();
			byte[] value = new byte[4] { b, b2, b3, b4 };
			return ScriptingRuntimeHelpers.Int32ToObject(BitConverter.ToInt32(value, 0));
		}

		private byte ReadIntPart()
		{
			if (_myBytes.MoveNext())
			{
				return _myBytes.Current;
			}
			return byte.MaxValue;
		}

		private object ReadFloat()
		{
			return ReadFloatStr();
		}

		private object ReadBinaryFloat()
		{
			return BitConverter.ToDouble(ReadBytes(8), 0);
		}

		private object ReadAsciiString()
		{
			string text = DecodeString(PythonAsciiEncoding.Instance, ReadBytes(ReadInt32()));
			_strings[_strings.Count] = text;
			return text;
		}

		private object ReadUnicodeString()
		{
			return DecodeString(Encoding.UTF8, ReadBytes(ReadInt32()));
		}

		private object ReadComplex()
		{
			double real = ReadFloatStr();
			double imaginary = ReadFloatStr();
			return new Complex(real, imaginary);
		}

		private object ReadBuffer()
		{
			return DecodeString(Encoding.UTF8, ReadBytes(ReadInt32()));
		}

		private object ReadLong()
		{
			byte[] array = ReadBytes(8);
			long num = 0L;
			for (int i = 0; i < 8; i++)
			{
				num |= (long)((ulong)array[i] << i * 8);
			}
			return num;
		}

		private object ReadBigInteger()
		{
			int num = ReadInt32();
			if (num == 0)
			{
				return BigInteger.Zero;
			}
			int num2 = 1;
			if (num < 0)
			{
				num2 = -1;
				num *= -1;
			}
			int num3 = num * 2;
			byte[] array = ReadBytes(num3);
			byte[] array2 = new byte[array.Length];
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			while (num4 < num3)
			{
				if (num6 == 0)
				{
					array2[num5] = array[num4];
				}
				else
				{
					array2[num5] = (byte)((array[num4] >> num6) | (array[num4 + 1] << 8 - num6));
				}
				num5++;
				num4++;
				if (num6 == 7)
				{
					num6 = 0;
				}
				else
				{
					if (num4 < num3 - 1)
					{
						array2[num5] = (byte)((array[num4] >> num6) | (array[num4 + 1] << 7 - num6));
					}
					else
					{
						array2[num5] = (byte)(array[num4] >> num6);
					}
					num5++;
					num6++;
				}
				num4++;
			}
			BigInteger bigInteger = new BigInteger(array2);
			return (num2 < 0) ? (-bigInteger) : bigInteger;
		}
	}

	public static byte[] GetBytes(object o, int version)
	{
		MarshalWriter marshalWriter = new MarshalWriter(version);
		marshalWriter.WriteObject(o);
		return marshalWriter.GetBytes();
	}

	public static object GetObject(IEnumerator<byte> bytes)
	{
		MarshalReader marshalReader = new MarshalReader(bytes);
		return marshalReader.ReadObject();
	}
}
