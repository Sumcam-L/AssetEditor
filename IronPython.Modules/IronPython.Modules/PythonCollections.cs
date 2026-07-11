using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronPython.Modules;

public class PythonCollections
{
	[DebuggerDisplay("deque, {__len__()} items")]
	[DontMapIEnumerableToContains]
	[PythonType]
	[DebuggerTypeProxy(typeof(CollectionDebugProxy))]
	public class deque : IComparable, ICodeFormattable, IStructuralEquatable, IStructuralComparable, ICollection, IEnumerable, IReversible
	{
		[PythonType("deque_iterator")]
		private sealed class DequeIterator : IEnumerable, IEnumerator
		{
			private readonly deque _deque;

			private int _curIndex;

			private int _moveCnt;

			private int _version;

			object IEnumerator.Current => _deque._data[_curIndex];

			public DequeIterator(deque d)
			{
				lock (d._lockObj)
				{
					_deque = d;
					_curIndex = d._head - 1;
					_version = d._version;
				}
			}

			bool IEnumerator.MoveNext()
			{
				lock (_deque._lockObj)
				{
					if (_version != _deque._version)
					{
						throw PythonOps.RuntimeError("deque mutated during iteration");
					}
					if (_moveCnt < _deque._itemCnt)
					{
						_curIndex++;
						_moveCnt++;
						if (_curIndex == _deque._data.Length)
						{
							_curIndex = 0;
						}
						return true;
					}
					return false;
				}
			}

			void IEnumerator.Reset()
			{
				_moveCnt = 0;
				_curIndex = _deque._head - 1;
			}

			public IEnumerator GetEnumerator()
			{
				return this;
			}
		}

		[PythonType]
		private class deque_reverse_iterator : IEnumerator
		{
			private readonly deque _deque;

			private int _curIndex;

			private int _moveCnt;

			private int _version;

			object IEnumerator.Current => _deque._data[_curIndex];

			public deque_reverse_iterator(deque d)
			{
				lock (d._lockObj)
				{
					_deque = d;
					_curIndex = d._tail;
					_version = d._version;
				}
			}

			bool IEnumerator.MoveNext()
			{
				lock (_deque._lockObj)
				{
					if (_version != _deque._version)
					{
						throw PythonOps.RuntimeError("deque mutated during iteration");
					}
					if (_moveCnt < _deque._itemCnt)
					{
						_curIndex--;
						_moveCnt++;
						if (_curIndex < 0)
						{
							_curIndex = _deque._data.Length - 1;
						}
						return true;
					}
					return false;
				}
			}

			void IEnumerator.Reset()
			{
				_moveCnt = 0;
				_curIndex = _deque._tail;
			}
		}

		private delegate bool DequeWalker(int curIndex);

		public const object __hash__ = null;

		private object[] _data;

		private object _lockObj = new object();

		private int _head;

		private int _tail;

		private int _itemCnt;

		private int _maxLen;

		private int _version;

		public object this[CodeContext context, object index]
		{
			get
			{
				lock (_lockObj)
				{
					return _data[IndexToSlot(context, index)];
				}
			}
			set
			{
				lock (_lockObj)
				{
					_version++;
					_data[IndexToSlot(context, index)] = value;
				}
			}
		}

		int ICollection.Count => _itemCnt;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		public deque()
		{
			_maxLen = -1;
			clear();
		}

		public deque(object iterable)
			: this()
		{
		}

		public deque(object iterable, object maxLen)
			: this()
		{
		}

		public deque(params object[] args)
			: this()
		{
		}

		public deque([ParamDictionary] IDictionary<object, object> dict, params object[] args)
			: this()
		{
		}

		private deque(int maxLen)
		{
			_maxLen = maxLen;
			clear();
		}

		public void __init__()
		{
			_maxLen = -1;
			clear();
		}

		public void __init__([ParamDictionary] IDictionary<object, object> dict)
		{
			_maxLen = VerifyMaxLen(dict);
			clear();
		}

		public void __init__(object iterable)
		{
			_maxLen = -1;
			clear();
			extend(iterable);
		}

		public void __init__(object iterable, object maxLen)
		{
			_maxLen = VerifyMaxLenValue(maxLen);
			clear();
			extend(iterable);
		}

		public void __init__(object iterable, [ParamDictionary] IDictionary<object, object> dict)
		{
			if (VerifyMaxLen(dict) < 0)
			{
				__init__(iterable);
			}
			else
			{
				__init__(iterable, VerifyMaxLen(dict));
			}
		}

		private static int VerifyMaxLen(IDictionary<object, object> dict)
		{
			if (dict.Count != 1)
			{
				throw PythonOps.TypeError("deque() takes at most 1 keyword argument ({0} given)", dict.Count);
			}
			if (!dict.TryGetValue("maxlen", out var value))
			{
				IEnumerator<object> enumerator = dict.Keys.GetEnumerator();
				if (enumerator.MoveNext())
				{
					throw PythonOps.TypeError("deque(): '{0}' is an invalid keyword argument", enumerator.Current);
				}
			}
			return VerifyMaxLenValue(value);
		}

		private static int VerifyMaxLenValue(object value)
		{
			if (value == null)
			{
				return -1;
			}
			if (value is int || value is BigInteger || value is double)
			{
				int num = (int)value;
				if (num < 0)
				{
					throw PythonOps.ValueError("maxlen must be non-negative");
				}
				return num;
			}
			if (value is Extensible<int>)
			{
				int value2 = ((Extensible<int>)value).Value;
				if (value2 < 0)
				{
					throw PythonOps.ValueError("maxlen must be non-negative");
				}
				return value2;
			}
			throw PythonOps.TypeError("deque(): keyword argument 'maxlen' requires integer");
		}

		public void append(object x)
		{
			lock (_lockObj)
			{
				_version++;
				if (_itemCnt == _maxLen)
				{
					_data[_tail++] = x;
					if (_tail == _data.Length)
					{
						_tail = 0;
					}
					_head = _tail;
					return;
				}
				if (_itemCnt == _data.Length)
				{
					GrowArray();
				}
				_itemCnt++;
				_data[_tail++] = x;
				if (_tail == _data.Length)
				{
					_tail = 0;
				}
			}
		}

		public void appendleft(object x)
		{
			lock (_lockObj)
			{
				_version++;
				if (_itemCnt == _maxLen)
				{
					_head--;
					if (_head < 0)
					{
						_head = _data.Length - 1;
					}
					_tail = _head;
					_data[_head] = x;
					return;
				}
				if (_itemCnt == _data.Length)
				{
					GrowArray();
				}
				_itemCnt++;
				_head--;
				if (_head < 0)
				{
					_head = _data.Length - 1;
				}
				_data[_head] = x;
			}
		}

		public void clear()
		{
			lock (_lockObj)
			{
				_version++;
				_head = (_tail = 0);
				_itemCnt = 0;
				if (_maxLen < 0)
				{
					_data = new object[8];
				}
				else
				{
					_data = new object[Math.Min(_maxLen, 8)];
				}
			}
		}

		public void extend(object iterable)
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(iterable);
			while (enumerator.MoveNext())
			{
				append(enumerator.Current);
			}
		}

		public void extendleft(object iterable)
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(iterable);
			while (enumerator.MoveNext())
			{
				appendleft(enumerator.Current);
			}
		}

		public object pop()
		{
			lock (_lockObj)
			{
				if (_itemCnt == 0)
				{
					throw PythonOps.IndexError("pop from an empty deque");
				}
				_version++;
				if (_tail != 0)
				{
					_tail--;
				}
				else
				{
					_tail = _data.Length - 1;
				}
				_itemCnt--;
				object result = _data[_tail];
				_data[_tail] = null;
				return result;
			}
		}

		public object popleft()
		{
			lock (_lockObj)
			{
				if (_itemCnt == 0)
				{
					throw PythonOps.IndexError("pop from an empty deque");
				}
				_version++;
				object result = _data[_head];
				_data[_head] = null;
				if (_head != _data.Length - 1)
				{
					_head++;
				}
				else
				{
					_head = 0;
				}
				_itemCnt--;
				return result;
			}
		}

		public void remove(object value)
		{
			bool lockTaken = false;
			object lockObj = default(object);
			try
			{
				Monitor.Enter(lockObj = _lockObj, ref lockTaken);
				int found = -1;
				int version = _version;
				WalkDeque(delegate(int index)
				{
					if (PythonOps.EqualRetBool(_data[index], value))
					{
						found = index;
						return false;
					}
					return true;
				});
				if (_version != version)
				{
					throw PythonOps.IndexError("deque mutated during remove().");
				}
				if (found == _head)
				{
					popleft();
					return;
				}
				if (found == ((_tail > 0) ? (_tail - 1) : (_data.Length - 1)))
				{
					pop();
					return;
				}
				if (found == -1)
				{
					throw PythonOps.ValueError("deque.remove(value): value not in deque");
				}
				_version++;
				int num = ((_head < _tail) ? _head : 0);
				bool flag = false;
				object obj = ((_tail != 0) ? _data[_tail - 1] : _data[_data.Length - 1]);
				for (int num2 = _tail - 2; num2 >= num; num2--)
				{
					object obj2 = _data[num2];
					_data[num2] = obj;
					if (num2 == found)
					{
						flag = true;
						break;
					}
					obj = obj2;
				}
				if (_head >= _tail && !flag)
				{
					for (int num3 = _data.Length - 1; num3 >= _head; num3--)
					{
						object obj3 = _data[num3];
						_data[num3] = obj;
						if (num3 == found)
						{
							break;
						}
						obj = obj3;
					}
				}
				_tail--;
				_itemCnt--;
				if (_tail < 0)
				{
					_tail = _data.Length - 1;
				}
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(lockObj);
				}
			}
		}

		public void rotate(CodeContext context)
		{
			rotate(context, 1);
		}

		public void rotate(CodeContext context, object n)
		{
			lock (_lockObj)
			{
				if (_itemCnt == 0)
				{
					return;
				}
				int num = PythonContext.GetContext(context).ConvertToInt32(n) % _itemCnt;
				num %= _itemCnt;
				if (num == 0)
				{
					return;
				}
				if (num < 0)
				{
					num += _itemCnt;
				}
				_version++;
				if (_itemCnt == _data.Length)
				{
					_head = (_tail = (_tail - num + _data.Length) % _data.Length);
					return;
				}
				object[] newData = new object[_itemCnt];
				int curWriteIndex = num;
				WalkDeque(delegate(int curIndex)
				{
					newData[curWriteIndex] = _data[curIndex];
					curWriteIndex = (curWriteIndex + 1) % _itemCnt;
					return true;
				});
				_head = (_tail = 0);
				_data = newData;
			}
		}

		public object __copy__(CodeContext context)
		{
			if (GetType() == typeof(deque))
			{
				deque deque2 = new deque(_maxLen);
				deque2.extend(((IEnumerable)this).GetEnumerator());
				return deque2;
			}
			return PythonCalls.Call(context, DynamicHelpers.GetPythonType(this), ((IEnumerable)this).GetEnumerator());
		}

		public void __delitem__(CodeContext context, object index)
		{
			bool lockTaken = false;
			object lockObj = default(object);
			try
			{
				Monitor.Enter(lockObj = _lockObj, ref lockTaken);
				int realIndex = IndexToSlot(context, index);
				_version++;
				if (realIndex == _head)
				{
					popleft();
					return;
				}
				if (realIndex == _tail - 1 || (realIndex == _data.Length - 1 && _tail == _data.Length))
				{
					pop();
					return;
				}
				object[] newData = new object[_data.Length];
				int writeIndex = 0;
				WalkDeque(delegate(int curIndex)
				{
					if (curIndex != realIndex)
					{
						newData[writeIndex++] = _data[curIndex];
					}
					return true;
				});
				_head = 0;
				_tail = writeIndex;
				_data = newData;
				_itemCnt--;
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(lockObj);
				}
			}
		}

		public PythonTuple __reduce__()
		{
			bool lockTaken = false;
			object lockObj = default(object);
			try
			{
				Monitor.Enter(lockObj = _lockObj, ref lockTaken);
				object[] items = new object[_itemCnt];
				int curItem = 0;
				WalkDeque(delegate(int curIndex)
				{
					items[curItem++] = _data[curIndex];
					return true;
				});
				return PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(GetType()), PythonTuple.MakeTuple(List.FromArrayNoCopy(items)), null);
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(lockObj);
				}
			}
		}

		public int __len__()
		{
			return _itemCnt;
		}

		int IComparable.CompareTo(object obj)
		{
			if (!(obj is deque otherDeque))
			{
				throw new ValueErrorException("expected deque");
			}
			return CompareToWorker(otherDeque);
		}

		private int CompareToWorker(deque otherDeque)
		{
			return CompareToWorker(otherDeque, null);
		}

		private int CompareToWorker(deque otherDeque, IComparer comparer)
		{
			if (otherDeque._itemCnt == 0 && _itemCnt == 0)
			{
				return 0;
			}
			if (CompareUtil.Check(this))
			{
				return 0;
			}
			CompareUtil.Push(this);
			try
			{
				int num = otherDeque._head;
				int num2 = _head;
				do
				{
					int num3 = comparer?.Compare(_data[num2], otherDeque._data[num]) ?? PythonOps.Compare(_data[num2], otherDeque._data[num]);
					if (num3 != 0)
					{
						return num3;
					}
					num++;
					if (num == otherDeque._data.Length)
					{
						num = 0;
					}
					if (num == otherDeque._tail)
					{
						break;
					}
					num2++;
					if (num2 == _data.Length)
					{
						num2 = 0;
					}
				}
				while (num2 != _tail);
				if (otherDeque._itemCnt == _itemCnt)
				{
					return 0;
				}
				return (_itemCnt > otherDeque._itemCnt) ? 1 : (-1);
			}
			finally
			{
				CompareUtil.Pop(this);
			}
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (!(other is deque otherDeque))
			{
				throw new ValueErrorException("expected deque");
			}
			return CompareToWorker(otherDeque, comparer);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new DequeIterator(this);
		}

		public virtual IEnumerator __reversed__()
		{
			return new deque_reverse_iterator(this);
		}

		private void GrowArray()
		{
			if (_data.Length != _maxLen)
			{
				object[] array = ((_maxLen >= 0) ? new object[Math.Min(_maxLen, _data.Length * 2)] : new object[_data.Length * 2]);
				int num;
				int length;
				if (_head >= _tail)
				{
					num = _data.Length - _head;
					length = _data.Length - num;
				}
				else
				{
					num = _tail - _head;
					length = _data.Length - num;
				}
				Array.Copy(_data, _head, array, 0, num);
				Array.Copy(_data, 0, array, num, length);
				_head = 0;
				_tail = _data.Length;
				_data = array;
			}
		}

		private int IndexToSlot(CodeContext context, object index)
		{
			if (_itemCnt == 0)
			{
				throw PythonOps.IndexError("deque index out of range");
			}
			int num = PythonContext.GetContext(context).ConvertToInt32(index);
			if (num >= 0)
			{
				if (num >= _itemCnt)
				{
					throw PythonOps.IndexError("deque index out of range");
				}
				int num2 = _head + num;
				if (num2 >= _data.Length)
				{
					num2 -= _data.Length;
				}
				return num2;
			}
			if (num * -1 > _itemCnt)
			{
				throw PythonOps.IndexError("deque index out of range");
			}
			int num3 = _tail + num;
			if (num3 < 0)
			{
				num3 += _data.Length;
			}
			return num3;
		}

		private void WalkDeque(DequeWalker walker)
		{
			if (_itemCnt == 0)
			{
				return;
			}
			int num = ((_head < _tail) ? _tail : _data.Length);
			for (int i = _head; i < num; i++)
			{
				if (!walker(i))
				{
					return;
				}
			}
			if (_head >= _tail)
			{
				for (int j = 0; j < _tail && walker(j); j++)
				{
				}
			}
		}

		public virtual string __repr__(CodeContext context)
		{
			List<object> andCheckInfinite = PythonOps.GetAndCheckInfinite(this);
			if (andCheckInfinite == null)
			{
				return "[...]";
			}
			int count = andCheckInfinite.Count;
			andCheckInfinite.Add(this);
			try
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("deque([");
				string comma = "";
				lock (_lockObj)
				{
					WalkDeque(delegate(int index)
					{
						sb.Append(comma);
						sb.Append(PythonOps.Repr(context, _data[index]));
						comma = ", ";
						return true;
					});
				}
				if (_maxLen < 0)
				{
					sb.Append("])");
				}
				else
				{
					sb.Append("], maxlen=");
					sb.Append(_maxLen);
					sb.Append(')');
				}
				return sb.ToString();
			}
			finally
			{
				andCheckInfinite.RemoveAt(count);
			}
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			if (CompareUtil.Check(this))
			{
				return 0;
			}
			CompareUtil.Push(this);
			try
			{
				return ((IStructuralEquatable)new PythonTuple(this)).GetHashCode(comparer);
			}
			finally
			{
				CompareUtil.Pop(this);
			}
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is deque))
			{
				return false;
			}
			return EqualsWorker((deque)other, comparer);
		}

		private bool EqualsWorker(deque other)
		{
			return EqualsWorker(other, null);
		}

		private bool EqualsWorker(deque otherDeque, IEqualityComparer comparer)
		{
			if (otherDeque._itemCnt != _itemCnt)
			{
				return false;
			}
			if (otherDeque._itemCnt == 0)
			{
				return true;
			}
			if (CompareUtil.Check(this))
			{
				return true;
			}
			CompareUtil.Push(this);
			try
			{
				int num = otherDeque._head;
				int num2 = _head;
				while (num2 != _tail)
				{
					if (!(comparer?.Equals(_data[num2], otherDeque._data[num]) ?? PythonOps.EqualRetBool(_data[num2], otherDeque._data[num])))
					{
						return false;
					}
					num++;
					if (num == otherDeque._data.Length)
					{
						num = 0;
					}
					num2++;
					if (num2 == _data.Length)
					{
						num2 = 0;
					}
				}
				return true;
			}
			finally
			{
				CompareUtil.Pop(this);
			}
		}

		[return: MaybeNotImplemented]
		public static object operator >(deque self, object other)
		{
			if (!(other is deque otherDeque))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(self.CompareToWorker(otherDeque) > 0);
		}

		[return: MaybeNotImplemented]
		public static object operator <(deque self, object other)
		{
			if (!(other is deque otherDeque))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(self.CompareToWorker(otherDeque) < 0);
		}

		[return: MaybeNotImplemented]
		public static object operator >=(deque self, object other)
		{
			if (!(other is deque otherDeque))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(self.CompareToWorker(otherDeque) >= 0);
		}

		[return: MaybeNotImplemented]
		public static object operator <=(deque self, object other)
		{
			if (!(other is deque otherDeque))
			{
				return NotImplementedType.Value;
			}
			return ScriptingRuntimeHelpers.BooleanToObject(self.CompareToWorker(otherDeque) <= 0);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			int num = 0;
			foreach (object item in (IEnumerable)this)
			{
				array.SetValue(item, index + num++);
			}
		}
	}

	[PythonType]
	public class defaultdict : PythonDictionary
	{
		private object _factory;

		private CallSite<Func<CallSite, CodeContext, object, object>> _missingSite;

		public object default_factory
		{
			get
			{
				return _factory;
			}
			set
			{
				_factory = value;
			}
		}

		public defaultdict(CodeContext context)
		{
			_missingSite = CallSite<Func<CallSite, CodeContext, object, object>>.Create(new PythonInvokeBinder(PythonContext.GetContext(context), new CallSignature(0)));
		}

		public void __init__(object default_factory)
		{
			_factory = default_factory;
		}

		public void __init__(CodeContext context, object default_factory, params object[] args)
		{
			_factory = default_factory;
			foreach (object b in args)
			{
				update(context, b);
			}
		}

		public void __init__(CodeContext context, object default_factory, [ParamDictionary] IDictionary<object, object> dict, params object[] args)
		{
			__init__(context, default_factory, args);
			foreach (KeyValuePair<object, object> item in dict)
			{
				this[item.Key] = item.Value;
			}
		}

		public object __missing__(CodeContext context, object key)
		{
			object factory = _factory;
			if (factory == null)
			{
				throw PythonOps.KeyError(key);
			}
			return this[key] = _missingSite.Target(_missingSite, context, factory);
		}

		public object __copy__(CodeContext context)
		{
			return copy(context);
		}

		public override PythonDictionary copy(CodeContext context)
		{
			defaultdict defaultdict2 = new defaultdict(context);
			defaultdict2.default_factory = default_factory;
			defaultdict2.update(context, this);
			return defaultdict2;
		}

		public override string __repr__(CodeContext context)
		{
			return $"defaultdict({PythonOps.Repr(context, default_factory)}, {base.__repr__(context)})";
		}

		public PythonTuple __reduce__()
		{
			return PythonTuple.MakeTuple(DynamicHelpers.GetPythonType(this), PythonTuple.MakeTuple(default_factory), null, null, iteritems());
		}
	}

	public const string __doc__ = "High performance data structures\n";
}
