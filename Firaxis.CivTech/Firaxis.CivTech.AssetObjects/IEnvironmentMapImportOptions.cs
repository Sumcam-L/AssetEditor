using Firaxis.CivTech.TextureExport;

namespace Firaxis.CivTech.AssetObjects;

public interface IEnvironmentMapImportOptions
{
	PixelFormat PixelFormat { get; set; }

	bool AllowArtistMips { get; set; }

	bool RequirePow2 { get; set; }

	uint MaxWidth { get; set; }

	uint MinWidth { get; set; }
}
