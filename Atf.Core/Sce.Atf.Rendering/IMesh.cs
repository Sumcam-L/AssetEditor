using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering;

public interface IMesh : IAdaptable, INameable
{
	IEnumerable<IDataSet> DataSets { get; }

	IEnumerable<IPrimitiveSet> PrimitiveSets { get; }

	Box BoundingBox { get; }
}
