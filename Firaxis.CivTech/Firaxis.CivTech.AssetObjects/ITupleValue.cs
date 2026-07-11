namespace Firaxis.CivTech.AssetObjects;

public interface ITupleValue : IValue
{
	IValueSet Elements { get; }
}
