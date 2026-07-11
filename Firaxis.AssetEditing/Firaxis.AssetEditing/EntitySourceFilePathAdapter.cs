using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class EntitySourceFilePathAdapter : DomNodeAdapter
{
	public string Path
	{
		get
		{
			return GetAttribute<string>(EntitySchema.EntitySourceFilePathType.PathAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.EntitySourceFilePathType.PathAttribute, value);
		}
	}
}
