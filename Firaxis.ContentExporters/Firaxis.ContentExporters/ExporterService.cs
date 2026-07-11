using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Properties;
using Firaxis.ContentExporters.Implementations;

namespace Firaxis.ContentExporters;

public static class ExporterService
{
	private static IList<IContentExporter> RegisteredExporters;

	private static IList<IContentCreationTool> RegisteredContentCreationTools;

	private static List<string> OpenableExtensions;

	public const string LOOSE_LAYER_EXPORT = "loose_layers_only";

	public static string SanitizePath(this string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		string fileName = Path.GetFileName(path);
		if ((!string.IsNullOrEmpty(directoryName) && directoryName.IndexOfAny(Path.GetInvalidPathChars()) != -1) || (!string.IsNullOrEmpty(fileName) && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1))
		{
			throw new IOException("The path contains illegal characters.");
		}
		if (path.Length >= 260)
		{
			throw new PathTooLongException($"The path couldn't be sanitized because it was too long:\r\n\t{path}");
		}
		return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).ToLower();
	}

	public static void RegisterCreatedExporter(IContentExporter exporter)
	{
		RegisterExporter(exporter);
		if (exporter is IContentCreationTool)
		{
			RegisterContentCreationTool(exporter as IContentCreationTool);
		}
	}

	static ExporterService()
	{
		RegisteredExporters = new List<IContentExporter>();
		RegisteredContentCreationTools = new List<IContentCreationTool>();
		OpenableExtensions = new List<string>();
		RegisterCreatedExporter(new LooseTextureExporter());
		RegisterCreatedExporter(new EnvironmentLightExporter());
		RegisterCreatedExporter(new FiraxisGeometryExporter());
		if (!Resources.ModTools)
		{
			RegisterCreatedExporter(new PhotoshopExporter());
			RegisterCreatedExporter(new MaxExporter());
			RegisterCreatedExporter(new MayaExporter());
			RegisterCreatedExporter(new ParticleExporter());
		}
	}

	public static IEnumerable<string> GetSupportedSourceFileExtensions(InstanceType entityType)
	{
		return GetContentExporters(entityType).SelectMany((IContentExporter exp) => exp.SupportedFileTypes);
	}

	public static IEnumerable<IContentCreationTool> GetContentCreationTools(InstanceType entityType)
	{
		return new List<IContentCreationTool>(RegisteredContentCreationTools.Where((IContentCreationTool exp) => exp.SupportedInstanceTypes.Contains(entityType)));
	}

	public static IEnumerable<IContentExporter> GetContentExporters(InstanceType entityType)
	{
		return new List<IContentExporter>(RegisteredExporters.Where((IContentExporter exp) => exp.SupportedInstanceTypes.Contains(entityType)));
	}

	public static IEnumerable<string> GetExportableFileTypes()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in RegisteredExporters.SelectMany((IContentExporter exp) => exp.SupportedFileTypes))
		{
			hashSet.Add(item);
		}
		return hashSet;
	}

	public static IEnumerable<string> GetOpenableFileTypes()
	{
		return OpenableExtensions;
	}

	public static void RegisterExporter(IContentExporter exporter)
	{
		RegisteredExporters.Add(exporter);
	}

	public static void RegisterContentCreationTool(IContentCreationTool contentCreationTool)
	{
		RegisteredContentCreationTools.Add(contentCreationTool);
		OpenableExtensions.AddRange(contentCreationTool.SupportedFileTypes);
	}

	public static IContentExporter GetExporter(IImportedEntity entity)
	{
		return GetExporter(Path.GetExtension(entity.SourceFilePath), entity.Type);
	}

	public static IContentCreationTool GetContentCreationTool(IImportedEntity entity)
	{
		return GetContentCreationTool(Path.GetExtension(entity.SourceFilePath));
	}

	public static IContentExporter GetExporter(string fileExtension)
	{
		return GetExporters(fileExtension).FirstOrDefault();
	}

	public static IEnumerable<IContentExporter> GetExporters(string fileExtension)
	{
		return RegisteredExporters.Where((IContentExporter exp) => exp.SupportedFileTypes.Contains(fileExtension, StringComparer.InvariantCultureIgnoreCase));
	}

	public static IContentExporter GetExporter(string extension, InstanceType entityType)
	{
		return GetExporters(extension, entityType).FirstOrDefault();
	}

	public static IEnumerable<IContentExporter> GetExporters(string extension, InstanceType entityType)
	{
		return RegisteredExporters.Where((IContentExporter exp) => exp.SupportedFileTypes.Contains(extension, StringComparer.InvariantCultureIgnoreCase) && exp.SupportedInstanceTypes.Contains(entityType));
	}

	public static IContentCreationTool GetContentCreationTool(string fileExtension)
	{
		return RegisteredContentCreationTools.FirstOrDefault((IContentCreationTool exp) => exp.SupportedFileTypes.Contains(fileExtension, StringComparer.InvariantCultureIgnoreCase));
	}
}
