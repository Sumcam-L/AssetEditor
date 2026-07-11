namespace Firaxis.CivTech.AssetPreviewer;

public interface IValueKnob<T> : IValueKnobBase, IKnob
{
	T Value { get; set; }

	void SetUIValue(T value);
}
