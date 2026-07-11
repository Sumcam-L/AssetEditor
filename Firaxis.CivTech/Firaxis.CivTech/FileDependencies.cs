namespace Firaxis.CivTech;

public class FileDependencies : IFileDependencies<string>
{
	private FileDependencyInfo m_pantryDepInfo = new FileDependencyInfo();

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

	public IFileDependencyCatalog<string> Dependencies => m_pantryDepInfo.Dependencies;

	public IFileDependencyCatalog<string> Dependants => m_pantryDepInfo.Dependants;

	public IFileCatalog<string> Files => m_pantryDepInfo.Files;
}
