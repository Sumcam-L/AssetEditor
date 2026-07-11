namespace Sce.Atf;

public static class Resource
{
	public static string GetPathName(this IResource resource)
	{
		return resource.Uri.LocalPath;
	}
}
