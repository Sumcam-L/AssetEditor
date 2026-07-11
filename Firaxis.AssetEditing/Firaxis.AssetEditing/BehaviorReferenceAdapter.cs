using System.Collections.Generic;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class BehaviorReferenceAdapter : DomNodeAdapter, IAssetBrowserTypeProvider
{
	public IEnumerable<string> AllowedBehaviorClasses { get; set; }

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.BehaviorReferenceType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.BehaviorReferenceType.NameAttribute, value);
		}
	}

	public IEnumerable<string> ValidClassNames => AllowedBehaviorClasses;

	public IEnumerable<InstanceType> ValidTypes => new InstanceType[1] { InstanceType.IT_BEHAVIOR };

	public IEntityFilteringContext EntityFilteringContext => CivTechRegistry.EntityFilteringService.GetFilteringContext(ValidTypes, ValidClassNames);
}
