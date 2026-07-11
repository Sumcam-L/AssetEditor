using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class Element : DomNodeAdapter, ICircuitElement, IGraphNode, IVisible
{
	private ICircuitElementType m_elementType;

	private Size m_size;

	private CircuitElementInfo m_elementInfo;

	private bool m_syncingElementInfo;

	protected abstract AttributeInfo NameAttribute { get; }

	protected abstract AttributeInfo LabelAttribute { get; }

	protected abstract AttributeInfo XAttribute { get; }

	protected abstract AttributeInfo YAttribute { get; }

	protected abstract AttributeInfo VisibleAttribute { get; }

	protected virtual AttributeInfo SourceGuidAttribute => null;

	protected virtual AttributeInfo ShowUnconnectedPinsAttribute => null;

	public virtual string Id
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

	public virtual string Name
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

	public virtual Point Position
	{
		get
		{
			return new Point(GetAttribute<int>(XAttribute), GetAttribute<int>(YAttribute));
		}
		set
		{
			SetAttribute(XAttribute, value.X);
			SetAttribute(YAttribute, value.Y);
		}
	}

	public virtual ICircuitElementType Type
	{
		get
		{
			ICircuitElementType circuitElementType = null;
			if (base.DomNode.Is<ICircuitElement>())
			{
				ICircuitElement circuitElement = base.DomNode.Cast<ICircuitElement>();
				if (circuitElement != this)
				{
					circuitElementType = base.DomNode.Cast<ICircuitElement>().Type;
				}
			}
			if (circuitElementType == null)
			{
				if (m_elementType == null)
				{
					m_elementType = base.DomNode.Type.GetTag<ICircuitElementType>();
				}
				circuitElementType = m_elementType;
			}
			return circuitElementType;
		}
	}

	public CircuitElementInfo ElementInfo => m_elementInfo;

	public int Level => base.DomNode.Ancestry.Count();

	public virtual IEnumerable<ICircuitPin> AllInputPins => Type.Inputs;

	public virtual IEnumerable<ICircuitPin> AllOutputPins => Type.Outputs;

	public virtual bool Visible
	{
		get
		{
			return VisibleAttribute == null || GetAttribute<bool>(VisibleAttribute);
		}
		set
		{
			SetAttribute(VisibleAttribute, value);
		}
	}

	public virtual Rectangle Bounds
	{
		get
		{
			return new Rectangle(Position, m_size);
		}
		set
		{
			SetAttribute(XAttribute, value.X);
			SetAttribute(YAttribute, value.Y);
			m_size = value.Size;
		}
	}

	public Guid SourceGuid
	{
		get
		{
			if (SourceGuidAttribute == null)
			{
				return Guid.Empty;
			}
			string text = base.DomNode.GetAttribute(SourceGuidAttribute) as string;
			if (string.IsNullOrEmpty(text))
			{
				return Guid.Empty;
			}
			return new Guid(text);
		}
		set
		{
			if (SourceGuidAttribute != null)
			{
				base.DomNode.SetAttribute(SourceGuidAttribute, value.ToString());
			}
		}
	}

	public virtual bool HasInputPin(ICircuitPin pin)
	{
		if (this.Is<Group>())
		{
			return this.Cast<Group>().HasInputPin(pin);
		}
		return Type.Inputs.Contains(pin);
	}

	public virtual bool HasOutputPin(ICircuitPin pin)
	{
		if (this.Is<Group>())
		{
			return this.Cast<Group>().HasOutputPin(pin);
		}
		return Type.Outputs.Contains(pin);
	}

	public virtual ICircuitPin InputPin(int pinIndex)
	{
		return Type.Inputs[pinIndex];
	}

	public virtual ICircuitPin OutputPin(int pinIndex)
	{
		return Type.Outputs[pinIndex];
	}

	public virtual Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		Pair<Element, ICircuitPin> result = default(Pair<Element, ICircuitPin>);
		if (pinTarget != null && pinTarget.LeafDomNode == base.DomNode && (inputSide ? (pinTarget.LeafPinIndex < Type.Inputs.Count) : (pinTarget.LeafPinIndex < Type.Outputs.Count)))
		{
			result.First = this;
			result.Second = (inputSide ? Type.Inputs[pinTarget.LeafPinIndex] : Type.Outputs[pinTarget.LeafPinIndex]);
		}
		return result;
	}

	public virtual Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		return MatchPinTarget(pinTarget, inputSide);
	}

	public virtual int PinDisplayOrder(int pinIndex, bool inputSide)
	{
		return pinIndex;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		m_elementInfo = CreateElementInfo();
		if (ShowUnconnectedPinsAttribute != null)
		{
			m_elementInfo.ShowUnconnectedPins = GetAttribute<bool>(ShowUnconnectedPinsAttribute);
		}
		m_elementInfo.PropertyChanged += delegate
		{
			if (!m_syncingElementInfo)
			{
				m_syncingElementInfo = true;
				try
				{
					if (ShowUnconnectedPinsAttribute != null)
					{
						SetAttribute(ShowUnconnectedPinsAttribute, m_elementInfo.ShowUnconnectedPins);
					}
				}
				finally
				{
					m_syncingElementInfo = false;
				}
			}
		};
		base.DomNode.AttributeChanged += delegate(object sender, AttributeEventArgs args)
		{
			if (!m_syncingElementInfo && args.DomNode == base.DomNode)
			{
				m_syncingElementInfo = true;
				try
				{
					if (args.AttributeInfo.Equivalent(ShowUnconnectedPinsAttribute))
					{
						m_elementInfo.ShowUnconnectedPins = (bool)args.NewValue;
					}
				}
				finally
				{
					m_syncingElementInfo = false;
				}
			}
		};
	}

	protected virtual CircuitElementInfo CreateElementInfo()
	{
		return new CircuitElementInfo();
	}
}
