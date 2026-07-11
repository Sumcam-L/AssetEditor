using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtConsumerAdapter : DomNodeAdapter
{
	private RelativePathContainerAdapter _relativePathContainerAdapter;

	private LibraryReferenceContainerAdapter _libraryReferences;

	private IArtConsumer _artConsumer;

	private IGameArtSpecification _gameArtSpecification;

	public string ConsumerName
	{
		get
		{
			return GetAttribute<string>(GameArtSpecificationSchema.ArtConsumerType.ConsumerNameAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.ArtConsumerType.ConsumerNameAttribute, value);
		}
	}

	public bool LoadsLibraries
	{
		get
		{
			return GetAttribute<bool>(GameArtSpecificationSchema.ArtConsumerType.LoadsLibrariesAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.ArtConsumerType.LoadsLibrariesAttribute, value);
		}
	}

	public RelativePathContainerAdapter RelativeArtDefPaths
	{
		get
		{
			return _relativePathContainerAdapter;
		}
		private set
		{
			_relativePathContainerAdapter = value;
		}
	}

	public LibraryReferenceContainerAdapter LibraryReferences
	{
		get
		{
			return _libraryReferences;
		}
		private set
		{
			_libraryReferences = value;
		}
	}

	public IArtConsumer ArtConsumer
	{
		get
		{
			return _artConsumer;
		}
		set
		{
			if (_artConsumer != value)
			{
				_artConsumer = value;
				LibraryReferences.ArtConsumer = value;
				RelativeArtDefPaths.PathHost = new ArtDefPathHost(value);
				ConsumerName = _artConsumer.ConsumerName;
				LoadsLibraries = _artConsumer.LoadsLibraries;
			}
		}
	}

	public IGameArtSpecification ArtSpecification
	{
		get
		{
			return _gameArtSpecification;
		}
		set
		{
			_gameArtSpecification = value;
		}
	}

	public static ArtConsumerAdapter Create(IGameArtSpecification gameArtSpec, IArtConsumer artConsumer)
	{
		DomNode domNode = new DomNode(GameArtSpecificationSchema.ArtConsumerType.Type);
		domNode.InitializeExtensions();
		ArtConsumerAdapter artConsumerAdapter = domNode.As<ArtConsumerAdapter>();
		artConsumerAdapter.ArtSpecification = gameArtSpec;
		artConsumerAdapter.ArtConsumer = artConsumer;
		artConsumerAdapter.RegisterForDomChanges();
		return artConsumerAdapter;
	}

	public static ArtConsumerAdapter Create(IGameArtSpecification gameArtSpec, string consumerName)
	{
		return Create(gameArtSpec, gameArtSpec.AddConsumer(consumerName));
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		RelativeArtDefPaths = CreatePathContainerAdapter();
		LibraryReferences = CreateLibraryContainerAdapter();
	}

	private RelativePathContainerAdapter CreatePathContainerAdapter()
	{
		DomNode domNode = new DomNode(GameArtSpecificationSchema.RelativePathContainerType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(GameArtSpecificationSchema.ArtConsumerType.RelativePathsChild, domNode);
		return domNode.As<RelativePathContainerAdapter>();
	}

	private LibraryReferenceContainerAdapter CreateLibraryContainerAdapter()
	{
		DomNode domNode = new DomNode(GameArtSpecificationSchema.LibraryReferenceContainerType.Type);
		domNode.InitializeExtensions();
		base.DomNode.SetChild(GameArtSpecificationSchema.ArtConsumerType.LibraryReferencesChild, domNode);
		return domNode.As<LibraryReferenceContainerAdapter>();
	}

	public bool IsValidName(string name)
	{
		if (name == ConsumerName)
		{
			return true;
		}
		DomNode parent = base.DomNode.Parent;
		if (parent == null)
		{
			return true;
		}
		ArtConsumerContainerAdapter artConsumerContainerAdapter = parent.As<ArtConsumerContainerAdapter>();
		if (artConsumerContainerAdapter != null)
		{
			return !artConsumerContainerAdapter.ArtConsumers.Any((ArtConsumerAdapter lib) => lib.ConsumerName == name);
		}
		return true;
	}

	protected virtual void RegisterForDomChanges()
	{
		base.DomNode.AttributeChanged += HandleAttributeChanged;
	}

	protected virtual void HandleAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == GameArtSpecificationSchema.ArtConsumerType.ConsumerNameAttribute)
		{
			ArtConsumer.ConsumerName = ConsumerName;
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.ArtConsumerType.LoadsLibrariesAttribute)
		{
			ArtConsumer.LoadsLibraries = LoadsLibraries;
		}
	}

	public override string ToString()
	{
		return ConsumerName;
	}
}
