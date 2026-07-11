using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class GroupPin : Pin, ICircuitGroupPin<Element>, ICircuitPin, IEdgeRoute, IVisible
{
	private PinTarget m_pinTarget;

	private Size m_size;

	private CircuitGroupPinInfo m_info;

	private bool m_infoBeingSynced;

	private bool m_isDefaultName;

	private bool m_inputSide;

	protected abstract AttributeInfo IndexAttribute { get; }

	protected abstract AttributeInfo PinYAttribute { get; }

	protected abstract AttributeInfo ElementAttribute { get; }

	protected abstract AttributeInfo PinAttribute { get; }

	protected abstract AttributeInfo PinnedAttribute { get; }

	protected abstract AttributeInfo VisibleAttribute { get; }

	public override int Index
	{
		get
		{
			return GetAttribute<int>(IndexAttribute);
		}
		set
		{
			SetAttribute(IndexAttribute, value);
		}
	}

	public override bool AllowFanIn
	{
		get
		{
			ICircuitPin circuitPin = (IsInputSide ? PinTarget.LeafDomNode.Cast<ICircuitElement>().Type.Inputs[PinTarget.LeafPinIndex] : PinTarget.LeafDomNode.Cast<ICircuitElement>().Type.Outputs[PinTarget.LeafPinIndex]);
			return circuitPin.AllowFanIn;
		}
	}

	public override bool AllowFanOut
	{
		get
		{
			ICircuitPin circuitPin = (IsInputSide ? PinTarget.LeafDomNode.Cast<ICircuitElement>().Type.Inputs[PinTarget.LeafPinIndex] : PinTarget.LeafDomNode.Cast<ICircuitElement>().Type.Outputs[PinTarget.LeafPinIndex]);
			return circuitPin.AllowFanOut;
		}
	}

	public virtual Point Position
	{
		get
		{
			return new Point(0, GetAttribute<int>(PinYAttribute));
		}
		set
		{
			SetAttribute(PinYAttribute, value.Y);
		}
	}

	public bool IsDefaultName
	{
		get
		{
			return m_isDefaultName;
		}
		set
		{
			m_isDefaultName = value;
		}
	}

	public bool Pinned
	{
		get
		{
			return GetAttribute<bool>(PinnedAttribute);
		}
		set
		{
			SetAttribute(PinnedAttribute, value);
		}
	}

	public bool Visible
	{
		get
		{
			return GetAttribute<bool>(VisibleAttribute);
		}
		set
		{
			SetAttribute(VisibleAttribute, value);
		}
	}

	public Element InternalElement
	{
		get
		{
			return GetReference<Element>(ElementAttribute);
		}
		set
		{
			SetReference(ElementAttribute, value);
		}
	}

	public int InternalPinIndex
	{
		get
		{
			return GetAttribute<int>(PinAttribute);
		}
		set
		{
			base.DomNode.SetAttribute(PinAttribute, value);
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
			Position = value.Location;
			m_size = value.Size;
		}
	}

	public CircuitGroupPinInfo Info
	{
		get
		{
			return m_info;
		}
		set
		{
			m_info = value;
		}
	}

	public PinTarget PinTarget
	{
		get
		{
			return m_pinTarget;
		}
		set
		{
			m_pinTarget = value;
		}
	}

	public bool IsInputSide => m_inputSide;

	public Point DesiredLocation { get; set; }

	public GroupPin()
	{
		m_info = new CircuitGroupPinInfo();
		m_info.Changed += InfoChanged;
		m_isDefaultName = true;
	}

	public virtual string DefaultName(bool inputSide)
	{
		string text = (inputSide ? InternalElement.InputPin(InternalPinIndex).Name : InternalElement.OutputPin(InternalPinIndex).Name);
		return InternalElement.Name + ":" + text;
	}

	public void SetPinTarget(bool inputSide)
	{
		m_inputSide = inputSide;
		DomNode instancingNode;
		DomNode leafDomNode = GetLeafDomNode(inputSide, out instancingNode);
		if (m_pinTarget != null && m_pinTarget.InstancingNode != null)
		{
			instancingNode = m_pinTarget.InstancingNode;
		}
		PinTarget = new PinTarget(leafDomNode, GetLeafPinIndex(inputSide), instancingNode);
	}

	public IEnumerable<GroupPin> SinkChain(bool inputSide)
	{
		yield return this;
		GroupPin current = this;
		while (current.InternalElement.Is<Group>())
		{
			Group childSubGraph = current.InternalElement.Cast<Group>();
			current = (inputSide ? childSubGraph.InputGroupPins.First((GroupPin x) => x.PinTarget == PinTarget) : childSubGraph.OutputGroupPins.First((GroupPin x) => x.PinTarget == PinTarget));
			yield return current;
		}
	}

	private IEnumerable<GroupPin> GetLineage(bool inputSide)
	{
		GroupPin grpPin = this;
		DomNode domNode = DomNode;
		while (grpPin != null)
		{
			yield return grpPin;
			GroupPin nextGrpPin = null;
			Group subGraph = domNode.Parent.Cast<Group>();
			if (subGraph.ParentGraph.Is<Group>())
			{
				Group parentSubGraph = subGraph.ParentGraph.Cast<Group>();
				if (parentSubGraph != null)
				{
					IEnumerable<GroupPin> parentPins = (inputSide ? parentSubGraph.InputGroupPins : parentSubGraph.OutputGroupPins);
					foreach (GroupPin parentPin in parentPins)
					{
						if (parentPin.InternalElement.DomNode == subGraph.DomNode && parentPin.InternalPinIndex == grpPin.Index)
						{
							nextGrpPin = parentPin;
							domNode = nextGrpPin.DomNode;
							break;
						}
					}
				}
			}
			grpPin = nextGrpPin;
		}
	}

	public IEnumerable<GroupPin> GetAncestry(bool inputSide)
	{
		bool firstTime = true;
		foreach (GroupPin grpPin in GetLineage(inputSide))
		{
			if (firstTime)
			{
				firstTime = false;
			}
			else
			{
				yield return grpPin;
			}
		}
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
		m_infoBeingSynced = true;
		try
		{
			m_info.Pinned = GetAttribute<bool>(PinnedAttribute);
			m_info.Visible = GetAttribute<bool>(VisibleAttribute);
		}
		finally
		{
			m_infoBeingSynced = false;
		}
		Group parentAs = GetParentAs<Group>();
		if (parentAs != null)
		{
			bool inputSide = parentAs.InputGroupPins.Contains(this);
			IsDefaultName = Name == DefaultName(inputSide);
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (sender != base.DomNode || m_infoBeingSynced)
		{
			return;
		}
		try
		{
			m_infoBeingSynced = true;
			if (e.AttributeInfo == PinnedAttribute)
			{
				m_info.Pinned = (bool)e.NewValue;
			}
			else if (e.AttributeInfo == VisibleAttribute)
			{
				m_info.Visible = (bool)e.NewValue;
			}
		}
		finally
		{
			m_infoBeingSynced = false;
		}
	}

	private void InfoChanged(object sender, EventArgs e)
	{
		if (!m_infoBeingSynced)
		{
			try
			{
				m_infoBeingSynced = true;
				SetAttribute(PinnedAttribute, m_info.Pinned);
				SetAttribute(VisibleAttribute, m_info.Visible);
			}
			finally
			{
				m_infoBeingSynced = false;
			}
		}
	}

    private DomNode GetLeafDomNode(bool inputSide, out DomNode instancingNode)
    {
        instancingNode = null;
        GroupPin current = this;

        while (current.InternalElement.Is<Group>())
        {
            // 记录第一个遇到的 IReference<DomNode> 的 DomNode
            if (instancingNode == null && current.InternalElement.Is<IReference<DomNode>>())
            {
                instancingNode = current.InternalElement.DomNode;
            }

            var group = current.InternalElement.Cast<Group>();
            var pins = inputSide ? group.InputGroupPins : group.OutputGroupPins;
            current = pins.First(pin => pin.Index == current.InternalPinIndex);
        }

        // 返回最终的 DomNode
        if (current.InternalElement.Is<IReference<DomNode>>())
        {
            instancingNode ??= current.InternalElement.DomNode;
            return current.InternalElement.As<IReference<DomNode>>().Target;
        }

        return current.InternalElement.DomNode;
    }

    private int GetLeafPinIndex(bool inputSide)
    {
        GroupPin current = this;  // 用局部变量替代对 this 的赋值

        while (current.InternalElement.Is<Group>())
        {
            Group group = current.InternalElement.Cast<Group>();

            if (inputSide)
            {
                current = group.InputGroupPins.First(x => x.Index == current.InternalPinIndex);
            }
            else
            {
                current = group.OutputGroupPins.First(x => x.Index == current.InternalPinIndex);
            }
        }

        return current.InternalPinIndex;
    }
}
