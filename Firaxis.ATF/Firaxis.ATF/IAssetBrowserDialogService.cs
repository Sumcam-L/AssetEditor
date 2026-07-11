using System.Collections.Generic;
using System.Windows.Forms;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public interface IAssetBrowserDialogService
{
	string ForcedInitialDirectory { get; set; }

	string InitialDirectory { get; set; }

	DialogResult ConfirmFileClose(string message);

	DialogResult OpenEntities(IDictionary<string, InstanceType> entities, IEnumerable<InstanceType> filter, IEnumerable<string> allowedClasses);

	DialogResult OpenFileName(ref string pathName, IEntityFilteringContext entityFilteringContext);

	DialogResult OpenFileName(ref string pathName, IEnumerable<InstanceType> filter);

	DialogResult OpenFileName(ref string pathName, IEnumerable<InstanceType> filter, IEnumerable<string> allowedClasses);

	DialogResult OpenFileNames(ref string[] pathNames, IEnumerable<InstanceType> filter);

	DialogResult OpenFileNames(ref string[] pathNames, IEnumerable<InstanceType> filter, IEnumerable<string> allowedClasses);

	bool PathExists(string pathName);

	DialogResult SaveFileName(out string pathName, IInstanceEntity filter);
}
