using System;

namespace SharpDX;

public class CppObject : DisposeBase
{
	protected internal unsafe void* _nativePointer;

	public object Tag { get; set; }

	public unsafe IntPtr NativePointer
	{
		get
		{
			return (IntPtr)_nativePointer;
		}
		set
		{
			void* ptr = (void*)value;
			if (_nativePointer != ptr)
			{
				NativePointerUpdating();
				void* nativePointer = _nativePointer;
				_nativePointer = ptr;
				NativePointerUpdated((IntPtr)nativePointer);
			}
		}
	}

	public CppObject(IntPtr pointer)
	{
		NativePointer = pointer;
	}

	protected CppObject()
	{
	}

	public static explicit operator IntPtr(CppObject cppObject)
	{
		return cppObject?.NativePointer ?? IntPtr.Zero;
	}

	protected void FromTemp(CppObject temp)
	{
		NativePointer = temp.NativePointer;
		temp.NativePointer = IntPtr.Zero;
	}

	protected void FromTemp(IntPtr temp)
	{
		NativePointer = temp;
	}

	protected virtual void NativePointerUpdating()
	{
	}

	protected virtual void NativePointerUpdated(IntPtr oldNativePointer)
	{
	}

	protected override void Dispose(bool disposing)
	{
	}

	public static T FromPointer<T>(IntPtr comObjectPtr) where T : CppObject
	{
		if (!(comObjectPtr == IntPtr.Zero))
		{
			return (T)Activator.CreateInstance(typeof(T), comObjectPtr);
		}
		return null;
	}

	internal static T FromPointerUnsafe<T>(IntPtr comObjectPtr)
	{
		if (!(comObjectPtr == IntPtr.Zero))
		{
			return (T)Activator.CreateInstance(typeof(T), comObjectPtr);
		}
		return (T)(object)null;
	}

	public static IntPtr ToCallbackPtr<TCallback>(ICallbackable callback) where TCallback : ICallbackable
	{
		if (callback == null)
		{
			return IntPtr.Zero;
		}
		if (callback is CppObject)
		{
			return ((CppObject)callback).NativePointer;
		}
		ShadowContainer shadowContainer = callback.Shadow as ShadowContainer;
		if (shadowContainer == null)
		{
			shadowContainer = new ShadowContainer();
			shadowContainer.Initialize(callback);
		}
		return shadowContainer.Find(typeof(TCallback));
	}
}
