using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Xml;

namespace Sce.Atf;

public class EmbeddedResourceStringLocalizer : XmlStringLocalizer
{
	private readonly string m_resourceDirectory1;

	private readonly string m_resourceDirectory2;

	public EmbeddedResourceStringLocalizer()
	{
		string twoLetterISOLanguageName = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
		string name = Thread.CurrentThread.CurrentUICulture.Name;
		name = name.Replace('-', '_');
		bool globalAssemblyCache = Assembly.GetExecutingAssembly().GlobalAssemblyCache;
		m_resourceDirectory1 = ".Resources." + twoLetterISOLanguageName + ".";
		m_resourceDirectory2 = ".Resources." + name + ".";
		Assembly[] loadedAssemblies = AssemblyUtil.GetLoadedAssemblies();
		foreach (Assembly assembly in loadedAssemblies)
		{
			if (globalAssemblyCache || !assembly.GlobalAssemblyCache)
			{
				LoadEmeddedResources(assembly);
			}
		}
		AppDomain.CurrentDomain.AssemblyLoad += CurrentDomainOnAssemblyLoad;
	}

	private void CurrentDomainOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
	{
		LoadEmeddedResources(args.LoadedAssembly);
	}

	private void LoadEmeddedResources(Assembly assembly)
	{
		if (assembly is AssemblyBuilder || assembly.IsDynamic)
		{
			return;
		}
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		foreach (string text in manifestResourceNames)
		{
			if (IsEmbeddedResource(text, m_resourceDirectory1, m_resourceDirectory2))
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(assembly.GetManifestResourceStream(text));
				AddLocalizedStrings(xmlDocument);
			}
		}
	}

	private bool IsEmbeddedResource(string resourceName, string namespace1, string namespace2)
	{
		return resourceName.EndsWith(".Localization.xml") && (resourceName.Contains(namespace1) || resourceName.Contains(namespace2));
	}
}
