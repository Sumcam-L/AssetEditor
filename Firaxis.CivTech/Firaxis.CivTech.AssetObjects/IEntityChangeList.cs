using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IEntityChangeList : IAssemblyInstance, IDisposable
{
	IEnumerable<IEntityChangedEvent> EntityChanges { get; }

	T Push<T>(EntityID entity) where T : IEntityChangedEvent;

	void Clear();
}
