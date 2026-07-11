namespace Firaxis.ATF;

public interface IRangeProvider<T>
{
	T MaxValue { get; }

	T MinValue { get; }
}
