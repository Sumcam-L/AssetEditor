using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class Circuit : DomNodeAdapter, IGraph<Element, Wire, ICircuitPin>, IAnnotatedDiagram, ICircuitContainer
{
	private static IList<Annotation> s_emptyAnnotations = new List<Annotation>().AsReadOnly();

	private DomNodeListAdapter<Element> m_elements;

	private DomNodeListAdapter<Wire> m_wires;

	private DomNodeListAdapter<Annotation> m_annotations;

	private bool m_dirty;

	protected abstract ChildInfo ElementChildInfo { get; }

	protected abstract ChildInfo WireChildInfo { get; }

	protected virtual ChildInfo AnnotationChildInfo => null;

	public IList<Element> Elements => m_elements;

	public IList<Wire> Wires => m_wires;

	public IList<Annotation> Annotations
	{
		get
		{
			IList<Annotation> annotations = m_annotations;
			return annotations ?? s_emptyAnnotations;
		}
	}

	public bool Expanded
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public bool Dirty
	{
		get
		{
			return m_dirty;
		}
		set
		{
			m_dirty = value;
		}
	}

	IEnumerable<Element> IGraph<Element, Wire, ICircuitPin>.Nodes => m_elements.Where((Element x) => x.Visible);

	IEnumerable<Wire> IGraph<Element, Wire, ICircuitPin>.Edges => m_wires.Where((Wire x) => x.InputElement.Visible && x.OutputElement.Visible);

	IEnumerable<IAnnotation> IAnnotatedDiagram.Annotations => Annotations.AsIEnumerable<IAnnotation>();

	protected override void OnNodeSet()
	{
		m_elements = new DomNodeListAdapter<Element>(base.DomNode, ElementChildInfo);
		m_wires = new DomNodeListAdapter<Wire>(base.DomNode, WireChildInfo);
		if (AnnotationChildInfo != null)
		{
			m_annotations = new DomNodeListAdapter<Annotation>(base.DomNode, AnnotationChildInfo);
		}
		foreach (Wire wire in Wires)
		{
			wire.SetPinTarget();
		}
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.OnNodeSet();
	}

	public void Update()
	{
		Dirty = false;
	}

	public Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		Pair<Element, ICircuitPin> result = default(Pair<Element, ICircuitPin>);
		foreach (Element element in Elements)
		{
			result = element.MatchPinTarget(pinTarget, inputSide);
			if (result.First != null)
			{
				break;
			}
		}
		return result;
	}

	public Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		Pair<Element, ICircuitPin> result = default(Pair<Element, ICircuitPin>);
		foreach (Element element in Elements)
		{
			result = element.FullyMatchPinTarget(pinTarget, inputSide);
			if (result.First != null)
			{
				break;
			}
		}
		return result;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		Dirty = true;
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		Dirty = true;
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		Dirty = true;
	}
}
