using System;
using System.IO;
using System.Xml;

namespace Sce.Atf;

public class SatelliteAssemblyStringLocalizer : XmlStringLocalizer
{
	public SatelliteAssemblyStringLocalizer()
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		baseDirectory = Path.Combine(baseDirectory, "Resources\\");
		baseDirectory = PathUtil.GetCulturePath(baseDirectory);
		try
		{
			string[] files = Directory.GetFiles(baseDirectory, "*.Localization.xml", SearchOption.TopDirectoryOnly);
			foreach (string filename in files)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(filename);
				AddLocalizedStrings(xmlDocument);
			}
		}
		catch (IOException ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
		}
	}
}
