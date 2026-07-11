using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.AssetCloudFramework;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.CookerInterface;
using Firaxis.Utility;

namespace Firaxis.ATF;

public class BatchCooker : IBatchCooker
{
	private readonly CivTechContext m_civTechContext = Context.EnsureCreated<CivTechContext>();

	private DateTime m_lastCookTime = DateTime.Now.ToUniversalTime();

	private CookerHotLoadData m_lastHotLoadData;

	protected ICivTechService CivTechService { get; private set; }

	protected ICookService CookService { get; private set; }

	public BatchCooker(ICivTechService civTechService, ICookService cookService)
	{
		if (civTechService == null)
		{
			throw new ArgumentNullException("civTechService");
		}
		if (cookService == null)
		{
			throw new ArgumentNullException("cookService");
		}
		CivTechService = civTechService;
		CookService = cookService;
	}

	public virtual CookResult Cook(IEnumerable<Uri> urisToCook)
	{
		ICookerOptions cookOptions = CookHelpers.CreateCookerArgs(CivTechService, useAbsolutePaths: false);
		return CookImpl(urisToCook, cookOptions);
	}

	public virtual IHotLoadData GetHotLoadData()
	{
		return m_lastHotLoadData;
	}

	protected CookResult CookImpl(IEnumerable<Uri> urisToCook, ICookerOptions cookOptions)
	{
		m_lastCookTime = DateTime.Now;
		if (cookOptions.PackageRoot.Contains("{PLATFORM}"))
		{
			string platformDirectory = StaticMethods.GetPlatformDirectory(cookOptions.Platform);
			cookOptions.PackageRoot = cookOptions.PackageRoot.Replace("{PLATFORM}", platformDirectory);
		}
		string xLPRoot = CivTechService.PrimaryProject.Paths.XLPRoot;
		string artDefRoot = CivTechService.PrimaryProject.Paths.ArtDefRoot;
		string gamePantry = CivTechService.PrimaryProject.Paths.GamePantry;
		string packageRoot = cookOptions.PackageRoot;
		string artDefDestinationRoot = cookOptions.ArtDefDestinationRoot;
		string dependencyOutputRoot = cookOptions.DependencyOutputRoot;
		ICollection<string> collection = new List<string>();
		ICollection<string> collection2 = new List<string>();
		ICollection<string> collection3 = new List<string>();
		foreach (Uri item in urisToCook)
		{
			string localPath = item.LocalPath;
			string extension = Path.GetExtension(localPath);
			if (extension.Equals(".xlp", StringComparison.CurrentCultureIgnoreCase))
			{
				AddPathToCollection(localPath, xLPRoot, collection);
			}
			else if (extension.Equals(".artdef", StringComparison.CurrentCultureIgnoreCase))
			{
				AddPathToCollection(localPath, artDefRoot, collection2);
			}
			else if (localPath.EndsWith(".art.xml", StringComparison.CurrentCultureIgnoreCase))
			{
				AddPathToCollection(localPath, gamePantry, collection3);
			}
		}
		CookResult cookResult = PerformCook(cookOptions, CookerMode.XLP, collection, cookOptions.XLPs);
		CookResult cookResult2 = PerformCook(cookOptions, CookerMode.ArtDef, collection2, cookOptions.ArtDefs);
		CookResult cookResult3 = PerformCook(cookOptions, CookerMode.ArtSpecification, collection3, cookOptions.ArtSpecificationFiles);
		List<CookItemResultCode> list = new List<CookItemResultCode>();
		if (cookResult != null)
		{
			list.AddRange(cookResult.ItemResults);
		}
		if (cookResult2 != null)
		{
			list.AddRange(cookResult2.ItemResults);
		}
		if (cookResult3 != null)
		{
			list.AddRange(cookResult3.ItemResults);
		}
		m_lastHotLoadData = new CookerHotLoadData(artDefRoot, m_lastCookTime, urisToCook, packageRoot, artDefDestinationRoot, dependencyOutputRoot, CivTechService);
		return new CookResult(list);
	}

	private void AddPathToCollection(string localPath, string pantryPath, ICollection<string> collection)
	{
		if (!string.IsNullOrEmpty(pantryPath))
		{
			string relativePath = GetRelativePath(localPath, pantryPath);
			collection.Add(relativePath);
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

	private CookResult PerformCook(ICookerOptions cookOptions, CookerMode mode, IEnumerable<string> sourceCollection, ICollection<string> fileCollection)
	{
		if (!sourceCollection.Any())
		{
			return null;
		}
		cookOptions.Mode = mode;
		foreach (string item in sourceCollection)
		{
			fileCollection.Add(item);
		}
		CookResult result = CookService.CookCustom(cookOptions);
		fileCollection.Clear();
		return result;
	}
}
