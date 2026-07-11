using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Firaxis.Error;
using Firaxis.Utility.Properties;
using Microsoft.Win32;

namespace Firaxis.Utility;

public class ProjectConfig
{
	public XmlDocument Xml { get; private set; }

	public List<string> Projects { get; private set; }

	public ProjectPathMappingInfoDictionary PathMappings { get; private set; }

	public IEnumerable<string> MappedPaths
	{
		get
		{
			foreach (PathMappingInfoDictionary kPMID in PathMappings.Values)
			{
				foreach (PathMappingInfo value in kPMID.Values)
				{
					yield return value.DepotPath;
				}
			}
		}
	}

	public PathMappingInfoDictionary ProjectPathMappings => PathMappings[CurrentProjectName];

	public IEnumerable<string> ProjectMappedPaths
	{
		get
		{
			foreach (PathMappingInfo value in ProjectPathMappings.Values)
			{
				yield return value.DepotPath;
			}
		}
	}

	public string OptionsRegistryKeyName => $"{Resources.ProjectsRegKey}\\{CurrentProjectName}";

	public RegistryKey OptionsRegistryKey
	{
		get
		{
			RegistryKey registryKey = null;
			registryKey = Registry.CurrentUser.OpenSubKey(OptionsRegistryKeyName, writable: true);
			if (registryKey != null)
			{
				return registryKey;
			}
			return Registry.CurrentUser.CreateSubKey(OptionsRegistryKeyName);
		}
	}

	public static bool RegistryKeyExists
	{
		get
		{
			RegistryKey registryKey = null;
			bool result = true;
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey(Resources.ConfigRegKey, writable: false);
				result = registryKey != null;
			}
			catch
			{
				result = false;
			}
			finally
			{
				registryKey?.Close();
			}
			return result;
		}
	}

	public Version Version
	{
		get
		{
			if (Xml == null)
			{
				return null;
			}
			XmlNodeList elementsByTagName = Xml.GetElementsByTagName("FiraxisProjects");
			foreach (XmlNode item in elementsByTagName)
			{
				foreach (XmlAttribute attribute in item.Attributes)
				{
					if (attribute.Name == "version")
					{
						return new Version(attribute.Value);
					}
				}
			}
			return null;
		}
	}

	public string DefaultProjectName
	{
		get
		{
			if (Xml != null)
			{
				XmlDoc xmlDoc = new XmlDoc(Xml);
				XmlNode node;
				if ((node = xmlDoc.Find("FiraxisProject")) != null)
				{
					return xmlDoc.GetAttrib(node, "name");
				}
			}
			return "";
		}
	}

	public string CurrentProjectName
	{
		get
		{
			RegistryKey registryKey = null;
			registryKey = Registry.CurrentUser.OpenSubKey(Resources.ProjectsRegKey, writable: false);
			if (registryKey != null)
			{
				return registryKey.GetValue("CurrentProjectName", DefaultProjectName) as string;
			}
			return DefaultProjectName;
		}
		set
		{
			RegistryKey registryKey = null;
			registryKey = Registry.CurrentUser.OpenSubKey(Resources.ProjectsRegKey, writable: true);
			if (registryKey == null)
			{
				registryKey = Registry.CurrentUser.CreateSubKey(Resources.ProjectsRegKey);
			}
			registryKey.SetValue("CurrentProjectName", value);
		}
	}

	public XmlNode CurrentProjectNode
	{
		get
		{
			if (Xml != null)
			{
				XmlDoc xmlDoc = new XmlDoc(Xml);
				string currentProjectName = CurrentProjectName;
				for (XmlNode xmlNode = xmlDoc.Find("FiraxisProject"); xmlNode != null; xmlNode = xmlDoc.Sibling(xmlNode))
				{
					if (string.Compare(xmlDoc.GetAttrib(xmlNode, "name"), currentProjectName, ignoreCase: true) == 0)
					{
						return xmlNode;
					}
				}
			}
			return null;
		}
	}

	public ProjectConfig()
	{
		Projects = new List<string>();
		PathMappings = new ProjectPathMappingInfoDictionary();
		Xml = new XmlDocument();
	}

	public bool Init()
	{
		if (Available.Enterprise)
		{
			return InitEnterprise();
		}
		return true;
	}

	public bool Init(XmlDocument kDocument)
	{
		if (Xml.ChildNodes.Count > 0)
		{
			XPathNavigator xPathNavigator = Xml.CreateNavigator();
			XPathNodeIterator xPathNodeIterator = xPathNavigator.Select("/FiraxisProjects");
			xPathNodeIterator.MoveNext();
			XPathNavigator xPathNavigator2 = kDocument.CreateNavigator();
			XPathNodeIterator xPathNodeIterator2 = xPathNavigator2.Select("//FiraxisProject");
			while (xPathNodeIterator2.MoveNext())
			{
				if (xPathNodeIterator2.Current.IsNode)
				{
					xPathNodeIterator.Current.AppendChild(xPathNodeIterator2.Current);
				}
			}
		}
		else
		{
			Xml = kDocument;
		}
		return true;
	}

	private bool InitEnterprise()
	{
		RegistryKey registryKey = null;
		bool flag = true;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey(Resources.ConfigRegKey, writable: false);
			if (registryKey != null)
			{
				string filename = (string)registryKey.GetValue(Resources.ConfigXmlName, "");
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(filename);
				Init(xmlDocument);
			}
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e, "Init config files");
			flag = false;
		}
		finally
		{
			registryKey?.Close();
		}
		if (flag)
		{
			PopulateLists();
		}
		return flag;
	}

	public PathMappingInfoDictionary GetProjectPaths()
	{
		PathMappingInfoDictionary result = new PathMappingInfoDictionary();
		XmlNode currentProjectNode;
		if ((currentProjectNode = CurrentProjectNode) != null)
		{
			XmlDoc xmlDoc = new XmlDoc(Xml);
		}
		return result;
	}

	public T GetOption<T>(string szCategory, string szOption, T DefaultValue)
	{
		RegistryKey registryKey = OptionsRegistryKey.CreateSubKey(szCategory);
		if (registryKey == null)
		{
			return DefaultValue;
		}
		object value = registryKey.GetValue(szOption);
		if (value != null)
		{
			return (T)Convert.ChangeType(value, typeof(T));
		}
		return DefaultValue;
	}

	public void SetOption<T>(string szCategory, string szOption, T Value)
	{
		RegistryKey registryKey = OptionsRegistryKey.CreateSubKey(szCategory);
		registryKey.SetValue(szOption, Value);
	}

	public string ConvertPathLocation(string szPath, string szFromLocation, string szToLocation)
	{
		PathMappingInfoDictionary pathMappingInfoDictionary = PathMappings[CurrentProjectName];
		if (!pathMappingInfoDictionary.ContainsKey(szFromLocation) || !pathMappingInfoDictionary.ContainsKey(szToLocation))
		{
			return null;
		}
		string localPath = pathMappingInfoDictionary[szFromLocation].LocalPath;
		string localPath2 = pathMappingInfoDictionary[szToLocation].LocalPath;
		localPath = localPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		if (!szPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).StartsWith(localPath, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		string text = szPath.Remove(0, localPath.Length);
		return localPath2 + text;
	}

	public string GetControlledLocalPath(string mappedPathName)
	{
		return GetLocalPath(mappedPathName);
	}

	public void SetLocalPath(string project, string mappedPathName, string localPath)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.CurrentUser.CreateSubKey($"{Resources.ProjectsRegKey}\\{project}");
			registryKey?.SetValue(mappedPathName, localPath);
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e, "Set Local Path");
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public string GetLocalPath(string mappedPathName)
	{
		string currentProjectName = CurrentProjectName;
		string result = "";
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.CurrentUser.OpenSubKey($"{Resources.ProjectsRegKey}\\{currentProjectName}", writable: false);
			if (registryKey != null)
			{
				result = (string)registryKey.GetValue(mappedPathName);
			}
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e, "Get Local Path");
		}
		finally
		{
			registryKey?.Close();
		}
		return result;
	}

	private void PopulateLists()
	{
		XmlDoc xmlDoc = new XmlDoc(Xml);
		Projects.Clear();
		PathMappings.Clear();
		try
		{
			XmlNode node = xmlDoc.Find("FiraxisProjects");
			for (node = xmlDoc.Child(node, "FiraxisProject"); node != null; node = xmlDoc.Sibling(node, "FiraxisProject"))
			{
				string attrib = xmlDoc.GetAttrib(node, "name");
				PathMappingInfoDictionary value = new PathMappingInfoDictionary();
				Projects.Add(attrib);
				PathMappings.Add(attrib, value);
			}
		}
		catch (Exception e)
		{
			ExceptionLogger.Log(e, "PopulateLists");
		}
	}
}
