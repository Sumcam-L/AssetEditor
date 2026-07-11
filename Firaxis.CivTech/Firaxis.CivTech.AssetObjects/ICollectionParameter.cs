namespace Firaxis.CivTech.AssetObjects;

public interface ICollectionParameter : IParameter
{
	bool HasNamedEntries { get; set; }

	bool FixedSize { get; set; }

	IValue EntryDefaultValue { get; }

	IParameter EntryParameter { get; }

	ValueType EntryValueType { get; }

	InstanceType EntryObjectType { get; }

	ParameterType EntryParameterType { get; }
}
