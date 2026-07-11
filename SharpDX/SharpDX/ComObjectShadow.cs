using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace SharpDX;

internal abstract class ComObjectShadow : CppObjectShadow
{
	internal class ComObjectVtbl : CppObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int QueryInterfaceDelegate(IntPtr thisObject, IntPtr guid, out IntPtr output);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int AddRefDelegate(IntPtr thisObject);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int ReleaseDelegate(IntPtr thisObject);

		public ComObjectVtbl(int numberOfCallbackMethods)
			: base(numberOfCallbackMethods + 3)
		{
			AddMethod(new QueryInterfaceDelegate(QueryInterfaceImpl));
			AddMethod(new AddRefDelegate(AddRefImpl));
			AddMethod(new ReleaseDelegate(ReleaseImpl));
		}

		protected unsafe static int QueryInterfaceImpl(IntPtr thisObject, IntPtr guid, out IntPtr output)
		{
			ComObjectShadow comObjectShadow = CppObjectShadow.ToShadow<ComObjectShadow>(thisObject);
			if (comObjectShadow == null)
			{
				output = IntPtr.Zero;
				return Result.NoInterface.Code;
			}
			return comObjectShadow.QueryInterfaceImpl(thisObject, ref *(Guid*)(void*)guid, out output);
		}

		protected static int AddRefImpl(IntPtr thisObject)
		{
			return CppObjectShadow.ToShadow<ComObjectShadow>(thisObject)?.AddRefImpl(thisObject) ?? 0;
		}

		protected static int ReleaseImpl(IntPtr thisObject)
		{
			return CppObjectShadow.ToShadow<ComObjectShadow>(thisObject)?.ReleaseImpl(thisObject) ?? 0;
		}
	}

	private int count = 1;

	public static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

	protected int QueryInterfaceImpl(IntPtr thisObject, ref Guid guid, out IntPtr output)
	{
		ComObjectShadow comObjectShadow = (ComObjectShadow)((ShadowContainer)base.Callback.Shadow).FindShadow(guid);
		if (comObjectShadow != null)
		{
			comObjectShadow.AddRefImpl(thisObject);
			output = comObjectShadow.NativePointer;
			return Result.Ok.Code;
		}
		output = IntPtr.Zero;
		return Result.NoInterface.Code;
	}

	protected virtual int AddRefImpl(IntPtr thisObject)
	{
		return Interlocked.Increment(ref count);
	}

	protected virtual int ReleaseImpl(IntPtr thisObject)
	{
		return Interlocked.Decrement(ref count);
	}
}
