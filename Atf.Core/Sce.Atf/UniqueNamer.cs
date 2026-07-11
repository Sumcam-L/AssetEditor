using System;
using System.Collections.Generic;

namespace Sce.Atf;

public class UniqueNamer
{
	private const int MaxMinNumDigits = 10;

	private const string Zeros = "000000000";

	private readonly HashSet<string> m_names = new HashSet<string>();

	private readonly char m_separator;

	private readonly int m_minNumDigits;

	public UniqueNamer()
		: this('_', 1)
	{
	}

	public UniqueNamer(char suffixSeparator)
		: this(suffixSeparator, 1)
	{
	}

	public UniqueNamer(char suffixSeparator, int minNumDigits)
	{
		if (suffixSeparator != ' ' && suffixSeparator != '-' && suffixSeparator != '_' && suffixSeparator != '/' && suffixSeparator != '\\' && suffixSeparator != '(')
		{
			throw new ArgumentException("Invalid suffix separator");
		}
		m_separator = suffixSeparator;
		if (minNumDigits > 10)
		{
			throw new NotSupportedException("Maximum 10 digits supported");
		}
		m_minNumDigits = minNumDigits;
	}

	public bool IsTaken(string desired)
	{
		return m_names.Contains(desired);
	}

	public string Name(string desired)
	{
		string text = desired;
		if (m_names.Contains(desired))
		{
			Parse(desired, out var root, out var _);
			int num = 1;
			while (true)
			{
				text = root + m_separator;
				string text2 = num.ToString();
				if (m_minNumDigits > 1)
				{
					int length = text2.Length;
					if (length < m_minNumDigits)
					{
						text += "000000000".Substring(0, m_minNumDigits - length);
					}
				}
				text += text2;
				if (m_separator == '(')
				{
					text += ")";
				}
				if (!m_names.Contains(text))
				{
					break;
				}
				num++;
			}
		}
		m_names.Add(text);
		return text;
	}

	public void Retire(string name)
	{
		if (m_names.Count != 0)
		{
			m_names.Remove(name);
		}
	}

	public string Change(string oldName, string newName)
	{
		Retire(oldName);
		return Name(newName);
	}

	public void Clear()
	{
		m_names.Clear();
	}

	public void Parse(string name, out string root, out int suffixNumber)
	{
		root = name;
		suffixNumber = 0;
		int num = name.LastIndexOf(m_separator);
		if (num >= 0)
		{
			int num2 = num + 1;
			int num3 = name.Length - num2;
			if (m_separator == '(')
			{
				num3--;
			}
			string s = name.Substring(num2, num3);
			if (int.TryParse(s, out suffixNumber))
			{
				root = name.Substring(0, num);
			}
		}
	}
}
