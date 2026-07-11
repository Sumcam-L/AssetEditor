using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IParameterSet
{
	IEnumerable<IParameter> Items { get; }

	T Push<T>(string paramName) where T : IParameter;

	T Push<T>(string name, InstanceType instanceType) where T : IObjectParameter;

	T PushCollection<T>(string name, InstanceType instanceType) where T : IObjectCollectionParameter;

	void Clear();

	void Remove(IParameter parameter);

	IParameter FindByName(string paramName);

	List<IParameter> GetParameters();
}
