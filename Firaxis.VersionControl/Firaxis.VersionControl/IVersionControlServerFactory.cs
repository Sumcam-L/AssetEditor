using Firaxis.Error;

namespace Firaxis.VersionControl;

public interface IVersionControlServerFactory
{
	ResultCode Create(string connectionStr, out IVersionControlServer vcSrv);
}
