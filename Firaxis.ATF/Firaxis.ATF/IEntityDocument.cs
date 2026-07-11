using Firaxis.CivTech.AssetObjects;
using Sce.Atf;

namespace Firaxis.ATF;

public interface IEntityDocument : IProjectSpecificDocument, IDocument, IResource
{
	IInstanceEntity InstanceEntity { get; set; }

	IInstanceSet InstanceSet { get; set; }
}
