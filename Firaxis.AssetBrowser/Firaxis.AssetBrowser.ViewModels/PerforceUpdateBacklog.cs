using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetBrowser.ViewModels;

internal class PerforceUpdateBacklog : IDisposable
{
	private EntityFileInfo[] currentInfos;

	private EntityFileInfo[] pendingInfos;

	private readonly object pendingLock = new object();

	private readonly object currentLock = new object();

	public void AddPendingInfos(EntityFileInfo[] infos)
	{
		lock (pendingLock)
		{
			pendingInfos = infos;
		}
	}

	public EntityFileInfo[] GetNextCurrent()
	{
		lock (pendingLock)
		{
			lock (currentLock)
			{
				currentInfos = pendingInfos;
				pendingInfos = new EntityFileInfo[0];
				return currentInfos;
			}
		}
	}

	public void Dispose()
	{
		lock (pendingLock)
		{
			lock (currentLock)
			{
				currentInfos = new EntityFileInfo[0];
				pendingInfos = new EntityFileInfo[0];
			}
		}
	}
}
