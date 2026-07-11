using Firaxis.CivTech;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ProjectConfigOperation : TransactionContext.Operation
{
	private ICivTechService m_civTechService;

	private IProjectConfigService m_projectConfigService;

	private string m_redoXML;

	private string m_undoXML;

	public ProjectConfigOperation(ICivTechService civTechSvc, IProjectConfigService projCfgSvc)
	{
		m_civTechService = civTechSvc;
		m_projectConfigService = projCfgSvc;
		m_undoXML = m_civTechService.PrimaryProject.Config.SerializeIntoXML();
		m_projectConfigService.LoadConfig();
		m_redoXML = m_civTechService.PrimaryProject.Config.SerializeIntoXML();
	}

	public override void Do()
	{
		m_projectConfigService.LoadConfig(m_redoXML);
	}

	public override void Undo()
	{
		m_projectConfigService.LoadConfig(m_undoXML);
	}
}
