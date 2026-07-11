using System.Collections.Generic;
using System.Text;

namespace Sce.Atf;

public class FileFilterBuilder
{
	private readonly StringBuilder m_sb = new StringBuilder();

	private readonly List<string> m_extensions = new List<string>();

	private int m_count;

	public int Count => m_count;

	public void AddFileType(string fileType, params string[] extensions)
	{
		AddFileType(fileType, (IEnumerable<string>)extensions);
	}

	public void AddFileType(string fileType, IEnumerable<string> extensions)
	{
		if (m_sb.Length > 0)
		{
			m_sb.Append('|');
		}
		m_sb.AppendFormat("{0} files".Localize("For example, 'Circuit files' or 'All files'"), fileType);
		m_sb.Append(" (");
		foreach (string extension in extensions)
		{
			m_extensions.Add(extension);
			AppendExtension(extension);
			m_sb.Append(";");
		}
		m_sb.Length--;
		m_sb.Append(")|");
		foreach (string extension2 in extensions)
		{
			AppendExtension(extension2);
			m_sb.Append(";");
		}
		m_sb.Length--;
		m_count++;
	}

	public void AddAllFiles()
	{
		if (m_count > 0)
		{
			m_sb.Append("|");
		}
		m_sb.Append("All files".Localize() + " (*.*)|*.*|");
	}

	public void AddAllFilesWithExtensions()
	{
		string[] extensions = m_extensions.ToArray();
		m_extensions.Clear();
		AddFileType("All".Localize("As in 'All files'"), extensions);
	}

	public override string ToString()
	{
		return m_sb.ToString();
	}

	public static string GetFilterString(string fileType, params string[] extensions)
	{
		return GetFilterString(fileType, (IEnumerable<string>)extensions);
	}

	public static string GetFilterString(string fileType, IEnumerable<string> extensions)
	{
		FileFilterBuilder fileFilterBuilder = new FileFilterBuilder();
		fileFilterBuilder.AddFileType(fileType, extensions);
		return fileFilterBuilder.ToString();
	}

	private void AppendExtension(string extension)
	{
		m_sb.Append("*");
		m_sb.Append(extension);
	}
}
