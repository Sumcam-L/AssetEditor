using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering;

public interface IShader : IAdaptable, INameable
{
	IList<IBinding> Bindings { get; }

	IList<object> CustomAttributes { get; }
}
