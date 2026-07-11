using System;
using System.Reflection;
using System.Xml;

namespace Sce.Atf;

public class ResourceStreamResolver : XmlUrlResolver
{
	private readonly Assembly m_assembly;

	private readonly Uri m_rootPath;

	public ResourceStreamResolver(Assembly assembly, string resourceNamespace)
	{
		m_assembly = assembly;
		m_rootPath = new Uri(Uri.UriSchemeFile + ":///" + resourceNamespace + "/");
	}

	public override object GetEntity(Uri absoluteUri, string role, Type returnType)
	{
		if (absoluteUri.IsFile)
		{
			string text = absoluteUri.AbsolutePath.Replace('/', '.');
			text = text.Substring(1, text.Length - 1);
			return m_assembly.GetManifestResourceStream(text);
		}
		return base.GetEntity(absoluteUri, role, returnType);
	}

	public override Uri ResolveUri(Uri baseUri, string relativeUri)
	{
		return new Uri(m_rootPath, relativeUri);
	}
}
