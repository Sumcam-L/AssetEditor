using System;
using System.Collections.Generic;

namespace SharpDX;

public class DisposeCollector : DisposeBase
{
	private List<object> disposables;

	public int Count => disposables.Count;

	public void DisposeAndClear()
	{
		if (disposables == null)
		{
			return;
		}
		for (int num = disposables.Count - 1; num >= 0; num--)
		{
			object obj = disposables[num];
			if (obj is IDisposable)
			{
				((IDisposable)obj).Dispose();
			}
			else
			{
				Utilities.FreeMemory((IntPtr)obj);
			}
			disposables.RemoveAt(num);
		}
		disposables.Clear();
	}

	protected override void Dispose(bool disposeManagedResources)
	{
		DisposeAndClear();
		disposables = null;
	}

	public T Collect<T>(T toDispose)
	{
		if (!(toDispose is IDisposable) && !(toDispose is IntPtr))
		{
			throw new ArgumentException("Argument must be IDisposable or IntPtr");
		}
		if (toDispose is IntPtr memoryPtr && !Utilities.IsMemoryAligned(memoryPtr))
		{
			throw new ArgumentException("Memory pointer is invalid. Memory must have been allocated with Utilties.AllocateMemory");
		}
		Component component = toDispose as Component;
		if (component != null && component.IsAttached)
		{
			return toDispose;
		}
		if (!object.Equals(toDispose, default(T)))
		{
			if (disposables == null)
			{
				disposables = new List<object>();
			}
			if (!disposables.Contains(toDispose))
			{
				disposables.Add(toDispose);
				if (component != null)
				{
					component.IsAttached = true;
				}
			}
		}
		return toDispose;
	}

	public void RemoveAndDispose<T>(ref T objectToDispose)
	{
		if (disposables != null)
		{
			Remove(objectToDispose);
			if (objectToDispose is IDisposable disposable)
			{
				disposable.Dispose();
			}
			else
			{
				object obj = objectToDispose;
				IntPtr alignedBuffer = (IntPtr)obj;
				Utilities.FreeMemory(alignedBuffer);
			}
			objectToDispose = default(T);
		}
	}

	public void Remove<T>(T toDisposeArg)
	{
		if (disposables != null && disposables.Contains(toDisposeArg))
		{
			disposables.Remove(toDisposeArg);
			if (toDisposeArg is Component component)
			{
				component.IsAttached = false;
			}
		}
	}
}
