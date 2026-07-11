using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GameArtSpecificationAdapter : DomNodeAdapter
{
	private ArtConsumerContainerAdapter _artConsumerAdapters;

	private GameLibraryContainerAdapter _gameLibraryAdapters;

	private RequiredGameArtIDContainerAdapter _requiredGameArtIDAdapters;

	public string Name
	{
		get
		{
			return GetAttribute<string>(GameArtSpecificationSchema.GameArtSpecificationType.NameAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.GameArtSpecificationType.NameAttribute, value);
		}
	}

	public string ID
	{
		get
		{
			return GetAttribute<string>(GameArtSpecificationSchema.GameArtSpecificationType.IDAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.GameArtSpecificationType.IDAttribute, value);
		}
	}

	public ArtConsumerContainerAdapter ArtConsumers
	{
		get
		{
			return _artConsumerAdapters;
		}
		private set
		{
			_artConsumerAdapters = value;
		}
	}

	public GameLibraryContainerAdapter GameLibraries
	{
		get
		{
			return _gameLibraryAdapters;
		}
		private set
		{
			_gameLibraryAdapters = value;
		}
	}

	public RequiredGameArtIDContainerAdapter RequiredGameArtIDs
	{
		get
		{
			return _requiredGameArtIDAdapters;
		}
		private set
		{
			if (_requiredGameArtIDAdapters != value)
			{
				_requiredGameArtIDAdapters = value;
			}
		}
	}

	private IGameArtSpecification ArtSpecification
	{
		get
		{
			GameArtSpecificationDocument gameArtSpecificationDocument = base.DomNode.As<GameArtSpecificationDocument>();
			BugSubmitter.Assert(gameArtSpecificationDocument != null, "Tried to access the Art Specification without initializing extensions first. @assign bwhitman");
			IGameArtSpecification gameArtSpecification = gameArtSpecificationDocument.GameArtSpecification;
			BugSubmitter.Assert(gameArtSpecification != null, "Tried to access the Art Specification without assigning it to the Document first. @assign bwhitman");
			return gameArtSpecification;
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		DomNode domNode = new DomNode(GameArtSpecificationSchema.ArtConsumerContainerType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(GameArtSpecificationSchema.GameArtSpecificationType.ArtConsumersChild, domNode);
		ArtConsumers = domNode.As<ArtConsumerContainerAdapter>();
		domNode = new DomNode(GameArtSpecificationSchema.GameLibraryContainerType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(GameArtSpecificationSchema.GameArtSpecificationType.GameLibrariesChild, domNode);
		GameLibraries = domNode.As<GameLibraryContainerAdapter>();
		domNode = new DomNode(GameArtSpecificationSchema.RequiredGameArtIDContainerType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(GameArtSpecificationSchema.GameArtSpecificationType.RequiredGameArtIDsChild, domNode);
		RequiredGameArtIDs = domNode.As<RequiredGameArtIDContainerAdapter>();
		RegisterForDomChanges();
	}

	public void Initialize()
	{
		UnregisterFromDomChanges();
		if (Name != ArtSpecification.ID.Name)
		{
			Name = ArtSpecification.ID.Name;
		}
		if (ID != ArtSpecification.ID.ID)
		{
			ID = ArtSpecification.ID.ID;
		}
		ArtConsumers.ArtSpecification = ArtSpecification;
		RequiredGameArtIDs.ArtSpecification = ArtSpecification;
		GameLibraries.ArtSpecification = ArtSpecification;
		ArtConsumers.Initialize();
		RequiredGameArtIDs.Initialize();
		GameLibraries.Initialize();
		RegisterForDomChanges();
	}

	public string GenerateNewConsumerName()
	{
		return ArtConsumers.GenerateNewConsumerName();
	}

	public void AddConsumer(string consumerName)
	{
		ArtConsumers.AddConsumer(consumerName);
	}

	public void RemoveConsumer(string consumerName)
	{
		ArtConsumers.RemoveConsumer(consumerName);
	}

	public string GenerateNewLibraryName()
	{
		return GameLibraries.GenerateNewLibraryName();
	}

	public void AddGameLibrary(string libraryName)
	{
		GameLibraries.AddLibraryReference(libraryName);
	}

	public void RemoveGameLibrary(string libraryName)
	{
		GameLibraries.RemoveLibraryReference(libraryName);
	}

	public void AddRequiredGameArtID(string gameName, string ID)
	{
		RequiredGameArtIDs.AddRequiredGameArtID(gameName, ID);
	}

	public void RemoveRequiredGameArtID(string ID)
	{
		RequiredGameArtIDs.RemoveRequiredGameArtID(ID);
	}

	public RelativePathContainerAdapter GetPathContainerAdapter(IRelativePathHost pathHost)
	{
		RelativePathContainerAdapter result = null;
		string adapterName = pathHost.Name;
		if (pathHost.Type == HostType.ArtConsumer)
		{
			ArtConsumerAdapter artConsumerAdapter = ArtConsumers.ArtConsumers.FirstOrDefault((ArtConsumerAdapter consumer) => consumer.ConsumerName == adapterName);
			if (artConsumerAdapter != null)
			{
				result = artConsumerAdapter.RelativeArtDefPaths;
			}
		}
		else if (pathHost.Type == HostType.GameLibrary)
		{
			GameLibraryAdapter gameLibraryAdapter = GameLibraries.GameLibraries.FirstOrDefault((GameLibraryAdapter library) => library.LibraryName == adapterName);
			if (gameLibraryAdapter != null)
			{
				result = gameLibraryAdapter.RelativePackagePaths;
			}
		}
		else
		{
			BugSubmitter.SilentReport("Add correct PathHost resolution here. @assign bwhitman");
		}
		return result;
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
		if (e.AttributeInfo == GameArtSpecificationSchema.GameArtSpecificationType.NameAttribute)
		{
			ArtSpecification.ID.Name = Name;
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.GameArtSpecificationType.IDAttribute)
		{
			ArtSpecification.ID.ID = ID;
		}
	}
}
