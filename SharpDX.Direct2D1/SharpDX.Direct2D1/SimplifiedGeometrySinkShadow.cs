using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

internal class SimplifiedGeometrySinkShadow : ComObjectShadow
{
	public class SimplifiedGeometrySinkVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void SetFillModeDelegate(IntPtr thisPtr, FillMode fillMode);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void SetSegmentFlagsDelegate(IntPtr thisPtr, PathSegment vertexFlags);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void BeginFigureDelegate(IntPtr thisPtr, Vector2 startPoint, FigureBegin figureBegin);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddLinesDelegate(IntPtr thisPtr, IntPtr points, int pointsCount);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddBeziersDelegate(IntPtr thisPtr, IntPtr beziers, int beziersCount);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void EndFigureDelegate(IntPtr thisPtr, FigureEnd figureEnd);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int CloseDelegate(IntPtr thisPtr);

		public SimplifiedGeometrySinkVtbl(int nbMethods)
			: base(nbMethods + 7)
		{
			AddMethod(new SetFillModeDelegate(SetFillModeImpl));
			AddMethod(new SetSegmentFlagsDelegate(SetSegmentFlagsImpl));
			AddMethod(new BeginFigureDelegate(BeginFigureImpl));
			AddMethod(new AddLinesDelegate(AddLinesImpl));
			AddMethod(new AddBeziersDelegate(AddBeziersImpl));
			AddMethod(new EndFigureDelegate(EndFigureImpl));
			AddMethod(new CloseDelegate(CloseImpl));
		}

		private static void SetFillModeImpl(IntPtr thisPtr, FillMode fillMode)
		{
			SimplifiedGeometrySinkShadow simplifiedGeometrySinkShadow = CppObjectShadow.ToShadow<SimplifiedGeometrySinkShadow>(thisPtr);
			SimplifiedGeometrySink simplifiedGeometrySink = (SimplifiedGeometrySink)simplifiedGeometrySinkShadow.Callback;
			simplifiedGeometrySink.SetFillMode(fillMode);
		}

		private static void SetSegmentFlagsImpl(IntPtr thisPtr, PathSegment vertexFlags)
		{
			SimplifiedGeometrySinkShadow simplifiedGeometrySinkShadow = CppObjectShadow.ToShadow<SimplifiedGeometrySinkShadow>(thisPtr);
			SimplifiedGeometrySink simplifiedGeometrySink = (SimplifiedGeometrySink)simplifiedGeometrySinkShadow.Callback;
			simplifiedGeometrySink.SetSegmentFlags(vertexFlags);
		}

		private static void BeginFigureImpl(IntPtr thisPtr, Vector2 startPoint, FigureBegin figureBegin)
		{
			SimplifiedGeometrySinkShadow simplifiedGeometrySinkShadow = CppObjectShadow.ToShadow<SimplifiedGeometrySinkShadow>(thisPtr);
			SimplifiedGeometrySink simplifiedGeometrySink = (SimplifiedGeometrySink)simplifiedGeometrySinkShadow.Callback;
			simplifiedGeometrySink.BeginFigure(startPoint, figureBegin);
		}

		private static void AddLinesImpl(IntPtr thisPtr, IntPtr points, int pointsCount)
		{
			SimplifiedGeometrySinkShadow simplifiedGeometrySinkShadow = CppObjectShadow.ToShadow<SimplifiedGeometrySinkShadow>(thisPtr);
			SimplifiedGeometrySink simplifiedGeometrySink = (SimplifiedGeometrySink)simplifiedGeometrySinkShadow.Callback;
			Vector2[] array = new Vector2[pointsCount];
			Utilities.Read(points, array, 0, pointsCount);
			simplifiedGeometrySink.AddLines(array);
		}

		private static void AddBeziersImpl(IntPtr thisPtr, IntPtr beziers, int beziersCount)
		{
			SimplifiedGeometrySinkShadow simplifiedGeometrySinkShadow = CppObjectShadow.ToShadow<SimplifiedGeometrySinkShadow>(thisPtr);
			SimplifiedGeometrySink simplifiedGeometrySink = (SimplifiedGeometrySink)simplifiedGeometrySinkShadow.Callback;
			BezierSegment[] array = new BezierSegment[beziersCount];
			Utilities.Read(beziers, array, 0, beziersCount);
			simplifiedGeometrySink.AddBeziers(array);
		}

		private static void EndFigureImpl(IntPtr thisPtr, FigureEnd figureEnd)
		{
			SimplifiedGeometrySinkShadow simplifiedGeometrySinkShadow = CppObjectShadow.ToShadow<SimplifiedGeometrySinkShadow>(thisPtr);
			SimplifiedGeometrySink simplifiedGeometrySink = (SimplifiedGeometrySink)simplifiedGeometrySinkShadow.Callback;
			simplifiedGeometrySink.EndFigure(figureEnd);
		}

		private static int CloseImpl(IntPtr thisPtr)
		{
			try
			{
				SimplifiedGeometrySinkShadow simplifiedGeometrySinkShadow = CppObjectShadow.ToShadow<SimplifiedGeometrySinkShadow>(thisPtr);
				SimplifiedGeometrySink simplifiedGeometrySink = (SimplifiedGeometrySink)simplifiedGeometrySinkShadow.Callback;
				simplifiedGeometrySink.Close();
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly SimplifiedGeometrySinkVtbl Vtbl = new SimplifiedGeometrySinkVtbl(0);

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(SimplifiedGeometrySink callback)
	{
		return CppObject.ToCallbackPtr<SimplifiedGeometrySink>(callback);
	}
}
