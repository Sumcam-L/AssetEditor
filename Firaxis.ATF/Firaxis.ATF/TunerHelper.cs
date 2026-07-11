using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.Packages;
using Sce.Atf;

namespace Firaxis.ATF;

public static class TunerHelper
{
	public static IEnumerable<string> GetArtDefConsumerNames(string artDefRelativePath, string artDefOutputRoot)
	{
		yield return artDefRelativePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}

	public static string GetArtDefSubSystem()
	{
		return "ARTDEF";
	}

	public static IEnumerable<string> GetXLPConsumerNames(IXLP xlp, string gameDirectory)
	{
		if (xlp != null && !string.IsNullOrEmpty(gameDirectory))
		{
			_ = string.Empty;
			string text = xlp.Package + ".blp";
			string text2;
			try
			{
				text2 = text.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			}
			catch (ArgumentException ex)
			{
				string text3 = "Unable to combine path with the following elements: Game Directory: " + gameDirectory + "; BLP Relative Path: " + text;
				Outputs.WriteLine(OutputMessageType.Error, text3);
				BugSubmitter.SilentReport(string.Format("{0}\n\n{1}\n\n{2}", text3, ex.ToString(), "@assign bwhitman @summary InvalidBLPPath"));
				yield break;
			}
			yield return text2 ?? "";
		}
	}

	public static string GetXLPSubSystem(IXLP xlp)
	{
		if (xlp == null)
		{
			return string.Empty;
		}
		return "BLP";
	}
}
