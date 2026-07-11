using System;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class FloatValueKnob : ValueKnobBase, IValueKnob<float>
{
	internal float m_value = 0f;

	public unsafe virtual float Value
	{
		get
		{
			return m_value;
		}
		set
		{
			//IL_0028: Expected I, but got I8
			m_value = value;
			IKnobMessenger* pKnobMessenger = m_pKnobMessenger;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, sbyte*, float, void>)(*(ulong*)(*(long*)pKnobMessenger + 16)))((nint)pKnobMessenger, GetNativeKnobName(), GetNativeGroupName(), value);
		}
	}

	public unsafe FloatValueKnob(IKnobMessenger* pMessenger, sbyte* pKnob, sbyte* pGroup)
		: base(KnobType.KT_VALUE_FLOAT, pMessenger, pKnob, pGroup)
	{
	}

	public virtual Type GetValueType()
	{
		return typeof(float);
	}

	public virtual void SetUIValue(float value)
	{
		Value = value;
		RaiseHasUpdateEvent();
	}
}
