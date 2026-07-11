namespace Firaxis.CivTech.AssetPreviewer;

public static class IKnobExtensions
{
	public static void SetValue<T>(this IKnob knob, T data)
	{
		if (knob is IValueKnob<T> valueKnob)
		{
			valueKnob.SetUIValue(data);
		}
	}
}
