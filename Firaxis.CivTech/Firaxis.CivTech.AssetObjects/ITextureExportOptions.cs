using Firaxis.CivTech.TextureExport;

namespace Firaxis.CivTech.AssetObjects;

public interface ITextureExportOptions
{
	PixelFormat ExportPixelFormat { get; set; }

	FilterType DefaultMipFilter { get; set; }

	TextureType ExportTextureType { get; set; }

	float DefaultMipSupportScale { get; set; }

	uint MaxWidth { get; set; }

	uint MinWidth { get; set; }

	uint MaxHeight { get; set; }

	uint MinHeight { get; set; }

	uint MaxDepth { get; set; }

	uint MinDepth { get; set; }

	float ExportGammaIn { get; set; }

	float ExportGammaOut { get; set; }

	float ExportClampMin { get; set; }

	float ExportClampMax { get; set; }

	bool AllowArtistMips { get; set; }

	bool RequireSquare { get; set; }

	bool RequirePow2 { get; set; }
}
