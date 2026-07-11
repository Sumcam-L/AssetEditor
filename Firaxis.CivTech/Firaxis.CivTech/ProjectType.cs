using System.ComponentModel.DataAnnotations;

namespace Firaxis.CivTech;

public enum ProjectType
{
	[Display(Name = "Normal")]
	eNormal,
	[Display(Name = "Mod")]
	eMod,
	[Display(Name = "Testing")]
	eTesting
}
