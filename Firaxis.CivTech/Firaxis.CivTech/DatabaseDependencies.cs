namespace Firaxis.CivTech;

public class DatabaseDependencies : IDatabaseDependencies
{
	private DepotDependencyInfo m_pantryDepInfo = new DepotDependencyInfo();

	public long Timestamp
	{
		get
		{
			return m_pantryDepInfo.Timestamp;
		}
		set
		{
			m_pantryDepInfo.Timestamp = value;
		}
	}

	public uint Changelist
	{
		get
		{
			return m_pantryDepInfo.Changelist;
		}
		set
		{
			m_pantryDepInfo.Changelist = value;
		}
	}

	public IDependencyCatalog Dependencies => m_pantryDepInfo.Dependencies;

	public IDependencyCatalog Dependants => m_pantryDepInfo.Dependants;

	public IDepotCatalog Files => m_pantryDepInfo.Files;
}
