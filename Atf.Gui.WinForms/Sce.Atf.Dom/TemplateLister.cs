using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

[InheritedExport(typeof(TemplateLister))]
[InheritedExport(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Any)]
public class TemplateLister : TreeControlEditor, IControlHostClient, IInitializable
{
	[Import]
	private IControlHostService m_controlHostService;

	[Import]
	private IContextRegistry m_contextRegistry;

	private ITemplatingContext m_templatingContext;

	private readonly ControlInfo m_controlInfo;

	private readonly Image m_templateLibraryImage = ResourceUtil.GetImage16(Resources.ComponentsImage);

	public ControlInfo ControlInfo => m_controlInfo;

	public ITemplatingContext TemplateContext
	{
		get
		{
			return m_templatingContext;
		}
		set
		{
			if (m_templatingContext != value)
			{
				m_templatingContext = value;
				base.TreeView = m_templatingContext;
			}
		}
	}

	[ImportingConstructor]
	public TemplateLister(ICommandService commandService)
		: base(commandService)
	{
		Configure(out m_controlInfo);
	}

	protected virtual void Configure(out ControlInfo controlInfo)
	{
		controlInfo = new ControlInfo("Templates".Localize(), "Reference subgraphs from templates".Localize(), StandardControlGroup.Right, m_templateLibraryImage, null);
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
		if (m_templatingContext != null)
		{
			m_contextRegistry.ActiveContext = m_templatingContext;
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
		IDataObject instances = m_templatingContext.GetInstances(items);
		base.TreeControl.DoDragDrop(instances, DragDropEffects.All | DragDropEffects.Link);
	}

	protected override void OnLastHitChanged(EventArgs e)
	{
		if (m_templatingContext != null)
		{
			m_templatingContext.SetActiveItem(base.LastHit);
		}
		base.OnLastHitChanged(e);
	}

	protected override void OnDragOver(DragEventArgs e)
	{
		e.Effect = DragDropEffects.None;
		if (m_templatingContext != null)
		{
			IInstancingContext instancingContext = m_templatingContext.As<IInstancingContext>();
			if (instancingContext != null && instancingContext.CanInsert(e.Data))
			{
				e.Effect = DragDropEffects.Move;
			}
		}
	}

	protected override void OnDragDrop(DragEventArgs e)
	{
		if (m_templatingContext == null)
		{
			return;
		}
		IInstancingContext instancingContext = m_templatingContext.As<IInstancingContext>();
		if (instancingContext != null)
		{
			string text = "Drag and Drop".Localize();
			instancingContext.As<ITransactionContext>().DoTransaction(delegate
			{
				instancingContext.Insert(e.Data);
			}, text);
			if (base.StatusService != null)
			{
				base.StatusService.ShowStatus(text);
			}
		}
	}

	private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		TemplateContext = m_contextRegistry.GetMostRecentContext<ITemplatingContext>();
	}
}
