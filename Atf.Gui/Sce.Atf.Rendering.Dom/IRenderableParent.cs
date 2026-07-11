using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering.Dom;

public interface IRenderableParent : IAdaptable
{
	bool IsRenderableChild(object child);
}
