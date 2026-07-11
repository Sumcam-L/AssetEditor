using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public interface IGameArtSpecificationEditingContext : IObservableContext
{
	DomNode DomNode { get; }

	string GameName { get; set; }

	string ID { get; set; }

	bool IsNewGameArt { get; }

	IEnumerable<ArtConsumerAdapter> ArtConsumers { get; }

	IEnumerable<GameLibraryAdapter> GameLibraries { get; }

	IEnumerable<GameArtIDAdapter> GameDependencies { get; }

	IEnumerable<string> ParentArtConsumers { get; }

	IEnumerable<string> ParentGameLibraries { get; }

	IEnumerable<IGameArtSpecification> AvailableArtSpecifications { get; }

	void GenerateNewID();

	void AddArtConsumer();

	void AddArtConsumerFromParent(string consumerName);

	void RemoveArtConsumer(IEnumerable<ArtConsumerAdapter> artConsumers);

	void AddGameLibrary();

	void AddGameLibraryFromParent(string libraryName);

	void RemoveGameLibraries(IEnumerable<GameLibraryAdapter> gameLibraries);

	void AddGameArtDependency(IEnumerable<IGameArtSpecification> gameDependencies);

	void RemoveGameArtDependency(IEnumerable<GameArtIDAdapter> gameDependencies);
}
