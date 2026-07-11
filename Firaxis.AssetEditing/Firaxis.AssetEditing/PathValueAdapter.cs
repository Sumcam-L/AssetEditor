using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class PathValueAdapter : DomNodeAdapter
{
	public string Path
	{
		get
		{
			return GetAttribute<string>(BaseSchema.PathValueType.PathAttribute);
		}
		set
		{
			SetAttribute(BaseSchema.PathValueType.PathAttribute, value);
		}
	}
}
