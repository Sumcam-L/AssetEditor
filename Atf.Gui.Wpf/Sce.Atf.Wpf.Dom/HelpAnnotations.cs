using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Wpf.Dom;

public static class HelpAnnotations
{
	private class HelpContext : IHelpContext
	{
		private string[] m_keys;

		public HelpContext(string[] keys)
		{
			m_keys = keys;
		}

		public string[] GetHelpKeys()
		{
			return m_keys;
		}
	}

	public static void ParseAnnotations(XmlSchemaSet schemaSet, IDictionary<NamedMetadata, IList<XmlNode>> annotations)
	{
		foreach (KeyValuePair<NamedMetadata, IList<XmlNode>> annotation in annotations)
		{
			List<string> list = new List<string>();
			foreach (XmlNode item in annotation.Value)
			{
				if (item.Name == "atf.wpf.help.key")
				{
					list.Add(item.Attributes["key"].Value);
				}
			}
			if (list.Count > 0)
			{
				annotation.Key.SetTag((IHelpContext)new HelpContext(list.ToArray()));
			}
		}
	}
}
