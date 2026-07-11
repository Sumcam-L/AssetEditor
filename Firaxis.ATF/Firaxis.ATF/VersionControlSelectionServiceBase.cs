using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Firaxis.CivTech;
using Firaxis.Threading;

namespace Firaxis.ATF;

public class VersionControlSelectionServiceBase : IVersionControlSelectionService
{
	private IDictionary<VersionControlInfo, IVersionControlService> m_cache = new Dictionary<VersionControlInfo, IVersionControlService>();

	private ReaderWriterLockSlim m_cacheLock = new ReaderWriterLockSlim();

	private object m_creationLock = new object();

	public virtual IDictionary<string, VersionControlInfo> VersionControlInfoMap { get; private set; } = new ConcurrentDictionary<string, VersionControlInfo>();

	public virtual IVersionControlService this[string name]
	{
		get
		{
			if (!VersionControlInfoMap.ContainsKey(name))
			{
				return null;
			}
			return GetVersionControlService(VersionControlInfoMap[name]);
		}
	}

	protected void AddOrUpdateVersionControlService(string projName, VersionControlInfo info, IVersionControlService vcs)
	{
		VersionControlInfoMap[projName] = info;
		using (new ScopedWriterLock(m_cacheLock))
		{
			m_cache[info] = vcs;
		}
	}

	protected IVersionControlService GetVersionControlService(VersionControlInfo info)
	{
		using ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_cacheLock);
		IVersionControlService value = null;
		if (m_cache.TryGetValue(info, out value))
		{
			return value;
		}
		lock (m_creationLock)
		{
			if (m_cache.TryGetValue(info, out value))
			{
				return value;
			}
			value = new VersionControlService(info);
			using (new ScopedWriterLock(upgrableReadLock))
			{
				m_cache[info] = value;
				return value;
			}
		}
	}
}
