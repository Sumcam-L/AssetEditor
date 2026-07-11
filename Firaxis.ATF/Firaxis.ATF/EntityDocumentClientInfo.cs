using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public class EntityDocumentClientInfo : DocumentClientInfoEx
{
	public readonly string NewEntityName;

	public readonly string DocumentIconName;

	private InstanceType[] m_extensionTypes;

	public InstanceType[] ExtensionTypes
	{
		get
		{
			return m_extensionTypes;
		}
		set
		{
			m_extensionTypes = value;
		}
	}

	public EntityDocumentClientInfo(string fileType, string extension, InstanceType extType, string newIconName, string openIconName, string newEntityName, string docIconName)
		: this(fileType, new string[1] { extension }, new InstanceType[1] { extType }, newIconName, openIconName, newEntityName, multiDocument: true, docIconName)
	{
	}

	public EntityDocumentClientInfo(string fileType, string extension, InstanceType extType, string newIconName, string openIconName, string newEntityName, bool multiDocument, string docIconName)
		: this(fileType, new string[1] { extension }, new InstanceType[1] { extType }, newIconName, openIconName, newEntityName, multiDocument, docIconName)
	{
	}

	public EntityDocumentClientInfo(string fileType, string[] extensions, InstanceType[] extTypes, string newIconName, string openIconName, string newEntityName, bool multiDocument, string docIconName)
		: this(fileType, extensions, extTypes, newIconName, openIconName, newEntityName, multiDocument, isHidden: false, docIconName)
	{
	}

	public EntityDocumentClientInfo(string fileType, string[] extensions, InstanceType[] extTypes, string newIconName, string openIconName, string newEntityName, bool multiDocument, bool isHidden, string docIconName)
		: this(fileType, extensions, extTypes, newIconName, openIconName, newEntityName, multiDocument, isHidden, "Entities", docIconName)
	{
	}

	public EntityDocumentClientInfo(string fileType, string[] extensions, InstanceType[] extTypes, string newIconName, string openIconName, string newEntityName, bool multiDocument, bool isHidden, string category, string docIconName)
		: base(fileType, extensions, newIconName, openIconName, multiDocument, isHidden, category)
	{
		m_extensionTypes = extTypes;
		NewEntityName = newEntityName;
		DocumentIconName = docIconName;
	}
}
