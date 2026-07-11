namespace Firaxis.CivTech.AssetPreviewer;

public interface IRangeKnob<T> : IValueKnob<T>, IValueKnobBase, IKnob
{
	T MinValue { get; }

	T MaxValue { get; }
}
