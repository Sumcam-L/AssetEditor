using System.Collections.Generic;
using System.Windows.Forms;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public static class AssetBrowserDialogServices
{
	public static DialogResult OpenFileName(this IAssetBrowserDialogService service, ref string pathName, IEnumerable<InstanceType> filter, string directory)
	{
		string forcedInitialDirectory = service.ForcedInitialDirectory;
		try
		{
			service.ForcedInitialDirectory = directory;
			return service.OpenFileName(ref pathName, filter);
		}
		finally
		{
			service.ForcedInitialDirectory = forcedInitialDirectory;
		}
	}

	public static DialogResult OpenFileNames(this IAssetBrowserDialogService service, ref string[] pathNames, IEnumerable<InstanceType> filter, string directory)
	{
		string forcedInitialDirectory = service.ForcedInitialDirectory;
		try
		{
			service.ForcedInitialDirectory = directory;
			return service.OpenFileNames(ref pathNames, filter);
		}
		finally
		{
			service.ForcedInitialDirectory = forcedInitialDirectory;
		}
	}

	public static DialogResult SaveFileName(this IAssetBrowserDialogService service, out string pathName, IInstanceEntity filter, string directory)
	{
		string forcedInitialDirectory = service.ForcedInitialDirectory;
		try
		{
			service.ForcedInitialDirectory = directory;
			return service.SaveFileName(out pathName, filter);
		}
		finally
		{
			service.ForcedInitialDirectory = forcedInitialDirectory;
		}
	}
}
