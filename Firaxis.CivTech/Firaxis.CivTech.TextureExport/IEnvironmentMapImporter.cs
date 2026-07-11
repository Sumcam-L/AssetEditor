using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.TextureExport;

public interface IEnvironmentMapImporter : IAssemblyInstance, IDisposable
{
	IEnumerable<string> SupportedFileTypes { get; }

	IEnvironmentSource OpenSourceFile(string path);

	ICubeMap CreateCube(IEnvironmentSource source, EnvironmentMapParameterization eSourceParametrization, uint SampleCount, IEnvironmentMapImportOptions opts, bool useIdentityBasis, uint width);

	ICubeMap OpenCubeMap(string dds);

	float[] DirectionToThumbnailUV(float x, float y, float z);

	float[] ThumbnailUVToDirection(float u, float v);
}
