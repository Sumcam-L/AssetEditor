using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetCloudFramework.Data;

public class SourceControlServiceStatusResult : IServiceNameProvider, ISourceControlServiceRequest
{
	public string Description { get; set; }

	public List<EntityFileInfo> EntityFileInfo { get; internal set; }

	public SourceControlServiceStatusResult()
	{
		Description = string.Empty;
		EntityFileInfo = new List<EntityFileInfo>();
	}

	private string GetPantryFolder(string gamePantry, InstanceType instType)
	{
		string text = StaticMethods.PantryRootForInstanceType(gamePantry, instType);
		if (text != null)
		{
			text = text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		}
		return text;
	}

	public virtual IEnumerable<string> GetFullyQualifiedFilePaths(string gamePantry)
	{
		List<string> list = new List<string>();
		foreach (EntityFileInfo item in EntityFileInfo)
		{
			string pantryFolder = GetPantryFolder(gamePantry, item.instanceType);
			if (pantryFolder == null)
			{
				continue;
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(pantryFolder);
			FileInfo[] files = directoryInfo.GetFiles(item.name + ".*", SearchOption.TopDirectoryOnly);
			if (files.Length == 0)
			{
				continue;
			}
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				int num = fileInfo.Name.LastIndexOf('.');
				if (num > -1 && num <= item.name.Length && fileInfo.Name.Substring(0, num) == item.name.Substring(item.name.Length - num))
				{
					list.Add(fileInfo.FullName);
				}
			}
		}
		return list;
	}

	public string GetServiceName()
	{
		return "SourceControlService";
	}
}
