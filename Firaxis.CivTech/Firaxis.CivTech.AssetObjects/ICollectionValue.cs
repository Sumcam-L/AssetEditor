using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface ICollectionValue : IValue
{
	IEnumerable<IValue> Items { get; }

	InstanceType EntryObjectType { get; }

	ValueType EntryValueType { get; }

	T Push<T>(string paramName) where T : IValue;

	T Push<T>(string paramName, InstanceType instanceType, string objectName) where T : IObjectValue;

	void Clear();

	void Remove(IValue value);
}
