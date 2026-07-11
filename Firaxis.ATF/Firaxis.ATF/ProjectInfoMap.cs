using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Firaxis.CivTech;
using Firaxis.Threading;

namespace Firaxis.ATF;

internal class ProjectInfoMap : IProjectInfoMap
{
	private ReaderWriterLockSlim m_fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	private IDictionary<string, ProjectInfo> m_projects = new Dictionary<string, ProjectInfo>();

	private IDictionary<string, string> m_alternateNameMap = new Dictionary<string, string>();

	public ProjectInfo this[string name]
	{
		get
		{
			ProjectInfo project = null;
			GetProjectImpl(name, ref project);
			return project;
		}
	}

	public IEnumerable<ProjectInfo> ProjectInfos
	{
		get
		{
			using (new ScopedReaderLock(m_fileLock))
			{
				return m_projects.Values.ToArray();
			}
		}
	}

	public IEnumerable<string> ProjectNames
	{
		get
		{
			using (new ScopedReaderLock(m_fileLock))
			{
				return m_projects.Keys.ToArray();
			}
		}
	}

	private bool GetProjectImpl(string projName, ref ProjectInfo project)
	{
		using (new ScopedReaderLock(m_fileLock))
		{
			if (m_projects.TryGetValue(projName, out project))
			{
				return true;
			}
			string value = string.Empty;
			if (m_alternateNameMap.TryGetValue(projName, out value) && m_projects.TryGetValue(value, out project))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsProject(string name)
	{
		ProjectInfo project = null;
		return GetProjectImpl(name, ref project);
	}

	public bool AddAlternateKey(string primaryKey, string altKey)
	{
		using (ScopedUpgrableReaderLock upgrableReadLock = new ScopedUpgrableReaderLock(m_fileLock))
		{
			if (m_alternateNameMap.ContainsKey(altKey))
			{
				BugSubmitter.SilentReport("Attempting to add alternate key \"" + altKey + "\" for project \"" + primaryKey + "\" but \"" + altKey + "\" already exists @summary Attempting to add alternate key that already exists! @assign bwhitman");
				return false;
			}
			if (m_projects.ContainsKey(altKey))
			{
				BugSubmitter.SilentReport("Attempting to add alternate key \"" + altKey + "\" for project \"" + primaryKey + "\" but \"" + altKey + "\" is already a primary key @summary Attempting to add alternate key that matches existing primary key! @assign bwhitman");
				return false;
			}
			if (!m_projects.ContainsKey(primaryKey))
			{
				BugSubmitter.SilentReport("Attempting to add alternate key \"" + altKey + "\" for project \"" + primaryKey + "\" but \"" + primaryKey + "\" does not exists @summary Attempting to add alternate key for item that does not exists! @assign bwhitman");
				return false;
			}
			using (new ScopedWriterLock(upgrableReadLock))
			{
				m_alternateNameMap[altKey] = primaryKey;
			}
		}
		return true;
	}

	public bool AddProject(ProjectInfo project)
	{
		if (ContainsProject(project.Name))
		{
			return false;
		}
		using (new ScopedWriterLock(m_fileLock))
		{
			m_projects[project.Name] = project;
		}
		return true;
	}

	public bool GetProject(string name, ref ProjectInfo project)
	{
		return GetProjectImpl(name, ref project);
	}

	public void RemoveProject(string name)
	{
		ProjectInfo project = null;
		if (GetProjectImpl(name, ref project))
		{
			using (new ScopedWriterLock(m_fileLock))
			{
				m_projects.Remove(project.Name);
				KeyValuePair<string, string>[] array = m_alternateNameMap.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<string, string> keyValuePair = array[i];
					if (keyValuePair.Value == project.Name)
					{
						m_alternateNameMap.Remove(keyValuePair.Key);
					}
				}
				return;
			}
		}
		BugSubmitter.SilentReport(string.Format("Tried to remove project {0} from project map containing:\n{1}\n @summary Tried to remove non-existent project from project map @assign bwhitman", name, string.Join("\n", ProjectNames)));
	}
}
