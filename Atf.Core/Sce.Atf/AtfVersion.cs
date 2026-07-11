using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;

namespace Sce.Atf;

public class AtfVersion
{
	public static Version GetVersion()
	{
		return GetVersion(Assembly.GetExecutingAssembly());
	}

	public static Version GetVersion(Assembly assem)
	{
		if (assem is AssemblyBuilder)
		{
			return null;
		}
		string text = null;
		string[] manifestResourceNames = assem.GetManifestResourceNames();
		string[] array = manifestResourceNames;
		foreach (string text2 in array)
		{
			if (text2.EndsWith("wws_atf.component", StringComparison.OrdinalIgnoreCase))
			{
				text = text2;
			}
		}
		if (text != null)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(assem.GetManifestResourceStream(text));
			XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("xsi", "http://www.ship.scea.com/wws_sdk/component/v1");
			XmlNode xmlNode = xmlDocument.SelectSingleNode("xsi:component/xsi:version", xmlNamespaceManager);
			if (xmlNode != null)
			{
				return new Version(xmlNode.InnerText);
			}
		}
		text = null;
		string[] array2 = manifestResourceNames;
		foreach (string text3 in array2)
		{
			if (text3.EndsWith("AtfVersion.xml", StringComparison.OrdinalIgnoreCase))
			{
				text = text3;
			}
		}
		if (text != null)
		{
			XmlDocument xmlDocument2 = new XmlDocument();
			xmlDocument2.Load(assem.GetManifestResourceStream(text));
			XmlNode xmlNode2 = xmlDocument2.SelectSingleNode("root/AtfVersion");
			if (xmlNode2 != null)
			{
				return new Version(xmlNode2.InnerText);
			}
		}
		return null;
	}

	public static Version GetEntryAssemblyVersion()
	{
		return Assembly.GetEntryAssembly()?.GetName().Version ?? Assembly.GetExecutingAssembly()?.GetName().Version;
	}
}
