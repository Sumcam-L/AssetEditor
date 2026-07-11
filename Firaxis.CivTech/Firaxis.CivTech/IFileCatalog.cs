using System;

namespace Firaxis.CivTech;

public interface IFileCatalog<TKey>
{
	int Count { get; }

	bool ContainsKey(TKey fileKey);

	void Remove(TKey fileKey);

	bool AddOrUpdate(TKey fileKey, DepotFileInfo info);

	bool TryGetValue(TKey fileKey, out DepotFileInfo info);

	void ForEachKey(Action<TKey> visit);

	void ForEachValue(Action<DepotFileInfo> visit);

	void ParallelForEachValue(Action<DepotFileInfo> visit);

	void ForEachValue(Func<DepotFileInfo, bool> predicate, Action<DepotFileInfo> visit);
}
