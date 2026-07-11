using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ContentExporters;

public class SourceFileModel
{
	private List<SourceObjectModel> _sourceObjects = new List<SourceObjectModel>();

	public Uri SourceFilePath { get; private set; }

	public string SourceFileName { get; private set; }

	public string Extension { get; private set; }

	public IEnumerable<SourceObjectModel> SourceObjects => _sourceObjects;

	public DateTime LastModifiedTime { get; private set; }

	public InstanceType EntityType { get; private set; }

	private IContentExporter Exporter { get; set; }

	public SourceFileModel(string sourceFilePath, InstanceType entityType)
		: this(new Uri(sourceFilePath), entityType)
	{
	}

	public SourceFileModel(Uri sourceFilePath, InstanceType entityType)
	{
		SourceFilePath = sourceFilePath;
		Extension = Path.GetExtension(SourceFilePath.LocalPath);
		SourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath.LocalPath);
		Exporter = ExporterService.GetExporter(Extension, entityType);
		EntityType = entityType;
		IEnumerable<string> sourceObjectNames = Exporter.GetSourceObjectNames(sourceFilePath.LocalPath, entityType);
		foreach (string item in sourceObjectNames)
		{
			_sourceObjects.Add(new SourceObjectModel(sourceFilePath, SourceFileName, item));
		}
		if (File.Exists(sourceFilePath.LocalPath))
		{
			LastModifiedTime = File.GetLastWriteTime(sourceFilePath.LocalPath).ToLocalTime();
		}
	}
}
