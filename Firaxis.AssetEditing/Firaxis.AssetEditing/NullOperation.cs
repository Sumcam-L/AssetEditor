using System;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class NullOperation : TransactionContext.Operation
{
	public NullOperation(Action action)
	{
		action?.Invoke();
	}

	public override void Do()
	{
	}

	public override void Undo()
	{
	}
}
