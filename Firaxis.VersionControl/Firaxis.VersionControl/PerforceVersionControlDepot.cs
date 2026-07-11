namespace Firaxis.VersionControl;

public class PerforceVersionControlDepot : IVersionControlDepot
{
	private string m_depotName;

	private DepotType m_depotType;

	public string Name => m_depotName;

	public DepotType Type => m_depotType;

	public PerforceVersionControlDepot(string depotName, DepotType depotType)
	{
		m_depotName = depotName;
		m_depotType = depotType;
	}
}
