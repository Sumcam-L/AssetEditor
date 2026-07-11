using System;
using System.IO;
using System.Web.Script.Serialization;

namespace Firaxis.CivTech;

public static class VersionControlInfoHelpers
{
	public static string GenerateVersionControlConfigPathForProject(string projName)
	{
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "AssetCloud", $"{projName}.vcs");
	}

	public static VersionControlInfo LoadVersionControlInfo(string projName)
	{
		string text = LoadVersionControlInfoJsonFromFile(projName);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return LoadVersionControlInfoFromJsonText(text);
	}

	public static void SaveVersionControlInfo(string projName, VersionControlInfo info)
	{
		string text = SaveVersionControlInfoToJsonText(info);
		if (!string.IsNullOrEmpty(text))
		{
			SaveVersionControlInfoJsonTextToFile(GenerateVersionControlConfigPathForProject(projName), text);
		}
	}

	private static void SaveVersionControlInfoJsonTextToFile(string projPath, string jsonText)
	{
		try
		{
			TextWriter textWriter = File.CreateText(projPath);
			try
			{
				textWriter.Write(jsonText);
			}
			catch (IOException)
			{
			}
			finally
			{
				textWriter.Dispose();
			}
		}
		catch (IOException)
		{
		}
	}

	private static string SaveVersionControlInfoToJsonText(VersionControlInfo info)
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 20971520;
		try
		{
			return javaScriptSerializer.Serialize(info);
		}
		catch (Exception)
		{
		}
		return string.Empty;
	}

	private static VersionControlInfo LoadVersionControlInfoFromJsonText(string jsonText)
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = 20971520;
		try
		{
			return javaScriptSerializer.Deserialize<VersionControlInfo>(jsonText);
		}
		catch (Exception)
		{
		}
		return null;
	}

	private static string LoadVersionControlInfoJsonFromFile(string projName)
	{
		string path = GenerateVersionControlConfigPathForProject(projName);
		string result = string.Empty;
		try
		{
			TextReader textReader = File.OpenText(path);
			try
			{
				result = textReader.ReadToEnd();
			}
			catch (IOException)
			{
			}
			finally
			{
				textReader.Dispose();
			}
		}
		catch (IOException)
		{
		}
		return result;
	}
}
