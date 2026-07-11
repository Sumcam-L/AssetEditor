using System.IO;
using System.Linq;

namespace Firaxis.CivTech.CookerInterface;

public static class ICookerOptionsExtensions
{
	public static string GetPantryDirectory(this ICookerOptions options, ProjectPaths projectPaths)
	{
		string result = string.Empty;
		if (options.PantryRoots.Any())
		{
			string path = options.PantryRoots.First();
			if (options.Mode == CookerMode.ArtDef)
			{
				result = Path.Combine(path, projectPaths.ArtDefRoot);
			}
			else if (options.Mode == CookerMode.XLP)
			{
				result = Path.Combine(path, projectPaths.XLPRoot);
			}
		}
		return result;
	}
}
