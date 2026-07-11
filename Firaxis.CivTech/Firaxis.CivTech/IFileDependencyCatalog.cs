using System.Collections.Generic;

namespace Firaxis.CivTech;

public interface IFileDependencyCatalog<TKey>
{
	IEnumerable<TKey> this[TKey key] { get; }

	IEnumerable<TKey> Keys { get; }

	IEnumerable<ISet<TKey>> Values { get; }

	void Clear();

	bool ContainsKey(TKey fileKey);

	void RemoveKey(TKey fileKey);

	void AddChildren(TKey fileKey, IEnumerable<TKey> fileChildren);

	bool AddIfUnique(TKey fileKey, TKey fileChild);

	bool RemoveChild(TKey fileKey, TKey fileChild);

	bool TryGetValue(TKey fileKey, out TKey[] children);
}
