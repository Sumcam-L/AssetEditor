using System;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class IntValueKnob : ValueKnobBase, IValueKnob<int>
{
	internal int m_value = 0;

	public unsafe virtual int Value
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
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, sbyte*, int, void>)(*(ulong*)(*(long*)pKnobMessenger + 24)))((nint)pKnobMessenger, GetNativeKnobName(), GetNativeGroupName(), value);
		}
	}

	public unsafe IntValueKnob(IKnobMessenger* pMessenger, sbyte* pKnob, sbyte* pGroup)
		: base(KnobType.KT_VALUE_INT, pMessenger, pKnob, pGroup)
	{
	}

	public virtual Type GetValueType()
	{
		return typeof(int);
	}

	public virtual void SetUIValue(int value)
	{
		Value = value;
		RaiseHasUpdateEvent();
	}
}
