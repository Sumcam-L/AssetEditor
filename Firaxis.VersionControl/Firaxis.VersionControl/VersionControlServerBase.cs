using System;
using System.Collections.Generic;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public abstract class VersionControlServerBase : IVersionControlServer
{
	protected VersionControlContext m_context;

	protected IList<string> m_users = new List<string>();

	protected IDictionary<string, IVersionControlDepot> m_depots = new Dictionary<string, IVersionControlDepot>();

	protected IDictionary<string, IVersionControlWorkspace> m_workspaces = new Dictionary<string, IVersionControlWorkspace>();

	public VersionControlContext Context => m_context;

	public IEnumerable<string> Users => m_users;

	public IEnumerable<IVersionControlDepot> Depots => m_depots.Values;

	public IEnumerable<IVersionControlWorkspace> Workspaces => m_workspaces.Values;

	public abstract bool IsConnected { get; }

	public abstract bool Reconnect(string passsword);

	public abstract void GetNumPendingOperations(Action<ResultCode, int> result);

	public VersionControlServerBase(VersionControlContext context)
	{
		m_context = context;
	}

	public IVersionControlDepot FindDepotByName(string depotName)
	{
		if (!m_depots.ContainsKey(depotName))
		{
			return null;
		}
		return m_depots[depotName];
	}

	public IVersionControlWorkspace FindWorkspaceByName(string workspaceName)
	{
		if (!m_workspaces.ContainsKey(workspaceName))
		{
			return null;
		}
		return m_workspaces[workspaceName];
	}
}
