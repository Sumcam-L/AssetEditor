using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering.Dom;

public interface IRenderObject : IBuildSceneNode, IAdaptable
{
	bool Init(SceneNode node);

	TraverseState Traverse(Stack<SceneNode> graphPath, IRenderAction renderAction, Camera camera, ICollection<TraverseNode> traverseList);

	TraverseState PostTraverse(Stack<SceneNode> graphPath, IRenderAction renderAction, Camera camera, ICollection<TraverseNode> traverseList);

	void Dispatch(SceneNode[] graphPath, RenderState renderState, IRenderAction renderAction, Camera camera);

	void Release();

	Type[] GetDependencies();

	IIntersectable GetIntersectable();
}
