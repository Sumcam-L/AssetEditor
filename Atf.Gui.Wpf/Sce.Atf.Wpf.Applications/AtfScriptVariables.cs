using System.ComponentModel.Composition;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AtfScriptVariables : IInitializable
{
	private readonly ScriptingService m_scriptingService;

	[Import(AllowDefault = true)]
	private IControlHostService m_controlHostService = null;

	[Import(AllowDefault = true)]
	private ICommandService m_commandService = null;

	[Import(AllowDefault = true)]
	private StandardSelectionCommands m_standardSelectionCommands = null;

	[Import(AllowDefault = true)]
	private StandardEditCommands m_standardEditCommands = null;

	[Import(AllowDefault = true)]
	private IContextRegistry m_contextRegistry = null;

	[Import(AllowDefault = true)]
	private IDocumentRegistry m_documentRegistry = null;

	[Import(AllowDefault = true)]
	private IDocumentService m_documentService = null;

	[Import(AllowDefault = true)]
	private StandardFileCommands m_standardFileCommands = null;

	[Import(AllowDefault = true)]
	private StandardFileExitCommand m_standardFileExitCommand = null;

	[Import(AllowDefault = true)]
	private StandardEditHistoryCommands m_standardEditHistoryCommands = null;

	[Import(AllowDefault = true)]
	private PropertyEditor m_propertyEditor = null;

	[Import(AllowDefault = true)]
	private SourceControlService m_sourceControlService = null;

	[Import(AllowDefault = true)]
	private IWindowLayoutService m_windowLayoutService = null;

	[ImportingConstructor]
	public AtfScriptVariables(ScriptingService scriptService)
	{
		m_scriptingService = scriptService;
	}

	public virtual void Initialize()
	{
		if (m_scriptingService != null)
		{
			if (m_controlHostService != null)
			{
				m_scriptingService.SetVariable("atfControls", m_controlHostService);
			}
			if (m_commandService != null)
			{
				m_scriptingService.SetVariable("atfCommands", m_commandService);
			}
			if (m_standardSelectionCommands != null)
			{
				m_scriptingService.SetVariable("atfSelect", m_standardSelectionCommands);
			}
			if (m_standardFileCommands != null)
			{
				m_scriptingService.SetVariable("atfFile", m_standardFileCommands);
			}
			if (m_standardFileExitCommand != null)
			{
				m_scriptingService.SetVariable("atfFileExit", m_standardFileExitCommand);
			}
			if (m_standardEditCommands != null)
			{
				m_scriptingService.SetVariable("atfEdit", m_standardEditCommands);
			}
			if (m_standardEditHistoryCommands != null)
			{
				m_scriptingService.SetVariable("atfHistory", m_standardEditHistoryCommands);
			}
			if (m_contextRegistry != null)
			{
				m_scriptingService.SetVariable("atfContextReg", m_contextRegistry);
			}
			if (m_documentRegistry != null)
			{
				m_scriptingService.SetVariable("atfDocReg", m_documentRegistry);
			}
			if (m_documentService != null)
			{
				m_scriptingService.SetVariable("atfDocService", m_documentService);
			}
			if (m_propertyEditor != null)
			{
				m_scriptingService.SetVariable("atfPropertyEditor", m_propertyEditor);
			}
			if (m_sourceControlService != null)
			{
				m_scriptingService.SetVariable("atfSourceControl", m_sourceControlService);
			}
			if (m_windowLayoutService != null)
			{
				m_scriptingService.SetVariable("atfLayout", m_windowLayoutService);
			}
		}
	}
}
