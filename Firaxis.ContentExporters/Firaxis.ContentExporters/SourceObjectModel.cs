using System;
using System.IO;

namespace Firaxis.ContentExporters;

public class SourceObjectModel
{
	private string _smartName;

	private string _dispayName;

	public Uri SourceFilePath { get; private set; }

	public string SourceFileName { get; private set; }

	public string SourceObjectName { get; private set; }

	public string SmartName
	{
		get
		{
			if (_smartName == null)
			{
				string text = Path.GetExtension(SourceFilePath.LocalPath).ToLower();
				if (string.IsNullOrWhiteSpace(SourceObjectName) || SourceObjectName.Equals("loose_layers_only"))
				{
					_smartName = SourceFileName;
				}
				else if (text.Contains("psd"))
				{
					_smartName = $"{SourceFileName}_{SourceObjectName}";
				}
				else
				{
					_smartName = SourceObjectName;
				}
				_smartName = _smartName.Replace(':', '_');
			}
			return _smartName;
		}
	}

	public string DisplayName
	{
		get
		{
			if (_dispayName == null)
			{
				if (string.IsNullOrWhiteSpace(SourceObjectName))
				{
					_dispayName = SourceFileName;
				}
				else
				{
					_dispayName = SourceObjectName;
				}
			}
			return _dispayName;
		}
	}

	public SourceObjectModel(Uri sourceFilePath, string sourceFileName, string sourceObjectName)
	{
		SourceFilePath = sourceFilePath;
		SourceFileName = sourceFileName;
		SourceObjectName = sourceObjectName;
	}
}
