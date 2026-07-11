using System;
using System.Runtime.InteropServices;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class StringValueKnob : ValueKnobBase, IValueKnob<string>
{
	internal string m_value = string.Empty;

	public unsafe virtual string Value
	{
		get
		{
			return m_value;
		}
		set
		{
			//IL_0034: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			m_value = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				IKnobMessenger* pKnobMessenger = m_pKnobMessenger;
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, sbyte*, sbyte*, void>)(*(ulong*)(*(ulong*)pKnobMessenger)))((nint)pKnobMessenger, GetNativeKnobName(), GetNativeGroupName(), standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}

	public unsafe StringValueKnob(IKnobMessenger* pMessenger, sbyte* pKnob, sbyte* pGroup)
		: base(KnobType.KT_VALUE_STRING, pMessenger, pKnob, pGroup)
	{
	}

	public virtual Type GetValueType()
	{
		return typeof(string);
	}

	public virtual void SetUIValue(string value)
	{
		Value = value;
		RaiseHasUpdateEvent();
	}

	internal unsafe void SetValue(sbyte* pValue)
	{
		IntPtr ptr = new IntPtr(pValue);
		m_value = Marshal.PtrToStringAnsi(ptr);
	}
}
