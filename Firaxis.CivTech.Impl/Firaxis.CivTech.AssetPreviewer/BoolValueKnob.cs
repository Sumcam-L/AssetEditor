using System;
using System.Runtime.InteropServices;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class BoolValueKnob : ValueKnobBase, IValueKnob<bool>
{
	internal bool m_value = false;

	public unsafe virtual bool Value
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return m_value;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			//IL_0028: Expected I, but got I8
			m_value = value;
			IKnobMessenger* pKnobMessenger = m_pKnobMessenger;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, sbyte*, byte, void>)(*(ulong*)(*(long*)pKnobMessenger + 32)))((nint)pKnobMessenger, GetNativeKnobName(), GetNativeGroupName(), value ? ((byte)1) : ((byte)0));
		}
	}

	public unsafe BoolValueKnob(IKnobMessenger* pMessenger, sbyte* pKnob, sbyte* pGroup)
		: base(KnobType.KT_VALUE_BOOL, pMessenger, pKnob, pGroup)
	{
	}

	public virtual Type GetValueType()
	{
		return typeof(bool);
	}

	public virtual void SetUIValue([MarshalAs(UnmanagedType.U1)] bool value)
	{
		Value = value;
		RaiseHasUpdateEvent();
	}
}
