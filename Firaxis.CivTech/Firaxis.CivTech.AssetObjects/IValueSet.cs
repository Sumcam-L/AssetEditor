using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IValueSet : IAssemblyInstance, IDisposable
{
	IEnumerable<IValue> Items { get; }

	T Push<T>(string paramName) where T : IValue;

	T Push<T>(string paramName, InstanceType instanceType, string objectName) where T : IObjectValue;

	T PushCollection<T>(string paramName, InstanceType instanceType) where T : IObjectCollectionValue;

	void Clear();

	void Remove(IValue value);

	IValue FindValue(string paramName);

	void RemoveAllValues();

	string SerializeIntoXML();

	bool DeserializeFromXML(string xml);

	bool UpdateFromXML(string pmXml);

	void RemoveUnusedValues(IParameterSet parameters);

	void AddDefaultValuesAsNecessary(IParameterSet parameters);

	void CopyFrom(IValueSet other);
}
