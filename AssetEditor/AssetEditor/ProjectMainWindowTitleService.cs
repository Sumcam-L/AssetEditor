using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Applications;

namespace AssetEditor;

[Export(typeof(IInitializable))]
[Export(typeof(IProjectChangeWatcher))]
[Export(typeof(MainWindowTitleService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ProjectMainWindowTitleService : MainWindowTitleService, IProjectChangeWatcher
{
	private readonly ICivTechService m_civTechService;

	private readonly ISynchronizeInvoke m_uiInvoker;

	[ImportingConstructor]
	public ProjectMainWindowTitleService(IMainWindow mainWindow, IDocumentRegistry documentRegistry, ICivTechService civTechSvc, ISynchronizeInvoke uiInvoker)
		: base(mainWindow, documentRegistry)
	{
		m_civTechService = civTechSvc;
		m_uiInvoker = uiInvoker;
	}

	public virtual void HandleProjectChange(Action<string> statusMessagePrinter)
	{
		statusMessagePrinter("Updating title service...");
		if (m_uiInvoker != null && m_uiInvoker.InvokeRequired)
		{
			m_uiInvoker.Invoke((Action)delegate
			{
				HandleProjectChange_Impl();
			}, null);
		}
		else
		{
			HandleProjectChange_Impl();
		}
	}

	private void HandleProjectChange_Impl()
	{
		UpdateMainWindow(base.MainWindow, base.DocumentRegistry.ActiveDocument);
		foreach (IDocument document in base.DocumentRegistry.Documents)
		{
			if (document is CompositeDomDocument compositeDomDocument)
			{
				compositeDomDocument.UpdateControlInfo();
			}
		}
	}

	protected override void UpdateMainWindow(IMainWindow mainWindow, IDocument activeDocument)
	{
		StringBuilder stringBuilder = new StringBuilder(Application.ProductName ?? "");
		if (activeDocument != null && activeDocument.Uri != null)
		{
			stringBuilder.Append(" - ");
			if (activeDocument.Uri.IsFile)
			{
				stringBuilder.Append(activeDocument.Uri.LocalPath);
			}
			else
			{
				stringBuilder.Append(activeDocument.Uri.ToString());
			}
			stringBuilder.Append("(" + m_civTechService.GetProjectName(activeDocument.Uri) + ")");
			if (activeDocument.Dirty)
			{
				stringBuilder.Append("*");
			}
			if (activeDocument.IsReadOnly)
			{
				stringBuilder.Append("(Read Only)");
			}
		}
		mainWindow.Text = stringBuilder.ToString();
	}
}
