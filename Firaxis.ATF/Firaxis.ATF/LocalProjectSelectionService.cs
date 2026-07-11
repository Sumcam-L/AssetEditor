using System.ComponentModel.Composition;
using Firaxis.CivTech;

namespace Firaxis.ATF;

[Export(typeof(IProjectSelectionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LocalProjectSelectionService : ProjectSelectionService
{
	private IProjectRootProvider RootProvider { get; set; }

	[ImportingConstructor]
	public LocalProjectSelectionService(IAssetCloudSettingService assCldSetSvc, ICommonConfigsRootProvider rootProvider)
		: base(assCldSetSvc, rootProvider.EnvironmentPath, string.Empty)
	{
	}
}
