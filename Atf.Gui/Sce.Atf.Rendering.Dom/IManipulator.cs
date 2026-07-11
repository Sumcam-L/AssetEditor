using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering.Dom;

public interface IManipulator : IRenderObject, IBuildSceneNode, IAdaptable
{
	void OnHit(HitRecord[] hits, float x, float y, IPickAction pickAction, IRenderAction renderAction, Camera camera, object context);

	void OnDrag(HitRecord[] hits, float x, float y, IPickAction pickAction, IRenderAction renderAction, Camera camera, object context);

	void OnEndDrag(HitRecord[] hits, float x, float y, IPickAction pickAction, IRenderAction renderAction, Camera camera, object context);
}
