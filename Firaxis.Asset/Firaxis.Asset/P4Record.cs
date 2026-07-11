using System.Collections.Generic;

namespace Firaxis.Asset;

public class P4Record
{
	private Dictionary<string, string> m_Fields;

	private Dictionary<string, string[]> m_ArrayFields;

	public Dictionary<string, string> Fields => m_Fields;

	public Dictionary<string, string[]> ArrayFields => m_ArrayFields;

	public string this[string key]
	{
		get
		{
			return string.Empty;
		}
		set
		{
		}
	}

	internal P4Record()
	{
		m_Fields = new Dictionary<string, string>();
		m_ArrayFields = new Dictionary<string, string[]>();
	}
}
