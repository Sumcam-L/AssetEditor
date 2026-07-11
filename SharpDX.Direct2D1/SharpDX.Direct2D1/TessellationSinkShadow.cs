using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

internal class TessellationSinkShadow : ComObjectShadow
{
	public class TessellationSinkVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddTrianglesDelegate(IntPtr thisPtr, IntPtr triangles, int trianglesCount);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int CloseDelegate(IntPtr thisPtr);

		public TessellationSinkVtbl()
			: base(2)
		{
			AddMethod(new AddTrianglesDelegate(AddTrianglesImpl));
			AddMethod(new CloseDelegate(CloseImpl));
		}

		private static void AddTrianglesImpl(IntPtr thisPtr, IntPtr triangles, int trianglesCount)
		{
			TessellationSinkShadow tessellationSinkShadow = CppObjectShadow.ToShadow<TessellationSinkShadow>(thisPtr);
			TessellationSink tessellationSink = (TessellationSink)tessellationSinkShadow.Callback;
			Triangle[] array = new Triangle[trianglesCount];
			Utilities.Read(triangles, array, 0, trianglesCount);
			tessellationSink.AddTriangles(array);
		}

		private static int CloseImpl(IntPtr thisPtr)
		{
			try
			{
				TessellationSinkShadow tessellationSinkShadow = CppObjectShadow.ToShadow<TessellationSinkShadow>(thisPtr);
				TessellationSink tessellationSink = (TessellationSink)tessellationSinkShadow.Callback;
				tessellationSink.Close();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly TessellationSinkVtbl Vtbl = new TessellationSinkVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(TessellationSink tessellationSink)
	{
		return CppObject.ToCallbackPtr<TessellationSink>(tessellationSink);
	}
}
