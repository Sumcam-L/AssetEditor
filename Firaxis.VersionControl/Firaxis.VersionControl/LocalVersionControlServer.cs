using System;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public class LocalVersionControlServer : VersionControlServerBase
{
	public override bool IsConnected => true;

	public LocalVersionControlServer(VersionControlContext context, string wksName, string wksRoot)
		: base(context)
	{
		m_workspaces[wksName] = new LocalVersionControlWorkspace(context, wksName, wksRoot);
	}

	public override bool Reconnect(string passsword)
	{
		return true;
	}

	public override void GetNumPendingOperations(Action<ResultCode, int> result)
	{
		result(ResultCode.Success, 0);
	}
}
