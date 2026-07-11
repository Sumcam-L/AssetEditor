using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

internal class GeometrySinkShadow : SimplifiedGeometrySinkShadow
{
	private class GeometrySinkVtbl : SimplifiedGeometrySinkVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddLineDelegate(IntPtr thisPtr, Vector2 point);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddBezierDelegate(IntPtr thisPtr, IntPtr bezier);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddQuadraticBezierDelegate(IntPtr thisPtr, IntPtr bezier);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddQuadraticBeziersDelegate(IntPtr thisPtr, IntPtr beziers, int beziersCount);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void AddArcDelegate(IntPtr thisPtr, IntPtr arc);

		public GeometrySinkVtbl()
			: base(5)
		{
			AddMethod(new AddLineDelegate(AddLineImpl));
			AddMethod(new AddBezierDelegate(AddBezierImpl));
			AddMethod(new AddQuadraticBezierDelegate(AddQuadraticBezierImpl));
			AddMethod(new AddQuadraticBeziersDelegate(AddQuadraticBeziersImpl));
			AddMethod(new AddArcDelegate(AddArcImpl));
		}

		private static void AddLineImpl(IntPtr thisPtr, Vector2 point)
		{
			GeometrySinkShadow geometrySinkShadow = CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr);
			GeometrySink geometrySink = (GeometrySink)geometrySinkShadow.Callback;
			geometrySink.AddLine(point);
		}

		private unsafe static void AddBezierImpl(IntPtr thisPtr, IntPtr bezier)
		{
			GeometrySinkShadow geometrySinkShadow = CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr);
			GeometrySink geometrySink = (GeometrySink)geometrySinkShadow.Callback;
			geometrySink.AddBezier(*(BezierSegment*)(void*)bezier);
		}

		private unsafe static void AddQuadraticBezierImpl(IntPtr thisPtr, IntPtr bezier)
		{
			GeometrySinkShadow geometrySinkShadow = CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr);
			GeometrySink geometrySink = (GeometrySink)geometrySinkShadow.Callback;
			geometrySink.AddQuadraticBezier(*(QuadraticBezierSegment*)(void*)bezier);
		}

		private static void AddQuadraticBeziersImpl(IntPtr thisPtr, IntPtr beziers, int beziersCount)
		{
			GeometrySinkShadow geometrySinkShadow = CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr);
			GeometrySink geometrySink = (GeometrySink)geometrySinkShadow.Callback;
			QuadraticBezierSegment[] array = new QuadraticBezierSegment[beziersCount];
			Utilities.Read(beziers, array, 0, beziersCount);
			geometrySink.AddQuadraticBeziers(array);
		}

		private unsafe static void AddArcImpl(IntPtr thisPtr, IntPtr arc)
		{
			GeometrySinkShadow geometrySinkShadow = CppObjectShadow.ToShadow<GeometrySinkShadow>(thisPtr);
			GeometrySink geometrySink = (GeometrySink)geometrySinkShadow.Callback;
			geometrySink.AddArc(*(ArcSegment*)(void*)arc);
		}
	}

	private static readonly GeometrySinkVtbl Vtbl = new GeometrySinkVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(GeometrySink geometrySink)
	{
		return CppObject.ToCallbackPtr<GeometrySink>(geometrySink);
	}
}
