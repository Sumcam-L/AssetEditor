using System;
using System.Runtime.InteropServices;
using SharpDX.Diagnostics;

namespace SharpDX;

public class ComObject : CppObject, IUnknown
{
	public ComObject(IntPtr pointer)
		: base(pointer)
	{
	}

	public ComObject(object iunknowObject)
	{
		base.NativePointer = Marshal.GetIUnknownForObject(iunknowObject);
	}

	protected ComObject()
	{
	}

	public virtual void QueryInterface(Guid guid, out IntPtr outPtr)
	{
		((IUnknown)this).QueryInterface(ref guid, out outPtr).CheckError();
	}

	public virtual IntPtr QueryInterfaceOrNull(Guid guid)
	{
		IntPtr comObject = IntPtr.Zero;
		((IUnknown)this).QueryInterface(ref guid, out comObject);
		return comObject;
	}

	public static bool EqualsComObject<T>(T left, T right) where T : ComObject
	{
		if (object.Equals(left, right))
		{
			return true;
		}
		if (left == null || right == null)
		{
			return false;
		}
		return left.NativePointer == right.NativePointer;
	}

	public virtual T QueryInterface<T>() where T : ComObject
	{
		QueryInterface(Utilities.GetGuidFromType(typeof(T)), out var outPtr);
		return CppObject.FromPointer<T>(outPtr);
	}

	internal virtual T QueryInterfaceUnsafe<T>()
	{
		QueryInterface(Utilities.GetGuidFromType(typeof(T)), out var outPtr);
		return CppObject.FromPointerUnsafe<T>(outPtr);
	}

	public static T As<T>(object comObject) where T : ComObject
	{
		using ComObject comObject2 = new ComObject(Marshal.GetIUnknownForObject(comObject));
		return comObject2.QueryInterface<T>();
	}

	public static T As<T>(IntPtr iunknownPtr) where T : ComObject
	{
		using ComObject comObject = new ComObject(iunknownPtr);
		return comObject.QueryInterface<T>();
	}

	internal static T AsUnsafe<T>(IntPtr iunknownPtr)
	{
		using ComObject comObject = new ComObject(iunknownPtr);
		return comObject.QueryInterfaceUnsafe<T>();
	}

	public static T QueryInterface<T>(object comObject) where T : ComObject
	{
		using ComObject comObject2 = new ComObject(Marshal.GetIUnknownForObject(comObject));
		return comObject2.QueryInterface<T>();
	}

	public static T QueryInterfaceOrNull<T>(IntPtr comPointer) where T : ComObject
	{
		if (comPointer == IntPtr.Zero)
		{
			return null;
		}
		Guid iid = Utilities.GetGuidFromType(typeof(T));
		if (!((Result)Marshal.QueryInterface(comPointer, ref iid, out var ppv)).Failure)
		{
			return CppObject.FromPointerUnsafe<T>(ppv);
		}
		return null;
	}

	public virtual T QueryInterfaceOrNull<T>() where T : ComObject
	{
		return CppObject.FromPointer<T>(QueryInterfaceOrNull(Utilities.GetGuidFromType(typeof(T))));
	}

	public static explicit operator ComObject(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new ComObject(nativePointer);
		}
		return null;
	}

	protected void QueryInterfaceFrom<T>(T fromObject) where T : ComObject
	{
		fromObject.QueryInterface(Utilities.GetGuidFromType(GetType()), out var outPtr);
		base.NativePointer = outPtr;
	}

	Result IUnknown.QueryInterface(ref Guid guid, out IntPtr comObject)
	{
		return Marshal.QueryInterface(base.NativePointer, ref guid, out comObject);
	}

	int IUnknown.AddReference()
	{
		if (base.NativePointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("COM Object pointer is null");
		}
		return Marshal.AddRef(base.NativePointer);
	}

	int IUnknown.Release()
	{
		if (base.NativePointer == IntPtr.Zero)
		{
			throw new InvalidOperationException("COM Object pointer is null");
		}
		return Marshal.Release(base.NativePointer);
	}

	protected unsafe override void Dispose(bool disposing)
	{
		if (base.NativePointer != IntPtr.Zero)
		{
			if (!disposing && Configuration.EnableTrackingReleaseOnFinalizer)
			{
				ObjectTracker.Find(this);
				_ = Configuration.EnableReleaseOnFinalizer;
			}
			if (disposing || Configuration.EnableReleaseOnFinalizer)
			{
				((IUnknown)this).Release();
			}
			if (Configuration.EnableObjectTracking)
			{
				ObjectTracker.UnTrack(this);
			}
			_nativePointer = null;
		}
		base.Dispose(disposing);
	}

	protected override void NativePointerUpdating()
	{
		if (Configuration.EnableObjectTracking)
		{
			ObjectTracker.UnTrack(this);
		}
	}

	protected override void NativePointerUpdated(IntPtr oldNativePointer)
	{
		if (Configuration.EnableObjectTracking)
		{
			ObjectTracker.Track(this);
		}
	}
}
