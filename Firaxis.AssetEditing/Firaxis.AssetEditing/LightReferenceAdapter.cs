using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LightReferenceAdapter : DomNodeAdapter
{
	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LightReferenceType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightReferenceType.NameAttribute, value);
		}
	}
}
