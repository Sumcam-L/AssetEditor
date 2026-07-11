using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firaxis.Threading;
using Firaxis.Utility;

namespace Firaxis.CivTech;

public class FileDepotCatelog : IDepotCatalog, IFileCatalog<string>
{
	private ReaderWriterLockSlim m_fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	private IDictionary<string, DepotFileInfo> m_files = new Dictionary<string, DepotFileInfo>(new PathComparer());

	public int Count
	{
		get
		{
			using (new ScopedReaderLock(m_fileLock))
			{
				return m_files.Count;
			}
		}
	}

	public bool AddOrUpdate(string fileKey, DepotFileInfo info)
	{
		using ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_fileLock);
		DepotFileInfo empty = DepotFileInfo.Empty;
		bool result = m_files.ContainsKey(fileKey);
		using (new ScopedWriterLock(upgrableReadLock))
		{
			m_files[fileKey] = info;
		}
		return result;
	}

	public bool ContainsKey(string fileKey)
	{
		using (new ScopedReaderLock(m_fileLock))
		{
			return m_files.ContainsKey(fileKey);
		}
	}

	public void Remove(string fileKey)
	{
		using (new ScopedWriterLock(m_fileLock))
		{
			m_files.Remove(fileKey);
		}
	}

	public bool TryGetValue(string fileKey, out DepotFileInfo info)
	{
		using (new ScopedReaderLock(m_fileLock))
		{
			return m_files.TryGetValue(fileKey, out info);
		}
	}

	public void ForEachKey(Action<string> visit)
	{
		string[] array = Enumerable.Empty<string>().ToArray();
		using (new ScopedReaderLock(m_fileLock))
		{
			array = m_files.Keys.ToArray();
		}
		string[] array2 = array;
		foreach (string obj in array2)
		{
			visit(obj);
		}
	}

	public void ParallelForEachValue(Action<DepotFileInfo> visit)
	{
		DepotFileInfo[] array = Enumerable.Empty<DepotFileInfo>().ToArray();
		using (new ScopedReaderLock(m_fileLock))
		{
			array = m_files.Values.ToArray();
		}
		Parallel.ForEach(m_files.Values, delegate(DepotFileInfo file)
		{
			visit(file);
		});
	}

	public void ForEachValue(Action<DepotFileInfo> visit)
	{
		DepotFileInfo[] array = Enumerable.Empty<DepotFileInfo>().ToArray();
		using (new ScopedReaderLock(m_fileLock))
		{
			array = m_files.Values.ToArray();
		}
		DepotFileInfo[] array2 = array;
		foreach (DepotFileInfo obj in array2)
		{
			visit(obj);
		}
	}

	public void ForEachValue(Func<DepotFileInfo, bool> predicate, Action<DepotFileInfo> visit)
	{
		DepotFileInfo[] array = Enumerable.Empty<DepotFileInfo>().ToArray();
		using (new ScopedReaderLock(m_fileLock))
		{
			array = m_files.Values.ToArray();
		}
		DepotFileInfo[] array2 = array;
		foreach (DepotFileInfo depotFileInfo in array2)
		{
			if (predicate(depotFileInfo))
			{
				visit(depotFileInfo);
			}
		}
	}
}
