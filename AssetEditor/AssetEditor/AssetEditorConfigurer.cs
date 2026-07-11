using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.Theme;
using Sce.Atf;
using Sce.Atf.Applications;

namespace AssetEditor;

[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetEditorConfigurer : IInitializable
{
	private readonly AutoDocumentService m_autoDocumentService;

	private readonly AssetBrowserFileCommands m_assetBrowserFileCommands;

	[ImportingConstructor]
	public AssetEditorConfigurer(AutoDocumentService autoDocumentService, AssetBrowserFileCommands assetBrowserCmds, IThemeService themeSvc)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Starting up AssetEditorConfigurer");
		m_autoDocumentService = autoDocumentService;
		m_assetBrowserFileCommands = assetBrowserCmds;
		themeSvc.ActiveTheme = new FiraxisTheme();
	}

	void IInitializable.Initialize()
	{
		m_assetBrowserFileCommands.FileSaveFailureBehavior = FileSaveFailureBehavior.Prompt;
		m_autoDocumentService.AutoNewDocument = false;
	}
}
