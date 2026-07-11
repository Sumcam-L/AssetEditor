using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Firaxis.CivTech;
using Firaxis.Threading;

namespace Firaxis.ATF;

internal class ProjectMap : IProjectMap
{
	private ReaderWriterLockSlim m_fileLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	private IDictionary<string, ProjectEnvironment> m_projects = new Dictionary<string, ProjectEnvironment>();

	public ProjectEnvironment this[string name]
	{
		get
		{
			ProjectEnvironment project = null;
			GetProjectImpl(name, ref project);
			return project;
		}
	}

	public ProjectEnvironment this[Guid id]
	{
		get
		{
			ProjectEnvironment project = null;
			GetProjectImpl(id, ref project);
			return project;
		}
	}

	public IEnumerable<ProjectEnvironment> Projects
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

	private bool GetProjectImpl(Guid projGuid, ref ProjectEnvironment project)
	{
		using (new ScopedReaderLock(m_fileLock))
		{
			foreach (ProjectEnvironment value in m_projects.Values)
			{
				try
				{
					if (Guid.Parse(value.PrimaryArtSpecification.ID.ID) == projGuid)
					{
						project = value;
						return true;
					}
				}
				catch (FormatException exObj)
				{
					BugSubmitter.SilentException(exObj);
				}
			}
		}
		return false;
	}

	private bool GetProjectImpl(string projName, ref ProjectEnvironment project)
	{
		using (new ScopedReaderLock(m_fileLock))
		{
			foreach (ProjectEnvironment value in m_projects.Values)
			{
				if (value.Name == projName)
				{
					project = value;
					return true;
				}
				if (value.PrimaryArtSpecification.ID.Name == projName)
				{
					project = value;
					return true;
				}
			}
		}
		return false;
	}

	public bool ContainsProject(string name)
	{
		ProjectEnvironment project = null;
		return GetProjectImpl(name, ref project);
	}

	public bool ContainsProject(Guid id)
	{
		ProjectEnvironment project = null;
		return GetProjectImpl(id, ref project);
	}

	public bool AddProject(ProjectEnvironment project)
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

	public bool GetProject(string name, ref ProjectEnvironment project)
	{
		return GetProjectImpl(name, ref project);
	}

	public bool GetProject(Guid id, ref ProjectEnvironment project)
	{
		return GetProjectImpl(id, ref project);
	}

	public void RemoveProject(string name)
	{
		ProjectEnvironment project = null;
		if (GetProjectImpl(name, ref project))
		{
			using (new ScopedWriterLock(m_fileLock))
			{
				m_projects.Remove(project.Name);
				return;
			}
		}
		BugSubmitter.SilentReport(string.Format("Tried to remove project {0} from project map containing:\n{1}\n @summary Tried to remove non-existent project from project map @assign bwhitman", name, string.Join("\n", ProjectNames)));
	}

	public void RemoveProject(Guid id)
	{
		ProjectEnvironment project = null;
		if (GetProjectImpl(id, ref project))
		{
			using (new ScopedWriterLock(m_fileLock))
			{
				m_projects.Remove(project.Name);
				return;
			}
		}
		BugSubmitter.SilentReport(string.Format("Tried to remove project {0} from project map containing:\n{1}\n @summary Tried to remove non-existent project from project map @assign bwhitman", id, string.Join("\n", Projects.Select((ProjectEnvironment sc) => sc.PrimaryArtSpecification.ID.ID))));
	}
}
