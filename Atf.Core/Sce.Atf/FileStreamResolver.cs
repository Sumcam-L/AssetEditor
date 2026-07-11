using System;
using System.Xml;

namespace Sce.Atf;

public class FileStreamResolver : XmlUrlResolver
{
	private readonly Uri m_rootPath;

	public FileStreamResolver(string rootPath)
	{
		m_rootPath = new Uri(Uri.UriSchemeFile + ":///" + rootPath + "/");
	}

	public override Uri ResolveUri(Uri baseUri, string relativeUri)
	{
		Uri uri = new Uri(m_rootPath, relativeUri);
		if (uri.IsFile)
		{
			string culturePath = PathUtil.GetCulturePath(uri.LocalPath);
			uri = new Uri(culturePath);
		}
		return uri;
	}
}
