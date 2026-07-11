namespace Firaxis.CivTech.AssetObjects;

public interface IParameter
{
	string Name { get; }

	string Category { get; set; }

	string Description { get; set; }

	ValueType ParameterValueType { get; }

	ParameterType ParameterType { get; }

	IValue DefaultValue { get; }
}
