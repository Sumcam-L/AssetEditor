using System;
using System.IO;
using Firaxis.IO;

namespace UtilityTools.Helpers;

public class AssetFileManager
{
	public static string getXMLData(string filePath)
	{
		WindowsPath windowsPath = new WindowsPath(filePath);
		string result = "";
		if (windowsPath != null)
		{
			try
			{
				result = File.ReadAllText(windowsPath.FullPath);
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine("File not found: {0}", ex.FileName);
			}
			catch (IOException ex2)
			{
				if (ex2.Source != null)
				{
					Console.WriteLine("IOException source: {0}", ex2.Source);
				}
				throw;
			}
		}
		return result;
	}
}
