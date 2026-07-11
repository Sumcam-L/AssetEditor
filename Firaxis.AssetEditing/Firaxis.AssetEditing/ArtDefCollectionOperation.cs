using System.Collections.Generic;
using System.Linq;
using Firaxis.Error;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public abstract class ArtDefCollectionOperation : ArtDefOperationBase
{
	private ArtDefDocument Document { get; set; }

	public ArtDefCollectionOperation(ArtDefDocument doc, IList<string> pathToParent)
		: base(pathToParent)
	{
		PlatformAssert.If(pathToParent.Count == 0);
		PlatformAssert.If(pathToParent.Count % 2 == 0);
		Document = doc;
	}

	protected ArtDefCollectionAdapter GetCollection()
	{
		ArtDefCollectionAdapter artDefCollectionAdapter = Document.As<ArtDefSetAdapter>().RootCollections.First((ArtDefCollectionAdapter col) => col.Name == base.PathToParent[0]);
		ArtDefElementAdapter artDefElementAdapter = null;
		int idx = 1;
		while (idx < base.PathToParent.Count)
		{
			if (idx % 2 == 0)
			{
				artDefCollectionAdapter = artDefElementAdapter.Collections.First((ArtDefCollectionAdapter col) => col.Name == base.PathToParent[idx]);
			}
			else
			{
				artDefElementAdapter = artDefCollectionAdapter.Elements.First((ArtDefElementAdapter elem) => elem.Name == base.PathToParent[idx]);
			}
			int num = idx + 1;
			idx = num;
		}
		PlatformAssert.If(artDefCollectionAdapter.Name != base.PathToParent[base.PathToParent.Count - 1]);
		return artDefCollectionAdapter;
	}
}
