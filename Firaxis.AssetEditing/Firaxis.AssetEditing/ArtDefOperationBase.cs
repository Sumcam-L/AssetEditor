using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public abstract class ArtDefOperationBase : TransactionContext.Operation
{
	private protected IList<string> PathToParent { get; set; }

	public ArtDefOperationBase(IList<string> pathToParent)
	{
		PathToParent = pathToParent;
	}
}
