using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class RelativePathAdapter : DomNodeAdapter
{
	public string RelativePath
	{
		get
		{
			return GetAttribute<string>(GameArtSpecificationSchema.RelativePathType.RelativePathAttribute);
		}
		set
		{
			SetAttribute(GameArtSpecificationSchema.RelativePathType.RelativePathAttribute, value);
		}
	}

	public static RelativePathAdapter Create(string relativePath)
	{
		DomNode domNode = new DomNode(GameArtSpecificationSchema.RelativePathType.Type);
		domNode.InitializeExtensions();
		RelativePathAdapter relativePathAdapter = domNode.As<RelativePathAdapter>();
		relativePathAdapter.RelativePath = relativePath;
		return relativePathAdapter;
	}

	public override string ToString()
	{
		return RelativePath;
	}
}
