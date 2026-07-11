using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class Wire : DomNodeAdapter, IGraphEdge<Element, ICircuitPin>, IGraphEdge<Element>
{
	private PinTarget m_inputPinTarget;

	private PinTarget m_outputPinTarget;

	protected abstract AttributeInfo LabelAttribute { get; }

	protected abstract AttributeInfo InputElementAttribute { get; }

	protected abstract AttributeInfo OutputElementAttribute { get; }

	protected abstract AttributeInfo InputPinAttribute { get; }

	protected abstract AttributeInfo OutputPinAttribute { get; }

	public virtual Element OutputElement
	{
		get
		{
			return GetReference<Element>(OutputElementAttribute);
		}
		set
		{
			SetReference(OutputElementAttribute, value);
		}
	}

	public virtual ICircuitPin OutputPin
	{
		get
		{
			int attribute = GetAttribute<int>(OutputPinAttribute);
			return OutputElement.OutputPin(attribute);
		}
		set
		{
			base.DomNode.SetAttribute(OutputPinAttribute, value.Index);
		}
	}

	public virtual Element InputElement
	{
		get
		{
			return GetReference<Element>(InputElementAttribute);
		}
		set
		{
			SetReference(InputElementAttribute, value);
		}
	}

	public virtual ICircuitPin InputPin
	{
		get
		{
			int attribute = GetAttribute<int>(InputPinAttribute);
			return InputElement.InputPin(attribute);
		}
		set
		{
			base.DomNode.SetAttribute(InputPinAttribute, value.Index);
		}
	}

	public virtual string Label
	{
		get
		{
			return GetAttribute<string>(LabelAttribute);
		}
		set
		{
			SetAttribute(LabelAttribute, value);
		}
	}

	Element IGraphEdge<Element>.FromNode => OutputElement;

	ICircuitPin IGraphEdge<Element, ICircuitPin>.FromRoute => OutputPin;

	Element IGraphEdge<Element>.ToNode => InputElement;

	ICircuitPin IGraphEdge<Element, ICircuitPin>.ToRoute => InputPin;

	string IGraphEdge<Element>.Label => Label;

	public PinTarget InputPinTarget
	{
		get
		{
			if (m_inputPinTarget == null)
			{
				SetPinTarget();
			}
			return m_inputPinTarget;
		}
		set
		{
			m_inputPinTarget = value;
		}
	}

	public PinTarget OutputPinTarget
	{
		get
		{
			if (m_outputPinTarget == null)
			{
				SetPinTarget();
			}
			return m_outputPinTarget;
		}
		set
		{
			m_outputPinTarget = value;
		}
	}

	public IEnumerable<GroupPin> InputPinSinkChain
	{
		get
		{
			if (InputPin.Is<GroupPin>())
			{
				return InputPin.Cast<GroupPin>().SinkChain(inputSide: true);
			}
			return EmptyEnumerable<GroupPin>.Instance;
		}
	}

	public IEnumerable<GroupPin> OutputPinSinkChain
	{
		get
		{
			if (OutputPin.Is<GroupPin>())
			{
				return OutputPin.Cast<GroupPin>().SinkChain(inputSide: false);
			}
			return EmptyEnumerable<GroupPin>.Instance;
		}
	}

	public virtual void SetOutput(Element outputElement, ICircuitPin outputPin)
	{
		OutputElement = outputElement;
		OutputPin = outputPin;
	}

	public virtual void SetInput(Element inputElement, ICircuitPin inputPin)
	{
		InputElement = inputElement;
		InputPin = inputPin;
	}

	public void SetPinTarget()
	{
		if (InputPin != null)
		{
			bool flag = InputElement.DomNode.Is<IReference<DomNode>>();
			if (InputPin.Is<GroupPin>())
			{
				if (flag)
				{
					PinTarget pinTarget = InputPin.Cast<GroupPin>().PinTarget;
					InputPinTarget = new PinTarget(pinTarget.LeafDomNode, pinTarget.LeafPinIndex, InputElement.DomNode);
				}
				else
				{
					InputPinTarget = InputPin.Cast<GroupPin>().PinTarget;
				}
			}
			else if (flag)
			{
				IReference<DomNode> reference = InputElement.As<IReference<DomNode>>();
				InputPinTarget = new PinTarget(reference.Target, InputPin.Index, InputElement.DomNode);
			}
			else
			{
				InputPinTarget = new PinTarget(InputElement.DomNode, InputPin.Index, null);
			}
		}
		if (OutputPin == null)
		{
			return;
		}
		bool flag2 = OutputElement.DomNode.Is<IReference<DomNode>>();
		if (OutputPin.Is<GroupPin>())
		{
			if (flag2)
			{
				PinTarget pinTarget2 = OutputPin.Cast<GroupPin>().PinTarget;
				OutputPinTarget = new PinTarget(pinTarget2.LeafDomNode, pinTarget2.LeafPinIndex, OutputElement.DomNode);
			}
			else
			{
				OutputPinTarget = OutputPin.Cast<GroupPin>().PinTarget;
			}
		}
		else if (flag2)
		{
			IReference<DomNode> reference2 = OutputElement.Cast<IReference<DomNode>>();
			OutputPinTarget = new PinTarget(reference2.Target, OutputPin.Index, OutputElement.DomNode);
		}
		else
		{
			OutputPinTarget = new PinTarget(OutputElement.DomNode, OutputPin.Index, null);
		}
	}

	internal bool IsValid(out int inputPinIndex, out int outputPinIndex)
	{
		outputPinIndex = OutputPin.Index;
		inputPinIndex = InputPin.Index;
		return outputPinIndex < OutputElement.Type.Outputs.Count && inputPinIndex < InputElement.Type.Inputs.Count;
	}
}
