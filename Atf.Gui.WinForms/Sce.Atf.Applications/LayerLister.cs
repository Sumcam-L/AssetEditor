using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(LayerLister))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LayerLister : TreeControlEditor, IControlHostClient, IInitializable
{
	[Import]
	private IControlHostService m_controlHostService;

	[Import]
	private IContextRegistry m_contextRegistry;

	private readonly ControlInfo m_controlInfo;

	private ILayeringContext m_layeringContext;

	private static readonly Image s_layerImage = ResourceUtil.GetImage16(Resources.LayerImage);

	public ControlInfo ControlInfo => m_controlInfo;

	public ILayeringContext LayeringContext
	{
		get
		{
			return m_layeringContext;
		}
		set
		{
			if (m_layeringContext != value)
			{
				m_layeringContext = value;
				base.TreeView = m_layeringContext;
			}
		}
	}

	[ImportingConstructor]
	public LayerLister(ICommandService commandService)
		: base(commandService)
	{
		Configure(out m_controlInfo);
		base.TreeControl.NodeCheckStateEdited += treeControl_NodeCheckStateEdited;
	}

	protected virtual void Configure(out ControlInfo controlInfo)
	{
		controlInfo = new ControlInfo("Layers".Localize(), "Edits document layers".Localize(), StandardControlGroup.Right, s_layerImage, null);
		base.TreeControl.ShowRoot = false;
		base.TreeControl.AllowDrop = true;
		base.TreeControl.SelectionMode = SelectionMode.MultiExtended;
	}

	void IInitializable.Initialize()
	{
		m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
		m_controlHostService.RegisterControl(base.TreeControl, m_controlInfo, this);
	}

	void IControlHostClient.Activate(Control control)
	{
		if (m_layeringContext != null)
		{
			m_contextRegistry.ActiveContext = m_layeringContext;
		}
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	protected override void OnLastHitChanged(EventArgs e)
	{
		if (m_layeringContext != null)
		{
			m_layeringContext.SetActiveItem(base.LastHit);
		}
		base.OnLastHitChanged(e);
	}

	private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		LayeringContext = m_contextRegistry.GetMostRecentContext<ILayeringContext>();
		if (LayeringContext != null)
		{
			base.TreeControl.Text = "Copy items from the document and paste them here to create layers whose visibility can be controlled by clicking on a check box.".Localize();
		}
		else
		{
			base.TreeControl.Text = null;
		}
	}

	private void treeControl_NodeCheckStateEdited(object sender, TreeControl.NodeEventArgs e)
	{
		ShowLayer(e.Node.Tag, e.Node.CheckState == CheckState.Checked);
	}

	public void ShowLayer(object layer, bool show)
	{
		ITransactionContext context = m_layeringContext.As<ITransactionContext>();
		context.DoTransaction(delegate
		{
			m_layeringContext.SetVisible(layer, show);
		}, "Show/Hide Layer".Localize());
	}
}
