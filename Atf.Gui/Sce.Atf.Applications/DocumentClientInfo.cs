using System;
using System.IO;

namespace Sce.Atf.Applications;

public class DocumentClientInfo
{
	private string m_newIconName;

	private string m_openIconName;

	private string m_newDocumentName = "Untitled".Localize();

	private bool m_allowStandardFileCommands = true;

	public string FileType { get; set; }

	public string[] Extensions { get; set; }

	public string DefaultExtension { get; set; }

	public string NewIconName
	{
		get
		{
			return (!string.IsNullOrEmpty(m_newIconName)) ? m_newIconName : (NewIconKey as string);
		}
		set
		{
			m_newIconName = value;
		}
	}

	public object NewIconKey { get; set; }

	public string OpenIconName
	{
		get
		{
			return (!string.IsNullOrEmpty(m_openIconName)) ? m_openIconName : (OpenIconKey as string);
		}
		set
		{
			m_openIconName = value;
		}
	}

	public object OpenIconKey { get; set; }

	public string NewDocumentName
	{
		get
		{
			return m_newDocumentName;
		}
		set
		{
			m_newDocumentName = value;
		}
	}

	public bool MultiDocument { get; set; }

	public string InitialDirectory { get; set; }

	public bool AllowStandardFileCommands
	{
		get
		{
			return m_allowStandardFileCommands;
		}
		set
		{
			m_allowStandardFileCommands = value;
		}
	}

	public DocumentClientInfo()
	{
	}

	public DocumentClientInfo(string fileType, string extension, string newIconName, string openIconName)
		: this(fileType, new string[1] { extension }, newIconName, openIconName, multiDocument: true)
	{
	}

	public DocumentClientInfo(string fileType, string extension, string newIconName, string openIconName, bool multiDocument)
		: this(fileType, new string[1] { extension }, newIconName, openIconName, multiDocument)
	{
	}

	public DocumentClientInfo(string fileType, string[] extensions, string newIconName, string openIconName, bool multiDocument)
	{
		FileType = fileType;
		Extensions = extensions;
		m_newIconName = newIconName;
		m_openIconName = openIconName;
		MultiDocument = multiDocument;
	}

	public DocumentClientInfo(string fileType, string extension, object newIconKey, object openIconKey)
		: this(fileType, new string[1] { extension }, newIconKey, openIconKey, multiDocument: true)
	{
	}

	public DocumentClientInfo(string fileType, string extension, object newIconKey, object openIconKey, bool multiDocument)
		: this(fileType, new string[1] { extension }, newIconKey, openIconKey, multiDocument)
	{
	}

	public DocumentClientInfo(string fileType, string[] extensions, object newIconKey, object openIconKey, bool multiDocument)
	{
		FileType = fileType;
		Extensions = extensions;
		NewIconKey = newIconKey;
		OpenIconKey = openIconKey;
		MultiDocument = multiDocument;
	}

	public bool IsCompatiblePath(string filePath)
	{
		string extension = Path.GetExtension(filePath);
		string[] extensions = Extensions;
		foreach (string text in extensions)
		{
			if (string.Compare(text, extension, ignoreCase: true) == 0 || filePath.EndsWith(text))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCompatibleUri(Uri uri)
	{
		string filePath = Uri.UnescapeDataString(uri.ToString());
		return IsCompatiblePath(filePath);
	}

	public string GetFilterString()
	{
		return FileFilterBuilder.GetFilterString(FileType, Extensions);
	}
}
