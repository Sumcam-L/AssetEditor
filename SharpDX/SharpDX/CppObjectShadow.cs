using System;
using System.Runtime.InteropServices;

namespace SharpDX;

internal abstract class CppObjectShadow : CppObject
{
	public ICallbackable Callback { get; private set; }

	protected abstract CppObjectVtbl GetVtbl { get; }

	public unsafe virtual void Initialize(ICallbackable callbackInstance)
	{
		Callback = callbackInstance;
		base.NativePointer = Marshal.AllocHGlobal(IntPtr.Size * 2);
		GCHandle value = GCHandle.Alloc(this);
		Marshal.WriteIntPtr(base.NativePointer, GetVtbl.Pointer);
		((IntPtr*)(void*)base.NativePointer)[1] = GCHandle.ToIntPtr(value);
	}

	protected unsafe override void Dispose(bool disposing)
	{
		if (base.NativePointer != IntPtr.Zero)
		{
			GCHandle.FromIntPtr(((IntPtr*)(void*)base.NativePointer)[1]).Free();
			Marshal.FreeHGlobal(base.NativePointer);
			base.NativePointer = IntPtr.Zero;
		}
		Callback = null;
		base.Dispose(disposing);
	}

	internal unsafe static T ToShadow<T>(IntPtr thisPtr) where T : CppObjectShadow
	{
		return (T)GCHandle.FromIntPtr(((IntPtr*)(void*)thisPtr)[1]).Target;
	}
}
