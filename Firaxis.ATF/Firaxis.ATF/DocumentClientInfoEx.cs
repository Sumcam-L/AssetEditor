using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class DocumentClientInfoEx : DocumentClientInfo
{
	private bool m_isHidden;

	private string m_category = string.Empty;

	public bool IsHidden
	{
		get
		{
			return m_isHidden;
		}
		set
		{
			m_isHidden = value;
		}
	}

	public string Category
	{
		get
		{
			return m_category;
		}
		set
		{
			m_category = value;
		}
	}

	public DocumentClientInfoEx(string fileType, string extension, string newIconName, string openIconName)
		: this(fileType, extension, newIconName, openIconName, multiDocument: true)
	{
	}

	public DocumentClientInfoEx(string fileType, string extension, string newIconName, string openIconName, bool multiDocument)
		: this(fileType, new string[1] { extension }, newIconName, openIconName, multiDocument, isHidden: false)
	{
	}

	public DocumentClientInfoEx(string fileType, string[] extensions, string newIconName, string openIconName, bool multiDocument)
		: this(fileType, extensions, newIconName, openIconName, multiDocument, isHidden: false)
	{
	}

	public DocumentClientInfoEx(string fileType, string[] extensions, string newIconName, string openIconName, bool multiDocument, bool isHidden)
		: this(fileType, extensions, newIconName, openIconName, multiDocument, isHidden, "Entities")
	{
	}

	public DocumentClientInfoEx(string fileType, string extension, string newIconName, string openIconName, bool multiDocument, bool isHidden, string category)
		: this(fileType, new string[1] { extension }, newIconName, openIconName, multiDocument, isHidden, category)
	{
		m_isHidden = isHidden;
		m_category = category;
	}

	public DocumentClientInfoEx(string fileType, string[] extensions, string newIconName, string openIconName, bool multiDocument, bool isHidden, string category)
		: base(fileType, extensions, newIconName, openIconName, multiDocument)
	{
		m_isHidden = isHidden;
		m_category = category;
	}
}
