using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public interface IEntityPreviewComponent
{
	IEntityChangeList EntityChanges { get; }

	IDisposable SuspendPreviewerUpdates();
}
