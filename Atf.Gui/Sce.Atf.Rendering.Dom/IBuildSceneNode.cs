using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering.Dom;

public interface IBuildSceneNode : IAdaptable
{
	bool CreateByGraphBuilder { get; }

	void OnBuildNode(SceneNode node);
}
