using System.Collections.Generic;

namespace Firaxis.Granny;

public interface IGrannySkeleton
{
	string Name { get; }

	IList<IGrannyBone> Bones { get; }

	int LODType { get; }
}
