using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class GroupReference : Group, IReference<Group>, IReference<DomNode>
{
	private Group m_targetGroup;

	private List<GroupPin> m_inputs;

	private List<GroupPin> m_outputs;

	private CircuitGroupInfo m_info;

	private bool m_expanded;

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
			Refresh();
		}
	}

	public override IGraph<Element, Wire, ICircuitPin> ParentGraph => base.DomNode.Parent.As<IGraph<Element, Wire, ICircuitPin>>();

	public override IList<ICircuitPin> Inputs
	{
		get
		{
			if (m_targetGroup == null)
			{
				Element element = Template.Target.As<Element>();
				return element.Type.Inputs;
			}
			return (from n in m_inputs
				orderby n.Index
				where n.Visible
				select n).ToArray();
		}
	}

	public override IList<ICircuitPin> Outputs
	{
		get
		{
			if (m_targetGroup == null)
			{
				Element element = Template.Target.As<Element>();
				return element.Type.Outputs;
			}
			return (from n in m_outputs
				orderby n.Index
				where n.Visible
				select n).ToArray();
		}
	}

	public override bool Expanded
	{
		get
		{
			if (((IReference<Group>)this).Target.Type is MissingElementType)
			{
				return false;
			}
			return m_expanded;
		}
		set
		{
			if (value != m_expanded && !(((IReference<Group>)this).Target.Type is MissingElementType))
			{
				m_expanded = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	Group IReference<Group>.Target
	{
		get
		{
			return Template.Target.As<Group>();
		}
		set
		{
			throw new InvalidOperationException("The group template determines the target");
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

	public Group Group => m_targetGroup;

	public override IEnumerable<GroupPin> InputGroupPins
	{
		get
		{
			Group obj = Template.Target.As<Group>();
			if (obj == null)
			{
				return EmptyEnumerable<GroupPin>.Instance;
			}
			return m_inputs;
		}
	}

	public override IEnumerable<GroupPin> OutputGroupPins
	{
		get
		{
			Group obj = Template.Target.As<Group>();
			if (obj == null)
			{
				return EmptyEnumerable<GroupPin>.Instance;
			}
			return m_outputs;
		}
	}

	protected override void OnNodeSet()
	{
		m_info = new CircuitGroupInfo();
		m_inputs = new List<GroupPin>();
		m_outputs = new List<GroupPin>();
		Refresh();
	}

	public override bool HasInputPin(ICircuitPin pin)
	{
		if (m_targetGroup == null)
		{
			return false;
		}
		if (InputGroupPins.Contains(pin))
		{
			return true;
		}
		return m_targetGroup.HasInputPin(pin);
	}

	public override bool HasOutputPin(ICircuitPin pin)
	{
		if (m_targetGroup == null)
		{
			return false;
		}
		if (OutputGroupPins.Contains(pin))
		{
			return true;
		}
		return m_targetGroup.HasOutputPin(pin);
	}

	public override ICircuitPin InputPin(int pinIndex)
	{
		if (m_targetGroup == null)
		{
			Element element = Template.Target.As<Element>();
			return element.Type.Inputs[pinIndex];
		}
		return m_inputs.FirstOrDefault((GroupPin x) => x.Index == pinIndex);
	}

	public override ICircuitPin OutputPin(int pinIndex)
	{
		if (m_targetGroup == null)
		{
			Element element = Template.Target.As<Element>();
			return element.Type.Outputs[pinIndex];
		}
		return m_outputs.FirstOrDefault((GroupPin x) => x.Index == pinIndex);
	}

	public override Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		return MatchPinTarget(pinTarget, inputSide);
	}

	public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		if (pinTarget == null || pinTarget.InstancingNode != base.DomNode)
		{
			return default(Pair<Element, ICircuitPin>);
		}
		Pair<Element, ICircuitPin> result = m_targetGroup.MatchPinTarget(pinTarget, inputSide);
		if (result.First != null)
		{
			result.First = this;
		}
		return result;
	}

	bool IReference<Group>.CanReference(Group item)
	{
		return true;
	}

	bool IReference<DomNode>.CanReference(DomNode item)
	{
		return item.Is<Group>();
	}

	public void Refresh()
	{
		m_inputs.Clear();
		m_outputs.Clear();
		if (Template == null)
		{
			return;
		}
		Group obj = Template.Target.As<Group>();
		if (m_targetGroup != obj)
		{
			if (m_targetGroup != null)
			{
				m_targetGroup.Changed -= TargetGroupChanged;
			}
			m_targetGroup = obj;
			if (m_targetGroup != null)
			{
				m_targetGroup.Changed += TargetGroupChanged;
			}
		}
		if (m_targetGroup != null)
		{
			GroupPin[] array = m_targetGroup.InputGroupPins.ToArray();
			GroupPin[] array2 = m_targetGroup.OutputGroupPins.ToArray();
			DomNode[] array3 = DomNode.Copy(array.AsIEnumerable<DomNode>());
			for (int i = 0; i < array3.Length; i++)
			{
				DomNode adaptable = array3[i];
				GroupPin groupPin = adaptable.Cast<GroupPin>();
				groupPin.SetPinTarget(inputSide: true);
				groupPin.PinTarget.InstancingNode = base.DomNode;
				groupPin.Info = array[i].Info;
				m_inputs.Add(groupPin);
			}
			DomNode[] array4 = DomNode.Copy(array2.AsIEnumerable<DomNode>());
			for (int j = 0; j < array4.Length; j++)
			{
				DomNode adaptable2 = array4[j];
				GroupPin groupPin2 = adaptable2.Cast<GroupPin>();
				groupPin2.SetPinTarget(inputSide: false);
				groupPin2.PinTarget.InstancingNode = base.DomNode;
				groupPin2.Info = array2[j].Info;
				m_outputs.Add(groupPin2);
			}
			Info.Offset = m_targetGroup.Info.Offset;
		}
	}

	private void TargetGroupChanged(object sender, EventArgs eventArgs)
	{
		Refresh();
		OnChanged(eventArgs);
	}
}
