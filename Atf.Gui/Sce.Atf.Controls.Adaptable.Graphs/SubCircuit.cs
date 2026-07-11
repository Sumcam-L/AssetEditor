using System;
using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

[Obsolete("Circuit groups and circuit templates have replaced mastered circuits")]
public abstract class SubCircuit : Circuit, ICircuitElementType
{
	private DomNodeListAdapter<ICircuitPin> m_inputPins;

	private DomNodeListAdapter<ICircuitPin> m_outputPins;

	protected abstract AttributeInfo NameAttribute { get; }

	protected abstract ChildInfo InputChildInfo { get; }

	protected abstract ChildInfo OutputChildInfo { get; }

	public string Name
	{
		get
		{
			return GetAttribute<string>(NameAttribute);
		}
		set
		{
			SetAttribute(NameAttribute, value);
		}
	}

	public Size InteriorSize => Size.Empty;

	public Image Image => null;

	public IList<ICircuitPin> Inputs => m_inputPins;

	public IList<ICircuitPin> Outputs => m_outputPins;

	protected override void OnNodeSet()
	{
		m_inputPins = new DomNodeListAdapter<ICircuitPin>(base.DomNode, InputChildInfo);
		m_outputPins = new DomNodeListAdapter<ICircuitPin>(base.DomNode, OutputChildInfo);
		base.OnNodeSet();
	}
}
