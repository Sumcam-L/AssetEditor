using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

namespace Sce.Atf.Applications;

[Export(typeof(ContentTypeService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ContentTypeService
{
	private readonly Dictionary<string, string> m_extensionMap;

	public ContentTypeService()
	{
		m_extensionMap = new Dictionary<string, string>();
	}

	public void SetContentType(string extension, string type)
	{
		if (!m_extensionMap.ContainsKey(extension))
		{
			m_extensionMap.Add(extension, type);
		}
	}

	public string GetContentType(string extension)
	{
		extension = extension.TrimStart('.');
		m_extensionMap.TryGetValue(extension, out var value);
		return value;
	}

	public string GetContentType(Uri uri)
	{
		string extension = Path.GetExtension(uri.LocalPath);
		return GetContentType(extension);
	}

	public IEnumerable<string> GetAllExtensions()
	{
		return m_extensionMap.Keys;
	}

	public ICollection<string> GetAllExtensions(string type)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, string> item in m_extensionMap)
		{
			if (item.Value == type)
			{
				list.Add(item.Key);
			}
		}
		return list;
	}
}
