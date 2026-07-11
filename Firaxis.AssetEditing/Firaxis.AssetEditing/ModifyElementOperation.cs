using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class ModifyElementOperation : ArtDefOperationBase
{
	private readonly ArtDefDocument m_document;

	private readonly string m_newName;

	private readonly string m_oldName;

	public ModifyElementOperation(ArtDefDocument doc, IList<string> pathToParent, string oldName, string newName)
		: base(pathToParent)
	{
		m_document = doc;
		m_oldName = oldName;
		m_newName = newName;
	}

	public override void Do()
	{
		ArtDefElementAdapter element = GetElement(m_oldName);
		if (element != null)
		{
			element.Name = m_newName;
		}
		else
		{
			BugSubmitter.SilentReport("Unable to find an element with the name " + m_oldName + " to perform the Redo operation! @assign bwhitman");
		}
	}

	public override void Undo()
	{
		ArtDefElementAdapter element = GetElement(m_newName);
		if (element != null)
		{
			element.Name = m_oldName;
		}
		else
		{
			BugSubmitter.SilentReport("Unable to find an element with the name " + m_newName + " to perform the Undo operation! @assign bwhitman");
		}
	}

	private ArtDefElementAdapter GetElement(string elemName)
	{
		ArtDefCollectionAdapter artDefCollectionAdapter = m_document.As<ArtDefSetAdapter>().RootCollections.FirstOrDefault((ArtDefCollectionAdapter col) => col.Name == base.PathToParent[0]);
		ArtDefElementAdapter artDefElementAdapter = null;
		for (int num = 1; num < base.PathToParent.Count - 1; num++)
		{
			if (num % 2 == 0)
			{
				artDefCollectionAdapter = artDefElementAdapter.FindCollection(base.PathToParent[num]);
			}
			else
			{
				artDefElementAdapter = artDefCollectionAdapter.FindElement(base.PathToParent[num]);
			}
		}
		return artDefCollectionAdapter.FindElement(elemName);
	}
}
