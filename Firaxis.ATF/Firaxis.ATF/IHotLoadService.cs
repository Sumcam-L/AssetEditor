using System;
using System.Collections.Generic;
using Sce.Atf;

namespace Firaxis.ATF;

public interface IHotLoadService
{
	bool IsHotLoading { get; }

	bool HotLoadOnReimport { get; }

	event EventHandler HotLoadCompleted;

	event EventHandler HotLoadStarted;

	void AddHotLoadData(IHotLoadData hotLoadData);

	void BeginHotLoadRequest();

	void EndHotLoadRequest();

	void RequestHotLoad(IDocument doc);

	void RequestHotLoad(string systemName, IEnumerable<string> consumerNames);

	void RequestHotLoad(IEnumerable<HotLoadData> systemConsumerNamesCollection);
}
