using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("fbec5e44-f7be-4b65-b7f8-c0c81fef026d")]
public class DevelopRaw : BitmapFrameDecode
{
	public PropertyBag CurrentParameterSet
	{
		get
		{
			GetCurrentParameterSet(out var currentParameterSetOut);
			return currentParameterSetOut;
		}
	}

	public double ExposureCompensation
	{
		get
		{
			GetExposureCompensation(out var eVRef);
			return eVRef;
		}
		set
		{
			SetExposureCompensation(value);
		}
	}

	public NamedWhitePoint NamedWhitePoint
	{
		get
		{
			GetNamedWhitePoint(out var whitePointRef);
			return whitePointRef;
		}
		set
		{
			SetNamedWhitePoint(value);
		}
	}

	public int WhitePointKelvin
	{
		get
		{
			GetWhitePointKelvin(out var whitePointKelvinRef);
			return whitePointKelvinRef;
		}
		set
		{
			SetWhitePointKelvin(value);
		}
	}

	public double Contrast
	{
		get
		{
			GetContrast(out var contrastRef);
			return contrastRef;
		}
		set
		{
			SetContrast(value);
		}
	}

	public double Gamma
	{
		get
		{
			GetGamma(out var gammaRef);
			return gammaRef;
		}
		set
		{
			SetGamma(value);
		}
	}

	public double Sharpness
	{
		get
		{
			GetSharpness(out var sharpnessRef);
			return sharpnessRef;
		}
		set
		{
			SetSharpness(value);
		}
	}

	public double Saturation
	{
		get
		{
			GetSaturation(out var saturationRef);
			return saturationRef;
		}
		set
		{
			SetSaturation(value);
		}
	}

	public double Tint
	{
		get
		{
			GetTint(out var tintRef);
			return tintRef;
		}
		set
		{
			SetTint(value);
		}
	}

	public double NoiseReduction
	{
		get
		{
			GetNoiseReduction(out var noiseReductionRef);
			return noiseReductionRef;
		}
		set
		{
			SetNoiseReduction(value);
		}
	}

	public ColorContext DestinationColorContext
	{
		set
		{
			SetDestinationColorContext(value);
		}
	}

	public double Rotation
	{
		get
		{
			GetRotation(out var rotationRef);
			return rotationRef;
		}
		set
		{
			SetRotation(value);
		}
	}

	public RawRenderMode RenderMode
	{
		get
		{
			GetRenderMode(out var renderModeRef);
			return renderModeRef;
		}
		set
		{
			SetRenderMode(value);
		}
	}

	internal DevelopRawNotificationCallback NotificationCallback
	{
		set
		{
			SetNotificationCallback(value);
		}
	}

	public DevelopRaw(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator DevelopRaw(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new DevelopRaw(nativePointer);
		}
		return null;
	}

	public unsafe void QueryRawCapabilitiesInfo(ref RawCapabilitiesInfo infoRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RawCapabilitiesInfo, IntPtr>(ref infoRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void LoadParameterSet(RawParameterSet parameterSet)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, (int)parameterSet)).CheckError();
	}

	internal unsafe void GetCurrentParameterSet(out PropertyBag currentParameterSetOut)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &zero);
		currentParameterSetOut = ((zero == IntPtr.Zero) ? null : new PropertyBag(zero));
		result.CheckError();
	}

	internal unsafe void SetExposureCompensation(double ev)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)14 * (nint)sizeof(void*))))(_nativePointer, ev)).CheckError();
	}

	internal unsafe void GetExposureCompensation(out double eVRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref eVRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)15 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void SetWhitePointRGB(int red, int green, int blue)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)16 * (nint)sizeof(void*))))(_nativePointer, red, green, blue)).CheckError();
	}

	public unsafe void GetWhitePointRGB(out int redRef, out int greenRef, out int blueRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref redRef))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref greenRef))
			{
				fixed (IntPtr* ptr3 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref blueRef))
				{
					result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)17 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2, ptr3);
				}
			}
		}
		result.CheckError();
	}

	internal unsafe void SetNamedWhitePoint(NamedWhitePoint whitePoint)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)18 * (nint)sizeof(void*))))(_nativePointer, (int)whitePoint)).CheckError();
	}

	internal unsafe void GetNamedWhitePoint(out NamedWhitePoint whitePointRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<NamedWhitePoint, IntPtr>(ref whitePointRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)19 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetWhitePointKelvin(int whitePointKelvin)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)20 * (nint)sizeof(void*))))(_nativePointer, whitePointKelvin)).CheckError();
	}

	internal unsafe void GetWhitePointKelvin(out int whitePointKelvinRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref whitePointKelvinRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)21 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	public unsafe void GetKelvinRangeInfo(out int minKelvinTempRef, out int maxKelvinTempRef, out int kelvinTempStepValueRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref minKelvinTempRef))
		{
			fixed (IntPtr* ptr2 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref maxKelvinTempRef))
			{
				fixed (IntPtr* ptr3 = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref kelvinTempStepValueRef))
				{
					result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)22 * (nint)sizeof(void*))))(_nativePointer, ptr, ptr2, ptr3);
				}
			}
		}
		result.CheckError();
	}

	internal unsafe void SetContrast(double contrast)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)23 * (nint)sizeof(void*))))(_nativePointer, contrast)).CheckError();
	}

	internal unsafe void GetContrast(out double contrastRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref contrastRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)24 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetGamma(double gamma)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)25 * (nint)sizeof(void*))))(_nativePointer, gamma)).CheckError();
	}

	internal unsafe void GetGamma(out double gammaRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref gammaRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)26 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetSharpness(double sharpness)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)27 * (nint)sizeof(void*))))(_nativePointer, sharpness)).CheckError();
	}

	internal unsafe void GetSharpness(out double sharpnessRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref sharpnessRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)28 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetSaturation(double saturation)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)29 * (nint)sizeof(void*))))(_nativePointer, saturation)).CheckError();
	}

	internal unsafe void GetSaturation(out double saturationRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref saturationRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)30 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetTint(double tint)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)31 * (nint)sizeof(void*))))(_nativePointer, tint)).CheckError();
	}

	internal unsafe void GetTint(out double tintRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref tintRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)32 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetNoiseReduction(double noiseReduction)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)33 * (nint)sizeof(void*))))(_nativePointer, noiseReduction)).CheckError();
	}

	internal unsafe void GetNoiseReduction(out double noiseReductionRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref noiseReductionRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)34 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetDestinationColorContext(ColorContext colorContextRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = colorContextRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)35 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}

	public unsafe void SetToneCurve(int toneCurveSize, RawToneCurve[] toneCurveRef)
	{
		RawToneCurve.__Native[] array = new RawToneCurve.__Native[toneCurveRef.Length];
		for (int i = 0; i < toneCurveRef.Length; i++)
		{
			toneCurveRef[i].__MarshalTo(ref array[i]);
		}
		Result result;
		fixed (IntPtr* ptr = array)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)36 * (nint)sizeof(void*))))(_nativePointer, toneCurveSize, ptr);
		}
		for (int j = 0; j < toneCurveRef.Length; j++)
		{
			toneCurveRef[j].__MarshalFree(ref array[j]);
		}
		result.CheckError();
	}

	public unsafe void GetToneCurve(int toneCurveBufferSize, RawToneCurve[] toneCurveRef, IntPtr actualToneCurveBufferSizeRef)
	{
		RawToneCurve.__Native[] array = ((toneCurveRef == null) ? null : new RawToneCurve.__Native[toneCurveRef.Length]);
		Result result;
		fixed (IntPtr* ptr = array)
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)37 * (nint)sizeof(void*))))(_nativePointer, toneCurveBufferSize, ptr, (void*)actualToneCurveBufferSizeRef);
		}
		for (int i = 0; i < toneCurveRef.Length; i++)
		{
			toneCurveRef[i].__MarshalFrom(ref array[i]);
		}
		result.CheckError();
	}

	internal unsafe void SetRotation(double rotation)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, double, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)38 * (nint)sizeof(void*))))(_nativePointer, rotation)).CheckError();
	}

	internal unsafe void GetRotation(out double rotationRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<double, IntPtr>(ref rotationRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)39 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetRenderMode(RawRenderMode renderMode)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)40 * (nint)sizeof(void*))))(_nativePointer, (int)renderMode)).CheckError();
	}

	internal unsafe void GetRenderMode(out RawRenderMode renderModeRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<RawRenderMode, IntPtr>(ref renderModeRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)41 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void SetNotificationCallback(DevelopRawNotificationCallback callbackRef)
	{
		void* nativePointer = _nativePointer;
		IntPtr intPtr = callbackRef?.NativePointer ?? IntPtr.Zero;
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)42 * (nint)sizeof(void*))))(nativePointer, (void*)intPtr)).CheckError();
	}
}
