using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class CircuitEditingContext : EditingContext, IEnumerableContext, INamingContext, IInstancingContext, IObservableContext, IColoringContext, IEditableGraphContainer<Element, Wire, ICircuitPin>, IEditableGraph<Element, Wire, ICircuitPin>
{
	private enum MoveElementBehavior
	{
		MoveConstrainToCursorContainment,
		MoveConstrainToContainerBounds
	}

	public Func<AdaptableControl, Element, RectangleF> GetLocalBound;

	public Func<AdaptableControl, IEnumerable<Element>, Point> GetWorldOffset;

	public Func<AdaptableControl, int> GetTitleHeight;

	public Func<AdaptableControl, int> GetLabelHeight;

	public Func<AdaptableControl, Point> GetSubContentOffset;

	private static Color s_zeroColor = default(Color);

	private IInstancingContext m_instancingContext;

	private ITemplatingContext m_templatingContext;

	private ICircuitContainer m_circuitContainer;

	private IViewingContext m_viewingContext;

	private XmlSchemaTypeLoader m_schemaLoader;

	private MoveElementBehavior m_moveElementBehavior = MoveElementBehavior.MoveConstrainToCursorContainment;

	private bool m_supportsNestedGroup = true;

	protected abstract DomNodeType WireType { get; }

	public ICircuitContainer CircuitContainer => m_circuitContainer;

	public XmlSchemaTypeLoader SchemaLoader
	{
		get
		{
			return m_schemaLoader;
		}
		set
		{
			m_schemaLoader = value;
		}
	}

	public bool SupportsNestedGroup
	{
		get
		{
			return m_supportsNestedGroup;
		}
		set
		{
			m_supportsNestedGroup = value;
		}
	}

	public static string CircuitFormat { get; set; }

	IEnumerable<object> IEnumerableContext.Items
	{
		get
		{
			foreach (Element element in CircuitContainer.Elements)
			{
				yield return element;
			}
			foreach (Wire wire in CircuitContainer.Wires)
			{
				yield return wire;
			}
			if (CircuitContainer.Annotations == null)
			{
				yield break;
			}
			foreach (Annotation annotation in CircuitContainer.Annotations)
			{
				yield return annotation;
			}
		}
	}

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded
	{
		add
		{
		}
		remove
		{
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		m_instancingContext = base.DomNode.Type.AsAll<IInstancingContext>().FirstOrDefault((IInstancingContext x) => x != this);
		m_templatingContext = base.DomNode.As<ITemplatingContext>();
		m_circuitContainer = base.DomNode.Cast<ICircuitContainer>();
		m_viewingContext = base.DomNode.Cast<IViewingContext>();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		foreach (DomNode item in base.DomNode.Subtree)
		{
			AddNode(item);
		}
	}

	private void AddNode(DomNode node)
	{
		if (node.Is<Group>())
		{
			node.Cast<Group>().Changed += GroupChanged;
		}
	}

	private void RemoveNode(DomNode node)
	{
		if (node.Is<Group>())
		{
			node.Cast<Group>().Changed -= GroupChanged;
		}
	}

	string INamingContext.GetName(object item)
	{
		Element element = item.As<Element>();
		if (element != null)
		{
			return element.Name;
		}
		Wire wire = item.As<Wire>();
		if (wire != null)
		{
			return wire.Label;
		}
		return item.As<GroupPin>()?.Name;
	}

	bool INamingContext.CanSetName(object item)
	{
		return item.Is<Element>() || item.Is<Wire>() || item.Is<GroupPin>();
	}

	void INamingContext.SetName(object item, string name)
	{
		Element element = item.As<Element>();
		if (element != null)
		{
			element.Name = name;
			return;
		}
		Wire wire = item.As<Wire>();
		if (wire != null)
		{
			wire.Label = name;
			return;
		}
		GroupPin groupPin = item.As<GroupPin>();
		if (groupPin != null)
		{
			groupPin.Name = name;
		}
	}

	public virtual bool CanCopy()
	{
		if (m_instancingContext != null)
		{
			return m_instancingContext.CanCopy();
		}
		return base.Selection.Count > 0;
	}

	public virtual object Copy()
	{
		D2dAnnotationAdapter d2dAnnotationAdapter = m_viewingContext.Cast<AdaptableControl>().As<D2dAnnotationAdapter>();
		if (d2dAnnotationAdapter != null)
		{
			foreach (Annotation item in base.Selection.AsIEnumerable<Annotation>())
			{
				if (d2dAnnotationAdapter.CanCopyText(item))
				{
					DataObject dataObject = new DataObject();
					dataObject.SetText(d2dAnnotationAdapter.TextSelected(item));
					Clipboard.SetText(d2dAnnotationAdapter.TextSelected(item));
					return dataObject;
				}
			}
		}
		if (m_instancingContext != null)
		{
			return m_instancingContext.Copy();
		}
		HashSet<Element> hashSet = new HashSet<Element>(base.Selection.AsIEnumerable<Element>());
		HashSet<object> hashSet2 = new HashSet<object>(hashSet.AsIEnumerable<object>());
		foreach (Wire item2 in base.Selection.AsIEnumerable<Wire>())
		{
			if (IsConnectionCopyable(item2, hashSet))
			{
				hashSet2.Add(item2);
			}
		}
		if (hashSet2.Count == hashSet.Count)
		{
			foreach (Wire wire in m_circuitContainer.Wires)
			{
				if (IsConnectionCopyable(wire, hashSet))
				{
					hashSet2.Add(wire);
				}
			}
		}
		foreach (Annotation item3 in base.Selection.AsIEnumerable<Annotation>())
		{
			hashSet2.Add(item3.As<DomNode>());
		}
		DataObject dataObject2 = new DataObject(hashSet2.ToArray());
		DomNodeSerializer domNodeSerializer = new DomNodeSerializer();
		byte[] data = domNodeSerializer.Serialize(hashSet2.AsIEnumerable<DomNode>());
		dataObject2.SetData(CircuitFormat, data);
		return dataObject2;
	}

	public virtual bool CanInsert(object insertingObject)
	{
		D2dAnnotationAdapter d2dAnnotationAdapter = m_viewingContext.Cast<AdaptableControl>().As<D2dAnnotationAdapter>();
		if (d2dAnnotationAdapter != null)
		{
			foreach (Annotation item in base.Selection.AsIEnumerable<Annotation>())
			{
				if (d2dAnnotationAdapter.CanInsertText(item))
				{
					return true;
				}
			}
		}
		if (m_instancingContext != null)
		{
			return m_instancingContext.CanInsert(insertingObject);
		}
		IDataObject dataObject = (IDataObject)insertingObject;
		IEnumerable<object> compatibleData = GetCompatibleData(dataObject);
		return compatibleData != null;
	}

	public virtual void Insert(object insertingObject)
	{
		D2dAnnotationAdapter d2dAnnotationAdapter = m_viewingContext.Cast<AdaptableControl>().As<D2dAnnotationAdapter>();
		if (d2dAnnotationAdapter != null)
		{
			foreach (Annotation item in base.Selection.AsIEnumerable<Annotation>())
			{
				if (d2dAnnotationAdapter.CanInsertText(item))
				{
					d2dAnnotationAdapter.PasteFromClipboard(item);
					return;
				}
			}
		}
		if (m_instancingContext != null)
		{
			m_instancingContext.Insert(insertingObject);
			return;
		}
		AdaptableControl adaptableControl = m_viewingContext.As<AdaptableControl>();
		Point center = new Point(adaptableControl.Width / 2, adaptableControl.Height / 2);
		DragDropAdapter dragDropAdapter = adaptableControl.As<DragDropAdapter>();
		Group obj = null;
		if (dragDropAdapter != null && dragDropAdapter.IsDropping)
		{
			center = dragDropAdapter.MousePosition;
			GraphHitRecord<Element, Wire, ICircuitPin> graphHitRecord = Pick(dragDropAdapter.MousePosition);
			if (graphHitRecord != null)
			{
				if (graphHitRecord.SubItem.Is<Group>())
				{
					obj = graphHitRecord.SubItem.Cast<Group>();
				}
				else if (graphHitRecord.Item.Is<Group>())
				{
					obj = graphHitRecord.Item.Cast<Group>();
				}
			}
		}
		DomNode[] array = Insert(insertingObject, center);
		if (obj != null && array != null)
		{
			((IEditableGraphContainer<Element, Wire, ICircuitPin>)this).Move((object)obj, (IEnumerable<object>)array);
		}
	}

	public virtual bool CanDelete()
	{
		if (m_instancingContext != null)
		{
			return m_instancingContext.CanDelete();
		}
		return base.Selection.Count > 0;
	}

	public virtual void Delete()
	{
		if (m_instancingContext != null)
		{
			m_instancingContext.Delete();
			return;
		}
		foreach (DomNode item in base.Selection.AsIEnumerable<DomNode>())
		{
			if (item.Is<IAnnotation>())
			{
				D2dAnnotationAdapter d2dAnnotationAdapter = m_viewingContext.As<AdaptableControl>().As<D2dAnnotationAdapter>();
				if (d2dAnnotationAdapter != null && d2dAnnotationAdapter.CanDeleteTextSelection(item.Cast<IAnnotation>()))
				{
					d2dAnnotationAdapter.DeleteTextSelection(item.Cast<IAnnotation>());
					return;
				}
			}
			item.RemoveFromParent();
		}
		base.Selection.Clear();
	}

	protected virtual void OnObjectInserted(ItemInsertedEventArgs<object> e)
	{
		this.ItemInserted.Raise(this, e);
	}

	protected virtual void OnObjectRemoved(ItemRemovedEventArgs<object> e)
	{
		this.ItemRemoved.Raise(this, e);
	}

	protected virtual void OnObjectChanged(ItemChangedEventArgs<object> e)
	{
		this.ItemChanged.Raise(this, e);
	}

	public void NotifyObjectChanged(object element)
	{
		OnObjectChanged(new ItemChangedEventArgs<object>(element));
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (!IsCircuitItem(e.DomNode, e.DomNode.Parent))
		{
			return;
		}
		NotifyObjectChanged(e.DomNode);
		CircuitValidator circuitValidator = base.DomNode.GetRoot().As<CircuitValidator>();
		if (circuitValidator != null)
		{
			HistoryContext activeHistoryContext = circuitValidator.ActiveHistoryContext;
			if (activeHistoryContext != null && activeHistoryContext != this && activeHistoryContext.DomNode.Ancestry.FirstOrDefault((DomNode x) => x == e.DomNode.Parent) != null && activeHistoryContext.InTransaction)
			{
				activeHistoryContext.AddOperation(new AttributeChangedOperation(e));
			}
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		AddNode(e.Child);
		if (!IsCircuitItem(e.Child, e.Parent))
		{
			return;
		}
		if (e.Child.Is<Wire>())
		{
			Wire wire = e.Child.Cast<Wire>();
			if (wire.InputElement.Is<Group>())
			{
				wire.InputElement.Cast<Group>().Dirty = true;
			}
			if (wire.OutputElement.Is<Group>())
			{
				wire.OutputElement.Cast<Group>().Dirty = true;
			}
		}
		OnObjectInserted(new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
		CircuitValidator circuitValidator = base.DomNode.GetRoot().As<CircuitValidator>();
		if (circuitValidator != null)
		{
			HistoryContext activeHistoryContext = circuitValidator.ActiveHistoryContext;
			if (activeHistoryContext != null && activeHistoryContext != this && activeHistoryContext.DomNode.Ancestry.FirstOrDefault((DomNode x) => x == e.Parent) != null && activeHistoryContext.InTransaction)
			{
				activeHistoryContext.AddOperation(new ChildInsertedOperation(e));
			}
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		RemoveNode(e.Child);
		if (!IsCircuitItem(e.Child, e.Parent))
		{
			return;
		}
		if (e.Child.Is<Wire>())
		{
			Wire wire = e.Child.Cast<Wire>();
			if (wire.InputElement.Is<Group>())
			{
				wire.InputElement.Cast<Group>().Dirty = true;
			}
			if (wire.OutputElement.Is<Group>())
			{
				wire.OutputElement.Cast<Group>().Dirty = true;
			}
		}
		OnObjectRemoved(new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
		CircuitValidator circuitValidator = base.DomNode.GetRoot().As<CircuitValidator>();
		if (circuitValidator != null)
		{
			HistoryContext activeHistoryContext = circuitValidator.ActiveHistoryContext;
			if (activeHistoryContext != null && activeHistoryContext != this && activeHistoryContext.DomNode.Ancestry.FirstOrDefault((DomNode x) => x == e.Parent) != null && activeHistoryContext.InTransaction)
			{
				activeHistoryContext.AddOperation(new ChildRemovedOperation(e));
			}
		}
	}

	private static bool IsCircuitItem(DomNode child, DomNode parent)
	{
		if (parent == null)
		{
			return false;
		}
		while (parent != null && parent.Is<LayerFolder>())
		{
			parent = parent.Parent;
		}
		return child.Is<Group>() || parent.Is<Circuit>() || parent.Is<Group>();
	}

	private void GroupChanged(object sender, EventArgs eventArgs)
	{
		Group obj = sender.Cast<Group>();
		foreach (DomNode item in obj.DomNode.Lineage)
		{
			item.As<CircuitEditingContext>()?.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(obj.DomNode));
		}
	}

	Color IColoringContext.GetColor(ColoringTypes kind, object item)
	{
		if (item.Is<Annotation>())
		{
			switch (kind)
			{
			case ColoringTypes.BackColor:
				return item.Cast<Annotation>().BackColor;
			case ColoringTypes.ForeColor:
				return item.Cast<Annotation>().ForeColor;
			}
		}
		return s_zeroColor;
	}

	bool IColoringContext.CanSetColor(ColoringTypes kind, object item)
	{
		if (item.Is<Annotation>())
		{
			switch (kind)
			{
			case ColoringTypes.BackColor:
				return true;
			case ColoringTypes.ForeColor:
				return true;
			}
		}
		return false;
	}

	void IColoringContext.SetColor(ColoringTypes kind, object item, Color newValue)
	{
		if (item.Is<Annotation>())
		{
			switch (kind)
			{
			case ColoringTypes.BackColor:
				item.Cast<Annotation>().BackColor = newValue;
				break;
			case ColoringTypes.ForeColor:
				item.Cast<Annotation>().ForeColor = newValue;
				break;
			}
		}
	}

	public virtual bool CanConnect(Element fromNode, ICircuitPin outputPin, Element toNode, ICircuitPin inputPin)
	{
		if (fromNode == null || outputPin == null || toNode == null || inputPin == null || outputPin.TypeName != inputPin.TypeName)
		{
			return false;
		}
		if (fromNode.HasOutputPin(outputPin) && toNode.HasInputPin(inputPin))
		{
			return true;
		}
		if (fromNode.HasInputPin(outputPin) && toNode.HasOutputPin(inputPin))
		{
			return true;
		}
		return false;
	}

	public virtual Wire Connect(Element fromNode, ICircuitPin fromRoute, Element toNode, ICircuitPin toRoute, Wire existingEdge)
	{
		DomNode adaptable = new DomNode(WireType);
		Wire wire = adaptable.As<Wire>();
		wire.OutputElement = fromNode;
		wire.OutputPin = fromRoute;
		wire.InputElement = toNode;
		wire.InputPin = toRoute;
		wire.SetPinTarget();
		if (existingEdge != null)
		{
			wire.Label = existingEdge.Label;
		}
		m_circuitContainer.Wires.Add(wire);
		if (!fromRoute.AllowFanOut || !toRoute.AllowFanIn)
		{
			Wire[] array = m_circuitContainer.Wires.ToArray();
			if (!fromRoute.AllowFanOut)
			{
				Wire[] array2 = array;
				foreach (Wire wire2 in array2)
				{
					if (wire2 != wire && wire2.OutputPin == fromRoute && wire2.OutputElement == wire.OutputElement)
					{
						m_circuitContainer.Wires.Remove(wire2);
					}
				}
			}
			if (!toRoute.AllowFanIn)
			{
				Wire[] array3 = array;
				foreach (Wire wire3 in array3)
				{
					if (wire3 != wire && wire3.InputPin == toRoute && wire3.InputElement == wire.InputElement)
					{
						m_circuitContainer.Wires.Remove(wire3);
					}
				}
			}
		}
		return wire;
	}

	public virtual bool CanDisconnect(Wire edge)
	{
		return true;
	}

	public virtual void Disconnect(Wire edge)
	{
		m_circuitContainer.Wires.Remove(edge);
	}

	bool IEditableGraphContainer<Element, Wire, ICircuitPin>.CanMove(object newParent, IEnumerable<object> movingObjects)
	{
		if (newParent == null)
		{
			newParent = this;
		}
		if (!newParent.Is<ICircuitContainer>())
		{
			return false;
		}
		ICircuitContainer circuitContainer = newParent.Cast<ICircuitContainer>();
		if (circuitContainer != m_circuitContainer && !circuitContainer.Expanded)
		{
			return false;
		}
		IEnumerable<Element> modules = movingObjects.AsIEnumerable<Element>();
		if (!modules.Any())
		{
			return false;
		}
		bool flag = newParent.Is<Group>();
		foreach (Element item in modules)
		{
			if (item.DomNode.GetRoot() != base.DomNode.GetRoot())
			{
				return false;
			}
			foreach (DomNode item2 in circuitContainer.Cast<DomNode>().Lineage)
			{
				if (item.DomNode.Equals(item2))
				{
					return false;
				}
			}
			if (item.DomNode.Parent == circuitContainer.Cast<DomNode>())
			{
				return false;
			}
			if (IsSelfContainedOrIntersected(item.DomNode, circuitContainer.Cast<DomNode>()))
			{
				return false;
			}
			if (!IsContained(item.DomNode, circuitContainer.Cast<DomNode>()))
			{
				return false;
			}
			if (!SupportsNestedGroup && flag && item.Is<Group>())
			{
				return false;
			}
		}
		return modules.Skip(1).All((Element x) => x.DomNode.Parent == modules.First().DomNode.Parent);
	}

	void IEditableGraphContainer<Element, Wire, ICircuitPin>.Move(object newParent, IEnumerable<object> movingObjects)
	{
		if (newParent == null)
		{
			newParent = this;
		}
		object[] array = movingObjects.ToArray();
		HashSet<Element> modules = new HashSet<Element>();
		Element[] array2 = array.AsIEnumerable<Element>().ToArray();
		ICircuitContainer circuitContainer = newParent.Cast<ICircuitContainer>();
		ICircuitContainer circuitContainer2 = array2.First().DomNode.Parent.Cast<ICircuitContainer>();
		List<Wire> list = new List<Wire>();
		List<Wire> incomingConnections = new List<Wire>();
		List<Wire> outgoingConnections = new List<Wire>();
		CircuitUtil.GetSubGraph(circuitContainer2, array, modules, list, incomingConnections, outgoingConnections);
		CircuitValidator circuitValidator = base.DomNode.GetRoot().Cast<CircuitValidator>();
		circuitValidator.Suspended = true;
		Element[] array3 = array2;
		foreach (Element item in array3)
		{
			circuitContainer2.Elements.Remove(item);
			circuitContainer.Elements.Add(item);
		}
		foreach (Wire item2 in list)
		{
			circuitContainer2.Wires.Remove(item2);
			circuitContainer.Wires.Add(item2);
		}
		Point relativeOffset = GetRelativeOffset(circuitContainer2, circuitContainer);
		Element[] array4 = array2;
		foreach (Element element in array4)
		{
			Point location = element.Bounds.Location;
			location.Offset(relativeOffset);
			element.Bounds = new Rectangle(location, element.Bounds.Size);
		}
		circuitValidator.Suspended = false;
		circuitValidator.MovingCrossContainer = true;
	}

	bool IEditableGraphContainer<Element, Wire, ICircuitPin>.CanResize(object container, DiagramBorder borderPart)
	{
		if (container.Is<Group>())
		{
			if (container.Is<IReference<Group>>())
			{
				return false;
			}
			Group obj = container.Cast<Group>();
			if (obj.Expanded && (borderPart.Border == DiagramBorder.BorderType.Right || borderPart.Border == DiagramBorder.BorderType.Bottom))
			{
				ILayoutContext layoutContext = m_viewingContext.Cast<ILayoutContext>();
				BoundsSpecified boundsSpecified = layoutContext.CanSetBounds(obj);
				if ((boundsSpecified & BoundsSpecified.Width) != BoundsSpecified.None || (boundsSpecified & BoundsSpecified.Height) != BoundsSpecified.None)
				{
					return true;
				}
			}
		}
		return false;
	}

	void IEditableGraphContainer<Element, Wire, ICircuitPin>.Resize(object container, int newWidth, int newHeight)
	{
		AdaptableControl arg = m_viewingContext.Cast<AdaptableControl>();
		Group obj = container.Cast<Group>();
		if (!string.IsNullOrEmpty(obj.Name))
		{
			newHeight -= GetLabelHeight(arg);
		}
		if (obj.AutoSize)
		{
			obj.Info.MinimumSize = new Size(newWidth, newHeight);
		}
		else
		{
			obj.Bounds = new Rectangle(obj.Bounds.Location.X, obj.Bounds.Location.Y, newWidth, newHeight);
		}
		obj.OnChanged(EventArgs.Empty);
	}

	public void Center(IEnumerable<object> items, Point p)
	{
		LayoutContexts.GetBounds(m_viewingContext.Cast<ILayoutContext>(), items, out var _);
		Matrix transform = m_viewingContext.Cast<AdaptableControl>().Cast<ITransformAdapter>().Transform;
		p = GdiUtil.InverseTransform(transform, p);
		m_viewingContext.Cast<ILayoutContext>().Center(items, p);
	}

	protected virtual GraphHitRecord<Element, Wire, ICircuitPin> Pick(Point point)
	{
		return null;
	}

	public T Insert<T>(DomNode domNode, int xPos, int yPos) where T : class
	{
		DataObject dataObject = new DataObject(new object[1] { domNode });
		ITransactionContext context = this.As<ITransactionContext>();
		context.DoTransaction(delegate
		{
			Insert(dataObject, new Point(xPos, yPos));
		}, "Scripted Insert Object");
		return base.Selection.GetLastSelected<T>();
	}

	public void SetProperty(DomNode node, AttributeInfo attr, object newValue)
	{
		ITransactionContext context = this.As<ITransactionContext>();
		context.DoTransaction(delegate
		{
			node.SetAttribute(attr, newValue);
		}, "Scripted Edit Property");
	}

	private DomNode[] Insert(object insertingObject, Point center)
	{
		IDataObject dataObject = (IDataObject)insertingObject;
		IEnumerable<object> compatibleData = GetCompatibleData(dataObject);
		if (compatibleData == null)
		{
			return null;
		}
		if (compatibleData.All((object x) => x.Is<Template>()))
		{
			List<object> list = new List<object>();
			foreach (Template item in compatibleData.AsIEnumerable<Template>())
			{
				list.Add(InsertReference(item));
			}
			Center(list, center);
			base.Selection.SetRange(list);
			return null;
		}
		DomNode[] array = DomNode.Copy(compatibleData.AsIEnumerable<DomNode>());
		List<Element> list2 = new List<Element>(array.AsIEnumerable<Element>());
		foreach (Element item2 in list2)
		{
			m_circuitContainer.Elements.Add(item2);
		}
		foreach (Wire item3 in array.AsIEnumerable<Wire>())
		{
			m_circuitContainer.Wires.Add(item3);
		}
		foreach (Annotation item4 in array.AsIEnumerable<Annotation>())
		{
			m_circuitContainer.Annotations.Add(item4);
		}
		Center(array, center);
		base.Selection.SetRange(array);
		return array;
	}

	private DomNode InsertReference(Template template)
	{
		Element element = m_templatingContext.CreateReference(template).Cast<Element>();
		m_circuitContainer.Elements.Add(element);
		return element.DomNode;
	}

	private IEnumerable<object> GetCompatibleData(IDataObject dataObject)
	{
		IEnumerable<object> enumerable = dataObject.GetData(typeof(object[])) as object[];
		if (enumerable == null)
		{
			return null;
		}
		if (!ValidTemplateReferences(enumerable))
		{
			return null;
		}
		if (AreCircuitItems(enumerable) || AreTemplateItems(enumerable))
		{
			return enumerable;
		}
		if (dataObject.GetData(CircuitFormat) is byte[] data)
		{
			try
			{
				DomNodeSerializer domNodeSerializer = new DomNodeSerializer();
				IEnumerable<DomNode> enumerable2 = domNodeSerializer.Deserialize(data, m_schemaLoader.GetNodeType);
				enumerable = enumerable2.AsIEnumerable<object>();
				if (AreCircuitItems(enumerable))
				{
					return enumerable;
				}
			}
			catch
			{
			}
		}
		return null;
	}

	private bool AreCircuitItems(IEnumerable<object> items)
	{
		return items.All(IsCircuitItem);
	}

	private bool AreTemplateItems(IEnumerable<object> items)
	{
		return items.All(IsTemplateItem);
	}

	private static bool IsCircuitItem(object item)
	{
		return item.Is<Element>() || item.Is<Wire>() || item.Is<Annotation>();
	}

	private bool IsTemplateItem(object item)
	{
		return m_templatingContext != null && m_templatingContext.CanReference(item);
	}

	private bool ValidTemplateReferences(IEnumerable<object> items)
	{
		TemplatingContext templatingContext = m_templatingContext.As<TemplatingContext>();
		if (templatingContext == null)
		{
			return true;
		}
		foreach (object item in items)
		{
			IReference<DomNode> reference = item.As<IReference<DomNode>>();
			if (reference == null)
			{
				continue;
			}
			Guid guid = Guid.Empty;
			GroupReference groupReference = reference.As<GroupReference>();
			if (groupReference != null)
			{
				guid = groupReference.Template.Guid;
			}
			else
			{
				ElementReference elementReference = reference.As<ElementReference>();
				if (elementReference != null)
				{
					guid = elementReference.Template.Guid;
				}
			}
			if (guid != Guid.Empty && templatingContext.SearchForTemplateByGuid(templatingContext.RootFolder, guid) == null)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsConnectionCopyable(Wire wire, HashSet<Element> modules)
	{
		return modules.Contains(wire.OutputElement) && modules.Contains(wire.InputElement);
	}

	private Point GetRelativeOffset(ICircuitContainer oldContainer, ICircuitContainer newContainer)
	{
		AdaptableControl arg = m_viewingContext.Cast<AdaptableControl>();
		Point result = default(Point);
		DomNode domNode = oldContainer.Cast<DomNode>();
		DomNode domNode2 = newContainer.Cast<DomNode>();
		DomNode commonAncestor = DomNode.GetLowestCommonAncestor(domNode, domNode2);
		IEnumerable<DomNode> enumerable = domNode.Lineage.TakeWhile((DomNode x) => x != commonAncestor);
		Point point = GetWorldOffset(arg, enumerable.AsIEnumerable<Element>());
		result.Offset(point.X, point.Y);
		IEnumerable<DomNode> enumerable2 = domNode2.Lineage.TakeWhile((DomNode x) => x != commonAncestor).Reverse();
		Point point2 = GetWorldOffset(arg, enumerable2.AsIEnumerable<Element>());
		result.Offset(-point2.X, -point2.Y);
		return result;
	}

	private bool IsSelfContainedOrIntersected(DomNode element, DomNode container)
	{
		DomNode commonAncestor = DomNode.GetLowestCommonAncestor(element, container);
		DomNode domNode = element.Lineage.First((DomNode x) => x.Parent == commonAncestor);
		if (element == domNode)
		{
			return false;
		}
		AdaptableControl adaptableControl = m_viewingContext.Cast<AdaptableControl>();
		RectangleF rect = GetLocalBound(adaptableControl, element.Cast<Element>());
		RectangleF rectangleF = GetLocalBound(adaptableControl, domNode.Cast<Element>());
		rectangleF.Location = new PointF(0f, GetTitleHeight(adaptableControl));
		rectangleF.Height -= GetLabelHeight(adaptableControl);
		rect.Offset(GetSubContentOffset(adaptableControl));
		bool flag = rectangleF.Contains(rect);
		rectangleF.Height -= GetTitleHeight(adaptableControl);
		bool flag2 = rectangleF.IntersectsWith(rect);
		return flag || flag2;
	}

	private bool IsContained(DomNode element, DomNode container)
	{
		if (container.Is<Circuit>())
		{
			return true;
		}
		if (m_moveElementBehavior == MoveElementBehavior.MoveConstrainToCursorContainment)
		{
			return true;
		}
		if (m_moveElementBehavior == MoveElementBehavior.MoveConstrainToContainerBounds)
		{
			AdaptableControl adaptableControl = m_viewingContext.Cast<AdaptableControl>();
			Point relativeOffset = GetRelativeOffset(element.Parent.Cast<ICircuitContainer>(), container.Cast<ICircuitContainer>());
			RectangleF rect = GetLocalBound(adaptableControl, element.Cast<Element>());
			RectangleF rectangleF = GetLocalBound(adaptableControl, container.Cast<Element>());
			rect.Offset(relativeOffset);
			rectangleF.Location = new PointF(0f, GetTitleHeight(adaptableControl));
			rectangleF.Height -= GetLabelHeight(adaptableControl);
			rect.Offset(GetSubContentOffset(adaptableControl));
			return rectangleF.Contains(rect);
		}
		return false;
	}
}
