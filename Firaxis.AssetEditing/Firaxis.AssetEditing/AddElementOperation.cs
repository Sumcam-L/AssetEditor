using System.Collections.Generic;

namespace Firaxis.AssetEditing;

public class AddElementOperation : ArtDefCollectionOperation
{
	private readonly string m_elementName;

	public AddElementOperation(ArtDefDocument doc, IList<string> pathToParent, string elementName)
		: base(doc, pathToParent)
	{
		m_elementName = elementName;
		Do();
	}

	public override void Do()
	{
		GetCollection().AddElement(m_elementName, -1);
	}

	public override void Undo()
	{
		GetCollection().RemoveElement(m_elementName);
	}
}
