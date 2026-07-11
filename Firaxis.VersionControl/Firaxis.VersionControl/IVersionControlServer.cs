using System;
using System.Collections.Generic;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public interface IVersionControlServer
{
	VersionControlContext Context { get; }

	bool IsConnected { get; }

	IEnumerable<string> Users { get; }

	IEnumerable<IVersionControlDepot> Depots { get; }

	IEnumerable<IVersionControlWorkspace> Workspaces { get; }

	bool Reconnect(string passsword);

	void GetNumPendingOperations(Action<ResultCode, int> result);

	IVersionControlDepot FindDepotByName(string depotName);

	IVersionControlWorkspace FindWorkspaceByName(string workspaceName);
}
