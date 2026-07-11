using System.ComponentModel.DataAnnotations;

namespace Firaxis.CivTech.AssetObjects;

public enum InstanceType
{
	[Display(Name = "Asset")]
	IT_ASSET,
	[Display(Name = "Material")]
	IT_MATERIAL,
	[Display(Name = "Geometry")]
	IT_GEOMETRY,
	[Display(Name = "Texture")]
	IT_TEXTURE,
	[Display(Name = "Animation")]
	IT_ANIMATION,
	[Display(Name = "DSG")]
	IT_DSG,
	[Display(Name = "Analytic Light")]
	IT_ANALYTIC_LIGHT,
	[Display(Name = "Environment Light")]
	IT_ENVIRONMENT_LIGHT,
	[Display(Name = "Light Rig")]
	IT_LIGHT_RIG,
	[Display(Name = "Particle Effect")]
	IT_PARTICLE_EFFECT,
	[Display(Name = "Behavior")]
	IT_BEHAVIOR,
	[Display(Name = "FireFX")]
	IT_FIREFX,
	[Display(Name = "Invalid")]
	IT_INVALID,
	[Display(Name = "Count")]
	IT_COUNT
}
