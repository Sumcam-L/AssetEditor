using System;
using System.Collections.Generic;
using IronPython.Runtime.Operations;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public class WeakRefTracker
{
	private struct CallbackInfo
	{
		private object callback;

		private WeakHandle _longRef;

		private WeakHandle _shortRef;

		public object Callback => callback;

		public WeakHandle LongRef => _longRef;

		public bool IsFinalizing
		{
			get
			{
				if (!_longRef.IsAlive || _shortRef.IsAlive)
				{
					return _longRef.Target != _shortRef.Target;
				}
				return true;
			}
		}

		public CallbackInfo(object callback, object weakRef)
		{
			this.callback = callback;
			_longRef = new WeakHandle(weakRef, trackResurrection: true);
			_shortRef = new WeakHandle(weakRef, trackResurrection: false);
		}
	}

	private List<CallbackInfo> callbacks;

	public int HandlerCount => callbacks.Count;

	public WeakRefTracker(object callback, object weakRef)
	{
		callbacks = new List<CallbackInfo>(1);
		ChainCallback(callback, weakRef);
	}

	public void ChainCallback(object callback, object weakRef)
	{
		callbacks.Add(new CallbackInfo(callback, weakRef));
	}

	public void RemoveHandlerAt(int index)
	{
		callbacks.RemoveAt(index);
	}

	public void RemoveHandler(object o)
	{
		for (int i = 0; i < HandlerCount; i++)
		{
			if (GetWeakRef(i) == o)
			{
				RemoveHandlerAt(i);
				break;
			}
		}
	}

	public object GetHandlerCallback(int index)
	{
		return callbacks[index].Callback;
	}

	public object GetWeakRef(int index)
	{
		return callbacks[index].LongRef.Target;
	}

	~WeakRefTracker()
	{
		int num = callbacks.Count - 1;
		while (num >= 0)
		{
			CallbackInfo callbackInfo = callbacks[num];
			try
			{
				try
				{
					if (callbackInfo.Callback != null && (!callbackInfo.IsFinalizing || callbackInfo.LongRef.Target is InstanceFinalizer))
					{
						if (callbackInfo.Callback is InstanceFinalizer instanceFinalizer)
						{
							instanceFinalizer.CallDirect(DefaultContext.Default);
						}
						else
						{
							PythonCalls.Call(callbackInfo.Callback, callbackInfo.LongRef.Target);
						}
					}
				}
				catch (Exception)
				{
				}
				callbacks[num].LongRef.Free();
			}
			catch (InvalidOperationException)
			{
			}
			num--;
		}
	}
}
