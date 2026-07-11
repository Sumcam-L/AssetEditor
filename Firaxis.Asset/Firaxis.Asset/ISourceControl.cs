using System.Collections.Generic;
using Firaxis.IO;

namespace Firaxis.Asset;

public interface ISourceControl
{
	bool IsConnected { get; }

	bool VerifyConnection { get; }

	string User { get; set; }

	string Password { get; set; }

	string Port { get; set; }

	string Client { get; set; }

	void Submit(string description, string[] files);

	WindowsPath GetLocalPathFromDepot(SourceControlPath depot);

	List<ISourceControlLabel> CollectLabels();

	List<ISourceControlLabel> CollectLabels(string pattern);
}
