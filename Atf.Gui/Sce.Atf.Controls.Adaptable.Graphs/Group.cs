using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class Group : Element, ICircuitGroupType<Element, Wire, ICircuitPin>, IHierarchicalGraphNode<Element, Wire, ICircuitPin>, IGraphNode, ICircuitElementType, IGraph<Element, Wire, ICircuitPin>, IAnnotatedDiagram, ICircuitContainer
{
	public enum PinOrderStyle
	{
		NodeY,
		DepthFirst
	}

	private static IList<Annotation> s_emptyAnnotations = new List<Annotation>().AsReadOnly();

	private PinOrderStyle m_defaultPinOrder = PinOrderStyle.NodeY;

	private DomNodeListAdapter<Element> m_elements;

	private DomNodeListAdapter<Wire> m_wires;

	private DomNodeListAdapter<Annotation> m_annotations;

	private DomNodeListAdapter<GroupPin> m_inputs;

	private DomNodeListAdapter<GroupPin> m_outputs;

	private int[] m_inputPinsMap;

	private int[] m_outputPinsMap;

	private bool m_dirty;

	private bool m_expanded;

	private CircuitGroupInfo m_info;

	protected abstract AttributeInfo MinWidthAttribute { get; }

	protected abstract AttributeInfo MinHeightAttribute { get; }

	protected abstract AttributeInfo WidthAttribute { get; }

	protected abstract AttributeInfo HeightAttribute { get; }

	protected abstract AttributeInfo AutosizeAttribute { get; }

	protected virtual AttributeInfo ExpandedAttribute => null;

	protected abstract AttributeInfo ShowExpandedGroupPinsAttribute { get; }

	protected abstract ChildInfo ElementChildInfo { get; }

	protected abstract ChildInfo WireChildInfo { get; }

	protected abstract ChildInfo InputChildInfo { get; }

	protected abstract ChildInfo OutputChildInfo { get; }

	protected virtual ChildInfo AnnotationChildInfo => null;

	protected abstract DomNodeType GroupPinType { get; }

	public PinOrderStyle DefaultPinOrder
	{
		get
		{
			return m_defaultPinOrder;
		}
		set
		{
			m_defaultPinOrder = value;
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
			if (value)
			{
				m_inputPinsMap = null;
				m_outputPinsMap = null;
			}
			m_dirty = value;
		}
	}

	public bool IgnoreFanInOut { get; set; }

	public override ICircuitElementType Type => this;

	public override IEnumerable<ICircuitPin> AllInputPins => InputGroupPins;

	public override IEnumerable<ICircuitPin> AllOutputPins => OutputGroupPins;

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

	public virtual IList<ICircuitPin> Inputs => (from n in m_inputs
		orderby n.Index
		where n.Visible
		select n).ToArray();

	public virtual IList<ICircuitPin> Outputs => (from n in m_outputs
		orderby n.Index
		where n.Visible
		select n).ToArray();

	public virtual IEnumerable<GroupPin> InputGroupPins => m_inputs;

	public virtual IEnumerable<GroupPin> OutputGroupPins => m_outputs;

	Size ICircuitElementType.InteriorSize => default(Size);

	Image ICircuitElementType.Image => null;

	IList<ICircuitPin> ICircuitElementType.Inputs => Inputs;

	IList<ICircuitPin> ICircuitElementType.Outputs => Outputs;

	string ICircuitElementType.Name => "Group".Localize();

	IEnumerable<Element> IGraph<Element, Wire, ICircuitPin>.Nodes => m_elements;

	IEnumerable<Wire> IGraph<Element, Wire, ICircuitPin>.Edges => m_wires;

	IEnumerable<IAnnotation> IAnnotatedDiagram.Annotations => m_annotations.AsIEnumerable<IAnnotation>();

	public Size Size
	{
		get
		{
			return new Size(GetAttribute<int>(WidthAttribute), GetAttribute<int>(HeightAttribute));
		}
		set
		{
			SetAttribute(WidthAttribute, value.Width);
			SetAttribute(HeightAttribute, value.Height);
		}
	}

	public Size MinimumSize
	{
		get
		{
			return new Size(GetAttribute<int>(MinWidthAttribute), GetAttribute<int>(MinHeightAttribute));
		}
		set
		{
			SetAttribute(MinWidthAttribute, value.Width);
			SetAttribute(MinHeightAttribute, value.Height);
		}
	}

	IEnumerable<Element> IHierarchicalGraphNode<Element, Wire, ICircuitPin>.SubNodes => ((IGraph<Element, Wire, ICircuitPin>)this).Nodes;

	IEnumerable<Wire> ICircuitGroupType<Element, Wire, ICircuitPin>.SubEdges => m_wires;

	public virtual bool Expanded
	{
		get
		{
			return m_expanded;
		}
		set
		{
			if (value != m_expanded)
			{
				m_expanded = value;
				OnChanged(EventArgs.Empty);
			}
		}
	}

	public virtual CircuitGroupInfo Info => m_info;

	public virtual bool AutoSize
	{
		get
		{
			return GetAttribute<bool>(AutosizeAttribute);
		}
		set
		{
			SetAttribute(AutosizeAttribute, value);
		}
	}

	public virtual bool ShowExpandedGroupPins
	{
		get
		{
			return GetAttribute<bool>(ShowExpandedGroupPinsAttribute);
		}
		set
		{
			SetAttribute(ShowExpandedGroupPinsAttribute, value);
			Info.ShowExpandedGroupPins = value;
		}
	}

	public virtual IGraph<Element, Wire, ICircuitPin> ParentGraph => GetParentAs<IGraph<Element, Wire, ICircuitPin>>();

	public event EventHandler Changed;

	protected Group()
	{
		m_info = new CircuitGroupInfo();
		m_info.PropertyChanged += GroupInfoChanged;
	}

	protected override void OnNodeSet()
	{
		SetUpGraphData();
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		Info.MinimumSize = MinimumSize;
		Info.ShowExpandedGroupPins = ShowExpandedGroupPins;
		foreach (GroupPin inputGroupPin in InputGroupPins)
		{
			inputGroupPin.SetPinTarget(inputSide: true);
			if (inputGroupPin.InternalElement.Is<IReference<DomNode>>())
			{
				inputGroupPin.PinTarget.InstancingNode = inputGroupPin.InternalElement.DomNode;
			}
		}
		foreach (GroupPin outputGroupPin in OutputGroupPins)
		{
			outputGroupPin.SetPinTarget(inputSide: false);
			if (outputGroupPin.InternalElement.Is<IReference<DomNode>>())
			{
				outputGroupPin.PinTarget.InstancingNode = outputGroupPin.InternalElement.DomNode;
			}
		}
		foreach (Wire wire in Wires)
		{
			wire.SetPinTarget();
		}
		Info.HiddenInputPins = m_inputs.Where((GroupPin x) => !x.Visible).AsIEnumerable<ICircuitPin>();
		Info.HiddenOutputPins = m_outputs.Where((GroupPin x) => !x.Visible).AsIEnumerable<ICircuitPin>();
		UpdateGroupPinInfo();
		UpdateOffset();
	}

	public virtual void OnChanged(EventArgs e)
	{
		UpdateOffset();
		this.Changed.Raise(this, e);
		if (base.DomNode.Parent != null && base.DomNode.Parent.Is<Group>())
		{
			base.DomNode.Parent.Cast<Group>().Dirty = true;
			base.DomNode.Parent.Cast<Group>().Update();
		}
	}

	public override bool HasInputPin(ICircuitPin pin)
	{
		return InputGroupPins.Contains(pin);
	}

	public override bool HasOutputPin(ICircuitPin pin)
	{
		return OutputGroupPins.Contains(pin);
	}

	public override ICircuitPin InputPin(int pinIndex)
	{
		if (m_inputPinsMap == null)
		{
			m_inputPinsMap = new int[m_inputs.Count];
			int num = m_inputs.Count;
			while (--num >= 0)
			{
				m_inputPinsMap[m_inputs[num].Index] = num;
			}
		}
		return m_inputs[m_inputPinsMap[pinIndex]];
	}

	public override ICircuitPin OutputPin(int pinIndex)
	{
		if (m_outputPinsMap == null)
		{
			m_outputPinsMap = new int[m_outputs.Count];
			int num = m_outputs.Count;
			while (--num >= 0)
			{
				m_outputPinsMap[m_outputs[num].Index] = num;
			}
		}
		return m_outputs[m_outputPinsMap[pinIndex]];
	}

	public void UpdateGroupPinInfo()
	{
		bool flag = !ParentGraph.Is<Group>();
		if (ParentGraph == null || ParentGraph.Edges == null)
		{
			return;
		}
		IEnumerable<Wire> source = ParentGraph.Edges.Where((Wire x) => x.InputElement.DomNode == base.DomNode);
		foreach (GroupPin groupPin in InputGroupPins)
		{
			groupPin.Info.ExternalConnected = source.Any((Wire e) => e.InputPinTarget == groupPin.PinTarget);
			if (groupPin.Info.ExternalConnected)
			{
				groupPin.Visible = true;
			}
			if (!(groupPin.Info.ExternalConnected && flag))
			{
				continue;
			}
			foreach (GroupPin item in groupPin.SinkChain(inputSide: true))
			{
				item.Info.ExternalConnected = true;
				item.Visible = true;
			}
		}
		IEnumerable<Wire> source2 = ParentGraph.Edges.Where((Wire x) => x.OutputElement.DomNode == base.DomNode);
		foreach (GroupPin groupPin2 in OutputGroupPins)
		{
			groupPin2.Info.ExternalConnected = source2.Any((Wire e) => e.OutputPinTarget == groupPin2.PinTarget);
			if (groupPin2.Info.ExternalConnected)
			{
				groupPin2.Visible = true;
			}
			if (!(groupPin2.Info.ExternalConnected && flag))
			{
				continue;
			}
			foreach (GroupPin item2 in groupPin2.SinkChain(inputSide: false))
			{
				item2.Info.ExternalConnected = true;
				item2.Visible = true;
			}
		}
	}

	private void UpdatePinOrders()
	{
		for (int i = 0; i < 2; i++)
		{
			GroupPin[] array = ((i == 0) ? m_inputs.OrderBy((GroupPin n) => n.Position.Y).ToArray() : m_outputs.OrderBy((GroupPin n) => n.Position.Y).ToArray());
			int num = 0;
			foreach (GroupPin groupPin in array)
			{
				if (groupPin.Visible)
				{
					groupPin.Index = num;
					num++;
				}
			}
			foreach (GroupPin groupPin2 in array)
			{
				if (!groupPin2.Visible)
				{
					groupPin2.Index = num;
					num++;
				}
			}
		}
		foreach (GroupPin inputGroupPin in InputGroupPins)
		{
			if (inputGroupPin.PinTarget == null)
			{
				inputGroupPin.SetPinTarget(inputSide: true);
				inputGroupPin.SetPinTarget(inputSide: true);
			}
			if (!ParentGraph.Is<Group>())
			{
				continue;
			}
			Group obj = ParentGraph.Cast<Group>();
			foreach (GroupPin inputGroupPin2 in obj.InputGroupPins)
			{
				if (inputGroupPin2.PinTarget == inputGroupPin.PinTarget)
				{
					inputGroupPin2.InternalPinIndex = inputGroupPin.Index;
				}
			}
		}
		foreach (GroupPin outputGroupPin in OutputGroupPins)
		{
			if (outputGroupPin.PinTarget == null)
			{
				outputGroupPin.SetPinTarget(inputSide: false);
				outputGroupPin.SetPinTarget(inputSide: false);
			}
			if (!ParentGraph.Is<Group>())
			{
				continue;
			}
			Group obj2 = ParentGraph.Cast<Group>();
			foreach (GroupPin outputGroupPin2 in obj2.OutputGroupPins)
			{
				if (outputGroupPin2.PinTarget == outputGroupPin.PinTarget)
				{
					outputGroupPin2.InternalPinIndex = outputGroupPin.Index;
				}
			}
		}
	}

	public void Update()
	{
		if (Dirty)
		{
			List<Wire> internalConnections = new List<Wire>();
			List<Wire> list = new List<Wire>();
			GetSubGraphConnections(internalConnections, list, list);
			UpdateGroupPins(m_elements, internalConnections, list);
			UpdatePinOrders();
			Dirty = false;
			OnChanged(EventArgs.Empty);
		}
	}

	public void UpdateGroupPins(IEnumerable<Element> modules, List<Wire> internalConnections, List<Wire> externalConnections)
	{
		foreach (Wire externalConnection in externalConnections)
		{
			if (modules.Contains(externalConnection.InputElement))
			{
				GroupPin groupPin = MatchedGroupPin(externalConnection.InputElement, externalConnection.InputPin.Index, inputSide: true);
				if (groupPin == null)
				{
					ICircuitPin circuitPin = externalConnection.InputElement.Type.Inputs[externalConnection.InputPin.Index];
					GroupPin groupPin2 = new DomNode(GroupPinType).As<GroupPin>();
					groupPin2.TypeName = circuitPin.TypeName;
					groupPin2.InternalElement = externalConnection.InputElement;
					groupPin2.InternalPinIndex = circuitPin.Index;
					groupPin2.Index = m_inputs.Count;
					groupPin2.Position = new Point(0, (groupPin2.InternalPinIndex + 1) * 16 + externalConnection.InputElement.Bounds.Location.Y);
					groupPin2.IsDefaultName = true;
					groupPin2.Visible = true;
					m_inputs.Add(groupPin2);
					groupPin2.SetPinTarget(inputSide: true);
					if (groupPin2.InternalElement.Is<IReference<DomNode>>())
					{
						groupPin2.PinTarget.InstancingNode = groupPin2.InternalElement.DomNode;
					}
					groupPin2.Name = groupPin2.DefaultName(inputSide: true);
				}
				else
				{
					groupPin.Visible = true;
				}
				continue;
			}
			GroupPin groupPin3 = MatchedGroupPin(externalConnection.OutputElement, externalConnection.OutputPin.Index, inputSide: false);
			if (groupPin3 == null)
			{
				ICircuitPin circuitPin2 = externalConnection.OutputElement.Type.Outputs[externalConnection.OutputPin.Index];
				GroupPin groupPin4 = new DomNode(GroupPinType).As<GroupPin>();
				groupPin4.TypeName = circuitPin2.TypeName;
				groupPin4.InternalElement = externalConnection.OutputElement;
				groupPin4.InternalPinIndex = circuitPin2.Index;
				groupPin4.Index = m_outputs.Count;
				groupPin4.Position = new Point(0, (groupPin4.InternalPinIndex + 1) * 16 + externalConnection.OutputElement.Bounds.Location.Y);
				groupPin4.IsDefaultName = true;
				groupPin4.Visible = true;
				m_outputs.Add(groupPin4);
				groupPin4.SetPinTarget(inputSide: false);
				if (groupPin4.InternalElement.Is<IReference<DomNode>>())
				{
					groupPin4.PinTarget.InstancingNode = groupPin4.InternalElement.DomNode;
				}
				groupPin4.Name = groupPin4.DefaultName(inputSide: false);
			}
			else
			{
				groupPin3.Visible = true;
			}
		}
		foreach (Element module in modules)
		{
			Element element = module;
			foreach (ICircuitPin item in module.AllInputPins.OrderBy((ICircuitPin n) => element.PinDisplayOrder(n.Index, inputSide: true)))
			{
				GroupPin groupPin5 = MatchedGroupPin(module, item.Index, inputSide: true);
				if (CanExposePin(module, item, internalConnections, inputSide: true))
				{
					if (groupPin5 == null)
					{
						GroupPin groupPin6 = new DomNode(GroupPinType).As<GroupPin>();
						groupPin6.TypeName = item.TypeName;
						groupPin6.InternalElement = module;
						groupPin6.InternalPinIndex = item.Index;
						groupPin6.Index = m_inputs.Count;
						groupPin6.Position = new Point(0, (groupPin6.InternalPinIndex + 1) * 16 + module.Bounds.Location.Y);
						groupPin6.Visible = (item.Is<IVisible>() ? item.Cast<IVisible>().Visible : ShowExpandedGroupPins);
						groupPin6.IsDefaultName = true;
						m_inputs.Add(groupPin6);
						groupPin6.SetPinTarget(inputSide: true);
						if (groupPin6.InternalElement.Is<IReference<DomNode>>())
						{
							groupPin6.PinTarget.InstancingNode = groupPin6.InternalElement.DomNode;
						}
						groupPin6.Name = groupPin6.DefaultName(inputSide: true);
					}
					else
					{
						groupPin5.InternalElement = module;
					}
				}
				else if (groupPin5 != null)
				{
					m_inputs.Remove(groupPin5);
				}
			}
			foreach (ICircuitPin item2 in module.AllOutputPins.OrderBy((ICircuitPin n) => element.PinDisplayOrder(n.Index, inputSide: false)))
			{
				GroupPin groupPin7 = MatchedGroupPin(module, item2.Index, inputSide: false);
				if (CanExposePin(module, item2, internalConnections, inputSide: false))
				{
					if (groupPin7 == null)
					{
						GroupPin groupPin8 = new DomNode(GroupPinType).As<GroupPin>();
						groupPin8.TypeName = item2.TypeName;
						groupPin8.InternalElement = module;
						groupPin8.InternalPinIndex = item2.Index;
						groupPin8.Index = m_outputs.Count;
						groupPin8.Position = new Point(0, (groupPin8.InternalPinIndex + 1) * 16 + module.Bounds.Location.Y);
						groupPin8.Visible = (item2.Is<IVisible>() ? item2.Cast<IVisible>().Visible : ShowExpandedGroupPins);
						groupPin8.IsDefaultName = true;
						m_outputs.Add(groupPin8);
						groupPin8.SetPinTarget(inputSide: false);
						if (groupPin8.InternalElement.Is<IReference<DomNode>>())
						{
							groupPin8.PinTarget.InstancingNode = groupPin8.InternalElement.DomNode;
						}
						groupPin8.Name = groupPin8.DefaultName(inputSide: false);
					}
					else
					{
						groupPin7.InternalElement = module;
					}
				}
				else if (groupPin7 != null)
				{
					m_outputs.Remove(groupPin7);
				}
			}
		}
		RemoveDanglingGroupPins();
		Info.HiddenInputPins = m_inputs.Where((GroupPin x) => !x.Visible).AsIEnumerable<ICircuitPin>();
		Info.HiddenOutputPins = m_outputs.Where((GroupPin x) => !x.Visible).AsIEnumerable<ICircuitPin>();
	}

	private void RemoveDanglingGroupPins()
	{
		GroupPin[] array = m_inputs.ToArray();
		foreach (GroupPin groupPin in array)
		{
			PinTarget pinTarget = groupPin.PinTarget;
			if (pinTarget != null && groupPin.InternalElement.Is<IReference<DomNode>>())
			{
				pinTarget.InstancingNode = groupPin.InternalElement.DomNode;
			}
			if (pinTarget == null || !Elements.Contains(groupPin.InternalElement) || groupPin.InternalElement.MatchPinTarget(pinTarget, inputSide: true).First == null)
			{
				m_inputs.Remove(groupPin);
			}
			else if (groupPin.IsDefaultName)
			{
				groupPin.Name = groupPin.DefaultName(inputSide: true);
			}
		}
		GroupPin[] array2 = m_outputs.ToArray();
		foreach (GroupPin groupPin2 in array2)
		{
			PinTarget pinTarget = groupPin2.PinTarget;
			if (pinTarget != null && groupPin2.InternalElement.Is<IReference<DomNode>>())
			{
				pinTarget.InstancingNode = groupPin2.InternalElement.DomNode;
			}
			if (pinTarget == null || !Elements.Contains(groupPin2.InternalElement) || groupPin2.InternalElement.MatchPinTarget(pinTarget, inputSide: false).First == null)
			{
				m_outputs.Remove(groupPin2);
			}
			else if (groupPin2.IsDefaultName)
			{
				groupPin2.Name = groupPin2.DefaultName(inputSide: false);
			}
		}
	}

	public void InitializeGroupPinIndexes(IEnumerable<Wire> internalConnections)
	{
		if (DefaultPinOrder == PinOrderStyle.NodeY)
		{
			List<GroupPin> list = (from n in m_inputs
				orderby n.InternalElement.Bounds.Location.Y, n.InternalElement.PinDisplayOrder(n.InternalPinIndex, inputSide: true)
				select n).ToList();
			foreach (GroupPin input in m_inputs)
			{
				int index = list.IndexOf(input);
				input.Index = index;
			}
			List<GroupPin> list2 = (from n in m_outputs
				orderby n.InternalElement.Bounds.Location.Y, n.InternalElement.PinDisplayOrder(n.InternalPinIndex, inputSide: false)
				select n).ToList();
			foreach (GroupPin output in m_outputs)
			{
				int index2 = list2.IndexOf(output);
				output.Index = index2;
			}
			int num = 0;
			int num2 = 0;
			Element element = null;
			foreach (GroupPin item in list)
			{
				if (element == null)
				{
					num2 = item.InternalElement.Bounds.Location.Y;
					item.Position = new Point(0, num);
					element = item.InternalElement;
					continue;
				}
				int num3 = num + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin;
				if (element != item.InternalElement)
				{
					num3 = Math.Max(item.InternalElement.Bounds.Location.Y - num2, num + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinElementMargin);
					element = item.InternalElement;
				}
				item.Position = new Point(0, num3);
				num = num3;
			}
			num = 0;
			element = null;
			foreach (GroupPin item2 in list2)
			{
				if (element == null)
				{
					num2 = item2.InternalElement.Bounds.Location.Y;
					item2.Position = new Point(0, num);
					element = item2.InternalElement;
					continue;
				}
				int num4 = num + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin;
				if (element != item2.InternalElement)
				{
					num4 = Math.Max(item2.InternalElement.Bounds.Location.Y - num2, num + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinElementMargin);
					element = item2.InternalElement;
				}
				item2.Position = new Point(0, num4);
				num = num4;
			}
		}
		else if (DefaultPinOrder == PinOrderStyle.DepthFirst)
		{
			IOrderedEnumerable<Element> orderedEnumerable = from x in Elements
				where !Wires.Any((Wire e) => e.OutputElement == x)
				orderby x.Bounds.Y
				select x;
			List<GroupPin> list3 = new List<GroupPin>();
			foreach (Element item3 in orderedEnumerable)
			{
				BackDepthOrderVistor(item3, internalConnections, list3);
			}
			foreach (GroupPin input2 in m_inputs)
			{
				int index3 = list3.IndexOf(input2);
				input2.Index = index3;
			}
			List<GroupPin> list4 = (from n in m_outputs
				orderby n.InternalElement.Bounds.Location.Y, n.InternalPinIndex
				select n).ToList();
			foreach (GroupPin output2 in m_outputs)
			{
				int index4 = list4.IndexOf(output2);
				output2.Index = index4;
			}
			int num5 = 0;
			foreach (GroupPin item4 in m_inputs.OrderBy((GroupPin n) => n.Index))
			{
				item4.Position = new Point(0, num5 * (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin));
				num5++;
			}
			num5 = 0;
			foreach (GroupPin item5 in m_outputs.OrderBy((GroupPin n) => n.Index))
			{
				item5.Position = new Point(0, num5 * (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin));
				num5++;
			}
		}
		UpdatePinOrders();
	}

	protected virtual bool CanExposePin(Element element, ICircuitPin pin, IEnumerable<Wire> internalConnections, bool inputSide)
	{
		DomNode referencingDomNode = (element.Is<IReference<DomNode>>() ? element.DomNode : null);
		if (!IgnoreFanInOut)
		{
			foreach (Wire internalConnection in internalConnections)
			{
				if (inputSide)
				{
					PinTarget pinTarget = (pin.Is<GroupPin>() ? pin.Cast<GroupPin>().PinTarget : new PinTarget(element.DomNode, pin.Index, null));
					if (pinTarget == internalConnection.InputPinTarget && !pin.AllowFanIn)
					{
						return false;
					}
				}
				else
				{
					PinTarget pinTarget2 = (pin.Is<GroupPin>() ? pin.Cast<GroupPin>().PinTarget : new PinTarget(element.DomNode, pin.Index, referencingDomNode));
					if (pinTarget2 == internalConnection.OutputPinTarget && !pin.AllowFanOut)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public void Validate()
	{
		List<Wire> internalConnections = new List<Wire>();
		List<Wire> list = new List<Wire>();
		GetSubGraphConnections(internalConnections, list, list);
		List<GroupPin> source = m_inputs.Where((GroupPin p) => p.Visible).ToList();
		List<GroupPin> source2 = m_outputs.Where((GroupPin p) => p.Visible).ToList();
		IEnumerable<IGrouping<GroupPin, GroupPin>> enumerable = from g in source
			group g by g into w
			where w.Count() > 1
			select w;
		enumerable = from g in source2
			group g by g into w
			where w.Count() > 1
			select w;
		IEnumerable<IGrouping<Element, GroupPin>> enumerable2 = from g in source
			group g by g.InternalElement;
		foreach (IGrouping<Element, GroupPin> item in enumerable2)
		{
			IEnumerable<IGrouping<int, GroupPin>> enumerable3 = from g in item
				group g by g.Index into w
				where w.Count() > 1
				select w;
		}
		enumerable2 = from g in source2
			group g by g.InternalElement;
		foreach (IGrouping<Element, GroupPin> item2 in enumerable2)
		{
			IEnumerable<IGrouping<int, GroupPin>> enumerable4 = from g in item2
				group g by g.Index into w
				where w.Count() > 1
				select w;
		}
		foreach (GroupPin inputGroupPin in InputGroupPins)
		{
		}
		foreach (GroupPin outputGroupPin in OutputGroupPins)
		{
		}
		IEnumerable<IGrouping<int, GroupPin>> enumerable5 = from g in source
			group g by g.Index into w
			where w.Count() > 1
			select w;
		enumerable5 = from g in source2
			group g by g.Index into w
			where w.Count() > 1
			select w;
		foreach (Element element3 in Elements)
		{
			foreach (ICircuitPin input in element3.Type.Inputs)
			{
				if (CanExposePin(element3, input, Wires, inputSide: true))
				{
					GroupPin groupPin = MatchedGroupPin(element3, input.Index, inputSide: true);
				}
				else
				{
					GroupPin groupPin2 = MatchedGroupPin(element3, input.Index, inputSide: true);
				}
			}
			foreach (ICircuitPin output in element3.Type.Outputs)
			{
				if (CanExposePin(element3, output, Wires, inputSide: false))
				{
					GroupPin groupPin3 = MatchedGroupPin(element3, output.Index, inputSide: false);
				}
				else
				{
					GroupPin groupPin4 = MatchedGroupPin(element3, output.Index, inputSide: false);
				}
			}
		}
		if (ParentGraph == null)
		{
			return;
		}
		IEnumerable<Wire> enumerable6 = ParentGraph.Edges.Where((Wire x) => x.InputElement.DomNode == base.DomNode);
		foreach (Wire item3 in enumerable6)
		{
		}
		foreach (GroupPin input2 in m_inputs)
		{
			Element element = input2.PinTarget.LeafDomNode.Cast<Element>();
			ICircuitPin circuitPin = element.Type.Inputs[input2.PinTarget.LeafPinIndex];
			if (!circuitPin.AllowFanIn)
			{
				GroupPin pin = input2;
				int num = enumerable6.Count((Wire x) => x.InputPinTarget.FullyEquals(pin.PinTarget));
				int num2 = Wires.Count((Wire x) => x.InputPinTarget.FullyEquals(pin.PinTarget));
			}
		}
		IEnumerable<Wire> enumerable7 = ParentGraph.Edges.Where((Wire x) => x.OutputElement.DomNode == base.DomNode);
		foreach (Wire item4 in enumerable7)
		{
			string domNodeName = CircuitUtil.GetDomNodeName(item4.DomNode);
		}
		foreach (GroupPin output2 in m_outputs)
		{
			Element element2 = output2.PinTarget.LeafDomNode.Cast<Element>();
			ICircuitPin circuitPin2 = element2.Type.Outputs[output2.PinTarget.LeafPinIndex];
			if (!circuitPin2.AllowFanOut)
			{
				GroupPin pin2 = output2;
				int num3 = enumerable7.Count((Wire x) => x.OutputPinTarget == pin2.PinTarget);
				int num4 = Wires.Count((Wire x) => x.OutputPinTarget == pin2.PinTarget);
			}
		}
	}

	private void GetSubGraphConnections(ICollection<Wire> internalConnections, ICollection<Wire> incomingConnections, ICollection<Wire> outgoingConnections)
	{
		foreach (Wire wire in Wires)
		{
			bool flag = Elements.Contains(wire.OutputElement);
			bool flag2 = Elements.Contains(wire.InputElement);
			if (flag && flag2)
			{
				internalConnections.Add(wire);
			}
			else if (flag)
			{
				outgoingConnections.Add(wire);
			}
			else if (flag2)
			{
				incomingConnections.Add(wire);
			}
		}
	}

	public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		Pair<Element, ICircuitPin> result = default(Pair<Element, ICircuitPin>);
		GroupPin groupPin = (inputSide ? m_inputs.FirstOrDefault((GroupPin x) => x.PinTarget == pinTarget) : m_outputs.FirstOrDefault((GroupPin x) => x.PinTarget == pinTarget));
		if (groupPin != null && Elements.Contains(groupPin.InternalElement))
		{
			result.First = this;
			result.Second = groupPin;
		}
		return result;
	}

	public override Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
	{
		Pair<Element, ICircuitPin> result = default(Pair<Element, ICircuitPin>);
		GroupPin groupPin = (inputSide ? m_inputs.FirstOrDefault((GroupPin x) => x.PinTarget.FullyEquals(pinTarget)) : m_outputs.FirstOrDefault((GroupPin x) => x.PinTarget.FullyEquals(pinTarget)));
		if (groupPin != null && Elements.Contains(groupPin.InternalElement))
		{
			result.First = this;
			result.Second = groupPin;
		}
		return result;
	}

	public GroupPin MatchedGroupPin(Element node, int pinIndex, bool inputSide)
	{
		if (node.Is<Group>())
		{
			Group obj = node.Cast<Group>();
			if (inputSide)
			{
				GroupPin nestedGrpPin = obj.InputGroupPins.First((GroupPin x) => x.Index == pinIndex);
				return m_inputs.FirstOrDefault((GroupPin x) => x.PinTarget.FullyEquals(nestedGrpPin.PinTarget));
			}
			GroupPin nestedGrpPin2 = obj.OutputGroupPins.First((GroupPin x) => x.Index == pinIndex);
			return m_outputs.FirstOrDefault((GroupPin x) => x.PinTarget.FullyEquals(nestedGrpPin2.PinTarget));
		}
		return inputSide ? m_inputs.FirstOrDefault((GroupPin x) => x.InternalElement.DomNode == node.DomNode && x.InternalPinIndex == pinIndex) : m_outputs.FirstOrDefault((GroupPin x) => x.InternalElement.DomNode == node.DomNode && x.InternalPinIndex == pinIndex);
	}

	public bool IsNameAttribute(AttributeInfo attributeInfo)
	{
		return attributeInfo == NameAttribute || attributeInfo == LabelAttribute;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		Dirty = true;
		if (e.AttributeInfo == MinHeightAttribute || e.AttributeInfo == MinWidthAttribute)
		{
			Info.MinimumSize = MinimumSize;
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		Dirty = true;
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		Dirty = true;
	}

	private void SetUpGraphData()
	{
		m_elements = new DomNodeListAdapter<Element>(base.DomNode, ElementChildInfo);
		m_wires = new DomNodeListAdapter<Wire>(base.DomNode, WireChildInfo);
		if (AnnotationChildInfo != null)
		{
			m_annotations = new DomNodeListAdapter<Annotation>(base.DomNode, AnnotationChildInfo);
		}
		m_inputs = new DomNodeListAdapter<GroupPin>(base.DomNode, InputChildInfo);
		m_outputs = new DomNodeListAdapter<GroupPin>(base.DomNode, OutputChildInfo);
	}

	private void BackDepthOrderVistor(Element outputNode, IEnumerable<Wire> internalConnections, List<GroupPin> grpPinVisited)
	{
		int num = 0;
		foreach (ICircuitPin input in outputNode.Type.Inputs)
		{
			if (CanExposePin(outputNode, input, internalConnections, inputSide: true))
			{
				PinTarget inputPinTarget = (input.Is<GroupPin>() ? input.Cast<GroupPin>().PinTarget : new PinTarget(GetDomLeafNode(outputNode.DomNode), input.Index, null));
				GroupPin item = m_inputs.First((GroupPin n) => n.PinTarget == inputPinTarget);
				grpPinVisited.Add(item);
			}
			else
			{
				foreach (Wire internalConnection in internalConnections)
				{
					if (internalConnection.InputElement == outputNode && internalConnection.InputPin.Index == num)
					{
						BackDepthOrderVistor(internalConnection.OutputElement, internalConnections, grpPinVisited);
						break;
					}
				}
			}
			num++;
		}
	}

	private DomNode GetDomLeafNode(DomNode node)
	{
		DomNode domNode = node;
		while (domNode.Is<IReference<DomNode>>())
		{
			domNode = domNode.Cast<IReference<DomNode>>().Target;
		}
		return domNode;
	}

	private void ConstrainCoords()
	{
		Point point = default(Point);
		if (Elements.Any())
		{
			point.X = Elements.Select((Element x) => x.Bounds.Location.X).Min();
			point.Y = Elements.Select((Element x) => x.Bounds.Location.Y).Min();
		}
		if (InputGroupPins.Any())
		{
			point.Y = Math.Min(InputGroupPins.Select((GroupPin x) => x.Position.Y).Min(), point.Y);
		}
		if (OutputGroupPins.Any())
		{
			point.Y = Math.Min(OutputGroupPins.Select((GroupPin x) => x.Position.Y).Min(), point.Y);
		}
		Point p = default(Point);
		if (point.X < 0)
		{
			p.X = -point.X;
		}
		if (point.Y < 0)
		{
			p.Y = -point.Y;
		}
		if (p.IsEmpty)
		{
			return;
		}
		foreach (Element element in Elements)
		{
			Point location = element.Bounds.Location;
			location.Offset(p);
			element.Position = location;
		}
		foreach (GroupPin inputGroupPin in InputGroupPins)
		{
			Point location2 = inputGroupPin.Bounds.Location;
			location2.Offset(p);
			inputGroupPin.Position = location2;
		}
		foreach (GroupPin outputGroupPin in OutputGroupPins)
		{
			Point location3 = outputGroupPin.Bounds.Location;
			location3.Offset(p);
			outputGroupPin.Position = location3;
		}
	}

	private void UpdateOffset()
	{
		Point empty = Point.Empty;
		if (Elements == null)
		{
			return;
		}
		if (Elements.Any())
		{
			int num = Elements.Min((Element n) => n.Bounds.Location.X);
			int num2 = Elements.Min((Element n) => n.Bounds.Location.Y);
			if (Inputs.Any())
			{
				num2 = Math.Min(num2, Inputs.Min((ICircuitPin p) => p.Cast<GroupPin>().Bounds.Location.Y));
			}
			if (Outputs.Any())
			{
				num2 = Math.Min(num2, Outputs.Min((ICircuitPin p) => p.Cast<GroupPin>().Bounds.Location.Y));
			}
			empty.X = -num;
			empty.Y = -num2;
		}
		Info.Offset = empty;
	}

	protected virtual void GroupInfoChanged(object sender, PropertyChangedEventArgs e)
	{
		MinimumSize = Info.MinimumSize;
		ShowExpandedGroupPins = Info.ShowExpandedGroupPins;
	}
}
