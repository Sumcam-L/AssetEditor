using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class ElementReference : Element, IReference<Element>, IReference<DomNode>
{
	protected abstract AttributeInfo GuidRefAttribute { get; }

	public virtual Template Template
	{
		get
		{
			return GetReference<Template>(GuidRefAttribute);
		}
		set
		{
			SetReference(GuidRefAttribute, value.DomNode);
		}
	}

	DomNode IReference<DomNode>.Target
	{
		get
		{
			return Template.Target;
		}
		set
		{
			throw new InvalidOperationException("The group template determines the target");
		}
	}

	Element IReference<Element>.Target
	{
		get
		{
			return Template.Target.As<Element>();
		}
		set
		{
			throw new InvalidOperationException("The group template determines the target");
		}
	}

	public Element Element => Template.Target.As<Element>();

	public override ICircuitElementType Type => Element.Type;

	public IList<ICircuitPin> Inputs => Element.Type.Inputs;

	public IList<ICircuitPin> Outputs => Element.Type.Outputs;

	bool IReference<DomNode>.CanReference(DomNode item)
	{
		return item.Is<Element>();
	}

	bool IReference<Element>.CanReference(Element item)
	{
		return true;
	}

	public override ICircuitPin InputPin(int pinIndex)
	{
		return Element.Type.Inputs[pinIndex];
	}

	public override ICircuitPin OutputPin(int pinIndex)
	{
		return Element.Type.Outputs[pinIndex];
	}

	public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		if (pinTarget.InstancingNode != base.DomNode)
		{
			return default(Pair<Element, ICircuitPin>);
		}
		Pair<Element, ICircuitPin> result = Element.MatchPinTarget(pinTarget, inputSide);
		if (result.First != null)
		{
			result.First = this;
		}
		return result;
	}

	public override Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		return MatchPinTarget(pinTarget, inputSide);
	}
}
