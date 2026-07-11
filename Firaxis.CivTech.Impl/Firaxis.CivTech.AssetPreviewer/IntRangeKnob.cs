using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class IntRangeKnob : IntValueKnob, IRangeKnob<int>
{
	private int m_Min = 0;

	private int m_Max = 1;

	public virtual int MaxValue => m_Max;

	public virtual int MinValue => m_Min;

	public unsafe IntRangeKnob(IKnobMessenger* pkKnob, sbyte* pKnob, sbyte* pGroup)
		: base(KnobType.KT_VALUE_INT, pkKnob, pKnob, pGroup)
	{
	}

	internal void SetRange(int Min, int Max)
	{
		m_Min = Min;
		m_Max = Max;
	}
}
