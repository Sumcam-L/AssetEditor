using System;
using System.Drawing;

namespace Firaxis.CivTech.Texture;

public interface IDDSDataExtractor : IAssemblyInstance, IDisposable
{
	bool LoadDDSFile(string filePath);

	uint GetTextureWidth();

	uint GetTextureHeight();

	uint GetTextureDepth();

	uint GetTextureMipMapCount();

	Bitmap CreateThumbnailImage(int width, int height);
}
