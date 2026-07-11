using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GameLibraryAdapter : DomNodeAdapter
{
	public const string NewName = "NewLibrary";

	private RelativePathContainerAdapter _relativePackagePaths;

	private IGameLibrary _gameLibrary;

	public string LibraryName
	{
		get
		{
			return GetAttribute<string>(GameArtSpecificationSchema.GameLibraryType.LibraryNameAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.GameLibraryType.LibraryNameAttribute, value);
		}
	}

	public RelativePathContainerAdapter RelativePackagePaths
	{
		get
		{
			return _relativePackagePaths;
		}
		private set
		{
			_relativePackagePaths = value;
		}
	}

	public IGameLibrary GameLibrary
	{
		get
		{
			return _gameLibrary;
		}
		set
		{
			if (_gameLibrary != value)
			{
				_gameLibrary = value;
				UnregisterFromDomChanges();
				LibraryName = _gameLibrary.LibraryName;
				RelativePackagePaths.PathHost = new GameLibraryPathHost(value);
				RegisterForDomChanges();
			}
		}
	}

	public static GameLibraryAdapter Create(IGameLibrary gameLibrary)
	{
		DomNode domNode = new DomNode(GameArtSpecificationSchema.GameLibraryType.Type);
		domNode.InitializeExtensions();
		GameLibraryAdapter gameLibraryAdapter = domNode.As<GameLibraryAdapter>();
		gameLibraryAdapter.GameLibrary = gameLibrary;
		return gameLibraryAdapter;
	}

	public static GameLibraryAdapter Create(IGameArtSpecification artSpec, string libraryName)
	{
		return Create(artSpec.AddGameLibrary(libraryName));
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		DomNode domNode = new DomNode(GameArtSpecificationSchema.RelativePathContainerType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(GameArtSpecificationSchema.GameLibraryType.RelativePathsChild, domNode);
		RelativePackagePaths = domNode.As<RelativePathContainerAdapter>();
		RegisterForDomChanges();
	}

	public bool HasBeenNamed()
	{
		return !LibraryName.StartsWith("NewLibrary");
	}

	public bool IsValidName(string name)
	{
		if (name == LibraryName)
		{
			return true;
		}
		DomNode parent = base.DomNode.Parent;
		if (parent == null)
		{
			return true;
		}
		GameLibraryContainerAdapter gameLibraryContainerAdapter = parent.As<GameLibraryContainerAdapter>();
		if (gameLibraryContainerAdapter != null)
		{
			return !gameLibraryContainerAdapter.GameLibraries.Any((GameLibraryAdapter lib) => lib.LibraryName == name);
		}
		return true;
	}

	public IEnumerable<string> GetReferencingConsumerNames()
	{
		DomNode root = base.DomNode.GetRoot();
		if (root == null)
		{
			return Enumerable.Empty<string>();
		}
		GameArtSpecificationAdapter gameArtSpecificationAdapter = root.As<GameArtSpecificationAdapter>();
		if (gameArtSpecificationAdapter == null)
		{
			return Enumerable.Empty<string>();
		}
		Func<ArtConsumerAdapter, bool> predicate = (ArtConsumerAdapter consumer) => consumer.LibraryReferences.LibraryReferences.Any((LibraryReferenceAdapter libraryReference) => libraryReference.LibraryName == LibraryName);
		return (from consumer in gameArtSpecificationAdapter.ArtConsumers.ArtConsumers.Where(predicate)
			select consumer.ConsumerName).ToArray();
	}

	protected virtual void RegisterForDomChanges()
	{
		base.DomNode.AttributeChanged += HandleAttributeChanged;
	}

	protected virtual void UnregisterFromDomChanges()
	{
		base.DomNode.AttributeChanged -= HandleAttributeChanged;
	}

	protected virtual void HandleAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == GameArtSpecificationSchema.GameLibraryType.LibraryNameAttribute)
		{
			string oldLibraryName = e.OldValue.ToString();
			string libraryName = LibraryName;
			GameLibrary.LibraryName = LibraryName;
			UpdateLibraryReferences(oldLibraryName, libraryName);
		}
	}

	private void UpdateLibraryReferences(string oldLibraryName, string newLibraryName)
	{
		DomNode root = base.DomNode.GetRoot();
		if (root == null)
		{
			return;
		}
		GameArtSpecificationAdapter gameArtSpecificationAdapter = root.As<GameArtSpecificationAdapter>();
		if (gameArtSpecificationAdapter == null)
		{
			return;
		}
		string[] libraryNames = new string[1] { oldLibraryName };
		string[] libraryNames2 = new string[1] { newLibraryName };
		foreach (ArtConsumerAdapter artConsumer in gameArtSpecificationAdapter.ArtConsumers.ArtConsumers)
		{
			if (artConsumer.LibraryReferences.LibraryReferences.Any((LibraryReferenceAdapter lr) => lr.LibraryName == oldLibraryName))
			{
				artConsumer.LibraryReferences.RemoveLibraryReferences(libraryNames);
				artConsumer.LibraryReferences.AddLibraryReferences(libraryNames2);
			}
		}
	}

	public override string ToString()
	{
		return LibraryName;
	}
}
