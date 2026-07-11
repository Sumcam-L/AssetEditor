using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Firaxis.Threading;

namespace Firaxis.CivTech;

public class FileDependencyCatalog<TKey> : IFileDependencyCatalog<TKey>
{
	private ReaderWriterLockSlim m_dependencyLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	private IDictionary<TKey, ISet<TKey>> m_fileChildren = new Dictionary<TKey, ISet<TKey>>();

	public IEnumerable<TKey> this[TKey key]
	{
		get
		{
			using (new ScopedReaderLock(m_dependencyLock))
			{
				if (m_fileChildren.ContainsKey(key))
				{
					return m_fileChildren[key].ToArray();
				}
				return Enumerable.Empty<TKey>();
			}
		}
	}

	public IEnumerable<TKey> Keys
	{
		get
		{
			using (new ScopedReaderLock(m_dependencyLock))
			{
				return m_fileChildren.Keys.ToArray();
			}
		}
	}

	public IEnumerable<ISet<TKey>> Values
	{
		get
		{
			using (new ScopedReaderLock(m_dependencyLock))
			{
				return m_fileChildren.Values.ToArray();
			}
		}
	}

	public void AddChildren(TKey fileKey, IEnumerable<TKey> fileChildren)
	{
		using (new ScopedWriterLock(m_dependencyLock))
		{
			if (!m_fileChildren.ContainsKey(fileKey))
			{
				m_fileChildren[fileKey] = new HashSet<TKey>(fileChildren);
			}
			else
			{
				m_fileChildren[fileKey].UnionWith(fileChildren);
			}
		}
	}

	public bool AddIfUnique(TKey fileKey, TKey fileChild)
	{
		using ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_dependencyLock);
		ISet<TKey> value = null;
		if (!m_fileChildren.TryGetValue(fileKey, out value))
		{
			using (new ScopedWriterLock(upgrableReadLock))
			{
				m_fileChildren[fileKey] = new HashSet<TKey>();
				return m_fileChildren[fileKey].Add(fileChild);
			}
		}
		if (!value.Contains(fileChild))
		{
			using (new ScopedWriterLock(upgrableReadLock))
			{
				return value.Add(fileChild);
			}
		}
		return false;
	}

	public void Clear()
	{
		using (new ScopedWriterLock(m_dependencyLock))
		{
			m_fileChildren.Clear();
		}
	}

	public bool ContainsKey(TKey fileKey)
	{
		using (new ScopedReaderLock(m_dependencyLock))
		{
			return m_fileChildren.ContainsKey(fileKey);
		}
	}

	public bool RemoveChild(TKey fileKey, TKey fileChild)
	{
		using (ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_dependencyLock))
		{
			ISet<TKey> value = null;
			if (m_fileChildren.TryGetValue(fileKey, out value))
			{
				using (new ScopedWriterLock(upgrableReadLock))
				{
					return value.Remove(fileChild);
				}
			}
		}
		return false;
	}

	public void RemoveKey(TKey fileKey)
	{
		using (new ScopedWriterLock(m_dependencyLock))
		{
			m_fileChildren.Remove(fileKey);
		}
	}

	public bool TryGetValue(TKey fileKey, out TKey[] children)
	{
		using (new ScopedReaderLock(m_dependencyLock))
		{
			if (m_fileChildren.TryGetValue(fileKey, out var value))
			{
				children = value.ToArray();
				return true;
			}
		}
		children = Enumerable.Empty<TKey>().ToArray();
		return false;
	}
}
