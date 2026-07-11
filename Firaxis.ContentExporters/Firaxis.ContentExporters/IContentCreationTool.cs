using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ContentExporters;

public interface IContentCreationTool
{
	IEnumerable<string> SupportedFileTypes { get; }

	IEnumerable<InstanceType> SupportedInstanceTypes { get; }

	void OpenFile(string filePath);

	void RenderSourceObjectFromFile(string filePath, string sourceObject, string outputPath);
}
