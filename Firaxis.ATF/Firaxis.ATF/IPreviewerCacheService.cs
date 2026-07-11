using System;

namespace Firaxis.ATF;

public interface IPreviewerCacheService
{
	event EventHandler<PreviewerCacheAdded> EntityAdded;

	event EventHandler<PreviewerCacheRemoved> EntityRemoved;

	void AddToCache(IEntityDocument entityDocument);

	bool RemoveFromCache(IEntityDocument entityDocument);

	bool IsCachedEntity(IEntityDocument entityDocument);

	string GetCachedXML(string key);

	void StartProjectChange();

	void FinishProjectChange();
}
