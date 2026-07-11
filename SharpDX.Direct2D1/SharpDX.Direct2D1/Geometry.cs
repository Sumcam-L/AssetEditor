using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906a1-12e2-11dc-9fed-001143a055f9")]
public class Geometry : Resource
{
	public const float DefaultFlatteningTolerance = 0.25f;

	private float _flatteningTolerance = 0.25f;

	public float FlatteningTolerance
	{
		get
		{
			return _flatteningTolerance;
		}
		set
		{
			_flatteningTolerance = value;
		}
	}

	public Geometry(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Geometry(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Geometry(nativePointer);
		}
		return null;
	}

	public unsafe RectangleF GetBounds(Matrix3x2? worldTransform)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		RectangleF result = default(RectangleF);
		void* nativePointer = _nativePointer;
		void* intPtr = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(nativePointer, intPtr, &result)).CheckError();
		return result;
	}

	public unsafe RectangleF GetWidenedBounds(float strokeWidth, StrokeStyle strokeStyle, Matrix3x2? worldTransform, float flatteningTolerance)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		RectangleF result = default(RectangleF);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(strokeStyle?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, float, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(nativePointer, strokeWidth, intPtr, intPtr2, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe Bool StrokeContainsPoint(Vector2 point, float strokeWidth, StrokeStyle strokeStyle, Matrix3x2? worldTransform, float flatteningTolerance)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		Bool result = default(Bool);
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(strokeStyle?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, Vector2, float, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(nativePointer, point, strokeWidth, intPtr, intPtr2, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe Bool FillContainsPoint(Vector2 point, Matrix3x2? worldTransform, float flatteningTolerance)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		Bool result = default(Bool);
		void* nativePointer = _nativePointer;
		void* intPtr = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, Vector2, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(nativePointer, point, intPtr, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe GeometryRelation Compare(Geometry inputGeometry, Matrix3x2? inputGeometryTransform, float flatteningTolerance)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (inputGeometryTransform.HasValue)
		{
			value = inputGeometryTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(inputGeometry?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (inputGeometryTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		GeometryRelation result = default(GeometryRelation);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(nativePointer, intPtr, intPtr2, flatteningTolerance, &result)).CheckError();
		return result;
	}

	internal unsafe void Simplify_(GeometrySimplificationOption simplificationOption, Matrix3x2? worldTransform, float flatteningTolerance, IntPtr geometrySink)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, int, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(nativePointer, (int)simplificationOption, intPtr, flatteningTolerance, (void*)geometrySink)).CheckError();
	}

	internal unsafe void Tessellate_(Matrix3x2? worldTransform, float flatteningTolerance, IntPtr tessellationSink)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(nativePointer, intPtr, flatteningTolerance, (void*)tessellationSink)).CheckError();
	}

	internal unsafe void Combine_(Geometry inputGeometry, CombineMode combineMode, Matrix3x2? inputGeometryTransform, float flatteningTolerance, IntPtr geometrySink)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (inputGeometryTransform.HasValue)
		{
			value = inputGeometryTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(inputGeometry?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (inputGeometryTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(nativePointer, intPtr, (int)combineMode, intPtr2, flatteningTolerance, (void*)geometrySink)).CheckError();
	}

	internal unsafe void Outline_(Matrix3x2? worldTransform, float flatteningTolerance, IntPtr geometrySink)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(nativePointer, intPtr, flatteningTolerance, (void*)geometrySink)).CheckError();
	}

	public unsafe float ComputeArea(Matrix3x2? worldTransform, float flatteningTolerance)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		float result = default(float);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(nativePointer, intPtr, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe float ComputeLength(Matrix3x2? worldTransform, float flatteningTolerance)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		float result = default(float);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(nativePointer, intPtr, flatteningTolerance, &result)).CheckError();
		return result;
	}

	public unsafe Vector2 ComputePointAtLength(float length, Matrix3x2? worldTransform, float flatteningTolerance, out Vector2 unitTangentVector)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		Vector2 result = default(Vector2);
		unitTangentVector = default(Vector2);
		Result result2;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Vector2, IntPtr>(ref unitTangentVector))
		{
			void* nativePointer = _nativePointer;
			void* intPtr = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
			result2 = ((delegate* unmanaged[Stdcall]<void*, float, void*, float, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(nativePointer, length, intPtr, flatteningTolerance, &result, ptr);
		}
		result2.CheckError();
		return result;
	}

	internal unsafe void Widen_(float strokeWidth, StrokeStyle strokeStyle, Matrix3x2? worldTransform, float flatteningTolerance, IntPtr geometrySink)
	{
		Matrix3x2 value = default(Matrix3x2);
		if (worldTransform.HasValue)
		{
			value = worldTransform.Value;
		}
		void* nativePointer = _nativePointer;
		void* intPtr = (void*)(strokeStyle?.NativePointer ?? IntPtr.Zero);
		void* intPtr2 = (worldTransform.HasValue ? (&value) : ((void*)IntPtr.Zero));
		((Result)((delegate* unmanaged[Stdcall]<void*, float, void*, void*, float, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(nativePointer, strokeWidth, intPtr, intPtr2, flatteningTolerance, (void*)geometrySink)).CheckError();
	}

	public void Combine(Geometry inputGeometry, CombineMode combineMode, GeometrySink geometrySink)
	{
		Combine(inputGeometry, combineMode, null, FlatteningTolerance, geometrySink);
	}

	public void Combine(Geometry inputGeometry, CombineMode combineMode, float flatteningTolerance, GeometrySink geometrySink)
	{
		Combine(inputGeometry, combineMode, null, flatteningTolerance, geometrySink);
	}

	public void Combine(Geometry inputGeometry, CombineMode combineMode, Matrix3x2? inputGeometryTransform, float flatteningTolerance, GeometrySink geometrySink)
	{
		Combine_(inputGeometry, combineMode, inputGeometryTransform, flatteningTolerance, GeometrySinkShadow.ToIntPtr(geometrySink));
	}

	public GeometryRelation Compare(Geometry inputGeometry)
	{
		return Compare(inputGeometry, null, FlatteningTolerance);
	}

	public GeometryRelation Compare(Geometry inputGeometry, float flatteningTolerance)
	{
		return Compare(inputGeometry, null, flatteningTolerance);
	}

	public float ComputeArea()
	{
		return ComputeArea(null, FlatteningTolerance);
	}

	public float ComputeArea(float flatteningTolerance)
	{
		return ComputeArea(null, flatteningTolerance);
	}

	public float ComputeLength()
	{
		return ComputeLength(null, FlatteningTolerance);
	}

	public float ComputeLength(float flatteningTolerance)
	{
		return ComputeLength(null, flatteningTolerance);
	}

	public Vector2 ComputePointAtLength(float length, out Vector2 unitTangentVector)
	{
		return ComputePointAtLength(length, null, FlatteningTolerance, out unitTangentVector);
	}

	public Vector2 ComputePointAtLength(float length, float flatteningTolerance, out Vector2 unitTangentVector)
	{
		return ComputePointAtLength(length, null, flatteningTolerance, out unitTangentVector);
	}

	public bool FillContainsPoint(Point point)
	{
		return FillContainsPoint(new Vector2(point.X, point.Y), null, FlatteningTolerance);
	}

	public bool FillContainsPoint(Vector2 point)
	{
		return FillContainsPoint(point, null, FlatteningTolerance);
	}

	public bool FillContainsPoint(Point point, float flatteningTolerance)
	{
		return FillContainsPoint(new Vector2(point.X, point.Y), null, flatteningTolerance);
	}

	public bool FillContainsPoint(Vector2 point, float flatteningTolerance)
	{
		return FillContainsPoint(point, null, flatteningTolerance);
	}

	public bool FillContainsPoint(Point point, Matrix3x2 worldTransform, float flatteningTolerance)
	{
		return FillContainsPoint(new Vector2(point.X, point.Y), worldTransform, flatteningTolerance);
	}

	public RectangleF GetBounds()
	{
		return GetBounds(null);
	}

	public RectangleF GetWidenedBounds(float strokeWidth)
	{
		return GetWidenedBounds(strokeWidth, null, null, FlatteningTolerance);
	}

	public RectangleF GetWidenedBounds(float strokeWidth, float flatteningTolerance)
	{
		return GetWidenedBounds(strokeWidth, null, null, flatteningTolerance);
	}

	public RectangleF GetWidenedBounds(float strokeWidth, StrokeStyle strokeStyle, float flatteningTolerance)
	{
		return GetWidenedBounds(strokeWidth, strokeStyle, null, flatteningTolerance);
	}

	public void Outline(GeometrySink geometrySink)
	{
		Outline(null, FlatteningTolerance, geometrySink);
	}

	public void Outline(float flatteningTolerance, GeometrySink geometrySink)
	{
		Outline(null, flatteningTolerance, geometrySink);
	}

	public void Outline(Matrix3x2? worldTransform, float flatteningTolerance, GeometrySink geometrySink)
	{
		Outline_(worldTransform, flatteningTolerance, GeometrySinkShadow.ToIntPtr(geometrySink));
	}

	public void Simplify(GeometrySimplificationOption simplificationOption, SimplifiedGeometrySink geometrySink)
	{
		Simplify(simplificationOption, null, FlatteningTolerance, geometrySink);
	}

	public void Simplify(GeometrySimplificationOption simplificationOption, float flatteningTolerance, SimplifiedGeometrySink geometrySink)
	{
		Simplify(simplificationOption, null, flatteningTolerance, geometrySink);
	}

	public void Simplify(GeometrySimplificationOption simplificationOption, Matrix3x2? worldTransform, float flatteningTolerance, SimplifiedGeometrySink geometrySink)
	{
		Simplify_(simplificationOption, worldTransform, flatteningTolerance, SimplifiedGeometrySinkShadow.ToIntPtr(geometrySink));
	}

	public bool StrokeContainsPoint(Point point, float strokeWidth)
	{
		return StrokeContainsPoint(point, strokeWidth, null);
	}

	public bool StrokeContainsPoint(Vector2 point, float strokeWidth)
	{
		return StrokeContainsPoint(point, strokeWidth, null);
	}

	public bool StrokeContainsPoint(Point point, float strokeWidth, StrokeStyle strokeStyle)
	{
		return StrokeContainsPoint(new Vector2(point.X, point.Y), strokeWidth, strokeStyle);
	}

	public bool StrokeContainsPoint(Vector2 point, float strokeWidth, StrokeStyle strokeStyle)
	{
		return StrokeContainsPoint(point, strokeWidth, strokeStyle, null, FlatteningTolerance);
	}

	public bool StrokeContainsPoint(Point point, float strokeWidth, StrokeStyle strokeStyle, Matrix3x2 transform)
	{
		return StrokeContainsPoint(point, strokeWidth, strokeStyle, transform, FlatteningTolerance);
	}

	public bool StrokeContainsPoint(Vector2 point, float strokeWidth, StrokeStyle strokeStyle, Matrix3x2 transform)
	{
		return StrokeContainsPoint(point, strokeWidth, strokeStyle, transform, FlatteningTolerance);
	}

	public bool StrokeContainsPoint(Point point, float strokeWidth, StrokeStyle strokeStyle, Matrix3x2 transform, float flatteningTolerance)
	{
		return StrokeContainsPoint(new Vector2(point.X, point.Y), strokeWidth, strokeStyle, transform, flatteningTolerance);
	}

	public void Tessellate(TessellationSink tessellationSink)
	{
		Tessellate(null, FlatteningTolerance, tessellationSink);
	}

	public void Tessellate(float flatteningTolerance, TessellationSink tessellationSink)
	{
		Tessellate(null, flatteningTolerance, tessellationSink);
	}

	public void Tessellate(Matrix3x2? worldTransform, float flatteningTolerance, TessellationSink tessellationSink)
	{
		Tessellate_(worldTransform, flatteningTolerance, TessellationSinkShadow.ToIntPtr(tessellationSink));
	}

	public void Widen(float strokeWidth, GeometrySink geometrySink)
	{
		Widen(strokeWidth, null, null, FlatteningTolerance, geometrySink);
	}

	public void Widen(float strokeWidth, float flatteningTolerance, GeometrySink geometrySink)
	{
		Widen(strokeWidth, null, null, flatteningTolerance, geometrySink);
	}

	public void Widen(float strokeWidth, StrokeStyle strokeStyle, float flatteningTolerance, GeometrySink geometrySink)
	{
		Widen(strokeWidth, strokeStyle, null, flatteningTolerance, geometrySink);
	}

	public void Widen(float strokeWidth, StrokeStyle strokeStyle, Matrix3x2? worldTransform, float flatteningTolerance, GeometrySink geometrySink)
	{
		Widen_(strokeWidth, strokeStyle, worldTransform, flatteningTolerance, GeometrySinkShadow.ToIntPtr(geometrySink));
	}
}
