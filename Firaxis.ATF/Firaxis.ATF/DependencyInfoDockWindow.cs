using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class DependencyInfoDockWindow : IInitializable, IControlHostClient, IPartImportsSatisfiedNotification
{
	[Import(AllowDefault = false)]
	private IControlHostService m_controlHostService;

	private DependencyInfoControl m_dependencyInfoControl;

	[Import(AllowDefault = false)]
	private IDocumentRegistry m_documentRegistry;

	private ICivTechService m_civTechSvc;

	private IDocumentService m_documentSvc;

	private ICommandService m_cmdSvc;

	[ImportMany]
	private IEnumerable<Lazy<IDocumentClient>> m_documentClients;

	[ImportingConstructor]
	public DependencyInfoDockWindow(ICivTechService civTechSvc, IDocumentService docSvc, ICommandService cmdSvc)
	{
		m_civTechSvc = civTechSvc;
		m_documentSvc = docSvc;
		m_cmdSvc = cmdSvc;
	}

	void IControlHostClient.Activate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	void IInitializable.Initialize()
	{
		if (m_controlHostService != null && m_documentRegistry != null)
		{
			m_documentRegistry.ActiveDocumentChanged += DocumentRegistry_ActiveDocumentChanged;
			m_controlHostService.RegisterControl(m_dependencyInfoControl, "Dependency Info", "Dependency information view", StandardControlGroup.Bottom, null, this);
		}
	}

	private void DocumentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		m_dependencyInfoControl.SetActiveDocument(m_documentRegistry.ActiveDocument);
	}

	public void OnImportsSatisfied()
	{
		m_dependencyInfoControl = new DependencyInfoControl(m_civTechSvc, m_documentSvc, m_cmdSvc, m_documentClients);
	}
}
