using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(ProjectLister))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ProjectLister : FilteredTreeControlEditor, IControlHostClient, IInitializable
{
	private readonly IControlHostService m_controlHostService;

	private readonly IContextRegistry m_contextRegistry;

	private readonly IDocumentRegistry m_documentRegistry;

	[ImportingConstructor]
	public ProjectLister(ICommandService commandService, IControlHostService controlHostService, IContextRegistry contextRegistry, IDocumentRegistry documentRegistry)
		: base(commandService)
	{
		m_controlHostService = controlHostService;
		m_contextRegistry = contextRegistry;
		m_documentRegistry = documentRegistry;
		m_documentRegistry.ActiveDocumentChanged += documentRegistry_ActiveDocumentChanged;
	}

	protected override void Configure(out TreeControl treeControl, out TreeControlAdapter treeControlAdapter)
	{
		base.Configure(out treeControl, out treeControlAdapter);
		treeControl.AllowDrop = true;
	}

	private void documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		ITreeView treeView = m_documentRegistry.GetMostRecentDocument<ITreeView>();
		if (treeView != null)
		{
			treeView = new FilteredTreeView(treeView, base.DefaultFilter);
		}
		if (!FilteredTreeView.Equals(base.TreeView, treeView))
		{
			if (base.TreeView != null)
			{
				m_contextRegistry.RemoveContext(base.TreeView);
			}
			base.TreeView = treeView;
			if (treeView != null)
			{
				m_contextRegistry.ActiveContext = treeView;
				m_controlHostService.Show(base.TreeControl);
			}
		}
	}

	public void Initialize()
	{
		m_controlHostService.RegisterControl(base.Control, new ControlInfo("Project Lister".Localize(), "Lists objects in the current document".Localize(), StandardControlGroup.Left), this);
	}

	void IControlHostClient.Activate(Control control)
	{
		if (base.TreeView != null)
		{
			m_contextRegistry.ActiveContext = base.TreeView;
		}
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}
}
