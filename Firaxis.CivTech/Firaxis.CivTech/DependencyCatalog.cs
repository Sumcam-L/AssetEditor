using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Firaxis.Threading;
using Firaxis.Utility;

namespace Firaxis.CivTech;

public class DependencyCatalog : IDependencyCatalog, IFileDependencyCatalog<string>
{
	private ReaderWriterLockSlim m_dependencyLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	private IDictionary<string, ISet<string>> m_fileChildren = new Dictionary<string, ISet<string>>(new PathComparer());

	public IEnumerable<string> this[string key]
	{
		get
		{
			using (new ScopedReaderLock(m_dependencyLock))
			{
				if (m_fileChildren.ContainsKey(key))
				{
					return m_fileChildren[key].ToArray();
				}
				return Enumerable.Empty<string>();
			}
		}
	}

	public IEnumerable<string> Keys
	{
		get
		{
			using (new ScopedReaderLock(m_dependencyLock))
			{
				return m_fileChildren.Keys.ToArray();
			}
		}
	}

	public IEnumerable<ISet<string>> Values
	{
		get
		{
			using (new ScopedReaderLock(m_dependencyLock))
			{
				return m_fileChildren.Values.ToArray();
			}
		}
	}

	public void AddChildren(string fileKey, IEnumerable<string> fileChildren)
	{
		using (new ScopedWriterLock(m_dependencyLock))
		{
			if (!m_fileChildren.ContainsKey(fileKey))
			{
				m_fileChildren[fileKey] = new HashSet<string>(fileChildren, new PathComparer());
			}
			else
			{
				m_fileChildren[fileKey].UnionWith(fileChildren);
			}
		}
	}

	public bool AddIfUnique(string fileKey, string fileChild)
	{
		using ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_dependencyLock);
		ISet<string> value = null;
		if (!m_fileChildren.TryGetValue(fileKey, out value))
		{
			using (new ScopedWriterLock(upgrableReadLock))
			{
				m_fileChildren[fileKey] = new HashSet<string>(new PathComparer());
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

	public bool ContainsKey(string fileKey)
	{
		using (new ScopedReaderLock(m_dependencyLock))
		{
			return m_fileChildren.ContainsKey(fileKey);
		}
	}

	public bool RemoveChild(string fileKey, string fileChild)
	{
		using (ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_dependencyLock))
		{
			ISet<string> value = null;
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

	public void RemoveKey(string fileKey)
	{
		using (new ScopedWriterLock(m_dependencyLock))
		{
			m_fileChildren.Remove(fileKey);
		}
	}

	public bool TryGetValue(string fileKey, out string[] children)
	{
		using (new ScopedReaderLock(m_dependencyLock))
		{
			if (m_fileChildren.TryGetValue(fileKey, out var value))
			{
				children = value.ToArray();
				return true;
			}
		}
		children = Enumerable.Empty<string>().ToArray();
		return false;
	}
}
