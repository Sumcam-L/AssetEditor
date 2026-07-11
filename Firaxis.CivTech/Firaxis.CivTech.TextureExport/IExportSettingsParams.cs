using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.TextureExport;

public interface IExportSettingsParams : IAssemblyInstance, IDisposable
{
	PixelFormat PixelFormat { get; set; }

	FilterType FilterType { get; set; }

	ExportMode ExportMode { get; set; }

	bool UseMips { get; set; }

	uint NumManualMips { get; set; }

	bool CompleteMipChain { get; set; }

	float ValueClampMin { get; set; }

	float ValueClampMax { get; set; }

	float SupportScale { get; set; }

	float GammaIn { get; set; }

	float GammaOut { get; set; }

	uint SlabWidth { get; set; }

	uint SlabHeight { get; set; }

	uint ColorKeyX { get; set; }

	uint ColorKeyY { get; set; }

	uint ColorKeyZ { get; set; }

	bool SampleFromTopLayer { get; set; }

	void AssignFromTextureClass(ITextureClass texClass);

	void SerializeToFile(string filePath);

	string SerializeToXMLString();

	void DeserializeFromFile(string filePath);

	void DeserializeFromXML(string filePath);
}
