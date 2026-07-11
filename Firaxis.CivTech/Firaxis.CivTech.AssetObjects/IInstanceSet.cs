using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IInstanceSet : IAssemblyInstance, IDisposable
{
	IEnumerable<IInstanceEntity> Items { get; }

	T LoadByName<T>(string entityName) where T : IInstanceEntity;

	T LoadByPath<T>(string entityPath) where T : IInstanceEntity;

	T Push<T>(string entityName) where T : IInstanceEntity;

	void Clear();

	void Remove(IInstanceEntity entity);

	void Remove(IEnumerable<EntityID> entityIDs);

	void RemoveExcept(IEnumerable<EntityID> entityIDs);

	IInstanceEntity FindByNameAndType(string name, InstanceType eType);

	IInstanceEntity FindByPath(string pmEntityPath);
}
