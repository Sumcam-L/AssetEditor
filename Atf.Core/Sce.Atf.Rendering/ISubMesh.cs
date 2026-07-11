using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering;

public interface ISubMesh : IPrimitiveSet, IAdaptable, INameable, IVisible
{
	IEnumerable<IDataSet> DataSets { get; }

	int Count { get; }

	IMesh Parent { get; }
}
