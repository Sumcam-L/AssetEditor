using System.ComponentModel.DataAnnotations;

namespace Firaxis.CivTech.AssetObjects;

public enum TriggerType
{
	[Display(Name = "Sound")]
	TT_SOUND,
	[Display(Name = "Asset VFX")]
	TT_ASSET_VFX,
	[Display(Name = "Transfer")]
	TT_TRANSFER,
	[Display(Name = "Action")]
	TT_ACTION,
	[Display(Name = "ArtDef VFX")]
	TT_ARTDEF_VFX,
	[Display(Name = "Light")]
	TT_LIGHT,
	[Display(Name = "Unknown")]
	TT_COUNT
}
