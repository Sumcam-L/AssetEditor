using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class ColorValueKnob : ValueKnobBase, IColorValueKnob, IValueKnob<Color>
{
	internal Color m_value;

	public virtual int GValue
	{
		get
		{
			return Value.G;
		}
		set
		{
			Color value2 = Value;
			int green = ((value > 255) ? 255 : value);
			Color value3 = Color.FromArgb(value2.R, green, value2.B);
			Value = value3;
		}
	}

	public virtual int BValue
	{
		get
		{
			return Value.B;
		}
		set
		{
			Color value2 = Value;
			int blue = ((value > 255) ? 255 : value);
			Color value3 = Color.FromArgb(value2.R, value2.G, blue);
			Value = value3;
		}
	}

	public virtual int RValue
	{
		get
		{
			return Value.R;
		}
		set
		{
			Color value2 = Value;
			Color value3 = Color.FromArgb((value > 255) ? 255 : value, value2.G, value2.B);
			Value = value3;
		}
	}

	public unsafe virtual Color Value
	{
		get
		{
			return m_value;
		}
		set
		{
			//IL_0071: Expected I, but got I8
			m_value = value;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector4 fGXVector);
			global::_003CModule_003E.FGXVector4_002E_007Bctor_007D(&fGXVector);
			*(float*)(&fGXVector) = (float)(int)value.R * 0.003921569f;
			System.Runtime.CompilerServices.Unsafe.As<FGXVector4, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 4)) = (float)(int)value.G * 0.003921569f;
			System.Runtime.CompilerServices.Unsafe.As<FGXVector4, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 8)) = (float)(int)value.B * 0.003921569f;
			System.Runtime.CompilerServices.Unsafe.As<FGXVector4, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXVector, 12)) = 1f;
			IKnobMessenger* pKnobMessenger = m_pKnobMessenger;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, sbyte*, FGXVector4, void>)(*(ulong*)(*(long*)pKnobMessenger + 8)))((nint)pKnobMessenger, GetNativeKnobName(), GetNativeGroupName(), fGXVector);
		}
	}

	public unsafe ColorValueKnob(IKnobMessenger* pMessenger, sbyte* pKnob, sbyte* pGroup)
		: base(KnobType.KT_VALUE_COLOR, pMessenger, pKnob, pGroup)
	{
	}

	public virtual Type GetValueType()
	{
		return typeof(Color);
	}

	public virtual void SetUIValue(Color value)
	{
		Value = value;
		RaiseHasUpdateEvent();
	}
}
