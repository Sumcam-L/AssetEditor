using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LibraryReferenceAdapter : DomNodeAdapter
{
	public string LibraryName
	{
		get
		{
			return GetAttribute<string>(GameArtSpecificationSchema.LibraryReferenceType.LibraryNameAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.LibraryReferenceType.LibraryNameAttribute, value);
		}
	}

	public static LibraryReferenceAdapter Create(string libraryName)
	{
		DomNode domNode = new DomNode(GameArtSpecificationSchema.LibraryReferenceType.Type);
		domNode.InitializeExtensions();
		LibraryReferenceAdapter libraryReferenceAdapter = domNode.As<LibraryReferenceAdapter>();
		libraryReferenceAdapter.LibraryName = libraryName;
		return libraryReferenceAdapter;
	}

	public override string ToString()
	{
		return LibraryName;
	}
}
