using System.Drawing;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.TextureExport;

public interface ICubeMap : IEnvironmentSource
{
	Bitmap CreateThumbnail();

	IFloatVector3 GetLightIntensity(float x, float y, float z);

	IFloatVector3 FindSun();

	void ReExposeFrom(ICubeMap source, float Multiplier);

	ICubeMap Clone();

	void CopyFrom(ICubeMap source);

	void SaveDDS(string where, PixelFormat eSaveFormat);
}
