using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(PrototypeLister))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PrototypeLister : TreeControlEditor, IControlHostClient, IInitializable
{
	[Import]
	private IControlHostService m_controlHostService;

	[Import]
	private IContextRegistry m_contextRegistry;

	private IPrototypingContext m_prototypingContext;

	private readonly ControlInfo m_controlInfo;

	private static readonly Image s_factoryImage = ResourceUtil.GetImage16(Resources.FactoryImage);

	public ControlInfo ControlInfo => m_controlInfo;

	public IPrototypingContext PrototypeContext
	{
		get
		{
			return m_prototypingContext;
		}
		set
		{
			if (m_prototypingContext != value)
			{
				m_prototypingContext = value;
				base.TreeView = m_prototypingContext;
			}
		}
	}

	[ImportingConstructor]
	public PrototypeLister(ICommandService commandService)
		: base(commandService)
	{
		Configure(out m_controlInfo);
	}

	protected virtual void Configure(out ControlInfo controlInfo)
	{
		controlInfo = new ControlInfo("Prototypes".Localize(), "Creates new instances from prototypes".Localize(), StandardControlGroup.Right, s_factoryImage, null);
		base.TreeControl.ShowRoot = false;
		base.TreeControl.AllowDrop = true;
		base.TreeControl.SelectionMode = SelectionMode.One;
	}

	void IInitializable.Initialize()
	{
		m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
		m_controlHostService.RegisterControl(base.TreeControl, m_controlInfo, this);
	}

	void IControlHostClient.Activate(Control control)
	{
		if (m_prototypingContext != null)
		{
			m_contextRegistry.ActiveContext = m_prototypingContext;
		}
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	protected override void OnStartDrag(IEnumerable<object> items)
	{
		IDataObject instances = m_prototypingContext.GetInstances(items);
		base.TreeControl.DoDragDrop(instances, DragDropEffects.All | DragDropEffects.Link);
	}

	protected override void OnLastHitChanged(EventArgs e)
	{
		if (m_prototypingContext != null)
		{
			m_prototypingContext.SetActiveItem(base.LastHit);
		}
		base.OnLastHitChanged(e);
	}

	private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		PrototypeContext = m_contextRegistry.GetMostRecentContext<IPrototypingContext>();
		base.TreeControl.Text = ((PrototypeContext != null) ? "Copy items from the document and paste them here to create prototypes that can be dragged and dropped onto a canvas.".Localize() : null);
	}
}
