namespace Firaxis.ATF;

public interface IValueProvider<T>
{
	T Value { get; set; }
}
