using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GeometryReferenceAdapter : DomNodeAdapter
{
	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.GeometryReferenceType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.GeometryReferenceType.NameAttribute, value);
		}
	}
}
