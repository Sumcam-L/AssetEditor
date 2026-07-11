using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering;

public interface ILodGroup
{
	IList<float> Thresholds { get; }

	Box BoundingBox { get; }
}
