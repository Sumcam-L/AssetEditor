using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firaxis.Error;

namespace Firaxis.VersionControl;

public static class VersionControlConnector
{
	private static IDictionary<string, IVersionControlServerFactory> s_factories;

	static VersionControlConnector()
	{
		s_factories = new Dictionary<string, IVersionControlServerFactory>();
		Register("perforce", new PerforceVersionControlServerFactory());
		Register("local", new LocalVersionControlServerFactory());
	}

	public static void Connect(string versionControlURI, out IVersionControlServer vcSrv, Action<ResultCode> result)
	{
		Regex regex = new Regex("^(.+)://.*$");
		MatchCollection matchCollection = regex.Matches(versionControlURI);
		if (matchCollection.Count == 1 && matchCollection[0].Groups.Count == 2)
		{
			string text = matchCollection[0].Groups[1].Value.ToLower();
			if (s_factories.ContainsKey(text))
			{
				result(s_factories[text].Create(versionControlURI, out vcSrv));
				return;
			}
			vcSrv = null;
			result(new ResultCode("No factory for version control provider \"%s\"", text));
		}
		else
		{
			vcSrv = null;
			result(new ResultCode("Failed to parse connection URI."));
		}
	}

	public static void Disconnect(IVersionControlServer vcSrv, Action<ResultCode> result)
	{
		vcSrv = null;
		result(ResultCode.Success);
	}

	public static void Register(string vcSvc, IVersionControlServerFactory facFun)
	{
		s_factories[vcSvc] = facFun;
	}
}
