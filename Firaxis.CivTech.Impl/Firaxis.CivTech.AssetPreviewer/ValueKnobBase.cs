using System.Runtime.InteropServices;
using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class ValueKnobBase : Knob
{
	private bool m_bReadOnly = false;

	public virtual bool IsReadOnly
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return m_bReadOnly;
		}
	}

	public unsafe ValueKnobBase(KnobType knobType, IKnobMessenger* pMessenger, sbyte* pKnob, sbyte* pGroup)
		: base(knobType, pMessenger, pKnob, pGroup)
	{
	}
}
