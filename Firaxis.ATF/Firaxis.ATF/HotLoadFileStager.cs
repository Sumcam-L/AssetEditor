using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Firaxis.ATF;

public static class HotLoadStagingGate
{
	public static bool StageThenSend(Action stage, Action send, Action<System.Exception> onFailure)
	{
		try
		{
			stage();
		}
		catch (System.Exception obj)
		{
			onFailure?.Invoke(obj);
			return false;
		}
		send();
		return true;
	}
}

public sealed class HotLoadFileStager
{
	private const string GameName = "Sid Meier's Civilization VI";

	private readonly string _projectName;

	private readonly string _gamePantry;

	private readonly string _artDefOutputRoot;

	private readonly string _xlpOutputRoot;

	private readonly string _deployedModRoot;

	public HotLoadFileStager(string projectName, string gamePantry, string artDefOutputRoot, string xlpOutputRoot, string personalFolder)
	{
		if (string.IsNullOrWhiteSpace(projectName) || Path.GetFileName(projectName) != projectName || projectName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
		{
			throw new ArgumentException("The active project name is not a valid directory name.", "projectName");
		}
		_projectName = projectName;
		_gamePantry = RequireRoot(gamePantry, "gamePantry");
		_artDefOutputRoot = RequireRoot(artDefOutputRoot, "artDefOutputRoot");
		_xlpOutputRoot = RequireRoot(xlpOutputRoot, "xlpOutputRoot");
		string text = RequireRoot(personalFolder, "personalFolder");
		_deployedModRoot = Path.Combine(text, "My Games", GameName, "Mods", projectName);
	}

	public void Stage(IEnumerable<string> relativeArtDefPaths, IEnumerable<string> relativePackagePaths, Action<string, string> onStaged = null)
	{
		ValidateProjectIdentity();
		List<StagedPath> list = new List<StagedPath>();
		AddMappings(list, relativeArtDefPaths, _artDefOutputRoot, Path.Combine(_deployedModRoot, "ArtDefs"));
		AddMappings(list, relativePackagePaths, _xlpOutputRoot, Path.Combine(_deployedModRoot, "Platforms", "Windows", "BLPs"));
		foreach (StagedPath item in list)
		{
			CopyAtomically(item.Source, item.Destination);
			onStaged?.Invoke(item.Source, item.Destination);
		}
	}

	private void ValidateProjectIdentity()
	{
		string text = Path.Combine(_gamePantry, _projectName + ".civ6proj");
		string text2 = Path.Combine(_deployedModRoot, _projectName + ".modinfo");
		if (!File.Exists(text))
		{
			throw new FileNotFoundException("The active Mod project file was not found.", text);
		}
		if (!File.Exists(text2))
		{
			throw new FileNotFoundException("The deployed Mod was not found. Build/deploy it with ModBuddy before using Hotload.", text2);
		}
		string xmlValue = LoadXmlValue(text, "//*[local-name()='Guid']", "The active Mod project does not contain a Guid.");
		string xmlValue2 = LoadXmlValue(text2, "/*[local-name()='Mod']/@id", "The deployed Mod info does not contain a Mod id.");
		Guid result;
		Guid result2;
		if (!Guid.TryParse(xmlValue, out result) || !Guid.TryParse(xmlValue2, out result2) || result != result2)
		{
			throw new InvalidOperationException("The deployed Mod does not match the active AssetEditor project. Source ID: '" + xmlValue + "'; deployed ID: '" + xmlValue2 + "'.");
		}
	}

	private static string LoadXmlValue(string path, string xpath, string missingValueMessage)
	{
		XmlDocument xmlDocument = new XmlDocument
		{
			XmlResolver = null
		};
		xmlDocument.Load(path);
		XmlNode xmlNode = xmlDocument.SelectSingleNode(xpath);
		string text = xmlNode?.Value ?? xmlNode?.InnerText;
		if (string.IsNullOrWhiteSpace(text))
		{
			throw new InvalidOperationException(missingValueMessage + " File: " + path);
		}
		return text;
	}

	private static void AddMappings(ICollection<StagedPath> mappings, IEnumerable<string> relativePaths, string sourceRoot, string destinationRoot)
	{
		if (relativePaths == null)
		{
			return;
		}
		foreach (string item in relativePaths.Where((string path) => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase))
		{
			string safePath = GetSafePath(sourceRoot, item);
			string safePath2 = GetSafePath(destinationRoot, item);
			if (!File.Exists(safePath))
			{
				throw new FileNotFoundException("A cooked Hotload file was not found.", safePath);
			}
			mappings.Add(new StagedPath(safePath, safePath2));
		}
	}

	private static string GetSafePath(string root, string relativePath)
	{
		if (Path.IsPathRooted(relativePath))
		{
			throw new InvalidOperationException("Hotload paths must be relative: " + relativePath);
		}
		string text = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		string fullPath = Path.GetFullPath(Path.Combine(root, text));
		string text2 = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
		if (!fullPath.StartsWith(text2, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("Hotload path escapes its allowed root: " + relativePath);
		}
		return fullPath;
	}

	private static void CopyAtomically(string source, string destination)
	{
		string directoryName = Path.GetDirectoryName(destination);
		Directory.CreateDirectory(directoryName);
		string text = destination + ".hotload-" + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			File.Copy(source, text, overwrite: true);
			if (File.Exists(destination))
			{
				File.Replace(text, destination, null);
			}
			else
			{
				File.Move(text, destination);
			}
		}
		finally
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
	}

	private static string RequireRoot(string value, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("A required Hotload path is empty.", parameterName);
		}
		return Path.GetFullPath(value);
	}

	private sealed class StagedPath
	{
		public string Source { get; }

		public string Destination { get; }

		public StagedPath(string source, string destination)
		{
			Source = source;
			Destination = destination;
		}
	}
}
