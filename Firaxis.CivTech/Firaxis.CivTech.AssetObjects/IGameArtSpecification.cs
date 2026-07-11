using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IGameArtSpecification : IAssemblyInstance, IDisposable, ISerializable
{
	IGameArtID ID { get; }

	IEnumerable<IArtConsumer> ArtConsumers { get; }

	IEnumerable<IGameLibrary> GameLibraries { get; }

	IEnumerable<IGameArtID> RequiredGameArtIDs { get; set; }

	IArtConsumer AddConsumer(string consumerName);

	bool RemoveConsumer(string consumerName);

	void ClearConsumers();

	IGameLibrary AddGameLibrary(string libraryName);

	bool RemoveGameLibrary(string libraryName);

	void ClearGameLibraries();

	IGameArtID AddRequiredGameArtID(string name, string id);

	bool RemoveRequiredGameArtID(string id);

	void ClearRequiredGameArtIDs();
}
