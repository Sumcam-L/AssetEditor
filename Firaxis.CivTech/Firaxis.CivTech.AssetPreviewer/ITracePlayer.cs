using System;

namespace Firaxis.CivTech.AssetPreviewer;

public interface ITracePlayer : IDisposable
{
	event LogEventHandler Logger;

	void Play();
}
