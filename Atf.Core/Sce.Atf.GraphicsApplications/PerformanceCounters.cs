using System.Collections.Generic;
using System.Text;

namespace Sce.Atf.GraphicsApplications;

public class PerformanceCounters
{
	private readonly SortedDictionary<string, int> m_counts = new SortedDictionary<string, int>();

	public int this[string id]
	{
		get
		{
			m_counts.TryGetValue(id, out var value);
			return value;
		}
		set
		{
			m_counts.TryGetValue(id, out var _);
			m_counts[id] = value;
		}
	}

	public void Clear()
	{
		m_counts.Clear();
	}

	public string GetDisplayString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(m_counts);
		foreach (KeyValuePair<string, int> item in list)
		{
			stringBuilder.Append(item.Key);
			stringBuilder.Append(": ");
			stringBuilder.Append(item.Value.ToString());
			stringBuilder.Append(" ");
		}
		return stringBuilder.ToString();
	}
}
