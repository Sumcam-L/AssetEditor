using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpDX;

public class ComArray : DisposeBase, IEnumerable
{
	protected ComObject[] values;

	private IntPtr nativeBuffer;

	public IntPtr NativePointer => nativeBuffer;

	public int Length
	{
		get
		{
			if (values != null)
			{
				return values.Length;
			}
			return 0;
		}
	}

	public ComArray(params ComObject[] array)
	{
		values = array;
		nativeBuffer = IntPtr.Zero;
		if (values != null)
		{
			int num = array.Length;
			values = new ComObject[num];
			nativeBuffer = Utilities.AllocateMemory(num * Utilities.SizeOf<IntPtr>());
			for (int i = 0; i < num; i++)
			{
				Set(i, array[i]);
			}
		}
	}

	public ComArray(int size)
	{
		values = new ComObject[size];
		nativeBuffer = Utilities.AllocateMemory(size * Utilities.SizeOf<IntPtr>());
	}

	public ComObject Get(int index)
	{
		return values[index];
	}

	internal unsafe void SetFromNative(int index, ComObject value)
	{
		values[index] = value;
		value.NativePointer = ((IntPtr*)(void*)nativeBuffer)[index];
	}

	public unsafe void Set(int index, ComObject value)
	{
		values[index] = value;
		((IntPtr*)(void*)nativeBuffer)[index] = value.NativePointer;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			values = null;
		}
		Utilities.FreeMemory(nativeBuffer);
		nativeBuffer = IntPtr.Zero;
	}

	public IEnumerator GetEnumerator()
	{
		return values.GetEnumerator();
	}
}
public class ComArray<T> : ComArray, IEnumerable<T>, IEnumerable where T : ComObject
{
	private struct ArrayEnumerator<T1> : IEnumerator<T1>, IDisposable, IEnumerator where T1 : ComObject
	{
		private readonly IEnumerator enumerator;

		public T1 Current => (T1)enumerator.Current;

		object IEnumerator.Current => Current;

		public ArrayEnumerator(IEnumerator enumerator)
		{
			this.enumerator = enumerator;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			return enumerator.MoveNext();
		}

		public void Reset()
		{
			enumerator.Reset();
		}
	}

	public T this[int i]
	{
		get
		{
			return (T)Get(i);
		}
		set
		{
			Set(i, value);
		}
	}

	public ComArray(params T[] array)
		: base(array)
	{
	}

	public ComArray(int size)
		: base(size)
	{
	}

	public new IEnumerator<T> GetEnumerator()
	{
		return new ArrayEnumerator<T>(values.GetEnumerator());
	}
}
