using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(HotloadTargetsDockWindow))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class HotloadTargetsDockWindow : IInitializable, IControlHostClient
{
	private HotloadTargetsControl m_autoHotloadControl;

	[Import(AllowDefault = false)]
	private IControlHostService m_controlHostService;

	[Import(AllowDefault = false)]
	private IDocumentRegistry m_documentRegistry;

	public Control Control => m_autoHotloadControl;

	[ImportingConstructor]
	public HotloadTargetsDockWindow(ICookableRegistry cookableReg, IFileWatcherService fileWatcher, ICivTechService civTechSvc)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			m_autoHotloadControl = new HotloadTargetsControl(cookableReg);
		}
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
			m_documentRegistry.DocumentAdded += DocumentRegistry_DocumentAdded;
			m_documentRegistry.DocumentRemoved += DocumentRegistry_DocumentRemoved;
			m_controlHostService.RegisterControl(m_autoHotloadControl, "Hotload Targets", "Hotload Target Controls", StandardControlGroup.Bottom, null, this);
		}
	}

	private void DocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		m_autoHotloadControl.PopulateItems();
	}

	private void DocumentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		m_autoHotloadControl.PopulateItems();
	}
}
