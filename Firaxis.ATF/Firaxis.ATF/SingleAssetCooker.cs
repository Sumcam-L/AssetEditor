using System;
using System.Collections.Generic;
using Firaxis.AssetCloudFramework;
using Firaxis.CivTech;
using Firaxis.CivTech.CookerInterface;

namespace Firaxis.ATF;

public class SingleAssetCooker : BatchCooker, IBatchCookerOutputPathsOverride
{
	private ITemporaryArtOutputPaths m_overridePaths;

	public SingleAssetCooker(ICivTechService civTechService, ICookService cookService)
		: base(civTechService, cookService)
	{
	}

	public void SetOutputPathOverrides(ITemporaryArtOutputPaths paths)
	{
		m_overridePaths = paths;
	}

	public override CookResult Cook(IEnumerable<Uri> urisToCook)
	{
		if (m_overridePaths == null)
		{
			return new CookResult("Temporary workspace cook location was not set!");
		}
		ICookerOptions cookerOptions = CookHelpers.CreateCookerArgs(base.CivTechService, useAbsolutePaths: true);
		cookerOptions.ArtDefDestinationRoot = m_overridePaths.ArtDefOutputDirectory;
		cookerOptions.PackageRoot = m_overridePaths.BLPOutputDirectory;
		cookerOptions.DependencyOutputRoot = m_overridePaths.TemporaryCookLocation;
		cookerOptions.PantryRoots = GeneratePantryRootsWithTempOverlay(cookerOptions.PantryRoots, m_overridePaths.TemporaryPantryDirectory);
		return CookImpl(urisToCook, cookerOptions);
	}

	private IEnumerable<string> GeneratePantryRootsWithTempOverlay(IEnumerable<string> baseRoots, string tempRoot)
	{
		List<string> list = new List<string>();
		list.Add(tempRoot);
		list.AddRange(baseRoots);
		return list;
	}
}
