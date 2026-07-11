using System;

namespace Firaxis.CivTech.TextureExport;

public interface ILooseFileExporter : IAssemblyInstance, IDisposable
{
	bool ExportAsDDS(string inFilePath, string outFilePath, IExportSettingsParams settings, out string errorMessage);
}
