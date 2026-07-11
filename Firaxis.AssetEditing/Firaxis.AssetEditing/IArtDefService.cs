using System;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;

namespace Firaxis.AssetEditing;

public interface IArtDefService
{
	event EventHandler<ItemInsertedEventArgs<object>> ElementAdded;

	event EventHandler<ItemRemovedEventArgs<object>> ElementRemoved;

	event EventHandler<EventArgs> ProjectConfigChanged;

	void AddElement(ArtDefCollectionAdapter collection);

	void RemoveElement(ArtDefCollectionAdapter collection, IArtDefElement element);
}
