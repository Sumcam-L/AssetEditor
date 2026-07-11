using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.AssetCloudFramework;

public static class ArtDefHelper
{
	public static IArtDef LoadArtDef(Uri path, ICivTechService civTechSvc)
	{
		IArtDef artDef = Context.EnsureCreated<CivTechContext>().CreateInstance<IArtDef>(new object[1] { civTechSvc.PrimaryProject.Config });
		if (!artDef.DeserializeFromFile(path.LocalPath))
		{
			return null;
		}
		return artDef;
	}

	private static string GetPantryRelativePath(string path, ICivTechService civTechSvc)
	{
		string text = path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string oldValue = civTechSvc.PrimaryProject.Paths.GamePantry.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		return text.Replace(oldValue, "");
	}

	public static IEnumerable<Uri> EnumerateArtDefs(ICivTechService civTechSvc, SearchOption searchOption)
	{
		if (!Directory.Exists(civTechSvc.PrimaryProject.Paths.ArtDefRoot))
		{
			yield break;
		}
		foreach (string path in Directory.EnumerateFiles(civTechSvc.PrimaryProject.Paths.ArtDefRoot, "*.artdef", searchOption))
		{
			yield return new Uri(path);
		}
	}
}
