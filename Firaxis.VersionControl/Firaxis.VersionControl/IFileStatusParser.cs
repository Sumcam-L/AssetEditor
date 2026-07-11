namespace Firaxis.VersionControl;

internal interface IFileStatusParser
{
	bool MatchesLine(string line);

	bool ParseStatusLine(IVersionControlWorkspace workspace, string line, ref FileStatusResultCode itemResults);
}
