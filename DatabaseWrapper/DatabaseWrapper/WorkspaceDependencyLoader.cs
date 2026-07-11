using Firaxis.CivTech;
using Sce.Atf;

namespace DatabaseWrapper;

public class WorkspaceDependencyLoader : WorkspaceDependencyBase
{
	private string m_depFilePath;

	public WorkspaceDependencyLoader(IProjectMapService projMapSvc, IProjectConfigService projCfgSvc, string targetProject, string depFilePath)
		: base(projMapSvc, projCfgSvc, targetProject, testFilesExist: true)
	{
		m_depFilePath = depFilePath;
	}

	public override bool UpdateDependencies(IDatabaseDependencies workspaceDependencies)
	{
		if (!workspaceDependencies.Load(m_depFilePath))
		{
			Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Normal, "Failed to load dependency information from \"{0}\"", m_depFilePath);
			return false;
		}
		return true;
	}
}
