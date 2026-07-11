using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace Sce.Atf.Wpf.Models;

public class AboutDialogViewModel : DialogViewModelBase
{
	private XmlDocument m_xmlDoc = null;

	private const string m_propertyNameTitle = "Title";

	private const string m_propertyNameDescription = "Description";

	private const string m_propertyNameProduct = "Product";

	private const string m_propertyNameCopyright = "Copyright";

	private const string m_propertyNameCompany = "Company";

	private const string m_xPathRoot = "ApplicationInfo/";

	private const string m_xPathTitle = "ApplicationInfo/Title";

	private const string m_xPathVersion = "ApplicationInfo/Version";

	private const string m_xPathDescription = "ApplicationInfo/Description";

	private const string m_xPathProduct = "ApplicationInfo/Product";

	private const string m_xPathCopyright = "ApplicationInfo/Copyright";

	private const string m_xPathCompany = "ApplicationInfo/Company";

	private const string m_xPathLink = "ApplicationInfo/Link";

	private const string m_xPathLinkUri = "ApplicationInfo/Link/@Uri";

	public List<string> Assemblies { get; private set; }

	public string ProductTitle
	{
		get
		{
			string text = Product;
			if (string.IsNullOrEmpty(text))
			{
				text = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
			}
			return text;
		}
	}

	public string Version => AtfVersion.GetVersion().ToString();

	public string FileVersion
	{
		get
		{
			string empty = string.Empty;
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				return (customAttributes[0] as AssemblyFileVersionAttribute).Version;
			}
			Version version = entryAssembly.GetName().Version;
			if (version != null)
			{
				return version.ToString();
			}
			return GetLogicalResourceString("ApplicationInfo/Version");
		}
	}

	public string Description => CalculatePropertyValue<AssemblyDescriptionAttribute>("Description", "ApplicationInfo/Description");

	public string Product => CalculatePropertyValue<AssemblyProductAttribute>("Product", "ApplicationInfo/Product");

	public string Copyright => CalculatePropertyValue<AssemblyCopyrightAttribute>("Copyright", "ApplicationInfo/Copyright");

	public string Company => CalculatePropertyValue<AssemblyCompanyAttribute>("Company", "ApplicationInfo/Company");

	public string LinkText => GetLogicalResourceString("ApplicationInfo/Link");

	public string LinkUri => GetLogicalResourceString("ApplicationInfo/Link/@Uri");

	public Uri Banner
	{
		get
		{
			Uri result = null;
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			AssemblyName name = entryAssembly.GetName();
			object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyBannerAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				AssemblyBannerAttribute assemblyBannerAttribute = (AssemblyBannerAttribute)customAttributes[0];
				PropertyInfo property = assemblyBannerAttribute.GetType().GetProperty("BannerPath", BindingFlags.Instance | BindingFlags.Public);
				if (property != null)
				{
					string text = property.GetValue(customAttributes[0], null) as string;
					if (!string.IsNullOrEmpty(text))
					{
						string text2 = string.Concat("/", name.Name, ";v", name.Version, ";component/", text);
						string uriString = "pack://application:,,," + text2;
						result = new Uri(uriString);
					}
				}
			}
			return result;
		}
	}

	protected virtual XmlDocument ResourceXmlDocument
	{
		get
		{
			if (m_xmlDoc == null && Application.Current.TryFindResource("aboutProvider") is XmlDataProvider xmlDataProvider)
			{
				m_xmlDoc = xmlDataProvider.Document;
			}
			return m_xmlDoc;
		}
	}

	public AboutDialogViewModel()
	{
		base.Title = "About".Localize() + " " + ProductTitle;
		Assemblies = new List<string>();
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		AssemblyName[] referencedAssemblies = entryAssembly.GetReferencedAssemblies();
		foreach (AssemblyName assemblyName in referencedAssemblies)
		{
			Assemblies.Add(assemblyName.Name + ", Version=" + assemblyName.Version);
		}
	}

	protected virtual string GetLogicalResourceString(string xpathQuery)
	{
		string result = string.Empty;
		XmlDocument resourceXmlDocument = ResourceXmlDocument;
		if (resourceXmlDocument != null)
		{
			XmlNode xmlNode = resourceXmlDocument.SelectSingleNode(xpathQuery);
			if (xmlNode != null)
			{
				result = ((!(xmlNode is XmlAttribute)) ? xmlNode.InnerText : xmlNode.Value);
			}
		}
		return result;
	}

	private string CalculatePropertyValue<T>(string propertyName, string xpathQuery)
	{
		string text = string.Empty;
		object[] customAttributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(T), inherit: false);
		if (customAttributes.Length != 0)
		{
			PropertyInfo property = ((T)customAttributes[0]/*cast due to .constrained prefix*/).GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
			if (property != null)
			{
				text = property.GetValue(customAttributes[0], null) as string;
			}
		}
		if (text == string.Empty)
		{
			text = GetLogicalResourceString(xpathQuery);
		}
		return text;
	}
}
