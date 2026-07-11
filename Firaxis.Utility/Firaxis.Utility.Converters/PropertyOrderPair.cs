using System;

namespace Firaxis.Utility.Converters;

public class PropertyOrderPair : IComparable
{
	private int m_order;

	private string m_name;

	public string Name => m_name;

	public PropertyOrderPair(string name, int order)
	{
		m_order = order;
		m_name = name;
	}

	public int CompareTo(object obj)
	{
		int order = ((PropertyOrderPair)obj).m_order;
		if (order == m_order)
		{
			string name = ((PropertyOrderPair)obj).m_name;
			return string.Compare(m_name, name);
		}
		if (order > m_order)
		{
			return -1;
		}
		return 1;
	}
}
