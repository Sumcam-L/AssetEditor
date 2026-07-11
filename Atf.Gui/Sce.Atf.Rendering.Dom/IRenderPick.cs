using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering.Dom;

public interface IRenderPick : IRenderObject, IBuildSceneNode, IAdaptable
{
	void PickDispatch(SceneNode[] graphPath, RenderState renderState, IRenderAction renderAction, Camera camera);
}
