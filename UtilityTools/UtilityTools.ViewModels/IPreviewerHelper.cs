using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace UtilityTools.ViewModels;

public interface IPreviewerHelper
{
	IEnumerable<string> GetKnownPreviewModules(IProjectConfig projectConfig);

	IEnumerable<string> GetPreviewerModulesThatSupportLightRigClass(string lightRigClassName);

	IEnumerable<string> GetAssetClassesThatSupportPreviewer(string previewModuleName);

	bool DoesPreviewerSupportLighting(string previewModuleName);

	IEnumerable<string> GetAllowedLightRigClasses(string previewModuleName);
}
