using System;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

[Obsolete("Circuit groups and circuit templates have replaced mastered circuits")]
public abstract class SubCircuitInstance : Element
{
	protected abstract AttributeInfo TypeAttribute { get; }

	public SubCircuit SubCircuit
	{
		get
		{
			return GetReference<SubCircuit>(TypeAttribute);
		}
		set
		{
			SetReference(TypeAttribute, value);
		}
	}

	public override ICircuitElementType Type => SubCircuit;
}
