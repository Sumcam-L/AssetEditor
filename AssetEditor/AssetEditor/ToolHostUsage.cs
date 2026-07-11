using System.ComponentModel.Composition;
using Firaxis.CivTech;

namespace AssetEditor;

[Export(typeof(IToolHostPolicy))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ToolHostUsage : IToolHostPolicy
{
	public bool UseOnDemandLoading => false;
}
