using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonIterTools
{
	[PythonHidden]
	[PythonType]
	public class IterBase : IEnumerator
	{
		private IEnumerator _inner;

		internal IEnumerator InnerEnumerator
		{
			set
			{
				_inner = value;
			}
		}

		object IEnumerator.Current => _inner.Current;

		bool IEnumerator.MoveNext()
		{
			return _inner.MoveNext();
		}

		void IEnumerator.Reset()
		{
			_inner.Reset();
		}

		public object __iter__()
		{
			return this;
		}
	}

	[PythonType]
	public class chain : IterBase
	{
		private chain()
		{
		}

		public chain(params object[] iterables)
		{
			base.InnerEnumerator = LazyYielder(iterables);
		}

		[ClassMethod]
		public static chain from_iterable(CodeContext context, PythonType cls, object iterables)
		{
			chain chain2;
			if (cls == DynamicHelpers.GetPythonTypeFromType(typeof(chain)))
			{
				chain2 = new chain();
				chain2.InnerEnumerator = LazyYielder(iterables);
			}
			else
			{
				chain2 = (chain)cls.CreateInstance(context);
				chain2.InnerEnumerator = LazyYielder(iterables);
			}
			return chain2;
		}

		private static IEnumerator<object> LazyYielder(object iterables)
		{
			IEnumerator ie = PythonOps.GetEnumerator(iterables);
			while (ie.MoveNext())
			{
				IEnumerator inner = PythonOps.GetEnumerator(ie.Current);
				while (inner.MoveNext())
				{
					yield return inner.Current;
				}
			}
		}
	}

	[PythonType]
	public class compress : IterBase
	{
		private compress()
		{
		}

		public compress(CodeContext context, [NotNull] object data, [NotNull] object selectors)
		{
			EnsureIterator(context, data);
			EnsureIterator(context, selectors);
			base.InnerEnumerator = LazyYielder(data, selectors);
		}

		private static void EnsureIterator(CodeContext context, object iter)
		{
			if (iter is IEnumerable || iter is IEnumerator || iter is IEnumerable<object> || iter is IEnumerator<object> || (iter != null && (PythonOps.HasAttr(context, iter, "__iter__") || PythonOps.HasAttr(context, iter, "__getitem__"))))
			{
				return;
			}
			if (iter is OldInstance)
			{
				throw PythonOps.TypeError("iteration over non-sequence");
			}
			throw PythonOps.TypeError("'{0}' object is not iterable", PythonTypeOps.GetName(iter));
		}

		private static IEnumerator<object> LazyYielder(object data, object selectors)
		{
			IEnumerator de = PythonOps.GetEnumerator(data);
			IEnumerator se = PythonOps.GetEnumerator(selectors);
			while (de.MoveNext() && se.MoveNext())
			{
				if (PythonOps.IsTrue(se.Current))
				{
					yield return de.Current;
				}
			}
		}
	}

	[PythonType]
	public class count : IterBase, ICodeFormattable
	{
		private int _curInt;

		private object _step;

		private object _cur;

		public count()
		{
			_curInt = 0;
			_step = 1;
			base.InnerEnumerator = IntYielder(this, 0, 1);
		}

		public count(int start)
		{
			_curInt = start;
			_step = 1;
			base.InnerEnumerator = IntYielder(this, start, 1);
		}

		public count(BigInteger start)
		{
			_cur = start;
			_step = 1;
			base.InnerEnumerator = BigIntYielder(this, start, 1);
		}

		public count([DefaultParameterValue(0)] int start, [DefaultParameterValue(1)] int step)
		{
			_curInt = start;
			_step = step;
			base.InnerEnumerator = IntYielder(this, start, step);
		}

		public count([DefaultParameterValue(0)] int start, BigInteger step)
		{
			_curInt = start;
			_step = step;
			base.InnerEnumerator = IntYielder(this, start, step);
		}

		public count(BigInteger start, int step)
		{
			_cur = start;
			_step = step;
			base.InnerEnumerator = BigIntYielder(this, start, step);
		}

		public count(BigInteger start, BigInteger step)
		{
			_cur = start;
			_step = step;
			base.InnerEnumerator = BigIntYielder(this, start, step);
		}

		public count(CodeContext context, [DefaultParameterValue(0)] object start, [DefaultParameterValue(1)] object step)
		{
			EnsureNumeric(context, start);
			EnsureNumeric(context, step);
			_cur = start;
			_step = step;
			base.InnerEnumerator = ObjectYielder(PythonContext.GetContext(context), this, start, step);
		}

		private static void EnsureNumeric(CodeContext context, object num)
		{
			if (num is int || num is double || num is BigInteger || num is Complex || (num != null && (PythonOps.HasAttr(context, num, "__int__") || PythonOps.HasAttr(context, num, "__float__"))))
			{
				return;
			}
			throw PythonOps.TypeError("a number is required");
		}

		private static IEnumerator<object> IntYielder(count c, int start, int step)
		{
			int prev;
			while (true)
			{
				prev = c._curInt;
				try
				{
					start = checked(start + step);
				}
				catch (OverflowException)
				{
					break;
				}
				c._curInt = start;
				yield return prev;
			}
			BigInteger startBig = (BigInteger)start + (BigInteger)step;
			c._cur = startBig;
			yield return prev;
			startBig += (BigInteger)step;
			while (true)
			{
				object prevObj = c._cur;
				c._cur = startBig;
				yield return prevObj;
				startBig += (BigInteger)step;
			}
		}

		private static IEnumerator<object> IntYielder(count c, int start, BigInteger step)
		{
			BigInteger startBig = start + step;
			c._cur = startBig;
			yield return start;
			startBig += step;
			while (true)
			{
				object prevObj = c._cur;
				c._cur = startBig;
				yield return prevObj;
				startBig += step;
			}
		}

		private static IEnumerator<BigInteger> BigIntYielder(count c, BigInteger start, int step)
		{
			start += (BigInteger)step;
			while (true)
			{
				BigInteger prev = (BigInteger)c._cur;
				c._cur = start;
				yield return prev;
				start += (BigInteger)step;
			}
		}

		private static IEnumerator<BigInteger> BigIntYielder(count c, BigInteger start, BigInteger step)
		{
			start += step;
			while (true)
			{
				BigInteger prev = (BigInteger)c._cur;
				c._cur = start;
				yield return prev;
				start += step;
			}
		}

		private static IEnumerator<object> ObjectYielder(PythonContext context, count c, object start, object step)
		{
			start = context.Operation(PythonOperationKind.Add, start, step);
			while (true)
			{
				object prev = c._cur;
				c._cur = start;
				yield return prev;
				start = context.Operation(PythonOperationKind.Add, start, step);
			}
		}

		public PythonTuple __reduce__()
		{
			PythonTuple pythonTuple = ((!StepIsOne()) ? PythonOps.MakeTuple((_cur == null) ? ((object)_curInt) : _cur, _step) : PythonOps.MakeTuple((_cur == null) ? ((object)_curInt) : _cur));
			return PythonOps.MakeTuple(DynamicHelpers.GetPythonType(this), pythonTuple);
		}

		public PythonTuple __reduce_ex__([Optional] int protocol)
		{
			return __reduce__();
		}

		private bool StepIsOne()
		{
			if (_step is int)
			{
				return (int)_step == 1;
			}
			if (_step is Extensible<int> extensible)
			{
				return extensible.Value == 1;
			}
			return false;
		}

		public string __repr__(CodeContext context)
		{
			object o = ((_cur == null) ? ((object)_curInt) : _cur);
			if (StepIsOne())
			{
				return $"count({PythonOps.Repr(context, o)})";
			}
			return $"count({PythonOps.Repr(context, o)}, {PythonOps.Repr(context, _step)})";
		}
	}

	[PythonType]
	public class cycle : IterBase
	{
		public cycle(object iterable)
		{
			base.InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable));
		}

		private IEnumerator<object> Yielder(IEnumerator iter)
		{
			List result = new List();
			while (MoveNextHelper(iter))
			{
				result.AddNoLock(iter.Current);
				yield return iter.Current;
			}
			if (result.__len__() == 0)
			{
				yield break;
			}
			while (true)
			{
				for (int i = 0; i < result.__len__(); i++)
				{
					yield return result[i];
				}
			}
		}
	}

	[PythonType]
	public class dropwhile : IterBase
	{
		private readonly CodeContext _context;

		public dropwhile(CodeContext context, object predicate, object iterable)
		{
			_context = context;
			base.InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
		}

		private IEnumerator<object> Yielder(object predicate, IEnumerator iter)
		{
			PythonContext pc = PythonContext.GetContext(_context);
			while (MoveNextHelper(iter))
			{
				if (!Converter.ConvertToBoolean(pc.CallSplat(predicate, iter.Current)))
				{
					yield return iter.Current;
					break;
				}
			}
			while (MoveNextHelper(iter))
			{
				yield return iter.Current;
			}
		}
	}

	[PythonType]
	public class groupby : IterBase
	{
		private static readonly object _starterKey = new object();

		private bool _fFinished;

		private object _key;

		private readonly CodeContext _context;

		public groupby(CodeContext context, object iterable)
		{
			base.InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable));
			_context = context;
		}

		public groupby(CodeContext context, object iterable, object key)
		{
			base.InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable));
			_context = context;
			if (key != null)
			{
				_key = key;
			}
		}

		private IEnumerator<object> Yielder(IEnumerator iter)
		{
			object curKey = _starterKey;
			if (!MoveNextHelper(iter))
			{
				yield break;
			}
			while (!_fFinished)
			{
				while (PythonContext.Equal(GetKey(iter.Current), curKey))
				{
					if (!MoveNextHelper(iter))
					{
						_fFinished = true;
						yield break;
					}
				}
				curKey = GetKey(iter.Current);
				yield return PythonTuple.MakeTuple(curKey, Grouper(iter, curKey));
			}
		}

		private IEnumerator<object> Grouper(IEnumerator iter, object curKey)
		{
			while (PythonContext.Equal(GetKey(iter.Current), curKey))
			{
				yield return iter.Current;
				if (!MoveNextHelper(iter))
				{
					_fFinished = true;
					break;
				}
			}
		}

		private object GetKey(object val)
		{
			if (_key == null)
			{
				return val;
			}
			return PythonContext.GetContext(_context).CallSplat(_key, val);
		}
	}

	[PythonType]
	public class ifilter : IterBase
	{
		private readonly CodeContext _context;

		public ifilter(CodeContext context, object predicate, object iterable)
		{
			_context = context;
			base.InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
		}

		private IEnumerator<object> Yielder(object predicate, IEnumerator iter)
		{
			while (MoveNextHelper(iter))
			{
				if (ShouldYield(predicate, iter.Current))
				{
					yield return iter.Current;
				}
			}
		}

		private bool ShouldYield(object predicate, object current)
		{
			if (predicate == null)
			{
				return PythonOps.IsTrue(current);
			}
			return Converter.ConvertToBoolean(PythonContext.GetContext(_context).CallSplat(predicate, current));
		}
	}

	[PythonType]
	public class ifilterfalse : IterBase
	{
		private readonly CodeContext _context;

		public ifilterfalse(CodeContext context, object predicate, object iterable)
		{
			_context = context;
			base.InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
		}

		private IEnumerator<object> Yielder(object predicate, IEnumerator iter)
		{
			while (MoveNextHelper(iter))
			{
				if (ShouldYield(predicate, iter.Current))
				{
					yield return iter.Current;
				}
			}
		}

		private bool ShouldYield(object predicate, object current)
		{
			if (predicate == null)
			{
				return !PythonOps.IsTrue(current);
			}
			return !Converter.ConvertToBoolean(PythonContext.GetContext(_context).CallSplat(predicate, current));
		}
	}

	[PythonType]
	public class imap : IEnumerator
	{
		private object _function;

		private IEnumerator[] _iterables;

		private readonly CodeContext _context;

		object IEnumerator.Current
		{
			get
			{
				object[] array = new object[_iterables.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = _iterables[i].Current;
				}
				if (_function == null)
				{
					return PythonTuple.MakeTuple(array);
				}
				return PythonContext.GetContext(_context).CallSplat(_function, array);
			}
		}

		public imap(CodeContext context, object function, params object[] iterables)
		{
			if (iterables.Length < 1)
			{
				throw PythonOps.TypeError("imap() must have at least two arguments");
			}
			_function = function;
			_context = context;
			_iterables = new IEnumerator[iterables.Length];
			for (int i = 0; i < iterables.Length; i++)
			{
				_iterables[i] = PythonOps.GetEnumerator(iterables[i]);
			}
		}

		bool IEnumerator.MoveNext()
		{
			for (int i = 0; i < _iterables.Length; i++)
			{
				if (!MoveNextHelper(_iterables[i]))
				{
					return false;
				}
			}
			return true;
		}

		void IEnumerator.Reset()
		{
			for (int i = 0; i < _iterables.Length; i++)
			{
				_iterables[i].Reset();
			}
		}

		public object __iter__()
		{
			return this;
		}
	}

	[PythonType]
	public class islice : IterBase
	{
		public islice(object iterable, object stop)
			: this(iterable, 0, stop, 1)
		{
		}

		public islice(object iterable, object start, object stop)
			: this(iterable, start, stop, 1)
		{
		}

		public islice(object iterable, object start, object stop, object step)
		{
			int result = 0;
			int result2 = -1;
			if ((start != null && !Converter.TryConvertToInt32(start, out result)) || result < 0)
			{
				throw PythonOps.ValueError("start argument must be non-negative integer, ({0})", start);
			}
			if (stop != null && (!Converter.TryConvertToInt32(stop, out result2) || result2 < 0))
			{
				throw PythonOps.ValueError("stop argument must be non-negative integer ({0})", stop);
			}
			int result3 = 1;
			if ((step != null && !Converter.TryConvertToInt32(step, out result3)) || result3 <= 0)
			{
				throw PythonOps.ValueError("step must be 1 or greater for islice");
			}
			base.InnerEnumerator = Yielder(PythonOps.GetEnumerator(iterable), result, result2, result3);
		}

		private IEnumerator<object> Yielder(IEnumerator iter, int start, int stop, int step)
		{
			if (!MoveNextHelper(iter))
			{
				yield break;
			}
			int cur = 0;
			while (true)
			{
				if (cur < start)
				{
					if (MoveNextHelper(iter))
					{
						cur++;
						continue;
					}
					break;
				}
				while (cur < stop || stop == -1)
				{
					yield return iter.Current;
					if (cur + step < 0)
					{
						break;
					}
					for (int i = 0; i < step; i++)
					{
						if (stop != -1)
						{
							int num;
							cur = (num = cur + 1);
							if (num >= stop)
							{
								yield break;
							}
						}
						if (!MoveNextHelper(iter))
						{
							yield break;
						}
					}
				}
				break;
			}
		}
	}

	[PythonType]
	public class izip : IEnumerator
	{
		private readonly IEnumerator[] _iters;

		private PythonTuple _current;

		object IEnumerator.Current => _current;

		public izip(params object[] iterables)
		{
			_iters = new IEnumerator[iterables.Length];
			for (int i = 0; i < iterables.Length; i++)
			{
				_iters[i] = PythonOps.GetEnumerator(iterables[i]);
			}
		}

		bool IEnumerator.MoveNext()
		{
			if (_iters.Length == 0)
			{
				return false;
			}
			object[] array = new object[_iters.Length];
			for (int i = 0; i < _iters.Length; i++)
			{
				if (!MoveNextHelper(_iters[i]))
				{
					return false;
				}
				array[i] = _iters[i].Current;
			}
			_current = PythonTuple.MakeTuple(array);
			return true;
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public object __iter__()
		{
			return this;
		}
	}

	[PythonType]
	public class izip_longest : IEnumerator
	{
		private readonly IEnumerator[] _iters;

		private readonly object _fill;

		private PythonTuple _current;

		object IEnumerator.Current => _current;

		public izip_longest(params object[] iterables)
		{
			_iters = new IEnumerator[iterables.Length];
			for (int i = 0; i < iterables.Length; i++)
			{
				_iters[i] = PythonOps.GetEnumerator(iterables[i]);
			}
		}

		public izip_longest([ParamDictionary] IDictionary<object, object> paramDict, params object[] iterables)
		{
			if (paramDict.TryGetValue("fillvalue", out var value))
			{
				_fill = value;
				if (paramDict.Count != 1)
				{
					paramDict.Remove("fillvalue");
					throw UnexpectedKeywordArgument(paramDict);
				}
			}
			else if (paramDict.Count != 0)
			{
				throw UnexpectedKeywordArgument(paramDict);
			}
			_iters = new IEnumerator[iterables.Length];
			for (int i = 0; i < iterables.Length; i++)
			{
				_iters[i] = PythonOps.GetEnumerator(iterables[i]);
			}
		}

		bool IEnumerator.MoveNext()
		{
			if (_iters.Length == 0)
			{
				return false;
			}
			object[] array = new object[_iters.Length];
			bool flag = false;
			for (int i = 0; i < _iters.Length; i++)
			{
				if (!MoveNextHelper(_iters[i]))
				{
					array[i] = _fill;
					continue;
				}
				flag = true;
				array[i] = _iters[i].Current;
			}
			if (flag)
			{
				_current = PythonTuple.MakeTuple(array);
				return true;
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public object __iter__()
		{
			return this;
		}
	}

	[PythonType]
	public class product : IterBase
	{
		public product(params object[] iterables)
		{
			base.InnerEnumerator = Yielder(ArrayUtils.ConvertAll(iterables, (object x) => new List(PythonOps.GetEnumerator(x))));
		}

		public product([ParamDictionary] IDictionary<object, object> paramDict, params object[] iterables)
		{
			int num = 1;
			if (paramDict.TryGetValue("repeat", out var value))
			{
				if (!(value is int))
				{
					throw PythonOps.TypeError("an integer is required");
				}
				num = (int)value;
				if (paramDict.Count != 1)
				{
					paramDict.Remove("repeat");
					throw UnexpectedKeywordArgument(paramDict);
				}
			}
			else if (paramDict.Count != 0)
			{
				throw UnexpectedKeywordArgument(paramDict);
			}
			List[] array = new List[iterables.Length * num];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < iterables.Length; j++)
				{
					array[i * iterables.Length + j] = new List(iterables[j]);
				}
			}
			base.InnerEnumerator = Yielder(array);
		}

		private IEnumerator<object> Yielder(List[] iterables)
		{
			if (iterables.Length > 0)
			{
				IEnumerator[] enums = new IEnumerator[iterables.Length];
				enums[0] = iterables[0].GetEnumerator();
				int curDepth = 0;
				do
				{
					if (enums[curDepth].MoveNext())
					{
						if (curDepth == enums.Length - 1)
						{
							object[] final = new object[enums.Length];
							for (int i = 0; i < enums.Length; i++)
							{
								final[i] = enums[i].Current;
							}
							yield return PythonTuple.MakeTuple(final);
						}
						else
						{
							curDepth++;
							enums[curDepth] = iterables[curDepth].GetEnumerator();
						}
					}
					else
					{
						curDepth--;
					}
				}
				while (curDepth != -1);
			}
			else
			{
				yield return PythonTuple.EMPTY;
			}
		}
	}

	[PythonType]
	public class combinations : IterBase
	{
		private readonly List _data;

		public combinations(object iterable, object r)
		{
			_data = new List(iterable);
			base.InnerEnumerator = Yielder(GetR(r, _data));
		}

		private IEnumerator<object> Yielder(int r)
		{
			IEnumerator[] enums = new IEnumerator[r];
			if (r > 0)
			{
				enums[0] = _data.GetEnumerator();
				int curDepth = 0;
				int[] curIndices = new int[enums.Length];
				do
				{
					if (enums[curDepth].MoveNext())
					{
						curIndices[curDepth]++;
						bool shouldSkip = false;
						for (int i = 0; i < curDepth; i++)
						{
							if (curIndices[i] >= curIndices[curDepth])
							{
								shouldSkip = true;
								break;
							}
						}
						if (shouldSkip)
						{
							continue;
						}
						if (curDepth == enums.Length - 1)
						{
							object[] final = new object[r];
							for (int j = 0; j < enums.Length; j++)
							{
								final[j] = enums[j].Current;
							}
							yield return PythonTuple.MakeTuple(final);
						}
						else
						{
							curDepth++;
							enums[curDepth] = _data.GetEnumerator();
							curIndices[curDepth] = 0;
						}
					}
					else
					{
						curDepth--;
					}
				}
				while (curDepth != -1);
			}
			else
			{
				yield return PythonTuple.EMPTY;
			}
		}
	}

	[PythonType]
	public class combinations_with_replacement : IterBase
	{
		private readonly List _data;

		public combinations_with_replacement(object iterable, object r)
		{
			_data = new List(iterable);
			base.InnerEnumerator = Yielder(GetR(r, _data));
		}

		private IEnumerator<object> Yielder(int r)
		{
			IEnumerator[] enums = new IEnumerator[r];
			if (r > 0)
			{
				enums[0] = _data.GetEnumerator();
				int curDepth = 0;
				int[] curIndices = new int[enums.Length];
				do
				{
					if (enums[curDepth].MoveNext())
					{
						curIndices[curDepth]++;
						bool shouldSkip = false;
						for (int i = 0; i < curDepth; i++)
						{
							if (curIndices[i] > curIndices[curDepth])
							{
								shouldSkip = true;
								break;
							}
						}
						if (shouldSkip)
						{
							continue;
						}
						if (curDepth == enums.Length - 1)
						{
							object[] final = new object[r];
							for (int j = 0; j < enums.Length; j++)
							{
								final[j] = enums[j].Current;
							}
							yield return PythonTuple.MakeTuple(final);
						}
						else
						{
							curDepth++;
							enums[curDepth] = _data.GetEnumerator();
							curIndices[curDepth] = 0;
						}
					}
					else
					{
						curDepth--;
					}
				}
				while (curDepth != -1);
			}
			else
			{
				yield return PythonTuple.EMPTY;
			}
		}
	}

	[PythonType]
	public class permutations : IterBase
	{
		private readonly List _data;

		public permutations(object iterable)
		{
			_data = new List(iterable);
			base.InnerEnumerator = Yielder(_data.Count);
		}

		public permutations(object iterable, object r)
		{
			_data = new List(iterable);
			base.InnerEnumerator = Yielder(GetR(r, _data));
		}

		private IEnumerator<object> Yielder(int r)
		{
			if (r > 0)
			{
				IEnumerator[] enums = new IEnumerator[r];
				enums[0] = _data.GetEnumerator();
				int curDepth = 0;
				int[] curIndices = new int[enums.Length];
				do
				{
					if (enums[curDepth].MoveNext())
					{
						curIndices[curDepth]++;
						bool shouldSkip = false;
						for (int i = 0; i < curDepth; i++)
						{
							if (curIndices[i] == curIndices[curDepth])
							{
								shouldSkip = true;
								break;
							}
						}
						if (shouldSkip)
						{
							continue;
						}
						if (curDepth == enums.Length - 1)
						{
							object[] final = new object[r];
							for (int j = 0; j < enums.Length; j++)
							{
								final[j] = enums[j].Current;
							}
							yield return PythonTuple.MakeTuple(final);
						}
						else
						{
							curDepth++;
							enums[curDepth] = _data.GetEnumerator();
							curIndices[curDepth] = 0;
						}
					}
					else
					{
						curDepth--;
					}
				}
				while (curDepth != -1);
			}
			else
			{
				yield return PythonTuple.EMPTY;
			}
		}
	}

	[PythonType]
	[DontMapICollectionToLen]
	public class repeat : IterBase, ICodeFormattable, ICollection, IEnumerable
	{
		private int _remaining;

		private bool _fInfinite;

		private object _obj;

		int ICollection.Count => __length_hint__();

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot => this;

		public repeat(object @object)
		{
			_obj = @object;
			base.InnerEnumerator = Yielder();
			_fInfinite = true;
		}

		public repeat(object @object, int times)
		{
			_obj = @object;
			base.InnerEnumerator = Yielder();
			_remaining = times;
		}

		private IEnumerator<object> Yielder()
		{
			while (_fInfinite || _remaining > 0)
			{
				_remaining--;
				yield return _obj;
			}
		}

		public int __length_hint__()
		{
			if (_fInfinite)
			{
				throw PythonOps.TypeError("len() of unsized object");
			}
			return Math.Max(_remaining, 0);
		}

		public virtual string __repr__(CodeContext context)
		{
			if (_fInfinite)
			{
				return $"{PythonOps.GetPythonTypeName(this)}({PythonOps.Repr(context, _obj)})";
			}
			return $"{PythonOps.GetPythonTypeName(this)}({PythonOps.Repr(context, _obj)}, {_remaining})";
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (_fInfinite)
			{
				throw new InvalidOperationException();
			}
			if (_remaining > array.Length - index)
			{
				throw new IndexOutOfRangeException();
			}
			for (int i = 0; i < _remaining; i++)
			{
				array.SetValue(_obj, index + i);
			}
			_remaining = 0;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			while (_fInfinite || _remaining > 0)
			{
				_remaining--;
				yield return _obj;
			}
		}
	}

	[PythonType]
	public class starmap : IterBase
	{
		public starmap(CodeContext context, object function, object iterable)
		{
			base.InnerEnumerator = Yielder(context, function, PythonOps.GetEnumerator(iterable));
		}

		private IEnumerator<object> Yielder(CodeContext context, object function, IEnumerator iter)
		{
			PythonContext pc = PythonContext.GetContext(context);
			while (MoveNextHelper(iter))
			{
				object[] objargs;
				if (iter.Current is PythonTuple args)
				{
					objargs = new object[args.__len__()];
					for (int i = 0; i < objargs.Length; i++)
					{
						objargs[i] = args[i];
					}
				}
				else
				{
					List list = new List(PythonOps.GetEnumerator(iter.Current));
					objargs = ArrayUtils.ToArray(list);
				}
				yield return pc.CallSplat(function, objargs);
			}
		}
	}

	[PythonType]
	public class takewhile : IterBase
	{
		private readonly CodeContext _context;

		public takewhile(CodeContext context, object predicate, object iterable)
		{
			_context = context;
			base.InnerEnumerator = Yielder(predicate, PythonOps.GetEnumerator(iterable));
		}

		private IEnumerator<object> Yielder(object predicate, IEnumerator iter)
		{
			while (MoveNextHelper(iter) && Converter.ConvertToBoolean(PythonContext.GetContext(_context).CallSplat(predicate, iter.Current)))
			{
				yield return iter.Current;
			}
		}
	}

	[PythonHidden]
	public class TeeIterator : IEnumerator, IWeakReferenceable
	{
		internal IEnumerator _iter;

		internal List _data;

		private int _curIndex = -1;

		private WeakRefTracker _weakRef;

		object IEnumerator.Current => _data[_curIndex];

		public TeeIterator(object iterable)
		{
			if (iterable is TeeIterator teeIterator)
			{
				_iter = teeIterator._iter;
				_data = teeIterator._data;
			}
			else
			{
				_iter = PythonOps.GetEnumerator(iterable);
				_data = new List();
			}
		}

		public TeeIterator(IEnumerator iter, List dataList)
		{
			_iter = iter;
			_data = dataList;
		}

		bool IEnumerator.MoveNext()
		{
			lock (_data)
			{
				_curIndex++;
				if (_curIndex >= _data.__len__() && MoveNextHelper(_iter))
				{
					_data.append(_iter.Current);
				}
				return _curIndex < _data.__len__();
			}
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		public object __iter__()
		{
			return this;
		}

		WeakRefTracker IWeakReferenceable.GetWeakRef()
		{
			return _weakRef;
		}

		bool IWeakReferenceable.SetWeakRef(WeakRefTracker value)
		{
			_weakRef = value;
			return true;
		}

		void IWeakReferenceable.SetFinalizer(WeakRefTracker value)
		{
			_weakRef = value;
		}
	}

	public const string __doc__ = "Provides functions and classes for working with iterable objects.";

	public static object tee(object iterable)
	{
		return tee(iterable, 2);
	}

	public static object tee(object iterable, int n)
	{
		if (n < 0)
		{
			throw PythonOps.ValueError("n cannot be negative");
		}
		object[] array = new object[n];
		if (!(iterable is TeeIterator))
		{
			IEnumerator enumerator = PythonOps.GetEnumerator(iterable);
			List dataList = new List();
			for (int i = 0; i < n; i++)
			{
				array[i] = new TeeIterator(enumerator, dataList);
			}
		}
		else if (n != 0)
		{
			TeeIterator teeIterator = (TeeIterator)(array[0] = iterable as TeeIterator);
			for (int j = 1; j < n; j++)
			{
				array[1] = new TeeIterator(teeIterator._iter, teeIterator._data);
			}
		}
		return PythonTuple.MakeTuple(array);
	}

	private static Exception UnexpectedKeywordArgument(IDictionary<object, object> paramDict)
	{
		using (IEnumerator<object> enumerator = paramDict.Keys.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				return PythonOps.TypeError("got unexpected keyword argument {0}", current);
			}
		}
		throw new InvalidOperationException();
	}

	private static int GetR(object r, List data)
	{
		int num;
		if (r != null)
		{
			num = Converter.ConvertToInt32(r);
			if (num < 0)
			{
				throw PythonOps.ValueError("r cannot be negative");
			}
		}
		else
		{
			num = data.Count;
		}
		return num;
	}

	private static bool MoveNextHelper(IEnumerator move)
	{
		try
		{
			return move.MoveNext();
		}
		catch (IndexOutOfRangeException)
		{
			return false;
		}
		catch (StopIterationException)
		{
			return false;
		}
	}
}
