using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;

namespace Firaxis.ATF;

public class CookerHotLoadData : IHotLoadData
{
	private readonly ICollection<Uri> m_dependencyFileUris = new List<Uri>();

	private readonly ICollection<string> m_relativePackagePaths = new List<string>();

	private readonly ICollection<string> m_relativeArtDefPaths = new List<string>();

	public IEnumerable<Uri> DependencyFileUris => m_dependencyFileUris;

	public IEnumerable<string> RelativeArtDefPaths => m_relativeArtDefPaths;

	public IEnumerable<string> RelativePackagePaths => m_relativePackagePaths;

	public CookerHotLoadData(string artDefPantry, DateTime lastCookedTime, IEnumerable<Uri> urisToCook, string blpOutputRoot, string artDefOutputRoot, string dependencyOutputRoot, ICivTechService civTechService)
	{
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		new List<Uri>();
		foreach (Uri item in urisToCook)
		{
			string localPath = item.LocalPath;
			string extension = Path.GetExtension(localPath);
			if (extension.Equals(".xlp", StringComparison.CurrentCultureIgnoreCase))
			{
				using IXLP iXLP = civTechContext.CreateInstance<IXLP>();
				if ((bool)iXLP.DeserializeFromFile(localPath))
				{
					string text = iXLP.Package.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					string filePath = Path.Combine(blpOutputRoot, text + ".blp");
					if (WasFileCooked(filePath, lastCookedTime))
					{
						m_relativePackagePaths.Add(iXLP.Package + ".blp");
					}
				}
			}
			else if (extension.Equals(".artdef", StringComparison.CurrentCultureIgnoreCase))
			{
				IProjectConfig config = civTechService.PrimaryProject.Config;
				using IArtDef artDef = civTechContext.CreateInstance<IArtDef>(new object[1] { config });
				if ((bool)artDef.DeserializeFromFile(localPath))
				{
					string relativePath = GetRelativePath(localPath, artDefPantry);
					string filePath2 = Path.Combine(artDefOutputRoot, relativePath);
					if (WasFileCooked(filePath2, lastCookedTime))
					{
						m_relativeArtDefPaths.Add(relativePath);
					}
				}
			}
			else
			{
				if (!localPath.EndsWith(".art.xml", StringComparison.CurrentCultureIgnoreCase))
				{
					continue;
				}
				IGameArtSpecification gameArtSpecification = civTechContext.CreateInstance<IGameArtSpecification>();
				if ((bool)gameArtSpecification.DeserializeFromFile(localPath))
				{
					string text2 = Path.Combine(dependencyOutputRoot, gameArtSpecification.ID.Name + ".dep");
					if (WasFileCooked(text2, lastCookedTime) && Uri.TryCreate(text2, UriKind.Absolute, out var result))
					{
						m_dependencyFileUris.Add(result);
					}
				}
			}
		}
	}

	private string GetRelativePath(string localPath, string pantryPath)
	{
		if (string.IsNullOrEmpty(pantryPath))
		{
			return string.Empty;
		}
		if (!localPath.StartsWith(pantryPath, StringComparison.OrdinalIgnoreCase))
		{
			return localPath;
		}
		return localPath.Substring(pantryPath.Length).TrimStart(Path.DirectorySeparatorChar);
	}

	private bool WasFileCooked(string filePath, DateTime lastCookedTime)
	{
		if (File.Exists(filePath) && new FileInfo(filePath).LastWriteTimeUtc >= lastCookedTime.ToUniversalTime())
		{
			return true;
		}
		return false;
	}
}
