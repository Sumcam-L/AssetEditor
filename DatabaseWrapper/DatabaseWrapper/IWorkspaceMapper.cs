namespace DatabaseWrapper;

public interface IWorkspaceMapper
{
	string GetDepotRootedPath(string path);

	string GetWorkspaceRootedPath(string path);
}
