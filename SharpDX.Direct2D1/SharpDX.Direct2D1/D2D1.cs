using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace SharpDX.Direct2D1;

internal static class D2D1
{
	public unsafe static Bool IsMatrixInvertible(ref Matrix3x2 matrix)
	{
		Bool result;
		fixed (IntPtr* arg = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref matrix))
		{
			result = D2D1IsMatrixInvertible_(arg);
		}
		return result;
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1IsMatrixInvertible")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern Bool D2D1IsMatrixInvertible_(void* arg0);

	public unsafe static Bool InvertMatrix(ref Matrix3x2 matrix)
	{
		Bool result;
		fixed (IntPtr* arg = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref matrix))
		{
			result = D2D1InvertMatrix_(arg);
		}
		return result;
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1InvertMatrix")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern Bool D2D1InvertMatrix_(void* arg0);

	public unsafe static void MakeSkewMatrix(float angleX, float angleY, Vector2 center, out Matrix3x2 matrix)
	{
		matrix = default(Matrix3x2);
		fixed (IntPtr* arg = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref matrix))
		{
			D2D1MakeSkewMatrix_(angleX, angleY, center, arg);
		}
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1MakeSkewMatrix")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern void D2D1MakeSkewMatrix_(float arg0, float arg1, Vector2 arg2, void* arg3);

	public unsafe static void CreateFactory(FactoryType factoryType, Guid riid, FactoryOptions? factoryOptionsRef, out IntPtr iFactoryOut)
	{
		FactoryOptions value = default(FactoryOptions);
		if (factoryOptionsRef.HasValue)
		{
			value = factoryOptionsRef.Value;
		}
		Result result;
		fixed (IntPtr* arg = &iFactoryOut)
		{
			result = D2D1CreateFactory_((int)factoryType, &riid, factoryOptionsRef.HasValue ? (&value) : ((void*)IntPtr.Zero), arg);
		}
		result.CheckError();
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1CreateFactory")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern int D2D1CreateFactory_(int arg0, void* arg1, void* arg2, void* arg3);

	public unsafe static void MakeRotateMatrix(float angle, Vector2 center, out Matrix3x2 matrix)
	{
		matrix = default(Matrix3x2);
		fixed (IntPtr* arg = &System.Runtime.CompilerServices.Unsafe.As<Matrix3x2, IntPtr>(ref matrix))
		{
			D2D1MakeRotateMatrix_(angle, center, arg);
		}
	}

	[DllImport("d2d1.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "D2D1MakeRotateMatrix")]
	[SuppressUnmanagedCodeSecurity]
	private unsafe static extern void D2D1MakeRotateMatrix_(float arg0, Vector2 arg1, void* arg2);
}
