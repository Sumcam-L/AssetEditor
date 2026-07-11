using AssetPreviewer;

namespace Firaxis.CivTech.AssetPreviewer;

public class FloatRangeKnob : FloatValueKnob, IRangeKnob<float>
{
	private float m_Min = 0f;

	private float m_Max = 1f;

	public virtual float MaxValue => m_Max;

	public virtual float MinValue => m_Min;

	public unsafe FloatRangeKnob(IKnobMessenger* pkKnob, sbyte* pKnob, sbyte* pGroup)
		: base(KnobType.KT_VALUE_FLOAT, pkKnob, pKnob, pGroup)
	{
	}

	public void SetRange(float Min, float Max)
	{
		m_Min = Min;
		m_Max = Max;
	}
}
