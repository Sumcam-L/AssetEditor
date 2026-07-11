namespace Firaxis.CivTech.TextureExport;

public interface IEnvironmentSource
{
	PixelFormat PixelFormat { get; }

	uint Width { get; }

	uint Height { get; }

	bool IsCubeMap { get; }
}
