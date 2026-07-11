using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Collections;

namespace Firaxis.AssetCloudFramework.Data;

public class SourceControlServiceRequest : ISourceControlServiceRequest, ISourceControlServiceRequestDescriptor, IQueryService, IValidateService, IServiceNameProvider
{
	public string Description { get; set; }

	public SerializableDictionary<InstanceType, List<string>> InstanceItems { get; set; }

	public SourceControlServiceRequest()
	{
		Description = string.Empty;
		InstanceItems = new SerializableDictionary<InstanceType, List<string>>();
		for (int i = 0; i < 13; i++)
		{
			InstanceItems.Add((InstanceType)i, new List<string>());
		}
	}

	public SourceControlServiceRequest(IEnumerable<IInstanceEntity> entities)
		: this()
	{
		foreach (IInstanceEntity entity in entities)
		{
			InstanceItems[entity.Type].Add(entity.Name);
		}
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
		List<string> files = new List<string>();
		Action<string, DirectoryInfo, List<string>> action = delegate(string name, DirectoryInfo directoryInfo, List<string> errors)
		{
			FileInfo[] files2 = directoryInfo.GetFiles(name + ".*", SearchOption.TopDirectoryOnly);
			if (files2.Length == 0)
			{
				errors.Add($"- No files found for '{name}'");
			}
			else
			{
				FileInfo[] array = files2;
				foreach (FileInfo fileInfo in array)
				{
					int num2 = fileInfo.Name.LastIndexOf('.');
					if (num2 > -1 && num2 <= name.Length && fileInfo.Name.Substring(0, num2) == name.Substring(name.Length - num2))
					{
						files.Add(fileInfo.FullName);
					}
				}
			}
		};
		List<string> errorMsgs = new List<string>();
		for (int num = 0; num < 13; num++)
		{
			string pantryFolder = GetPantryFolder(gamePantry, (InstanceType)num);
			if (pantryFolder != null)
			{
				DirectoryInfo info = new DirectoryInfo(pantryFolder);
				InstanceItems[(InstanceType)num].ForEach(delegate(string name)
				{
					action(name, info, errorMsgs);
				});
			}
		}
		return files;
	}

	public bool Validate()
	{
		if (Description != null)
		{
			for (int i = 0; i < 13; i++)
			{
				if (InstanceItems[(InstanceType)i].Contains(null))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public string GetServiceName()
	{
		return "SourceControlService";
	}
}
