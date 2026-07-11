using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

[Export(typeof(CircuitControlRegistry))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CircuitControlRegistry
{
	private readonly IControlHostService m_controlHostService;

	private readonly IContextRegistry m_contextRegistry;

	private readonly Dictionary<DomNode, Pair<Control, ControlInfo>> m_circuitNodeControls = new Dictionary<DomNode, Pair<Control, ControlInfo>>();

	public IEnumerable<KeyValuePair<DomNode, Pair<Control, ControlInfo>>> CircuitNodeControls => m_circuitNodeControls;

	[ImportingConstructor]
	public CircuitControlRegistry(IControlHostService controlHostService, IContextRegistry contextRegistry, IDocumentService documentService)
	{
		m_controlHostService = controlHostService;
		m_contextRegistry = contextRegistry;
		documentService.DocumentOpened += documentService_DocumentOpened;
		documentService.DocumentClosed += documentService_DocumentClosed;
	}

	public virtual void RegisterControl(DomNode circuitNode, Control control, ControlInfo controlInfo, IControlHostClient client)
	{
		m_circuitNodeControls.Add(circuitNode, new Pair<Control, ControlInfo>(control, controlInfo));
		m_controlHostService.RegisterControl(control, controlInfo, client);
	}

	public virtual bool UnregisterControl(Control control)
	{
		bool result = false;
		KeyValuePair<DomNode, Pair<Control, ControlInfo>> keyValuePair = m_circuitNodeControls.FirstOrDefault((KeyValuePair<DomNode, Pair<Control, ControlInfo>> n) => n.Value.First == control);
		if (keyValuePair.Key != null)
		{
			UnregisterControl(keyValuePair.Key, keyValuePair.Value.First);
			result = true;
		}
		return result;
	}

	public ControlInfo GetCircuitControlInfo(DomNode domNode)
	{
		return (from ctrol in m_circuitNodeControls
			where ctrol.Key == domNode
			select ctrol.Value.Second).FirstOrDefault();
	}

	public DomNode GetDomNode(Control control)
	{
		return (from ctrol in m_circuitNodeControls
			where ctrol.Value.Second.Control == control
			select ctrol.Key).FirstOrDefault();
	}

	private void UnregisterControl(DomNode circuitNode, Control control)
	{
		m_contextRegistry.RemoveContext(circuitNode.As<CircuitEditingContext>());
		m_controlHostService.UnregisterControl(control);
		control.Visible = false;
		control.Dispose();
		m_circuitNodeControls.Remove(circuitNode);
		circuitNode.Cast<ViewingContext>().Control = null;
	}

	private void documentService_DocumentOpened(object sender, DocumentEventArgs e)
	{
		if (e.Document.Is<DomNode>())
		{
			DomNode domNode = e.Document.Cast<DomNode>();
			domNode.AttributeChanged += OnDocumentNodeAttributeChanged;
			domNode.ChildRemoved += docNode_ChildRemoved;
		}
	}

	private void docNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.Child.Is<Group>())
		{
			CloseEditingContext(e.Child.As<CircuitEditingContext>());
		}
	}

	private void documentService_DocumentClosed(object sender, DocumentEventArgs e)
	{
		if (!e.Document.Is<DomNode>())
		{
			return;
		}
		DomNode domNode = e.Document.Cast<DomNode>();
		domNode.AttributeChanged -= OnDocumentNodeAttributeChanged;
		domNode.ChildRemoved -= docNode_ChildRemoved;
		KeyValuePair<DomNode, Pair<Control, ControlInfo>>[] array = m_circuitNodeControls.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<DomNode, Pair<Control, ControlInfo>> keyValuePair = array[i];
			if (keyValuePair.Key.Lineage.Contains(domNode))
			{
				UnregisterControl(keyValuePair.Key, keyValuePair.Value.First);
			}
		}
	}

	protected virtual void OnDocumentNodeAttributeChanged(object sender, AttributeEventArgs e)
	{
		Group obj = e.DomNode.As<Group>();
		if (obj == null || !obj.IsNameAttribute(e.AttributeInfo))
		{
			return;
		}
		foreach (KeyValuePair<DomNode, Pair<Control, ControlInfo>> circuitNodeControl in m_circuitNodeControls)
		{
			if (circuitNodeControl.Key.Is<Group>())
			{
				circuitNodeControl.Value.Second.Name = CircuitUtil.GetGroupPath(circuitNodeControl.Key.Cast<Group>());
			}
		}
	}

	private void CloseEditingContext(CircuitEditingContext editingContext)
	{
		m_contextRegistry.RemoveContext(editingContext);
		if (editingContext.Is<ViewingContext>())
		{
			ViewingContext viewingContext = editingContext.Cast<ViewingContext>();
			if (viewingContext.Control != null)
			{
				UnregisterControl(viewingContext.DomNode, viewingContext.Control);
				viewingContext.Control = null;
			}
		}
	}
}
