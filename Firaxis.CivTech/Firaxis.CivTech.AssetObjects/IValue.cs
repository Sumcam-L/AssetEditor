namespace Firaxis.CivTech.AssetObjects;

public interface IValue
{
	string ParameterName { get; }

	ValueType ParameterType { get; }

	bool Equals(IValue otherValue);

	void CopyDataFrom(IValue otherValue);
}
