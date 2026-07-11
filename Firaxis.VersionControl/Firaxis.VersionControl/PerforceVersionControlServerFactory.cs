using System.Text.RegularExpressions;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public class PerforceVersionControlServerFactory : IVersionControlServerFactory
{
	public ResultCode Create(string connectionStr, out IVersionControlServer vcSrv)
	{
		Regex regex = new Regex("^(.+)://(.*)@(.+)$");
		MatchCollection matchCollection = regex.Matches(connectionStr);
		if (matchCollection.Count == 1 && matchCollection[0].Groups.Count == 4)
		{
			string value = matchCollection[0].Groups[2].Value;
			string value2 = matchCollection[0].Groups[3].Value;
			VersionControlContext context = new VersionControlContext(connectionStr, value2, value, PerforceVersionControlServer.DefaultTimeout);
			ResultCode result = ResultCode.Success;
			try
			{
				vcSrv = new PerforceVersionControlServer(context);
			}
			catch (ResultCodeException ex)
			{
				vcSrv = null;
				result = ex.Result;
			}
			return result;
		}
		vcSrv = null;
		return new ResultCode("Failed to parse perforce connection URI.");
	}
}
