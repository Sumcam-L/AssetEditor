using System;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class FunctionKnob : Knob, IFunctionKnob
{
	public unsafe FunctionKnob(IKnobMessenger* pMessenger, sbyte* pKnobName, sbyte* pKnobGroup)
		: base(KnobType.KT_FUNCTION, pMessenger, pKnobName, pKnobGroup)
	{
	}

	public unsafe virtual void CallFunction()
	{
		//IL_0020: Expected I, but got I8
		IKnobMessenger* pKnobMessenger = m_pKnobMessenger;
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, sbyte*, void>)(*(ulong*)(*(long*)pKnobMessenger + 40)))((nint)pKnobMessenger, GetNativeKnobName(), GetNativeGroupName());
	}
}
