using System.Collections.Generic;
using System.Drawing;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.TextureExport;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public interface IEnvironmentLightEditingContext : IPropertyEditingContext
{
	IEnvironmentMapImporter Importer { get; }

	IEnvironmentSource Source { get; }

	ICubeMap Cube { get; set; }

	IEnvironmentLightInstance EnvironmentLight { get; }

	IEnvironmentLightClass LightClass { get; }

	bool ApplyChangedAutomatically { get; set; }

	bool SampleIntensityFromMap { get; set; }

	bool HasSource { get; }

	uint LastSampleCount { get; }

	IList<LightDirectionTagAdapter> LightDirectionTags { get; }

	Image CubeImage { get; }

	void ApplyChanges();

	LightDirectionTagAdapter AddDirectionTag(string name, float u, float v);

	void OpenSourceFile(string path);

	void SetSourceFile(string path);

	void CreateCube(EnvironmentMapParameterization eSourceParametrization, uint sampleCount, uint width);

	void SetCubePath(string path);
}
