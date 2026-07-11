using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering;

public interface IPrimitiveSet : IAdaptable, INameable, IVisible
{
	int BindingCount { get; }

	IList<IBinding> Bindings { get; }

	string PrimitiveType { get; set; }

	int[] PrimitiveSizes { get; set; }

	int[] PrimitiveIndices { get; set; }

	IShader Shader { get; set; }

	int FindBinding(string binding);
}
