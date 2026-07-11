using System;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public interface IVersionControlConnector
{
	void Connect(string vesionControlURI, out IVersionControlServer vcSrv, Action<ResultCode> result);

	void Disconnect(IVersionControlServer vcSrv, Action<ResultCode> result);
}
