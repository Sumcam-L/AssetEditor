using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IClassSet
{
	IEnumerable<IClassEntity> Items { get; }

	T Push<T>(string name) where T : IClassEntity;

	void Clear();

	void Remove(IClassEntity entity);

	IClassEntity FindForInstance(IInstanceEntity instanceEntity);

	IClassEntity FindByName(string name);

	List<IClassEntity> GetClasses();
}
