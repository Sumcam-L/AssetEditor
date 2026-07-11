using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal abstract class PixelSnappingShadow : ComObjectShadow
{
	public class PixelSnappingVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int IsPixelSnappingDisabledDelegate(IntPtr thisPtr, IntPtr clientDrawingContext, out int isDisabled);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetCurrentTransformDelegate(IntPtr thisPtr, IntPtr clientDrawingContext, IntPtr transform);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetPixelsPerDipDelegate(IntPtr thisPtr, IntPtr clientDrawingContext, out float pixelPerDip);

		protected PixelSnappingVtbl(int nbMethods)
			: base(nbMethods + 3)
		{
			AddMethod(new IsPixelSnappingDisabledDelegate(IsPixelSnappingDisabledImpl));
			AddMethod(new GetCurrentTransformDelegate(GetCurrentTransformImpl));
			AddMethod(new GetPixelsPerDipDelegate(GetPixelsPerDipImpl));
		}

		private static int IsPixelSnappingDisabledImpl(IntPtr thisPtr, IntPtr clientDrawingContextPtr, out int isDisabled)
		{
			isDisabled = 0;
			try
			{
				PixelSnappingShadow pixelSnappingShadow = CppObjectShadow.ToShadow<PixelSnappingShadow>(thisPtr);
				PixelSnapping pixelSnapping = (PixelSnapping)pixelSnappingShadow.Callback;
				isDisabled = (pixelSnapping.IsPixelSnappingDisabled(GCHandle.FromIntPtr(clientDrawingContextPtr).Target) ? 1 : 0);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int GetCurrentTransformImpl(IntPtr thisPtr, IntPtr clientDrawingContextPtr, IntPtr transform)
		{
			try
			{
				PixelSnappingShadow pixelSnappingShadow = CppObjectShadow.ToShadow<PixelSnappingShadow>(thisPtr);
				PixelSnapping pixelSnapping = (PixelSnapping)pixelSnappingShadow.Callback;
				Matrix3x2 data = pixelSnapping.GetCurrentTransform(GCHandle.FromIntPtr(clientDrawingContextPtr).Target);
				Utilities.Write(transform, ref data);
			}
			catch (SharpDXException ex)
			{
				return ex.ResultCode.Code;
			}
			catch (Exception)
			{
				return Result.Fail.Code;
			}
			return Result.Ok.Code;
		}

		private static int GetPixelsPerDipImpl(IntPtr thisPtr, IntPtr clientDrawingContextPtr, out float pixelPerDip)
		{
			pixelPerDip = 0f;
			try
			{
				PixelSnappingShadow pixelSnappingShadow = CppObjectShadow.ToShadow<PixelSnappingShadow>(thisPtr);
				PixelSnapping pixelSnapping = (PixelSnapping)pixelSnappingShadow.Callback;
				pixelPerDip = pixelSnapping.GetPixelsPerDip(GCHandle.FromIntPtr(clientDrawingContextPtr).Target);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}
}
